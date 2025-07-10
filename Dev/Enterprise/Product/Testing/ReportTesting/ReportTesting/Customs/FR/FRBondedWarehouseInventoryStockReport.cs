using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.FR
{
	[TemplateName("Bonded Warehouse Inventory Stock Report")]
	public class TestBondedWarehouseInventoryStockReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
		}

		protected override bool ReportRequiresColumnHeadings => false;
	}
}
