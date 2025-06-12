namespace CargoWise.eHub.Products.NZCustoms.Client
{
	public interface IServiceApi
	{
		CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2.SubmitLodgementResponse SendLodgementRequest(CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2.SubmitLodgementRequest request);
		CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1.RequestLodgeResponseResponse SendPullRequest(CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1.RequestLodgeResponseRequest requestLodgeResponseRequest);
	}
}
