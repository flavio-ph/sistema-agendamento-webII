using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Service
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProfessionalId { get; set; }
    [ForeignKey("ProfessionalId")]
    public Professional Professional { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}