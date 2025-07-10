using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingPendingTransactionsNotificationGroupRegistryItem : StronglyTypedRegistryItem<EInvoicingPendingTransactionsNotificationGroup>
	{
		public EInvoicingPendingTransactionsNotificationGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new EInvoicingPendingTransactionsNotificationGroupRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public EInvoicingPendingTransactionsNotificationGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, EInvoicingPendingTransactionsNotificationGroup defaultValue)
			: base(new EInvoicingPendingTransactionsNotificationGroupRegistryItemImpl(name, category, caption, hint, storage, option, defaultValue))
		{
		}

		public class EInvoicingPendingTransactionsNotificationGroupRegistryItemImpl : RegistryItemImpl
		{
			readonly EInvoicingPendingTransactionsNotificationGroup defaultValue;

			public EInvoicingPendingTransactionsNotificationGroupRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new EInvoicingPendingTransactionsNotificationGroupRegistryDataType(), storage, option)
			{
			}

			public EInvoicingPendingTransactionsNotificationGroupRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, EInvoicingPendingTransactionsNotificationGroup defaultValue)
				: this(name, category, caption, hint, storage, option)
			{
				this.defaultValue = defaultValue;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
				=> defaultValue ?? new EInvoicingPendingTransactionsNotificationGroup();
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.EInvoicingPendingTransactionsNotificationGroupRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class EInvoicingPendingTransactionsNotificationGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EInvoicingPendingTransactionsNotificationGroup>
	{
	}
}
