namespace MyApp.DTOs; // ← replace MyApp

public class SampleGrandResponse // ← rename to match the exam, e.g. RentalMovieResponse
{
    public string Title { get; set; } = null!;  // ← rename to match exam JSON key, e.g. "title" → Title
    public decimal Price { get; set; }          // ← rename to match exam JSON key, e.g. "priceAtRental" → PriceAtRental
}
