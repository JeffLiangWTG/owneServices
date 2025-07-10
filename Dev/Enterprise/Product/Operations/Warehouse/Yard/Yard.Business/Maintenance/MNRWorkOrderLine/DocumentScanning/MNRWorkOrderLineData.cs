using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(MNRWorkOrderLineData),
	Enterprise.Core.Constants.DocManagerCodes.MNRWorkOrderLine)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderLineData : CYDAssemblyData<MNRWorkOrderLine>
	{
		protected override Type CollectionType => typeof(MNRWorkOrderLineCollection);

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("MNRWorkOrderLineData|HumanReadableName", "Yard Work Order Line"); } }
	}
}
