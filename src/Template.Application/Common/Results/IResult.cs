namespace Template.Application.Common.Results;

/// <summary>
/// Tüm operasyonlar için standart sonuç arayüzü
/// </summary>
public interface IResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// İşlem başarısız mı?
    /// </summary>
    bool IsFailure => !IsSuccess;

    /// <summary>
    /// Hata mesajı (başarısız operasyonlar için)
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Başarı mesajı (başarılı operasyonlar için)
    /// </summary>
    string? SuccessMessage { get; }
}

/// <summary>
/// Veri dönen operasyonlar için generic sonuç arayüzü
/// </summary>
/// <typeparam name="T">Dönüş verisi türü</typeparam>
public interface IResult<out T> : IResult
{
    /// <summary>
    /// İşlem sonucu dönen veri
    /// </summary>
    T? Data { get; }
} 