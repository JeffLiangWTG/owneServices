using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehouseStocktakeData),
	Enterprise.Core.Constants.DocManagerCodes.WarehouseStocktake)]

namespace Enterprise.Warehouse.Transactions.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class WarehouseStocktakeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsStocktake); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsStocktakeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsStocktakeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsStocktake; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("e6d2e69b-5589-4bbb-983d-01bf0b0df193", "Warehouse Stocktake"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
