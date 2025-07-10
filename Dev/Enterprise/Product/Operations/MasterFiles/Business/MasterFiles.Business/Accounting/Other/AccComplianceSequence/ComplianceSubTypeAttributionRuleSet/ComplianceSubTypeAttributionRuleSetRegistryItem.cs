using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAttributionRuleSetRegistryItem : StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleSet>
	{
		public ComplianceSubTypeAttributionRuleSetRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceSubtypeAttributionRulesetRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}
	}

	public class ComplianceSubtypeAttributionRulesetRegistryItemImpl : RegistryItemImpl
	{
		public ComplianceSubtypeAttributionRulesetRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ComplianceSubTypeAttributionRuleSetRegistryDataType(), storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var defaultValue = string.Empty;
			var factory = RegistryFactory.Instance;
			GlbCompany company = null;
			if (companyPK != Guid.Empty)
			{
				company = factory.Load<GlbCompany>(companyPK);
			}
			if (company != null)
			{
				var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(company.Country.Code);
				if (ruleSetProvider != null)
				{
					defaultValue = ruleSetProvider.GetDefaultRuleSet();
				}
			}

			return new ComplianceSubTypeAttributionRuleSet(new FallbackLevel(companyPK, branchPK, departmentPK), factory, defaultValue);
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceSubTypeAttributionRuleSetRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceSubTypeAttributionRuleSetRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceSubTypeAttributionRuleSet>
	{
		public ComplianceSubTypeAttributionRuleSetRegistryDataType()
		{
		}
	}
}
