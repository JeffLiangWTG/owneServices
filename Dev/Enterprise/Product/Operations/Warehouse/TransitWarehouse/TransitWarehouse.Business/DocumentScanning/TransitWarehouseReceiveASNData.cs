using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TransitWarehouseReceiveASNData),
	Enterprise.Core.Constants.DocManagerCodes.TransitReceiveASN)]

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehouseReceiveASNData : TransitAssemblyData<WhsItemReceiveASN>
	{
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("TransitWarehouseReceiveASNData|HumanReadableName", "Transit Receive ASN");

		protected override Type CollectionType => typeof(WhsItemReceiveASNCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new WhsItemReceiveASNCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemReceiveASN;
	}
}
