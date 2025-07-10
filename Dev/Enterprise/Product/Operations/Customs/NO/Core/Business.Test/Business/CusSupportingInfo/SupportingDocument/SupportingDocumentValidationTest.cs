using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupportingDocumentValidation))]
sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	const string ValidImportCode = "CE";
	const string ValidExportCode = "B1";

	public void TestType()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(supportingDocument.CSI_CodeInfo,
				supportingDocument.CSI_ReferenceNumberInfo, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocument.CSI_CodeInfo, ValidExportCode,
				ValidImportCode);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocument.CSI_CodeInfo, ValidImportCode,
				ValidExportCode);
		});
	}

	public void TestReference()
	{
		ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(supportingDocument.CSI_ReferenceNumberInfo, supportingDocument.CSI_CodeInfo, false);
	}

	public void TestCheckCSI_ReferenceNumberMaxLength() => CombineAssertions(() =>
	{
		const string messageNonTXT = "The length of Reference cannot exceed 17 characters when Type is other than TXT.";

		supportingDocument.CSI_Code = "TXT";
		supportingDocument.CSI_ReferenceNumber = new ZString('A', 20);
		AssertNoMessageErrorContaining("When CSI_Code == TXT and CSI_ReferenceNumber length > 17", supportingDocument.CSI_ReferenceNumberInfo, messageNonTXT);

		supportingDocument.CSI_Code = "AAA";
		AssertHasMessageError("When CSI_Code != TXT and CSI_ReferenceNumber length > 17", supportingDocument.CSI_ReferenceNumberInfo, messageNonTXT);

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 17);
		AssertNoMessageErrorContaining("When CSI_Code != TXT and CSI_ReferenceNumber length <= 17", supportingDocument.CSI_ReferenceNumberInfo, messageNonTXT);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		CreateTestCodes();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	SupportingDocument supportingDocument;

	void CreateTestCodes()
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
		refDataHelper.CreateCusCodeList(
			Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection,
			ValidImportCode,
			ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTimeValue);
		refDataHelper.CreateCusCodeList(
			Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection,
			ValidExportCode,
			ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}
}
