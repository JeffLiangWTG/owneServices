using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyContainerCommodityCodeCollection : ContainerCommodityCodeCollection
	{
		public AgencyContainerCommodityCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string GetErrorMessageWhenAdditionalFilterNotMet()
		{
			return Res.GetString("5b5cb5a2-4856-4be9-bf35-e4fe5ecb2cad", "You can only select Commodity Codes marked as for shipping.");
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(RefCommodityCodeSchema.RH_IsShipping, true);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(RefCommodityCode newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.RH_IsShipping = true;
			newElement.RH_IsForwarding = false;
		}
	}
}
