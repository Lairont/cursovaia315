using cursovaia2.Data;
using cursovaia2.ModelsDb;

namespace cursovaia2.Services
{
    public static class ReferenceDataSeeder
    {
        public static void EnsureDeliveryAndPaymentMethods(ApplicationDbContext context)
        {
            var changed = false;

            if (!context.PaymentMethods.Any())
            {
                context.PaymentMethods.AddRange(
                    new PaymentMethodDb { Name = "Банковская карта" },
                    new PaymentMethodDb { Name = "Наличные при получении" },
                    new PaymentMethodDb { Name = "QR-код / СБП" });
                changed = true;
            }

            if (!context.DeliveryMethods.Any())
            {
                context.DeliveryMethods.AddRange(
                    new DeliveryMethodDb { Name = "Курьер по городу", Price = 300 },
                    new DeliveryMethodDb { Name = "Самовывоз из магазина", Price = 0 },
                    new DeliveryMethodDb { Name = "Почта России", Price = 250 });
                changed = true;
            }

            if (changed)
                context.SaveChanges();
        }
    }
}
