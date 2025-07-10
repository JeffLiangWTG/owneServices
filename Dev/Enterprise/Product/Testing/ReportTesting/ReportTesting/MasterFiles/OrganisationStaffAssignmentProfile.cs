using System.Collections.Generic;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Organisation Staff Assignment Profile")]
	public class TestOrganisationStaffAssignmentProfileTemplate : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	public class TestOrganisationStaffAssignmentProfileReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Staff Assignment Profile"; }
		}

		public override string Hint
		{
			get
			{
				return "The Organization - Staff Assignment Profile report lists staff assigned roles and departments in dealings with the selected Organizations. Report will display the staff assignments for every company assignment where multiples exist and/or company assignments set on a global level.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOrganisationStaffAssignmentProfileTemplate();
		}
	}
}
