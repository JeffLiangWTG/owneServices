namespace Enterprise.Customs.Business
{
	public static class CheckCreditHelper
	{
		public static bool CheckCredit(this BaseJobDeclaration jobDeclaration, out string reasonForNotAllowed)
		{
			reasonForNotAllowed = string.Empty;

			var helper = new MessageManagerCreditCheckWithSecurityHelper(jobDeclaration);
			var shouldContinue = helper.IsCreditCheckOKToSend;
			if (!shouldContinue && !helper.IsCreditCheckDoneOutsideCW1)
			{
				reasonForNotAllowed = helper.ReasonForNotAllowed;
			}

			return shouldContinue;
		}
	}
}
