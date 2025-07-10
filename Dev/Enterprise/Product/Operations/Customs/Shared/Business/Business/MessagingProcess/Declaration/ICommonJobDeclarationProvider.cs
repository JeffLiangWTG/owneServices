namespace Enterprise.Customs.Business.MessagingProcess.Declaration
{
	public interface ICommonJobDeclarationProvider : ISupportPreSendValidation, ISupportConfigureProcess, ISupportSecurityCheckpoints, ISupportCreditAndDPSCheck
	{
	}

	public interface ICommonJobDeclarationProviderFactory
	{
		ICommonJobDeclarationProvider Provider { get; }
	}
}
