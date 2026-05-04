using System.ComponentModel.DataAnnotations;

namespace MyApp.DTOs; // ← replace MyApp

public class CreateSampleItemRequest // ← rename to match the exam, e.g. CreateRentalMovieRequest
{
    [Required] public string Title { get; set; } = null!;                   // ← rename to match exam POST body key, e.g. "title" → Title
    [Required][Range(0.0, double.MaxValue)] public decimal Price { get; set; } // ← rename to match exam POST body key, e.g. "rentalPrice" → RentalPrice
}
