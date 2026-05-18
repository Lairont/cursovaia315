using cursovaia2.Data;
using Microsoft.EntityFrameworkCore;

namespace cursovaia2.Services
{
    public static class CustomerHelper
    {
        public static int? GetCustomerId(ApplicationDbContext context, ISession session)
        {
            var userId = session.GetInt32("UserId");
            if (!userId.HasValue)
                return null;

            return context.Customers
                .Where(c => c.UserId == userId.Value)
                .Select(c => (int?)c.Id)
                .FirstOrDefault();
        }
    }
}
