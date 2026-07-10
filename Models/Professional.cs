using SistemaAgendamentoWebII.Models;
namespace SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Professional
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid? EstablishmentId { get; set; }

    public string? Biography { get; set; }

    // --- CAMPOS QUE ESTAVAM FALTANDO ---
    [MaxLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }
    // ------------------------------------

    [MaxLength(100)]
    public string? Specialty { get; set; }

    [Range(0, 80, ErrorMessage = "Os anos de experiência devem estar entre 0 e 80.")]
    public int? ExperienceYears { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0.0m;

    public bool IsActive { get; set; } = true;

    // Propriedades de Navegação
    public User? User { get; set; }
    public Establishment? Establishment { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}