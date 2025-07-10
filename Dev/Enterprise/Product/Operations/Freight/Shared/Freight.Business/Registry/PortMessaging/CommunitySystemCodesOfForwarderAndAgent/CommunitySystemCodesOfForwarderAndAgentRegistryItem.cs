using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public class CommunitySystemCodesOfForwarderAndAgentRegistryItem : StronglyTypedRegistryItem<CommunitySystemCodesOfForwarderAndAgentCollection>
	{
		public CommunitySystemCodesOfForwarderAndAgentRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CommunitySystemCodesOfForwarderAndAgentCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CommunitySystemCodesOfForwarderAndAgentRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.GUI.CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor, Enterprise.Freight.GUI")]
	public class CommunitySystemCodesOfForwarderAndAgentRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CommunitySystemCodesOfForwarderAndAgentCollection>
	{
		public CommunitySystemCodesOfForwarderAndAgentRegistryDataType()
		{
		}
	}
}
