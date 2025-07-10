using System.Collections.Generic;
using WTG.CreditCheck;

namespace Enterprise.MasterFiles.GUI
{
	public class CreditCheckCodeMapping : CreditCheckPriceItemCodeMapping
	{
		Dictionary<string, string> priceItemCodeDescriptionMapping;

		public IReadOnlyDictionary<string, string> PriceItemCodeDescriptionMapping
		{
			get
			{
				if (priceItemCodeDescriptionMapping == null)
				{
					priceItemCodeDescriptionMapping = new Dictionary<string, string>
					{
						[Constants.BusinessVerification] = ResourceStringHelper.BusinessVerification,
						[Constants.EnterpriseManagement] = ResourceStringHelper.EnterpriseManagement,
						[Constants.DelinquencyScore] = ResourceStringHelper.DelinquencyScore,
						[Constants.DecisionSupport] = ResourceStringHelper.DecisionSupport,
					};
				}

				return priceItemCodeDescriptionMapping;
			}
		}
	}
}
