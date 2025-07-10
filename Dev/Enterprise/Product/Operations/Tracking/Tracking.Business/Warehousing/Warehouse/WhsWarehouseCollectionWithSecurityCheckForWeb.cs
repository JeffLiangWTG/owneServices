using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Tracking.Business
{
	[CodeAlive("This is used by reflection code in WarehouseCollectionProviderForWeb.cs.")]
	class WhsWarehouseCollectionWithSecurityCheckForWeb : WhsWarehouseCollectionWithSecurityCheck, IWhsWarehouseCollectionWithSecurityCheckForWeb
	{
		public WhsWarehouseCollectionWithSecurityCheckForWeb(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();

			filter.AddToFilter(OrgRestrictionFilterFactory.GetOrderedProhibitedWarehouseQuery());

			return filter;
		}
	}
}
