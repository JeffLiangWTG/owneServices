using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class UCMEDIMessageTestTypeRegistryItem : StronglyTypedRegistryItem<UCMEDIMessageTestTypeCollection>
	{
		public UCMEDIMessageTestTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new UCMEDIMessageTestTypeRegistryItemDataType(), RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers))
		{
		}
	}
}
