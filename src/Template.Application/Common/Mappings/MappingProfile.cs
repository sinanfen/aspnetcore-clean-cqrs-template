using AutoMapper;
using Template.Domain.Entities.Identity;

namespace Template.Application.Common.Mappings;

/// <summary>
/// AutoMapper profil konfigürasyonu
/// Entity'ler ve DTO'lar arasındaki dönüşüm kuralları
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Mapping konfigürasyonları
    /// </summary>
    public MappingProfile()
    {
        // AppUser mappings
        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));

        // RefreshToken mappings
        CreateMap<RefreshToken, RefreshTokenDto>();

        // Tersine mapping'ler (gerektiğinde)
        CreateMap<UserDto, AppUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());
    }
}

/// <summary>
/// Kullanıcı bilgileri için DTO
/// </summary>
public class UserDto
{
    /// <summary>
    /// Kullanıcı benzersiz kimliği
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Kullanıcı adı
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Ad
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Soyad
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Tam ad
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// İki faktörlü kimlik doğrulama aktif mi?
    /// </summary>
    public bool Is2FAEnabled { get; set; }

    /// <summary>
    /// E-posta doğrulandı mı?
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Telefon numarası doğrulandı mı?
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }
}

/// <summary>
/// Refresh Token bilgileri için DTO
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// Token benzersiz kimliği
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Token değeri
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Son kullanma tarihi
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Token aktif mi?
    /// </summary>
    public bool IsActive { get; set; }
} 