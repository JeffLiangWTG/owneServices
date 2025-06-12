namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1034")]
	public static class Constants
	{
		public static class UniversalEvent
		{
			public static class TargetTypes
			{
				public const string NctsHeader = "NctsHeader";
				public const string CustomsDeclaration = "CustomsDeclaration";
			}

			public static class EventTypes
			{
				public const string FrenchCustomsMessages = "FRM";
				public const string APPLUS = "ISN";
			}

			public static class Statuses
			{
				public const string MessageNotDelivered = "MND";
				public const string MessageDelivered = "MDL";
				public const string MessageRejectedByMareva = "MNA";
				public const string MessageRejectedByCustoms = "MRJ";
				public const string MessageAcknowledged = "MAK";
			}
		}

		public static class Extensions
		{
			public const string Xml = ".xml";
		}

		public static class Directions
		{
			public const string FluxImpValue = "IMP";
			public const string FluxExpValue = "EXP";
		}

		public static class MessageFlows
		{
			public const string FluxImpType = "DSI";
			public const string FluxExpType = "DSE";
			public const string FluxDCG = "DCG";
			public const string FluxCOD = "COD";
			public const string FluxDeltaTDepart = "DTD";
			public const string FluxDeltaTArrivee = "DTA";
			public const string FluxNSTI = "NSTI";
			public const string FluxECS = "ECS";
			public const string FluxCIN = "CIN";
			public const string FluxPNTS = "PNT";
		}

		public static class MessageTypes
		{
			public const string Delta = "DELTA";
			public const string APPLUS = "APPLUS";
			public const string CIN = "CIN";
			public const string EDIFACT = "EDIFACT";
			public const string DeltaAck = "ACK";
			public const string DeltaNack = "NACK";
			public const string DeltaReceiveErr1 = "ERR1";
			public const string DeltaReponse = "DELTA";
			public const string COD = "COD";

			public static class DeltaIE
			{
				public const string IE404 = "IE404";
				public const string IE410 = "IE410";
				public const string IE413 = "IE413";
				public const string IE414 = "IE414";
				public const string IE415 = "IE415";
				public const string IE426 = "IE426";
				public const string IE428 = "IE428";
				public const string IE429 = "IE429";
				public const string IE431 = "IE431";
				public const string IE432 = "IE432";
				public const string IE433 = "IE433";
				public const string IE451 = "IE451";
				public const string IE456 = "IE456";
				public const string IE457 = "IE457";
				public const string IE460 = "IE460";
				public const string IE462 = "IE462";
				public const string IE917 = "IE917";
				public const string FRA101 = "FRA101";
				public const string FRA102 = "FRA102";
				public const string FRA103 = "FRA103";
				public const string FRA112 = "FRA112";
			}

			public static class DeltaT
			{
				public const string IE007 = "IE007";
				public const string IE013 = "IE013";
				public const string IE014 = "IE014";
				public const string IE015 = "IE015";
				public const string IEF15 = "IEF15";
				public const string IE034 = "IE034";
				public const string IE044 = "IE044";
				public const string IE141 = "IE141";
				public const string IE170 = "IE170";
			}

			public static class DeltaT_Phase5
			{
				public const string IE007 = "CC007C";
				public const string IE013 = "CC013C";
				public const string IE014 = "CC014C";
				public const string IE015 = "CC015C";
				public const string IE034 = "CC034C";
				public const string IE044 = "CC044C";
				public const string IE141 = "CC141C";
				public const string IE170 = "CC170C";
			}

			public static class PNTS
			{
				public const string IETS007 = "IETS007";
				public const string IETS015 = "IETS015";
				public const string IETS016 = "IETS016";
				public const string IETS028 = "IETS028";
				public const string IETS029 = "IETS029";
				public const string IETS030 = "IETS030";
				public const string IETS115 = "IETS115";
				public const string IETS410 = "IETS410";
				public const string IETS413 = "IETS413";
				public const string IETS414 = "IETS414";
				public const string IETS460 = "IETS460";
				public const string IETS906 = "IETS906";
				public const string IETS928 = "IETS928";
			}

			public static class ECS
			{
				public const string IE507 = "IE507";
				public const string IE618 = "IE618";
			}
		}

		public static class MessageSchemas
		{
			public const string DeltaCImportDeclarationSchema = "MessageCDecImp";
			public const string DeltaCExportDeclarationSchema = "MessageCDecExp";
			public const string DeltaDImportDeclarationSchema = "MessageDecImp";
			public const string DeltaDExportDeclarationSchema = "MessageDecExp";
			public const string DcgSchema = "MessageDcg";
			public const string CodSchema = "MessageCOD";
			public const string IE007Schema = "IE007";
			public const string IE013Schema = "IE013";
			public const string IE014Schema = "IE014";
			public const string IE015Schema = "IE015";
			public const string IEF15Schema = "IEF15";
			public const string IE044Schema = "IE044";
			public const string IE141Schema = "IE141";
			public const string IE507Schema = "MessageIE507";
			public const string IE618Schema = "MessageIE618";
			public const string IE413Schema = "IE413";
			public const string IE414Schema = "IE414";
			public const string IE415Schema = "IE415";
			public const string IE432Schema = "IE432";
			public const string IE433Schema = "IE433";
			public const string CIN745Schema = "745";
			public const string CIN750Schema = "750";
			public const string CIN755Schema = "755";
			public const string APPLUSSchema = "APPLUS";
			public const string IETS007Schema = "IETS007";
			public const string IETS015Schema = "IETS015";
			public const string IETS115Schema = "IETS115";
			public const string IETS413Schema = "IETS413";
			public const string IETS414Schema = "IETS414";
			public const string CC007CSchema = "CC007C";
			public const string CC013CSchema = "CC013C";
			public const string CC014CSchema = "CC014C";
			public const string CC015CSchema = "CC015C";
			public const string CC034CSchema = "CC034C";
			public const string CC044CSchema = "CC044C";
			public const string CC141CSchema = "CC141C";
			public const string CC170CSchema = "CC170C";
		}

		public static class ApplicationTypes
		{
			public const string DeltaC = "DELTAC";
			public const string DeltaD = "DELTAD";
			public const string DeltaDV30 = "DELTADV30";
			public const string DeltaCG = "DELTAC/G";
			public const string DeltaDG = "DELTADV30/G";
			public const string DeltaIE = "DELTAIE";
			public const string DeltaT = "DELTAT";
			public const string TP5 = "TP5";
			public const string ECS = "ECS";
			public const string APPLUS = "APPLUS";
			public const string CIN = "CIN";
			public const string PNTS = "PNTS";
		}

		public const string CW1 = "CW1";

		public static string[] StandardResponseMessageTypeList = new string[]
		{
			MessageTypes.DeltaReponse,
			MessageTypes.EDIFACT,
			MessageTypes.APPLUS,
			MessageTypes.PNTS.IETS016,
			MessageTypes.PNTS.IETS028,
			MessageTypes.PNTS.IETS029,
			MessageTypes.PNTS.IETS030,
			MessageTypes.PNTS.IETS410,
			MessageTypes.PNTS.IETS460,
			MessageTypes.PNTS.IETS906,
			MessageTypes.PNTS.IETS928,
			MessageTypes.DeltaIE.FRA101,
			MessageTypes.DeltaIE.FRA102,
			MessageTypes.DeltaIE.FRA103,
			MessageTypes.DeltaIE.IE404,
			MessageTypes.DeltaIE.IE410,
			MessageTypes.DeltaIE.IE426,
			MessageTypes.DeltaIE.IE428,
			MessageTypes.DeltaIE.IE429,
			MessageTypes.DeltaIE.IE431,
			MessageTypes.DeltaIE.IE451,
			MessageTypes.DeltaIE.IE456,
			MessageTypes.DeltaIE.IE457,
			MessageTypes.DeltaIE.IE460,
			MessageTypes.DeltaIE.IE462,
			MessageTypes.DeltaIE.IE917,
		};

		public const string DeltaTPhase5NameSpace = "http://ncts.dgtaxud.ec";

		public static string[] DeltaTPhase5TagsNeedingNameSpace = new string[]
		{
			"Consignment",
			"correlationIdentifier",
			"CTLControl",
			"CustomsOfficeOfDeparture",
			"CustomsOfficeOfDestinationDeclared",
			"CustomsOfficeOfTransitDeclared",
			"CustomsOfficeOfExitForTransitDeclared",
			"FunctionalError",
			"HolderOfTheTransitProcedure",
			"Guarantee",
			"Guarantor",
			"Header",
			"Invalidation",
			"messageIdentification",
			"messageRecipient",
			"messageSender",
			"messageType",
			"preparationDateAndTime",
			"RecoveryNotification",
			"RiskAnalysisIdentification",
			"TransitOperation",
			"XMLError",
		};
	}
}
