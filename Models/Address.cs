using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAgendamentoWebII.Models;

public class Address
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EstablishmentId { get; set; }

    [ForeignKey("EstablishmentId")]
    public Establishment Establishment { get; set; }

    [Required, MaxLength(150)]
    public string Street { get; set; }

    [Required, MaxLength(20)]
    public string Number { get; set; }

    [Required, MaxLength(100)]
    public string District { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; }

    [Required, MaxLength(50)]
    public string State { get; set; }

    [Required, MaxLength(15)]
    public string ZipCode { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }
}