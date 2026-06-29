using SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Professional
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }

    public Guid? EstablishmentId { get; set; }
    [ForeignKey("EstablishmentId")]
    public Establishment? Establishment { get; set; }

    public string? Biography { get; set; }

    public string Description { get; set; }
    public string RegistrationNumber { get; set; }

    [MaxLength(100)]
    public string? Specialty { get; set; }

    public int? ExperienceYears { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navegação
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}