using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HConsigneeAddressTest : TestCaseWithFactory
	{
		public void TestLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, consigneeAddress.Line);
				bill.ABL_ConsigneeStreet1 = "Street1";
				bill.ABL_ConsigneeStreet2 = "Street2";
				bill.ABL_ConsigneeCity = "City";
				bill.ABL_ConsigneeState = "State";
				bill.ABL_ConsigneePostcode = "Postcode";
				bill.ABL_RN_NKConsigneeCountry = "TW";
				AssertEquals("Street1 Street2 City State Postcode Taiwan", consigneeAddress.Line);
			});
		}

		public void TestChineseLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, consigneeAddress.ChineseLine);
				bill.ABL_ConsigneeLocalStreet1 = "Street1";
				bill.ABL_ConsigneeLocalStreet2 = "Street2";
				bill.ABL_ConsigneeLocalCity = "City";
				bill.ABL_ConsigneeLocalState = "State";
				bill.ABL_ConsigneePostcode = "Postcode";
				bill.ABL_RN_NKConsigneeCountry = "TW";
				AssertEquals("台灣StateCityStreet1Street2Postcode", consigneeAddress.ChineseLine);
			});
		}

		public void TestCountryCode()
		{
			AssertNullOrEmpty(consigneeAddress.CountryCode);
		}

		public void TestCountrySubDivisionID()
		{
			AssertNullOrEmpty(consigneeAddress.CountrySubDivisionID);
		}

		public void TestCountrySubDivisionName()
		{
			AssertNullOrEmpty(consigneeAddress.CountrySubDivisionName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			consigneeAddress = new N5101HConsigneeAddress(bill);
		}

		AsycudaBill bill;
		N5101HConsigneeAddress consigneeAddress;
	}
}
