namespace SistemaAgendamentoWebII.Models.ViewModels;

public class ProfessionalDashboardViewModel
{
    public User User { get; set; }
    public Professional Professional { get; set; }

    // Métricas
    public int AppointmentsTodayCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public double AverageRating { get; set; }

    // Coleções
    public IEnumerable<dynamic> UpcomingAppointments { get; set; }
    public IEnumerable<dynamic> WeeklyOverview { get; set; }
}