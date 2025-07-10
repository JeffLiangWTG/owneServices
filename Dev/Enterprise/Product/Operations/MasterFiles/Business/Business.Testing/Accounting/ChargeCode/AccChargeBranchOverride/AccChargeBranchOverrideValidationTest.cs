using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeBranchOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckYA_JobType()
		{
			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertNoErrors(BranchOverride.YA_JobTypeInfo);

			BranchOverride.YA_JobType = "";
			AssertHasErrors(BranchOverride.YA_JobTypeInfo);

			BranchOverride.YA_JobType = "ABC";
			AssertHasErrors(BranchOverride.YA_JobTypeInfo);

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.CTOCusExportHAWB.Code;
			AssertNoErrors(BranchOverride.YA_JobTypeInfo);

			var setting1 = ChargeCode.BranchOverrides.AddNew();
			var setting2 = ChargeCode.BranchOverrides.AddNew();

			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting1.YA_JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.YA_JobTypeInfo);

			setting1.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			setting1.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			setting1.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			setting2.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			setting2.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			setting2.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;

			string expectedError = "At least one more record already sets behaviour for the same Job parameters.";
			ChargeCode.RunPreSaveValidation();
			AssertHasErrors(expectedError, setting1.YA_JobTypeInfo);
			AssertHasErrors(expectedError, setting2.YA_JobTypeInfo);

			setting1.YA_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			ChargeCode.RunPreSaveValidation();
			AssertNoErrors(setting1.YA_JobTypeInfo);
			AssertNoErrors(setting2.YA_JobTypeInfo);

			setting1.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting2.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			setting2.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			setting1.YA_Direction = Constants.FreightShipmentDirection.Code.Export;
			ChargeCode.RunPreSaveValidation();
			AssertNoErrors(setting1.YA_JobTypeInfo);
			AssertNoErrors(setting2.YA_JobTypeInfo);

			setting2.YA_Direction = Constants.FreightShipmentDirection.Code.Export;
			ChargeCode.RunPreSaveValidation();
			AssertHasErrors(expectedError, setting1.YA_JobTypeInfo);
			AssertHasErrors(expectedError, setting2.YA_JobTypeInfo);

			setting2.YA_TransportMode = Constants.TransportModes.Air;
			ChargeCode.RunPreSaveValidation();
			AssertNoErrors(setting1.YA_JobTypeInfo);
			AssertNoErrors(setting2.YA_JobTypeInfo);
		}

		public void TestCheckYA_Direction()
		{
			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			AssertNoErrors(BranchOverride.YA_DirectionInfo);

			BranchOverride.YA_Direction = "";
			AssertHasErrors(BranchOverride.YA_DirectionInfo);

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_Direction = "ABC";
			AssertHasErrors(BranchOverride.YA_DirectionInfo);

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_Direction = "ABC";
			AssertHasErrors(BranchOverride.YA_DirectionInfo);

			BranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Import;
			AssertNoErrors(BranchOverride.YA_DirectionInfo);

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_Direction = "ABC";
			AssertHasErrors(BranchOverride.YA_DirectionInfo);
		}

		public void TestCheckYA_TransportMode()
		{
			string expectedError = "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type.";

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Courier;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = "";
			AssertHasErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_TransportMode = "ABC";
			AssertHasErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Precondition: YA_DirectionInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			BranchOverride.YA_TransportMode = "ABC";
			AssertHasErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Courier;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_JobType = "CLL";
			BranchOverride.YA_TransportMode = BranchOverride.YA_TransportMode;
			AssertHasError(BranchOverride.YA_TransportModeInfo, expectedError);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertHasError(BranchOverride.YA_TransportModeInfo, expectedError);

			BranchOverride.YA_JobType = "CSH";
			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertHasError(BranchOverride.YA_TransportModeInfo, expectedError);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertHasError(BranchOverride.YA_TransportModeInfo, expectedError);

			BranchOverride.YA_JobType = "SHP";
			BranchOverride.YA_TransportMode = BranchOverride.YA_TransportMode;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertEquals("Precondition: YA_TransportInfo.ReadOnly", false, BranchOverride.YA_TransportModeInfo.ReadOnly);
			foreach (var mode in new[] { "ALL", "AIR", "FIX", "IWT", "OWN", "MAI", "RAI", "ROA", "SEA" })
			{
				BranchOverride.YA_TransportMode = mode;
				AssertNoErrors($"Brokerage TransportMode {mode}", BranchOverride.YA_TransportModeInfo);
			}

			BranchOverride.YA_TransportMode = "ABC";
			AssertHasErrors(BranchOverride.YA_TransportModeInfo);

			BranchOverride.YA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(BranchOverride.YA_TransportModeInfo);
		}

		public void TestCheckYA_DefaultingRule()
		{
			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			AssertNoErrors(BranchOverride.YA_DefaultingRuleInfo);

			BranchOverride.YA_DefaultingRule = "ABC";
			AssertHasErrors(BranchOverride.YA_DefaultingRuleInfo);

			BranchOverride.YA_DefaultingRule = "";
			AssertHasErrors(BranchOverride.YA_DefaultingRuleInfo);

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO;
			AssertHasErrors(BranchOverride.YA_DefaultingRuleInfo);

			BranchOverride.YA_DefaultingRule = "TS2";
			AssertHasErrors(BranchOverride.YA_DefaultingRuleInfo);
		}

		public void TestCheckYA_GB_SpecificBranch()
		{
			var filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(filter);
			var nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var currentBranch = ChargeCode.Company.FirstActiveBranch;

			BranchOverride.YA_DefaultingRule = "";
			BranchOverride.YA_GB_SpecificBranch = ZGuid.Empty;
			AssertNoErrors(BranchOverride.YA_GB_SpecificBranchInfo);

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			AssertHasErrors(BranchOverride.YA_GB_SpecificBranchInfo);

			BranchOverride.YA_GB_SpecificBranch = currentBranch.PK;
			AssertNoErrors(BranchOverride.YA_GB_SpecificBranchInfo);

			BranchOverride.YA_GB_SpecificBranch = nonCurrentCompanyBranch.PK;
			AssertHasErrors(BranchOverride.YA_GB_SpecificBranchInfo);

			BranchOverride.YA_GB_SpecificBranch = nonCurrentBranch.PK;
			AssertNoErrors(BranchOverride.YA_GB_SpecificBranchInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ChargeCode = Factory.New<AccChargeCode>();
			BranchOverride = ChargeCode.BranchOverrides.AddNew();
		}

		AccChargeCode ChargeCode;
		AccChargeBranchOverride BranchOverride;
	}
}
