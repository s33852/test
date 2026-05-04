using MyApp.DTOs; // ← replace MyApp

namespace MyApp.Services; // ← replace MyApp

public interface ISampleService // ← rename, e.g. ICustomerService
{
    Task<SampleResponse?> GetDetailsAsync(int id);              // ← rename method + return type to match your DTO
    Task CreateItemAsync(int id, CreateSampleRequest request);  // ← rename method + request type to match your DTO
}
