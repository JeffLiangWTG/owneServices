using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DA63AdditionalDuty))]
	sealed class DA63AdditionalDutyTest : Customs.Business.Testing.CusCodeDataTest<DA63AdditionalDuty>
	{
		public void TestSetDefaultValues()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, duty.CY_ParentTableCode);
			AssertEquals(CusCodeDataTypeList.Codes.DA63AdditionalDuty, duty.CY_Type);
			Assert(duty.CY_Value.IsEmpty);
			AssertEquals("", duty.CY_Code);
			AssertEquals("0.00", duty.CY_Data);
		}

		public void TestCY_Value_ReadOnly()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			Assert(duty.CY_Value_ReadOnly);
			Assert(duty.CY_ValueInfo.ReadOnly);
			duty.CY_Code = "12B";
			Assert(!duty.CY_Value_ReadOnly);
			Assert(!duty.CY_ValueInfo.ReadOnly);
		}

		public void TestClearCY_ValueIfNeeded()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			duty.CY_Code = "12B";
			duty.CY_Value = 11m;
			duty.CY_Code = ZString.Empty;
			Assert(duty.CY_Value.IsEmpty);
		}

		public void TestOriginValue()
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
			testEntryLine.Fees.AddOrUpdate("1P1", 21, false);
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
			var da63Duties = testLine.DA63AdditionalDuties;
			AssertEquals(1, da63Duties.Count);
			AssertEquals(22m, da63Duties.GetFirstElementHaving("12B").OriginValue);
		}

		public void TestLookups()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			AssertType<DA63AdditionalDutyLookups>(duty.Lookups);
		}

		public void TestValidation()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			AssertType<DA63AdditionalDutyValidation>(duty.Validation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<DA63AdditionalDuty>();
		}

		protected override IEnumerable<DA63AdditionalDuty> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<DA63AdditionalDuty>();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.DA63AdditionalDuties.Add(result);
			yield return result;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<DA63AdditionalDuty>();
		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
