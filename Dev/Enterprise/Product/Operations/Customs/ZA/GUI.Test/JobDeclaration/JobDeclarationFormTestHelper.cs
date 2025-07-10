using System.Collections.Generic;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	static class JobDeclarationFormTestHelper
	{
		public static Dictionary<string, string> GetIgnoreControlForLock(Dictionary<string, string> baseResult)
		{
			var result = baseResult;
			if (result.ContainsKey(Core.Constants.Customs.DeclarationTabPages.Codes.Misc))
			{
				result[Core.Constants.Customs.DeclarationTabPages.Codes.Misc] += ",PaymentPartyDropEdit";
			}
			else
			{
				result.Add(Core.Constants.Customs.DeclarationTabPages.Codes.Misc, "PaymentPartyDropEdit");
			}

			return result;
		}
	}
}
