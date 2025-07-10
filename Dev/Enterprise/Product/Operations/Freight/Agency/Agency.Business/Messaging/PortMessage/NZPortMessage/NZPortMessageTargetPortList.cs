using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class NZPortMessageTargetPortList : PortMessageTargetPortList
	{
		public NZPortMessageTargetPortList(JobVoyage voyage)
			: base(voyage)
		{
		}

		protected override bool ShouldIncludePort(ZString port)
		{
			var portConfig = PortManifestRegistryHelper.RetrievePortConfiguration(port);

			return port.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode
				&& portConfig != null && portConfig.Enabled;
		}
	}
}
