using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	abstract class SupportingDocumentLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			CreateTestList();
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected SupportingDocument supportingDocument;

		protected abstract string MessageType { get; }

		protected const string ValidImportCode = "CE";
		protected const string ValidExportCode = "B1";

		protected void CreateTestList()
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
}
