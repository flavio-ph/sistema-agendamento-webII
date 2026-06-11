using SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Favorite
{
    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey("ClientId")]
    public User Client { get; set; }

    [Required]
    public Guid ProfessionalId { get; set; }

    [ForeignKey("ProfessionalId")]
    public Professional Professional { get; set; }
}