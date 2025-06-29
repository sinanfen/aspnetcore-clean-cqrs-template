using System.Security.Cryptography;
using System.Text;
using Template.Application.Services.TwoFactor;
using Template.Domain.Entities.Identity;

namespace Template.Infrastructure.Services.TwoFactor;

/// <summary>
/// İki faktörlü kimlik doğrulama servisi implementasyonu
/// Google Authenticator (TOTP - Time-based One-time Password) uyumlu
/// </summary>
public class TwoFactorService : ITwoFactorService
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const int DefaultSecretLength = 32;
    private const int CodeLength = 6;
    private const int TimeStep = 30; // 30 saniye

    /// <summary>
    /// Kullanıcı için 2FA secret key oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Base32 encoded secret key</returns>
    public async Task<string> GenerateSecretKeyAsync(AppUser user)
    {
        return await GenerateRandomSecretAsync(DefaultSecretLength);
    }

    /// <summary>
    /// Google Authenticator için QR kod URI'si oluşturur
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="secretKey">Secret key</param>
    /// <param name="issuer">Uygulama adı</param>
    /// <returns>QR kod URI'si</returns>
    public async Task<string> GenerateQrCodeUriAsync(AppUser user, string secretKey, string? issuer = null)
    {
        issuer ??= "Template App";
        var accountTitle = $"{issuer}:{user.Email}";
        
        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(accountTitle)}" +
                       $"?secret={secretKey}" +
                       $"&issuer={Uri.EscapeDataString(issuer)}" +
                       $"&algorithm=SHA1" +
                       $"&digits={CodeLength}" +
                       $"&period={TimeStep}";

        return await Task.FromResult(qrCodeUri);
    }

    /// <summary>
    /// TOTP kodunu doğrular
    /// </summary>
    /// <param name="secretKey">Kullanıcının secret key'i</param>
    /// <param name="code">6 haneli TOTP kodu</param>
    /// <returns>Kod geçerli mi?</returns>
    public async Task<bool> ValidateCodeAsync(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
            return false;

        try
        {
            var secretBytes = FromBase32String(secretKey);
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Zaman toleransı için önceki ve sonraki time step'leri de kontrol et
            for (int i = -1; i <= 1; i++)
            {
                var timeStep = (currentTime / TimeStep) + i;
                var expectedCode = GenerateTotp(secretBytes, timeStep);
                
                if (code == expectedCode)
                {
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }

    /// <summary>
    /// Backup kodları oluşturur
    /// </summary>
    /// <param name="count">Oluşturulacak kod sayısı</param>
    /// <returns>Backup kodları listesi</returns>
    public async Task<string[]> GenerateBackupCodesAsync(int count = 10)
    {
        var codes = new string[count];
        using var rng = RandomNumberGenerator.Create();
        
        for (int i = 0; i < count; i++)
        {
            var bytes = new byte[5];
            rng.GetBytes(bytes);
            
            // 10 haneli backup kod oluştur (XXXXX-XXXXX formatında)
            var base64String = Convert.ToBase64String(bytes)
                .Replace('+', '0')
                .Replace('/', '1')
                .Replace('=', '2');
                
            // String uzunluğunu kontrol et
            var code = base64String.Length >= 10 
                ? base64String.Substring(0, 10)
                : base64String.PadRight(10, '0');
                
            codes[i] = $"{code.Substring(0, 5)}-{code.Substring(5, 5)}".ToUpper();
        }

        return await Task.FromResult(codes);
    }

    /// <summary>
    /// Backup kodu doğrular (Bu implementasyonda cache/database kontrolü yok)
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="backupCode">Backup kod</param>
    /// <returns>Kod geçerli mi?</returns>
    public async Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode)
    {
        // TODO: Bu metot veritabanından kullanıcının backup kodlarını kontrol etmeli
        // Şu an basit format kontrolü yapıyor
        
        if (string.IsNullOrWhiteSpace(backupCode))
            return false;

        // Format: XXXXX-XXXXX
        var pattern = @"^[A-Z0-9]{5}-[A-Z0-9]{5}$";
        var isValidFormat = System.Text.RegularExpressions.Regex.IsMatch(backupCode.ToUpper(), pattern);
        
        return await Task.FromResult(isValidFormat);
    }

    /// <summary>
    /// TOTP kodunu doğrular (kullanıcı ile)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="secretKey">Secret key</param>
    /// <param name="totpCode">TOTP kodu</param>
    /// <returns>Kod geçerli mi?</returns>
    public async Task<bool> ValidateTotpAsync(AppUser user, string secretKey, string totpCode)
    {
        return await ValidateCodeAsync(secretKey, totpCode);
    }

    /// <summary>
    /// Backup kodu doğrular (kullanıcı ile)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <param name="backupCode">Backup kod</param>
    /// <returns>Kod geçerli mi?</returns>
    public async Task<bool> ValidateBackupCodeAsync(AppUser user, string backupCode)
    {
        return await ValidateBackupCodeAsync(user.Id, backupCode);
    }

    /// <summary>
    /// Machine token oluşturur (Remember Machine için)
    /// </summary>
    /// <param name="user">Kullanıcı entity'si</param>
    /// <returns>Machine token</returns>
    public async Task<string> GenerateMachineTokenAsync(AppUser user)
    {
        // Machine identifier oluştur
        var machineId = Environment.MachineName + "_" + user.Id;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        // Token payload
        var payload = $"{machineId}:{timestamp}:{user.Id}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        
        // HMAC ile imzala (gerçek implementasyonda güvenli bir key kullanılmalı)
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("machine-token-secret-key"));
        var signature = hmac.ComputeHash(payloadBytes);
        
        // Base64 encode
        var token = Convert.ToBase64String(payloadBytes) + "." + Convert.ToBase64String(signature);
        
        return await Task.FromResult(token);
    }

    /// <summary>
    /// Kullanıcının kalan backup kod sayısını döndürür
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <returns>Kalan backup kod sayısı</returns>
    public async Task<int> GetRemainingBackupCodesCountAsync(Guid userId)
    {
        // TODO: Bu metot veritabanından kullanıcının kalan backup kod sayısını almalı
        return await Task.FromResult(8); // Geçici değer
    }

    /// <summary>
    /// Güvenli random secret key oluşturur
    /// </summary>
    /// <param name="length">Key uzunluğu</param>
    /// <returns>Base32 encoded random key</returns>
    public async Task<string> GenerateRandomSecretAsync(int length = DefaultSecretLength)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        
        return await Task.FromResult(ToBase32String(bytes));
    }

    /// <summary>
    /// Secret key'i Base32 formatına çevirir
    /// </summary>
    /// <param name="input">Çevrilecek byte array</param>
    /// <returns>Base32 encoded string</returns>
    public string ToBase32String(byte[] input)
    {
        if (input == null || input.Length == 0)
            return string.Empty;

        var output = new StringBuilder();
        int bits = 0;
        int value = 0;

        foreach (byte b in input)
        {
            value = (value << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                output.Append(Base32Alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
        {
            output.Append(Base32Alphabet[(value << (5 - bits)) & 31]);
        }

        return output.ToString();
    }

    /// <summary>
    /// Base32 string'i byte array'e çevirir
    /// </summary>
    /// <param name="input">Base32 encoded string</param>
    /// <returns>Byte array</returns>
    public byte[] FromBase32String(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Array.Empty<byte>();

        input = input.ToUpper().Replace(" ", "").Replace("-", "");
        var output = new List<byte>();
        int bits = 0;
        int value = 0;

        foreach (char c in input)
        {
            int index = Base32Alphabet.IndexOf(c);
            if (index < 0) continue;

            value = (value << 5) | index;
            bits += 5;

            if (bits >= 8)
            {
                output.Add((byte)((value >> (bits - 8)) & 255));
                bits -= 8;
            }
        }

        return output.ToArray();
    }

    /// <summary>
    /// TOTP kodu oluşturur
    /// </summary>
    /// <param name="secret">Secret key bytes</param>
    /// <param name="timeStep">Time step değeri</param>
    /// <returns>6 haneli TOTP kodu</returns>
    private static string GenerateTotp(byte[] secret, long timeStep)
    {
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
        }

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(timeBytes);

        int offset = hash[hash.Length - 1] & 0x0F;
        int truncatedHash = ((hash[offset] & 0x7F) << 24) |
                           ((hash[offset + 1] & 0xFF) << 16) |
                           ((hash[offset + 2] & 0xFF) << 8) |
                           (hash[offset + 3] & 0xFF);

        int code = truncatedHash % (int)Math.Pow(10, CodeLength);
        return code.ToString($"D{CodeLength}");
    }
} 