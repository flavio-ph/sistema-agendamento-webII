using SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaAgendamento.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(150)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? ProfileImage { get; set; }

    [Required, MaxLength(30)]
    public string Role { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Propriedades de Navegação
    public Professional? Professional { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}