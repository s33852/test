// No changes needed — copy as-is into every project.
namespace MyApp.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
