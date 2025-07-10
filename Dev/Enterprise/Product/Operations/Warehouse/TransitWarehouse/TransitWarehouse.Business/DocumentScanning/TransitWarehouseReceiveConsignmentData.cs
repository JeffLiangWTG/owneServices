using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseReceiveConsignmentData),
	Enterprise.Core.Constants.DocManagerCodes.TransitReceiveConsignment)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseReceiveConsignmentData : TransitAssemblyData<WhsItemReceiveConsignment>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseReceiveConsignmentData|HumanReadableName", "Transit Receive Consignment");

		protected override Type CollectionType => typeof(WhsItemReceiveConsignmentCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemReceiveConsignmentCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsTransitReceiveConsignment;
	}
}
