using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public class CusAuthorisationRuleLookups : Customs.Business.CusAuthorisationRuleLookups
	{
		public CusAuthorisationRuleLookups(Customs.Business.CusAuthorisationRule parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList RuleCodeList
		{
			get
			{
				var authorisationType = Parent.AuthorisationHeader?.CPH_Type ?? ZString.Empty;
				return Factory.GetCachedValue("Enterprise.Customs.NO.Business.CusAuthorisationRuleLookups.RuleCodeList" + "|" + authorisationType, () =>
				{
					var authRules = new CodeDescriptionPairList();
					if (authorisationType == CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration ||
						authorisationType == CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration)
					{
						authRules.AddPairIfNotExist(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, CusAuthorisationRuleTypeList.Descriptions.MainCustomsOffice);
					}
					else
					{
						authRules.AddRange(base.RuleCodeList);
					}
					authRules.Sort();
					return authRules;
				});
			}
		}
	}
}
