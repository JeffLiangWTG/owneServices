using System;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbConsignmenActionAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.LandTransportConsignmentAction)]
namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmenActionAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(DtbConsignmentAction);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(DtbConsignmentActionCollection);

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("54af8e82-e8b3-40ff-a20e-ffc9dcdba3b6", "Land Transport Consignment Action"); }
		}
	}
}
