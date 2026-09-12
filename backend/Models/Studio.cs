namespace KilnLog.Api.Models;

public class Studio
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public ICollection<Kiln> Kilns { get; set; } = new List<Kiln>();
}
