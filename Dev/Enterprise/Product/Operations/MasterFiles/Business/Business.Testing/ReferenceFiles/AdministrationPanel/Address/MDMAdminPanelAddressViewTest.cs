using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MDMAdminPanelAddressView))]
	sealed class MDMAdminPanelAddressViewTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This is a view, doesn't need to save", true);
		}

		public void TestIsValidationEnabledCore()
		{
			var mdmView = Factory.NewWithValidTestData<MDMAdminPanelAddressViewForTest>();
			AssertEquals("There is no need to validate a view.", false, mdmView.IsValidationEnabledCoreForTest);
		}

		public void TestHasAddressInfoChanges()
		{
			var mdmView = Factory.NewWithValidTestData<MDMAdminPanelAddressViewForTest>();

			mdmView.HasAddressInfoChanges = false;
			Assert(!mdmView.HasAddressInfoChanges);

			mdmView.HasAddressInfoChanges = true;
			Assert(mdmView.HasAddressInfoChanges);
		}

		public void TestAutoVerifyState()
		{
			var mdmView = Factory.NewWithValidTestData<MDMAdminPanelAddressViewForTest>();

			mdmView.AutoVerifyState = AddressAutoVerifyState.Waiting;
			AssertEquals(AddressAutoVerifyState.Waiting, mdmView.AutoVerifyState);

			mdmView.AutoVerifyState = AddressAutoVerifyState.Verifying;
			AssertEquals(AddressAutoVerifyState.Verifying, mdmView.AutoVerifyState);

			mdmView.AutoVerifyState = AddressAutoVerifyState.Skipped;
			AssertEquals(AddressAutoVerifyState.Skipped, mdmView.AutoVerifyState);

			mdmView.AutoVerifyState = AddressAutoVerifyState.Verified;
			AssertEquals(AddressAutoVerifyState.Verified, mdmView.AutoVerifyState);
		}
	}
}
