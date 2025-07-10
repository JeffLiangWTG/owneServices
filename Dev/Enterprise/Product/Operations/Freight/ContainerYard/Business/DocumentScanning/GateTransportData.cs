using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GateTransportData),
	Enterprise.Core.Constants.DocManagerCodes.GateTransport)]

namespace Enterprise.Freight.ContainerYard.Business
{
	using System;
	using ZArchitecture.Core;

	class GateTransportData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GateTransport);

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("6b5eedc1-fae5-4c24-adb1-6cacdf40f526", "Container Yard Gate Transport");
			}
		}

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => null;
	}
}
