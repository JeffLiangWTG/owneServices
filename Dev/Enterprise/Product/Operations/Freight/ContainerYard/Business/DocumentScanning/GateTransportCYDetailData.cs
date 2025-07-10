using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GateTransportCYDetailData),
	Enterprise.Core.Constants.DocManagerCodes.GateTransportCYDetail)]

namespace Enterprise.Freight.ContainerYard.Business
{
	using System;
	using ZArchitecture.Core;

	class GateTransportCYDetailData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GateTransportCYDetail);

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("36ca336c-8b59-41ed-8436-f50672cadd0a", "Container Yard Gate Transport Detail");
			}
		}

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => null;
	}
}
