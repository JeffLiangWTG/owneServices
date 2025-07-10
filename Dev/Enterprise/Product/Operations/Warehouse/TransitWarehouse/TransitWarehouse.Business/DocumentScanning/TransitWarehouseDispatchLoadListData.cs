using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseDispatchLoadListData),
	Enterprise.Core.Constants.DocManagerCodes.TransitDispatchLoadList)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseDispatchLoadListData : TransitAssemblyData<WhsItemDispatchLoadList>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseDispatchLoadListData|HumanReadableName", "Transit Dispatch Load List");

		protected override Type CollectionType => typeof(WhsItemDispatchLoadListCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemDispatchLoadListCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemDispatchLoadList;
	}
}
