namespace Ecs.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalNumberOfEmployees { get; set; }
        public int TotalNumberOfActiveClocks { get; set; }
        public int TotalNumberOfClosedClocks { get; set; }

        public AveragesViewModel Averages { get; set; } = new AveragesViewModel();
    }
}