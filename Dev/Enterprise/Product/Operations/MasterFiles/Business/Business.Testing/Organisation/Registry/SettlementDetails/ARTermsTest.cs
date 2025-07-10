using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARTerms))]
	sealed class ARTermsTest : RegistryBusinessObjectTemplateTestCase
	{
		ZXmlSerializer TermsSerialiser
		{
			get
			{
				return termsSerialiser ?? (termsSerialiser = ZXmlSerializer.New(typeof(ARTerms)));
			}
		}
		ZXmlSerializer termsSerialiser;

		public void TestJobType()
		{
			Terms1.JobType = "SHP";
			AssertEquals("JobType", "SHP", Terms1.JobType);
			AssertEquals("Direction", "ALL", Terms1.Direction);
			AssertEquals("TransportMode", "ALL", Terms1.TransportMode);
			Assert("DirectionInfo not readonly", !Terms1.DirectionInfo.ReadOnly);
			Assert("TransportModeInfo not readonly", !Terms1.TransportModeInfo.ReadOnly);

			Terms1.JobType = "BRK";
			AssertEquals("JobType", "BRK", Terms1.JobType);
			AssertEquals("Direction", "ALL", Terms1.Direction);
			AssertEquals("TransportMode", "ALL", Terms1.TransportMode);
			Assert("DirectionInfo not readonly", !Terms1.DirectionInfo.ReadOnly);
			Assert("TransportModeInfo not readonly", !Terms1.TransportModeInfo.ReadOnly);

			Terms1.JobType = "WKI";
			AssertEquals("JobType", "WKI", Terms1.JobType);
			AssertEquals("Direction", "ALL", Terms1.Direction);
			AssertEquals("TransportMode", "ALL", Terms1.TransportMode);
			Assert("DirectionInfo readonly", Terms1.DirectionInfo.ReadOnly);
			Assert("TransportModeInfo readonly", Terms1.TransportModeInfo.ReadOnly);
		}

		public void TestXMLSerializationWithARTermsCycles()
		{
			Terms1.InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			Terms1.InvoiceDays = 3;
			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			Terms1.TreatDisbursementsAsStandardValue = 2.5;
			var cycle1 = Terms1.ARTermsCycles.AddNew();
			var cycle2 = Terms1.ARTermsCycles.AddNew();
			cycle1.ToDay = 3;
			cycle1.PaymentDay = 6;
			cycle2.ToDay = 9;
			cycle2.PaymentDay = 12;

			string result;
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				TermsSerialiser.Serialize(writer, Terms1);
				result = stringWriter.ToString();
			}

			AssertContains("<ARTerms><JobType>ALL</JobType><BranchPK>00000000-0000-0000-0000-000000000000</BranchPK><DeptPK>00000000-0000-0000-0000-000000000000</DeptPK><Direction>ALL</Direction><TransportMode>ALL</TransportMode><InvoiceClass>DSB</InvoiceClass><InvoiceDays>3</InvoiceDays><InvoiceTerm>MIC</InvoiceTerm><TreatDisbursementsAsStandardValue>2.5</TreatDisbursementsAsStandardValue><ARTermsCycles><ARTermsCycle><ToDay>3</ToDay><PaymentDay>6</PaymentDay></ARTermsCycle><ARTermsCycle><ToDay>9</ToDay><PaymentDay>12</PaymentDay></ARTermsCycle></ARTermsCycles><ARPaymentCycles /></ARTerms>", result);

			using (StringReader stringReader = new StringReader(result))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				Terms2 = (ARTerms)TermsSerialiser.Deserialize(reader);
			}

			AssertEquals(OrgARTermsLookups.InvoiceTypes.DSB.Code, Terms2.InvoiceClass);
			AssertEquals((ZByte)3, Terms2.InvoiceDays);
			AssertEquals(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, Terms2.InvoiceTerm);
			AssertEquals(new ZDecimal(2.5m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(2, Terms2.ARTermsCycles.Count);
			AssertEquals((ZByte)3, Terms2.ARTermsCycles[0].ToDay);
			AssertEquals((ZByte)6, Terms2.ARTermsCycles[0].PaymentDay);
			AssertEquals((ZByte)9, Terms2.ARTermsCycles[1].ToDay);
			AssertEquals((ZByte)12, Terms2.ARTermsCycles[1].PaymentDay);
		}

		public void TestCloneWithARTermsCycles()
		{
			Terms1.InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			Terms1.InvoiceDays = 3;
			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			Terms1.TreatDisbursementsAsStandardValue = 2.5;
			var cycle1 = Terms1.ARTermsCycles.AddNew();
			var cycle2 = Terms1.ARTermsCycles.AddNew();
			cycle1.ToDay = 3;
			cycle1.PaymentDay = 6;
			cycle2.ToDay = 9;
			cycle2.PaymentDay = 12;

			Terms2 = (ARTerms)Terms1.Clone(Terms1.CurrentFallbackLevel, Factory);

			AssertEquals(OrgARTermsLookups.InvoiceTypes.DSB.Code, Terms2.InvoiceClass);
			AssertEquals((ZByte)3, Terms2.InvoiceDays);
			AssertEquals(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, Terms2.InvoiceTerm);
			AssertEquals(new ZDecimal(2.5m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(2, Terms2.ARTermsCycles.Count);
			AssertEquals((ZByte)3, Terms2.ARTermsCycles[0].ToDay);
			AssertEquals((ZByte)6, Terms2.ARTermsCycles[0].PaymentDay);
			AssertEquals((ZByte)9, Terms2.ARTermsCycles[1].ToDay);
			AssertEquals((ZByte)12, Terms2.ARTermsCycles[1].PaymentDay);
		}

		public void TestXMLSerializationWithARPaymentCycles()
		{
			Terms1.InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			Terms1.InvoiceDays = 4;
			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			Terms1.TreatDisbursementsAsStandardValue = 3.5;
			var cycle1 = Terms1.ARPaymentCycles.AddNew();
			var cycle2 = Terms1.ARPaymentCycles.AddNew();
			cycle1.ToDay = 1;
			cycle1.PaymentDay = 4;
			cycle2.ToDay = 2;
			cycle2.PaymentDay = 9;

			string result;
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				TermsSerialiser.Serialize(writer, Terms1);
				result = stringWriter.ToString();
			}

			AssertContains("<ARTerms><JobType>ALL</JobType><BranchPK>00000000-0000-0000-0000-000000000000</BranchPK><DeptPK>00000000-0000-0000-0000-000000000000</DeptPK><Direction>ALL</Direction><TransportMode>ALL</TransportMode><InvoiceClass>ALL</InvoiceClass><InvoiceDays>4</InvoiceDays><InvoiceTerm>DPC</InvoiceTerm><TreatDisbursementsAsStandardValue>3.5</TreatDisbursementsAsStandardValue><ARTermsCycles /><ARPaymentCycles><ARPaymentCycle><ToDay>1</ToDay><PaymentDay>4</PaymentDay></ARPaymentCycle><ARPaymentCycle><ToDay>2</ToDay><PaymentDay>9</PaymentDay></ARPaymentCycle></ARPaymentCycles></ARTerms>", result);

			using (StringReader stringReader = new StringReader(result))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				Terms2 = (ARTerms)TermsSerialiser.Deserialize(reader);
			}

			AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, Terms2.InvoiceClass);
			AssertEquals((ZByte)4, Terms2.InvoiceDays);
			AssertEquals(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, Terms2.InvoiceTerm);
			AssertEquals(new ZDecimal(3.5m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(2, Terms2.ARPaymentCycles.Count);
			AssertEquals((ZByte)1, Terms2.ARPaymentCycles[0].ToDay);
			AssertEquals((ZByte)4, Terms2.ARPaymentCycles[0].PaymentDay);
			AssertEquals((ZByte)2, Terms2.ARPaymentCycles[1].ToDay);
			AssertEquals((ZByte)9, Terms2.ARPaymentCycles[1].PaymentDay);
		}

		public void TestCloneWithARPaymentCycles()
		{
			Terms1.InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			Terms1.InvoiceDays = 4;
			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			Terms1.TreatDisbursementsAsStandardValue = 3.5;
			var cycle1 = Terms1.ARPaymentCycles.AddNew();
			var cycle2 = Terms1.ARPaymentCycles.AddNew();
			cycle1.ToDay = 1;
			cycle1.PaymentDay = 4;
			cycle2.ToDay = 2;
			cycle2.PaymentDay = 9;

			Terms2 = (ARTerms)Terms1.Clone(Terms1.CurrentFallbackLevel, Factory);

			AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, Terms2.InvoiceClass);
			AssertEquals((ZByte)4, Terms2.InvoiceDays);
			AssertEquals(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, Terms2.InvoiceTerm);
			AssertEquals(new ZDecimal(3.5m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(2, Terms2.ARPaymentCycles.Count);
			AssertEquals((ZByte)1, Terms2.ARPaymentCycles[0].ToDay);
			AssertEquals((ZByte)4, Terms2.ARPaymentCycles[0].PaymentDay);
			AssertEquals((ZByte)2, Terms2.ARPaymentCycles[1].ToDay);
			AssertEquals((ZByte)9, Terms2.ARPaymentCycles[1].PaymentDay);
		}

		public void TestInvoiceTermValidation()
		{
			Terms1.InvoiceDays = 2;
			Terms2.InvoiceDays = 3;
			Terms3.InvoiceDays = 4;

			Terms1.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Terms2.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Terms3.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertNoErrors(Terms1.InvoiceTermInfo);
			AssertNoErrors(Terms2.InvoiceTermInfo);
			AssertNoErrors(Terms3.InvoiceTermInfo);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertHasError(Terms1.InvoiceTermInfo, "Invoice Cycle data must be entered.");

			Terms1.ARTermsCycles.AddNew();
			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertNoErrors(Terms1.InvoiceTermInfo);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertHasError(Terms1.InvoiceTermInfo, "Payment Cycle data must be entered.");
			AssertEquals(0, Terms1.ARTermsCycles.Count);

			Terms1.ARPaymentCycles.AddNew();
			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertNoErrors(Terms1.InvoiceTermInfo);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertNoErrors(Terms1.InvoiceTermInfo);
			AssertEquals(0, Terms1.ARPaymentCycles.Count);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertHasError(Terms1.InvoiceTermInfo, "Invoice Cycle data must be entered.");
			AssertEquals(0, Terms1.ARTermsCycles.Count);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertHasError(Terms1.InvoiceTermInfo, "Payment Cycle data must be entered.");
			AssertEquals(0, Terms1.ARPaymentCycles.Count);

			Terms1.InvoiceTerm = "XYQ";
			AssertHasError(Terms1.InvoiceTermInfo, "Enter a valid selection.");

			Terms2.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Terms2.InvoiceTerm = Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors(Terms2.InvoiceTermInfo);

			Terms2.JobType = JobInvoicingConsumerTypes.BrokerageCode;
			Terms2.InvoiceTerm = Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors(Terms2.InvoiceTermInfo);

			Terms2.JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			Terms2.InvoiceTerm = Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertHasError(Terms2.InvoiceTermInfo, "DLP invoice term is only available for Shipment, Brokerage and Port Transport Job Type.");

			AssertEquals((ZByte)0, Terms1.InvoiceDays);
			AssertEquals((ZByte)3, Terms2.InvoiceDays);
			AssertEquals((ZByte)4, Terms3.InvoiceDays);
		}

		public void TestInvoiceClassValidation()
		{
			Terms1.InvoiceClass = "XYQ";
			AssertHasError(Terms1.InvoiceClassInfo, "Enter a valid selection.");
		}

		public void TestValidateJobType()
		{
			SetupTermsInfo(Terms1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

			Terms2.JobType = "RRR";
			AssertHasError("invalid code, has errors", Terms2.JobTypeInfo, "Enter a valid selection.");

			Terms2.JobType = "SHP";
			AssertNoErrors("valid code, no errors", Terms2.JobTypeInfo);
		}

		public void TestValidateDirection()
		{
			SetupTermsInfo(Terms1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

			Terms2.Direction = "ZZZ";
			AssertHasError("invalid code, has errors", Terms2.DirectionInfo, "Enter a valid selection.");

			Terms2.Direction = "EXP";
			AssertNoErrors("valid code, no errors", Terms2.DirectionInfo);
		}

		public void TestValidateTransportMode()
		{
			SetupTermsInfo(Terms1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

			Terms2.TransportMode = "ZZZ";
			AssertHasError("invalid code, has errors", Terms2.TransportModeInfo, "Enter a valid selection.");

			Terms2.TransportMode = "AIR";
			AssertNoErrors("valid code, no errors", Terms2.TransportModeInfo);
		}

		public void TestValidateBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			SetupTermsInfo(Terms1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

			Terms2.BranchPK = ZGuid.NewZGuid();
			AssertHasError("invalid code, has errors", Terms2.BranchPKInfo, "Enter a valid selection.");

			Terms2.BranchPK = branch.PK;
			AssertNoErrors("valid code, no errors", Terms2.BranchPKInfo);
		}

		public void TestValidateDepartment()
		{
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			SetupTermsInfo(Terms1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");

			Terms2.DeptPK = ZGuid.NewZGuid();
			AssertHasError("invalid code, has errors", Terms2.DeptPKInfo, "Enter a valid selection.");

			Terms2.DeptPK = dept.PK;
			AssertNoErrors("valid code, no errors", Terms2.DeptPKInfo);
		}

		public void TestDuplicateRow()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			var dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			SetupTermsInfo(Terms1, "SHP", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			var errorMsg = string.Format(@"Term settings for following already exists -
Job Type: SHP, Direction: EXP, Transport Mode: SEA, Branch: {0}, Dept.: {1}, Invoice type: DSB", branch1.GB_Code, dept1.GE_Code);

			SetupTermsInfo(Terms2, "BRK", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			AssertNoRowError(Terms2, errorMsg);

			Terms2.JobType = "SHP";
			AssertHasRowError(Terms2, errorMsg);
			Terms2.JobType = "BRK";
			AssertNoRowError(Terms2, errorMsg);
			Terms2.JobType = "SHP";
			Terms2.Direction = "EXP";
			Terms2.TransportMode = "SEA";

			Terms2.BranchPK = branch2.PK;
			AssertNoRowError(Terms2, errorMsg);
			Terms2.BranchPK = branch1.PK;
			AssertHasRowError(Terms2, errorMsg);

			Terms2.DeptPK = dept2.PK;
			AssertNoRowError(Terms2, errorMsg);
			Terms2.DeptPK = dept1.PK;
			AssertHasRowError(Terms2, errorMsg);

			Terms2.Direction = "IMP";
			AssertNoRowError(Terms2, errorMsg);
			Terms2.Direction = "EXP";
			AssertHasRowError(Terms2, errorMsg);

			Terms2.TransportMode = "AIR";
			AssertNoRowError(Terms2, errorMsg);
			Terms2.TransportMode = "SEA";
			AssertHasRowError(Terms2, errorMsg);

			Terms2.InvoiceClass = "ALL";
			AssertNoRowError(Terms2, errorMsg);
			Terms2.InvoiceClass = "DSB";
			AssertHasRowError(Terms2, errorMsg);
		}

		public void TestDefaultRow()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			var dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			SetupTermsInfo(Terms2, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL");
			SetupTermsInfo(Terms1, "SHP", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			SetupTermsInfo(Terms3, "CFS", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			var errorMsg = "At least one term settings row with Job Type: ALL and Invoice Type: ALL must exist.";

			Terms2.JobType = "SHP";
			Terms2.RunPreSaveValidation();
			AssertHasRowError(Terms2, errorMsg);

			Terms2.JobType = "ALL";
			Terms2.RunPreSaveValidation();
			AssertNoRowError(Terms2, errorMsg);

			Terms2.BranchPK = branch1.PK;
			Terms2.RunPreSaveValidation();
			AssertHasRowError(Terms2, errorMsg);

			Terms2.BranchPK = ZGuid.Empty;
			Terms2.RunPreSaveValidation();
			AssertNoRowError(Terms2, errorMsg);

			Terms2.DeptPK = dept1.PK;
			Terms2.RunPreSaveValidation();
			AssertHasRowError(Terms2, errorMsg);

			Terms2.DeptPK = ZGuid.Empty;
			Terms2.RunPreSaveValidation();
			AssertNoRowError(Terms2, errorMsg);

			Terms2.InvoiceClass = "DSB";
			Terms2.RunPreSaveValidation();
			AssertHasRowError(Terms2, errorMsg);

			Terms2.InvoiceClass = "ALL";
			Terms2.RunPreSaveValidation();
			AssertNoRowError(Terms2, errorMsg);
		}

		void SetupTermsInfo(ARTerms term, ZString jobType, ZGuid branchPK, ZGuid deptPK, ZString direction, ZString transportMode, ZString invoiceType)
		{
			using (term.GetValidationSuspender())
			{
				term.JobType = jobType;
				term.BranchPK = branchPK;
				term.DeptPK = deptPK;
				term.Direction = direction;
				term.TransportMode = transportMode;
				term.InvoiceClass = invoiceType;
			}
		}

		public void TestInvoiceDaysReadOnly()
		{
			Terms1.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(true, Terms1.InvoiceDaysInfo.ReadOnly);

			Terms1.InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.FromPeriodEnd;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
			Terms1.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertEquals(false, Terms1.InvoiceDaysInfo.ReadOnly);
		}

		public void TestDisbursementValueIsSynchronized()
		{
			AssertEquals(new ZDecimal(0m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(0m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(0m), Terms3.TreatDisbursementsAsStandardValue);

			Terms1.TreatDisbursementsAsStandardValue = new ZDecimal(1.1m);
			AssertEquals(new ZDecimal(1.1m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(1.1m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(1.1m), Terms3.TreatDisbursementsAsStandardValue);

			Terms2.TreatDisbursementsAsStandardValue = new ZDecimal(2.2m);
			AssertEquals(new ZDecimal(2.2m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(2.2m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(2.2m), Terms3.TreatDisbursementsAsStandardValue);

			Terms3.TreatDisbursementsAsStandardValue = new ZDecimal(3.3m);
			AssertEquals(new ZDecimal(3.3m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(3.3m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(3.3m), Terms3.TreatDisbursementsAsStandardValue);

			ARTerms terms4 = (ARTerms)Terms1.ParentCollection.AddNew();
			AssertEquals(new ZDecimal(3.3m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(3.3m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(3.3m), Terms3.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(3.3m), terms4.TreatDisbursementsAsStandardValue);

			terms4.TreatDisbursementsAsStandardValue = new ZDecimal(0m);
			AssertEquals(new ZDecimal(0m), Terms1.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(0m), Terms2.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(0m), Terms3.TreatDisbursementsAsStandardValue);
			AssertEquals(new ZDecimal(0m), terms4.TreatDisbursementsAsStandardValue);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ARTerms(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		ARTerms Terms1;
		ARTerms Terms2;
		ARTerms Terms3;

		protected override void SetUp()
		{
			base.SetUp();
			ARTermsCollection collection = new ARTermsCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			Terms1 = collection.AddNew();
			Terms2 = collection.AddNew();
			Terms3 = collection.AddNew();
		}

		#endregion
	}
}
