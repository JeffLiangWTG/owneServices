namespace Enterprise.MasterFiles.Business
{
	public interface IAPIKeyGeneratorStrategy
	{
		string GenerateAPIKey(GlbCompany company);
	}
}
