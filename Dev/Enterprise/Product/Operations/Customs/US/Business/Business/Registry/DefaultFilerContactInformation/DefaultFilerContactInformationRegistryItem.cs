using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class DefaultFilerContactInformationRegistryItem : StronglyTypedRegistryItem<DefaultFilerContactInformation>
	{
		public DefaultFilerContactInformationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultFilerContactInformationRegistryDataType(), storage, options))
		{
		}
	}
}
