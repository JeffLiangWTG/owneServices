using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultEPaymentReferenceRegistryItem : StronglyTypedRegistryItem<DefaultEPaymentReferenceCollection>
	{
		public DefaultEPaymentReferenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new DefaultEPaymentReferenceRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		internal class DefaultEPaymentReferenceRegistryItemImpl : RegistryItemImpl
		{
			public DefaultEPaymentReferenceRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new DefaultEPaymentReferenceRegistryDataType(), storage, options)
			{
			}

			/*
			 * This function overrides the GetDefaultValueCore() function of the RegistryItemImpl class.
			 * This creates a new object for DefaultEPaymentReferenceCollection and uses it to call PopulateDefaultPaymentReferenceForAllProviders().
			 * And returns the object.
			 * */
			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultReference = new DefaultEPaymentReferenceCollection(new FallbackLevel(companyPK, branchPK, departmentPK));
				defaultReference.PopulateDefaultPaymentReferenceForAllProviders();
				return defaultReference;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.DefaultEPaymentReferenceRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class DefaultEPaymentReferenceRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultEPaymentReferenceCollection>
	{
		public DefaultEPaymentReferenceRegistryDataType()
		{
		}
	}
}
