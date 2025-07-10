using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Module.DangerousGoodsManifest
{
	public static class DangerousGoodsManifestHelper
	{
		public static ZBool IsVoyageContainsEnabledPorts(JobVoyage voyage)
		{
			if (voyage == null)
			{
				return false;
			}

			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return voyage.Origins.OfType<VoyageOrigin>().Any(x => x.JA_RL_NKPortOfLoading.Left(2) == currentCountry && IsPortEnabled(x.JA_RL_NKPortOfLoading))
				|| voyage.Destinations.OfType<VoyageDestination>().Any(x => x.JB_RL_NKPortOfDischarge.Left(2) == currentCountry && IsPortEnabled(x.JB_RL_NKPortOfDischarge));
		}

		public static ZBool IsPortEnabled(ZString port)
		{
			var portConfigFound = AgencyRegistry.Instance.DangerousGoodsManifestPorts.Value.OfType<DangerousGoodsManifestPort>().Any(x => x.Port == port && x.Enabled);
			if (!portConfigFound)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.DangerousGoodsManifestPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				var systemPortConfigs = (DangerousGoodsManifestPortCollection)systemLevelRetriever.GetCurrentValue().Value;

				portConfigFound = systemPortConfigs.OfType<DangerousGoodsManifestPort>().Any(x => x.Port == port && x.Enabled);
			}

			return portConfigFound;
		}
	}
}
