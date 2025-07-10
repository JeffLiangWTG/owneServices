using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityStageProgressCollection : ActiveBusinessObjectCollection<OrgOpportunityStageProgress>
	{
		public OrgOpportunityStageProgressCollection(OrgOpportunity master)
			: base(master.Factory, master, new ZQuery(), OrgOpportunityStageProgressSchema.OSP_P8_Opportunity)
		{
		}
	}
}
