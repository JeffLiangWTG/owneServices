using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCommodityCodeValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation
		public void TestDuplicateRH_Code()
		{
			var code = Factory.NewWithValidTestData<RefCommodityCode>();
			code.RH_Code = "XXXZ";
			Factory.Save();
			CommodityCode.FillWithValidTestData();
			CommodityCode.RH_Code = "XXXZ";
			AssertHasError(CommodityCode.RH_CodeInfo, "Another Commodity with this code XXXZ already exists.");
			CommodityCode.RH_Code = "XXXR";
			AssertNoErrors(CommodityCode.RH_CodeInfo);
			Factory.Save();
			CommodityCode.RH_Code = "XXXZ";
			AssertHasError(CommodityCode.RH_CodeInfo, "Another Commodity with this code XXXZ already exists.");
		}

		public void TestValidateRH_Code()
		{
			CommodityCode.RH_Code = ZString.Empty;
			Assert("Expecting RH_Code to be empty and have errors.", CommodityCode.RH_CodeInfo.HasErrors());
			CommodityCode.RH_Code = new ZString("1");
			Assert("Expecting RH_Code to be 1 char long and have errors.", CommodityCode.RH_CodeInfo.HasErrors());
			CommodityCode.RH_Code = new ZString("er");
			Assert("RH_Code should be correct, not expecting errors.", !CommodityCode.RH_CodeInfo.HasNotifications());
			CommodityCode.RH_Code = new ZString("ere");
			Assert("RH_Code should be correct, not expecting errors.", !CommodityCode.RH_CodeInfo.HasNotifications());
			CommodityCode.RH_Code = new ZString("erer");
			Assert("RH_Code should be correct, not expecting errors.", !CommodityCode.RH_CodeInfo.HasNotifications());
		}

		public void TestValidateRH_Description()
		{
			CommodityCode.RH_Description = ZString.Empty;
			Assert("Expecting RH_Description to be empty and have errors.", CommodityCode.RH_DescriptionInfo.HasErrors());
			CommodityCode.RH_Description = new ZString("Australia, Dollars");
			Assert("RH_Description should be correct, not expecting errors.", !CommodityCode.RH_DescriptionInfo.HasNotifications());
		}

		public void TestValidateCommodityType()
		{
			CommodityCode.RH_IsForwarding = false;
			CommodityCode.RH_IsShipping = false;
			CommodityCode.RH_IsLandTransport = false;
			CommodityCode.RH_IsPersonalEffects = false;
			CommodityCode.RunPreSaveValidation();
			AssertHasError(CommodityCode.RH_IsForwardingInfo, "A commodity code must at least be configured to apply to one of Forwarding, Shipping, Land Transport or Personal Effects.");
			AssertHasError(CommodityCode.RH_IsShippingInfo, "A commodity code must at least be configured to apply to one of Forwarding, Shipping, Land Transport or Personal Effects.");
			AssertHasError(CommodityCode.RH_IsLandTransportInfo, "A commodity code must at least be configured to apply to one of Forwarding, Shipping, Land Transport or Personal Effects.");
			AssertHasError(CommodityCode.RH_IsPersonalEffectsInfo, "A commodity code must at least be configured to apply to one of Forwarding, Shipping, Land Transport or Personal Effects.");

			CommodityCode.RH_IsForwarding = true;
			CommodityCode.RH_IsShipping = false;
			CommodityCode.RH_IsLandTransport = false;
			CommodityCode.RH_IsPersonalEffects = false;
			AssertNoErrors(CommodityCode.RH_IsForwardingInfo);
			AssertNoErrors(CommodityCode.RH_IsShippingInfo);
			AssertNoErrors(CommodityCode.RH_IsLandTransportInfo);
			AssertNoErrors(CommodityCode.RH_IsPersonalEffectsInfo);

			CommodityCode.RH_IsForwarding = true;
			CommodityCode.RH_IsShipping = false;
			CommodityCode.RH_IsLandTransport = true;
			CommodityCode.RH_IsPersonalEffects = true;
			AssertNoErrors(CommodityCode.RH_IsForwardingInfo);
			AssertNoErrors(CommodityCode.RH_IsShippingInfo);
			AssertNoErrors(CommodityCode.RH_IsLandTransportInfo);
			AssertNoErrors(CommodityCode.RH_IsPersonalEffectsInfo);

			CommodityCode.RH_IsForwarding = false;
			CommodityCode.RH_IsShipping = false;
			CommodityCode.RH_IsLandTransport = false;
			CommodityCode.RH_IsPersonalEffects = true;
			AssertNoErrors(CommodityCode.RH_IsForwardingInfo);
			AssertNoErrors(CommodityCode.RH_IsShippingInfo);
			AssertNoErrors(CommodityCode.RH_IsLandTransportInfo);
			AssertNoErrors(CommodityCode.RH_IsPersonalEffectsInfo);
		}

		public void TestValidateRH_IATACommodityItem()
		{
			var messageCodeLengthWarning = "Enter an IATA Commodity between 4 and 7 digits long.";
			var messageNotExistWarning = "This code does not match IATA Specific Commodity.";

			CommodityCode.RH_IATACommodityItem = "123";
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "123s";
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "123@";
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = string.Empty;
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "1234";
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "12345";
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "123456";
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			CommodityCode.RH_IATACommodityItem = "1234567";
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			var iataCommodityCode = TestFactory.New<RefAirlineCommodityCode>();
			iataCommodityCode.RAC_Code = "12345";
			iataCommodityCode.RAC_AirlineID = "";
			CommodityCode.RH_IATACommodityItem = iataCommodityCode.RAC_Code;
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);

			var airlineSpecificCommodityCode = TestFactory.New<RefAirlineCommodityCode>();
			airlineSpecificCommodityCode.RAC_Code = "1234";
			airlineSpecificCommodityCode.RAC_AirlineID = "123";
			CommodityCode.RH_IATACommodityItem = airlineSpecificCommodityCode.RAC_Code;
			AssertNoWarning(CommodityCode.RH_IATACommodityItemInfo, messageCodeLengthWarning);
			AssertHasWarning(CommodityCode.RH_IATACommodityItemInfo, messageNotExistWarning);
		}

		BusinessObjectFactory TestFactory;
		RefCommodityCode CommodityCode;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			CommodityCode = TestFactory.New(typeof(RefCommodityCode)) as RefCommodityCode;
		}

		#endregion

	}
}
