using cursovaia2.ModelsDb;

namespace cursovaia2.Services
{
    public static class AuthSession
    {
        public const string AdminRoleName = "admin";
        public const string UserRoleName = "user";

        public static void SetUser(ISession session, UserDb user)
        {
            session.SetInt32("UserId", user.Id);
            session.SetString("Email", user.Email);

            if (user.RoleId.HasValue)
                session.SetInt32("RoleId", user.RoleId.Value);
            else
                session.Remove("RoleId");

            if (user.Role != null)
                session.SetString("RoleName", user.Role.Name);
            else
                session.Remove("RoleName");
        }

        public static bool IsLoggedIn(ISession session) =>
            session.GetInt32("UserId").HasValue;

        public static bool IsAdmin(ISession session) =>
            string.Equals(session.GetString("RoleName"), AdminRoleName, StringComparison.OrdinalIgnoreCase);
    }
}
