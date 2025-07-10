using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItem : StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch>
	{
		public ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceSubtypeAttributionRulesetByTransactionHeaderBranchRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}
	}

	public class ComplianceSubtypeAttributionRulesetByTransactionHeaderBranchRegistryItemImpl : RegistryItemImpl
	{
		public ComplianceSubtypeAttributionRulesetByTransactionHeaderBranchRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType(), storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(new FallbackLevel(companyPK, branchPK, departmentPK), RegistryFactory.Instance, string.Empty);
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceSubTypeAttributionRuleSetRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch>
	{
		public ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryDataType()
		{
		}
	}
}
