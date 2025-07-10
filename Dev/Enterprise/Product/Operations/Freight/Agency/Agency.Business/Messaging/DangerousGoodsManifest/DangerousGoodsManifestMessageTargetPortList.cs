using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestMessageTargetPortList : ReadOnlyCodeDescriptionPairList
	{
		public DangerousGoodsManifestMessageTargetPortList(JobVoyage voyage)
		{
			this.voyage = Argument.NotNull(voyage, "voyage");
		}
		readonly JobVoyage voyage;

		public void Load()
		{
			LoadCore();
		}

		public new PortMessageTargetPort this[int index]
		{
			get { return (PortMessageTargetPort)base[index]; }
		}

		public PortMessageTargetPort this[string code]
		{
			get
			{
				int index = IndexOfCode(code);
				return index < 0 ? null : this[index];
			}
		}

		protected void LoadCore()
		{
			Elements.Clear();

			var lookup = new SortedDictionary<string, PortDirections>();

			var orderedOrigins = voyage.Origins.OfType<VoyageOrigin>().OrderBy(x => x.JA_E_DEP);
			foreach (var origin in orderedOrigins.Skip(1))
			{
				if (ShouldIncludePort(origin.JA_RL_NKPortOfLoading))
				{
					lookup[origin.JA_RL_NKPortOfLoading] = PortDirections.LoadOrTransit;
				}
			}

			var orderedDestinations = voyage.Destinations.OfType<VoyageDestination>().OrderByDescending(x => x.JB_E_ARV);
			foreach (var destination in orderedDestinations.Skip(1))
			{
				if (ShouldIncludePort(destination.JB_RL_NKPortOfDischarge))
				{
					lookup.TryGetValue(destination.JB_RL_NKPortOfDischarge, out var existing);
					lookup[destination.JB_RL_NKPortOfDischarge] = existing | PortDirections.DischargeOrTransit;
				}
			}

			if (orderedOrigins.Any() && ShouldIncludePort(orderedOrigins.First().JA_RL_NKPortOfLoading))
			{
				lookup[orderedOrigins.First().JA_RL_NKPortOfLoading] = PortDirections.Load;
			}

			if (orderedDestinations.Any() && ShouldIncludePort(orderedDestinations.First().JB_RL_NKPortOfDischarge))
			{
				lookup[orderedDestinations.First().JB_RL_NKPortOfDischarge] = PortDirections.Discharge;
			}

			foreach (var pair in lookup)
			{
				Elements.Add(new PortMessageTargetPort(pair.Key, directions: pair.Value));
			}
		}

		bool ShouldIncludePort(ZString port)
		{
			var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration(port);

			return port.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode
				&& portConfig != null && portConfig.Enabled;
		}
	}
}
