using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MaximumAllowedTransactionAmountRegistryItem : StronglyTypedRegistryItem<MaximumAllowedTransactionAmount>
	{
		public MaximumAllowedTransactionAmountRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, MaximumAllowedTransactionAmount defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MaximumAllowedTransactionAmountRegistryDataType(), storage, options, defaultValue))
		{
		}

		public MaximumAllowedTransactionAmountRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool shouldCheckMaximumSettingExceedSystemDefined)
			: base(new MaximumAllowedTransactionAmountRegistryItemImpl(name, category, caption, hint, storage, options, shouldCheckMaximumSettingExceedSystemDefined))
		{
		}
	}

	class MaximumAllowedTransactionAmountRegistryItemImpl : RegistryItemImpl
	{
		public MaximumAllowedTransactionAmountRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool shouldCheckMaximumSettingExceedSystemDefined)
				: base(name, category, caption, hint, new MaximumAllowedTransactionAmountRegistryDataType(shouldCheckMaximumSettingExceedSystemDefined), storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.MaximumAllowedTransactionAmountRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class MaximumAllowedTransactionAmountRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MaximumAllowedTransactionAmount>
	{
		public MaximumAllowedTransactionAmountRegistryDataType()
		{
		}

		public MaximumAllowedTransactionAmountRegistryDataType(bool shouldValidate)
		{
			this.ShouldCheckMaximumSettingExceedSystemDefined = shouldValidate;
		}

		public bool ShouldCheckMaximumSettingExceedSystemDefined { get; internal set; }

		protected override MaximumAllowedTransactionAmount CloneValue(MaximumAllowedTransactionAmount value)
		{
			value.ShouldCheckMaximumSettingExceedSystemDefined = ShouldCheckMaximumSettingExceedSystemDefined;
			return (MaximumAllowedTransactionAmount)value.Clone(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), value.Factory);
		}

		protected override MaximumAllowedTransactionAmount DeserialiseCore(byte[] value)
		{
			var deserialisedValue = base.DeserialiseCore(value);
			deserialisedValue.ShouldCheckMaximumSettingExceedSystemDefined = ShouldCheckMaximumSettingExceedSystemDefined;
			return deserialisedValue;
		}
	}
}
