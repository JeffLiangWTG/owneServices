using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeBranchOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", Lookups.JobTypeList[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", Lookups.JobTypeList[0] is JobInvoicingConsumerType);
			AssertEquals("The JobTypeList should contain 'SHP'", true, Lookups.JobTypeList.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, Lookups.JobTypeList.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, Lookups.JobTypeList.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, Lookups.JobTypeList.ContainsCode("BRK"));
		}

		public void TestDirectionList()
		{
			AssertEquals("DirectionList.Count", 5, Lookups.DirectionList.Count);
			AssertEquals("The DirectionList should contain 'All' as the first element", "ALL", Lookups.DirectionList[0].Code);
			AssertEquals("The DirectionList should contain 'Import'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			AssertEquals("The DirectionList should contain 'Export'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			AssertEquals("The DirectionList should contain 'Domestic'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			AssertEquals("The DirectionList should contain 'Other'", true, Lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
		}

		public void TestModeList()
		{
			AssertEquals("TransportModeList.Count", 8, Lookups.TransportModeList.Count);
			AssertEquals("The TransportModeList should contain 'ALL' as the first element", "ALL", Lookups.TransportModeList[0].Code);
			AssertEquals("The TransportModeList should contain 'AIR'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Air));
			AssertEquals("The TransportModeList should contain 'SEA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Sea));
			AssertEquals("The TransportModeList should contain 'ROA'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Road));
			AssertEquals("The TransportModeList should contain 'RAI'", true, Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Rail));
		}

		public void TestDefaultingRuleList()
		{
			BranchOverride.YA_JobType = "";
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SBA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways));

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SBA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways));

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SBA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways));

			BranchOverride.YA_JobType = "DMY";
			AssertEquals("DefaultingRuleList.Count", 1, Lookups.DefaultingRuleList.Count);
			AssertEquals("The DefaultingRuleList should contain 'SBA'", true, Lookups.DefaultingRuleList.ContainsCode(Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways));
		}

		public void TestSpecificBranches()
		{
			var newFactory = new BusinessObjectFactory();
			var filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			var inactiveBranch = newFactory.LoadTop1<GlbBranch>(filter);
			inactiveBranch.GB_IsActive = false;
			newFactory.Save();

			Lookups.SpecificBranches.Load();
			AssertEquals("The SpecificBranches should contain only current company branches", false, Lookups.SpecificBranches.Any(branch => ((GlbBranch)branch).GB_GC != GlbCompany.CurrentCompany.PK));
			AssertEquals("The SpecificBranches should not contain inactive branches", false, Lookups.SpecificBranches.Any(branch => !((GlbBranch)branch).GB_IsActive));
		}

		public void TestSpecificBranchesUsesChargeCodeCompany()
		{
			var newFactory = new BusinessObjectFactory();
			var filter = new ZQuery(GlbBranchSchema.GB_GC, BranchOverride.ChargeCode.Company.PK);
			filter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = BranchOverride.ChargeCode.Company.PK;
			branch1.GB_IsActive = false;
			newFactory.Save();

			Lookups.SpecificBranches.Load();
			AssertEquals("The SpecificBranches should contain only company branches from the Charge Code Company", false, Lookups.SpecificBranches.Any(branch => ((GlbBranch)branch).GB_GC != BranchOverride.ChargeCode.Company.PK));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany chargeCompany = Factory.NewWithValidTestData<GlbCompany>();
			AccChargeCode testChargeCode = Factory.New<AccChargeCode>();
			BranchOverride = testChargeCode.BranchOverrides.AddNew();
			testChargeCode.AC_GC = chargeCompany.PK;
			Lookups = BranchOverride.Lookups;
		}
		AccChargeBranchOverrideLookups Lookups;
		AccChargeBranchOverride BranchOverride;

		#endregion
	}
}
