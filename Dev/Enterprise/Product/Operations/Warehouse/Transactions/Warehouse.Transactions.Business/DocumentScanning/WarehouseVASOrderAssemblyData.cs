using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(WarehouseVASOrderAssemblyData), Enterprise.Core.Constants.DocManagerCodes.WarehouseVASOrder)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseVASOrderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(WhsVASOrder);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(WhsVASOrderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsVASOrderCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsVASOrder;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("15D2111A-5F92-49E6-8811-1E1EA142323D", "Warehouse VAS Order");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
