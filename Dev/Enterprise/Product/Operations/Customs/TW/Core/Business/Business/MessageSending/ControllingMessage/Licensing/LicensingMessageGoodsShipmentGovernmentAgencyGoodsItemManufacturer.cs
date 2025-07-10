using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer : LicensingMessageManufacturer
	{
		public LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer(TWJobDocAddress localManufacturer)
			: base(localManufacturer)
		{
		}

		protected override ZString IDCore => ManufacturerAddress.FRICode;
	}
}
