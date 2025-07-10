using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.FR;

[TemplateName("Manufacturer Warehouse Stock Report")]
sealed class FRManufacturerWarehouseStockReport : TemplateTestCase
{
	protected override void SetUp()
	{
		base.SetUp();
		GlbCompany.CurrentCompany.SetCountry("FR");
	}

	protected override bool ReportRequiresColumnHeadings => false;
}
