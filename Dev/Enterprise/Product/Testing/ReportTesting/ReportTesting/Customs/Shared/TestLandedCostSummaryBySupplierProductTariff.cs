namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;

	[TemplateName("Landed Cost – Summary by Supplier, Product, Tariff")]
	public class TestLandedCostSummaryBySupplierProductTariffTemplate : TemplateTestCase
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

	public class TestLandedCostSummaryBySupplierProductTariffReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Landed Cost – Summary by Supplier, Product, Tariff"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report summarizes landed cost by product then tariff within supplier.
The report shows for a nominated Importer and Date range summary tariff level details including  Invoice Qty, line prices, landing costs and weighted average landed cost per unit.
The report offers selectable columns to detail up to 30 other charges, costs and prices.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestLandedCostSummaryBySupplierProductTariffTemplate();
		}
	}
}
