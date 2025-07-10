using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.DangerousGoodsManifestPortRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	internal sealed class DangerousGoodsManifestPortCollectionDataType : NonPersistentBusinessObjectRegistryDataType<DangerousGoodsManifestPortCollection>
	{
		public DangerousGoodsManifestPortCollectionDataType() { }

		public DangerousGoodsManifestPortCollectionDataType(DangerousGoodsManifestPortCollection defaultValue)
			: base(defaultValue) { }
	}
}


