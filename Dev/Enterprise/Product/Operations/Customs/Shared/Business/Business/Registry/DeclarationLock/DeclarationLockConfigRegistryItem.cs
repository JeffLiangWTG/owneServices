using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class DeclarationLockConfigRegistryItem : StronglyTypedRegistryItem<DeclarationLockConfigCollection>
	{
		public DeclarationLockConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new DeclarationLockConfigRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}
	}
}
