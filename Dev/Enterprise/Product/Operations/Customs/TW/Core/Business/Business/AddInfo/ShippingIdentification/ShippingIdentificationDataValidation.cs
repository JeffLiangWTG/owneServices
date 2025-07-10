using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.TW.Business
{
	public class ShippingIdentificationDataValidation : CusAddInfoValidation
	{
		public ShippingIdentificationDataValidation(ShippingIdentificationData parent) : base(parent)
		{
		}

		public new ShippingIdentificationData Parent => (ShippingIdentificationData)base.Parent;
	}
}
