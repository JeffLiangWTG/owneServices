using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitSharePartyLookups : AutoOrgProfitSharePartyLookups
	{
		public OrgProfitSharePartyLookups()
			: this(null)
		{
		}

		public OrgProfitSharePartyLookups(AutoOrgProfitShareParty parent) : base(parent)
		{
		}

		#region Party Type

		public CodeDescriptionPairList PartyTypes
		{
			get
			{
				var party = Parent as OrgProfitShareParty;
				if (party == null)
				{
					// When Parent is null we cannot access Factory (which is Parent.Factory)
					// This is when we try to load Party Types for Charge Code with Types on Registry.
					return GetPartyTypesForChargeCodeWithType();
				}

				return GetPartyTypesForPartyDetails(party.OrgProfitShareType);
			}
		}

		CodeDescriptionPairList GetPartyTypesForPartyDetails(string orgProfitShareType)
		{
			var result = new CodeDescriptionPairList();
			switch (orgProfitShareType)
			{
				case OrgProfitSharePartyCollection.OrgProfitShareTypeGeneral:
					var profitShareDetails = ((OrgProfitShareParty)Parent).ProfitShareDetails;
					if (profitShareDetails == null)
					{
						return result;
					}

					result.AddPair(PartyTypeCodes.SendingAgent, PartyTypeCodes.SendingAgentDescription);
					result.AddPair(PartyTypeCodes.ReceivingAgent, PartyTypeCodes.ReceivingAgentDescription);
					if (profitShareDetails.O4_JobType != JobTypesList.Codes.GCN)
					{
						result.AddPair(PartyTypeCodes.ControllingAgent, PartyTypeCodes.ControllingAgentDescription);
						result.AddPair(PartyTypeCodes.PickupAgent, PartyTypeCodes.PickupAgentDescription);
						result.AddPair(PartyTypeCodes.DeliveryAgent, PartyTypeCodes.DeliveryAgentDescription);
					}
					result.AddPair(PartyTypeCodes.HeadOfficeFranchisor, PartyTypeCodes.HeadOfficeFranchisorDescription);

					return result;

				case OrgProfitSharePartyCollection.OrgProfitShareTypeGatewayConsolProfitRedistribution:
					result.AddPair(PartyTypeCodes.ShipmentPickupAgent, PartyTypeCodes.ShipmentPickupAgentDescription);
					result.AddPair(PartyTypeCodes.ShipmentDeliveryAgent, PartyTypeCodes.ShipmentDeliveryAgentDescription);
					return result;

				default:
					return GetPartyTypesForChargeCodeWithType();
			}
		}

		static CodeDescriptionPairList GetPartyTypesForChargeCodeWithType()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(PartyTypeCodes.SendingAgent, PartyTypeCodes.SendingAgentDescription);
			result.AddPair(PartyTypeCodes.ReceivingAgent, PartyTypeCodes.ReceivingAgentDescription);
			result.AddPair(PartyTypeCodes.ControllingAgent, PartyTypeCodes.ControllingAgentDescription);
			result.AddPair(PartyTypeCodes.HeadOfficeFranchisor, PartyTypeCodes.HeadOfficeFranchisorDescription);
			result.AddPair(PartyTypeCodes.GatewayAgent, PartyTypeCodes.GatewayAgentDescription);
			result.AddPair(PartyTypeCodes.LeadGatewayAgent, PartyTypeCodes.LeadGatewayAgentDescription);
			result.AddPair(PartyTypeCodes.PickupAgent, PartyTypeCodes.PickupAgentDescription);
			result.AddPair(PartyTypeCodes.DeliveryAgent, PartyTypeCodes.DeliveryAgentDescription);
			return result;
		}

		public static class PartyTypeCodes
		{
			public const string SendingAgent = "SEN";
			public const string ReceivingAgent = "RCV";
			public const string ControllingAgent = "CON";
			public const string HeadOfficeFranchisor = "HDF";
			public const string GatewayAgent = "GWA";
			public const string LeadGatewayAgent = "LGA";
			public const string ShipmentPickupAgent = "SPA";
			public const string ShipmentDeliveryAgent = "SDA";
			public const string PickupAgent = "PIC";
			public const string DeliveryAgent = "DLY";

			/// <summary>
			/// Percentages of the following generic types when present should add up to 100% in total.
			/// </summary>
			public static HashSet<string> GeneralTypes =>
			[
				SendingAgent,
				ReceivingAgent,
				ControllingAgent,
				HeadOfficeFranchisor,
				PickupAgent,
				DeliveryAgent,
			];

			/// <summary>
			/// Percentages of the following gateway specific types when present should add up to 100% in total.
			/// </summary>
			public static HashSet<string> GatewayConsolProfitRedistributionTypes =>
			[
				ShipmentPickupAgent,
				ShipmentDeliveryAgent,
			];

			public static MultilingualString SendingAgentDescription => ResString.GetMultilingualString("3ad7f577-f2f0-4d65-8574-de1b802d81a9", "Sending Agent");
			public static MultilingualString ReceivingAgentDescription => ResString.GetMultilingualString("34baa181-8750-4bb0-83e5-3ec9ed4b0f21", "Receiving Agent");
			public static MultilingualString ControllingAgentDescription => ResString.GetMultilingualString("74cde3da-3401-4a10-b3cc-58fd30572c7a", "Controlling Agent");
			public static MultilingualString HeadOfficeFranchisorDescription => ResString.GetMultilingualString("85befe1f-1218-401b-8556-c159dc154223", "Head Office");
			public static MultilingualString GatewayAgentDescription => ResString.GetMultilingualString("09d2fb90-2dca-4ae8-ad61-7c6b1ca9a6f9", "G/W Agent (G/W Consol)");
			public static MultilingualString LeadGatewayAgentDescription => ResString.GetMultilingualString("99391c36-b7b4-4319-89d7-e3e7ef3c8f8d", "Lead G/W Agent");
			public static MultilingualString ShipmentPickupAgentDescription => ResString.GetMultilingualString("6d3f8588-34ae-4075-80bc-4736530e761b", "Shipment Pickup Agent");
			public static MultilingualString ShipmentDeliveryAgentDescription => ResString.GetMultilingualString("c2501269-c78c-4a83-9583-37f161ed8d73", "Shipment Delivery Agent");
			public static MultilingualString PickupAgentDescription => ResString.GetMultilingualString("b03bf194-1105-40e4-baf8-2c581bc6baea", "Pickup Agent");
			public static MultilingualString DeliveryAgentDescription => ResString.GetMultilingualString("8610d237-ec31-4279-880a-9d1dc4a2431e", "Delivery Agent");
		}

		#endregion

		#region Fee Basis

		public CodeDescriptionPairList FeeBasis
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(FeeBasisCodes.ChargeableUnit, Res.GetString("0bf1b1e7-fd5f-4337-a075-81ac3d2a6767", "Per Chargeable Unit"));
				result.AddPair(FeeBasisCodes.FlatFee, Res.GetString("be830e81-3fb1-4139-9873-9631587ee9c3", "Flat Fee per House-bill"));
				result.AddPair(FeeBasisCodes.GrossRevenue, Res.GetString("47b07403-91b0-424d-b521-6b6433426578", "Percentage of Gross Revenue"));
				result.AddPair(FeeBasisCodes.PerContainer, Res.GetString("a4f7dd12-bf93-4f54-b1e2-ddaa80a2d402", "Per Container"));

				return result;
			}
		}

		public static class FeeBasisCodes
		{
			public const string ChargeableUnit = "UNT";
			public const string FlatFee = "FLT";
			public const string GrossRevenue = "GRV";
			public const string PerContainer = "CNT";
		}

		#endregion
	}
}

