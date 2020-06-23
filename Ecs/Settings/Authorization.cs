namespace Ecs.Settings
{
    public class Authorization
    {
        public enum Roles
        {
            Administrator, User
        }

        public const string default_admin_username = "admin";
        public const string default_admin_email = "admin@example.com";
        public const string default_admin_password = "#Secret123";
        public const Roles default_admin_role = Roles.Administrator;
        public const Roles default_role = Roles.User;

    }
}
