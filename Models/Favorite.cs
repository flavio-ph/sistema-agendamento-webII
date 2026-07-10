using SistemaAgendamentoWebII.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Favorite
{
    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("Favorites")] // Avisa o EF para ligar com a ICollection<Favorite> do User
    public User? Client { get; set; }

    [Required]
    public Guid ProfessionalId { get; set; }

    [ForeignKey("ProfessionalId")]
    [InverseProperty("Favorites")] // Avisa o EF para ligar com a ICollection<Favorite> do Professional
    public Professional? Professional { get; set; }
}