using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI
{
	public static class CheckCreditGUIHelper
	{
		public static bool CheckCredit(this BaseJobDeclaration jobDeclaration)
		{
			var shouldContinue = jobDeclaration.CheckCredit(out var reasonForNotAllowed);
			if (!shouldContinue)
			{
				Globals.Message.ShowError(reasonForNotAllowed);
			}
			return shouldContinue;
		}
	}
}
