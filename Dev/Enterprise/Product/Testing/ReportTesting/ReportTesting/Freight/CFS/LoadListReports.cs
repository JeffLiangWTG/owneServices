namespace Enterprise.ReportTesting.Freight.CFS
{
	using System.Collections.Generic;

	[TemplateName("Load List Client Summary")]
	public class TestLoadListClientSummary : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"LoadListClientSummary"
				};
			}
		}
	}

	[TemplateName("Load List Profile")]
	public class TestLoadListProfile : TemplateTestCase
	{
	}
}
