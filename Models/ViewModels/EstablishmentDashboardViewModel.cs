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
}