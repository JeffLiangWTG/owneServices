using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.CMMFlagsRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	public class CMMFlagsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CMMFlags>
	{
		public CMMFlagsRegistryDataType()
			: base(new CMMFlags()) { }

		public CMMFlagsRegistryDataType(CMMFlags defaultValue)
			: base(defaultValue) { }
	}
}


