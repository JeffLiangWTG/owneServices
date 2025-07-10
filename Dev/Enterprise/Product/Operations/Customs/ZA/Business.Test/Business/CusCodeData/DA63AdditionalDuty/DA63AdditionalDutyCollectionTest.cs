using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DA63AdditionalDutyCollection))]
	sealed class DA63AdditionalDutyCollectionTest : CusCodeDataCollectionTest<DA63AdditionalDuty>
	{
		public void TestS1P2BDuty()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			AssertNull(da63Duties.S1P2BDuty);
			da63Duties.AddOrUpdate("12B", 11m);
			AssertNotNull(da63Duties.S1P2BDuty);
		}

		public void TestPenalties()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			AssertEquals(0, da63Duties.Penalties.Count());
			da63Duties.AddOrUpdate("PEN", 11m);
			AssertEquals(1, da63Duties.Penalties.Count());
		}

		public void TestProvisionalPayments()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			AssertEquals(0, da63Duties.ProvisionalPayments.Count());
			da63Duties.AddOrUpdate("PPA", 11m);
			AssertEquals(1, da63Duties.ProvisionalPayments.Count());
		}

		public void TestCustomsDutiesExcluding12B()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			AssertEquals(0, da63Duties.CustomsDutiesExcluding12B.Count());
			da63Duties.AddOrUpdate("12A", 11m);
			AssertEquals(1, da63Duties.CustomsDutiesExcluding12B.Count());
		}

		public void TestAddOrUpdate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			da63Duties.AddOrUpdate("12B", 11m);
			AssertEquals(1, da63Duties.Count);
			AssertEquals(11m, da63Duties[0].CY_Value);
			AssertEquals("12B", da63Duties[0].CY_Code);
			da63Duties.AddOrUpdate("12B", 21m);
			AssertEquals(1, da63Duties.Count);
			AssertEquals(21m, da63Duties[0].CY_Value);
			AssertEquals("12B", da63Duties[0].CY_Code);
		}

		public void TestAutoPopulateIfNeeded()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var testOrgDeclaration = Factory.New<JobDeclaration>();
			testOrgDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testOrgDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgEntry = testOrgDeclaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testEntryLine.CL_AdValoremTariff = "TESTTRF1";
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 20, false);
			testEntryLine.Fees.AddOrUpdate("12A", 21, false);
			testEntryLine.Fees.AddOrUpdate("12B", 22, false);
			testEntryLine.ProvisionalPayments.AddNew("PEN", 23);
			testEntryLine.ProvisionalPayments.AddNew("PPA", 24);
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var testInstruction = testOrgDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = testLine.EntryInstruction.CEI_Style + "YY";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 2;
			var da63Duties = new DA63AdditionalDutyCollection(testLine);
			da63Duties.AutoPopulateIfNeeded();
			AssertEquals(2, da63Duties.Count);
			AssertEquals(21m, da63Duties.GetFirstElementHaving("12A").OriginValue);
			AssertEquals(22m, da63Duties.GetFirstElementHaving("12B").OriginValue);
		}

		protected override CusCodeDataCollection<DA63AdditionalDuty> GetCusCodeDataCollection()
		{
			return new DA63AdditionalDutyCollection(Factory.New<JobComInvoiceLine>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<DA63AdditionalDuty>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			result.CY_ParentID = invoiceLine.PK;
			result.CY_ParentTableCode = invoiceLine.TablePrefix;
			return result;
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			base.SetUp();
		}
		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
