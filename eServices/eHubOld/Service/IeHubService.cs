using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CargoWise.eServices.eHub.Service
{
	[ServiceContract(Name = "eHubService", Namespace = "http://ehub.cargowise.com/2010-06-02")]
	public interface IeHubService
	{
		[OperationContract]
		SendResponse Send(SendRequest request);

		[OperationContract]
		RetrieveResponse Retrieve(RetrieveRequest request);
	}
}
