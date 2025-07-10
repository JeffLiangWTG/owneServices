using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GLJournalExchangeRateTypeRegistryItem : StronglyTypedRegistryItem<GLJournalExchangeRateType>
	{
		public GLJournalExchangeRateTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, GLJournalExchangeRateType defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GLJournalExchangeRateTypeRegistryDataType(), storage, options, defaultValue))
		{
		}

		public GLJournalExchangeRateTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool shouldCheckMaximumSettingExceedSystemDefined)
			: base(new GLJournalExchangeRateTypeRegistryItemImpl(name, category, caption, hint, storage, options, shouldCheckMaximumSettingExceedSystemDefined))
		{
		}
	}

	class GLJournalExchangeRateTypeRegistryItemImpl : RegistryItemImpl
	{
		public GLJournalExchangeRateTypeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool shouldCheckMaximumSettingExceedSystemDefined)
				: base(name, category, caption, hint, new GLJournalExchangeRateTypeRegistryDataType(), storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.GLJournalExchangeRateTypeRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class GLJournalExchangeRateTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GLJournalExchangeRateType>
	{
		public GLJournalExchangeRateTypeRegistryDataType()
		{
		}

		protected override GLJournalExchangeRateType CloneValue(GLJournalExchangeRateType value)
		{
			return (GLJournalExchangeRateType)value.Clone(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), value.Factory);
		}

		protected override GLJournalExchangeRateType DeserialiseCore(byte[] value)
		{
			var deserialisedValue = base.DeserialiseCore(value);
			return deserialisedValue;
		}
	}
}
