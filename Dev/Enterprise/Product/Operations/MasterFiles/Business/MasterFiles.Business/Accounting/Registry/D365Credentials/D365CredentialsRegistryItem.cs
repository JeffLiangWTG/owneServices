using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class D365CredentialsRegistryItem : StronglyTypedRegistryItem<D365Credentials>
	{
		public D365CredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new D365CredentialsRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public class D365CredentialsRegistryItemImpl : RegistryItemImpl
		{
			public D365CredentialsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new D365CredentialsRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
				=> new D365Credentials();
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.D365CredentialsRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class D365CredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<D365Credentials>
	{
		public D365CredentialsRegistryDataType()
		{
		}
	}
}
