namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface IErrorCodeDownloadContextProvider
	{
		ErrorCodeDownloadContext GetContext(ErrorCodeType type);
	}
}
