using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedCountryProvider : IVehicleVisitedCountry
	{
		public VisitedCountryProvider(VisitedPort port, ZDateTime effectiveDate, ZString transportMode, ZString manifestType)
		{
			this.port = port;
			this.effectiveDate = effectiveDate;
			this.transportMode = transportMode;
			this.manifestType = manifestType;
		}

		readonly VisitedPort port;
		readonly ZDateTime effectiveDate;
		readonly ZString transportMode;
		readonly ZString manifestType;

		public ZString PortLocationName
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!port.CY_Code.IsEmpty)
				{
					returnValue = port.CY_Code;
				}
				else if (!port.CY_Data.IsEmpty)
				{
					returnValue = port.CY_Data;

					if (transportMode == Core.Constants.TransportModes.Air)
					{
						returnValue = port.PortOfUNLOCO?.RL_IATA ?? ZString.Empty;
					}
				}

				return returnValue;
			}
		}
		public ZString CountryCode
		{
			get
			{
				ZString returnValue = ZString.Empty;
				ZString locationValue = ZString.Empty;
				if (!port.CY_Code.IsEmpty)
				{
					locationValue = port.CY_Code;
				}
				else if (!port.CY_Data.IsEmpty)
				{
					locationValue = port.CY_Data;
				}

				if (!locationValue.IsEmpty)
				{
					returnValue = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(port.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, locationValue.SubstringSafe(0, 2), effectiveDate);
				}
				return returnValue;
			}
		}

		public ZDateTime MovementDateTime
		{
			get
			{
				return ((transportMode == Core.Constants.TransportModes.Sea || transportMode == Core.Constants.TransportModes.Air) && TRManifestMessageHelper.IsForbidden(nameof(MovementDateTime), manifestType)) ? ZDateTime.Empty : port.CY_Date;
			}
		}
	}
}
