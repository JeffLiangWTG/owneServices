namespace Enterprise.MasterFiles.Business
{
	public interface ITaxFrameworkThresholdMethodProvider
	{
		bool IsTransactionLevelGroupThresholdMethodSupported { get; }
		bool IsTransactionLevelTaxBaseThresholdMethodSupported { get; }
	}
}
