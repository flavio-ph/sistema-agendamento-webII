using SistemaAgendamentoWebII.Models;

namespace SistemaAgendamentoWebII.Models.ViewModels;

public class EstablishmentDashboardViewModel
{
    public User? User { get; set; }
    public Establishment? Establishment { get; set; }

    public int TotalProfessionals { get; set; }
    public int AppointmentsTodayCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public decimal MonthlyRevenue { get; set; }

    // Propriedades da tela empresa
    public int ActiveProfessionalsCount { get; set; }
    public int TotalAppointments { get; set; }
    public List<Notification> PlatformNotices { get; set; } = new List<Notification>();
    public List<Professional> Professionals { get; set; } = new List<Professional>();
    public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
    public Guid EstablishmentId { get; set; }
}