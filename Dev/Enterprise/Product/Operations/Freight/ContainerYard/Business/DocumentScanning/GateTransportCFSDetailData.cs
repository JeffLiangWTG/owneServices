using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GateTransportCFSDetailData),
	Enterprise.Core.Constants.DocManagerCodes.GateTransportCFSDetail)]

namespace Enterprise.Freight.ContainerYard.Business
{
	using System;
	using ZArchitecture.Core;

	class GateTransportCFSDetailData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GateTransportCFSDetail);

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("18e651c6-ca5f-4db1-9350-9b139ae22b39", "CFS Gate Transport Detail");
			}
		}

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => null;
	}
}
