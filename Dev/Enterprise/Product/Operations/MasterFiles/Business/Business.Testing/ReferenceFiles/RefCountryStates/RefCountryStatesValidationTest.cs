using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryStatesValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation
		public void TestCheckRW_Code()
		{
			CountryStates.RW_Code = ZString.Empty;
			Assert("Expecting RW_Code to be empty and have errors.", CountryStates.RW_CodeInfo.HasErrors());
			CountryStates.RW_Code = new ZString("er");
			Assert("RW_Code should be correct, not expecting errors.", !CountryStates.RW_CodeInfo.HasNotifications());
		}

		public void TestCheckRW_Desc()
		{
			CountryStates.RW_Description = ZString.Empty;
			Assert("Expecting RW_Description to be empty and have errors.", CountryStates.RW_DescriptionInfo.HasErrors());
			CountryStates.RW_Description = new ZString("New South Wales");
			Assert("RW_Description should be correct, not expecting errors.", !CountryStates.RW_DescriptionInfo.HasNotifications());
		}

		public void TestCheckRW_RN_NKCountryCode()
		{
			CountryStates.RW_RN_NKCountryCode = ZString.Empty;
			Assert("Expecting RW_RN_NKCountryCode to be empty and have errors.", CountryStates.RW_RN_NKCountryCodeInfo.HasErrors());
		}

		#endregion

		#region Implementation

		BusinessObjectFactory TestFactory;
		RefCountryStates CountryStates;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			CountryStates = TestFactory.New(typeof(RefCountryStates)) as RefCountryStates;
		}

		#endregion

	}
}
