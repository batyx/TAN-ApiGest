namespace TanApiGest.DTOs.Responses;

/// <summary>
/// Résultat d'un appel à l'API : soit une valeur de type T (succès), soit
/// un message d'erreur lisible. Évite de faire remonter des exceptions
/// réseau jusqu'aux ViewModels.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string ErrorMessage { get; }

    private ApiResponse(T? data)
    {
        IsSuccess = true;
        Data = data;
        ErrorMessage = string.Empty;
    }

    private ApiResponse(string errorMessage)
    {
        IsSuccess = false;
        Data = default;
        ErrorMessage = errorMessage;
    }

    public static ApiResponse<T> Success(T? data) => new(data);
    public static ApiResponse<T> Failure(string errorMessage) => new(errorMessage);
}
