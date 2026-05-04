using MyApp.DTOs;         // ← replace MyApp
using MyApp.Repositories; // ← replace MyApp

namespace MyApp.Services; // ← replace MyApp

public class SampleService : ISampleService // ← rename both, e.g. CustomerService : ICustomerService
{
    private readonly ISampleRepository _repository; // ← replace ISampleRepository

    public SampleService(ISampleRepository repository) // ← replace ISampleRepository
    {
        _repository = repository;
    }

    public Task<SampleResponse?> GetDetailsAsync(int id)           // ← rename method + return type to match interface
        => _repository.GetDetailsAsync(id);                         // ← rename GetDetailsAsync to match your repository

    public Task CreateItemAsync(int id, CreateSampleRequest request) // ← rename method + request type to match interface
        => _repository.CreateItemAsync(id, request);                  // ← rename CreateItemAsync to match your repository
}
