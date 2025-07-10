using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class CustomsRuleHelper
	{
		public static ZString ValidateWithTariffRule(ZString tariff, BaseJobDeclaration declaration)
		{
			var result = ZString.Empty;
			if (!tariff.IsEmpty && declaration?.CustomsRule is CustomsRule customsRule)
			{
				var trfRules = customsRule.Rules.Where(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TariffNumber);
				foreach (var trfRule in trfRules)
				{
					if (trfRule.CPR_ValueTo.IsEmpty)
					{
						if (tariff.StartsWith(trfRule.CPR_ValueFrom))
						{
							result = string.Format(TRFRuleMessageError, customsRule.HumanReadableName);
							break;
						}
					}
					else
					{
						if (ZDecimal.TryParse(tariff, out var tariffNum)
							&& ZDecimal.TryParse(trfRule.CPR_ValueFrom.PadRight(10, '0'), out var rangeFrom)
							&& ZDecimal.TryParse(trfRule.CPR_ValueTo.PadRight(10, '0'), out var rangeTo)
							&& tariffNum >= rangeFrom && tariffNum <= rangeTo)
						{
							result = string.Format(TRFRuleMessageError, customsRule.HumanReadableName);
							break;
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message string")]
		public const string TRFRuleMessageError = "Tariff number flagged as error per Customs Rule: {0}.";
	}
}
