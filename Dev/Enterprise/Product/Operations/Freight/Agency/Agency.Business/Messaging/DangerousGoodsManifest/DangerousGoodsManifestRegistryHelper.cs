using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public static class DangerousGoodsManifestRegistryHelper
	{
		public static DangerousGoodsManifestPort RetrievePortConfiguration(ZString port, ZGuid principalPK)
		{
			var portConfig = SearchPortConfiguration(port, principalPK, AgencyRegistry.Instance.DangerousGoodsManifestPorts.Value);
			if (portConfig == null || !portConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.DangerousGoodsManifestPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				portConfig = SearchPortConfiguration(port, principalPK, (DangerousGoodsManifestPortCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			return portConfig;
		}

		static DangerousGoodsManifestPort SearchPortConfiguration(ZString port, ZGuid principalPK, DangerousGoodsManifestPortCollection ports)
		{
			var portConfig = ports.OfType<DangerousGoodsManifestPort>().FirstOrDefault(x => x.Port == port.ToString() && x.PrincipalPK == principalPK);
			if (portConfig == null && !principalPK.IsEmpty)
			{
				portConfig = ports.OfType<DangerousGoodsManifestPort>().FirstOrDefault(x => x.Port == port.ToString() && x.PrincipalPK == ZGuid.Empty);
			}

			return portConfig;
		}

		public static DangerousGoodsManifestPort RetrievePortConfiguration(ZString port)
		{
			var portConfig = SearchPortConfiguration(port, AgencyRegistry.Instance.DangerousGoodsManifestPorts.Value);
			if (portConfig == null || !portConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.DangerousGoodsManifestPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				portConfig = SearchPortConfiguration(port, (DangerousGoodsManifestPortCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			return portConfig;
		}

		static DangerousGoodsManifestPort SearchPortConfiguration(ZString port, DangerousGoodsManifestPortCollection ports)
		{
			return ports.OfType<DangerousGoodsManifestPort>().FirstOrDefault(x => x.Port == port.ToString());
		}
	}
}
