namespace Enterprise.ReportTesting.Warehouse
{
	using System.Collections.Generic;
	using Enterprise.ReportTesting;

	[TemplateName("Whs Job History")]
	public class TestWhsJobHistory : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return true; }
		}

		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Orders Detail" }; }
		}
	}

	[TemplateName("FTZ Report")]
	public class TestFTZAnnualReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Transactions")]
	public class TestWhsTransactions : WhsTemplateTestCase
	{
	}

	[TemplateName("Whs Unfinalised Jobs")]
	public class TestWhsUnfinalisedJobsReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Uninvoiced Jobs")]
	public class TestWhsUninvoicedJobsReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs KPI Release")]
	public class TestWhsKPIReleaseReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Receive Unders Overs")]
	public class TestWhsReceiveUndersOversReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Job Pick Planning")]
	public class TestWhsJobPickPlanningReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Job Planning")]
	public class TestWhsJobPlanningReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Job Transport Manifest")]
	public class TestWhsJobTransportManifestReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("Whs Staff Productivity Report")]
	public class TestWhsStaffProductivityReport : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	[TemplateName("TWH Package On Hand with UNDG Code")]
	public class TestWhsTransitPackageOnHandReport : WhsTemplateTestCase
	{
	}
}
