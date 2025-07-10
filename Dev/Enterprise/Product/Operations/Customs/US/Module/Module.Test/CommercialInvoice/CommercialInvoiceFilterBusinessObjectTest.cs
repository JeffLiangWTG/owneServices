using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceFilterBusinessObject))]
	sealed class CommercialInvoiceFilterBusinessObjectTest : Customs.Module.Testing.CommercialInvoiceFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			AssertEquals("Lookup Type", typeof(CommercialInvoiceFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestDrawbackDeclaration()
		{
			var filter = new CommercialInvoiceFilterBusinessObject();
			DisableAttachedToDeclarationFilter(filter);
			var standardDeclaration = Factory.New<JobDeclaration>();
			var standardInvoice = standardDeclaration.Invoices.AddNew();
			standardInvoice.JZ_InvoiceNumber = "STD-INV";
			var drawbackDeclaration = Factory.New<JobDeclaration>();
			drawbackDeclaration.JE_MessageType = Business.JobMessageTypeList.Codes.Drawback;
			var drawbackInvoice = drawbackDeclaration.Invoices.AddNew();
			drawbackInvoice.JZ_InvoiceNumber = "DRW-INV";
			Factory.Save();
			AssertEquals("(pre-condition)", false, standardDeclaration.IsDrawback);
			AssertEquals("(pre-condition)", true, drawbackDeclaration.IsDrawback);
			AssertQueryResults(filter.Filter, (standardInvoice, true), (drawbackInvoice, false));
		}

		public void TestNoExceptionThrownWhenPerformSearch()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoice.US_TariffType = ZString.Empty;
			invoice.JZ_InvoiceNumber = "INV01";
			invoice.JZ_SystemCreateTimeUtc = ZDateTime.UtcToday;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			Factory.Save();

			using (var module = new CommercialInvoiceModule())
			{
				using (var form = module.ShowPopup())
				{
					var filterBizObj = (CommercialInvoiceFilterBusinessObject)module.FilterBusinessObject;
					filterBizObj.QueryObjectType = typeof(BaseJobComInvoiceHeader);
					var filter = (ModuleDateFilter)filterBizObj["Created Time"];
					filter.IsActive = true;
					filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					filter.Property1 = ZDateTime.UtcToday.AddDays(-1);
					filter.Property2 = ZDateTime.UtcToday.AddDays(1);

					AssertNoExceptionThrown(() => ((IFilterModuleInternalsForTesting)module).PerformSearch());
				}
			}
		}

		protected override string ExpectCompanyFilterLiteralTextADO => "JZ_PK IN (SELECT JZ_PK FROM dbo.JobComInvoiceHeader WHERE JZ_GB = CONVERT('{0}', 'System.Guid') or (JZ_JE IN (SELECT JE_PK FROM dbo.JobDeclaration WHERE JE_GB = CONVERT('{0}', 'System.Guid') and (JE_ApplicationCode <> 'EMC' and JE_MessageType <> 'DRW') and JE_IsCancelled = 0)))";
	}
}
