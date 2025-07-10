using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class IPTDEC : BaseTradeNetMessage<InPayment>
	{
		public IPTDEC(IIPTDEC cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.IPTDEC;
		public override string MessageSubType => CUSDECEDIMessage.Declaration;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.InPayment = BuildDeclaration();
		}

		#region Cargo

		protected override Cargo BuildCargoCore()
		{
			var cargo = base.BuildCargoCore();
			BuildBlanketStartDate(cargo);

			return cargo;
		}

		protected override StorageLocation BuildStorageLocationCore(ZString placeOfStorage) => null;

		#endregion

		#region Transport
		protected override OutwardTransport BuildOutwardTransportCore() => null;

		#endregion

		#region Party
		protected override ConsigneeParty BuildConsigneePartyCore(IOrganisation consignee) => null;
		protected override ExporterParty BuildExporterPartyCore(IOrganisation exporter) => null;
		protected override OutwardCarrierAgentParty BuildOutwardCarrierAgentPartyCore(IOrganisation outwardCarrierAgent) => null;
		#endregion

		#region Invoice

		protected override bool SupportsInvoiceNumberSegment => true;

		#endregion

		#region Tariff

		protected override bool SupportsOtherTax => true;

		#endregion

		#region Item
		protected override bool SupportsOutHAWBHUCRHBLNumber => false;

		#region MotorVehicle
		protected override bool SupportsMotorVehicleSection => !CusDec.IsShortPayment;
		protected override bool SupportsRegistrationDateSection => true;
		#endregion

		protected override bool RequiresUnitPriceExchangeRate(ICusItem cusItem)
		{
			return true;
		}

		#endregion
	}
}
