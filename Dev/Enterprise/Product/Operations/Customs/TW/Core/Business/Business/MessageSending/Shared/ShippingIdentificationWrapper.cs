using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ShippingIdentificationWrapper : IShippingIdentification
	{
		public ShippingIdentificationWrapper(ZString lotNumberID, ZDateTime productBestBeforeDateTime, ZDecimal productLotNumberAmount, ZDateTime productManufacturedDate)
		{
			LotNumberID = lotNumberID;
			ProductBestBeforeDateTime = productBestBeforeDateTime;
			ProductLotNumberAmount = productLotNumberAmount;
			ProductManufacturedDate = productManufacturedDate;
		}

		public ZString LotNumberID { get; }

		public ZDateTime ProductBestBeforeDateTime { get; }

		public ZDecimal ProductLotNumberAmount { get; }

		public ZDateTime ProductManufacturedDate { get; }
	}
}
