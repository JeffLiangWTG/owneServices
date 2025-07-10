using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCityTownValidationTest : BusinessObjectValidationTestCase
	{
		#region R9_InternationalName

		public void TestCheckR9_InternationalName()
		{
			refCityTown.R9_InternationalName = "";
			Assert("No InternationalName, has errors", refCityTown.R9_InternationalNameInfo.HasErrors());

			refCityTown.R9_InternationalName = "International Name";
			Assert("Valid InternationalName, no errors", !refCityTown.R9_InternationalNameInfo.HasErrors());
		}

		#endregion

		#region R9_RN_NKCountry

		public void TestCheckR9_RN_NKCountry()
		{
			refCityTown.R9_RN_NKCountry = "";
			Assert("No Country, has errors", refCityTown.R9_RN_NKCountryInfo.HasErrors());

			refCityTown.R9_RN_NKCountry = "US";
			Assert("Valid Country, no errors", !refCityTown.R9_RN_NKCountryInfo.HasErrors());
		}

		#endregion

		#region R9_RW_NKState

		public void TestCheckR9_RW_NKState()
		{
			refCityTown.R9_RW_NKState = "";
			AssertEquals(false, refCityTown.R9_RW_NKStateInfo.HasNotifications());

			refCityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			refCityTown.Validation.ValidateR9_RW_NKState();
			AssertEquals(true, refCityTown.R9_RW_NKStateInfo.HasWarnings());

			refCityTown.R9_RW_NKState = "QQ";
			AssertEquals(true, refCityTown.R9_RW_NKStateInfo.HasErrors());

			refCityTown.R9_RW_NKState = "01";
			AssertEquals(true, refCityTown.R9_RW_NKStateInfo.HasErrors());

			refCityTown.R9_RW_NKState = "NSW";
			AssertEquals(false, refCityTown.R9_RW_NKStateInfo.HasWarnings());
			AssertEquals(false, refCityTown.R9_RW_NKStateInfo.HasErrors());
		}

		#endregion

		#region Implementation

		RefCityTown refCityTown;

		protected override void SetUp()
		{
			base.SetUp();

			refCityTown = Factory.New<RefCityTown>();
		}

		#endregion
	}
}
