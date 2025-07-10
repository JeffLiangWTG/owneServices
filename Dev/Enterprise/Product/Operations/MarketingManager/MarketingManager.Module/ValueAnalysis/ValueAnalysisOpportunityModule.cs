using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public abstract class ValueAnalysisOpportunityModule : ValueAnalysisModule
	{
		protected ValueAnalysisOpportunityModule()
		{
			Context = OrgOpportunitySchema.Constants.PK;
		}
	}
}
