namespace MyApp.DTOs; // ← replace MyApp

public class SampleResponse // ← rename to match the exam, e.g. CustomerRentalsResponse
{
    public string FirstName { get; set; } = null!;                      // ← rename to match exam JSON key, e.g. "firstName" → FirstName
    public string LastName { get; set; } = null!;                       // ← rename to match exam JSON key
    public List<SampleChildResponse> Children { get; set; } = new();   // ← rename Children + SampleChildResponse to match exam JSON
}
