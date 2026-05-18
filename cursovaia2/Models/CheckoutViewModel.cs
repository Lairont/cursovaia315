using cursovaia2.ModelsDb;

namespace cursovaia2.Models
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new();
        public List<DeliveryMethodDb> DeliveryMethods { get; set; } = new();
        public List<PaymentMethodDb> PaymentMethods { get; set; } = new();
        public decimal Subtotal { get; set; }
    }
}
