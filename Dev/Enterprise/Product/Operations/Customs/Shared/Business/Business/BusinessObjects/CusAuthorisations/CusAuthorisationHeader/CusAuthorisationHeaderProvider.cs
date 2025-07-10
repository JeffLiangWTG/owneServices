using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationHeaderProvider
	{
		public static CusAuthorisationHeaderProvider GetByCountryCode(ZString countryCode)
		{
			CusAuthorisationHeaderProvider result = null;
			if (!countryCode.IsEmpty)
			{
				var jurisdictionCountry = (ZString)Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
				var types = ObjectFactory.Get<Hashtable>("CusAuthorisationHeaderProviders");
				var objectHandle = (ObjectHandle)types[jurisdictionCountry.ToString()];
				result = (CusAuthorisationHeaderProvider)objectHandle?.GetObject(jurisdictionCountry);

				if (result == null)
				{
					if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(jurisdictionCountry))
					{
						objectHandle = (ObjectHandle)types[Core.Constants.CountryCodes.EuropeanUnion];
						result = (CusAuthorisationHeaderProvider)objectHandle?.GetObject(jurisdictionCountry);
					}
				}
			}
			return result ?? new CusAuthorisationHeaderProvider(countryCode);
		}

		public ZBool AllowMixedCaseAuthorisationNumbers(CusAuthorisationHeader header) => AllowMixedCaseAuthorisationNumbersCore(header);

		protected virtual ZBool AllowMixedCaseAuthorisationNumbersCore(CusAuthorisationHeader header) => false;

		protected CusAuthorisationHeaderProvider(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ZString CountryCode { get; }

		public bool EnableAdHoc => EnableAdHocCore;
		protected virtual bool EnableAdHocCore => false;

		public bool ShowRelatedAuthorisationWithoutReference => ShowRelatedAuthorisationWithoutReferenceCore;
		protected virtual bool ShowRelatedAuthorisationWithoutReferenceCore => false;

		public bool IsAgcNumberFieldALookup => IsAgcNumberFieldALookupCore;
		protected virtual bool IsAgcNumberFieldALookupCore => true;

		public bool ShowCustomsCode => ShowCustomsCodeCore;
		protected virtual bool ShowCustomsCodeCore => false;

		#region Authorisation Header
		public CusAuthorisationHeaderLookups GetNewLookups(CusAuthorisationHeader cusAuthorisationHeader) => GetNewLookupsCore(cusAuthorisationHeader);

		public CusAuthorisationHeaderValidation GetNewValidation(CusAuthorisationHeader cusAuthorisationHeader) => GetNewValidationCore(cusAuthorisationHeader);

		public CodeDescriptionPairList GetAuthorisationTypeList(BusinessObjectFactory factory) => GetAuthorisationTypeListCore(factory);

		protected ZString KeyForGeneralHeaderType => "KeyForGeneralHeaderType";

		protected virtual CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

		protected virtual CusAuthorisationHeaderValidation GetNewValidationCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderValidation(cusAuthorisationHeader);

		protected virtual List<ZString> GetAuthorizationTypesNeedAddress()
		{
			return new List<ZString>
			{
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
				CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
			};
		}

		protected virtual ZBool IsAuthorisationNumberValidCore(CusAuthorisationHeader authorisationHeader) => ZBool.True;

		protected virtual ZString GetAuthorisationNumberInvalidFormatMessageCore(CusAuthorisationHeader authorisationHeader) => ZString.Empty;

		protected virtual CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory) => factory.GetCachedValue<CusAuthorizationHeaderTypeList>();

		public string GetCustomsNumberProviderKey(ZString type)
		{
			return GetCustomsNumberProviderKeyCore(type);
		}

		protected virtual string GetCustomsNumberProviderKeyCore(ZString type)
		{
			return string.Empty;
		}
		#endregion

		#region Authorisation Rules
		public CusAuthorisationRuleLookups GetNewLookups(CusAuthorisationRule cusAuthorisationRule) => GetNewLookupsCore(cusAuthorisationRule);

		public CusAuthorisationRuleValidation GetNewValidation(CusAuthorisationRule cusAuthorisationRule) => GetNewValidationCore(cusAuthorisationRule);

		public ZString GetRuleDescription(CusAuthorisationRule cusAuthorisationRule)
		{
			var description = ZString.Empty;
			if (cusAuthorisationRule != null && RuleDescriptionFunctions.TryGetValue(cusAuthorisationRule.CPR_RuleCode, out var descriptionFunction))
			{
				description = descriptionFunction(cusAuthorisationRule);
			}
			return description;
		}
		public string GetRuleValueFromFieldType(CusAuthorisationRule cusAuthorisationRule) => GetRuleValueFromFieldTypeCore(cusAuthorisationRule);

		protected virtual string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = nameof(FieldType.Text);
			if (cusAuthorisationRule != null && RuleValueFieldTypes.TryGetValue(cusAuthorisationRule.CPR_RuleCode, out var fieldType))
			{
				result = fieldType.ToString();
			}
			return result;
		}

		public void AddOrUpdateLinkedRule(CusAuthorisationRule cusAuthorisationRule) => AddOrUpdateLinkedRuleCore(cusAuthorisationRule);
		protected virtual void AddOrUpdateLinkedRuleCore(CusAuthorisationRule cusAuthorisationRule)
		{
		}

		public ZInt GetRuleValueFromMaxLength(CusAuthorisationRule cusAuthorisationRule) => GetRuleValueFromMaxLengthCore(cusAuthorisationRule);

		public string GetRuleDescriptionFieldType(CusAuthorisationRule cusAuthorisationRule) => GetRuleDescriptionFieldTypeCore(cusAuthorisationRule);

		protected virtual string GetRuleDescriptionFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = nameof(FieldType.Text);
			if (cusAuthorisationRule != null && RuleDescriptionFieldTypes.TryGetValue(cusAuthorisationRule.CPR_RuleCode, out var fieldType))
			{
				result = fieldType.ToString();
			}
			return result;
		}

		public IEnumerable<CusAuthorisationRuleRequirement> GetValidAuthorisationRuleRequirements(CusAuthorisationHeader cusAuthorisationHeader, ZString authorisationType)
		{
			if (authorisationType.IsEmpty)
			{
				authorisationType = KeyForGeneralHeaderType;
			}
			return cusAuthorisationHeader.Factory.GetCachedValue($"ValidAuthorisationRuleRequirements|{CountryCode}|{authorisationType}|{cusAuthorisationHeader.CPH_IsAdHoc}", () =>
			{
				var result = new List<CusAuthorisationRuleRequirement>();
				var allRequirements = GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);
				if (allRequirements.TryGetValue(authorisationType, out var typedRequirements))
				{
					result.AddRange(typedRequirements);
				}

				if (!authorisationType.Equals(KeyForGeneralHeaderType))
				{
					if (allRequirements.TryGetValue(KeyForGeneralHeaderType, out var generalrequirements))
					{
						result.AddRange(generalrequirements);
					}
				}

				return result;
			});
		}

		public ImmutableHashSet<ZString> RuleCodesWithLinkedRules(CusAuthorisationRule cusAuthorisationRule) =>
			cusAuthorisationRule.Factory.GetCachedValue($"RuleCodesWithLinkedRules|{CountryCode}", RuleCodesWithLinkedRulesCore);

		public IEnumerable<ZString> AuthorizationTypesNeedAddress
		{
			get
			{
				if (authorizationTypesNeedAddress == null)
				{
					var list = GetAuthorizationTypesNeedAddress();
					list.Sort();
					authorizationTypesNeedAddress = list;
				}
				return authorizationTypesNeedAddress;
			}
		}
		IEnumerable<ZString> authorizationTypesNeedAddress;

		public IEnumerable<ZString> AuthorizationTypesNeedWarehouse => new List<ZString>
		{
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
		};

		public ZBool IsAuthorisationNumberValid(CusAuthorisationHeader authorisationHeader) => IsAuthorisationNumberValidCore(authorisationHeader);

		public ZString GetAuthorisationNumberInvalidFormatMessage(CusAuthorisationHeader authorisationHeader) => GetAuthorisationNumberInvalidFormatMessageCore(authorisationHeader);

		public CodeDescriptionPairList GetRuleCodeListForModule(BusinessObjectFactory factory) => GetRuleCodeListForModuleCore(factory);

		protected virtual CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory) => factory.GetCachedValue<CusAuthorisationRuleTypeList>();

		protected virtual CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected virtual CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

		protected virtual void SetupRuleDescriptionFunctions()
		{
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.Location, rule => GetCusCodeDescription(rule, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities));
		}

		protected void AddRuleDescriptionFunction(ZString ruleCode, Func<CusAuthorisationRule, ZString> descriptionFunction) => ruleDescriptionFunctions[ruleCode] = descriptionFunction;

		protected virtual Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			return new Dictionary<ZString, FieldType>
			{
				{ CusAuthorisationRuleTypeList.Codes.Location, FieldType.TextCodeFindBox }
			};
		}

		protected virtual Dictionary<ZString, FieldType> GetRuleDescriptionFieldTypesCore()
		{
			return new Dictionary<ZString, FieldType>
			{
				{ CusAuthorisationRuleTypeList.Codes.Location, FieldType.Text }
			};
		}

		protected virtual ZInt GetRuleValueFromMaxLengthCore(CusAuthorisationRule cusAuthorisationRule) => CusAuthorisationRule.Schema.CPR_ValueFromMaxLength;

		protected virtual Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader) => new Dictionary<ZString, List<CusAuthorisationRuleRequirement>>();

		protected void AddOrUpdateValidAuthorisationRuleRepititions(IDictionary<ZString, List<CusAuthorisationRuleRequirement>> existingRuleRepetitions, ZString authorisationTypeToAdd, List<CusAuthorisationRuleRequirement> additionalRuleRepetitions)
		{
			if (existingRuleRepetitions.TryGetValue(authorisationTypeToAdd, out var existingRuleRequirements))
			{
				foreach (var ruleRequirementToAdd in additionalRuleRepetitions)
				{
					existingRuleRequirements.RemoveAll(x => x.RuleType == ruleRequirementToAdd.RuleType);
					existingRuleRequirements.Add(ruleRequirementToAdd);
				}
			}
			else
			{
				existingRuleRepetitions.Add(authorisationTypeToAdd, additionalRuleRepetitions);
			}
		}
		protected virtual ImmutableHashSet<ZString> RuleCodesWithLinkedRulesCore() => ImmutableHashSet.Create<ZString>(CusAuthorisationRuleTypeList.Codes.Location);

		Dictionary<ZString, Func<CusAuthorisationRule, ZString>> RuleDescriptionFunctions
		{
			get
			{
				if (ruleDescriptionFunctions == null)
				{
					ruleDescriptionFunctions = new Dictionary<ZString, Func<CusAuthorisationRule, ZString>>();
					SetupRuleDescriptionFunctions();
				}
				return ruleDescriptionFunctions;
			}
		}
		Dictionary<ZString, Func<CusAuthorisationRule, ZString>> ruleDescriptionFunctions;

		protected virtual Dictionary<ZString, FieldType> RuleValueFieldTypes => ruleValueFieldTypes ?? (ruleValueFieldTypes = GetRuleValueFieldTypesCore());
		protected Dictionary<ZString, FieldType> ruleValueFieldTypes;

		protected virtual Dictionary<ZString, FieldType> RuleDescriptionFieldTypes => ruleDescriptionFieldTypes ?? (ruleDescriptionFieldTypes = GetRuleDescriptionFieldTypesCore());
		protected Dictionary<ZString, FieldType> ruleDescriptionFieldTypes;
		#endregion

		#region Linked Authorisation Rules
		public LinkedCusAuthorisationRuleLookups GetNewLookups(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => GetNewLookupsCore(linkedCusAuthorisationRule);

		public LinkedCusAuthorisationRuleValidation GetNewValidation(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => GetNewValidationCore(linkedCusAuthorisationRule);

		public ZString GetLinkedRuleDescription(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var description = ZString.Empty;
			if (linkedCusAuthorisationRule != null && LinkedRuleDescriptionFunctions.TryGetValue(linkedCusAuthorisationRule.CPR_RuleCode, out var descriptionFunction))
			{
				description = descriptionFunction(linkedCusAuthorisationRule);
			}
			return description;
		}

		public string GetLinkedRuleValueFromFieldType(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => GetLinkedRuleValueFromFieldTypeCore(linkedCusAuthorisationRule);

		protected virtual string GetLinkedRuleValueFromFieldTypeCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = nameof(FieldType.Text);
			if (linkedCusAuthorisationRule != null && LinkedRuleValueFieldTypes.TryGetValue(linkedCusAuthorisationRule.CPR_RuleCode, out var fieldType))
			{
				result = fieldType.ToString();
			}
			return result;
		}

		public IReadOnlyDictionary<ZString, List<LinkedCusAuthorisationRuleRange>> GetValidLinkedAuthorizationRuleRepetitions(BusinessObjectFactory factory) =>
					factory.GetCachedValue($"ValidLinkedAuthorizationRuleRepetitions|{CountryCode}", () => GetValidLinkedAuthorizationRuleRepetitionsCore().ToImmutableDictionary());

		protected virtual LinkedCusAuthorisationRuleLookups GetNewLookupsCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);

		protected virtual LinkedCusAuthorisationRuleValidation GetNewValidationCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => new LinkedCusAuthorisationRuleValidation(linkedCusAuthorisationRule);

		protected virtual void SetupLinkedRuleDescriptionFunctions()
		{
			AddLinkedRuleDescriptionFunction(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, rule => GetCusCodeDescription(rule, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
		}

		protected void AddLinkedRuleDescriptionFunction(ZString ruleCode, Func<LinkedCusAuthorisationRule, ZString> descriptionFunction) => linkedRuleDescriptionFunctions[ruleCode] = descriptionFunction;

		protected virtual Dictionary<ZString, FieldType> GetLinkedRuleValueFieldTypesCore()
		{
			return new Dictionary<ZString, FieldType>
			{
				{ "CUS", FieldType.TextCodeFindBox }
			};
		}

		protected virtual Dictionary<ZString, List<LinkedCusAuthorisationRuleRange>> GetValidLinkedAuthorizationRuleRepetitionsCore() => new Dictionary<ZString, List<LinkedCusAuthorisationRuleRange>>();

		protected void AddOrUpdateValidLinkedAuthorizationRuleRepetitions(IDictionary<ZString, List<LinkedCusAuthorisationRuleRange>> existingRuleRepetitions, ZString ruleTypeToAdd, List<LinkedCusAuthorisationRuleRange> additionalRuleRepetitions)
		{
			if (existingRuleRepetitions.TryGetValue(ruleTypeToAdd, out var existingRuleRanges))
			{
				foreach (var ruleRangeToAdd in additionalRuleRepetitions)
				{
					existingRuleRanges.RemoveAll(x => x.RuleType == ruleRangeToAdd.RuleType);
					existingRuleRanges.Add(ruleRangeToAdd);
				}
			}
			else
			{
				existingRuleRepetitions.Add(ruleTypeToAdd, additionalRuleRepetitions);
			}
		}

		Dictionary<ZString, Func<LinkedCusAuthorisationRule, ZString>> LinkedRuleDescriptionFunctions
		{
			get
			{
				if (linkedRuleDescriptionFunctions == null)
				{
					linkedRuleDescriptionFunctions = new Dictionary<ZString, Func<LinkedCusAuthorisationRule, ZString>>();
					SetupLinkedRuleDescriptionFunctions();
				}
				return linkedRuleDescriptionFunctions;
			}
		}
		Dictionary<ZString, Func<LinkedCusAuthorisationRule, ZString>> linkedRuleDescriptionFunctions;

		protected virtual Dictionary<ZString, FieldType> LinkedRuleValueFieldTypes => linkedRuleValueFieldTypes ?? (linkedRuleValueFieldTypes = GetLinkedRuleValueFieldTypesCore());
		protected Dictionary<ZString, FieldType> linkedRuleValueFieldTypes;

		public ZBool IsLinkedRuleCodeReadOnly(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => IsLinkedRuleCodeReadOnlyCore(linkedCusAuthorisationRule);
		protected virtual ZBool IsLinkedRuleCodeReadOnlyCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => true;

		public ZBool? IsLinkedRuleReadOnly(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => IsLinkedRuleReadOnlyCore(linkedCusAuthorisationRule);
		protected virtual ZBool? IsLinkedRuleReadOnlyCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => null;

		public LinkedCusAuthorisationRuleCollection GetLinkedCusAuthorisationRules(CusAuthorisationRule cusAuthorisationRule) => GetLinkedCusAuthorisationRulesCore(cusAuthorisationRule);
		protected virtual LinkedCusAuthorisationRuleCollection GetLinkedCusAuthorisationRulesCore(CusAuthorisationRule cusAuthorisationRule) => new LinkedCusAuthorisationRuleCollection(cusAuthorisationRule);

		public ZBool IsMasterLinkedRule(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => IsMasterLinkedRuleCore(linkedCusAuthorisationRule);
		protected virtual ZBool IsMasterLinkedRuleCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => false;
		#endregion

		protected ZString GetCusCodeDescription(CommonCusPermitRule rule, ZString codeType) => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(rule.Factory, rule.CPR_ValueFrom, CountryCode, codeType, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;

		public virtual ZString DefaultTemporaryAuthorizationNumber => ZString.Empty;

		public void CheckRuleMinRequirement(CusAuthorisationHeader parent, ZPropertyInfo info)
		{
			var authorisationType = parent.CPH_Type;
			var authorisationRuleRequirements = GetValidAuthorisationRuleRequirements(parent, authorisationType);
			foreach (var ruleRequirementDetails in authorisationRuleRequirements)
			{
				CheckAttributeTypeMinRequirement(parent, info, authorisationType, ruleRequirementDetails.RuleType, ruleRequirementDetails.MinRequired, ruleRequirementDetails.AdditionalMinAuthorisationCheck, ruleRequirementDetails.NotificationType);
			}
		}

		protected void CheckAttributeTypeMinRequirement(CusAuthorisationHeader parent, ZPropertyInfo info, ZString authorisationType, ZString ruleType, ZInt minCount, Func<bool> additionalAuthorisationCheck, INotificationType notificationType)
		{
			if (!minCount.IsEmpty
				&& (additionalAuthorisationCheck?.Invoke() ?? true)
				&& parent.CusAuthorisationRules.Count(x => x.CPR_RuleCode == ruleType) < minCount)
			{
				info.AddNotification(notificationType, Res.GetString("EA93EFA0-D358-4D21-8539-51B03E47317F",
					"You are required to have at least {0} authorization rule of type '{1}' for authorization type '{2}'.", minCount, ruleType, authorisationType));
			}
		}
	}
}
