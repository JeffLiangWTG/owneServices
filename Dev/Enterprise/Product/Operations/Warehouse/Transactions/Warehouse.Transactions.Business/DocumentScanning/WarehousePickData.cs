using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehousePickData),
	Enterprise.Core.Constants.DocManagerCodes.WarehousePick)]

namespace Enterprise.Warehouse.Transactions.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class WarehousePickData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsPick); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsPickCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsPickCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsRelease; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("3b0eb7ea-998c-4158-b61f-3267ae101f63", "Warehouse Pick"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
