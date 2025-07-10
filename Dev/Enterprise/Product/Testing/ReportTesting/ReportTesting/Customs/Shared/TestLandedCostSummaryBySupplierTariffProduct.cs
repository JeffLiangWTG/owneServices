namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;

	[TemplateName("Landed Cost – Summary by Supplier, Tariff, Product")]
	public class TestLandedCostSummaryBySupplierTariffProductTemplate : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			OrgSupplierPart supplierPart = Factory.New<OrgSupplierPart>();
			supplierPart.OP_PartNum = supplierPart.PK.ToString().Replace("-", "");
			Factory.Save();
		}

		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestLandedCostSummaryBySupplierTariffProductReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Landed Cost – Summary by Supplier, Tariff, Product"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report summarizes landed cost by tariff then product within supplier.
The report shows for a nominated Importer and Date range summary product level details including  Invoice Qty, line prices, landing costs and weighted average landed cost per unit.
The report offers selectable columns to detail up to 30 other charges, costs and prices.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestLandedCostSummaryBySupplierTariffProductTemplate();
		}
	}
}
