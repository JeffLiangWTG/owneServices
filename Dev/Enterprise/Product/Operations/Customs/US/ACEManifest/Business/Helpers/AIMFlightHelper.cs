using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public static class AIMFlightHelper
	{
		public static ZDate CalculateArrivalDate(AsycudaManifestHeader header, FlightDetail flightDetails)
		{
			Argument.NotNull(header, "header");
			Argument.NotNull(flightDetails, "flightDetails");

			if (flightDetails.IsArrival)
			{
				return flightDetails.FlightArrivalDate;
			}
			else
			{
				var estFirst = header.EstDateAtFirstArrival.Date;
				return estFirst.IsEmpty ? header.AMA_E_ARV.Date : estFirst;
			}
		}

		public static ZString CheckCombinedCarrierFlight(ZString combinedCarrierFlight, ZString carrierCode)
		{
			ZString errorString = ZString.Empty;

			if (combinedCarrierFlight.IsEmpty)
			{
				errorString = MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("F80C767B-8F92-4714-AA36-2B66C5CCF78F", "Flight"));
			}
			else if (combinedCarrierFlight.Length < MinimumCarrierFlightLength || combinedCarrierFlight.Length > MaximumCarrierFlightLength)
			{
				errorString = ValidationConstants.FlightNumberIsWrongLength;
			}
			else if (!Regex.IsMatch(combinedCarrierFlight, CompliancePattern, RegexOptions.IgnoreCase) && !CanFlightBePadded(combinedCarrierFlight, carrierCode))
			{
				errorString = ValidationConstants.FlightNumberIsNonCompliant;
			}

			return errorString;
		}

		static bool CanFlightBePadded(ZString combinedCarrierFlight, ZString carrierCode)
		{
			return !GetFlightNumber(combinedCarrierFlight, carrierCode).IsEmpty;
		}

		public static ZString CreatePaddedFlightNumberForMessage(ZString combinedCarrierFlight, ZString carrierCode)
		{
			var result = combinedCarrierFlight;

			var flightNum = GetFlightNumber(combinedCarrierFlight, carrierCode);
			if (!flightNum.IsEmpty)
			{
				var padSize = flightNum.IsNumbersOnlyOrEmpty ? 3 : 4;
				result = carrierCode + flightNum.PadLeft(padSize, '0');
			}

			return result;
		}

		static ZString GetFlightNumber(ZString combinedCarrierFlight, ZString carrierCode)
		{
			var result = ZString.Empty;

			if (!carrierCode.IsEmpty && carrierCode.Length <= 3 && combinedCarrierFlight.StartsWith(carrierCode, System.StringComparison.OrdinalIgnoreCase))
			{
				var flightNum = combinedCarrierFlight.SubstringSafe(carrierCode.Length);
				if (Regex.IsMatch(flightNum, FlightOnlyPattern, RegexOptions.IgnoreCase))
				{
					result = flightNum;
				}
			}

			return result;
		}

		const int MinimumCarrierFlightLength = 3;
		const int MaximumCarrierFlightLength = 8;
		const string CompliancePattern = "^[A-Z0-9]{2,3}[0-9]{3,4}[A-Z]?$";
		const string FlightOnlyPattern = "^[0-9]{1,2}[A-Z]?$|^[0-9]{3,4}[A-Z]?$";
	}
}
