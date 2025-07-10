using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class RelatedTransportLegsOfConsolFilter : ModuleGuidForeignCollectionFilter
	{
		public RelatedTransportLegsOfConsolFilter(ZString description, BusinessObjectFactory factory)
			: base(description, ModuleIDs.RelatedTransportLegs, JobConsolSchema.PK, JobConsolTransportSchema.JW_ParentGUID, new TransportNonDependentCollection(factory), typeof(ForwardingConsol))
		{
		}
	}
}
