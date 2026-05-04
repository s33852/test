using System.ComponentModel.DataAnnotations;

namespace MyApp.DTOs; // ← replace MyApp

public class CreateSampleRequest // ← rename to match the exam, e.g. CreateRentalRequest
{
    [Required] public int Id { get; set; }                                                          // ← rename if needed
    [Required] public DateTime Date { get; set; }                                                   // ← rename to match exam POST body, e.g. RentalDate
    [Required][MinLength(1)] public List<CreateSampleItemRequest> Items { get; set; } = new();      // ← rename Items + CreateSampleItemRequest to match exam POST body
}
