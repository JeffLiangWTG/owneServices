using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Registry
{
	public static class PhaseConstants
	{
		#region Phase

		public static class Phase
		{
			public const string ALL = "ALL";
		}

		public static CodeDescriptionPairList GetCommonPhaseList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Phase.ALL, ResString.GetMultilingualString("4fd33a98-ff0b-4cc6-93e0-08bd3d7f3738", "Open Security"));

			return result;
		}

		#endregion

		#region Locations

		public static class Locations
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code identifier")]
			public const string AnyLocation = "Any Location";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code identifier")]
			public static class Consol
			{
				public const string FirstLoadPort = "First Load Port";
				public const string FirstLoadOrOwnSendingAgentPort = "First Load Port OR Own Sending Agent Port";
				public const string FirstLoadCountry = "First Load Country";
				public const string FirstLoadOrOwnSendingAgentCountry = "First Load Country OR Own Sending Agent Country";
				public const string LoadPort = "Load Port";
				public const string LoadCountry = "Load Country";
				public const string TransitCountry = "Transit Country";
				public const string DischargePort = "Discharge Port";
				public const string DischargeCountry = "Discharge Country";
				public const string LastDischargePort = "Last Discharge Port";
				public const string LastDischargeOrOwnReceivingAgentPort = "Last Discharge Port OR Own Receiving Agent Port";
				public const string LastDischargeCountry = "Last Discharge Country";
				public const string LastDischargeOrOwnReceivingAgentCountry = "Last Discharge Country OR Own Receiving Agent Country";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code identifier")]
			public static class Shipment
			{
				public const string LoadPort = "Load Port";
				public const string LoadOrOwnSendingAgentPort = "Load Port OR Own Sending Agent Port";
				public const string LoadCountry = "Load Country";
				public const string LoadOrOwnSendingAgentCountry = "Load Country OR Own Sending Agent Country";
				public const string OriginPort = "Origin Port";
				public const string OriginCountry = "Origin Country";
				public const string TransitCountry = "Transit Country";
				public const string DestinationPort = "Destination Port";
				public const string DestinationCountry = "Destination Country";
				public const string DischargePort = "Discharge Port";
				public const string DischargeOrOwnReceivingAgentPort = "Discharge Port OR Own Receiving Agent Port";
				public const string DischargeCountry = "Discharge Country";
				public const string DischargeOrOwnReceivingAgentCountry = "Discharge Country OR Own Receiving Agent Country";
			}
		}

		public static CodeDescriptionPairList GetCommonLocationsList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Locations.AnyLocation, ResString.GetMultilingualString("40d85000-3a68-4ff9-9ad4-f9a9d418b08c", "Any Location"));

			return result;
		}

		public static CodeDescriptionPairList GetConsolLocationsList()
		{
			CodeDescriptionPairList result = GetCommonLocationsList();
			result.AddPair(Locations.Consol.FirstLoadPort, ResString.GetMultilingualString("99eb3e61-3f15-4e81-8def-2e6d8ce2108a", "Port matching Consol’s first load port"));
			result.AddPair(Locations.Consol.FirstLoadOrOwnSendingAgentPort, ResString.GetMultilingualString("c303f442-a728-4f84-a936-05ff9a9f1354", "Port matching Consol’s first load or Own Sending Agent port"));
			result.AddPair(Locations.Consol.FirstLoadCountry, ResString.GetMultilingualString("e8fdc4ee-dc5e-404b-a43e-138efe4da783", "Port matching Consol’s first load port’s country/region"));
			result.AddPair(Locations.Consol.FirstLoadOrOwnSendingAgentCountry, ResString.GetMultilingualString("85b17f16-fe4b-4e80-ae95-029e2d4ea524", "Port matching Consol’s first load or Own Sending Agent country/region"));
			result.AddPair(Locations.Consol.LoadPort, ResString.GetMultilingualString("7be0ac28-631e-4593-aefa-db72ba104936", "Load port"));
			result.AddPair(Locations.Consol.LoadCountry, ResString.GetMultilingualString("b47f3142-1500-4e6b-9d16-9a1b36b7a110", "Load country/region"));
			result.AddPair(Locations.Consol.TransitCountry, ResString.GetMultilingualString("fd06839f-0aa1-4b54-83f3-c2cd6180943e", "Country/Region of transshipment or transit"));
			result.AddPair(Locations.Consol.DischargePort, ResString.GetMultilingualString("22225d25-c77c-48b3-83c0-6172696c501e", "Discharge port"));
			result.AddPair(Locations.Consol.DischargeCountry, ResString.GetMultilingualString("c562c0d0-ef5f-4861-b3e4-eaa51ab29005", "Discharge country/region"));
			result.AddPair(Locations.Consol.LastDischargePort, ResString.GetMultilingualString("dbae46a4-85cc-4dc3-9a68-9b6bd286eafb", "Port matching Consol’s last discharge port"));
			result.AddPair(Locations.Consol.LastDischargeOrOwnReceivingAgentPort, ResString.GetMultilingualString("70ea905e-6223-4799-be0f-83f52fb0d33a", "Port matching Consol’s last discharge or Own Receiving Agent port"));
			result.AddPair(Locations.Consol.LastDischargeCountry, ResString.GetMultilingualString("0fefcae9-5599-4479-9596-7b4588b899c5", "Ports matching Consol’s last discharge port’s country/region"));
			result.AddPair(Locations.Consol.LastDischargeOrOwnReceivingAgentCountry, ResString.GetMultilingualString("7324f597-e398-4474-b63f-287d0779b66c", "Ports matching Consol’s last discharge or Own Receiving Agent country/region"));

			return result;
		}

		public static CodeDescriptionPairList GetShipmentLocationsList()
		{
			CodeDescriptionPairList result = GetCommonLocationsList();
			result.AddPair(Locations.Shipment.LoadPort, ResString.GetMultilingualString("99eb3e61-3f15-4e81-8def-2e6d8ce2108a", "Port matching Consol’s first load port"));
			result.AddPair(Locations.Shipment.LoadOrOwnSendingAgentPort, ResString.GetMultilingualString("0f16d4ad-50b8-4167-9ba1-746544eb2eaf", "Port matching Consol’s First Load or Own Sending Agent Port"));
			result.AddPair(Locations.Shipment.LoadCountry, ResString.GetMultilingualString("e8fdc4ee-dc5e-404b-a43e-138efe4da783", "Port matching Consol’s first load port’s country/region"));
			result.AddPair(Locations.Shipment.LoadOrOwnSendingAgentCountry, ResString.GetMultilingualString("5a773bd4-8c85-4045-b9ac-30c1fb20389a", "Port matching Consol’s First Load or Own Sending Agent Country/Region"));
			result.AddPair(Locations.Shipment.OriginPort, ResString.GetMultilingualString("d9c9c1b1-5628-4fea-ac91-47903503a68f", "Origin Port of Shipment/Booking"));
			result.AddPair(Locations.Shipment.OriginCountry, ResString.GetMultilingualString("9b6c8212-2c26-452d-ab6b-5adb7b781c0e", "Origin Country/Region of Shipment/Booking"));
			result.AddPair(Locations.Shipment.TransitCountry, ResString.GetMultilingualString("ae76c902-038d-4f08-96d5-f1842fbef979", "Country/Region of transshipment or transit of Shipment/Consol"));
			result.AddPair(Locations.Shipment.DestinationPort, ResString.GetMultilingualString("f1b79271-3845-4ed8-98fb-73656259c519", "Destination Port of Shipment"));
			result.AddPair(Locations.Shipment.DestinationCountry, ResString.GetMultilingualString("67030fc4-c4ab-4182-87ce-ecf692a02452", "Destination Country/Region of Shipment"));
			result.AddPair(Locations.Shipment.DischargePort, ResString.GetMultilingualString("dbae46a4-85cc-4dc3-9a68-9b6bd286eafb", "Port matching Consol’s last discharge port"));
			result.AddPair(Locations.Shipment.DischargeOrOwnReceivingAgentPort, ResString.GetMultilingualString("59480910-da39-4a57-8a6f-e8f090733dcb", "Port matching Consol’s Last Discharge or Own Receiving Agent Port"));
			result.AddPair(Locations.Shipment.DischargeCountry, ResString.GetMultilingualString("0fefcae9-5599-4479-9596-7b4588b899c5", "Ports matching Consol’s last discharge port’s country/region"));
			result.AddPair(Locations.Shipment.DischargeOrOwnReceivingAgentCountry, ResString.GetMultilingualString("eaf419a4-f19b-4a18-aea7-05b080a54668", "Port matching Consol’s Last Discharge or Own Receiving Agent Country/Region"));
			return result;
		}
		#endregion

		#region DependantType

		public static class DependantType
		{
			public const string Property = "PRO";
			public const string TypeName = "TYP";
		}

		#endregion
	}
}
