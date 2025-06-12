using System.Collections.Generic;
using System.ServiceModel;
using CargoWise.eHub.Common;

namespace DmsGateway
{
	[MaxFaultSize(5242880)]
	[ServiceContract(Name = "eHubStreamedService", Namespace = StreamedServiceConstants.StreamedServiceNamespace)]
	public interface IeHubStreamedService : IeAdapterStreamedService
	{
		[OperationContract]
		[SendStreamOperationFormat]
		[FaultContract(typeof(ApplicationFault), Action = StreamedServiceConstants.FaultContractAction)]
		RetrieveStreamResponse RetrieveStream();

		[OperationContract]
		[FaultContract(typeof(ApplicationFault), Action = StreamedServiceConstants.FaultContractAction)]
		void FinaliseBatch(string trackingID);

		[OperationContract]
		[FaultContract(typeof(ApplicationFault), Action = StreamedServiceConstants.FaultContractAction)]
		Dictionary<string, MessageStatus> GetMessageStatuses(string[] trackingIDs);
	}
}
