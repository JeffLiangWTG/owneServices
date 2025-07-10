using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WarehouseDynamicWorkOrderAssemblyData),
	Constants.DocManagerCodes.WarehouseDynamicWorkOrder)]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseDynamicWorkOrderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(WhsDynamicWorkOrder);

		public override string ReferenceType => Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(WhsDynamicWorkOrderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsDynamicWorkOrderCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsDynamicWorkOrder;

		public override MultilingualString HumanReadableName
			=> ResString.GetMultilingualString("c46f1c38-265b-4806-b814-b0fcae50be50", "Warehouse Dynamic Work Order Job");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
