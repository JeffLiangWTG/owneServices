using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(WarehouseLoadAssemblyData), Enterprise.Core.Constants.DocManagerCodes.WarehouseLoad)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseLoadAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(WhsLoad);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(WhsLoadCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsLoadCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsLoad;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("92cf92d9-2225-49dc-b363-dfbdb7e139ea", "Warehouse Load Planning");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
