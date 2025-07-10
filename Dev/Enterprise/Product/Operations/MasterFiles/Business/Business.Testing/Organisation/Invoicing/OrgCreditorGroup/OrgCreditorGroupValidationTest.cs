using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCreditorGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOG_Code_DuplicationInDB()
		{
			var creditorGroup = Factory.New<OrgCreditorGroup>();
			creditorGroup.OG_Code = "TST";
			Factory.Save();

			var targetCreditorGroup = Factory.New<OrgCreditorGroup>();
			targetCreditorGroup.OG_Code = "TS2";

			AssertNoErrors("Pre-condition", creditorGroup.OG_CodeInfo);
			AssertNoErrors(targetCreditorGroup.OG_CodeInfo);
			AssertEquals(true, creditorGroup.IsInDatabase);

			targetCreditorGroup.OG_Code = "TST";
			AssertHasError(targetCreditorGroup.OG_CodeInfo, "Creditor Group Code must be unique.");

			creditorGroup.Validation.ValidateAll();
			AssertHasError(creditorGroup.OG_CodeInfo, "Creditor Group Code must be unique.");
		}

		public void TestCheckOG_Code_DuplicationInFactory()
		{
			var creditorGroup = Factory.New<OrgCreditorGroup>();
			creditorGroup.OG_Code = "TST";

			var targetCreditorGroup = Factory.New<OrgCreditorGroup>();
			targetCreditorGroup.OG_Code = "TS2";

			AssertNoErrors("Pre-condition", creditorGroup.OG_CodeInfo);
			AssertNoErrors(targetCreditorGroup.OG_CodeInfo);
			AssertEquals(false, creditorGroup.IsInDatabase);

			targetCreditorGroup.OG_Code = "TST";
			AssertHasError(targetCreditorGroup.OG_CodeInfo, "Creditor Group Code must be unique.");

			creditorGroup.Validation.ValidateAll();
			AssertHasError(creditorGroup.OG_CodeInfo, "Creditor Group Code must be unique.");
		}

		public void TestValidateALMHoldOption()
		{
			var orgCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			var validation = new OrgCreditorGroupValidation(orgCreditorGroup);

			orgCreditorGroup.OG_DefaultHoldOption = "DNM";
			validation.ValidateOG_DefaultHoldOption();
			AssertNoNotifications(orgCreditorGroup.OG_DefaultHoldOptionInfo);

			orgCreditorGroup.OG_DefaultHoldOption = "xxx";
			validation.ValidateOG_DefaultHoldOption();
			AssertHasError(orgCreditorGroup.OG_DefaultHoldOptionInfo, "Enter a valid selection.");
		}

		public void TestDefaultHoldOption()
		{
			var orgCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			var validation = new OrgCreditorGroupValidation(orgCreditorGroup);

			AssertEquals(false, orgCreditorGroup.OG_IsAllowALM);
			AssertEquals(true, orgCreditorGroup.OG_IsAllowDNM);

			orgCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.DNM;
			validation.ValidateOG_DefaultHoldOption();
			AssertNoNotifications(orgCreditorGroup.OG_DefaultHoldOptionInfo);

			orgCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.ALM;
			validation.ValidateOG_DefaultHoldOption();
			AssertEquals(1, orgCreditorGroup.OG_DefaultHoldOptionInfo.Notifications.Count());
			AssertHasNotifications("Cannot pick a disabled hold option as default hold option.", orgCreditorGroup.OG_DefaultHoldOptionInfo);

			orgCreditorGroup.OG_IsAllowALM = true;

			AssertEquals(true, orgCreditorGroup.OG_IsAllowALM);
			AssertEquals(true, orgCreditorGroup.OG_IsAllowDNM);

			orgCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.DNM;
			validation.ValidateOG_DefaultHoldOption();
			AssertNoNotifications(orgCreditorGroup.OG_DefaultHoldOptionInfo);

			orgCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.ALM;
			validation.ValidateOG_DefaultHoldOption();
			AssertNoNotifications(orgCreditorGroup.OG_DefaultHoldOptionInfo);
		}

		public void TestDefaultHoldOptionWhenDisableAnEnabledOption()
		{
			var orgCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			var validation = new OrgCreditorGroupValidation(orgCreditorGroup);

			orgCreditorGroup.OG_IsAllowALM = true;

			AssertEquals(true, orgCreditorGroup.OG_IsAllowALM);
			AssertEquals(true, orgCreditorGroup.OG_IsAllowDNM);

			orgCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.ALM;
			validation.ValidateOG_DefaultHoldOption();
			AssertNoNotifications(orgCreditorGroup.OG_DefaultHoldOptionInfo);

			orgCreditorGroup.OG_IsAllowALM = false;

			AssertEquals(false, orgCreditorGroup.OG_IsAllowALM);

			validation.ValidateOG_DefaultHoldOption();
			AssertEquals(1, orgCreditorGroup.OG_DefaultHoldOptionInfo.Notifications.Count());
			AssertHasNotifications("Cannot pick a disabled hold option as default hold option.", orgCreditorGroup.OG_DefaultHoldOptionInfo);
		}
	}
}
