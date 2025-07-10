using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestParentIsLine()
	{
		Assert("supportingDocument parent is InvoiceLine", lineSupportingDocument.Parent is JobComInvoiceLine);
	}

	public void TestReferenceNumber_Attributes() => CombineAssertions(() =>
	AssertEntity<SupportingDocument>()
		.HasProperty(x => x.CSI_ReferenceNumber)
		.WithMaxLength(35)
		.WithCaption("Reference")
		.WithShortCaption("Ref.")
		.WithFullDescription("Reference Number"));

	public void TestCSICode_Attributes() => CombineAssertions(() =>
	AssertEntity<SupportingDocument>()
		.HasProperty(x => x.CSI_Code)
		.WithMaxLength(3)
		.WithCaption("Type"));

	public void TestDocumentDescription_Attributes() => CombineAssertions(() =>
		AssertEntity<SupportingDocument>()
			.HasProperty(d => d.DocumentDescription)
			.WithCaption("Description"));

	public void TestDocumentDescription_WhenCSI_CodeIsPresentInTheSystem()
	{
		var today = ZDateTime.Now;
		var tomorrow = today.AddDays(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection,
			"225",
			"Testing note 1",
			today,
			tomorrow);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection,
			"255",
			"Testing note 2",
			today,
			tomorrow);
		Factory.Save();

		CombineAssertions(() =>
		{
			lineSupportingDocument.CSI_Code = "255";
			AssertEquals("Testing note 2", lineSupportingDocument.DocumentDescription);
			lineSupportingDocument.CSI_Code = "225";
			AssertEquals("Testing note 1", lineSupportingDocument.DocumentDescription);
		});
	}

	public void TestGetSupportingDocumentDescription_CodeIsEmptyReturnsEmptyString()
	{
		lineSupportingDocument.CSI_Code = ZString.Empty;
		var result = lineSupportingDocument.DocumentDescription;
		AssertEquals(ZString.Empty, result);
	}

	public void TestGetSupportingDocumentDescription_CodeExistsAndNoMatchFoundReturnsEmptyString()
	{
		lineSupportingDocument.CSI_Code = new ZString("abc");
		var result = lineSupportingDocument.DocumentDescription;
		AssertEquals(ZString.Empty, result);
	}

	public void TestSupportingDocumentHumanReadableName()
	{
		AssertEquals("Supporting Document", Factory.New<SupportingDocument>().HumanReadableName);
	}

	public void TestLookupObjectIsNotCached()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportSupportingDocumentLookups>("Export lookups should not be cached", lineSupportingDocument.Lookups);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportSupportingDocumentLookups>("Import lookups should not be cached", lineSupportingDocument.Lookups);
	}

	#region Implementation

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
		lineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
	}

	SupportingDocument lineSupportingDocument;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	JobDeclaration declaration;

	#endregion
}
