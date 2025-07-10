namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public static class ActionChainTestExtensions
	{
		public static string GetChainAsString(this ActionChain chain)
		{
			var sChain = string.Empty;
			var fChain = string.Empty;
			var cChain = string.Empty;
			var bChain = string.Empty;

			if (chain.successChain != null)
			{
				sChain = $"success: {chain.successChain.GetChainAsString()} ";
			}
			if (chain.failureChain != null)
			{
				fChain = $"failure: {chain.failureChain.GetChainAsString()} ";
			}
			if (chain.commonChain != null)
			{
				cChain = $"common: {chain.commonChain.GetChainAsString()} ";
			}
			if (chain.subChain != null)
			{
				bChain = $"{chain.subChain.GetChainAsString()} ";
			}

			var result = $"{{ {chain.actionName}: {bChain}{sChain}{fChain}{cChain}}}";
			return result;
		}
	}
}
