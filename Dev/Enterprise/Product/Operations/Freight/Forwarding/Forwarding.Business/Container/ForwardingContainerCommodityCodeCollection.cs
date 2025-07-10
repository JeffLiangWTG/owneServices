using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingContainerCommodityCodeCollection : ContainerCommodityCodeCollection
	{
		public ForwardingContainerCommodityCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override string GetErrorMessageWhenAdditionalFilterNotMet()
		{
			return Res.GetString("8157655c-fc92-41f1-b77f-6e44e3782d03", "You can only select Commodity Codes marked as for forwarding.");
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(RefCommodityCodeSchema.RH_IsForwarding, true);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(RefCommodityCode newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.RH_IsForwarding = true;
			newElement.RH_IsShipping = false;
		}
	}
}
