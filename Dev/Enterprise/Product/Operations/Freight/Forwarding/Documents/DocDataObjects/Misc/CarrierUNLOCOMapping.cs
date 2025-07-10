using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CarrierUNLOCOMapping
	{
		public CarrierUNLOCOMapping(BusinessObjectFactory factory, ZGuid carrierPK)
		{
			this.factory = factory;
			this.carrierPK = carrierPK;
		}

		readonly BusinessObjectFactory factory;
		readonly ZGuid carrierPK;

		Dictionary<string, string> LocalCodeToForeignMapping { get; set; }
		Dictionary<string, string> ForeignCodeToLocalMapping { get; set; }

		public string GetLocalCode(string foreginCode, bool explicitAutoComplete = false)
		{
			if (ForeignCodeToLocalMapping == null)
			{
				LoadUnlocoMappings();
			}

			if (ForeignCodeToLocalMapping.TryGetValue(foreginCode, out var localCode))
			{
				return localCode;
			}
			else if (explicitAutoComplete && ForeignCodeToLocalMapping.Any(m => m.Key.StartsWith(foreginCode)))
			{
				return ForeignCodeToLocalMapping.First(m => m.Key.StartsWith(foreginCode)).Value;
			}

			return foreginCode;
		}

		public string GetForeignCode(string localCode, bool explicitAutoComplete = false)
		{
			if (LocalCodeToForeignMapping == null)
			{
				LoadUnlocoMappings();
			}

			if (LocalCodeToForeignMapping.TryGetValue(localCode, out var foreign))
			{
				return foreign;
			}
			else if (explicitAutoComplete && LocalCodeToForeignMapping.Any(m => m.Key.StartsWith(localCode)))
			{
				return LocalCodeToForeignMapping.First(m => m.Key.StartsWith(localCode)).Value;
			}

			return localCode;
		}

		void LoadUnlocoMappings()
		{
			var orgFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, carrierPK);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Port);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Context, Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage);

			var mappings = factory.Load<OrgPatternMatchOverride>(orgFilter);

			var localCodeToForeignMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var foreignCodeToLocalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var localUnlocoMap = LoadUnlocos(mappings);

			foreach (var mapping in mappings)
			{
				var localCode = mapping.OO_LocalCode;

				if (localCode.IsEmpty && !localUnlocoMap.TryGetValue(mapping.OO_LocalGuid, out localCode))
				{
					continue;
				}

				localCodeToForeignMapping[localCode] = mapping.OO_ForeignCode;
				foreignCodeToLocalMapping[mapping.OO_ForeignCode] = localCode;
			}

			LocalCodeToForeignMapping = localCodeToForeignMapping;
			ForeignCodeToLocalMapping = foreignCodeToLocalMapping;
		}

		Dictionary<ZGuid, ZString> LoadUnlocos(OrgPatternMatchOverride[] mappings)
		{
			var pks = mappings
				.Select(m => m.OO_LocalGuid)
				.ToArray();

			var query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.PK, pks);

			var map = new Dictionary<ZGuid, ZString>();
			var unlocos = factory.Load<RefUNLOCO>(query);

			foreach (var unloco in unlocos)
			{
				map[unloco.PK] = unloco.RL_Code;
			}

			return map;
		}
	}
}
