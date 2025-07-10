using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMConsignee : IAIMParty
	{
		public AIMConsignee(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		readonly AsycudaBill bill;

		public ZString Name => bill.ABL_ConsigneeName.Left(35);
		public ZString StreetAddress => bill.ABL_ConsigneeStreet1.Left(35);
		public ZString CityCountyTownship => bill.ABL_ConsigneeCity.Left(17);
		public ZString StateOrProvince => bill.ABL_ConsigneeState.Left(9);
		public ZString CountryCode => bill.ABL_RN_NKConsigneeCountry;
		public ZString PostalCode => bill.ABL_ConsigneePostcode.Left(9);
		public ZString TelephoneNumber => bill.ABL_ConsigneePhone.Replace("+", "").Replace(" ", "").Left(14);
	}
}
