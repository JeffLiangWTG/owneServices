using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseDispatchConsignmentData),
	Enterprise.Core.Constants.DocManagerCodes.TransitDispatchConsignment)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseDispatchConsignmentData : TransitAssemblyData<WhsItemDispatchConsignment>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseDispatchConsignmentData|HumanReadableName", "Transit Dispatch Consignment");

		protected override Type CollectionType => typeof(WhsItemDispatchConsignmentCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemDispatchConsignmentCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsTransitDispatchConsignment;
	}
}
