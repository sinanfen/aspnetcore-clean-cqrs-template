namespace Template.Application.Common.Results;

/// <summary>
/// Standart operasyon sonucu implementasyonu
/// </summary>
public class Result : IResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Başarı mesajı
    /// </summary>
    public string? SuccessMessage { get; private set; }

    /// <summary>
    /// Constructor - Protected olarak tanımlandı, factory metodları kullanılmalı
    /// </summary>
    /// <param name="isSuccess">Başarı durumu</param>
    /// <param name="errorMessage">Hata mesajı</param>
    /// <param name="successMessage">Başarı mesajı</param>
    protected Result(bool isSuccess, string? errorMessage, string? successMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        SuccessMessage = successMessage;
    }

    /// <summary>
    /// Başarılı sonuç oluşturur
    /// </summary>
    /// <param name="successMessage">Başarı mesajı</param>
    /// <returns>Başarılı Result</returns>
    public static Result Success(string? successMessage = null)
        => new(true, null, successMessage);

    /// <summary>
    /// Başarısız sonuç oluşturur
    /// </summary>
    /// <param name="errorMessage">Hata mesajı</param>
    /// <returns>Başarısız Result</returns>
    public static Result Failure(string errorMessage)
        => new(false, errorMessage, null);

    /// <summary>
    /// Başarılı veri dönen sonuç oluşturur
    /// </summary>
    /// <typeparam name="T">Veri türü</typeparam>
    /// <param name="data">Dönülecek veri</param>
    /// <param name="successMessage">Başarı mesajı</param>
    /// <returns>Başarılı Result with data</returns>
    public static Result<T> Success<T>(T data, string? successMessage = null)
        => new(true, data, null, successMessage);

    /// <summary>
    /// Başarısız veri dönen sonuç oluşturur
    /// </summary>
    /// <typeparam name="T">Veri türü</typeparam>
    /// <param name="errorMessage">Hata mesajı</param>
    /// <returns>Başarısız Result with data</returns>
    public static Result<T> Failure<T>(string errorMessage)
        => new(false, default, errorMessage, null);
}

/// <summary>
/// Veri dönen operasyon sonucu implementasyonu
/// </summary>
/// <typeparam name="T">Veri türü</typeparam>
public class Result<T> : Result, IResult<T>
{
    /// <summary>
    /// İşlem sonucu dönen veri
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Constructor - Protected olarak tanımlandı, factory metodları kullanılmalı
    /// </summary>
    /// <param name="isSuccess">Başarı durumu</param>
    /// <param name="data">Dönülecek veri</param>
    /// <param name="errorMessage">Hata mesajı</param>
    /// <param name="successMessage">Başarı mesajı</param>
    internal Result(bool isSuccess, T? data, string? errorMessage, string? successMessage)
        : base(isSuccess, errorMessage, successMessage)
    {
        Data = data;
    }
} 