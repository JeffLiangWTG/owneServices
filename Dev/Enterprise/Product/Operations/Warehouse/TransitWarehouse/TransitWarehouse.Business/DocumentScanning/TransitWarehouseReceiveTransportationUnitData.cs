using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseReceiveTransportationUnitData),
	Enterprise.Core.Constants.DocManagerCodes.TransitReceiveTransportationUnit)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseReceiveTransportationUnitData : TransitAssemblyData<WhsItemReceiveTransportationUnit>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseReceiveTransportationUnitData|HumanReadableName", "Transit Receive Transportation Unit");

		protected override Type CollectionType => typeof(WhsItemReceiveTransportationUnitCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemReceiveTransportationUnitCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemReceiveTransportationUnit;
	}
}
