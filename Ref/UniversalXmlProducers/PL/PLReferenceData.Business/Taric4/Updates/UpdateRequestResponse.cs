namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;

sealed class UpdateRequestResponse(UpdateRequest updateRequest, string requestResponse)
{
	public UpdateRequest UpdateRequest { get; } = updateRequest;
	public string RequestResponse { get; } = requestResponse;
}
