using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class LinkedCusAuthorisationRuleLookups : CusPermitRuleLookups
	{
		public LinkedCusAuthorisationRuleLookups(LinkedCusAuthorisationRule parent) : base(parent)
		{
		}

		public new LinkedCusAuthorisationRule Parent => (LinkedCusAuthorisationRule)base.Parent;

		public virtual CodeDescriptionPairList RuleCodeList => Factory.GetCachedValue<LinkedCusAuthorisationRuleTypeList>();

		public ICollection ValueList
		{
			get
			{
				ICollection result = new CodeDescriptionPairList();
				if (ValueListFromRuleCode.TryGetValue(Parent.CPR_RuleCode, out var valueListGetter) && valueListGetter != null)
				{
					result = valueListGetter.Invoke();
				}
				return result;
			}
		}

		Dictionary<ZString, Func<ICollection>> ValueListFromRuleCode
		{
			get
			{
				if (valueListFromRuleCode == null)
				{
					valueListFromRuleCode = new CachedProperty<Dictionary<ZString, Func<ICollection>>>(Parent.Factory, GetValueListFromRuleCodeCore);
				}
				return valueListFromRuleCode.Value;
			}
		}

		CachedProperty<Dictionary<ZString, Func<ICollection>>> valueListFromRuleCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			return new Dictionary<ZString, Func<ICollection>>
			{
				{ LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, () => CustomsOfficeList }
			};
		}

		ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			Factory,
			Parent.AuthorisationHeader.CPH_RN_NKCountryCode,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
			ZDateTime.Today);
	}
}
