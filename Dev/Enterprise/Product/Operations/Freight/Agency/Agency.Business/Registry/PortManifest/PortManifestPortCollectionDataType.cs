using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.PortManifestPortRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	internal sealed class PortManifestPortCollectionDataType : NonPersistentBusinessObjectRegistryDataType<PortManifestPortCollection>
	{
		public PortManifestPortCollectionDataType() { }

		public PortManifestPortCollectionDataType(PortManifestPortCollection defaultValue)
			: base(defaultValue) { }
	}
}


