using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceLineLookups))]
	class ImportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceLineLookups GetLookups() => new ImportJobComInvoiceLineLookups(invoiceLine);

		public void TestTaxOrFeeCodeList()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var today = ZDateTime.Today;
				refDataHelper.CreateTaxOrFee("MUAF", 0, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "ZERO");
				refDataHelper.CreateTaxOrFee("KD1", 0.01, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "1");
				refDataHelper.CreateTaxOrFee("KD8", 0.08, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "8");
				refDataHelper.CreateTaxOrFee("KD18", 0.18, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "18");
				Factory.Save();
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var list = invoiceLine.Lookups.TaxOrFeeCodeList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "MUAF - ZERO", "KD1 - 1", "KD8 - 8", "KD18 - 18", }, list.ToArray().Select(x => $"{x.Code} - {x.Description}"));
			}
		}
	}
}
