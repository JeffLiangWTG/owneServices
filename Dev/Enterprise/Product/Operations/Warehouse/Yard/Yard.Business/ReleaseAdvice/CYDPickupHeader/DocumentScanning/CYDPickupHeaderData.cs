using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDPickupHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.CYDPickupHeader)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderData : CYDAssemblyData<CYDPickupHeader>
	{
		protected override Type CollectionType => typeof(CYDPickupHeaderCollection);

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("CYDPickupHeaderData|HumanReadableName", "Yard Pickup Header"); } }
	}
}
