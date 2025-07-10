using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class BranchDistrictPortRegistryItem : StronglyTypedRegistryItem<BranchDistrictPortCollection>
	{
		public BranchDistrictPortRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, RegistryStorageFlags.Company)
		{
		}

		public BranchDistrictPortRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new BranchDistrictPortCollectionImpl(name, category, caption, hint, storage))
		{
		}

		class BranchDistrictPortCollectionImpl : RegistryItemImpl
		{
			public BranchDistrictPortCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new BranchDistrictPortCollectionRegistryDataType(), storage)
			{
			}
		}
	}
}
