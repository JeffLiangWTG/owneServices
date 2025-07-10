using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	[RegistryEditor("Enterprise.Freight.Agency.GUI.PortAuthoritySettingsRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
	internal sealed class PortAuthoritySettingsDataType : NonPersistentBusinessObjectRegistryDataType<PortAuthoritySettings>
	{
		public PortAuthoritySettingsDataType()
			: base(new PortAuthoritySettings()) { }
	}
}
