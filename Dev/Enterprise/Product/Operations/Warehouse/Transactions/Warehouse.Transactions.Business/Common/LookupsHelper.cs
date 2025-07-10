using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class LookupsHelper
	{
		#region PackTypesWithStandardUnits

		public static CodeDescriptionPairList PackTypesWithStandardUnits(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WhsInventoryLookups.PackTypesWithStandardUnits",
					() => new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits());
		}

		#endregion
	}
}
