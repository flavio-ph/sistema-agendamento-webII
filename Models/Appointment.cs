using SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Appointment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ClienteId { get; set; }

    [ForeignKey("ClienteId")]
    public User Client { get; set; }

    [Required]
    public Guid ProfessionalId { get; set; }

    [ForeignKey("ProfessionalId")]
    public Professional Professional { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public Service Service { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propriedade calculada para resolver o erro no DashboardController
    // [NotMapped] garante que o Entity Framework ignore este campo no banco de dados
    [NotMapped]
    public DateTime DataHoraInicio => AppointmentDate.ToDateTime(TimeOnly.FromTimeSpan(StartTime));
}