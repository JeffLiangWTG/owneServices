using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class TransportTypeList : CodeDescriptionPairList
	{
		public TransportTypeList()
		{
			AddPair(Codes.Air, Descriptions.Air);
			AddPair(Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
			AddPair(Codes.InlandWaterwayTransport, Descriptions.InlandWaterwayTransport);
			AddPair(Codes.OwnPropulsion, Descriptions.OwnPropulsion);
			AddPair(Codes.Mail, Descriptions.Mail);
			AddPair(Codes.Rail, Descriptions.Rail);
			AddPair(Codes.Road, Descriptions.Road);
			AddPair(Codes.Sea, Descriptions.Sea);
		}

		public static class Codes
		{
			public const string Air = Constants.TransportModes.Air;
			public const string Sea = Constants.TransportModes.Sea;
			public const string Mail = Constants.TransportModes.Mail;
			public const string Road = Constants.TransportModes.Road;
			public const string Rail = Constants.TransportModes.Rail;
			public const string FixedTransportInstallations = Constants.TransportModes.FixedTransportInstallations;
			public const string InlandWaterwayTransport = Constants.TransportModes.InlandWaterwayTransport;
			public const string OwnPropulsion = Constants.TransportModes.OwnPropulsion;
		}

		public static class Descriptions
		{
			public static string Air => Constants.TransportModeDescriptions.Air;
			public static string Sea => Constants.TransportModeDescriptions.Sea;
			public static string Mail => Constants.TransportModeDescriptions.Mail;
			public static string Road => Constants.TransportModeDescriptions.Road;
			public static string Rail => Constants.TransportModeDescriptions.Rail;
			public static string FixedTransportInstallations => Constants.TransportModeDescriptions.FixedTransportInstallations;
			public static string InlandWaterwayTransport => Constants.TransportModeDescriptions.InlandWaterwayTransport;
			public static string OwnPropulsion => Constants.TransportModeDescriptions.OwnPropulsion;
		}
	}
}
