using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitCountrySpecificInstruction : ISharedCountrySpecificInstruction
	{
		protected SharedCusPermitCountrySpecificInstruction(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		protected readonly BusinessObjectFactory Factory;

		#region Lists

		public virtual PermitTypeList GetTypeList()
		{
			return Factory.GetCachedValue<PermitTypeList>();
		}

		public virtual CodeDescriptionPairList GetSubTypeList(ZString typeCode)
		{
			return Factory.GetCachedValue<PermitSubTypeList>();
		}

		public virtual PermitQtyValIndicatorList GetQtyValIndicatorList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue<PermitQtyValIndicatorList>();
		}

		public virtual PermitTransactionTypeList GetTransactionTypeList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue<PermitTransactionTypeList>();
		}

		public virtual PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		public virtual PermitRuleCodeList GetRuleCodeListForModule()
		{
			return Factory.GetCachedValue<PermitRuleCodeList>();
		}

		public virtual ICollection GetLookupList(SharedCusPermitHeader permitHeader, ZString ruleCode)
		{
			switch (ruleCode)
			{
				case BaseCusPermitRule.RuleCodes.CountryOfOrigin:
					return new MasterFiles.Business.RefCountryCollection(Factory);
				default:
					return new CodeDescriptionPairList();
			}
		}

		public virtual ICollection GetPermitNumberCollection(SharedCusPermitHeader permitHeader) => null;

		public virtual AppliesToIndicator GetAppliesToIndicator(ZString permitType, ZString permitSubType)
		{
			return AppliesToIndicator.ForceEmpty;
		}

		#endregion

		#region CusPermitRule

		public virtual PermitMatchingType GetMatchingType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case SharedCusPermitRule.RuleCodes.CountryOfOrigin:
					return PermitMatchingType.SingleValue;
				default:
					return PermitMatchingType.Range;
			}
		}

		public virtual IComparer GetRangeComparer(ZString ruleCode)
		{
			return StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, false);
		}

		public virtual ZString GetValueFromFieldType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case SharedCusPermitRule.RuleCodes.CountryOfOrigin:
					return nameof(FieldType.TextCodeFindBox);
				default:
					return nameof(FieldType.Text);
			}
		}

		public virtual ZString GetValueToFieldType(ZString ruleCode)
		{
			return nameof(FieldType.Text);
		}

		#endregion

		public virtual bool IsQtyValIndicatorMandatory => true;
	}

	public enum AppliesToIndicator
	{
		Mandatory,
		Optional,
		ForceEmpty
	}
}
