using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesDashboardActivityCollection : BusinessObjectCollection<SalesDashboardActivity>, ISalesDashboardActivityCollection
	{
		public SalesDashboardActivityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public SalesDashboardActivityCollection(BusinessObjectFactory factory, IModuleGridCollectionRefreshable salesDashboardActivityRefreshable)
			: base(factory)
		{
			new ModuleGridCollectionSynchronisationManager(this, salesDashboardActivityRefreshable);
		}

		public SalesDashboardActivityCollection(BusinessObjectFactory factory, IOrgHeader org)
			: base(factory, new ZQuery(ViewSalesDashboardActivitySchema.VSA_OH, org.PK))
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Can not add new to collection");
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("Can not add new to collection");
		}
	}
}
