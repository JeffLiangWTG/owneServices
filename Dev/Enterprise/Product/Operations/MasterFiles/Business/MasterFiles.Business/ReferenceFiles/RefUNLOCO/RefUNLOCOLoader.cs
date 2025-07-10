using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class RefUNLOCOLoader : IRefUNLOCOLoader
	{
		internal RefUNLOCO LoadFromIATA(BusinessObjectFactory factory, ZString iataCode)
		{
			var iataFilter = new ZQuery(RefUNLOCOSchema.RL_IATA, iataCode);
			iataFilter.OrderBy = RefUNLOCOSchema.Constants.RL_IsActive + OrderByClause.Descending;
			var matchingUNLOCOs = factory.Load<RefUNLOCO>(iataFilter);
			if (matchingUNLOCOs.Length > 1)
			{
				foreach (var unloco in matchingUNLOCOs)
				{
					if (unloco.RL_Code.EndsWith(iataCode))
					{
						return unloco;
					}
				}

				return matchingUNLOCOs[0];
			}
			else if (matchingUNLOCOs.Length == 1)
			{
				return matchingUNLOCOs[0];
			}

			return null;
		}

		internal RefUNLOCO LoadFromIATARegionCode(BusinessObjectFactory factory, ZString iataCity)
		{
			var cityFilter = new ZQuery(RefUNLOCOSchema.RL_IATARegionCode, iataCity);
			cityFilter.OrderBy = RefUNLOCOSchema.Constants.RL_IsActive + OrderByClause.Descending;
			var matchingUNLOCOs = factory.Load<RefUNLOCO>(cityFilter);
			if (matchingUNLOCOs.Length > 1)
			{
				foreach (var unloco in matchingUNLOCOs)
				{
					if (unloco.RL_Code.EndsWith(iataCity, StringComparison.OrdinalIgnoreCase))
					{
						return unloco;
					}
				}

				return matchingUNLOCOs[0];
			}
			else if (matchingUNLOCOs.Length == 1)
			{
				return matchingUNLOCOs[0];
			}
			return null;
		}

		internal RefUNLOCO GetPortFromNameAndCountryCode(BusinessObjectFactory factory, ZString portName, ZString countryCode)
		{
			var portFilter = new ZQuery(RefUNLOCOSchema.RL_PortName, portName);
			var portsWithMatchingNames = factory.Load<RefUNLOCO>(portFilter);

			if (portsWithMatchingNames.Length > 0)
			{
				string trimedUpperCountryCode = countryCode.Trim().ToUpper();

				for (int i = 0; i < portsWithMatchingNames.Length; i++)
				{
					if (portsWithMatchingNames[i].Country != null)
					{
						string matchingPortCountryCode = portsWithMatchingNames[i].RL_RN_NKCountryCode.Trim().ToUpper();
						if (matchingPortCountryCode == trimedUpperCountryCode)
						{
							return portsWithMatchingNames[i];
						}
					}
				}
			}

			return null;
		}

		internal RefUNLOCO GetPortFromNameAndCountryName(BusinessObjectFactory factory, ZString portName, ZString countryName)
		{
			var portFilter = new ZQuery(RefUNLOCOSchema.RL_PortName, portName);
			var portsWithMatchingNames = factory.Load<RefUNLOCO>(portFilter);

			if (portsWithMatchingNames.Length > 0)
			{
				string trimmedUpperCountryName = countryName.Trim().ToUpper();

				for (int i = 0; i < portsWithMatchingNames.Length; i++)
				{
					if (portsWithMatchingNames[i].Country != null)
					{
						string matchingPortCountryName = portsWithMatchingNames[i].Country.Description.Trim().ToUpper();
						if (matchingPortCountryName == trimmedUpperCountryName)
						{
							return portsWithMatchingNames[i];
						}
					}
				}
			}

			return null;
		}

		internal RefUNLOCO LoadFromLocalMap(BusinessObjectFactory factory, ZString localPortCode, ZString countryCode, ZString systemUsage)
		{
			if (!localPortCode.IsEmpty && !countryCode.IsEmpty)
			{
				var country = RefCountry.LoadFromCountryCode(factory, countryCode);

				if (country != null)
				{
					var mapFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPortCode);
					mapFilter.AddToFilter(RefLocoMapSchema.RY_RN, country.PK);
					mapFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);

					var locoMap = factory.LoadTop1<RefLocoMap>(mapFilter);
					if (locoMap != null)
					{
						return factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, locoMap.RY_RL_NKLocoPort);
					}
				}
			}

			return null;
		}

		internal RefUNLOCO LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, OrgHeader organisation)
		{
			var unlocoFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, organisation.PK);
			unlocoFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Port);
			unlocoFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);

			var orgPatternMatchOverrides = factory.Load<OrgPatternMatchOverride>(unlocoFilter);
			if (orgPatternMatchOverrides.Length == 1)
			{
				var orgPatternMatch = orgPatternMatchOverrides[0];
				return factory.Load<RefUNLOCO>(orgPatternMatch.OO_LocalGuid);
			}

			return null;
		}

		#region IRefUNLOCOLoader Members

		IRefUNLOCO IRefUNLOCOLoader.LoadFromIATA(BusinessObjectFactory factory, ZString iataCode)
		{
			return LoadFromIATA(factory, iataCode);
		}

		IRefUNLOCO IRefUNLOCOLoader.LoadFromIATARegionCode(BusinessObjectFactory factory, ZString iataCity)
		{
			return LoadFromIATARegionCode(factory, iataCity);
		}

		IRefUNLOCO IRefUNLOCOLoader.GetPortFromNameAndCountryCode(BusinessObjectFactory factory, ZString portName, ZString countryCode)
		{
			return GetPortFromNameAndCountryCode(factory, portName, countryCode);
		}

		IRefUNLOCO IRefUNLOCOLoader.GetPortFromNameAndCountryName(BusinessObjectFactory factory, ZString portName, ZString countryName)
		{
			return GetPortFromNameAndCountryName(factory, portName, countryName);
		}

		IRefUNLOCO IRefUNLOCOLoader.LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, IOrgHeader organisation)
		{
			return LoadFromForeignCode(factory, foreignCode, (OrgHeader)organisation);
		}

		IRefUNLOCO IRefUNLOCOLoader.LoadFromLocalMap(BusinessObjectFactory factory, ZString localPortCode, ZString countryCode, ZString systemUsage)
		{
			return LoadFromLocalMap(factory, localPortCode, countryCode, systemUsage);
		}

		#endregion
	}
}
