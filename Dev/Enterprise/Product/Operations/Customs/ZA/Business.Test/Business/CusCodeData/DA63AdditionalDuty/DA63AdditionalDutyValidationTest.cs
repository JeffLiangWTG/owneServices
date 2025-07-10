using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DA63AdditionalDutyValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Value()
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
			testEntryLine.Fees.AddOrUpdate("12B", 22, false);
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
			var s12b = da63Duties.S1P2BDuty;
			s12b.CY_Value = 33m;
			AssertHasMessageError(s12b.CY_ValueInfo, JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
			s12b.CY_Value = 11m;
			AssertNoNotifications(s12b.CY_ValueInfo);
			var pen = da63Duties.AddNew("PEN");
			pen.CY_Value = 1m;
			AssertNoNotifications(pen.CY_ValueInfo);
		}

		public new void TestCheckCY_Code()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var da63Duties = testLine.DA63AdditionalDuties;
			da63Duties.AddOrUpdate("12B", 11m);
			var another12B = da63Duties.AddNew();
			another12B.CY_Code = "12B";
			AssertHasError(another12B.CY_CodeInfo, "Duty '12B' already exists.");
			another12B.CY_Code = "XX";
			AssertNoError(another12B.CY_CodeInfo, "Duty '12B' already exists.");
			AssertHasMessageError(another12B.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			another12B.CY_Code = "12A";
			AssertNoNotifications(another12B.CY_CodeInfo);
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
