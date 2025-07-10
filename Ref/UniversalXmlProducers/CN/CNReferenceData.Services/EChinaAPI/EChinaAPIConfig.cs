namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public interface IEChinaAPIConfig
	{
		string EChinaCheckForUpdateUrl { get; }
		string EChinaAppCode { get; }
		string EChinaGetUpdatesUrl { get; }
	}
}
