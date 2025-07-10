using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Bonded Warehouse Exposure Report")]
	public class TestZABondedWarehouseExposureReportTemplate : TemplateTestCase
	{
	}

	public class TestZABondedWarehouseExposureReportReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Bonded Warehouse Exposure Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This Report lists the Current Customs Liability in terms of goods stored in the Warehouse";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZABondedWarehouseExposureReportTemplate();
		}
	}
}
