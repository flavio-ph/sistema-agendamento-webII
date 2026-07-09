using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [MaxLength(150)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.com$", ErrorMessage = "O e-mail deve conter '@' e terminar com '.com'")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{6,}$", ErrorMessage = "A senha deve ter letras, números e no mínimo 6 caracteres.")]
    public string PasswordHash { get; set; } = string.Empty;

    [NotMapped] // Isso impede que o Entity Framework tente criar essa coluna no banco
    [Required(ErrorMessage = "Confirme sua senha.")]
    [Compare("PasswordHash", ErrorMessage = "As senhas não coincidem.")]
    public string PasswordConfirmation { get; set; } = string.Empty;

    [MaxLength(11, ErrorMessage = "O telefone deve ter no máximo 11 dígitos.")]
    [RegularExpression(@"^\d{1,11}$", ErrorMessage = "Digite apenas números.")]
    public string? Phone { get; set; }

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Cliente";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Professional? Professional { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}