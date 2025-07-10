using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public static class PortManifestRegistryHelper
	{
		public static PortManifestPort RetrievePortConfiguration(ZString port, ZGuid principalPK)
		{
			var portConfig = SearchPortConfiguration(port, principalPK, AgencyRegistry.Instance.LoadAndDischargeManifestPorts.Value);
			if (portConfig == null || !portConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.LoadAndDischargeManifestPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				portConfig = SearchPortConfiguration(port, principalPK, (PortManifestPortCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			return portConfig;
		}

		static PortManifestPort SearchPortConfiguration(ZString port, ZGuid principalPK, PortManifestPortCollection ports)
		{
			var portConfig = ports.OfType<PortManifestPort>().FirstOrDefault(x => x.Port == port.ToString() && x.PrincipalPK == principalPK);
			if (portConfig == null && !principalPK.IsEmpty)
			{
				portConfig = ports.OfType<PortManifestPort>().FirstOrDefault(x => x.Port == port.ToString() && x.PrincipalPK == ZGuid.Empty);
			}

			return portConfig;
		}

		public static PortManifestPort RetrievePortConfiguration(ZString port)
		{
			var portConfig = SearchPortConfiguration(port, AgencyRegistry.Instance.LoadAndDischargeManifestPorts.Value);
			if (portConfig == null || !portConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.LoadAndDischargeManifestPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				portConfig = SearchPortConfiguration(port, (PortManifestPortCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			return portConfig;
		}

		static PortManifestPort SearchPortConfiguration(ZString port, PortManifestPortCollection ports)
		{
			return ports.OfType<PortManifestPort>().FirstOrDefault(x => x.Port == port.ToString());
		}
	}
}
