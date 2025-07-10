namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public abstract class SendMessagesBaseActionProviderForTest
	{
		public int StepsTaken { get; set; }
		public string SuccessSteps { get; set; } = "";
		public string FailureSteps { get; set; } = "";

		protected ActionStep CommonActionStep(string methodName) => (ActionResult result) =>
		{
			StepsTaken++;

			if (SuccessSteps.Contains(methodName))
			{
				result.Success = true;
				result.AppendErrorNotification($"{methodName} Success");
			}
			else if (FailureSteps.Contains(methodName))
			{
				result.Success = false;
				result.AppendErrorNotification($"{methodName} Failure");
			}
			else
			{
				result.AppendInformationNotification(methodName);
			}

			return result;
		};
	}
}
