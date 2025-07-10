using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsItemPackageStateFetchStrategy(WhsItemPackageState packageState) : base(packageState)
		{
			this.PackageState = packageState;
		}

		readonly WhsItemPackageState PackageState;

		protected override void FetchForLoadCore()
		{
			var factory = PackageState.Factory;

			var availablePackageStatesAdditionalReferenceQuery = new ZQuery();
			availablePackageStatesAdditionalReferenceQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, PackageState.PK);
			availablePackageStatesAdditionalReferenceQuery.AddToFilter(CusEntryNumSchema.CE_Category, WarehouseAdditionalReferenceTypes.Codes.Other);
			factory.AddFetchHint(CusEntryNumSchema.Instance, availablePackageStatesAdditionalReferenceQuery);
		}
	}
}
