using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultEPaymentReasonRegistryItem : StronglyTypedRegistryItem<DefaultEPaymentReasonCollection>
	{
		public DefaultEPaymentReasonRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new DefaultEPaymentReasonRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		internal class DefaultEPaymentReasonRegistryItemImpl : RegistryItemImpl
		{
			public DefaultEPaymentReasonRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new DefaultEPaymentReasonRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultReasons = new DefaultEPaymentReasonCollection(new FallbackLevel(companyPK, branchPK, departmentPK));
				defaultReasons.PopulateDefaultPaymentReasonsForAllProviders();
				return defaultReasons;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.DefaultEPaymentReasonRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class DefaultEPaymentReasonRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultEPaymentReasonCollection>
	{
		public DefaultEPaymentReasonRegistryDataType()
		{
		}
	}
}
