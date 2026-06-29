using System.ComponentModel.DataAnnotations;
using System.Net;

namespace SistemaAgendamentoWebII.Models;

public class Establishment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    public string? LogoUrl { get; set; }

    [MaxLength(20)]
    public string? CNPJ { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; } // Ajustado para Guid conforme erro CS0029    public User User { get; set; } 

    // Navegação
    public Address? Address { get; set; }
    public ICollection<Professional> Professionals { get; set; } = new List<Professional>();
}