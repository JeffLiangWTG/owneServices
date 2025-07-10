namespace Enterprise.Customs.Business.Testing
{
	sealed class CreateAndSubmitBrokerageJobRunnerForTesting : CreateAndSubmitBrokerageJobRunner
	{
		protected override BrokerageSubmitRunner GetBrokerageSubmitRunnerCore()
		{
			return new BrokerageSubmitRunnerTest.BrokerageSubmitRunnerForTesting();
		}

		protected override CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			return new CreateDeclarationHelperForTesting();
		}
	}
}
