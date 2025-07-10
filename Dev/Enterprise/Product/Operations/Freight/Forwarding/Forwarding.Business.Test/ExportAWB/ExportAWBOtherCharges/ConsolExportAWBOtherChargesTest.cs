using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBOtherCharges))]
	sealed class ConsolExportAWBOtherChargesTest : ExportAWBOtherChargesTest
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Master Air Waybill Other Charge", OtherCharges.HumanReadableName);
		}

		public void TestEO_ChargeCode_SetsEntitlementCode()
		{
			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, OtherCharges.EO_EntitlementCode);

			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.MA; // with "due agent" in description
			AssertEquals(Core.Constants.AWB.EntitlementCode.Agent, OtherCharges.EO_EntitlementCode);

			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AW;
			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, OtherCharges.EO_EntitlementCode);
		}

		public void TestGetNewValidation()
		{
			AssertEquals("Type of Validation", typeof(ConsolExportAWBOtherChargesValidation), OtherCharges.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ConsolExportAWBOtherCharges>();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			OtherCharges.Master.EH_ParentID = consol.PK;
			return OtherCharges;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var header = factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.ForceSavingByFactory = true;
			header.EH_ParentID = consol.PK;

			var result = factory.NewWithValidTestData<ConsolExportAWBOtherCharges>();
			result.EO_EH = header.PK;

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			OtherCharges = Factory.New<ConsolExportAWBOtherCharges>();
			OtherCharges.EO_EH = Factory.New(typeof(ConsolExportAWBHeader)).PK;
		}

		#endregion
	}
}
