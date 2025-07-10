using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.TW.Business
{
	public class ShippingIdentificationDataCollection : DependentCusAddInfoCollection<ShippingIdentificationData, BusinessObject>
	{
		public ShippingIdentificationDataCollection(BusinessObject master) : base(master, CusAddInfoTypeAttribute.Codes.TWShippingIdentification)
		{
		}
	}
}
