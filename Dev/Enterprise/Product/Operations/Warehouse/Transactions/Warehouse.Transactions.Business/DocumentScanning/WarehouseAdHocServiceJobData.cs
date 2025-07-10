using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehouseAdHocServiceJobAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.WarehouseAdHocServiceJob)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseAdHocServiceJobAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(WhsAdHocServiceJob); }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		protected override Type CollectionType
		{
			get { return typeof(WhsAdHocServiceJobCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsAdHocServiceJobCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsAdHocServiceJob; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("CB9D3554-C22B-4F7D-8B86-BBDDB29A7B0E", "Warehouse Ad Hoc Service Job"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
