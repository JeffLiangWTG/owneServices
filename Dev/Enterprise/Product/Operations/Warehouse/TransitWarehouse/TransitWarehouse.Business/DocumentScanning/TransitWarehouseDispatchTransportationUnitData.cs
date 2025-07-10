using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseDispatchTransportationUnitData),
	Enterprise.Core.Constants.DocManagerCodes.TransitDispatchTransportationUnit)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseDispatchTransportationUnitData : TransitAssemblyData<WhsItemDispatchTransportationUnit>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseDispatchTransportationUnitData|HumanReadableName", "Transit Dispatch Transportation Unit");

		protected override Type CollectionType => typeof(WhsItemDispatchTransportationUnitCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemDispatchTransportationUnitCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemDispatchTransportationUnit;
	}
}
