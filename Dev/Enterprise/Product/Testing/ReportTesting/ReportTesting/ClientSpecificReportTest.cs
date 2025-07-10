using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting
{
	public abstract class ClientSpecificReportTestCase : ReportTestCase
	{
		public abstract ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase();

		ClientSpecificTemplateTestCase ClientSpecificTemplateTestCase
		{
			get
			{
				if (clientSpecificTemplateTestCase == null)
				{
					clientSpecificTemplateTestCase = GetClientSpecificTemplateTestCase();
				}
				return clientSpecificTemplateTestCase;
			}
		}
		ClientSpecificTemplateTestCase clientSpecificTemplateTestCase;

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_IsClientSpecific, true); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return ClientSpecificTemplateTestCase;
		}

		protected override DocumentEngine.Business.StmMenuItemBaseCollection CandidateMenuItems
		{
			get
			{
				if (base.CandidateMenuItems.Count < 1)
				{
					ClientSpecificTemplateTestCase.LoadClientSpecificDocuments();
					ResetCandidateMenuItems();
				}
				return base.CandidateMenuItems;
			}
		}
	}
}
