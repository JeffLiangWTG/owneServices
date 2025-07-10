using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HNotifyPartyAddressTest : TestCaseWithFactory
	{
		public void TestLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, notifyPartyAddress.Line);
				bill.ABL_NotifyPartyStreet1 = "Street1";
				bill.ABL_NotifyPartyStreet2 = "Street2";
				bill.ABL_NotifyPartyCity = "City";
				bill.ABL_NotifyPartyState = "State";
				bill.ABL_NotifyPartyPostcode = "Postcode";
				bill.ABL_RN_NKNotifyPartyCountry = "TW";
				AssertEquals("Street1 Street2 City State Postcode Taiwan", notifyPartyAddress.Line);
			});
		}

		public void TestChineseLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, notifyPartyAddress.ChineseLine);
				bill.ABL_NotifyPartyLocalStreet1 = "Street1";
				bill.ABL_NotifyPartyLocalStreet2 = "Street2";
				bill.ABL_NotifyPartyLocalCity = "City";
				bill.ABL_NotifyPartyLocalState = "State";
				bill.ABL_NotifyPartyPostcode = "Postcode";
				bill.ABL_RN_NKNotifyPartyCountry = "TW";
				AssertEquals("Postcode台灣CityStreet1Street2", notifyPartyAddress.ChineseLine);

				bill.ABL_RN_NKNotifyPartyCountry = "US";
				AssertEquals("Postcode美國StateCityStreet1Street2", notifyPartyAddress.ChineseLine);
			});
		}

		public void TestCountryCode()
		{
			AssertNullOrEmpty(notifyPartyAddress.CountryCode);
		}

		public void TestCountrySubDivisionID()
		{
			AssertNullOrEmpty(notifyPartyAddress.CountrySubDivisionID);
		}

		public void TestCountrySubDivisionName()
		{
			AssertNullOrEmpty(notifyPartyAddress.CountrySubDivisionName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			notifyPartyAddress = new N5101HNotifyPartyAddress(bill);
		}

		AsycudaBill bill;
		N5101HNotifyPartyAddress notifyPartyAddress;
	}
}
