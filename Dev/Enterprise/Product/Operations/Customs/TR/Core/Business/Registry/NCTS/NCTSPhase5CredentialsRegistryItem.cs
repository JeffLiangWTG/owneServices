using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSPhase5CredentialsRegistryItem : StronglyTypedRegistryItem<NCTSPhase5Credentials>
	{
		public NCTSPhase5CredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new NCTSPhase5CredentialsRegistryItemDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TR.GUI.NCTSPhase5CredentialsRegistryItemEditor, Enterprise.Customs.TR.GUI")]
	public class NCTSPhase5CredentialsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<NCTSPhase5Credentials>
	{
	}
}
