using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Opportunity)]
	public class OrgOpportunityCollection : BusinessObjectCollection<OrgOpportunity>
	{
		public OrgOpportunityCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgOpportunityCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
