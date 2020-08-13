namespace ECS.Models.ApiModels
{
    public class AuthenticationModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
