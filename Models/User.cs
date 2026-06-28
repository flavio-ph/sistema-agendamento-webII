using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Cliente";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Propriedades de Navegação
    // O C# moderno trata estas coleções como não-nulas por padrão, 
    // e inicializá-las no construtor ou na declaração resolve o aviso CS8618.
    public Professional? Professional { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}