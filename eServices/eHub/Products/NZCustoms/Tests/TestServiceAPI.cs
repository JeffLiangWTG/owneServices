using System;
using CargoWise.eHub.Products.NZCustoms.Client;
using CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1;
using CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2;
namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	class TestServiceAPI : IServiceApi
	{
		Func<SubmitLodgementRequest, SubmitLodgementResponse> actionSend;
		Func<RequestLodgeResponseRequest, RequestLodgeResponseResponse> actionPull;

		public TestServiceAPI()
		{
		}

		public TestServiceAPI(Func<SubmitLodgementRequest, SubmitLodgementResponse> action)
		{
			this.actionSend = action;
		}

		public TestServiceAPI(Func<RequestLodgeResponseRequest, RequestLodgeResponseResponse> action)
		{
			this.actionPull = action;
		}

		public SubmitLodgementResponse SendLodgementRequest(SubmitLodgementRequest request)
		{
			return actionSend(request);
		}

		public RequestLodgeResponseResponse SendPullRequest(RequestLodgeResponseRequest requestLodgeResponseRequest)
		{
			RequestLodgeResponseRequestTest = requestLodgeResponseRequest;

			if (actionPull != null) actionPull(requestLodgeResponseRequest);

			return RequestLodgeResponseResponseTest;
		}

		public RequestLodgeResponseRequest RequestLodgeResponseRequestTest { get; set; }
		public RequestLodgeResponseResponse RequestLodgeResponseResponseTest { get; set; }
	}
}
