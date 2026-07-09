using System.Net;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Establishment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required(ErrorMessage = "O nome do estabelecimento é obrigatório.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Digite apenas números no telefone.")]
    public string? Phone { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    public string? Email { get; set; }

    public string? LogoUrl { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "O CNPJ deve conter exatamente 14 números (sem traços ou pontos).")]
    public string? CNPJ { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propriedades de Navegação
    public User? User { get; set; }
    public Address? Address { get; set; }
    public ICollection<Professional> Professionals { get; set; } = new List<Professional>();
}