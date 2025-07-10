namespace Enterprise.Customs.Business
{
	public interface ILandedCostOnlyConfigurationProvider
	{
		ILandedCostOnlyConfiguration GetLandedCostOnlyConfiguration(BaseJobDeclaration declaration);
	}
}
