using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ChargeCodeOverrideRulesRankerTest : TestCaseWithFactory
	{
		public void TestGetBestSellComplianceDescriptionOverride()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			var sellComplianceDescription1 = chargeCode.ChargeComplianceDescriptions.AddNew();
			sellComplianceDescription1.ADE_JobType = new AllJobsConsumerType().Code;
			sellComplianceDescription1.ADE_TransportMode = "";
			sellComplianceDescription1.ADE_SupplyType = SupplyTypeClassificationCodes.INT;
			sellComplianceDescription1.ADE_Description = "TEST ALL INT";

			var sellComplianceDescription2 = chargeCode.ChargeComplianceDescriptions.AddNew();
			sellComplianceDescription2.ADE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			sellComplianceDescription2.ADE_TransportMode = Core.Constants.TransportModes.Air;
			sellComplianceDescription2.ADE_SupplyType = SupplyTypeClassificationCodes.LOC;
			sellComplianceDescription2.ADE_Description = "TEST SHP AIR LOC";

			var sellComplianceDescription3 = chargeCode.ChargeComplianceDescriptions.AddNew();
			sellComplianceDescription3.ADE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			sellComplianceDescription3.ADE_TransportMode = Core.Constants.FreightShipmentDirection.Code.All;
			sellComplianceDescription3.ADE_SupplyType = SupplyTypeClassificationCodes.LOX;
			sellComplianceDescription3.ADE_Description = "TEST SHP ALL LOX";

			Factory.Save();

			var completeFilter = chargeCode.ChargeComplianceDescriptions.CompleteFilter;
			AssertEquals($"ADE_AC = '{chargeCode.PK}'", completeFilter.ParameterisedText.LiteralTextSql);

			var chargeCodeOverrideRulesRanker = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeCodeOverrideRulesRanker();
			AssertRanker_GetBestSellComplianceDescriptionOverride(null, null, null, ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride(null, null, "", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("ALL", "", "", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("ALL", "", "INT", "TEST ALL INT");
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "ALL", "", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "SEA", "", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "AIR", "", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "ALL", "LOC", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "SEA", "LOC", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("ZZZ", "ZZZ", "ZZZ", ZString.Empty);
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "AIR", "LOC", "TEST SHP AIR LOC");
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "ALL", "LOX", "TEST SHP ALL LOX");
			AssertRanker_GetBestSellComplianceDescriptionOverride("SHP", "AIR", "LOX", "TEST SHP ALL LOX");

			void AssertRanker_GetBestSellComplianceDescriptionOverride(ZString? jobType, ZString? transportMode, ZString supplyType, ZString expectedValue)
			{
				var actualValue = chargeCodeOverrideRulesRanker.GetBestSellComplianceDescriptionOverride(chargeCode.Factory, chargeCode.ChargeComplianceDescriptions, jobType, transportMode, supplyType);

				AssertEquals(expectedValue, actualValue);
			}
		}
	}
}
