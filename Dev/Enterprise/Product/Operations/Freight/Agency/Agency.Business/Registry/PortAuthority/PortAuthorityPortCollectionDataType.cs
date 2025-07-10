using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.PortAuthorityPortRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	internal sealed class PortAuthorityPortCollectionDataType : NonPersistentBusinessObjectRegistryDataType<PortAuthorityPortCollection>
	{
		public PortAuthorityPortCollectionDataType(PortAuthorityPortCollection defaultValue)
			: base(defaultValue) { }
	}
}


