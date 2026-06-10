using System.ComponentModel.DataAnnotations;

namespace SistemaAgendamentoWebII.Models;

public class Category
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
