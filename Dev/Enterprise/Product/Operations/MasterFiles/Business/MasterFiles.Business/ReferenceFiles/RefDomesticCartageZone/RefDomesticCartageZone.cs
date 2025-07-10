using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefDomesticCartageZone : AutoRefDomesticCartageZone
	{
		public RefDomesticCartageZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static RefDomesticCartageZone GetZone(BusinessObjectFactory factory, ZString postCode, ZString unlocoCode)
		{
			return GetZone(factory, postCode, unlocoCode, ZString.Empty);
		}

		public static RefDomesticCartageZone GetZone(BusinessObjectFactory factory, ZString postCode, ZString unlocoCode, ZString city)
		{
			// Canadian postcodes are 6 digits but the last digit is a unit number and should be ignored.
			if (postCode.Length > 5 && unlocoCode.StartsWith(Core.Constants.CountryCodes.Canada))
			{
				postCode = postCode.Substring(0, 5);
			}

			var port = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, unlocoCode));
			if (port != null)
			{
				var query = new ZQuery(RefDomesticCartageZoneSchema.F1_CityTownPostCode, postCode);
				query.AddToFilter(RefDomesticCartageZoneSchema.F1_PortCode, port.RL_IATA);

				var zones = factory.Load<RefDomesticCartageZone>(query);
				if (zones.Length > 0)
				{
					if (!city.IsEmpty && zones.Length > 1)
					{
						foreach (var zone in zones)
						{
							if (zone.F1_CityTown.EqualsIgnoringCase(city))
							{
								return zone;
							}
						}
					}

					return zones[0];
				}
			}

			return null;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var hyperlinkName = Res.GetString("94C1885B-4676-4E4B-B107-5C4D330F7B8C", "Port Transport Zones (ACI)");
				if (!F1_AirportCity.IsEmpty)
				{
					hyperlinkName += (NoResString)" - Airport: " + F1_AirportCity;
				}

				if (!F1_Zone.IsEmpty)
				{
					hyperlinkName += (NoResString)" - Zone: " + F1_Zone;
				}

				return hyperlinkName;
			}
		}
	}
}
