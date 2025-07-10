using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehouseInventoryData),
	Enterprise.Core.Constants.DocManagerCodes.WarehouseInventory)]

namespace Enterprise.Warehouse.Transactions.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class WarehouseInventoryData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsInventoryView); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsInventoryViewCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsInventoryViewCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsInventory; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ddb64ed2-ee83-4902-afdb-a80c5d61c70e", "Warehouse Inventory"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new WarehouseInventoryEDocsViaUniversalXmlSupport();
	}
}
