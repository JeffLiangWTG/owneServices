using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

public class PLDefaultCommunicationChannelNCTSP5RegistryItem : StronglyTypedRegistryItem<PLDefaultCommunicationChannelNCTSP5>
{
	public PLDefaultCommunicationChannelNCTSP5RegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PLDefaultCommunicationChannelNCTSP5 defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new PLDefaultCommunicationChannelNCTSP5DataType(), storage, options, defaultValue))
	{
	}
}

[RegistryEditor("Enterprise.Customs.PL.GUI.Registry.PLDefaultCommunicationChannelNCTSP5RegistryItemEditor, Enterprise.Customs.PL.GUI")]
public class PLDefaultCommunicationChannelNCTSP5DataType : NonPersistentBusinessObjectRegistryDataType<PLDefaultCommunicationChannelNCTSP5>
{
}
