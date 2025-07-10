using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public class CusAuthorisationHeaderProvider : Customs.Business.CusAuthorisationHeaderProvider
	{
		protected CusAuthorisationHeaderProvider(ZString countryCode)
			: base(countryCode)
		{
		}

		#region Authorisation Header

		protected override List<ZString> GetAuthorizationTypesNeedAddress()
		{
			var result = base.GetAuthorizationTypesNeedAddress();
			result.Add(CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration);
			result.Add(CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration);
			return result;
		}

		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.NO.Business.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore", () =>
			{
				var authTypes = new CusAuthorizationHeaderTypeList();
				authTypes.Sort();
				return authTypes;
			});
		}

		protected override Customs.Business.CusAuthorisationHeaderValidation GetNewValidationCore(CusAuthorisationHeader cusAuthorisationHeader)
			=> new CusAuthorisationHeaderValidation(cusAuthorisationHeader);

		#endregion

		#region Authorisation Rules

		protected override Customs.Business.LinkedCusAuthorisationRuleCollection GetLinkedCusAuthorisationRulesCore(CusAuthorisationRule cusAuthorisationRule)
			=> new LinkedCusAuthorisationRuleCollection(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule)
			=> new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule)
			=> new CusAuthorisationRuleValidation(cusAuthorisationRule);

		protected override Dictionary<ZString, FieldType> GetRuleDescriptionFieldTypesCore()
		{
			var fieldTypes = base.GetRuleDescriptionFieldTypesCore();
			fieldTypes[CusAuthorisationRuleTypeList.Codes.MainCustomsOffice] = FieldType.Text;
			return fieldTypes;
		}

		protected override CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.NO.Business.CusAuthorisationHeaderProvider.GetRuleCodeListForModuleCore", () =>
			{
				var authRules = new CusAuthorisationRuleTypeList();
				authRules.Sort();
				return authRules;
			});
		}

		protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			var fieldTypes = base.GetRuleValueFieldTypesCore();
			fieldTypes[CusAuthorisationRuleTypeList.Codes.MainCustomsOffice] = FieldType.Text;
			return fieldTypes;
		}

		protected override Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader)
		{
			var rules = base.GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration, new List<CusAuthorisationRuleRequirement>()
			{
				new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, 1, 1),
			});
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration, new List<CusAuthorisationRuleRequirement>()
			{
				new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, 1, 1),
			});
			return rules;
		}

		protected override ImmutableHashSet<ZString> RuleCodesWithLinkedRulesCore() =>
			ImmutableHashSet.Create<ZString>(CusAuthorisationRuleTypeList.Codes.Location, CusAuthorisationRuleTypeList.Codes.MainCustomsOffice);

		protected override void AddOrUpdateLinkedRuleCore(CusAuthorisationRule cusAuthorisationRule)
		{
			if (cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice)
			{
				var linkedRule = cusAuthorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(c => c.IsMasterLinkedRule);
				if (linkedRule == null)
				{
					linkedRule = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
					linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
				}
				linkedRule.CPR_ValueFrom = cusAuthorisationRule.CPR_ValueFrom;
				linkedRule.CPR_ValueTo = cusAuthorisationRule.CPR_ValueTo;
				linkedRule.CPR_Description = cusAuthorisationRule.CPR_Description;
				linkedRule.IsMasterLinkedRule = true;
			}
		}

		#endregion

		#region Linked Authorisation Rules

		protected override Dictionary<ZString, FieldType> GetLinkedRuleValueFieldTypesCore()
		{
			var result = base.GetLinkedRuleValueFieldTypesCore();
			result[LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices] = FieldType.Text;
			return result;
		}

		protected override Customs.Business.LinkedCusAuthorisationRuleLookups GetNewLookupsCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
			=> new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);

		protected override Customs.Business.LinkedCusAuthorisationRuleValidation GetNewValidationCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
			=> new LinkedCusAuthorisationRuleValidation(linkedCusAuthorisationRule);

		protected override Dictionary<ZString, List<LinkedCusAuthorisationRuleRange>> GetValidLinkedAuthorizationRuleRepetitionsCore()
		{
			var rules = base.GetValidLinkedAuthorizationRuleRepetitionsCore();
			AddOrUpdateValidLinkedAuthorizationRuleRepetitions(rules, CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, new List<LinkedCusAuthorisationRuleRange>()
			{
				new LinkedCusAuthorisationRuleRange(LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, 1, 0)
			});
			return rules;
		}

		protected override ZBool? IsLinkedRuleReadOnlyCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => IsMasterLinkedRule(linkedCusAuthorisationRule);

		protected override ZBool IsMasterLinkedRuleCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = false;

			var parentRule = linkedCusAuthorisationRule.AuthorisationRule;
			if (parentRule != null)
			{
				result = parentRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice
				&& linkedCusAuthorisationRule.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices
				&& linkedCusAuthorisationRule.IsMasterLinkedRule;
			}

			return result;
		}

		#endregion
	}
}
