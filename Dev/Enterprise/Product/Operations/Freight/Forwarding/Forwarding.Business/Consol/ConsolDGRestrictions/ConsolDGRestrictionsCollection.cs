using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDGRestrictionsCollection : ActiveBusinessObjectCollection<ConsolDGRestrictions>
	{
		public ConsolDGRestrictionsCollection(ForwardingConsol parent)
			: base(parent.Factory, new DependentRelationship(parent, typeof(ConsolDGRestrictions), new ZQuery(JobConsolDGRestrictionsSchema.JKD_JK, parent.PK), JobConsolDGRestrictionsSchema.JKD_JK))
		{
		}
	}
}
