namespace Ecs.Models
{
    public class ApplicationStateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; }
        public bool HasActiveTimestamp { get; set; }

        public ApplicationStateModel()
        {
            FullName = string.Join(" ", FirstName, LastName);
        }
    }
}
