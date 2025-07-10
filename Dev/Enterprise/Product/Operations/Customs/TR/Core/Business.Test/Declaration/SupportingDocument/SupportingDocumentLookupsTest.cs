using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			AssertType<SupportingDocumentAvailabilityList>("Should be StatusList", supportingDocument.Lookups.StatusList);
		}

		public void TestCodeList()
		{
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);

			var support = invoiceHeader.SupportingDocuments.AddNew();
			collection = (ZZRefCusCodeListCombinedCollection)support.Lookups.CodeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PrepareCusCodeListData();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		}

		void PrepareCusCodeListData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Turkey, new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		#endregion
	}
}
