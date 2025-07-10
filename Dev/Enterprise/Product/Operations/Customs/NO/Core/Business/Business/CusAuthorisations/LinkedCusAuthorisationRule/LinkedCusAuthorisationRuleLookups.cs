using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public class LinkedCusAuthorisationRuleLookups : Customs.Business.LinkedCusAuthorisationRuleLookups
	{
		public LinkedCusAuthorisationRuleLookups(Customs.Business.LinkedCusAuthorisationRule parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList RuleCodeList
		{
			get
			{
				var authorisationType = Parent.AuthorisationHeader?.CPH_Type;
				var authorisationRuleType = Parent.AuthorisationRule?.CPR_RuleCode ?? ZString.Empty;
				return Factory.GetCachedValue(string.Join("|", "Enterprise.Customs.NO.Business.LinkedCusAuthorisationRuleLookups.RuleCodeList", authorisationType, authorisationRuleType), () =>
				{
					var linkedRules = new CodeDescriptionPairList();
					if (authorisationRuleType == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice)
					{
						linkedRules.AddPair(LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, LinkedCusAuthorisationRuleTypeList.Descriptions.ValidCustomsOffices);
					}
					else
					{
						linkedRules.AddRange(base.RuleCodeList);
					}
					return linkedRules;
				});
			}
		}
	}
}
