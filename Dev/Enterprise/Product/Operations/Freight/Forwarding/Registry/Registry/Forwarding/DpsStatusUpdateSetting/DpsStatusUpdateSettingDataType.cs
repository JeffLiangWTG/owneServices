using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Registry
{
	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.DpsStatusUpdateSettingRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	public class DpsStatusUpdateSettingDataType : NonPersistentBusinessObjectRegistryDataType<DpsStatusUpdateSetting>
	{
		public DpsStatusUpdateSettingDataType(PhaseSecurityRegistryItem phaseRegistryItem)
		{
			PhaseRegistryItem = phaseRegistryItem;
		}

		public PhaseSecurityRegistryItem PhaseRegistryItem { get; set; }

		protected override DpsStatusUpdateSetting DeserialiseCore(byte[] value)
		{
			var setting = base.DeserialiseCore(value);
			setting.PhaseRegistryItem = PhaseRegistryItem;
			return setting;
		}
	}
}
