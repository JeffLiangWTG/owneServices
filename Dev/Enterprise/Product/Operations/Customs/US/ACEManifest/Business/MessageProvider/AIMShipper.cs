using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMShipper : IAIMParty
	{
		public AIMShipper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		readonly AsycudaBill bill;

		public ZString Name => bill.ABL_ShipperName.Left(35);
		public ZString StreetAddress => bill.ABL_ShipperStreet1.Left(35);
		public ZString CityCountyTownship => bill.ABL_ShipperCity.Left(17);
		public ZString StateOrProvince => bill.ABL_ShipperState.Left(9);
		public ZString CountryCode => bill.ABL_RN_NKShipperCountry;
		public ZString PostalCode => bill.ABL_ShipperPostcode.Left(9);
		public ZString TelephoneNumber => bill.ABL_ShipperPhone.Left(14);
	}
}
