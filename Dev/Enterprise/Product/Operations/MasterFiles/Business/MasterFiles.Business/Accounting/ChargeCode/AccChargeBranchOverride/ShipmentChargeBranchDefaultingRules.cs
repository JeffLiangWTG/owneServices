namespace Enterprise.MasterFiles.Business
{
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Core;

	class ShipmentChargeBranchDefaultingRules
	{
		public static readonly CodeDescriptionPair ReceivingAgent = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent, ResString.GetMultilingualString("cc3b46a9-ac5c-4b1f-b1e7-48a1afa2a71f", "Receiving Agent"));

		public static readonly CodeDescriptionPair ShipmentDeliveryAgentWithFallbackToReceivingAgent = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.DeliveryAgentWithReceivingAgentFallback, ResString.GetMultilingualString("9a09af13-239f-4220-9328-fd01efb37eff", "Shipment Delivery Agent with Consol Receiving Agent Fallback"));

		public static readonly CodeDescriptionPair SendingAgent = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.SendingAgent, ResString.GetMultilingualString("9f300aa6-8b18-47ae-b003-7b2794ed24f0", "Sending Agent"));

		public static readonly CodeDescriptionPair ArrivalCTO = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO, ResString.GetMultilingualString("4792f0a7-cd66-4be6-8daf-33d1df4e2a02", "Arrival CTO"));

		public static readonly CodeDescriptionPair DepartureCTO = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.DepartureCTO, ResString.GetMultilingualString("1e6c1755-bc22-458c-a941-67d225b09540", "Departure CTO"));

		public static readonly CodeDescriptionPair ConsolArrivalLocalTransport = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport, ResString.GetMultilingualString("edb71163-e210-460f-be4d-0ddfbf663f06", "Consol Arrival Port Transport"));

		public static readonly CodeDescriptionPair ConsolDepartureLocalTransport = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ConsolDepartureLocalTransport, ResString.GetMultilingualString("4b0eb821-178a-46ae-85d9-cbf4453ee7c2", "Consol Departure Port Transport"));

		public static readonly CodeDescriptionPair ShipmentPickupLocalTransportCompany = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ShipmentPickupLocalTransportCompany, ResString.GetMultilingualString("a442c3eb-f4b4-4ae0-8a34-9f60312826e7", "Shipment Pickup Local Transport Company"));

		public static readonly CodeDescriptionPair ShipmentDeliveryLocalTransportCompany = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ShipmentDeliveryLocalTransportCompany, ResString.GetMultilingualString("3cd0867d-0d7c-40be-a052-db3ead834a18", "Shipment Delivery Local Transport Company"));

		public static readonly CodeDescriptionPair ShipmentImportBroker = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker, ResString.GetMultilingualString("08cff0c6-2016-4152-b9f7-ba1ffc0115e8", "Shipment Import Broker"));

		public static readonly CodeDescriptionPair ShipmentExportBroker = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.ShipmentExportBroker, ResString.GetMultilingualString("f669b1db-c13b-46f3-943c-19eb1b979eba", "Shipment Export Broker"));
	}
}
