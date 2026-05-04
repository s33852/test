namespace MyApp.DTOs; // ← replace MyApp

public class SampleChildResponse // ← rename to match the exam, e.g. RentalResponse
{
    public int Id { get; set; }                                             // ← rename if needed
    public DateTime Date { get; set; }                                      // ← rename to match exam JSON key, e.g. RentalDate
    public DateTime? NullableDate { get; set; }                             // ← rename or remove if not in your exam JSON
    public string Status { get; set; } = null!;                             // ← rename if needed
    public List<SampleGrandResponse> Items { get; set; } = new();          // ← rename Items + SampleGrandResponse to match exam JSON
}
