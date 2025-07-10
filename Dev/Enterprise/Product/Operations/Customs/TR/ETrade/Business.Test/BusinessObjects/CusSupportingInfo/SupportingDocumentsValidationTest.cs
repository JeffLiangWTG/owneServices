using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class SupportingDocumentsValidationTest : BusinessObjectValidationTestCase
	{
		SupportingDocuments supportingDocuments;
		public void TestCheckCSI_Code()
		{
			supportingDocuments.CSI_Code = "ABCD";
			supportingDocuments.Validation.ValidateAll();
			AssertHasMessageErrorContaining(supportingDocuments.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			supportingDocuments.CSI_Code = code;
			supportingDocuments.Validation.ValidateAll();
			AssertNoMessageErrors(supportingDocuments.CSI_CodeInfo);
		}
		public void TestCheckCSI_ReferenceNumber()
		{
			supportingDocuments.CSI_Code = ZString.Empty;
			AssertNoMessageErrors(supportingDocuments.CSI_ReferenceNumberInfo);

			supportingDocuments.CSI_Code = "0102";
			supportingDocuments.CSI_ReferenceNumber = "0000";
			AssertNoMessageErrors(supportingDocuments.CSI_ReferenceNumberInfo);

			supportingDocuments.CSI_Code = "0102";
			supportingDocuments.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(supportingDocuments.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_Status()
		{
			supportingDocuments.CSI_Code = ZString.Empty;
			AssertNoMessageErrors(supportingDocuments.CSI_StatusInfo);

			supportingDocuments.CSI_Code = "0102";
			supportingDocuments.CSI_Status = SupportingDocumentStatusList.Codes.EXS;
			AssertNoMessageErrors(supportingDocuments.CSI_StatusInfo);

			supportingDocuments.CSI_Code = "0102";
			supportingDocuments.CSI_Status = ZString.Empty;
			AssertHasMessageErrorContaining(supportingDocuments.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocuments.CSI_Status = "ABC";
			AssertHasMessageErrorContaining(supportingDocuments.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);

			supportingDocuments.CSI_Status = SupportingDocumentStatusList.Codes.EXS;
			supportingDocuments.Validation.ValidateAll();
			AssertNoMessageErrors(supportingDocuments.CSI_StatusInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRDOC", "TRDOC");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRDOC", code, yesterday, tomorrow);
			supportingDocuments = Factory.NewWithValidTestData<SupportingDocuments>();
			Factory.Save();
		}

		readonly ZString code = "0101";
	}
}
