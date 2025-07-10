using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class OrganisationRTUSRegistryItem : StronglyTypedRegistryItem<OrganisationRTUSCollection>
	{
		public OrganisationRTUSRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrganisationRTUSRegistryDataType(), storage, options, new OrganisationRTUSCollection()))
		{
		}
	}

	[RegistryEditor("Enterprise.TransportCommon.GUI.Registry.OrganisationRTUSEditor, Enterprise.TransportCommon.GUI")]
	public class OrganisationRTUSRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OrganisationRTUSCollection>
	{
	}
}
