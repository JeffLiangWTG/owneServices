using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	public class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCSI_CodeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Turkey, new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = "9001";
			AssertEquals("9001 DES", supportingDocument.CSI_CodeDescription);
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

		#endregion
	}
}
