using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsignorAddressTest : TestCaseWithFactory
	{
		public void TestLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, consignorAddress.Line);
				bill.ABL_ShipperStreet1 = "Street1";
				bill.ABL_ShipperStreet2 = "Street2";
				bill.ABL_ShipperCity = "City";
				bill.ABL_ShipperState = "State";
				bill.ABL_ShipperPostcode = "Postcode";
				bill.ABL_RN_NKShipperCountry = "TW";
				AssertEquals("Street1 Street2 City State Postcode Taiwan", consignorAddress.Line);
			});
		}

		public void TestChineseLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, consignorAddress.ChineseLine);
				bill.ABL_ShipperLocalStreet1 = "Street1";
				bill.ABL_ShipperLocalStreet2 = "Street2";
				bill.ABL_ShipperLocalCity = "City";
				bill.ABL_ShipperLocalState = "State";
				bill.ABL_ShipperPostcode = "Postcode";
				bill.ABL_RN_NKShipperCountry = "TW";
				AssertEquals("台灣StateCityStreet1Street2Postcode", consignorAddress.ChineseLine);
			});
		}

		public void TestCountryCode()
		{
			AssertNullOrEmpty(consignorAddress.CountryCode);
		}

		public void TestCountrySubDivisionID()
		{
			AssertNullOrEmpty(consignorAddress.CountrySubDivisionID);
		}

		public void TestCountrySubDivisionName()
		{
			AssertNullOrEmpty(consignorAddress.CountrySubDivisionName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			consignorAddress = new N5101HConsignorAddress(bill);
		}

		AsycudaBill bill;
		N5101HConsignorAddress consignorAddress;
	}
}
