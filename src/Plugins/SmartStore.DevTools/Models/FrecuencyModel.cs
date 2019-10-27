using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DevTools.Models
{
    public class FrecuencyModel : ModelBase
    {
        public string Text { get; set; }

        public bool Selected { get; set; }

        public int Value { get; set; }
    }
}