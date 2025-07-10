using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.Business.JobDeclarationExtensions
{
	public static class JobDeclarationExtensions
	{
		public static bool IsPublishToUniversalTransactionOK(this ISendsMessagesToCustoms notifier, PublishToUniversalResult universalResult)
		{
			var result = true;
			if (universalResult == null)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("01482892-F9BC-48EF-9EBB-E3D811F8879B", "Something went wrong during creation of the transaction"));
				result = false;
			}
			else if (universalResult.ResultType == UniversalResult.HadErrors)
			{
				notifier.NotifyUserOfAnInvalidOperation(universalResult.ErrorMessage);
				result = false;
			}
			return result;
		}
	}
}
