using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.ContainerTranshipmentIndicatorRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	public sealed class ContainerTranshipmentIndicatorRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ContainerTranshipmentIndicatorCollection>
	{
		public ContainerTranshipmentIndicatorRegistryDataType(ContainerTranshipmentIndicatorCollection defaultValue)
			: base(defaultValue) { }
	}
}


