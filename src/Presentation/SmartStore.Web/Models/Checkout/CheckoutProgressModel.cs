using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutProgressModel : ModelBase
    {
        public CheckoutProgressStep CheckoutProgressStep { get; set; }
    }

    public enum CheckoutProgressStep
    {
        Quality,
        Quantity,
        Size,
        Schedule,
        Cart,
        Address,
        Shipping,
        Payment,
        Confirm,
        Complete
    }
}