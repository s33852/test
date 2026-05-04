using MyApp.DTOs; // ← replace MyApp

namespace MyApp.Repositories; // ← replace MyApp

public interface ISampleRepository // ← rename, e.g. ICustomerRepository
{
    Task<SampleResponse?> GetDetailsAsync(int id);              // ← copy exactly from ISampleService — must be identical
    Task CreateItemAsync(int id, CreateSampleRequest request);  // ← copy exactly from ISampleService — must be identical
}
