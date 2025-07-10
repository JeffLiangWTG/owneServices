using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehouseTransferDocketData),
	Enterprise.Core.Constants.DocManagerCodes.WarehouseTransferDocket)]

namespace Enterprise.Warehouse.Transactions.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class WarehouseTransferDocketData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsTransfer); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsTransferCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsTransferCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsTransfer; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("b9f59488-a565-4538-b32b-4b0467a00567", "Warehouse Transfer Docket"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
