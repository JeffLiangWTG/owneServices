using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCreditorOverrideCollection))]
	sealed class AccChargeCreditorOverrideCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeCreditorOverrideCollection>
	{
		public void TestRelationship()
		{
			var masterBizo1 = Factory.NewWithValidTestData<AccChargeCode>();
			var masterBizo2 = Factory.NewWithValidTestData<AccChargeCode>();
			var collection1 = new AccChargeCreditorOverrideCollection(masterBizo1);
			var collection2 = new AccChargeCreditorOverrideCollection(masterBizo2);
			AssertEquals("Precondition: collection1.Count", 0, collection1.Count);
			AssertEquals("Precondition: collection2.Count", 0, collection2.Count);

			var bizo1 = collection1.AddNew();
			bizo1.ACC_JobType = "ALL";
			bizo1.ACC_TransportMode = "ALL";
			bizo1.ACC_DefaultingRule = "SCA";
			bizo1.ACC_Direction = "ALL";
			bizo1.ACC_PaymentTerm = "ALL";
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			bizo1.ACC_OH_Creditor = creditor.PK;
			AssertEquals("testBizo1.X0_ParentID", masterBizo1.PK, bizo1.ACC_AC_ChargeCode);

			var bizo2 = collection2.AddNew();
			bizo2.ACC_JobType = "ALL";
			bizo2.ACC_TransportMode = "ALL";
			bizo2.ACC_DefaultingRule = "SCA";
			bizo2.ACC_Direction = "ALL";
			bizo2.ACC_PaymentTerm = "ALL";
			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			bizo2.ACC_OH_Creditor = creditor2.PK;
			AssertEquals("testBizo2.X0_ParentID", masterBizo2.PK, bizo2.ACC_AC_ChargeCode);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var masterBizo1InAnotherFactory = anotherFactory.Load<AccChargeCode>(masterBizo1.PK);
			var masterBizo2InAnotherFactory = anotherFactory.Load<AccChargeCode>(masterBizo2.PK);
			var collection1InAnotherFactory = new AccChargeCreditorOverrideCollection(masterBizo1InAnotherFactory);
			var collection2InAnotherFactory = new AccChargeCreditorOverrideCollection(masterBizo2InAnotherFactory);
			AssertEquals("collection1InAnotherFactory.Count", 1, collection1InAnotherFactory.Count);
			AssertEquals("collection2InAnotherFactory.Count", 1, collection2InAnotherFactory.Count);

			AssertEquals("Only bizo related to masterBizo1 should be loaded here.", bizo1.PK, collection1InAnotherFactory[0].PK);
			AssertEquals("Only bizo related to masterBizo2 should be loaded here.", bizo2.PK, collection2InAnotherFactory[0].PK);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var creditorOverride = CreateCreditorOverride(chargeCode, "SHP", "EXP", "ALL", ZGuid.Empty, organization.PK);
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				creditorOverride.ACC_JobType = "ALL";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.CreditorOverrides.Delete(creditorOverride);
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Priority tests

		public void TestGetChargeCreditorOverride()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var code2 = Factory.NewWithValidTestData<AccChargeCode>();
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var override1 = CreateCreditorOverride(code2, "ALL", "", "", ZGuid.Empty, organization.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "ALL", "SEA", ZGuid.Empty, organization.PK);
			var override3 = CreateCreditorOverride(code, "SHP", "EXP", "ALL", ZGuid.Empty, organization.PK);
			var override4 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", department.PK, organization.PK);
			var override5 = CreateCreditorOverride(code, "SHP", "EXP", "AIR", ZGuid.Empty, organization.PK);
			var override6 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", ZGuid.Empty, organization.PK);
			var override7 = CreateCreditorOverride(code, "BRK", "ALL", "ALL", ZGuid.Empty, organization.PK);
			var override8 = CreateCreditorOverride(code, "WKI", "ALL", "", ZGuid.Empty, organization.PK);
			var override9 = CreateCreditorOverride(code, "BRK", "IMP", "ALL", ZGuid.Empty, organization.PK);
			var override10 = CreateCreditorOverride(code, "BRK", "EXP", "AIR", ZGuid.Empty, organization.PK);
			var override11 = CreateCreditorOverride(code, "BRK", "DOM", "FIX", ZGuid.Empty, organization.PK);
			var override12 = CreateCreditorOverride(code, "BRK", "DOM", "IWT", ZGuid.Empty, organization.PK);
			var override13 = CreateCreditorOverride(code, "BRK", "DOM", "OWN", ZGuid.Empty, organization.PK);
			var override14 = CreateCreditorOverride(code, "BRK", "DOM", "MAI", ZGuid.Empty, organization.PK);
			Factory.Save();

			var result = code2.CreditorOverrides.GetCreditorOverride(null, "", "", department: ZGuid.Empty);
			AssertEquals(null, result);
			result = code2.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Consol, "", "", department: ZGuid.Empty);
			AssertEquals(null, result);
			result = code2.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Consol, "", "AIR", department: ZGuid.Empty);
			AssertEquals(null, result);
			result = code2.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Consol, "IMP", "", department: ZGuid.Empty);
			AssertEquals(override1, result);
			result = code2.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Consol, "IMP", "AIR", department: ZGuid.Empty);
			AssertEquals(override1, result);
			result = code.CreditorOverrides.GetCreditorOverride(null, "", "", department: ZGuid.Empty);
			AssertEquals(null, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Consol, "IMP", "AIR", department: ZGuid.Empty);
			AssertEquals(null, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "EXP", "SEA", department: ZGuid.Empty);
			AssertEquals(override3, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "EXP", "AIR", department: ZGuid.Empty);
			AssertEquals(override5, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "SEA", department: ZGuid.Empty);
			AssertEquals(override2, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", department: ZGuid.Empty);
			AssertEquals(override6, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "ROA", department: department.PK);
			AssertEquals(override4, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "EXP", "SEA", department: ZGuid.Empty);
			AssertEquals(override7, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "IMP", "AIR", department: ZGuid.Empty);
			AssertEquals(override9, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.WorkItem, "OTH", "", department: ZGuid.Empty);
			AssertEquals(override8, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "EXP", "AIR", department: ZGuid.Empty);
			AssertEquals(override10, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "FIX", department: ZGuid.Empty);
			AssertEquals(override11, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "IWT", department: ZGuid.Empty);
			AssertEquals(override12, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "OWN", department: ZGuid.Empty);
			AssertEquals(override13, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "DOM", "MAI", department: ZGuid.Empty);
			AssertEquals(override14, result);
			result = code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Brokerage, "IMP", "AIR", department: ZGuid.Empty);
			AssertEquals(override9, result);
		}

		public void TestGetChargeCreditorOverridePriorities_JobType()
		{
			// Priority Order:
			// 1.Job Type
			// 2.Job Direction
			// 3.Job Transport Mode
			// 4.Payment Terms
			// 5.Charge Department
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var override1 = CreateCreditorOverride(code, "ALL", "", "", "", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"GIVEN override1 has jobType=ALL WHEN GetCreditorOverride THEN should return override1",
				override1,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "ALL", "IMP", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match jobType which has higher priority than matching direction",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "ALL", "ALL", "AIR", "ALL", ZGuid.Empty, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match jobType which has higher priority than matching transport-mode",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "ALL", "ALL", "ALL", "ALL", department.PK, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match jobType which has higher priority than matching department",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "ALL", "ALL", "ALL", "PPD", ZGuid.Empty, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match jobType which has higher priority than matching payment-term",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);
		}

		public void TestGetChargeCreditorOverridePriorities_Direction()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var override1 = CreateCreditorOverride(code, "SHP", "ALL", "AIR", "ALL", ZGuid.Empty, creditor.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "IMP", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match direction which has higher priority than matching transport-mode",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", department.PK, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "IMP", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match direction which has higher priority than matching department",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "PPD", ZGuid.Empty, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "IMP", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match direction which has higher priority than matching payment-term",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);
		}

		public void TestGetChargeCreditorOverridePriorities_TransportMode()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", department.PK, creditor.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "ALL", "AIR", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match transport-mode which has higher priority than matching department",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "ALL", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "PPD", ZGuid.Empty, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "ALL", "AIR", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match transport-mode which has higher priority than matching payment-term",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);
		}

		public void TestGetChargeCreditorOverridePriorities_PaymentTerm()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", department.PK, creditor.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "PPD", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match payment-term which has higher priority than matching department",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			code = Factory.NewWithValidTestData<AccChargeCode>();
			override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "PPD", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match payment-term which has higher priority than matching blank payment-term",
				override2,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);

			AssertEquals
			(
				"override1 with empty payment-term matched null job payment-term",
				override1,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", null, department.PK)
			);
		}

		public void TestGetChargeCreditorOverridePriorities_Department()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var override1 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", department.PK, creditor.PK);
			var override2 = CreateCreditorOverride(code, "SHP", "ALL", "ALL", "ALL", ZGuid.Empty, creditor.PK);
			Factory.Save();
			AssertEquals
			(
				"override2 has match department which has higher priority than blank department",
				override1,
				code.CreditorOverrides.GetCreditorOverride(JobInvoicingConsumerTypes.Shipment, "IMP", "AIR", "PPD", department.PK)
			);
		}

		#endregion

		#region Implementation of ActiveBusinessObjectCollectionTestCase

		protected override AccChargeCreditorOverrideCollection GetCollectionToTest()
		{
			return new AccChargeCreditorOverrideCollection(Factory.New<AccChargeCode>());
		}

		#endregion

		AccChargeCreditorOverride CreateCreditorOverride(AccChargeCode chargeCode, string jobType, string direction, string transportMode, ZGuid department, ZGuid creditor, string defaultingRule = "SCA")
		{
			var creditorOverride = chargeCode.CreditorOverrides.AddNew();
			creditorOverride.ACC_JobType = jobType;
			creditorOverride.ACC_Direction = direction;
			creditorOverride.ACC_TransportMode = transportMode;
			creditorOverride.ACC_GE_Department = department;
			creditorOverride.ACC_OH_Creditor = creditor;
			creditorOverride.ACC_DefaultingRule = defaultingRule;
			creditorOverride.ACC_PaymentTerm = "ALL";

			return creditorOverride;
		}

		AccChargeCreditorOverride CreateCreditorOverride(AccChargeCode chargeCode, string jobType, string direction, string transportMode, string paymentTerm, ZGuid department, ZGuid creditor, string defaultingRule = "SCA")
		{
			var accChargeCreditorOverride = CreateCreditorOverride(chargeCode, jobType, direction, transportMode, department, creditor, defaultingRule);
			accChargeCreditorOverride.ACC_PaymentTerm = paymentTerm;
			return accChargeCreditorOverride;
		}
	}
}
