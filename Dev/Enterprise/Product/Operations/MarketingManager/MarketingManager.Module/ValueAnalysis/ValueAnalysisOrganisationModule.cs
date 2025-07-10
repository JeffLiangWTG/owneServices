using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public abstract class ValueAnalysisOrganisationModule : ValueAnalysisModule
	{
		protected ValueAnalysisOrganisationModule()
		{
			Context = OrgHeaderSchema.Constants.PK;
		}
	}
}
