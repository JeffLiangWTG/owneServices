using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeBranchOverrideCollection))]
	sealed class AccChargeBranchOverrideCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeBranchOverrideCollection>
	{
		public void TestRelationship()
		{
			var masterBizo1 = Factory.NewWithValidTestData<AccChargeCode>();
			var masterBizo2 = Factory.NewWithValidTestData<AccChargeCode>();
			var collection1 = new AccChargeBranchOverrideCollection(masterBizo1);
			var collection2 = new AccChargeBranchOverrideCollection(masterBizo2);
			AssertEquals("Precondition: collection1.Count", 0, collection1.Count);
			AssertEquals("Precondition: collection2.Count", 0, collection2.Count);

			var bizo1 = collection1.AddNew();
			bizo1.YA_JobType = "ALL";
			bizo1.YA_DefaultingRule = "SDT";
			AssertEquals("testBizo1.X0_ParentID", masterBizo1.PK, bizo1.YA_AC_ChargeCode);

			var bizo2 = collection2.AddNew();
			bizo2.YA_JobType = "ALL";
			bizo2.YA_DefaultingRule = "SDT";
			AssertEquals("testBizo2.X0_ParentID", masterBizo2.PK, bizo2.YA_AC_ChargeCode);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var masterBizo1InAnotherFactory = anotherFactory.Load<AccChargeCode>(masterBizo1.PK);
			var masterBizo2InAnotherFactory = anotherFactory.Load<AccChargeCode>(masterBizo2.PK);
			var collection1InAnotherFactory = new AccChargeBranchOverrideCollection(masterBizo1InAnotherFactory);
			var collection2InAnotherFactory = new AccChargeBranchOverrideCollection(masterBizo2InAnotherFactory);
			AssertEquals("collection1InAnotherFactory.Count", 1, collection1InAnotherFactory.Count);
			AssertEquals("collection2InAnotherFactory.Count", 1, collection2InAnotherFactory.Count);

			AssertEquals("Only bizo related to masterBizo1 should be loaded here.", bizo1.PK, collection1InAnotherFactory[0].PK);
			AssertEquals("Only bizo related to masterBizo2 should be loaded here.", bizo2.PK, collection2InAnotherFactory[0].PK);
		}

		public void TestGetChargeBranchOverride()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var code2 = Factory.NewWithValidTestData<AccChargeCode>();

			var override1 = CreateBranchOverride(code2, "ALL", "ALL", "ALL");
			var override2 = CreateBranchOverride(code, "SHP", "ALL", "SEA");
			var override3 = CreateBranchOverride(code, "SHP", "EXP", "ALL");
			var override4 = CreateBranchOverride(code, "BRK", "ALL", "ALL");
			var override5 = CreateBranchOverride(code, "SHP", "EXP", "AIR");
			var override6 = CreateBranchOverride(code, "SHP", "ALL", "ALL");
			var override7 = CreateBranchOverride(code, "BRK", "ALL", "SEA");
			var override8 = CreateBranchOverride(code, "BRK", "EXP", "ALL");
			var override9 = CreateBranchOverride(code, "BRK", "EXP", "AIR");
			var override10 = CreateBranchOverride(code, "BRK", "DOM", "FIX");
			var override11 = CreateBranchOverride(code, "BRK", "DOM", "IWT");
			var override12 = CreateBranchOverride(code, "BRK", "DOM", "OWN");
			var override13 = CreateBranchOverride(code, "BRK", "DOM", "MAI");
			var override14 = CreateBranchOverride(code, "WKI", "ALL", "");
			var override15 = CreateBranchOverride(code, "WKP", "ALL", "");
			var override16 = CreateBranchOverride(code, "WKR", "ALL", "");
			Factory.Save();

			var result = code2.BranchOverrides.GetChargeBranchOverride(null, "", "");
			AssertEquals(null, result);
			result = code2.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Consol, "", "");
			AssertEquals(null, result);
			result = code2.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Consol, "", "AIR");
			AssertEquals(null, result);
			result = code2.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Consol, "IMP", "");
			AssertEquals(override1, result);
			result = code2.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Consol, "IMP", "AIR");
			AssertEquals(override1, result);
			result = code.BranchOverrides.GetChargeBranchOverride(null, "", "");
			AssertEquals(null, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Consol, "IMP", "AIR");
			AssertEquals(null, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Shipment, "EXP", "SEA");
			AssertEquals(override3, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Shipment, "EXP", "AIR");
			AssertEquals(override5, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "SEA");
			AssertEquals(override2, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR");
			AssertEquals(override6, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "IMP", "AIR");
			AssertEquals(override4, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "EXP", "SEA");
			AssertEquals(override8, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "EXP", "AIR");
			AssertEquals(override9, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "IMP", "SEA");
			AssertEquals(override7, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "FIX");
			AssertEquals(override10, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "IWT");
			AssertEquals(override11, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "OWN");
			AssertEquals(override12, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "MAI");
			AssertEquals(override13, result);

			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Brokerage, "", "AIR");
			AssertEquals(null, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.WorkItem, "OTH", "");
			AssertEquals(override14, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.Project, "OTH", "");
			AssertEquals(override15, result);
			result = code.BranchOverrides.GetChargeBranchOverride(JobInvoicingConsumerTypes.WorkRequest, "OTH", "");
			AssertEquals(override16, result);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var branchOverride = CreateBranchOverride(chargeCode, "SHP", "ALL", "SEA");
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				branchOverride.YA_JobType = "ALL";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.BranchOverrides.Delete(branchOverride);
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Implementation

		AccChargeBranchOverride CreateBranchOverride(AccChargeCode chargeCode, ZString jobType, ZString direction, ZString transportMode)
		{
			var branchOverride = chargeCode.BranchOverrides.AddNew();
			branchOverride.YA_JobType = jobType;
			branchOverride.YA_Direction = direction;
			branchOverride.YA_TransportMode = transportMode;
			branchOverride.YA_GB_SpecificBranch = GlbBranch.CurrentBranch.PK;
			branchOverride.YA_DefaultingRule = "SBA";
			Assert($"Expect no Errors for {jobType} {direction} {transportMode}", !branchOverride.HasErrors);

			return branchOverride;
		}

		protected override AccChargeBranchOverrideCollection GetCollectionToTest()
		{
			return new AccChargeBranchOverrideCollection(Factory.New<AccChargeCode>());
		}

		#endregion
	}
}
