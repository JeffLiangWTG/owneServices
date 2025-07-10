
namespace Enterprise.Freight.Agency.Business
{
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Freight.Business;

	public class PortAuthorityMessageTargetPortList : PortMessageTargetPortList
	{
		public PortAuthorityMessageTargetPortList(JobVoyage voyage, PortAuthoritySettingCollection settings)
			: base(voyage)
		{
			this.settings = settings;
		}

		protected override bool ShouldIncludePort(ZString port)
		{
			return settings?.Cast<PortAuthoritySetting>().Any(s => s.Port == port && s.Status != PortAuthoritySettingStatus.Codes.Disabled) ?? true;
		}

		readonly PortAuthoritySettingCollection settings;
	}
}
