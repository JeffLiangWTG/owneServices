using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	abstract class CertificateValidationAbstractTest<T> : CusCodeDataValidationTest where T : CertificateCusCodeData
	{
		[TestDate(2017, 2, 10)]
		public void TestCheckCY_Code_Certificate()
		{
			var startDate = ZDate.Today;
			var endDate = ZDate.Today;
			var dummyOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var permit1 = helper.CreatePermitHeader(dummyOrgHeader.PK, "TEST1", startDate, endDate, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitType);
			var permit2 = helper.CreatePermitHeader(orgHeader.PK, "TEST2", startDate, endDate, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.IMP);
			var permit3 = helper.CreatePermitHeader(orgHeader.PK, "TEST3", startDate, endDate, Customs.Business.PermitQtyValIndicatorList.Codes.QTY, PermitType);
			var permit4 = helper.CreatePermitHeader(orgHeader.PK, "TEST4", startDate.AddDays(1), endDate.AddDays(1), Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitType);
			var permit5 = helper.CreatePermitHeader(orgHeader.PK, "TEST5", startDate, endDate, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitType);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgHeader.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = GetCertificateCollection(instruction);
			var certificate1 = collection.AddNew();
			certificate1.CY_Code = "A";
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.InvoiceLine.PermitNotFound("A"));
			certificate1.CY_Code = permit1.CPH_Number;
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.InvoiceLine.PermitHolderInvalid(permit1.CPH_Number, orgHeader.OH_Code));
			certificate1.CY_Code = permit2.CPH_Number;
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.InvoiceLine.PermitTypeInvalid(permit2.CPH_Number, ExpectedPermitTypes));
			certificate1.CY_Code = permit3.CPH_Number;
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.EntryInstruction.PermitQtyValIndicatorInvalid(permit3.CPH_Number, Customs.Business.PermitQtyValIndicatorList.Codes.VAL));
			certificate1.CY_Code = permit4.CPH_Number;
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.EntryInstruction.PermitValidityPeriodNotContainingAssessmentdate(permit4.CPH_Number, ZDateTime.Today));
			certificate1.CY_Code = permit5.CPH_Number;
			AssertHasMessageError(certificate1.CY_CodeInfo, ValidationConstants.EntryInstruction.PermitMustHavePositiveValueBalance(permit5.CPH_Number));
			certificate1.CY_Code = "A";
			var certificate2 = collection.AddNew();
			certificate2.CY_Code = "A";
			certificate1.Validation.ValidateCY_Code();
			AssertHasWarning(certificate1.CY_CodeInfo, CertificateValidation.CertificateShouldNotBeSpecifiedMultipleTimes(certificate1.CY_Code));
			certificate2.CY_Code = "B";
			certificate1.Validation.ValidateCY_Code();
			AssertNoWarning(certificate1.CY_CodeInfo, CertificateValidation.CertificateShouldNotBeSpecifiedMultipleTimes(certificate1.CY_Code));
		}

		[TestDate(2017, 2, 10)]
		public void TestCheckCY_Code_ShouldNotLoadGuarantee()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var guarantee1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guarantee1.CPH_OH_PermitHolder = orgHeader.PK;
			guarantee1.CPH_Number = "TEST1";
			guarantee1.CPH_StartDate = ZDate.Today;
			guarantee1.CPH_EndDate = ZDate.Today;
			guarantee1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			guarantee1.CPH_Type = PermitType;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgHeader.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = GetCertificateCollection(instruction);
			var certificate1 = collection.AddNew();
			certificate1.CY_Code = "TEST1";
			AssertHasMessageError("It should not load guarantee, hence the message error.", certificate1.CY_CodeInfo, ValidationConstants.InvoiceLine.PermitNotFound("TEST1"));
		}

		public void TestCheckCY_Order()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var collection = GetCertificateCollection(instruction);
			var certificate = collection.AddNew();
			certificate.CY_Order = 1;
			AssertNoMessageError(certificate.CY_OrderInfo, CertificateValidation.NoDuplicateOrderAllowed);
			var certificate2 = collection.AddNew();
			certificate2.CY_Order = 1;
			AssertHasMessageError(certificate2.CY_OrderInfo, CertificateValidation.NoDuplicateOrderAllowed);
		}

		protected ZString PermitType => ExpectedPermitTypes[0];

		protected abstract ZString[] ExpectedPermitTypes { get; }

		protected abstract CusCodeDataCollection<T> GetCertificateCollection(CusEntryInstruction instruction);
	}
}
