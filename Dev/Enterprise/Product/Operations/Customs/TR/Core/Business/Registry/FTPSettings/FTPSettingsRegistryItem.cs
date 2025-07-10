using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	public class FTPSettingsRegistryItem : StronglyTypedRegistryItem<FTPSettings>
	{
		public FTPSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new FTPSettingsRegistryItemDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TR.GUI.FTPSettingsRegistryItemEditor, Enterprise.Customs.TR.GUI")]
	public class FTPSettingsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<FTPSettings>
	{
	}
}
