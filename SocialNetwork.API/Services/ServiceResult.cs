namespace SocialNetwork.API.Services;

public enum ServiceResultStatus
{
    Success,
    BadRequest,
    NotFound,
    Unauthorized
}

public class ServiceResult<T>
{
    private ServiceResult(ServiceResultStatus status, T? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public ServiceResultStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value, null);
    public static ServiceResult<T> BadRequest(string error) => new(ServiceResultStatus.BadRequest, default, error);
    public static ServiceResult<T> NotFound(string error) => new(ServiceResultStatus.NotFound, default, error);
    public static ServiceResult<T> Unauthorized(string error) => new(ServiceResultStatus.Unauthorized, default, error);
}
