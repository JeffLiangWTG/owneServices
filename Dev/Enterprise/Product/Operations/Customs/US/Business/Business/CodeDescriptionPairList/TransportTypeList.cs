using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class TransportTypeList : CodeDescriptionPairList
	{
		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.Auto, Descriptions.Auto);
			AddPair(Codes.BorderWaterBorne, Descriptions.BorderWaterBorne);
			AddPair(Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
			AddPair(Codes.Mail, Descriptions.Mail);
			AddPair(Codes.PassengerHandCarried, Descriptions.PassengerHandCarried);
			AddPair(Codes.Pedestrian, Descriptions.Pedestrian);
			AddPair(Codes.Rail, Descriptions.Rail);
			AddPair(Codes.Road, Descriptions.Road);
			AddPair(Codes.Sea, Descriptions.Sea);
			AddPair(Codes.Truck, Descriptions.Truck);
		}

		public static class Codes
		{
			public const string Air = Constants.TransportModes.Air;
			public const string Auto = "AUT";
			public const string BorderWaterBorne = "BWB";
			public const string FixedTransportInstallations = Constants.TransportModes.FixedTransportInstallations;
			public const string Mail = Constants.TransportModes.Mail;
			public const string PassengerHandCarried = "PHC";
			public const string Pedestrian = "PED";
			public const string Rail = Constants.TransportModes.Rail;
			public const string Road = Constants.TransportModes.Road;
			public const string Sea = Constants.TransportModes.Sea;
			public const string Truck = "TRK";
		}

		public static class Descriptions
		{
			public const string Air = "Air (Non Container, Container) (40, 41)";
			public const string Auto = "Auto (32)";
			public const string BorderWaterBorne = "Border Water-borne(only Mexico and Canada) (12)";
			public const string FixedTransportInstallations = "Fixed Transport Installations(Includes pipeline and powerhouse) (70)";
			public const string Mail = "Mail (50)";
			public const string PassengerHandCarried = "Passenger, hand carried (60)";
			public const string Pedestrian = "Pedestrian (33)";
			public const string Rail = "Rail (Non Container, Container) (20, 21)";
			public const string Road = "Road Other (Includes foot and animal borne) (34)";
			public const string Sea = "Sea (Non Container, Container) (10, 11)";
			public const string Truck = "Truck (Non Container, Container) (30, 31)";
		}

		public static bool IsNonAMSBillType(string code)
		{
			return code == TransportTypeList.Codes.Auto
				|| code == TransportTypeList.Codes.Pedestrian
				|| code == TransportTypeList.Codes.Road
				|| code == TransportTypeList.Codes.Mail
				|| code == TransportTypeList.Codes.PassengerHandCarried
				|| code == TransportTypeList.Codes.FixedTransportInstallations;
		}

		public static bool IsBorderTransportType(string transportType)
		{
			return transportType == Codes.Auto ||
				transportType == Codes.BorderWaterBorne ||
				transportType == Codes.Pedestrian ||
				transportType == Codes.Rail ||
				transportType == Codes.Road ||
				transportType == Codes.Truck;
		}

		public static bool IsMasterBillSCACMandatory(string transportType)
		{
			return transportType == Codes.Air ||
				transportType == Codes.Rail ||
				transportType == Codes.Sea ||
				transportType == Codes.Truck ||
				transportType == Codes.BorderWaterBorne;
		}

		public static bool IsMasterBillRelevant(string transportType)
		{
			return IsMasterBillSCACMandatory(transportType)
				|| transportType == Codes.PassengerHandCarried
				|| transportType == Codes.Mail;
		}

		public static bool IsHouseBillSCACMandatory(string transportType)
		{
			return transportType == Codes.Rail ||
				transportType == Codes.Sea;
		}

		public static bool IsVoyageFlightNoMandatory(string usTransportMode)
		{
			var transportType = ConvertFromTransportCode(usTransportMode);

			return transportType == Codes.Sea
				|| transportType == Codes.Air
				|| transportType == Codes.PassengerHandCarried;
		}

		public static bool IsIssuerSCACNotAllowed(string transportType)
		{
			return transportType == TransportTypeList.Codes.Auto
			|| transportType == TransportTypeList.Codes.Pedestrian
			|| transportType == TransportTypeList.Codes.Road
			|| transportType == TransportTypeList.Codes.PassengerHandCarried;
		}

		public static bool IsPortOfDischargeMandatory(string transportType)
		{
			return transportType == TransportTypeList.Codes.Mail || transportType == TransportTypeList.Codes.PassengerHandCarried || transportType == TransportTypeList.Codes.FixedTransportInstallations;
		}

		public static ZString ConvertFromTransportCode(ZString transportMode)
		{
			ZString result = ZString.Empty;
			switch (transportMode)
			{
				case TransportModeCodes.Codes.AirContainer:
				case TransportModeCodes.Codes.AirNonContainer:
					result = TransportTypeList.Codes.Air;
					break;
				case TransportModeCodes.Codes.Auto:
					result = TransportTypeList.Codes.Auto;
					break;
				case TransportModeCodes.Codes.BorderWaterBorne:
					result = TransportTypeList.Codes.BorderWaterBorne;
					break;
				case TransportModeCodes.Codes.FixedTransportInstallations:
					result = TransportTypeList.Codes.FixedTransportInstallations;
					break;
				case TransportModeCodes.Codes.Mail:
					result = TransportTypeList.Codes.Mail;
					break;
				case TransportModeCodes.Codes.PassengerHandCarried:
					result = TransportTypeList.Codes.PassengerHandCarried;
					break;
				case TransportModeCodes.Codes.Pedestrian:
					result = TransportTypeList.Codes.Pedestrian;
					break;
				case TransportModeCodes.Codes.RailContainer:
				case TransportModeCodes.Codes.RailNonContainer:
					result = TransportTypeList.Codes.Rail;
					break;
				case TransportModeCodes.Codes.RoadOther:
					result = TransportTypeList.Codes.Road;
					break;
				case TransportModeCodes.Codes.TruckContainer:
				case TransportModeCodes.Codes.TruckNonContainer:
					result = TransportTypeList.Codes.Truck;
					break;
				case TransportModeCodes.Codes.VesselContainer:
				case TransportModeCodes.Codes.VesselNonContainer:
					result = TransportTypeList.Codes.Sea;
					break;
				default:
					// do nothing
					break;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString[] ConvertFromTransportMode(ZString transportMode)
		{
			var result = new List<ZString>();
			switch (transportMode)
			{
				case TransportTypeList.Codes.Air:
					result.Add(TransportModeCodes.Codes.AirContainer);
					result.Add(TransportModeCodes.Codes.AirNonContainer);
					break;
				case TransportTypeList.Codes.Auto:
					result.Add(TransportModeCodes.Codes.Auto);
					break;
				case TransportTypeList.Codes.BorderWaterBorne:
					result.Add(TransportModeCodes.Codes.BorderWaterBorne);
					break;
				case TransportTypeList.Codes.FixedTransportInstallations:
					result.Add(TransportModeCodes.Codes.FixedTransportInstallations);
					break;
				case TransportTypeList.Codes.Mail:
					result.Add(TransportModeCodes.Codes.Mail);
					break;
				case TransportTypeList.Codes.PassengerHandCarried:
					result.Add(TransportModeCodes.Codes.PassengerHandCarried);
					break;
				case TransportTypeList.Codes.Pedestrian:
					result.Add(TransportModeCodes.Codes.Pedestrian);
					break;
				case TransportTypeList.Codes.Rail:
					result.Add(TransportModeCodes.Codes.RailContainer);
					result.Add(TransportModeCodes.Codes.RailNonContainer);
					break;
				case TransportTypeList.Codes.Road:
					result.Add(TransportModeCodes.Codes.RoadOther);
					break;
				case TransportTypeList.Codes.Truck:
					result.Add(TransportModeCodes.Codes.TruckContainer);
					result.Add(TransportModeCodes.Codes.TruckNonContainer);
					break;
				case TransportTypeList.Codes.Sea:
					result.Add(TransportModeCodes.Codes.VesselContainer);
					result.Add(TransportModeCodes.Codes.VesselNonContainer);
					break;
				default:
					// do nothing
					break;
			}
			return result.ToArray();
		}
	}
}
