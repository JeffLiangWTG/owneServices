using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EPaymentReasonRegistryItem : StronglyTypedRegistryItem<EPaymentReasonCollection>
	{
		public EPaymentReasonRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new EPaymentReasonRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class EPaymentReasonRegistryItemImpl : RegistryItemImpl
		{
			public EPaymentReasonRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new EPaymentReasonRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var configCollection = new EPaymentReasonCollection();
				configCollection.PopulatePaymentReasonsForAllProviders();
				return configCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.EPaymentReasonsRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class EPaymentReasonRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EPaymentReasonCollection>
	{
		public EPaymentReasonRegistryDataType()
		{
		}
	}
}
