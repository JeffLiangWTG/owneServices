using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(MNRWorkOrderHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.MNRWorkOrderHeader)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderData : CYDAssemblyData<MNRWorkOrderHeader>
	{
		protected override Type CollectionType => typeof(MNRWorkOrderHeaderCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("MNRWorkOrderHeaderData|HumanReadableName", "Work Order");
	}
}
