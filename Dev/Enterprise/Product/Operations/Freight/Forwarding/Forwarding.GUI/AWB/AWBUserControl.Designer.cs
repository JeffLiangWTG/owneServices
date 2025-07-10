using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBUserControl
	{
		CargoWise.Windows.UI.KPanel ShippersPanel;
		ZTextBox zTextBox4;
		CargoWise.Windows.UI.KPanel panel1;
		ZTextBox zTextBox5;
		ZCodeFindBox ByFirstCarrier;
		ZTextBox ByFirstCarrierName;
		public Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel AWBBottomPanel;
		protected ZPanel AWBPanel;
		CargoWise.Windows.UI.KPanel asAgreedPanel;
		Enterprise.ZArchitecture.GUI.ZButton ShipperAddressSaveButton;
		Enterprise.ZArchitecture.GUI.ZButton ConsigneeAddressSaveButton;
		Enterprise.ZArchitecture.GUI.ZButton AlsoNotifyAddressSaveButton;
		ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel zLabel93;
		Enterprise.ZArchitecture.ZLabel zLabel89;
		Enterprise.ZArchitecture.ZLabel zLabel88;
		Enterprise.ZArchitecture.ZLabel zLabel87;
		Enterprise.ZArchitecture.ZLabel zLabel86;
		Enterprise.ZArchitecture.ZLabel zLabel85;
		Enterprise.ZArchitecture.ZLabel zLabel84;
		Enterprise.ZArchitecture.ZLabel zLabel83;
		Enterprise.ZArchitecture.ZLabel zLabel82;
		Enterprise.ZArchitecture.ZLabel zLabel81;
		Enterprise.ZArchitecture.ZLabel zLabel80;
		Enterprise.ZArchitecture.ZLabel zLabel79;
		Enterprise.ZArchitecture.ZLabel zLabel78;
		Enterprise.ZArchitecture.ZLabel zLabel77;
		Enterprise.ZArchitecture.ZLabel zLabel76;
		Enterprise.ZArchitecture.ZLabel zLabel75;
		Enterprise.ZArchitecture.ZLabel zLabel74;
		Enterprise.ZArchitecture.ZLabel zLabel73;
		Enterprise.ZArchitecture.ZLabel zLabel72;
		Enterprise.ZArchitecture.ZLabel zLabel71;
		Enterprise.ZArchitecture.ZLabel zLabel70;
		Enterprise.ZArchitecture.ZLabel zLabel69;
		Enterprise.ZArchitecture.ZLabel zLabel68;
		Enterprise.ZArchitecture.ZLabel zLabel67;
		Enterprise.ZArchitecture.ZLabel zLabel66;
		Enterprise.ZArchitecture.ZLabel zLabel65;
		Enterprise.ZArchitecture.ZLabel zLabel63;
		Enterprise.ZArchitecture.ZLabel zLabel62;
		Enterprise.ZArchitecture.ZLabel zLabel61;
		Enterprise.ZArchitecture.ZLabel zLabel60;
		Enterprise.ZArchitecture.ZLabel zLabel59;
		Enterprise.ZArchitecture.ZLabel zLabel58;
		Enterprise.ZArchitecture.ZLabel zLabel57;
		Enterprise.ZArchitecture.ZLabel zLabel56;
		Enterprise.ZArchitecture.ZLabel zLabel55;
		Enterprise.ZArchitecture.ZLabel zLabel54;
		Enterprise.ZArchitecture.ZLabel zLabel53;
		Enterprise.ZArchitecture.ZLabel zLabel52;
		Enterprise.ZArchitecture.ZLabel zLabel51;
		Enterprise.ZArchitecture.ZLabel zLabel50;
		Enterprise.ZArchitecture.ZLabel zLabel49;
		Enterprise.ZArchitecture.ZLabel zLabel48;
		Enterprise.ZArchitecture.ZLabel zLabel47;
		Enterprise.ZArchitecture.ZLabel zLabel46;
		Enterprise.ZArchitecture.ZLabel zLabel45;
		Enterprise.ZArchitecture.ZLabel zLabel44;
		Enterprise.ZArchitecture.ZLabel zLabel43;
		Enterprise.ZArchitecture.ZLabel zLabel42;
		Enterprise.ZArchitecture.ZLabel zLabel41;
		Enterprise.ZArchitecture.ZLabel zLabel40;
		Enterprise.ZArchitecture.ZLabel zLabel39;
		Enterprise.ZArchitecture.ZLabel zLabel36;
		Enterprise.ZArchitecture.ZLabel zLabel35;
		Enterprise.ZArchitecture.ZLabel zLabel34;
		Enterprise.ZArchitecture.ZLabel zLabel32;
		Enterprise.ZArchitecture.ZLabel zLabel31;
		Enterprise.ZArchitecture.ZLabel zLabel30;
		Enterprise.ZArchitecture.ZLabel zLabel29;
		Enterprise.ZArchitecture.ZLabel zLabel28;
		Enterprise.ZArchitecture.ZLabel zLabel27;
		Enterprise.ZArchitecture.ZLabel zLabel103;
		Enterprise.ZArchitecture.ZLabel zLabel102;
		Enterprise.ZArchitecture.ZLabel zLabel101;
		Enterprise.ZArchitecture.ZLabel zLabel64;
		Enterprise.ZArchitecture.ZTextBox ECNCRNNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox ReferenceNumberTextBox;
		Enterprise.ZArchitecture.ZCalcEdit TotalLineTotalsCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TotalGrossWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TotalNoOfPiecesCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TotalPrepaidCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TotalCollectCalcEdit;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox10;
		RateLineCommodityItemNumberControl CommodityItemNumberControl10;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit10;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit10;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit10;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit10;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit10;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit10;
		RateLineCommodityItemNumberControl CommodityItemNumberControl9;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox9;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit9;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit9;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit9;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit9;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit9;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit9;
		RateLineCommodityItemNumberControl CommodityItemNumberControl8;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox8;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit8;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit8;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit8;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit8;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit8;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit8;
		RateLineCommodityItemNumberControl CommodityItemNumberControl7;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox7;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit7;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit7;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit7;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit7;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit7;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit7;
		RateLineCommodityItemNumberControl CommodityItemNumberControl6;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox6;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit6;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit6;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit6;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit6;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit6;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit6;
		RateLineCommodityItemNumberControl CommodityItemNumberControl5;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox5;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit5;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit5;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit5;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit5;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit5;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit5;
		RateLineCommodityItemNumberControl CommodityItemNumberControl4;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox4;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit4;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit4;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit4;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit4;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit4;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit4;
		RateLineCommodityItemNumberControl CommodityItemNumberControl3;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox3;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit3;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit3;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit3;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit3;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit3;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit3;
		RateLineCommodityItemNumberControl CommodityItemNumberControl2;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox2;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit2;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit2;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit2;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit2;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit2;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit2;
		RateLineCommodityItemNumberControl CommodityItemNumberControl1;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox1;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateClassDropEdit1;
		Enterprise.ZArchitecture.GUI.ZDropEdit RateUQDropEdit1;
		Enterprise.ZArchitecture.ZCalcEdit LineTotalCalcEdit1;
		Enterprise.ZArchitecture.ZCalcEdit RateChargeCalcEdit1;
		Enterprise.ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit1;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit1;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl AccountingInformationTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage AccountingInformationTabPage;
		Enterprise.ZArchitecture.ZGrid AccountingInformationGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage NotifyPartyInfoTabPage;
		Enterprise.ZArchitecture.ZTextBox NotifyContactDetailTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyCountryTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyCityTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyStateTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyNameTextBox;
		Enterprise.ZArchitecture.ZTextBox NotifyContactNameTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel18;
		Enterprise.ZArchitecture.ZTextBox BookingSecondFlightDateTextBox;
		Enterprise.ZArchitecture.ZTextBox BookingSecondFlightTextBox;
		Enterprise.ZArchitecture.ZTextBox BookingSecondCarrierTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel17;
		Enterprise.ZArchitecture.ZTextBox BookingFirstFlightDateTextBox;
		Enterprise.ZArchitecture.ZTextBox BookingFirstFlightTextBox;
		Enterprise.ZArchitecture.ZTextBox BookingFirstCarrierTextBox;
		Enterprise.ZArchitecture.ZTextBox AirlinePrefixTextBox;
		Enterprise.ZArchitecture.ZTextBox OriginCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox SerialNumberTextBox;
		Enterprise.ZArchitecture.ZCalcEdit WeightChargePPDCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit WeightChargeCollectCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit ValuationChargePPDCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit ValuationChargeCollectCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TaxChargeCollectCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TaxChargePPDCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OtherChargeAgentCollectCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OtherChargeAgentPPDCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OtherChargeCarrierCollectCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OtherChargeCarrierPPDCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit InsuranceValueCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit CustomsValueCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit DeclaredValueCalcEdit;
		Enterprise.ZArchitecture.ZTextBox ShippersSignatureTextBox;
		Enterprise.ZArchitecture.ZTextBox AgentsSignatureTextBox;
		Enterprise.ZArchitecture.ZTextBox IssueCityTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit AWBIssueDateDateEdit;
		Enterprise.ZArchitecture.ZTextBox CurrencyTextBox;
		Enterprise.ZArchitecture.ZTextBox AgentCityTextBox;
		Enterprise.ZArchitecture.ZTextBox AgentsIATACodeTextBox;
		Enterprise.ZArchitecture.ZTextBox AgentsAccountNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox AgentNameTextBox;
		Enterprise.ZArchitecture.ZTextBox HandlingInformationTextBox;
		Enterprise.ZArchitecture.ZTextBox DestinationTextBox;
		Enterprise.ZArchitecture.ZTextBox AirportOfDepartureTextBox;
		Enterprise.ZArchitecture.ZTextBox ToFirstTextBox;
		Enterprise.ZArchitecture.ZTextBox ToSecondTextBox;
		Enterprise.ZArchitecture.ZTextBox BySecondTextBox;
		Enterprise.ZArchitecture.ZTextBox ToThirdTextBox;
		Enterprise.ZArchitecture.ZTextBox ByThirdTextBox;
		public ZPanel AWBTopPanel;
		public Enterprise.ZArchitecture.GUI.ZCheckBox OverrideValuesCheckBox;
		Enterprise.ZArchitecture.ZTextBox ConsolNumberTextBox;
		internal CargoWise.Windows.UI.KLabel AWBImagelabel;
		Enterprise.ZArchitecture.GUI.ZDropEdit AsAgreed2ndDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit AsAgreed1stDropEdit;
		Enterprise.ZArchitecture.ZTextBox IssuingCarrierAddress1TextBox;
		Enterprise.ZArchitecture.ZTextBox IssuingCarrierAddress2TextBox;
		Enterprise.ZArchitecture.ZTextBox IssuingCarrierNameTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_NetRateTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_AgentApprovedExporterNumberTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_SecurityStatusTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperContactNameTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperContactDetailTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperTraderTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperTraderNoTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeTraderTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeTraderNoTextBox;
		Enterprise.ZArchitecture.ZTextBox AlsoNotifyTraderTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox AlsoNotifyTraderNoTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperStateTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperCityTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperNameTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox ShipperCountryTextBox;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl ShipperAddressTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ShipperNameAndAddressTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage ShipperNameAndAddressOverrideTabPage;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperAccountTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperOverride1TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperOverride2TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperOverride3TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperOverride4TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperOverride5TextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OverrideShipperAddressCheckBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeContactDetailTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeContactNameTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel26;
		Enterprise.ZArchitecture.ZTextBox ConsigneeAccountTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeStateTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeCityTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeNameTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox ConsigneeCountryTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OverrideConsigneeAddressCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsDeclarantForAdvanceCargoReportingCheckBox;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl ConsigneeNameAndAddressTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ConsigneeAddressTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ConsigneeOverrideTabPage;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneeOverride5TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneeOverride4TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneeOverride3TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneeOverride2TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneeOverride1TextBox;
		Enterprise.ZArchitecture.GUI.ZTabPage NotifyAddressOverrideTabPage;
		Enterprise.ZArchitecture.ZTextBox EH_NotifyOverride5TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_NotifyOverride4TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_NotifyOverride3TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_NotifyOverride2TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_NotifyOverride1TextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox EH_IsNotifyOverridenCheckBox;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox12;
		protected Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsControl NatureAndQtyOfGoodsTextBox11;
		Enterprise.ZArchitecture.ZTextBox EH_OptionalShippingInformationTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_OptionalShippingInformation2TextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ShipperPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_ConsigneePostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_AlsoNotifyPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox EH_AirportOfDestinationCodeTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit EH_ShipperContactCodeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ConsigneeContactCodeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit EH_AlsoNotifyContactCodeDropEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit EH_ShippingLoadAndCountCalcEdit;
		protected Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox2;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.ZTextBox NoPieces10TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces9TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces8TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces7TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces6TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces5TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces4TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces3TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces2TextBox;
		Enterprise.ZArchitecture.ZTextBox NoPieces1TextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit ChargesCodeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit WeightValueDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit OtherChargesDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox OverrideRateSectionCheckBox;
		ZDropEdit ChooseConsigneeAddressDropEdit;
		ZDropEdit ChooseShipperAddressDropEdit;
		ZDropEdit ChooseAlsoNotifyAddressDropEdit;
		ZDropEdit SCIDropEdit;
		ZTextBox NoPieces11TextBox;
		RateLineCommodityItemNumberControl CommodityItemNumberControl11;
		ZDropEdit RateClassDropEdit11;
		ZDropEdit RateUQDropEdit11;
		ZCalcEdit LineTotalCalcEdit11;
		ZCalcEdit RateChargeCalcEdit11;
		ZCalcEdit ChargeableWeightCalcEdit11;
		ZCalcEdit GrossWeightCalcEdit11;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AWBBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AWBPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.asAgreedPanel = new CargoWise.Windows.UI.KPanel();
			this.ByFirstCarrierName = new Enterprise.ZArchitecture.ZTextBox();
			this.ByFirstCarrier = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NoPieces11TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommodityItemNumberControl11 = new RateLineCommodityItemNumberControl();
			this.RateClassDropEdit11 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit11 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SCIDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargesCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OtherChargesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WeightValueDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoPieces1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces5TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces6TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces7TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces8TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces9TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NoPieces10TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.EH_ShippingLoadAndCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EH_AirportOfDestinationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_OptionalShippingInformation2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_OptionalShippingInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NatureAndQtyOfGoodsTextBox12 = GetNewNatureAndQtyOfGoodsControl();
			this.NatureAndQtyOfGoodsTextBox11 = GetNewNatureAndQtyOfGoodsControl();
			this.EH_IsNotifyOverridenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OverrideConsigneeAddressCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsDeclarantForAdvanceCargoReportingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsigneeNameAndAddressTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ConsigneeAddressTabControl = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.EH_ConsigneePostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeContactDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChooseConsigneeAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel26 = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeContactCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EH_ConsigneeOverride5TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ConsigneeOverride4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ConsigneeOverride3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ConsigneeOverride2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ConsigneeOverride1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideShipperAddressCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipperAddressTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ShipperNameAndAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShippersPanel = new CargoWise.Windows.UI.KPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.EH_ShipperContactCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipperContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperContactDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperTraderTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperTraderNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlsoNotifyTraderTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlsoNotifyTraderNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeTraderTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeTraderNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChooseShipperAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EH_ShipperPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ShipperAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperNameAndAddressOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EH_ShipperOverride5TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ShipperOverride4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ShipperOverride3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ShipperOverride2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_ShipperOverride1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_AgentApprovedExporterNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_SecurityStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_NetRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingCarrierNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingCarrierAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingCarrierAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AsAgreed1stDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AsAgreed2ndDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsolNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel93 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel89 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel88 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel87 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel86 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel85 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel84 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel83 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel82 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel81 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel80 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel79 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel78 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel77 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel76 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel75 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel74 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel73 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel72 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel71 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel70 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel69 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel68 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel67 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel66 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel65 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel63 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel62 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel61 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel60 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel59 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel58 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel57 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel56 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel55 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel54 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel53 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel52 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel51 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel50 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel49 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel48 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel47 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel46 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel45 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel44 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel43 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel42 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel40 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel27 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel103 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel102 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel101 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel64 = new Enterprise.ZArchitecture.ZLabel();
			this.ECNCRNNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalLineTotalsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalGrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalNoOfPiecesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPrepaidCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NatureAndQtyOfGoodsTextBox10 = GetNewNatureAndQtyOfGoodsControl();
			this.CommodityItemNumberControl10 = new RateLineCommodityItemNumberControl();
			this.RateClassDropEdit10 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit10 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl9 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox9 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit9 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit9 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl8 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox8 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit8 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit8 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl7 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox7 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit7 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit7 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl6 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox6 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit6 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit6 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl5 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox5 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit5 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit5 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl4 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox4 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl3 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox3 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl2 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox2 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommodityItemNumberControl1 = new RateLineCommodityItemNumberControl();
			this.NatureAndQtyOfGoodsTextBox1 = GetNewNatureAndQtyOfGoodsControl();
			this.RateClassDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateUQDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineTotalCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateChargeCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AccountingInformationTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.AccountingInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccountingInformationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NotifyPartyInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChooseAlsoNotifyAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EH_AlsoNotifyContactCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EH_AlsoNotifyPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyContactDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyAddressOverrideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EH_NotifyOverride5TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_NotifyOverride4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_NotifyOverride3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_NotifyOverride2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EH_NotifyOverride1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.BookingSecondFlightDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookingSecondFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookingSecondCarrierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.BookingFirstFlightDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookingFirstFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookingFirstCarrierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirlinePrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightChargePPDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WeightChargeCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValuationChargePPDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValuationChargeCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxChargeCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxChargePPDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherChargeAgentCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherChargeAgentPPDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherChargeCarrierCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherChargeCarrierPPDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InsuranceValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeclaredValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShippersSignatureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentsSignatureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssueCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AWBIssueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentsIATACodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentsAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HandlingInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirportOfDepartureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToFirstTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToSecondTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BySecondTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToThirdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ByThirdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AWBTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OverrideValuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipperAddressSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConsigneeAddressSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AlsoNotifyAddressSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OverrideRateSectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.AWBBottomPanel.SuspendLayout();
			this.AWBPanel.SuspendLayout();
			this.asAgreedPanel.SuspendLayout();
			this.ConsigneeNameAndAddressTabControl.SuspendLayout();
			this.ConsigneeAddressTabControl.SuspendLayout();
			this.panel1.SuspendLayout();
			this.ConsigneeOverrideTabPage.SuspendLayout();
			this.ShipperAddressTabControl.SuspendLayout();
			this.ShipperNameAndAddressTabPage.SuspendLayout();
			this.ShippersPanel.SuspendLayout();
			this.ShipperNameAndAddressOverrideTabPage.SuspendLayout();
			this.AccountingInformationTabControl.SuspendLayout();
			this.AccountingInformationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccountingInformationGrid)).BeginInit();
			this.NotifyPartyInfoTabPage.SuspendLayout();
			this.NotifyAddressOverrideTabPage.SuspendLayout();
			this.AWBTopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.IAWBParent);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.AWBBottomPanel);
			this.MainPanel.Controls.Add(this.AWBTopPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 1379, true);
			this.MainPanel.TabIndex = 274;
			// 
			// AWBBottomPanel
			// 
			this.AWBBottomPanel.AutoScroll = true;
			this.AWBBottomPanel.Controls.Add(this.AWBPanel);
			this.AWBBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.AWBBottomPanel.Name = "AWBBottomPanel";
			this.AWBBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 1347, true);
			this.AWBBottomPanel.TabIndex = 1;
			// 
			// AWBPanel
			// 
			this.AWBPanel.BackColor = System.Drawing.Color.White;
			this.AWBPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.AWBPanel.Controls.Add(this.ByFirstCarrierName);
			this.AWBPanel.Controls.Add(this.ByFirstCarrier);
			this.AWBPanel.Controls.Add(this.NoPieces11TextBox);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl11);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit11);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit11);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit11);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit11);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit11);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit11);
			this.AWBPanel.Controls.Add(this.SCIDropEdit);
			this.AWBPanel.Controls.Add(this.ChargesCodeDropEdit);
			this.AWBPanel.Controls.Add(this.OtherChargesDropEdit);
			this.AWBPanel.Controls.Add(this.WeightValueDropEdit);
			this.AWBPanel.Controls.Add(this.NoPieces1TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces2TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces3TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces4TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces5TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces6TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces7TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces8TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces9TextBox);
			this.AWBPanel.Controls.Add(this.NoPieces10TextBox);
			this.AWBPanel.Controls.Add(this.zTextBox3);
			this.AWBPanel.Controls.Add(this.zTextBox2);
			this.AWBPanel.Controls.Add(this.zTextBox1);
			this.AWBPanel.Controls.Add(this.zLabel7);
			this.AWBPanel.Controls.Add(this.EH_ShippingLoadAndCountCalcEdit);
			this.AWBPanel.Controls.Add(this.EH_AirportOfDestinationCodeTextBox);
			this.AWBPanel.Controls.Add(this.EH_OptionalShippingInformation2TextBox);
			this.AWBPanel.Controls.Add(this.EH_OptionalShippingInformationTextBox);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox12);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox11);
			this.AWBPanel.Controls.Add(this.EH_IsNotifyOverridenCheckBox);
			this.AWBPanel.Controls.Add(this.OverrideConsigneeAddressCheckBox);
			this.AWBPanel.Controls.Add(this.ConsigneeNameAndAddressTabControl);
			this.AWBPanel.Controls.Add(this.OverrideShipperAddressCheckBox);
			this.AWBPanel.Controls.Add(this.ShipperAddressTabControl);
			this.AWBPanel.Controls.Add(this.EH_AgentApprovedExporterNumberTextBox);
			this.AWBPanel.Controls.Add(this.EH_SecurityStatusTextBox);
			this.AWBPanel.Controls.Add(this.EH_NetRateTextBox);
			this.AWBPanel.Controls.Add(this.IssuingCarrierNameTextBox);
			this.AWBPanel.Controls.Add(this.IssuingCarrierAddress2TextBox);
			this.AWBPanel.Controls.Add(this.IssuingCarrierAddress1TextBox);
			this.AWBPanel.Controls.Add(this.asAgreedPanel);
			this.AWBPanel.Controls.Add(this.ConsolNumberTextBox);
			this.AWBPanel.Controls.Add(this.zLabel93);
			this.AWBPanel.Controls.Add(this.zLabel89);
			this.AWBPanel.Controls.Add(this.zLabel88);
			this.AWBPanel.Controls.Add(this.zLabel87);
			this.AWBPanel.Controls.Add(this.zLabel86);
			this.AWBPanel.Controls.Add(this.zLabel85);
			this.AWBPanel.Controls.Add(this.zLabel84);
			this.AWBPanel.Controls.Add(this.zLabel83);
			this.AWBPanel.Controls.Add(this.zLabel82);
			this.AWBPanel.Controls.Add(this.zLabel81);
			this.AWBPanel.Controls.Add(this.zLabel80);
			this.AWBPanel.Controls.Add(this.zLabel79);
			this.AWBPanel.Controls.Add(this.zLabel78);
			this.AWBPanel.Controls.Add(this.zLabel77);
			this.AWBPanel.Controls.Add(this.zLabel76);
			this.AWBPanel.Controls.Add(this.zLabel75);
			this.AWBPanel.Controls.Add(this.zLabel74);
			this.AWBPanel.Controls.Add(this.zLabel73);
			this.AWBPanel.Controls.Add(this.zLabel72);
			this.AWBPanel.Controls.Add(this.zLabel71);
			this.AWBPanel.Controls.Add(this.zLabel70);
			this.AWBPanel.Controls.Add(this.zLabel69);
			this.AWBPanel.Controls.Add(this.zLabel68);
			this.AWBPanel.Controls.Add(this.zLabel67);
			this.AWBPanel.Controls.Add(this.zLabel66);
			this.AWBPanel.Controls.Add(this.zLabel65);
			this.AWBPanel.Controls.Add(this.zLabel63);
			this.AWBPanel.Controls.Add(this.zLabel62);
			this.AWBPanel.Controls.Add(this.zLabel61);
			this.AWBPanel.Controls.Add(this.zLabel60);
			this.AWBPanel.Controls.Add(this.zLabel59);
			this.AWBPanel.Controls.Add(this.zLabel58);
			this.AWBPanel.Controls.Add(this.zLabel57);
			this.AWBPanel.Controls.Add(this.zLabel56);
			this.AWBPanel.Controls.Add(this.zLabel55);
			this.AWBPanel.Controls.Add(this.zLabel54);
			this.AWBPanel.Controls.Add(this.zLabel53);
			this.AWBPanel.Controls.Add(this.zLabel52);
			this.AWBPanel.Controls.Add(this.zLabel51);
			this.AWBPanel.Controls.Add(this.zLabel50);
			this.AWBPanel.Controls.Add(this.zLabel49);
			this.AWBPanel.Controls.Add(this.zLabel48);
			this.AWBPanel.Controls.Add(this.zLabel47);
			this.AWBPanel.Controls.Add(this.zLabel46);
			this.AWBPanel.Controls.Add(this.zLabel45);
			this.AWBPanel.Controls.Add(this.zLabel44);
			this.AWBPanel.Controls.Add(this.zLabel43);
			this.AWBPanel.Controls.Add(this.zLabel42);
			this.AWBPanel.Controls.Add(this.zLabel41);
			this.AWBPanel.Controls.Add(this.zLabel40);
			this.AWBPanel.Controls.Add(this.zLabel39);
			this.AWBPanel.Controls.Add(this.zLabel36);
			this.AWBPanel.Controls.Add(this.zLabel35);
			this.AWBPanel.Controls.Add(this.zLabel34);
			this.AWBPanel.Controls.Add(this.zLabel32);
			this.AWBPanel.Controls.Add(this.zLabel31);
			this.AWBPanel.Controls.Add(this.zLabel30);
			this.AWBPanel.Controls.Add(this.zLabel29);
			this.AWBPanel.Controls.Add(this.zLabel28);
			this.AWBPanel.Controls.Add(this.zLabel27);
			this.AWBPanel.Controls.Add(this.zLabel103);
			this.AWBPanel.Controls.Add(this.zLabel102);
			this.AWBPanel.Controls.Add(this.zLabel101);
			this.AWBPanel.Controls.Add(this.zLabel64);
			this.AWBPanel.Controls.Add(this.ECNCRNNumberTextBox);
			this.AWBPanel.Controls.Add(this.ReferenceNumberTextBox);
			this.AWBPanel.Controls.Add(this.TotalLineTotalsCalcEdit);
			this.AWBPanel.Controls.Add(this.TotalGrossWeightCalcEdit);
			this.AWBPanel.Controls.Add(this.TotalNoOfPiecesCalcEdit);
			this.AWBPanel.Controls.Add(this.TotalPrepaidCalcEdit);
			this.AWBPanel.Controls.Add(this.TotalCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox10);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl10);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit10);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit10);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit10);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit10);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit10);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit10);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl9);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox9);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit9);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit9);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit9);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit9);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit9);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit9);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl8);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox8);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit8);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit8);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit8);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit8);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit8);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit8);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl7);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox7);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit7);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit7);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit7);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit7);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit7);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit7);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl6);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox6);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit6);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit6);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit6);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit6);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit6);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit6);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl5);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox5);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit5);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit5);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit5);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit5);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit5);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit5);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl4);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox4);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit4);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit4);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit4);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit4);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit4);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit4);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl3);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox3);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit3);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit3);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit3);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit3);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit3);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit3);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl2);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox2);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit2);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit2);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit2);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit2);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit2);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit2);
			this.AWBPanel.Controls.Add(this.CommodityItemNumberControl1);
			this.AWBPanel.Controls.Add(this.NatureAndQtyOfGoodsTextBox1);
			this.AWBPanel.Controls.Add(this.RateClassDropEdit1);
			this.AWBPanel.Controls.Add(this.RateUQDropEdit1);
			this.AWBPanel.Controls.Add(this.LineTotalCalcEdit1);
			this.AWBPanel.Controls.Add(this.RateChargeCalcEdit1);
			this.AWBPanel.Controls.Add(this.ChargeableWeightCalcEdit1);
			this.AWBPanel.Controls.Add(this.GrossWeightCalcEdit1);
			this.AWBPanel.Controls.Add(this.AccountingInformationTabControl);
			this.AWBPanel.Controls.Add(this.zLabel18);
			this.AWBPanel.Controls.Add(this.BookingSecondFlightDateTextBox);
			this.AWBPanel.Controls.Add(this.BookingSecondFlightTextBox);
			this.AWBPanel.Controls.Add(this.BookingSecondCarrierTextBox);
			this.AWBPanel.Controls.Add(this.zLabel17);
			this.AWBPanel.Controls.Add(this.BookingFirstFlightDateTextBox);
			this.AWBPanel.Controls.Add(this.BookingFirstFlightTextBox);
			this.AWBPanel.Controls.Add(this.BookingFirstCarrierTextBox);
			this.AWBPanel.Controls.Add(this.AirlinePrefixTextBox);
			this.AWBPanel.Controls.Add(this.OriginCodeTextBox);
			this.AWBPanel.Controls.Add(this.SerialNumberTextBox);
			this.AWBPanel.Controls.Add(this.WeightChargePPDCalcEdit);
			this.AWBPanel.Controls.Add(this.WeightChargeCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.ValuationChargePPDCalcEdit);
			this.AWBPanel.Controls.Add(this.ValuationChargeCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.TaxChargeCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.TaxChargePPDCalcEdit);
			this.AWBPanel.Controls.Add(this.OtherChargeAgentCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.OtherChargeAgentPPDCalcEdit);
			this.AWBPanel.Controls.Add(this.OtherChargeCarrierCollectCalcEdit);
			this.AWBPanel.Controls.Add(this.OtherChargeCarrierPPDCalcEdit);
			this.AWBPanel.Controls.Add(this.InsuranceValueCalcEdit);
			this.AWBPanel.Controls.Add(this.CustomsValueCalcEdit);
			this.AWBPanel.Controls.Add(this.DeclaredValueCalcEdit);
			this.AWBPanel.Controls.Add(this.ShippersSignatureTextBox);
			this.AWBPanel.Controls.Add(this.AgentsSignatureTextBox);
			this.AWBPanel.Controls.Add(this.IssueCityTextBox);
			this.AWBPanel.Controls.Add(this.AWBIssueDateDateEdit);
			this.AWBPanel.Controls.Add(this.CurrencyTextBox);
			this.AWBPanel.Controls.Add(this.AgentCityTextBox);
			this.AWBPanel.Controls.Add(this.AgentsIATACodeTextBox);
			this.AWBPanel.Controls.Add(this.AgentsAccountNumberTextBox);
			this.AWBPanel.Controls.Add(this.AgentNameTextBox);
			this.AWBPanel.Controls.Add(this.HandlingInformationTextBox);
			this.AWBPanel.Controls.Add(this.OverrideRateSectionCheckBox);
			this.AWBPanel.Controls.Add(this.DestinationTextBox);
			this.AWBPanel.Controls.Add(this.AirportOfDepartureTextBox);
			this.AWBPanel.Controls.Add(this.ToFirstTextBox);
			this.AWBPanel.Controls.Add(this.ToSecondTextBox);
			this.AWBPanel.Controls.Add(this.BySecondTextBox);
			this.AWBPanel.Controls.Add(this.ToThirdTextBox);
			this.AWBPanel.Controls.Add(this.ByThirdTextBox);
			this.AWBPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBPanel.Name = "AWBPanel";
			this.AWBPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 1430, true);
			this.AWBPanel.TabIndex = 0;
			// 
			// ByFirstCarrierName
			// 
			this.BindingSource.SetBindingMember(this.ByFirstCarrierName, "AWBHeaderManager.EH_By1stAirlineName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_By1stAirlineName)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ByFirstCarrierName, false);
			this.ByFirstCarrierName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 488, true);
			this.ByFirstCarrierName.Name = "ByFirstCarrierName";
			this.ByFirstCarrierName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.ByFirstCarrierName.TabIndex = 230;
			// 
			// ByFirstCarrier
			// 
			this.BindingSource.SetBindingMember(this.ByFirstCarrier, "AWBHeaderManager.EH_By1st");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_By1st)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ByFirstCarrier, false);
			this.ByFirstCarrier.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 488, true);
			this.ByFirstCarrier.Name = "ByFirstCarrier";
			this.ByFirstCarrier.PreBoundMaxLength = 2;
			this.ByFirstCarrier.ShowDescriptionBox = false;
			this.ByFirstCarrier.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ByFirstCarrier.TabIndex = 39;
			// 
			// NoPieces11TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces11TextBox, "AWBHeaderManager.AWBRateLine11.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces11TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 910, true);
			this.NoPieces11TextBox.Name = "NoPieces11TextBox";
			this.NoPieces11TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces11TextBox.TabIndex = 183;
			// 
			// CommodityItemNumberControl11
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl11, "AWBHeaderManager.AWBRateLine11");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 910, true);
			this.CommodityItemNumberControl11.Name = "CommodityItemNumberControl11";
			this.CommodityItemNumberControl11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl11.TabIndex = 187;
			// 
			// RateClassDropEdit11
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit11, "AWBHeaderManager.AWBRateLine11.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 910, true);
			this.RateClassDropEdit11.Name = "RateClassDropEdit11";
			this.RateClassDropEdit11.PreBoundMaxLength = 1;
			this.RateClassDropEdit11.ShowDescriptionBox = false;
			this.RateClassDropEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit11.TabIndex = 186;
			// 
			// RateUQDropEdit11
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit11, "AWBHeaderManager.AWBRateLine11.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 910, true);
			this.RateUQDropEdit11.Name = "RateUQDropEdit11";
			this.RateUQDropEdit11.PreBoundMaxLength = 1;
			this.RateUQDropEdit11.ShowDescriptionBox = false;
			this.RateUQDropEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit11.TabIndex = 185;
			// 
			// LineTotalCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit11, "AWBHeaderManager.AWBRateLine11.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 910, true);
			this.LineTotalCalcEdit11.Name = "LineTotalCalcEdit11";
			this.LineTotalCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit11.TabIndex = 190;
			this.LineTotalCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit11, "AWBHeaderManager.AWBRateLine11.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit11, false);
			this.RateChargeCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 910, true);
			this.RateChargeCalcEdit11.Name = "RateChargeCalcEdit11";
			this.RateChargeCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit11.TabIndex = 189;
			this.RateChargeCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit11, "AWBHeaderManager.AWBRateLine11.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit11, false);
			this.ChargeableWeightCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 910, true);
			this.ChargeableWeightCalcEdit11.Name = "ChargeableWeightCalcEdit11";
			this.ChargeableWeightCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit11.TabIndex = 188;
			this.ChargeableWeightCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit11
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit11, "AWBHeaderManager.AWBRateLine11.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine11)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit11, false);
			this.GrossWeightCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 910, true);
			this.GrossWeightCalcEdit11.Name = "GrossWeightCalcEdit11";
			this.GrossWeightCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit11.TabIndex = 184;
			this.GrossWeightCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SCIDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SCIDropEdit, "AWBHeaderManager.EH_SpecialHandlingCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_SpecialHandlingCode)));
			this.SCIDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 620, true);
			this.SCIDropEdit.MaxItemsToShowInDropDown = 6;
			this.SCIDropEdit.Name = "SCIDropEdit";
			this.SCIDropEdit.PreBoundMaxLength = 3;
			this.SCIDropEdit.ShowDescriptionBox = false;
			this.SCIDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.SCIDropEdit.TabIndex = 81;
			// 
			// ChargesCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargesCodeDropEdit, "AWBHeaderManager.EH_ChargesCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ChargesCode)));
			this.ChargesCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 488, true);
			this.ChargesCodeDropEdit.Name = "ChargesCodeDropEdit";
			this.ChargesCodeDropEdit.PreBoundMaxLength = 2;
			this.ChargesCodeDropEdit.ShowDescriptionBox = false;
			this.ChargesCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ChargesCodeDropEdit.TabIndex = 51;
			// 
			// OtherChargesDropEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherChargesDropEdit, "AWBHeaderManager.EH_OtherPrepaidCollect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OtherPrepaidCollect)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherChargesDropEdit, false);
			this.OtherChargesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(623, 488, true);
			this.OtherChargesDropEdit.MaxItemsToShowInDropDown = 3;
			this.OtherChargesDropEdit.Name = "OtherChargesDropEdit";
			this.OtherChargesDropEdit.PreBoundMaxLength = 1;
			this.OtherChargesDropEdit.ShowDescriptionBox = false;
			this.OtherChargesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.OtherChargesDropEdit.TabIndex = 55;
			// 
			// WeightValueDropEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightValueDropEdit, "AWBHeaderManager.EH_WeightPrepaidCollect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_WeightPrepaidCollect)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WeightValueDropEdit, false);
			this.WeightValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 488, true);
			this.WeightValueDropEdit.MaxItemsToShowInDropDown = 3;
			this.WeightValueDropEdit.Name = "WeightValueDropEdit";
			this.WeightValueDropEdit.PreBoundMaxLength = 1;
			this.WeightValueDropEdit.ShowDescriptionBox = false;
			this.WeightValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.WeightValueDropEdit.TabIndex = 53;
			// 
			// NoPieces1TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces1TextBox, "AWBHeaderManager.AWBRateLine1.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 692, true);
			this.NoPieces1TextBox.Name = "NoPieces1TextBox";
			this.NoPieces1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces1TextBox.TabIndex = 93;
			// 
			// NoPieces2TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces2TextBox, "AWBHeaderManager.AWBRateLine2.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 714, true);
			this.NoPieces2TextBox.Name = "NoPieces2TextBox";
			this.NoPieces2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces2TextBox.TabIndex = 102;
			// 
			// NoPieces3TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces3TextBox, "AWBHeaderManager.AWBRateLine3.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 736, true);
			this.NoPieces3TextBox.Name = "NoPieces3TextBox";
			this.NoPieces3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces3TextBox.TabIndex = 111;
			// 
			// NoPieces4TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces4TextBox, "AWBHeaderManager.AWBRateLine4.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 758, true);
			this.NoPieces4TextBox.Name = "NoPieces4TextBox";
			this.NoPieces4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces4TextBox.TabIndex = 120;
			// 
			// NoPieces5TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces5TextBox, "AWBHeaderManager.AWBRateLine5.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces5TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 780, true);
			this.NoPieces5TextBox.Name = "NoPieces5TextBox";
			this.NoPieces5TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces5TextBox.TabIndex = 129;
			// 
			// NoPieces6TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces6TextBox, "AWBHeaderManager.AWBRateLine6.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces6TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 802, true);
			this.NoPieces6TextBox.Name = "NoPieces6TextBox";
			this.NoPieces6TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces6TextBox.TabIndex = 138;
			// 
			// NoPieces7TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces7TextBox, "AWBHeaderManager.AWBRateLine7.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces7TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 824, true);
			this.NoPieces7TextBox.Name = "NoPieces7TextBox";
			this.NoPieces7TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces7TextBox.TabIndex = 147;
			// 
			// NoPieces8TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces8TextBox, "AWBHeaderManager.AWBRateLine8.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces8TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 846, true);
			this.NoPieces8TextBox.Name = "NoPieces8TextBox";
			this.NoPieces8TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces8TextBox.TabIndex = 156;
			// 
			// NoPieces9TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces9TextBox, "AWBHeaderManager.AWBRateLine9.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces9TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 868, true);
			this.NoPieces9TextBox.Name = "NoPieces9TextBox";
			this.NoPieces9TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces9TextBox.TabIndex = 165;
			// 
			// NoPieces10TextBox
			// 
			this.BindingSource.SetBindingMember(this.NoPieces10TextBox, "AWBHeaderManager.AWBRateLine10.ER_NoOfPiecesOrRCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_NoOfPiecesOrRCP)));
			this.NoPieces10TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 889, true);
			this.NoPieces10TextBox.Name = "NoPieces10TextBox";
			this.NoPieces10TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.NoPieces10TextBox.TabIndex = 174;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "AWBHeaderManager.EH_ExtraShipperInfoLine1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ExtraShipperInfoLine1)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox3, false);
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1164, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.zTextBox3.TabIndex = 209;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "AWBHeaderManager.EH_ExtraShipperInfoLine2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ExtraShipperInfoLine2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1186, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.zTextBox2.TabIndex = 210;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "AWBHeaderManager.EH_ExtraCarrierInfoLine2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ExtraCarrierInfoLine2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1264, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.zTextBox1.TabIndex = 217;
			// 
			// zLabel7
			// 
			this.zLabel7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d3daca5b-1374-4df1-a43b-fa7a56803306", "Shippers Load and Count (FWB/FHL)");
			this.zLabel7.IsFontBold = true;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 953, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 36, true);
			this.zLabel7.TabIndex = 195;
			// 
			// EH_ShippingLoadAndCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EH_ShippingLoadAndCountCalcEdit, "AWBHeaderManager.EH_ShippingLoadAndCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShippingLoadAndCount)));
			this.EH_ShippingLoadAndCountCalcEdit.Decimals = 0;
			this.EH_ShippingLoadAndCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 960, true);
			this.EH_ShippingLoadAndCountCalcEdit.Name = "EH_ShippingLoadAndCountCalcEdit";
			this.EH_ShippingLoadAndCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.EH_ShippingLoadAndCountCalcEdit.TabIndex = 196;
			this.EH_ShippingLoadAndCountCalcEdit.Text = "0";
			this.EH_ShippingLoadAndCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EH_AirportOfDestinationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_AirportOfDestinationCodeTextBox, "AWBHeaderManager.EH_AirportOfDestinationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AirportOfDestinationCode)));
			this.EH_AirportOfDestinationCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|068b64ba-3287-448d-b701-327aaddc45c2", "Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.EH_AirportOfDestinationCodeTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.EH_AirportOfDestinationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 532, true);
			this.EH_AirportOfDestinationCodeTextBox.Name = "EH_AirportOfDestinationCodeTextBox";
			this.EH_AirportOfDestinationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.EH_AirportOfDestinationCodeTextBox.TabIndex = 63;
			// 
			// EH_OptionalShippingInformation2TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_OptionalShippingInformation2TextBox, "AWBHeaderManager.EH_OptionalShippingInformation2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OptionalShippingInformation2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EH_OptionalShippingInformation2TextBox, false);
			this.EH_OptionalShippingInformation2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(838, 444, true);
			this.EH_OptionalShippingInformation2TextBox.Name = "EH_OptionalShippingInformation2TextBox";
			this.EH_OptionalShippingInformation2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.EH_OptionalShippingInformation2TextBox.TabIndex = 35;
			// 
			// EH_OptionalShippingInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_OptionalShippingInformationTextBox, "AWBHeaderManager.EH_OptionalShippingInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OptionalShippingInformation)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EH_OptionalShippingInformationTextBox, false);
			this.EH_OptionalShippingInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 444, true);
			this.EH_OptionalShippingInformationTextBox.Name = "EH_OptionalShippingInformationTextBox";
			this.EH_OptionalShippingInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.EH_OptionalShippingInformationTextBox.TabIndex = 34;
			// 
			// NatureAndQtyOfGoodsTextBox12
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox12, "AWBHeaderManager.AWBRateLine12");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox12, false);
			this.NatureAndQtyOfGoodsTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 931, true);
			this.NatureAndQtyOfGoodsTextBox12.Name = "NatureAndQtyOfGoodsTextBox12";
			this.NatureAndQtyOfGoodsTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox12.TabIndex = 192;
			// 
			// NatureAndQtyOfGoodsTextBox11
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox11, "AWBHeaderManager.AWBRateLine11");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox11, false);
			this.NatureAndQtyOfGoodsTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 910, true);
			this.NatureAndQtyOfGoodsTextBox11.Name = "NatureAndQtyOfGoodsTextBox11";
			this.NatureAndQtyOfGoodsTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox11.TabIndex = 191;
			// 
			// EH_IsNotifyOverridenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EH_IsNotifyOverridenCheckBox, "AWBHeaderManager.EH_IsNotifyOverriden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IsNotifyOverriden)));
			this.EH_IsNotifyOverridenCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6b52fea0-68dd-499f-b43b-6114750d1f41", "Override Address");
			this.EH_IsNotifyOverridenCheckBox.CheckedChanged += new System.EventHandler(this.SetNotifyAddressOverrideTab_CheckedChanged);
			this.EH_IsNotifyOverridenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EH_IsNotifyOverridenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 271, true);
			this.EH_IsNotifyOverridenCheckBox.Name = "EH_IsNotifyOverridenCheckBox";
			this.EH_IsNotifyOverridenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.EH_IsNotifyOverridenCheckBox.TabIndex = 18;
			this.EH_IsNotifyOverridenCheckBox.UseVisualStyleBackColor = false;
			// 
			// OverrideConsigneeAddressCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideConsigneeAddressCheckBox, "AWBHeaderManager.EH_IsConsigneeOverriden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IsConsigneeOverriden)));
			this.OverrideConsigneeAddressCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|15483bce-b1d3-46b9-a32c-9f74fcd9697e", "Override Address");
			this.OverrideConsigneeAddressCheckBox.CheckedChanged += new System.EventHandler(this.SetConsigneeAddressOverrideTab_CheckedChanged);
			this.OverrideConsigneeAddressCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideConsigneeAddressCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 165, true);
			this.OverrideConsigneeAddressCheckBox.Name = "OverrideConsigneeAddressCheckBox";
			this.OverrideConsigneeAddressCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.OverrideConsigneeAddressCheckBox.TabIndex = 8;
			this.OverrideConsigneeAddressCheckBox.UseVisualStyleBackColor = false;
			// 
			// ConsigneeNameAndAddressTabControl
			// 
			this.ConsigneeNameAndAddressTabControl.Controls.Add(this.ConsigneeAddressTabControl);
			this.ConsigneeNameAndAddressTabControl.Controls.Add(this.ConsigneeOverrideTabPage);
			this.ConsigneeNameAndAddressTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 164, true);
			this.ConsigneeNameAndAddressTabControl.Name = "ConsigneeNameAndAddressTabControl";
			this.ConsigneeNameAndAddressTabControl.SelectedIndex = 0;
			this.ConsigneeNameAndAddressTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 132, true);
			this.ConsigneeNameAndAddressTabControl.TabIndex = 7;
			// 
			// ConsigneeAddressTabControl
			// 
			this.ConsigneeAddressTabControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|fd26576b-4496-4525-9c60-d89025351e1d", "Consignee\'s Name and Address");
			this.ConsigneeAddressTabControl.Controls.Add(this.panel1);
			this.ConsigneeAddressTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeAddressTabControl.Name = "ConsigneeAddressTabControl";
			this.ConsigneeAddressTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 105, true);
			this.ConsigneeAddressTabControl.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.AutoScroll = true;
			this.panel1.Controls.Add(this.IsDeclarantForAdvanceCargoReportingCheckBox);
			this.panel1.Controls.Add(this.ConsigneeTraderTypeTextBox);
			this.panel1.Controls.Add(this.EH_ConsigneePostCodeTextBox);
			this.panel1.Controls.Add(this.ConsigneeCountryTextBox);
			this.panel1.Controls.Add(this.ConsigneeTraderNoTextBox);
			this.panel1.Controls.Add(this.ConsigneeContactDetailTextBox);
			this.panel1.Controls.Add(this.ConsigneeContactNameTextBox);
			this.panel1.Controls.Add(this.ChooseConsigneeAddressDropEdit);
			this.panel1.Controls.Add(this.ConsigneeAddressSaveButton);
			this.panel1.Controls.Add(this.zTextBox5);
			this.panel1.Controls.Add(this.ConsigneeAddressTextBox);
			this.panel1.Controls.Add(this.zLabel26);
			this.panel1.Controls.Add(this.ConsigneeContactCodeDropEdit);
			this.panel1.Controls.Add(this.ConsigneeAccountTextBox);
			this.panel1.Controls.Add(this.ConsigneeNameTextBox);
			this.panel1.Controls.Add(this.ConsigneeCityTextBox);
			this.panel1.Controls.Add(this.ConsigneeStateTextBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 105, true);
			this.panel1.TabIndex = 19;
			// 
			// EH_ConsigneePostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneePostCodeTextBox, "AWBHeaderManager.EH_ConsigneePostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneePostCode)));
			this.EH_ConsigneePostCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|48556e5b-c74d-4391-a493-68cd21e0a9c6", "P/Code");
			this.EH_ConsigneePostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 84, true);
			this.EH_ConsigneePostCodeTextBox.Name = "EH_ConsigneePostCodeTextBox";
			this.EH_ConsigneePostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.EH_ConsigneePostCodeTextBox.TabIndex = 14;
			// 
			// ConsigneeCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeCountryTextBox, "AWBHeaderManager.EH_ConsigneeCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeCountryCode)));
			this.ConsigneeCountryTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d6b7e8e1-94ee-470e-9848-ed66f9819f64", "Country/Region");
			this.ConsigneeCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 105, true);
			this.ConsigneeCountryTextBox.Name = "ConsigneeCountryTextBox";
			this.ConsigneeCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.ConsigneeCountryTextBox.TabIndex = 17;
			// 
			// ConsigneeTraderTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeTraderTypeTextBox, "AWBHeaderManager.EH_ConsigneeTraderNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeTraderNoType)));
			this.ConsigneeTraderTypeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|0c5b7548-ab5e-4799-bc8e-a09e3ffb669d", "Comp. ID");
			this.ConsigneeTraderTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 126, true);
			this.ConsigneeTraderTypeTextBox.Name = "ConsigneeTraderTypeTextBox";
			this.ConsigneeTraderTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.ConsigneeTraderTypeTextBox.TabIndex = 21;
			// 
			// ConsigneeTraderNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeTraderNoTextBox, "AWBHeaderManager.EH_ConsigneeTraderNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeTraderNo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeTraderNoTextBox, false);
			this.ConsigneeTraderNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 126, true);
			this.ConsigneeTraderNoTextBox.Name = "ConsigneeTraderNoTextBox";
			this.ConsigneeTraderNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ConsigneeTraderNoTextBox.TabIndex = 23;
			//
			// IsDeclarantForAdvanceCargoReportingCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsDeclarantForAdvanceCargoReportingCheckBox, "AWBHeaderManager.EH_IsConsigneeDeclarantForAdvanceCargoReporting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IsConsigneeDeclarantForAdvanceCargoReporting)));
			this.IsDeclarantForAdvanceCargoReportingCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|1ed441fd-e600-4fd9-8f8d-5ef3c7e6cbc8", "Is Declarant for Advance Cargo Reporting");
			this.IsDeclarantForAdvanceCargoReportingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDeclarantForAdvanceCargoReportingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 147, true);
			this.IsDeclarantForAdvanceCargoReportingCheckBox.Name = "IsDeclarantForAdvanceCargoReportingCheckBox";
			this.IsDeclarantForAdvanceCargoReportingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.IsDeclarantForAdvanceCargoReportingCheckBox.TabIndex = 24;
			this.IsDeclarantForAdvanceCargoReportingCheckBox.UseVisualStyleBackColor = false;
			// 
			// ConsigneeContactDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeContactDetailTextBox, "AWBHeaderManager.EH_ConsigneeContactDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeContactDetail)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeContactDetailTextBox, false);
			this.ConsigneeContactDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 105, true);
			this.ConsigneeContactDetailTextBox.Name = "ConsigneeContactDetailTextBox";
			this.ConsigneeContactDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ConsigneeContactDetailTextBox.TabIndex = 20;
			// 
			// ConsigneeContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeContactNameTextBox, "AWBHeaderManager.EH_ConsigneeContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeContactName)));
			this.ConsigneeContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 105, true);
			this.ConsigneeContactNameTextBox.Name = "ConsigneeContactNameTextBox";
			this.ConsigneeContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.ConsigneeContactNameTextBox.TabIndex = 16;
			// 
			// ChooseConsigneeAddressDropEdit
			// 
			this.ChooseConsigneeAddressDropEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ChooseConsigneeAddressDropEdit, "AWBHeaderManager.EH_ConsigneeDefaultAddressPicker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeDefaultAddressPicker)));
			this.ChooseConsigneeAddressDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|7c82b24d-12a8-4563-b175-46caa971e511", "Choose");
			this.ChooseConsigneeAddressDropEdit.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.ChooseConsigneeAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.ChooseConsigneeAddressDropEdit.Name = "ChooseConsigneeAddressDropEdit";
			this.ChooseConsigneeAddressDropEdit.PreBoundMaxLength = 30;
			this.ChooseConsigneeAddressDropEdit.ShowDescriptionBox = false;
			this.ChooseConsigneeAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ChooseConsigneeAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ChooseConsigneeAddressDropEdit.TabIndex = 1;
			// 
			// ConsigneeAddressSaveButton
			// 
			this.SetDataSourceBinding(this.ConsigneeAddressSaveButton, "IsEnabledForBinding", "IsAWBValuesOverriddenProperty");
			this.ConsigneeAddressSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
			this.ConsigneeAddressSaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6d630b96-3a0d-423b-a49b-03c2e8ba5537", "Save");
			this.ConsigneeAddressSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 0, true);
			this.ConsigneeAddressSaveButton.Name = "ConsigneeAddressSaveButton";
			this.ConsigneeAddressSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ConsigneeAddressSaveButton.TabIndex = 23;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "AWBHeaderManager.EH_ConsigneeAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeAddress2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox5, false);
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 63, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.zTextBox5.TabIndex = 8;
			this.zTextBox5.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// ConsigneeAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAddressTextBox, "AWBHeaderManager.EH_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeAddress)));
			this.ConsigneeAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 42, true);
			this.ConsigneeAddressTextBox.Name = "ConsigneeAddressTextBox";
			this.ConsigneeAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.ConsigneeAddressTextBox.TabIndex = 7;
			this.ConsigneeAddressTextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// zLabel26
			// 
			this.zLabel26.AutoSize = true;
			this.zLabel26.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|7a08a31d-5e87-4492-b364-d8d734756c4d", "Account No.");
			this.zLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 21, true);
			this.zLabel26.Name = "zLabel26";
			this.zLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.zLabel26.TabIndex = 4;
			// 
			// ConsigneeContactCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeContactCodeDropEdit, "AWBHeaderManager.EH_ConsigneeContactCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeContactCode)));
			this.ConsigneeContactCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 105, true);
			this.ConsigneeContactCodeDropEdit.Name = "ConsigneeContactCodeDropEdit";
			this.ConsigneeContactCodeDropEdit.PreBoundMaxLength = 3;
			this.ConsigneeContactCodeDropEdit.ShowDescriptionBox = false;
			this.ConsigneeContactCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ConsigneeContactCodeDropEdit.TabIndex = 19;
			// 
			// ConsigneeAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAccountTextBox, "AWBHeaderManager.EH_ConsigneeAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeAccount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeAccountTextBox, false);
			this.ConsigneeAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 21, true);
			this.ConsigneeAccountTextBox.Name = "ConsigneeAccountTextBox";
			this.ConsigneeAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.ConsigneeAccountTextBox.TabIndex = 5;
			// 
			// ConsigneeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeNameTextBox, "AWBHeaderManager.EH_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeName)));
			this.ConsigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 21, true);
			this.ConsigneeNameTextBox.Name = "ConsigneeNameTextBox";
			this.ConsigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 20, true);
			this.ConsigneeNameTextBox.TabIndex = 3;
			this.ConsigneeNameTextBox.TextChanged += new System.EventHandler(this.ShipperNameTextBox_TextChanged);

			// 
			// ConsigneeCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeCityTextBox, "AWBHeaderManager.EH_ConsigneePlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneePlace)));
			this.ConsigneeCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 84, true);
			this.ConsigneeCityTextBox.Name = "ConsigneeCityTextBox";
			this.ConsigneeCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ConsigneeCityTextBox.TabIndex = 10;
			// 
			// ConsigneeStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeStateTextBox, "AWBHeaderManager.EH_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeState)));
			this.ConsigneeStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 84, true);
			this.ConsigneeStateTextBox.Name = "ConsigneeStateTextBox";
			this.ConsigneeStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.ConsigneeStateTextBox.TabIndex = 12;
			// 
			// ConsigneeOverrideTabPage
			// 
			this.ConsigneeOverrideTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|3c616a08-d7be-404c-acb8-c2c7a4c2b3ad", "Override");
			this.ConsigneeOverrideTabPage.Controls.Add(this.EH_ConsigneeOverride5TextBox);
			this.ConsigneeOverrideTabPage.Controls.Add(this.EH_ConsigneeOverride4TextBox);
			this.ConsigneeOverrideTabPage.Controls.Add(this.EH_ConsigneeOverride3TextBox);
			this.ConsigneeOverrideTabPage.Controls.Add(this.EH_ConsigneeOverride2TextBox);
			this.ConsigneeOverrideTabPage.Controls.Add(this.EH_ConsigneeOverride1TextBox);
			this.ConsigneeOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeOverrideTabPage.Name = "ConsigneeOverrideTabPage";
			this.ConsigneeOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 105, true);
			this.ConsigneeOverrideTabPage.TabIndex = 1;
			// 
			// EH_ConsigneeOverride5TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneeOverride5TextBox, "AWBHeaderManager.EH_ConsigneeOverride5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeOverride5)));
			this.EH_ConsigneeOverride5TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|55361688-1581-432a-9273-0b0e4f9f6490", "Line 5");
			this.EH_ConsigneeOverride5TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 85, true);
			this.EH_ConsigneeOverride5TextBox.Name = "EH_ConsigneeOverride5TextBox";
			this.EH_ConsigneeOverride5TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ConsigneeOverride5TextBox.TabIndex = 9;
			this.EH_ConsigneeOverride5TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ConsigneeOverride4TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneeOverride4TextBox, "AWBHeaderManager.EH_ConsigneeOverride4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeOverride4)));
			this.EH_ConsigneeOverride4TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d5ae1789-0cb4-47c9-a9fa-4160c2287a27", "Line 4");
			this.EH_ConsigneeOverride4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 64, true);
			this.EH_ConsigneeOverride4TextBox.Name = "EH_ConsigneeOverride4TextBox";
			this.EH_ConsigneeOverride4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ConsigneeOverride4TextBox.TabIndex = 7;
			this.EH_ConsigneeOverride4TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ConsigneeOverride3TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneeOverride3TextBox, "AWBHeaderManager.EH_ConsigneeOverride3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeOverride3)));
			this.EH_ConsigneeOverride3TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|af173395-4ed9-4aa2-be2f-5ea8d3deb03c", "Line 3");
			this.EH_ConsigneeOverride3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 43, true);
			this.EH_ConsigneeOverride3TextBox.Name = "EH_ConsigneeOverride3TextBox";
			this.EH_ConsigneeOverride3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ConsigneeOverride3TextBox.TabIndex = 5;
			this.EH_ConsigneeOverride3TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ConsigneeOverride2TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneeOverride2TextBox, "AWBHeaderManager.EH_ConsigneeOverride2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeOverride2)));
			this.EH_ConsigneeOverride2TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|aeaf6ae5-2b83-41b0-90e7-27e71b971ce4", "Line 2");
			this.EH_ConsigneeOverride2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 22, true);
			this.EH_ConsigneeOverride2TextBox.Name = "EH_ConsigneeOverride2TextBox";
			this.EH_ConsigneeOverride2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ConsigneeOverride2TextBox.TabIndex = 3;
			this.EH_ConsigneeOverride2TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ConsigneeOverride1TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ConsigneeOverride1TextBox, "AWBHeaderManager.EH_ConsigneeOverride1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsigneeOverride1)));
			this.EH_ConsigneeOverride1TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|788de5cf-daa1-4f91-ba55-f755148941b0", "Line 1");
			this.EH_ConsigneeOverride1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1, true);
			this.EH_ConsigneeOverride1TextBox.Name = "EH_ConsigneeOverride1TextBox";
			this.EH_ConsigneeOverride1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ConsigneeOverride1TextBox.TabIndex = 1;
			this.EH_ConsigneeOverride1TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// OverrideShipperAddressCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideShipperAddressCheckBox, "AWBHeaderManager.EH_IsShipperOverriden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IsShipperOverriden)));
			this.OverrideShipperAddressCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|100809e2-cc1b-4579-b6a4-a57394eca691", "Override Address");
			this.OverrideShipperAddressCheckBox.CheckedChanged += new System.EventHandler(this.SetShipperAddressOverrideTab_CheckedChanged);
			this.OverrideShipperAddressCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideShipperAddressCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 34, true);
			this.OverrideShipperAddressCheckBox.Name = "OverrideShipperAddressCheckBox";
			this.OverrideShipperAddressCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.OverrideShipperAddressCheckBox.TabIndex = 6;
			this.OverrideShipperAddressCheckBox.UseVisualStyleBackColor = false;
			// 
			// ShipperAddressTabControl
			// 
			this.ShipperAddressTabControl.Controls.Add(this.ShipperNameAndAddressTabPage);
			this.ShipperAddressTabControl.Controls.Add(this.ShipperNameAndAddressOverrideTabPage);
			this.ShipperAddressTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 32, true);
			this.ShipperAddressTabControl.Name = "ShipperAddressTabControl";
			this.ShipperAddressTabControl.SelectedIndex = 0;
			this.ShipperAddressTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 133, true);
			this.ShipperAddressTabControl.TabIndex = 5;
			// 
			// ShipperNameAndAddressTabPage
			// 
			this.ShipperNameAndAddressTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4a24ea0c-6ea1-400f-9adb-20819c21487a", "Shipper\'s Name and Address");
			this.ShipperNameAndAddressTabPage.Controls.Add(this.ShippersPanel);
			this.ShipperNameAndAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipperNameAndAddressTabPage.Name = "ShipperNameAndAddressTabPage";
			this.ShipperNameAndAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 106, true);
			this.ShipperNameAndAddressTabPage.TabIndex = 0;
			// 
			// ShippersPanel
			// 
			this.ShippersPanel.AutoScroll = true;
			this.ShippersPanel.Controls.Add(this.zLabel1);
			this.ShippersPanel.Controls.Add(this.ShipperTraderTypeTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperContactNameTextBox);
			this.ShippersPanel.Controls.Add(this.EH_ShipperContactCodeDropEdit);
			this.ShippersPanel.Controls.Add(this.ShipperContactDetailTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperTraderNoTextBox);
			this.ShippersPanel.Controls.Add(this.ChooseShipperAddressDropEdit);
			this.ShippersPanel.Controls.Add(this.ShipperAddressSaveButton);
			this.ShippersPanel.Controls.Add(this.EH_ShipperPostCodeTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperCountryTextBox);
			this.ShippersPanel.Controls.Add(this.EH_ShipperAccountTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperStateTextBox);
			this.ShippersPanel.Controls.Add(this.zTextBox4);
			this.ShippersPanel.Controls.Add(this.ShipperAddressTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperNameTextBox);
			this.ShippersPanel.Controls.Add(this.ShipperCityTextBox);
			this.ShippersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShippersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShippersPanel.Name = "ShippersPanel";
			this.ShippersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 106, true);
			this.ShippersPanel.TabIndex = 19;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d5226454-a137-484c-bcc0-7ca060b348d5", "Account No.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 24, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.zLabel1.TabIndex = 21;
			// 
			// EH_ShipperContactCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperContactCodeDropEdit, "AWBHeaderManager.EH_ShipperContactCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperContactCode)));
			this.EH_ShipperContactCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 105, true);
			this.EH_ShipperContactCodeDropEdit.Name = "EH_ShipperContactCodeDropEdit";
			this.EH_ShipperContactCodeDropEdit.PreBoundMaxLength = 3;
			this.EH_ShipperContactCodeDropEdit.ShowDescriptionBox = false;
			this.EH_ShipperContactCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.EH_ShipperContactCodeDropEdit.TabIndex = 19;
			// 
			// ShipperContactDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperContactDetailTextBox, "AWBHeaderManager.EH_ShipperContactDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperContactDetail)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipperContactDetailTextBox, false);
			this.ShipperContactDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 105, true);
			this.ShipperContactDetailTextBox.Name = "ShipperContactDetailTextBox";
			this.ShipperContactDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ShipperContactDetailTextBox.TabIndex = 20;
			// 
			// ShipperContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperContactNameTextBox, "AWBHeaderManager.EH_ShipperContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperContactName)));
			this.ShipperContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 105, true);
			this.ShipperContactNameTextBox.Name = "ShipperContactNameTextBox";
			this.ShipperContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.ShipperContactNameTextBox.TabIndex = 16;
			// 
			// ShipperTraderTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperTraderTypeTextBox, "AWBHeaderManager.EH_ShipperTraderNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperTraderNoType)));
			this.ShipperTraderTypeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|b2ee46e7-dcf7-492d-97b6-f631119a9881", "Comp. ID");
			this.ShipperTraderTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 126, true);
			this.ShipperTraderTypeTextBox.Name = "ShipperTraderTypeTextBox";
			this.ShipperTraderTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.ShipperTraderTypeTextBox.TabIndex = 22;
			// 
			// ShipperTraderNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperTraderNoTextBox, "AWBHeaderManager.EH_ShipperTraderNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperTraderNo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipperTraderNoTextBox, false);
			this.ShipperTraderNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 126, true);
			this.ShipperTraderNoTextBox.Name = "ShipperTraderNoTextBox";
			this.ShipperTraderNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ShipperTraderNoTextBox.TabIndex = 23;
			// 
			// ChooseShipperAddressDropEdit
			// 
			this.ChooseShipperAddressDropEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ChooseShipperAddressDropEdit, "AWBHeaderManager.EH_ShipperDefaultAddressPicker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperDefaultAddressPicker)));
			this.ChooseShipperAddressDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d52c1d87-2710-47c8-a3bc-710ab6d6d9dd", "Choose");
			this.ChooseShipperAddressDropEdit.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.ChooseShipperAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.ChooseShipperAddressDropEdit.Name = "ChooseShipperAddressDropEdit";
			this.ChooseShipperAddressDropEdit.PreBoundMaxLength = 30;
			this.ChooseShipperAddressDropEdit.ShowDescriptionBox = false;
			this.ChooseShipperAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ChooseShipperAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ChooseShipperAddressDropEdit.TabIndex = 1;
			// 
			// ShipperAddressSaveButton
			// 
			this.SetDataSourceBinding(this.ShipperAddressSaveButton, "IsEnabledForBinding", "IsAWBValuesOverriddenProperty");
			this.ShipperAddressSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
			this.ShipperAddressSaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|8b9a3f8d-7817-4a01-a49b-062f076ba232", "Save");
			this.ShipperAddressSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 0, true);
			this.ShipperAddressSaveButton.Name = "ShipperAddressSaveButton";
			this.ShipperAddressSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ShipperAddressSaveButton.TabIndex = 24;
			// 
			// EH_ShipperPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperPostCodeTextBox, "AWBHeaderManager.EH_ShipperPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperPostCode)));
			this.EH_ShipperPostCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6638ce9a-dca4-4dd3-aabb-dea6663e624d", "P/Code");
			this.EH_ShipperPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 84, true);
			this.EH_ShipperPostCodeTextBox.Name = "EH_ShipperPostCodeTextBox";
			this.EH_ShipperPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.EH_ShipperPostCodeTextBox.TabIndex = 14;
			// 
			// ShipperCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperCountryTextBox, "AWBHeaderManager.EH_ShipperCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperCountryCode)));
			this.ShipperCountryTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e949f035-9aee-4d7c-b9fd-df5331d87bc7", "Country/Region");
			this.ShipperCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 105, true);
			this.ShipperCountryTextBox.Name = "ShipperCountryTextBox";
			this.ShipperCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.ShipperCountryTextBox.TabIndex = 17;
			// 
			// EH_ShipperAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperAccountTextBox, "AWBHeaderManager.EH_ShipperAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperAccount)));
			this.EH_ShipperAccountTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|99bf781a-5836-494f-92f1-6bb8d6029d99", "Account No.");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EH_ShipperAccountTextBox, false);
			this.EH_ShipperAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 21, true);
			this.EH_ShipperAccountTextBox.Name = "EH_ShipperAccountTextBox";
			this.EH_ShipperAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.EH_ShipperAccountTextBox.TabIndex = 5;
			// 
			// ShipperStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperStateTextBox, "AWBHeaderManager.EH_ShipperState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperState)));
			this.ShipperStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 84, true);
			this.ShipperStateTextBox.Name = "ShipperStateTextBox";
			this.ShipperStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.ShipperStateTextBox.TabIndex = 12;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "AWBHeaderManager.EH_ShipperAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperAddress2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox4, false);
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 63, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.zTextBox4.TabIndex = 8;
			this.zTextBox4.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// ShipperAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperAddressTextBox, "AWBHeaderManager.EH_ShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperAddress)));
			this.ShipperAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 42, true);
			this.ShipperAddressTextBox.Name = "ShipperAddressTextBox";
			this.ShipperAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.ShipperAddressTextBox.TabIndex = 7;
			this.ShipperAddressTextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// ShipperNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperNameTextBox, "AWBHeaderManager.EH_ShipperName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperName)));
			this.ShipperNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 21, true);
			this.ShipperNameTextBox.Name = "ShipperNameTextBox";
			this.ShipperNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 20, true);
			this.ShipperNameTextBox.TabIndex = 3;
			this.ShipperNameTextBox.TextChanged += new System.EventHandler(this.ShipperNameTextBox_TextChanged);
			// 
			// ShipperCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperCityTextBox, "AWBHeaderManager.EH_ShipperPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperPlace)));
			this.ShipperCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 84, true);
			this.ShipperCityTextBox.Name = "ShipperCityTextBox";
			this.ShipperCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.ShipperCityTextBox.TabIndex = 10;
			// 
			// ShipperNameAndAddressOverrideTabPage
			// 
			this.ShipperNameAndAddressOverrideTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e9e1664b-fed6-40f0-a578-b60fc47bd840", "Override");
			this.ShipperNameAndAddressOverrideTabPage.Controls.Add(this.EH_ShipperOverride5TextBox);
			this.ShipperNameAndAddressOverrideTabPage.Controls.Add(this.EH_ShipperOverride4TextBox);
			this.ShipperNameAndAddressOverrideTabPage.Controls.Add(this.EH_ShipperOverride3TextBox);
			this.ShipperNameAndAddressOverrideTabPage.Controls.Add(this.EH_ShipperOverride2TextBox);
			this.ShipperNameAndAddressOverrideTabPage.Controls.Add(this.EH_ShipperOverride1TextBox);
			this.ShipperNameAndAddressOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipperNameAndAddressOverrideTabPage.Name = "ShipperNameAndAddressOverrideTabPage";
			this.ShipperNameAndAddressOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 106, true);
			this.ShipperNameAndAddressOverrideTabPage.TabIndex = 1;
			// 
			// EH_ShipperOverride5TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperOverride5TextBox, "AWBHeaderManager.EH_ShipperOverride5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperOverride5)));
			this.EH_ShipperOverride5TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|2b904210-fa32-4f34-8029-c59402e658c1", "Line 5");
			this.EH_ShipperOverride5TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 85, true);
			this.EH_ShipperOverride5TextBox.Name = "EH_ShipperOverride5TextBox";
			this.EH_ShipperOverride5TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ShipperOverride5TextBox.TabIndex = 9;
			this.EH_ShipperOverride5TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ShipperOverride4TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperOverride4TextBox, "AWBHeaderManager.EH_ShipperOverride4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperOverride4)));
			this.EH_ShipperOverride4TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e3695d02-5442-4196-ba33-9e051704af93", "Line 4");
			this.EH_ShipperOverride4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 64, true);
			this.EH_ShipperOverride4TextBox.Name = "EH_ShipperOverride4TextBox";
			this.EH_ShipperOverride4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ShipperOverride4TextBox.TabIndex = 7;
			this.EH_ShipperOverride4TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ShipperOverride3TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperOverride3TextBox, "AWBHeaderManager.EH_ShipperOverride3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperOverride3)));
			this.EH_ShipperOverride3TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|c09f91d1-920b-4afe-9b50-e5110da9111d", "Line 3");
			this.EH_ShipperOverride3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 43, true);
			this.EH_ShipperOverride3TextBox.Name = "EH_ShipperOverride3TextBox";
			this.EH_ShipperOverride3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ShipperOverride3TextBox.TabIndex = 5;
			this.EH_ShipperOverride3TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ShipperOverride2TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperOverride2TextBox, "AWBHeaderManager.EH_ShipperOverride2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperOverride2)));
			this.EH_ShipperOverride2TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|578197c6-e5fe-451b-b34d-15b480192bff", "Line 2");
			this.EH_ShipperOverride2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 22, true);
			this.EH_ShipperOverride2TextBox.Name = "EH_ShipperOverride2TextBox";
			this.EH_ShipperOverride2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ShipperOverride2TextBox.TabIndex = 3;
			this.EH_ShipperOverride2TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_ShipperOverride1TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_ShipperOverride1TextBox, "AWBHeaderManager.EH_ShipperOverride1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShipperOverride1)));
			this.EH_ShipperOverride1TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f8afd6b4-88b1-4c7b-98da-2b4bf33e5e65", "Line 1");
			this.EH_ShipperOverride1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1, true);
			this.EH_ShipperOverride1TextBox.Name = "EH_ShipperOverride1TextBox";
			this.EH_ShipperOverride1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_ShipperOverride1TextBox.TabIndex = 1;
			this.EH_ShipperOverride1TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_AgentApprovedExporterNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_AgentApprovedExporterNumberTextBox, "AWBHeaderManager.EH_AgentApprovedExporterNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AgentApprovedExporterNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EH_AgentApprovedExporterNumberTextBox, false);
			this.EH_AgentApprovedExporterNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(868, 1290, true);
			this.EH_AgentApprovedExporterNumberTextBox.Name = "EH_AgentApprovedExporterNumberTextBox";
			this.EH_AgentApprovedExporterNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.EH_AgentApprovedExporterNumberTextBox.TabIndex = 223;
			// 
			// EH_SecurityStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_SecurityStatusTextBox, "AWBHeaderManager.EH_SecurityStatusForNonBorrowedMAWBs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_SecurityStatusForNonBorrowedMAWBs)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EH_SecurityStatusTextBox, false);
			this.EH_SecurityStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(898, 1264, true);
			this.EH_SecurityStatusTextBox.Name = "EH_SecurityStatusTextBox";
			this.EH_SecurityStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.EH_SecurityStatusTextBox.TabIndex = 219;
			// 
			// EH_NetRateTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NetRateTextBox, "AWBHeaderManager.EH_NetRateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NetRateCode)));
			this.EH_NetRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 268, true);
			this.EH_NetRateTextBox.Name = "EH_NetRateTextBox";
			this.EH_NetRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.EH_NetRateTextBox.TabIndex = 20;
			// 
			// IssuingCarrierNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingCarrierNameTextBox, "AWBHeaderManager.EH_IssuingAgentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IssuingAgentName)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IssuingCarrierNameTextBox, false);
			this.IssuingCarrierNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 76, true);
			this.IssuingCarrierNameTextBox.Name = "IssuingCarrierNameTextBox";
			this.IssuingCarrierNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.IssuingCarrierNameTextBox.TabIndex = 11;
			// 
			// IssuingCarrierAddress2TextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingCarrierAddress2TextBox, "AWBHeaderManager.EH_IssuingAgentAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IssuingAgentAddress2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IssuingCarrierAddress2TextBox, false);
			this.IssuingCarrierAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 116, true);
			this.IssuingCarrierAddress2TextBox.Name = "IssuingCarrierAddress2TextBox";
			this.IssuingCarrierAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.IssuingCarrierAddress2TextBox.TabIndex = 13;
			// 
			// IssuingCarrierAddress1TextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingCarrierAddress1TextBox, "AWBHeaderManager.EH_IssuingAgentAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_IssuingAgentAddress1)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IssuingCarrierAddress1TextBox, false);
			this.IssuingCarrierAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 96, true);
			this.IssuingCarrierAddress1TextBox.Name = "IssuingCarrierAddress1TextBox";
			this.IssuingCarrierAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.IssuingCarrierAddress1TextBox.TabIndex = 12;
			// 
			// asAgreedPanel
			// 
			this.asAgreedPanel.Controls.Add(this.AsAgreed1stDropEdit);
			this.asAgreedPanel.Controls.Add(this.AsAgreed2ndDropEdit);
			this.asAgreedPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 620, true);
			this.asAgreedPanel.Name = "asAgreedPanel";
			this.asAgreedPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 20, true);
			this.asAgreedPanel.TabIndex = 79;
			// 
			// AsAgreed1stDropEdit
			// 
			this.BindingSource.SetBindingMember(this.AsAgreed1stDropEdit, "AWBHeaderManager.EH_AsAgreed1st");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AsAgreed1st)));
			this.AsAgreed1stDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			this.AsAgreed1stDropEdit.Name = "AsAgreed1stDropEdit";
			this.AsAgreed1stDropEdit.ShowDescriptionBox = false;
			this.AsAgreed1stDropEdit.PreBoundMaxLength = 3;
			this.AsAgreed1stDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 16, true);
			this.AsAgreed1stDropEdit.TabIndex = 1;
			// 
			// AsAgreed2ndDropEdit
			// 
			this.BindingSource.SetBindingMember(this.AsAgreed2ndDropEdit, "AWBHeaderManager.EH_AsAgreed2nd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AsAgreed2nd)));
			//this.AsAgreed2ndDropEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AsAgreed2ndDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 0, true);
			this.AsAgreed2ndDropEdit.Name = "AsAgreed2ndDropEdit";
			this.AsAgreed2ndDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 16, true);
			this.AsAgreed2ndDropEdit.ShowDescriptionBox = false;
			this.AsAgreed2ndDropEdit.PreBoundMaxLength = 3;
			this.AsAgreed2ndDropEdit.TabIndex = 2;
			// 
			// ConsolNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolNumberTextBox, "AWBHeaderManager.EH_ConsolNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ConsolNumber)));
			this.ConsolNumberTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|9f4042dc-b609-4161-8552-29e5b796ad1b", "Reference Number");
			this.ConsolNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 444, true);
			this.ConsolNumberTextBox.Name = "ConsolNumberTextBox";
			this.ConsolNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.ConsolNumberTextBox.TabIndex = 32;
			//
			// zLabel93
			// 
			this.zLabel93.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|7afe8e75-d2bd-4e29-b21f-746d2c7cb108", "ChgCode");
			this.zLabel93.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 471, true);
			this.zLabel93.Name = "zLabel93";
			this.zLabel93.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 14, true);
			this.zLabel93.TabIndex = 52;
			// 
			// zLabel89
			// 
			this.zLabel89.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f2fa7008-ab70-4c5b-b2dd-1cb1425fa802", "Not Negotiable");
			this.zLabel89.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 34, true);
			this.zLabel89.Name = "zLabel89";
			this.zLabel89.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 16, true);
			this.zLabel89.TabIndex = 9;
			// 
			// zLabel88
			// 
			this.zLabel88.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 172, true);
			this.zLabel88.Name = "zLabel88";
			this.zLabel88.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 93, true);
			this.zLabel88.TabIndex = 15;
			// 
			// zLabel87
			// 
			this.zLabel87.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4f70ba15-0118-499a-bec4-3ad3ca66f17a", "Issued By");
			this.zLabel87.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 80, true);
			this.zLabel87.Name = "zLabel87";
			this.zLabel87.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.zLabel87.TabIndex = 10;
			// 
			// zLabel86
			// 
			this.zLabel86.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(628, 515, true);
			this.zLabel86.Name = "zLabel86";
			this.zLabel86.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 40, true);
			this.zLabel86.TabIndex = 74;
			// 
			// zLabel85
			// 
			this.zLabel85.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 1123, true);
			this.zLabel85.Name = "zLabel85";
			this.zLabel85.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 40, true);
			this.zLabel85.TabIndex = 208;
			// 
			// zLabel84
			// 
			this.zLabel84.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|fa367bd8-808d-47bc-897e-7e672200b4a3", "At (Place)");
			this.zLabel84.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 1318, true);
			this.zLabel84.Name = "zLabel84";
			this.zLabel84.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 12, true);
			this.zLabel84.TabIndex = 225;
			// 
			// zLabel83
			// 
			this.zLabel83.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|88ab1aeb-1b86-4321-97dc-eeaf9efc3edf", "Total Prepaid");
			this.zLabel83.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 1253, true);
			this.zLabel83.Name = "zLabel83";
			this.zLabel83.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 12, true);
			this.zLabel83.TabIndex = 213;
			// 
			// zLabel82
			// 
			this.zLabel82.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|cb15b870-8578-48f5-8b1d-10b468d979b9", "Total Collect");
			this.zLabel82.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 1253, true);
			this.zLabel82.Name = "zLabel82";
			this.zLabel82.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 12, true);
			this.zLabel82.TabIndex = 214;
			// 
			// zLabel81
			// 
			this.zLabel81.AutoSize = true;
			this.zLabel81.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|b1931c87-c9b2-40c4-8cac-d0ec3d6a09e9", "Currency Conversion Rates");
			this.zLabel81.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 1294, true);
			this.zLabel81.Name = "zLabel81";
			this.zLabel81.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 12, true);
			this.zLabel81.TabIndex = 218;
			// 
			// zLabel80
			// 
			this.zLabel80.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4f57a2d1-9d1f-4716-8baa-fd3908344827", "For Carriers Use Only At Destination");
			this.zLabel80.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 1340, true);
			this.zLabel80.Name = "zLabel80";
			this.zLabel80.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 32, true);
			this.zLabel80.TabIndex = 227;
			// 
			// zLabel79
			// 
			this.zLabel79.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|046ea5ec-3431-48ed-ba31-763590c5d27b", "Executed on (date)");
			this.zLabel79.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1318, true);
			this.zLabel79.Name = "zLabel79";
			this.zLabel79.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
			this.zLabel79.TabIndex = 224;
			// 
			// zLabel78
			// 
			this.zLabel78.AutoSize = true;
			this.zLabel78.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|1d53a1f9-29fd-4ce0-b624-469d201a557f", "CC Charges in Dest. Currency", "CC Charges in Destination Currency");
			this.zLabel78.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 1294, true);
			this.zLabel78.Name = "zLabel78";
			this.zLabel78.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 12, true);
			this.zLabel78.TabIndex = 219;
			// 
			// zLabel77
			// 
			this.zLabel77.AutoSize = true;
			this.zLabel77.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4fb315bd-d3d8-4552-9b57-66faba51235d", "Charges at Destination");
			this.zLabel77.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 1338, true);
			this.zLabel77.Name = "zLabel77";
			this.zLabel77.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 12, true);
			this.zLabel77.TabIndex = 228;
			// 
			// zLabel76
			// 
			this.zLabel76.AutoSize = true;
			this.zLabel76.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|251f220c-4d6d-4715-9451-ff84b0f08a08", "Total Collect Charges");
			this.zLabel76.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 1338, true);
			this.zLabel76.Name = "zLabel76";
			this.zLabel76.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 12, true);
			this.zLabel76.TabIndex = 229;
			// 
			// zLabel75
			// 
			this.zLabel75.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|a314552c-d65c-4f6e-a404-9ec8da48b299", "Signature of Issuing Carrier or its Agent");
			this.zLabel75.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(732, 1318, true);
			this.zLabel75.Name = "zLabel75";
			this.zLabel75.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 12, true);
			this.zLabel75.TabIndex = 226;
			// 
			// zLabel74
			// 
			this.zLabel74.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|c930e489-014a-4b5b-a91a-d56aac8e7262", "Total Other Charge Due Carrier");
			this.zLabel74.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 1166, true);
			this.zLabel74.Name = "zLabel74";
			this.zLabel74.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 12, true);
			this.zLabel74.TabIndex = 205;
			// 
			// zLabel73
			// 
			this.zLabel73.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f5e7cb15-f36f-4c41-b28d-9a6daae62601", "Chargeable Weight");
			this.zLabel73.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 648, true);
			this.zLabel73.Name = "zLabel73";
			this.zLabel73.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 28, true);
			this.zLabel73.TabIndex = 86;
			// 
			// zLabel72
			// 
			this.BindingSource.SetBindingMember(this.zLabel72, "AWBHeaderManager.EH_RateClassLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_RateClassLabelText)));
			this.zLabel72.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 647, true);
			this.zLabel72.Name = "zLabel72";
			this.zLabel72.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 12, true);
			this.zLabel72.TabIndex = 85;
			// 
			// zLabel71
			// 
			this.zLabel71.AutoSize = true;
			this.zLabel71.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|12faf578-8f36-424d-b314-36c05e222b91", "Commodity Item");
			this.zLabel71.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 672, true);
			this.zLabel71.Name = "zLabel71";
			this.zLabel71.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 12, true);
			this.zLabel71.TabIndex = 88;
			// 
			// zLabel70
			// 
			this.zLabel70.AutoSize = true;
			this.zLabel70.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|164681bb-c512-49c0-9262-0c9f180bf5c5", "Rate");
			this.zLabel70.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 648, true);
			this.zLabel70.Name = "zLabel70";
			this.zLabel70.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.zLabel70.TabIndex = 87;
			// 
			// zLabel69
			// 
			this.zLabel69.AutoSize = true;
			this.zLabel69.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|2ee16c20-923b-4b97-ad48-b665f906c643", "Charge");
			this.zLabel69.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 664, true);
			this.zLabel69.Name = "zLabel69";
			this.zLabel69.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.zLabel69.TabIndex = 89;
			// 
			// zLabel68
			// 
			this.zLabel68.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|60298c61-c7f3-49be-9661-e28799a8e5cd", "Total");
			this.zLabel68.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 656, true);
			this.zLabel68.Name = "zLabel68";
			this.zLabel68.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 12, true);
			this.zLabel68.TabIndex = 90;
			// 
			// zLabel67
			// 
			this.zLabel67.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|fcd0d54a-1464-4de0-b99b-0d475d8ad517", "Curr");
			this.zLabel67.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 472, true);
			this.zLabel67.Name = "zLabel67";
			this.zLabel67.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 12, true);
			this.zLabel67.TabIndex = 49;
			// 
			// zLabel66
			// 
			this.zLabel66.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|0129a615-5463-4f92-9436-19732932dfbf", "WT/VAL");
			this.zLabel66.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 472, true);
			this.zLabel66.Name = "zLabel66";
			this.zLabel66.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 12, true);
			this.zLabel66.TabIndex = 52;
			// 
			// zLabel65
			// 
			this.zLabel65.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e8746290-fa56-4b44-84c6-8779810eec67", "Other");
			this.zLabel65.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(623, 472, true);
			this.zLabel65.Name = "zLabel65";
			this.zLabel65.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 12, true);
			this.zLabel65.TabIndex = 54;
			// 
			// zLabel63
			// 
			this.zLabel63.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|c57a8e1a-1636-4f8d-acb9-2f8ed327cf64", "SCI");
			this.zLabel63.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 601, true);
			this.zLabel63.Name = "zLabel63";
			this.zLabel63.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 12, true);
			this.zLabel63.TabIndex = 80;
			// 
			// zLabel62
			// 
			this.zLabel62.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f5e5ffae-ccc4-479f-a04d-a94dc9dff38c", "Nature and Quantity of Goods");
			this.zLabel62.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(761, 652, true);
			this.zLabel62.Name = "zLabel62";
			this.zLabel62.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 12, true);
			this.zLabel62.TabIndex = 91;
			// 
			// zLabel61
			// 
			this.zLabel61.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|51927371-c5ea-4ba7-8fa2-969eff63c85a", "(incl Dimensions or Volume)");
			this.zLabel61.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(765, 663, true);
			this.zLabel61.Name = "zLabel61";
			this.zLabel61.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 12, true);
			this.zLabel61.TabIndex = 92;
			// 
			// zLabel60
			// 
			this.zLabel60.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|727c6917-39c2-4af0-a53c-b91d60a1f297", "Prepaid");
			this.zLabel60.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 992, true);
			this.zLabel60.Name = "zLabel60";
			this.zLabel60.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 12, true);
			this.zLabel60.TabIndex = 198;
			// 
			// zLabel59
			// 
			this.zLabel59.AutoSize = true;
			this.zLabel59.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|b7c5d13c-2be8-4627-bea2-4dfba695b459", "Weight Charge");
			this.zLabel59.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 992, true);
			this.zLabel59.Name = "zLabel59";
			this.zLabel59.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 12, true);
			this.zLabel59.TabIndex = 199;
			// 
			// zLabel58
			// 
			this.zLabel58.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|70e961ae-66d1-46ff-a961-a99485e98921", "Collect");
			this.zLabel58.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 992, true);
			this.zLabel58.Name = "zLabel58";
			this.zLabel58.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 12, true);
			this.zLabel58.TabIndex = 200;
			// 
			// zLabel57
			// 
			this.zLabel57.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6019edf4-9e60-4a3f-b392-0abd9db5ec18", "Other Charges");
			this.zLabel57.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 993, true);
			this.zLabel57.Name = "zLabel57";
			this.zLabel57.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 12, true);
			this.zLabel57.TabIndex = 201;
			// 
			// zLabel56
			// 
			this.zLabel56.AutoSize = true;
			this.zLabel56.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6cb8cb4c-1765-49dc-ac34-c61c071c6635", "Valuation Charge");
			this.zLabel56.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 1035, true);
			this.zLabel56.Name = "zLabel56";
			this.zLabel56.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 12, true);
			this.zLabel56.TabIndex = 196;
			// 
			// zLabel55
			// 
			this.zLabel55.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|6d4cb691-fac5-4d99-815a-1341d601eeb5", "Tax");
			this.zLabel55.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 1078, true);
			this.zLabel55.Name = "zLabel55";
			this.zLabel55.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 12, true);
			this.zLabel55.TabIndex = 199;
			// 
			// zLabel54
			// 
			this.zLabel54.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|9d9ec636-db59-4b85-8195-2d85e1af6065", "Total Other Charge Due Agent");
			this.zLabel54.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 1121, true);
			this.zLabel54.Name = "zLabel54";
			this.zLabel54.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 12, true);
			this.zLabel54.TabIndex = 202;
			// 
			// zLabel53
			// 
			this.zLabel53.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e2fee380-f8ba-4a85-a1bf-2ac8b2d7bc45", "Signature of Shipper or his Agent");
			this.zLabel53.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 1235, true);
			this.zLabel53.Name = "zLabel53";
			this.zLabel53.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 12, true);
			this.zLabel53.TabIndex = 212;
			// 
			// zLabel52
			// 
			this.zLabel52.AutoSize = true;
			this.zLabel52.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|a8b33daa-4f60-442e-a453-e8dc30857b40", "Requested Flight Date");
			this.zLabel52.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 514, true);
			this.zLabel52.Name = "zLabel52";
			this.zLabel52.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.zLabel52.TabIndex = 66;
			// 
			// zLabel51
			// 
			this.zLabel51.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4c944b50-472f-4131-917c-9de668daa3d4", "by");
			this.zLabel51.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 472, true);
			this.zLabel51.Name = "zLabel51";
			this.zLabel51.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 12, true);
			this.zLabel51.TabIndex = 47;
			// 
			// zLabel50
			// 
			this.zLabel50.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f5d5ba20-696d-46a7-8536-973e524d60e8", "to");
			this.zLabel50.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 472, true);
			this.zLabel50.Name = "zLabel50";
			this.zLabel50.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 12, true);
			this.zLabel50.TabIndex = 41;
			// 
			// zLabel49
			// 
			this.zLabel49.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|5a42398e-3c8d-4337-9fb9-fe76328068c0", "to");
			this.zLabel49.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 472, true);
			this.zLabel49.Name = "zLabel49";
			this.zLabel49.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 12, true);
			this.zLabel49.TabIndex = 45;
			// 
			// zLabel48
			// 
			this.zLabel48.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|3f70afb1-eaa4-4210-a4aa-434cbe5aecad", "To");
			this.zLabel48.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 472, true);
			this.zLabel48.Name = "zLabel48";
			this.zLabel48.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 12, true);
			this.zLabel48.TabIndex = 36;
			// 
			// zLabel47
			// 
			this.zLabel47.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e06b917f-e7be-45ba-9fe5-679acf5040fe", "by");
			this.zLabel47.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 472, true);
			this.zLabel47.Name = "zLabel47";
			this.zLabel47.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 12, true);
			this.zLabel47.TabIndex = 43;
			// 
			// zLabel46
			// 
			this.zLabel46.AutoSize = true;
			this.zLabel46.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e90eb9b9-9722-44d8-85b9-4b873dcd8ccb", "By First Carrier");
			this.zLabel46.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 472, true);
			this.zLabel46.Name = "zLabel46";
			this.zLabel46.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 12, true);
			this.zLabel46.TabIndex = 38;
			// 
			// zLabel45
			// 
			this.zLabel45.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|69b28116-9564-4b8e-a99e-dcd9f6b16f71", "Routing and Dest.", "Routing and Destination");
			this.zLabel45.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.zLabel45.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 469, true);
			this.zLabel45.Name = "zLabel45";
			this.zLabel45.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.zLabel45.TabIndex = 40;
			this.zLabel45.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// zLabel44
			// 
			this.zLabel44.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e1e6ea02-9cdb-4a45-a360-282ff5f79f54", "Airport of Destination");
			this.zLabel44.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 515, true);
			this.zLabel44.Name = "zLabel44";
			this.zLabel44.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 12, true);
			this.zLabel44.TabIndex = 60;
			// 
			// zLabel43
			// 
			this.zLabel43.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|04ce7c69-2d02-47a7-9b18-b6db855c82ed", "Amount of Insurance");
			this.zLabel43.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 515, true);
			this.zLabel43.Name = "zLabel43";
			this.zLabel43.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 12, true);
			this.zLabel43.TabIndex = 71;
			// 
			// zLabel42
			// 
			this.zLabel42.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|5b8df27f-9071-40b5-96c9-ff9f00650665", "Handling Information");
			this.zLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 557, true);
			this.zLabel42.Name = "zLabel42";
			this.zLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 12, true);
			this.zLabel42.TabIndex = 75;
			// 
			// zLabel41
			// 
			this.zLabel41.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|2e47c529-36fa-40e1-b664-78c954296f0e", "No of Pieces RCP");
			this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 645, true);
			this.zLabel41.Name = "zLabel41";
			this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 40, true);
			this.zLabel41.TabIndex = 82;
			// 
			// zLabel40
			// 
			this.zLabel40.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|87a9e23a-c75d-40f3-b6e9-c0c8434e6554", "Gross Weight");
			this.zLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 648, true);
			this.zLabel40.Name = "zLabel40";
			this.zLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 29, true);
			this.zLabel40.TabIndex = 83;
			// 
			// zLabel39
			// 
			this.zLabel39.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|0a97c491-82c6-415b-9955-d80b1951cfa5", "kg lb");
			this.zLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 644, true);
			this.zLabel39.Name = "zLabel39";
			this.zLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 36, true);
			this.zLabel39.TabIndex = 84;
			// 
			// zLabel36
			// 
			this.zLabel36.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|a1ab580b-6573-4711-adef-b894b3703dd5", "Optional Shipping Information");
			this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 424, true);
			this.zLabel36.Name = "zLabel36";
			this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 13, true);
			this.zLabel36.TabIndex = 33;
			this.zLabel36.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// zLabel35
			// 
			this.zLabel35.AutoSize = true;
			this.zLabel35.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|07aebd72-0611-4f23-8c19-d050fb03791e", "Issuing Carrier\'s Agent Name and City");
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 296, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 13, true);
			this.zLabel35.TabIndex = 16;
			// 
			// zLabel34
			// 
			this.zLabel34.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|4fc22e50-714a-45ee-9780-f8542e595d96", "Declared Value for Carriage");
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(676, 472, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 13, true);
			this.zLabel34.TabIndex = 56;
			// 
			// zLabel32
			// 
			this.zLabel32.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|7190db91-73ea-49f3-952b-2adca527ff3a", "Agents IATA Code");
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 384, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 12, true);
			this.zLabel32.TabIndex = 25;
			// 
			// zLabel31
			// 
			this.zLabel31.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|a6924e88-b93e-4cc7-998d-ebc45468513d", "Declared Value for Customs");
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 472, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 12, true);
			this.zLabel31.TabIndex = 58;
			// 
			// zLabel30
			// 
			this.zLabel30.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|feebd59b-a430-4ddc-8cd8-16092e32843e", "Account No.");
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 384, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 12, true);
			this.zLabel30.TabIndex = 27;
			// 
			// zLabel29
			// 
			this.zLabel29.AutoSize = true;
			this.zLabel29.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|70e7e447-c391-481d-bedf-df239a063c48", "Reference Number");
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 428, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.zLabel29.TabIndex = 31;
			// 
			// zLabel28
			// 
			this.zLabel28.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|dfa41db7-aa0a-4106-adb3-7f5eccc8f182", "Airport of Departure (Addr. of First Carrier) and Requested Routing.", "Airport of Departure (Address of First Carrier) and Requested Routing.");
			this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 428, true);
			this.zLabel28.Name = "zLabel28";
			this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 16, true);
			this.zLabel28.TabIndex = 29;
			// 
			// zLabel27
			// 
			this.zLabel27.AutoSize = true;
			this.zLabel27.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|7497d458-e079-416f-8baa-0fe776a3a387", "Copies 1, 2 and 3 of this Air Waybill are originals and have the same validity.");
			this.zLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 144, true);
			this.zLabel27.Name = "zLabel27";
			this.zLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 13, true);
			this.zLabel27.TabIndex = 14;
			// 
			// zLabel103
			// 
			this.zLabel103.AutoSize = true;
			this.zLabel103.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|E57971F8-8070-4efb-A7E7-324AD13015D0", "Name:");
			this.zLabel103.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 324, true);
			this.zLabel103.Name = "zLabel103";
			this.zLabel103.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.zLabel103.TabIndex = 2;
			// 
			// zLabel102
			// 
			this.zLabel102.AutoSize = true;
			this.zLabel102.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|745F761F-C42E-440e-9C34-AFDBAA1E0D74", "City:");
			this.zLabel102.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 352, true);
			this.zLabel102.Name = "zLabel102";
			this.zLabel102.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 13, true);
			this.zLabel102.TabIndex = 9;
			// 
			// zLabel101
			// 
			this.zLabel101.AutoSize = true;
			this.zLabel101.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|5CF38C7D-804A-451b-8E81-C07F6F43B557", "Code:");
			this.zLabel101.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 515, true);
			this.zLabel101.Name = "zLabel101";
			this.zLabel101.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.zLabel101.TabIndex = 62;
			// 
			// zLabel64
			// 
			this.zLabel64.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|1DA90838-1669-4031-BD42-ED3BC5C7F6F3", "Net Rate:");
			this.zLabel64.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(860, 272, true);
			this.zLabel64.Name = "zLabel64";
			this.zLabel64.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 12, true);
			this.zLabel64.TabIndex = 19;
			// 
			// ECNCRNNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ECNCRNNumberTextBox, "AWBHeaderManager.EH_ECNCRNNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ECNCRNNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ECNCRNNumberTextBox, false);
			this.ECNCRNNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 572, true);
			this.ECNCRNNumberTextBox.Name = "ECNCRNNumberTextBox";
			this.ECNCRNNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ECNCRNNumberTextBox.TabIndex = 77;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "AWBHeaderManager.EH_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ReferenceNumber)));
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 8, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 4;
			// 
			// TotalLineTotalsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalLineTotalsCalcEdit, "AWBHeaderManager.EH_TotalLineTotals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalLineTotals)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalLineTotalsCalcEdit, false);
			this.TotalLineTotalsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 960, true);
			this.TotalLineTotalsCalcEdit.Name = "TotalLineTotalsCalcEdit";
			this.TotalLineTotalsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.TotalLineTotalsCalcEdit.TabIndex = 197;
			this.TotalLineTotalsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalGrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossWeightCalcEdit, "AWBHeaderManager.EH_TotalGrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalGrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalGrossWeightCalcEdit, false);
			this.TotalGrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 960, true);
			this.TotalGrossWeightCalcEdit.Name = "TotalGrossWeightCalcEdit";
			this.TotalGrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.TotalGrossWeightCalcEdit.TabIndex = 194;
			this.TotalGrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalNoOfPiecesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalNoOfPiecesCalcEdit, "AWBHeaderManager.EH_TotalNoOfPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalNoOfPieces)));
			this.TotalNoOfPiecesCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalNoOfPiecesCalcEdit, false);
			this.TotalNoOfPiecesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 960, true);
			this.TotalNoOfPiecesCalcEdit.Name = "TotalNoOfPiecesCalcEdit";
			this.TotalNoOfPiecesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.TotalNoOfPiecesCalcEdit.TabIndex = 193;
			this.TotalNoOfPiecesCalcEdit.Text = "0";
			this.TotalNoOfPiecesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPrepaidCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPrepaidCalcEdit, "AWBHeaderManager.EH_TotalPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalPrepaidCalcEdit, false);
			this.TotalPrepaidCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1270, true);
			this.TotalPrepaidCalcEdit.Name = "TotalPrepaidCalcEdit";
			this.TotalPrepaidCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalPrepaidCalcEdit.TabIndex = 215;
			this.TotalPrepaidCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCollectCalcEdit, "AWBHeaderManager.EH_TotalCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalCOL)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalCollectCalcEdit, false);
			this.TotalCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1270, true);
			this.TotalCollectCalcEdit.Name = "TotalCollectCalcEdit";
			this.TotalCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCollectCalcEdit.TabIndex = 216;
			this.TotalCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NatureAndQtyOfGoodsTextBox10
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox10, "AWBHeaderManager.AWBRateLine10");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox10, false);
			this.NatureAndQtyOfGoodsTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 889, true);
			this.NatureAndQtyOfGoodsTextBox10.Name = "NatureAndQtyOfGoodsTextBox10";
			this.NatureAndQtyOfGoodsTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox10.TabIndex = 182;
			// 
			// CommodityItemNumberControl10
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl10, "AWBHeaderManager.AWBRateLine10");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 889, true);
			this.CommodityItemNumberControl10.Name = "CommodityItemNumberControl10";
			this.CommodityItemNumberControl10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl10.TabIndex = 178;
			// 
			// RateClassDropEdit10
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit10, "AWBHeaderManager.AWBRateLine10.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 889, true);
			this.RateClassDropEdit10.Name = "RateClassDropEdit10";
			this.RateClassDropEdit10.PreBoundMaxLength = 1;
			this.RateClassDropEdit10.ShowDescriptionBox = false;
			this.RateClassDropEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit10.TabIndex = 177;
			// 
			// RateUQDropEdit10
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit10, "AWBHeaderManager.AWBRateLine10.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 889, true);
			this.RateUQDropEdit10.Name = "RateUQDropEdit10";
			this.RateUQDropEdit10.PreBoundMaxLength = 1;
			this.RateUQDropEdit10.ShowDescriptionBox = false;
			this.RateUQDropEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit10.TabIndex = 176;
			// 
			// LineTotalCalcEdit10
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit10, "AWBHeaderManager.AWBRateLine10.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 889, true);
			this.LineTotalCalcEdit10.Name = "LineTotalCalcEdit10";
			this.LineTotalCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit10.TabIndex = 181;
			this.LineTotalCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit10
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit10, "AWBHeaderManager.AWBRateLine10.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit10, false);
			this.RateChargeCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 889, true);
			this.RateChargeCalcEdit10.Name = "RateChargeCalcEdit10";
			this.RateChargeCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit10.TabIndex = 180;
			this.RateChargeCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit10
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit10, "AWBHeaderManager.AWBRateLine10.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit10, false);
			this.ChargeableWeightCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 889, true);
			this.ChargeableWeightCalcEdit10.Name = "ChargeableWeightCalcEdit10";
			this.ChargeableWeightCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit10.TabIndex = 179;
			this.ChargeableWeightCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit10
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit10, "AWBHeaderManager.AWBRateLine10.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine10)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit10, false);
			this.GrossWeightCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 889, true);
			this.GrossWeightCalcEdit10.Name = "GrossWeightCalcEdit10";
			this.GrossWeightCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit10.TabIndex = 175;
			this.GrossWeightCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl9
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl9, "AWBHeaderManager.AWBRateLine9");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 868, true);
			this.CommodityItemNumberControl9.Name = "CommodityItemNumberControl9";
			this.CommodityItemNumberControl9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl9.TabIndex = 169;
			// 
			// NatureAndQtyOfGoodsTextBox9
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox9, "AWBHeaderManager.AWBRateLine9");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox9, false);
			this.NatureAndQtyOfGoodsTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 868, true);
			this.NatureAndQtyOfGoodsTextBox9.Name = "NatureAndQtyOfGoodsTextBox9";
			this.NatureAndQtyOfGoodsTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox9.TabIndex = 173;
			// 
			// RateClassDropEdit9
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit9, "AWBHeaderManager.AWBRateLine9.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 868, true);
			this.RateClassDropEdit9.Name = "RateClassDropEdit9";
			this.RateClassDropEdit9.PreBoundMaxLength = 1;
			this.RateClassDropEdit9.ShowDescriptionBox = false;
			this.RateClassDropEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit9.TabIndex = 168;
			// 
			// RateUQDropEdit9
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit9, "AWBHeaderManager.AWBRateLine9.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 868, true);
			this.RateUQDropEdit9.Name = "RateUQDropEdit9";
			this.RateUQDropEdit9.PreBoundMaxLength = 1;
			this.RateUQDropEdit9.ShowDescriptionBox = false;
			this.RateUQDropEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit9.TabIndex = 167;
			// 
			// LineTotalCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit9, "AWBHeaderManager.AWBRateLine9.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 868, true);
			this.LineTotalCalcEdit9.Name = "LineTotalCalcEdit9";
			this.LineTotalCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit9.TabIndex = 172;
			this.LineTotalCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit9, "AWBHeaderManager.AWBRateLine9.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit9, false);
			this.RateChargeCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 868, true);
			this.RateChargeCalcEdit9.Name = "RateChargeCalcEdit9";
			this.RateChargeCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit9.TabIndex = 171;
			this.RateChargeCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit9, "AWBHeaderManager.AWBRateLine9.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit9, false);
			this.ChargeableWeightCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 868, true);
			this.ChargeableWeightCalcEdit9.Name = "ChargeableWeightCalcEdit9";
			this.ChargeableWeightCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit9.TabIndex = 170;
			this.ChargeableWeightCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit9, "AWBHeaderManager.AWBRateLine9.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine9)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit9, false);
			this.GrossWeightCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 868, true);
			this.GrossWeightCalcEdit9.Name = "GrossWeightCalcEdit9";
			this.GrossWeightCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit9.TabIndex = 166;
			this.GrossWeightCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl8
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl8, "AWBHeaderManager.AWBRateLine8");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 846, true);
			this.CommodityItemNumberControl8.Name = "CommodityItemNumberControl8";
			this.CommodityItemNumberControl8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl8.TabIndex = 160;
			// 
			// NatureAndQtyOfGoodsTextBox8
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox8, "AWBHeaderManager.AWBRateLine8");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox8, false);
			this.NatureAndQtyOfGoodsTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 846, true);
			this.NatureAndQtyOfGoodsTextBox8.Name = "NatureAndQtyOfGoodsTextBox8";
			this.NatureAndQtyOfGoodsTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox8.TabIndex = 164;
			// 
			// RateClassDropEdit8
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit8, "AWBHeaderManager.AWBRateLine8.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 846, true);
			this.RateClassDropEdit8.Name = "RateClassDropEdit8";
			this.RateClassDropEdit8.PreBoundMaxLength = 1;
			this.RateClassDropEdit8.ShowDescriptionBox = false;
			this.RateClassDropEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit8.TabIndex = 159;
			// 
			// RateUQDropEdit8
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit8, "AWBHeaderManager.AWBRateLine8.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 846, true);
			this.RateUQDropEdit8.Name = "RateUQDropEdit8";
			this.RateUQDropEdit8.PreBoundMaxLength = 1;
			this.RateUQDropEdit8.ShowDescriptionBox = false;
			this.RateUQDropEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit8.TabIndex = 158;
			// 
			// LineTotalCalcEdit8
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit8, "AWBHeaderManager.AWBRateLine8.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 846, true);
			this.LineTotalCalcEdit8.Name = "LineTotalCalcEdit8";
			this.LineTotalCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit8.TabIndex = 163;
			this.LineTotalCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit8
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit8, "AWBHeaderManager.AWBRateLine8.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit8, false);
			this.RateChargeCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 846, true);
			this.RateChargeCalcEdit8.Name = "RateChargeCalcEdit8";
			this.RateChargeCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit8.TabIndex = 162;
			this.RateChargeCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit8
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit8, "AWBHeaderManager.AWBRateLine8.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit8, false);
			this.ChargeableWeightCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 846, true);
			this.ChargeableWeightCalcEdit8.Name = "ChargeableWeightCalcEdit8";
			this.ChargeableWeightCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit8.TabIndex = 161;
			this.ChargeableWeightCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit8
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit8, "AWBHeaderManager.AWBRateLine8.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine8)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit8, false);
			this.GrossWeightCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 846, true);
			this.GrossWeightCalcEdit8.Name = "GrossWeightCalcEdit8";
			this.GrossWeightCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit8.TabIndex = 157;
			this.GrossWeightCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl7
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl7, "AWBHeaderManager.AWBRateLine7");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 824, true);
			this.CommodityItemNumberControl7.Name = "CommodityItemNumberControl7";
			this.CommodityItemNumberControl7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl7.TabIndex = 151;
			// 
			// NatureAndQtyOfGoodsTextBox7
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox7, "AWBHeaderManager.AWBRateLine7");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox7, false);
			this.NatureAndQtyOfGoodsTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 824, true);
			this.NatureAndQtyOfGoodsTextBox7.Name = "NatureAndQtyOfGoodsTextBox7";
			this.NatureAndQtyOfGoodsTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox7.TabIndex = 155;
			// 
			// RateClassDropEdit7
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit7, "AWBHeaderManager.AWBRateLine7.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 824, true);
			this.RateClassDropEdit7.Name = "RateClassDropEdit7";
			this.RateClassDropEdit7.PreBoundMaxLength = 1;
			this.RateClassDropEdit7.ShowDescriptionBox = false;
			this.RateClassDropEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit7.TabIndex = 150;
			// 
			// RateUQDropEdit7
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit7, "AWBHeaderManager.AWBRateLine7.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 824, true);
			this.RateUQDropEdit7.Name = "RateUQDropEdit7";
			this.RateUQDropEdit7.PreBoundMaxLength = 1;
			this.RateUQDropEdit7.ShowDescriptionBox = false;
			this.RateUQDropEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit7.TabIndex = 149;
			// 
			// LineTotalCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit7, "AWBHeaderManager.AWBRateLine7.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 824, true);
			this.LineTotalCalcEdit7.Name = "LineTotalCalcEdit7";
			this.LineTotalCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit7.TabIndex = 154;
			this.LineTotalCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit7, "AWBHeaderManager.AWBRateLine7.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit7, false);
			this.RateChargeCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 824, true);
			this.RateChargeCalcEdit7.Name = "RateChargeCalcEdit7";
			this.RateChargeCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit7.TabIndex = 153;
			this.RateChargeCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit7, "AWBHeaderManager.AWBRateLine7.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit7, false);
			this.ChargeableWeightCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 824, true);
			this.ChargeableWeightCalcEdit7.Name = "ChargeableWeightCalcEdit7";
			this.ChargeableWeightCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit7.TabIndex = 152;
			this.ChargeableWeightCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit7
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit7, "AWBHeaderManager.AWBRateLine7.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine7)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit7, false);
			this.GrossWeightCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 824, true);
			this.GrossWeightCalcEdit7.Name = "GrossWeightCalcEdit7";
			this.GrossWeightCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit7.TabIndex = 148;
			this.GrossWeightCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl6
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl6, "AWBHeaderManager.AWBRateLine6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 802, true);
			this.CommodityItemNumberControl6.Name = "CommodityItemNumberControl6";
			this.CommodityItemNumberControl6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl6.TabIndex = 142;
			// 
			// NatureAndQtyOfGoodsTextBox6
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox6, "AWBHeaderManager.AWBRateLine6");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox6, false);
			this.NatureAndQtyOfGoodsTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 802, true);
			this.NatureAndQtyOfGoodsTextBox6.Name = "NatureAndQtyOfGoodsTextBox6";
			this.NatureAndQtyOfGoodsTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox6.TabIndex = 146;
			// 
			// RateClassDropEdit6
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit6, "AWBHeaderManager.AWBRateLine6.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 802, true);
			this.RateClassDropEdit6.Name = "RateClassDropEdit6";
			this.RateClassDropEdit6.PreBoundMaxLength = 1;
			this.RateClassDropEdit6.ShowDescriptionBox = false;
			this.RateClassDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit6.TabIndex = 141;
			// 
			// RateUQDropEdit6
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit6, "AWBHeaderManager.AWBRateLine6.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 802, true);
			this.RateUQDropEdit6.Name = "RateUQDropEdit6";
			this.RateUQDropEdit6.PreBoundMaxLength = 1;
			this.RateUQDropEdit6.ShowDescriptionBox = false;
			this.RateUQDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit6.TabIndex = 140;
			// 
			// LineTotalCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit6, "AWBHeaderManager.AWBRateLine6.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 802, true);
			this.LineTotalCalcEdit6.Name = "LineTotalCalcEdit6";
			this.LineTotalCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit6.TabIndex = 145;
			this.LineTotalCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit6, "AWBHeaderManager.AWBRateLine6.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit6, false);
			this.RateChargeCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 802, true);
			this.RateChargeCalcEdit6.Name = "RateChargeCalcEdit6";
			this.RateChargeCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit6.TabIndex = 144;
			this.RateChargeCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit6, "AWBHeaderManager.AWBRateLine6.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit6, false);
			this.ChargeableWeightCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 802, true);
			this.ChargeableWeightCalcEdit6.Name = "ChargeableWeightCalcEdit6";
			this.ChargeableWeightCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit6.TabIndex = 143;
			this.ChargeableWeightCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit6, "AWBHeaderManager.AWBRateLine6.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine6)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit6, false);
			this.GrossWeightCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 802, true);
			this.GrossWeightCalcEdit6.Name = "GrossWeightCalcEdit6";
			this.GrossWeightCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit6.TabIndex = 139;
			this.GrossWeightCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl5
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl5, "AWBHeaderManager.AWBRateLine5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 780, true);
			this.CommodityItemNumberControl5.Name = "CommodityItemNumberControl5";
			this.CommodityItemNumberControl5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl5.TabIndex = 133;
			// 
			// NatureAndQtyOfGoodsTextBox5
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox5, "AWBHeaderManager.AWBRateLine5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5.NatureAndQtyOfGoodsDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox5, false);
			this.NatureAndQtyOfGoodsTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 780, true);
			this.NatureAndQtyOfGoodsTextBox5.Name = "NatureAndQtyOfGoodsTextBox5";
			this.NatureAndQtyOfGoodsTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox5.TabIndex = 137;
			// 
			// RateClassDropEdit5
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit5, "AWBHeaderManager.AWBRateLine5.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 780, true);
			this.RateClassDropEdit5.Name = "RateClassDropEdit5";
			this.RateClassDropEdit5.PreBoundMaxLength = 1;
			this.RateClassDropEdit5.ShowDescriptionBox = false;
			this.RateClassDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit5.TabIndex = 132;
			// 
			// RateUQDropEdit5
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit5, "AWBHeaderManager.AWBRateLine5.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 780, true);
			this.RateUQDropEdit5.Name = "RateUQDropEdit5";
			this.RateUQDropEdit5.PreBoundMaxLength = 1;
			this.RateUQDropEdit5.ShowDescriptionBox = false;
			this.RateUQDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit5.TabIndex = 131;
			// 
			// LineTotalCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit5, "AWBHeaderManager.AWBRateLine5.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 780, true);
			this.LineTotalCalcEdit5.Name = "LineTotalCalcEdit5";
			this.LineTotalCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit5.TabIndex = 136;
			this.LineTotalCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit5, "AWBHeaderManager.AWBRateLine5.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit5, false);
			this.RateChargeCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 780, true);
			this.RateChargeCalcEdit5.Name = "RateChargeCalcEdit5";
			this.RateChargeCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit5.TabIndex = 135;
			this.RateChargeCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit5, "AWBHeaderManager.AWBRateLine5.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit5, false);
			this.ChargeableWeightCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 780, true);
			this.ChargeableWeightCalcEdit5.Name = "ChargeableWeightCalcEdit5";
			this.ChargeableWeightCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit5.TabIndex = 134;
			this.ChargeableWeightCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit5, "AWBHeaderManager.AWBRateLine5.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine5)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit5, false);
			this.GrossWeightCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 780, true);
			this.GrossWeightCalcEdit5.Name = "GrossWeightCalcEdit5";
			this.GrossWeightCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit5.TabIndex = 130;
			this.GrossWeightCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl4
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl4, "AWBHeaderManager.AWBRateLine4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 758, true);
			this.CommodityItemNumberControl4.Name = "CommodityItemNumberControl4";
			this.CommodityItemNumberControl4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl4.TabIndex = 124;
			// 
			// NatureAndQtyOfGoodsTextBox4
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox4, "AWBHeaderManager.AWBRateLine4");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox4, false);
			this.NatureAndQtyOfGoodsTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 758, true);
			this.NatureAndQtyOfGoodsTextBox4.Name = "NatureAndQtyOfGoodsTextBox4";
			this.NatureAndQtyOfGoodsTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox4.TabIndex = 128;
			// 
			// RateClassDropEdit4
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit4, "AWBHeaderManager.AWBRateLine4.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 758, true);
			this.RateClassDropEdit4.Name = "RateClassDropEdit4";
			this.RateClassDropEdit4.PreBoundMaxLength = 1;
			this.RateClassDropEdit4.ShowDescriptionBox = false;
			this.RateClassDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit4.TabIndex = 123;
			// 
			// RateUQDropEdit4
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit4, "AWBHeaderManager.AWBRateLine4.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 758, true);
			this.RateUQDropEdit4.Name = "RateUQDropEdit4";
			this.RateUQDropEdit4.PreBoundMaxLength = 1;
			this.RateUQDropEdit4.ShowDescriptionBox = false;
			this.RateUQDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit4.TabIndex = 122;
			// 
			// LineTotalCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit4, "AWBHeaderManager.AWBRateLine4.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 758, true);
			this.LineTotalCalcEdit4.Name = "LineTotalCalcEdit4";
			this.LineTotalCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit4.TabIndex = 127;
			this.LineTotalCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit4, "AWBHeaderManager.AWBRateLine4.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit4, false);
			this.RateChargeCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 758, true);
			this.RateChargeCalcEdit4.Name = "RateChargeCalcEdit4";
			this.RateChargeCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit4.TabIndex = 126;
			this.RateChargeCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit4, "AWBHeaderManager.AWBRateLine4.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit4, false);
			this.ChargeableWeightCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 758, true);
			this.ChargeableWeightCalcEdit4.Name = "ChargeableWeightCalcEdit4";
			this.ChargeableWeightCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit4.TabIndex = 125;
			this.ChargeableWeightCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit4, "AWBHeaderManager.AWBRateLine4.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine4)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit4, false);
			this.GrossWeightCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 758, true);
			this.GrossWeightCalcEdit4.Name = "GrossWeightCalcEdit4";
			this.GrossWeightCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit4.TabIndex = 121;
			this.GrossWeightCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl3
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl3, "AWBHeaderManager.AWBRateLine3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 736, true);
			this.CommodityItemNumberControl3.Name = "CommodityItemNumberControl3";
			this.CommodityItemNumberControl3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl3.TabIndex = 115;
			// 
			// NatureAndQtyOfGoodsTextBox3
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox3, "AWBHeaderManager.AWBRateLine3");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox3, false);
			this.NatureAndQtyOfGoodsTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 736, true);
			this.NatureAndQtyOfGoodsTextBox3.Name = "NatureAndQtyOfGoodsTextBox3";
			this.NatureAndQtyOfGoodsTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox3.TabIndex = 119;
			// 
			// RateClassDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit3, "AWBHeaderManager.AWBRateLine3.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 736, true);
			this.RateClassDropEdit3.Name = "RateClassDropEdit3";
			this.RateClassDropEdit3.PreBoundMaxLength = 1;
			this.RateClassDropEdit3.ShowDescriptionBox = false;
			this.RateClassDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit3.TabIndex = 114;
			// 
			// RateUQDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit3, "AWBHeaderManager.AWBRateLine3.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 736, true);
			this.RateUQDropEdit3.Name = "RateUQDropEdit3";
			this.RateUQDropEdit3.PreBoundMaxLength = 1;
			this.RateUQDropEdit3.ShowDescriptionBox = false;
			this.RateUQDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit3.TabIndex = 113;
			// 
			// LineTotalCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit3, "AWBHeaderManager.AWBRateLine3.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 736, true);
			this.LineTotalCalcEdit3.Name = "LineTotalCalcEdit3";
			this.LineTotalCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit3.TabIndex = 118;
			this.LineTotalCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit3, "AWBHeaderManager.AWBRateLine3.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit3, false);
			this.RateChargeCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 736, true);
			this.RateChargeCalcEdit3.Name = "RateChargeCalcEdit3";
			this.RateChargeCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit3.TabIndex = 117;
			this.RateChargeCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit3, "AWBHeaderManager.AWBRateLine3.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit3, false);
			this.ChargeableWeightCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 736, true);
			this.ChargeableWeightCalcEdit3.Name = "ChargeableWeightCalcEdit3";
			this.ChargeableWeightCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit3.TabIndex = 116;
			this.ChargeableWeightCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit3, "AWBHeaderManager.AWBRateLine3.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine3)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit3, false);
			this.GrossWeightCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 736, true);
			this.GrossWeightCalcEdit3.Name = "GrossWeightCalcEdit3";
			this.GrossWeightCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit3.TabIndex = 112;
			this.GrossWeightCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl2
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl2, "AWBHeaderManager.AWBRateLine2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 714, true);
			this.CommodityItemNumberControl2.Name = "CommodityItemNumberControl2";
			this.CommodityItemNumberControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl2.TabIndex = 106;
			// 
			// NatureAndQtyOfGoodsTextBox2
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox2, "AWBHeaderManager.AWBRateLine2");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox2, false);
			this.NatureAndQtyOfGoodsTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 714, true);
			this.NatureAndQtyOfGoodsTextBox2.Name = "NatureAndQtyOfGoodsTextBox2";
			this.NatureAndQtyOfGoodsTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox2.TabIndex = 110;
			// 
			// RateClassDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit2, "AWBHeaderManager.AWBRateLine2.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 714, true);
			this.RateClassDropEdit2.Name = "RateClassDropEdit2";
			this.RateClassDropEdit2.PreBoundMaxLength = 1;
			this.RateClassDropEdit2.ShowDescriptionBox = false;
			this.RateClassDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit2.TabIndex = 105;
			// 
			// RateUQDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit2, "AWBHeaderManager.AWBRateLine2.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 714, true);
			this.RateUQDropEdit2.Name = "RateUQDropEdit2";
			this.RateUQDropEdit2.PreBoundMaxLength = 1;
			this.RateUQDropEdit2.ShowDescriptionBox = false;
			this.RateUQDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit2.TabIndex = 104;
			// 
			// LineTotalCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit2, "AWBHeaderManager.AWBRateLine2.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 714, true);
			this.LineTotalCalcEdit2.Name = "LineTotalCalcEdit2";
			this.LineTotalCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit2.TabIndex = 109;
			this.LineTotalCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit2, "AWBHeaderManager.AWBRateLine2.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit2, false);
			this.RateChargeCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 714, true);
			this.RateChargeCalcEdit2.Name = "RateChargeCalcEdit2";
			this.RateChargeCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit2.TabIndex = 108;
			this.RateChargeCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit2, "AWBHeaderManager.AWBRateLine2.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit2, false);
			this.ChargeableWeightCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 714, true);
			this.ChargeableWeightCalcEdit2.Name = "ChargeableWeightCalcEdit2";
			this.ChargeableWeightCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit2.TabIndex = 107;
			this.ChargeableWeightCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit2, "AWBHeaderManager.AWBRateLine2.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine2)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit2, false);
			this.GrossWeightCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 714, true);
			this.GrossWeightCalcEdit2.Name = "GrossWeightCalcEdit2";
			this.GrossWeightCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit2.TabIndex = 103;
			this.GrossWeightCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommodityItemNumberControl1
			// 
			this.BindingSource.SetBindingMember(this.CommodityItemNumberControl1, "AWBHeaderManager.AWBRateLine1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_CommodityItemNumber)));
			this.CommodityItemNumberControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 692, true);
			this.CommodityItemNumberControl1.Name = "CommodityItemNumberControl1";
			this.CommodityItemNumberControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberControl1.TabIndex = 97;
			// 
			// NatureAndQtyOfGoodsTextBox1
			// 
			this.BindingSource.SetBindingMember(this.NatureAndQtyOfGoodsTextBox1, "AWBHeaderManager.AWBRateLine1");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NatureAndQtyOfGoodsTextBox1, false);
			this.NatureAndQtyOfGoodsTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 692, true);
			this.NatureAndQtyOfGoodsTextBox1.Name = "NatureAndQtyOfGoodsTextBox1";
			this.NatureAndQtyOfGoodsTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.NatureAndQtyOfGoodsTextBox1.TabIndex = 101;
			// 
			// RateClassDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.RateClassDropEdit1, "AWBHeaderManager.AWBRateLine1.ER_RateClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_RateClass)));
			this.RateClassDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 692, true);
			this.RateClassDropEdit1.Name = "RateClassDropEdit1";
			this.RateClassDropEdit1.PreBoundMaxLength = 1;
			this.RateClassDropEdit1.ShowDescriptionBox = false;
			this.RateClassDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateClassDropEdit1.TabIndex = 96;
			// 
			// RateUQDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.RateUQDropEdit1, "AWBHeaderManager.AWBRateLine1.ER_WeightInLBsOrKGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_WeightInLBsOrKGs)));
			this.RateUQDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 692, true);
			this.RateUQDropEdit1.Name = "RateUQDropEdit1";
			this.RateUQDropEdit1.PreBoundMaxLength = 1;
			this.RateUQDropEdit1.ShowDescriptionBox = false;
			this.RateUQDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RateUQDropEdit1.TabIndex = 95;
			// 
			// LineTotalCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCalcEdit1, "AWBHeaderManager.AWBRateLine1.ER_Total");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_Total)));
			this.LineTotalCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 692, true);
			this.LineTotalCalcEdit1.Name = "LineTotalCalcEdit1";
			this.LineTotalCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.LineTotalCalcEdit1.TabIndex = 100;
			this.LineTotalCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateChargeCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.RateChargeCalcEdit1, "AWBHeaderManager.AWBRateLine1.ER_RateChargeOrDiscount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_RateChargeOrDiscount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateChargeCalcEdit1, false);
			this.RateChargeCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 692, true);
			this.RateChargeCalcEdit1.Name = "RateChargeCalcEdit1";
			this.RateChargeCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RateChargeCalcEdit1.TabIndex = 99;
			this.RateChargeCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit1, "AWBHeaderManager.AWBRateLine1.ER_ChargeableWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_ChargeableWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeableWeightCalcEdit1, false);
			this.ChargeableWeightCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 692, true);
			this.ChargeableWeightCalcEdit1.Name = "ChargeableWeightCalcEdit1";
			this.ChargeableWeightCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ChargeableWeightCalcEdit1.TabIndex = 98;
			this.ChargeableWeightCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit1, "AWBHeaderManager.AWBRateLine1.ER_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBRateLine1)).SyncRoot)).ER_GrossWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GrossWeightCalcEdit1, false);
			this.GrossWeightCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 692, true);
			this.GrossWeightCalcEdit1.Name = "GrossWeightCalcEdit1";
			this.GrossWeightCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.GrossWeightCalcEdit1.TabIndex = 94;
			this.GrossWeightCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccountingInformationTabControl
			// 
			this.AccountingInformationTabControl.Controls.Add(this.AccountingInformationTabPage);
			this.AccountingInformationTabControl.Controls.Add(this.NotifyPartyInfoTabPage);
			this.AccountingInformationTabControl.Controls.Add(this.NotifyAddressOverrideTabPage);
			this.AccountingInformationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 291, true);
			this.AccountingInformationTabControl.Multiline = true;
			this.AccountingInformationTabControl.Name = "AccountingInformationTabControl";
			this.AccountingInformationTabControl.SelectedIndex = 0;
			this.AccountingInformationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 134, true);
			this.AccountingInformationTabControl.TabIndex = 17;
			// 
			// AccountingInformationTabPage
			// 
			this.AccountingInformationTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|25566fdf-76e2-4d08-8dfe-4d70102b1ce7", "Accounting Info");
			this.AccountingInformationTabPage.Controls.Add(this.AccountingInformationGrid);
			this.AccountingInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountingInformationTabPage.Name = "AccountingInformationTabPage";
			this.AccountingInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 107, true);
			this.AccountingInformationTabPage.TabIndex = 1;
			// 
			// AccountingInformationGrid
			// 
			this.AccountingInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AccountingInformationGrid, "AWBHeaderManager.AWBAccountingInformations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBAccountingInformations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBAccountingInformation)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBAccountingInformations)).SyncRoot)).EA_InformationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBAccountingInformation)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBAccountingInformations)).SyncRoot)).EA_Information)));
			this.AccountingInformationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "EA_InformationID";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "EA_Information";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.AccountingInformationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AccountingInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AccountingInformationGrid.GridId = "f2a3cb9a-2420-4328-a46b-136410507d74";
			this.AccountingInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountingInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccountingInformationGrid.LayoutKey = "AccountingInformationGrid";
			this.AccountingInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccountingInformationGrid.Name = "AccountingInformationGrid";
			this.AccountingInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 107, true);
			this.AccountingInformationGrid.TabIndex = 0;
			// 
			// NotifyPartyInfoTabPage
			// 
			this.NotifyPartyInfoTabPage.AutoScroll = true;
			this.NotifyPartyInfoTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.NotifyPartyInfoTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|aeb8111e-5565-47a3-bcfa-ac1ba6a5534c", "Also Notify");
			this.NotifyPartyInfoTabPage.Controls.Add(this.ChooseAlsoNotifyAddressDropEdit);
			this.NotifyPartyInfoTabPage.Controls.Add(this.AlsoNotifyAddressSaveButton);
			this.NotifyPartyInfoTabPage.Controls.Add(this.EH_AlsoNotifyContactCodeDropEdit);
			this.NotifyPartyInfoTabPage.Controls.Add(this.EH_AlsoNotifyPostCodeTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyContactDetailTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyCountryTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyCityTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyStateTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyAddressTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyContactNameTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.NotifyNameTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.AlsoNotifyTraderTypeTextBox);
			this.NotifyPartyInfoTabPage.Controls.Add(this.AlsoNotifyTraderNoTextBox);
			this.NotifyPartyInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotifyPartyInfoTabPage.Name = "NotifyPartyInfoTabPage";
			this.NotifyPartyInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 107, true);
			this.NotifyPartyInfoTabPage.TabIndex = 0;
			// 
			// ChooseAlsoNotifyAddressDropEdit
			// 
			this.ChooseAlsoNotifyAddressDropEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ChooseAlsoNotifyAddressDropEdit, "AWBHeaderManager.EH_AlsoNotifyDefaultAddressPicker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyDefaultAddressPicker)));
			this.ChooseAlsoNotifyAddressDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|8973bad0-c34a-4fc3-945c-17be1896beda", "Choose");
			this.ChooseAlsoNotifyAddressDropEdit.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.ChooseAlsoNotifyAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 0, true);
			this.ChooseAlsoNotifyAddressDropEdit.Name = "ChooseAlsoNotifyAddressDropEdit";
			this.ChooseAlsoNotifyAddressDropEdit.PreBoundMaxLength = 30;
			this.ChooseAlsoNotifyAddressDropEdit.ShowDescriptionBox = false;
			this.ChooseAlsoNotifyAddressDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ChooseAlsoNotifyAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ChooseAlsoNotifyAddressDropEdit.TabIndex = 1;
			// 
			// AlsoNotifyAddressSaveButton
			// 
			this.SetDataSourceBinding(this.AlsoNotifyAddressSaveButton, "IsEnabledForBinding", "IsAWBValuesOverriddenProperty");
			this.AlsoNotifyAddressSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
			this.AlsoNotifyAddressSaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|81db0359-d1d0-47a2-a325-a9b55b3409d1", "Save");
			this.AlsoNotifyAddressSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.AlsoNotifyAddressSaveButton.Name = "ShipperAddressSaveButton";
			this.AlsoNotifyAddressSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.AlsoNotifyAddressSaveButton.TabIndex = 24;
			// 
			// EH_AlsoNotifyContactCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.EH_AlsoNotifyContactCodeDropEdit, "AWBHeaderManager.EH_AlsoNotifyContactCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyContactCode)));
			this.EH_AlsoNotifyContactCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 84, true);
			this.EH_AlsoNotifyContactCodeDropEdit.Name = "EH_AlsoNotifyContactCodeDropEdit";
			this.EH_AlsoNotifyContactCodeDropEdit.PreBoundMaxLength = 3;
			this.EH_AlsoNotifyContactCodeDropEdit.ShowDescriptionBox = false;
			this.EH_AlsoNotifyContactCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.EH_AlsoNotifyContactCodeDropEdit.TabIndex = 16;
			// 
			// EH_AlsoNotifyPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_AlsoNotifyPostCodeTextBox, "AWBHeaderManager.EH_AlsoNotifyPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyPostCode)));
			this.EH_AlsoNotifyPostCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|bcff3d50-f781-4388-ab8a-ab3cc9674abe", "P/Code");
			this.EH_AlsoNotifyPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 63, true);
			this.EH_AlsoNotifyPostCodeTextBox.Name = "EH_AlsoNotifyPostCodeTextBox";
			this.EH_AlsoNotifyPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.EH_AlsoNotifyPostCodeTextBox.TabIndex = 12;
			// 
			// NotifyContactDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyContactDetailTextBox, "AWBHeaderManager.EH_AlsoNotifyContactDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyContactDetail)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NotifyContactDetailTextBox, false);
			this.NotifyContactDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 84, true);
			this.NotifyContactDetailTextBox.Name = "NotifyContactDetailTextBox";
			this.NotifyContactDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.NotifyContactDetailTextBox.TabIndex = 17;
			// 
			// AlsoNotifyTraderTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AlsoNotifyTraderTypeTextBox, "AWBHeaderManager.EH_AlsoNotifyTraderNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyTraderNoType)));
			this.AlsoNotifyTraderTypeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|b8eda094-8891-4a7a-a255-002506dd286d", "Comp. ID");
			this.AlsoNotifyTraderTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 105, true);
			this.AlsoNotifyTraderTypeTextBox.Name = "AlsoNotifyTraderTypeTextBox";
			this.AlsoNotifyTraderTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.AlsoNotifyTraderTypeTextBox.TabIndex = 21;
			// 
			// AlsoNotifyTraderNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.AlsoNotifyTraderNoTextBox, "AWBHeaderManager.EH_AlsoNotifyTraderNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyTraderNo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AlsoNotifyTraderNoTextBox, false);
			this.AlsoNotifyTraderNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 105, true);
			this.AlsoNotifyTraderNoTextBox.Name = "AlsoNotifyTraderNoTextBox";
			this.AlsoNotifyTraderNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.AlsoNotifyTraderNoTextBox.TabIndex = 23;
			// 
			// NotifyCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyCountryTextBox, "AWBHeaderManager.EH_AlsoNotifyCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyCountryCode)));
			this.NotifyCountryTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|2274a18f-73d2-42eb-b5d0-ab270d9fbf0c", "Country/Region");
			this.NotifyCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 84, true);
			this.NotifyCountryTextBox.Name = "NotifyCountryTextBox";
			this.NotifyCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.NotifyCountryTextBox.TabIndex = 14;
			// 
			// NotifyCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyCityTextBox, "AWBHeaderManager.EH_AlsoNotifyPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyPlace)));
			this.NotifyCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 63, true);
			this.NotifyCityTextBox.Name = "NotifyCityTextBox";
			this.NotifyCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.NotifyCityTextBox.TabIndex = 8;
			// 
			// NotifyStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyStateTextBox, "AWBHeaderManager.EH_AlsoNotifyState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyState)));
			this.NotifyStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 63, true);
			this.NotifyStateTextBox.Name = "NotifyStateTextBox";
			this.NotifyStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.NotifyStateTextBox.TabIndex = 10;
			// 
			// NotifyAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyAddressTextBox, "AWBHeaderManager.EH_AlsoNotifyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyAddress)));
			this.NotifyAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 42, true);
			this.NotifyAddressTextBox.Name = "NotifyAddressTextBox";
			this.NotifyAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 20, true);
			this.NotifyAddressTextBox.TabIndex = 6;
			this.NotifyAddressTextBox.TextChanged += new System.EventHandler(this.ShipperNameTextBox_TextChanged);
			// 
			// NotifyContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyContactNameTextBox, "AWBHeaderManager.EH_AlsoNotifyContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyContactName)));
			this.NotifyContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 21, true);
			this.NotifyContactNameTextBox.Name = "NotifyContactNameTextBox";
			this.NotifyContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NotifyContactNameTextBox.TabIndex = 4;
			// 
			// NotifyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyNameTextBox, "AWBHeaderManager.EH_AlsoNotifyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AlsoNotifyName)));
			this.NotifyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 21, true);
			this.NotifyNameTextBox.Name = "NotifyNameTextBox";
			this.NotifyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.NotifyNameTextBox.TabIndex = 3;
			this.NotifyNameTextBox.TextChanged += new System.EventHandler(this.ShipperNameTextBox_TextChanged);
			// 
			// NotifyAddressOverrideTabPage
			// 
			this.NotifyAddressOverrideTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|84ab525b-2b6d-4c6f-a576-33628b2b3b43", "Notify Override");
			this.NotifyAddressOverrideTabPage.Controls.Add(this.EH_NotifyOverride5TextBox);
			this.NotifyAddressOverrideTabPage.Controls.Add(this.EH_NotifyOverride4TextBox);
			this.NotifyAddressOverrideTabPage.Controls.Add(this.EH_NotifyOverride3TextBox);
			this.NotifyAddressOverrideTabPage.Controls.Add(this.EH_NotifyOverride2TextBox);
			this.NotifyAddressOverrideTabPage.Controls.Add(this.EH_NotifyOverride1TextBox);
			this.NotifyAddressOverrideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotifyAddressOverrideTabPage.Name = "NotifyAddressOverrideTabPage";
			this.NotifyAddressOverrideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 107, true);
			this.NotifyAddressOverrideTabPage.TabIndex = 2;
			// 
			// EH_NotifyOverride5TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NotifyOverride5TextBox, "AWBHeaderManager.EH_NotifyOverride5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NotifyOverride5)));
			this.EH_NotifyOverride5TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|cb5bd447-6f7b-4e77-93c9-320526897486", "Line 5");
			this.EH_NotifyOverride5TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 84, true);
			this.EH_NotifyOverride5TextBox.Name = "EH_NotifyOverride5TextBox";
			this.EH_NotifyOverride5TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_NotifyOverride5TextBox.TabIndex = 19;
			this.EH_NotifyOverride5TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_NotifyOverride4TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NotifyOverride4TextBox, "AWBHeaderManager.EH_NotifyOverride4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NotifyOverride4)));
			this.EH_NotifyOverride4TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|2c10da90-8f76-4d9d-9b36-ac1514362b34", "Line 4");
			this.EH_NotifyOverride4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 63, true);
			this.EH_NotifyOverride4TextBox.Name = "EH_NotifyOverride4TextBox";
			this.EH_NotifyOverride4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_NotifyOverride4TextBox.TabIndex = 17;
			this.EH_NotifyOverride4TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_NotifyOverride3TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NotifyOverride3TextBox, "AWBHeaderManager.EH_NotifyOverride3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NotifyOverride3)));
			this.EH_NotifyOverride3TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|c3e24f3e-6ffc-45c7-b1d6-525137e76e79", "Line 3");
			this.EH_NotifyOverride3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 42, true);
			this.EH_NotifyOverride3TextBox.Name = "EH_NotifyOverride3TextBox";
			this.EH_NotifyOverride3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_NotifyOverride3TextBox.TabIndex = 15;
			this.EH_NotifyOverride3TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_NotifyOverride2TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NotifyOverride2TextBox, "AWBHeaderManager.EH_NotifyOverride2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NotifyOverride2)));
			this.EH_NotifyOverride2TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|f826ad01-c49f-424e-bebf-9b8388d2ea94", "Line 2");
			this.EH_NotifyOverride2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 21, true);
			this.EH_NotifyOverride2TextBox.Name = "EH_NotifyOverride2TextBox";
			this.EH_NotifyOverride2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_NotifyOverride2TextBox.TabIndex = 13;
			this.EH_NotifyOverride2TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// EH_NotifyOverride1TextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_NotifyOverride1TextBox, "AWBHeaderManager.EH_NotifyOverride1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_NotifyOverride1)));
			this.EH_NotifyOverride1TextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|29b7dc1b-69e1-4024-8106-d2951c25deb3", "Line 1");
			this.EH_NotifyOverride1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.EH_NotifyOverride1TextBox.Name = "EH_NotifyOverride1TextBox";
			this.EH_NotifyOverride1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.EH_NotifyOverride1TextBox.TabIndex = 11;
			this.EH_NotifyOverride1TextBox.TextChanged += new System.EventHandler(this.AddressOverrideTextBox_TextChange);
			// 
			// zLabel18
			// 
			this.zLabel18.AutoSize = true;
			this.zLabel18.BackColor = System.Drawing.SystemColors.Control;
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 536, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.zLabel18.TabIndex = 70;
			this.zLabel18.Text = "/";
			// 
			// BookingSecondFlightDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingSecondFlightDateTextBox, "AWBHeaderManager.EH_Booking2ndFlightDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking2ndFlightDate)));
			this.BookingSecondFlightDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 534, true);
			this.BookingSecondFlightDateTextBox.Name = "BookingSecondFlightDateTextBox";
			this.BookingSecondFlightDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.BookingSecondFlightDateTextBox.TabIndex = 70;
			// 
			// BookingSecondFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingSecondFlightTextBox, "AWBHeaderManager.EH_Booking2ndFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking2ndFlight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingSecondFlightTextBox, false);
			this.BookingSecondFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 534, true);
			this.BookingSecondFlightTextBox.Name = "BookingSecondFlightTextBox";
			this.BookingSecondFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.BookingSecondFlightTextBox.TabIndex = 68;
			// 
			// BookingSecondCarrierTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingSecondCarrierTextBox, "AWBHeaderManager.EH_Booking2ndCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking2ndCarrier)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingSecondCarrierTextBox, false);
			this.BookingSecondCarrierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 514, true);
			this.BookingSecondCarrierTextBox.Name = "BookingSecondCarrierTextBox";
			this.BookingSecondCarrierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.BookingSecondCarrierTextBox.TabIndex = 69;
			// 
			// zLabel17
			// 
			this.zLabel17.AutoSize = true;
			this.zLabel17.BackColor = System.Drawing.SystemColors.Control;
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 536, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.zLabel17.TabIndex = 67;
			this.zLabel17.Text = "/";
			// 
			// BookingFirstFlightDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingFirstFlightDateTextBox, "AWBHeaderManager.EH_Booking1stFlightDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking1stFlightDate)));
			this.BookingFirstFlightDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 534, true);
			this.BookingFirstFlightDateTextBox.Name = "BookingFirstFlightDateTextBox";
			this.BookingFirstFlightDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.BookingFirstFlightDateTextBox.TabIndex = 66;
			// 
			// BookingFirstFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingFirstFlightTextBox, "AWBHeaderManager.EH_Booking1stFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking1stFlight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingFirstFlightTextBox, false);
			this.BookingFirstFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 534, true);
			this.BookingFirstFlightTextBox.Name = "BookingFirstFlightTextBox";
			this.BookingFirstFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.BookingFirstFlightTextBox.TabIndex = 65;
			// 
			// BookingFirstCarrierTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingFirstCarrierTextBox, "AWBHeaderManager.EH_Booking1stCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Booking1stCarrier)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingFirstCarrierTextBox, false);
			this.BookingFirstCarrierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 514, true);
			this.BookingFirstCarrierTextBox.Name = "BookingFirstCarrierTextBox";
			this.BookingFirstCarrierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.BookingFirstCarrierTextBox.TabIndex = 64;
			// 
			// AirlinePrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirlinePrefixTextBox, "AWBHeaderManager.EH_AirlinePrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AirlinePrefix)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AirlinePrefixTextBox, false);
			this.AirlinePrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.AirlinePrefixTextBox.Name = "AirlinePrefixTextBox";
			this.AirlinePrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AirlinePrefixTextBox.TabIndex = 1;
			// 
			// OriginCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginCodeTextBox, "AWBHeaderManager.EH_AWBOriginCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AWBOriginCode)));
			this.OriginCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 8, true);
			this.OriginCodeTextBox.Name = "OriginCodeTextBox";
			this.OriginCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.OriginCodeTextBox.TabIndex = 2;
			// 
			// SerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "AWBHeaderManager.EH_AWBSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AWBSerialNo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SerialNumberTextBox, false);
			this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 8, true);
			this.SerialNumberTextBox.Name = "SerialNumberTextBox";
			this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.SerialNumberTextBox.TabIndex = 3;
			// 
			// WeightChargePPDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightChargePPDCalcEdit, "AWBHeaderManager.EH_TotalWeightPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalWeightPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WeightChargePPDCalcEdit, false);
			this.WeightChargePPDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1009, true);
			this.WeightChargePPDCalcEdit.Name = "WeightChargePPDCalcEdit";
			this.WeightChargePPDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WeightChargePPDCalcEdit.TabIndex = 194;
			this.WeightChargePPDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeightChargeCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightChargeCollectCalcEdit, "AWBHeaderManager.EH_TotalWeightCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TotalWeightCOL)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WeightChargeCollectCalcEdit, false);
			this.WeightChargeCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1009, true);
			this.WeightChargeCollectCalcEdit.Name = "WeightChargeCollectCalcEdit";
			this.WeightChargeCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WeightChargeCollectCalcEdit.TabIndex = 195;
			this.WeightChargeCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ValuationChargePPDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValuationChargePPDCalcEdit, "AWBHeaderManager.EH_ValuationPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ValuationPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValuationChargePPDCalcEdit, false);
			this.ValuationChargePPDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1052, true);
			this.ValuationChargePPDCalcEdit.Name = "ValuationChargePPDCalcEdit";
			this.ValuationChargePPDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ValuationChargePPDCalcEdit.TabIndex = 197;
			this.ValuationChargePPDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ValuationChargeCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValuationChargeCollectCalcEdit, "AWBHeaderManager.EH_ValuationCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ValuationCOL)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValuationChargeCollectCalcEdit, false);
			this.ValuationChargeCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1052, true);
			this.ValuationChargeCollectCalcEdit.Name = "ValuationChargeCollectCalcEdit";
			this.ValuationChargeCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ValuationChargeCollectCalcEdit.TabIndex = 198;
			this.ValuationChargeCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxChargeCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxChargeCollectCalcEdit, "AWBHeaderManager.EH_TaxesCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TaxesCOL)));
			this.TaxChargeCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1095, true);
			this.TaxChargeCollectCalcEdit.Name = "TaxChargeCollectCalcEdit";
			this.TaxChargeCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TaxChargeCollectCalcEdit.TabIndex = 201;
			this.TaxChargeCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxChargePPDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxChargePPDCalcEdit, "AWBHeaderManager.EH_TaxesPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_TaxesPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TaxChargePPDCalcEdit, false);
			this.TaxChargePPDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1095, true);
			this.TaxChargePPDCalcEdit.Name = "TaxChargePPDCalcEdit";
			this.TaxChargePPDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TaxChargePPDCalcEdit.TabIndex = 200;
			this.TaxChargePPDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherChargeAgentCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherChargeAgentCollectCalcEdit, "AWBHeaderManager.EH_OtherChargesDueAgentCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OtherChargesDueAgentCOL)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherChargeAgentCollectCalcEdit, false);
			this.OtherChargeAgentCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1139, true);
			this.OtherChargeAgentCollectCalcEdit.Name = "OtherChargeAgentCollectCalcEdit";
			this.OtherChargeAgentCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OtherChargeAgentCollectCalcEdit.TabIndex = 204;
			this.OtherChargeAgentCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherChargeAgentPPDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherChargeAgentPPDCalcEdit, "AWBHeaderManager.EH_OtherChargesDueAgentPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OtherChargesDueAgentPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherChargeAgentPPDCalcEdit, false);
			this.OtherChargeAgentPPDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1139, true);
			this.OtherChargeAgentPPDCalcEdit.Name = "OtherChargeAgentPPDCalcEdit";
			this.OtherChargeAgentPPDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OtherChargeAgentPPDCalcEdit.TabIndex = 203;
			this.OtherChargeAgentPPDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherChargeCarrierCollectCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherChargeCarrierCollectCalcEdit, "AWBHeaderManager.EH_OtherChargesDueCarrierCOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OtherChargesDueCarrierCOL)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherChargeCarrierCollectCalcEdit, false);
			this.OtherChargeCarrierCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1182, true);
			this.OtherChargeCarrierCollectCalcEdit.Name = "OtherChargeCarrierCollectCalcEdit";
			this.OtherChargeCarrierCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OtherChargeCarrierCollectCalcEdit.TabIndex = 207;
			this.OtherChargeCarrierCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OtherChargeCarrierPPDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherChargeCarrierPPDCalcEdit, "AWBHeaderManager.EH_OtherChargesDueCarrierPPD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_OtherChargesDueCarrierPPD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherChargeCarrierPPDCalcEdit, false);
			this.OtherChargeCarrierPPDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 1182, true);
			this.OtherChargeCarrierPPDCalcEdit.Name = "OtherChargeCarrierPPDCalcEdit";
			this.OtherChargeCarrierPPDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OtherChargeCarrierPPDCalcEdit.TabIndex = 206;
			this.OtherChargeCarrierPPDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InsuranceValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InsuranceValueCalcEdit, "AWBHeaderManager.EH_InsuranceValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_InsuranceValue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InsuranceValueCalcEdit, false);
			this.InsuranceValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 532, true);
			this.InsuranceValueCalcEdit.Name = "InsuranceValueCalcEdit";
			this.InsuranceValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.InsuranceValueCalcEdit.TabIndex = 73;
			this.InsuranceValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsValueCalcEdit, "AWBHeaderManager.EH_CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_CustomsValue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsValueCalcEdit, false);
			this.CustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 488, true);
			this.CustomsValueCalcEdit.Name = "CustomsValueCalcEdit";
			this.CustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.CustomsValueCalcEdit.TabIndex = 59;
			this.CustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DeclaredValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeclaredValueCalcEdit, "AWBHeaderManager.EH_DeclaredValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_DeclaredValue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeclaredValueCalcEdit, false);
			this.DeclaredValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(676, 488, true);
			this.DeclaredValueCalcEdit.Name = "DeclaredValueCalcEdit";
			this.DeclaredValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.DeclaredValueCalcEdit.TabIndex = 57;
			this.DeclaredValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShippersSignatureTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShippersSignatureTextBox, "AWBHeaderManager.EH_ShippersSignature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_ShippersSignature)));
			this.ShippersSignatureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 1209, true);
			this.ShippersSignatureTextBox.Name = "ShippersSignatureTextBox";
			this.ShippersSignatureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ShippersSignatureTextBox.TabIndex = 211;
			// 
			// AgentsSignatureTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentsSignatureTextBox, "AWBHeaderManager.EH_AWBAgentsSignature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AWBAgentsSignature)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AgentsSignatureTextBox, false);
			this.AgentsSignatureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 1290, true);
			this.AgentsSignatureTextBox.Name = "AgentsSignatureTextBox";
			this.AgentsSignatureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.AgentsSignatureTextBox.TabIndex = 222;
			// 
			// IssueCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssueCityTextBox, "AWBHeaderManager.EH_AWBIssuePlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AWBIssuePlace)));
			this.IssueCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 1290, true);
			this.IssueCityTextBox.Name = "IssueCityTextBox";
			this.IssueCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.IssueCityTextBox.TabIndex = 221;
			// 
			// AWBIssueDateDateEdit
			// 
			this.AWBIssueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AWBIssueDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AWBIssueDateDateEdit, "AWBHeaderManager.EH_AWBIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AWBIssueDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AWBIssueDateDateEdit, false);
			this.AWBIssueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 1290, true);
			this.AWBIssueDateDateEdit.Name = "AWBIssueDateDateEdit";
			this.AWBIssueDateDateEdit.TabIndex = 220;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "AWBHeaderManager.EH_Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_Currency)));
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 488, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.CurrencyTextBox.TabIndex = 50;
			// 
			// AgentCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentCityTextBox, "AWBHeaderManager.EH_AgentPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AgentPlace)));
			this.AgentCityTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|e6be524f-584b-47ef-873e-35e9199a55a8", "City");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AgentCityTextBox, false);
			this.AgentCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 352, true);
			this.AgentCityTextBox.Name = "AgentCityTextBox";
			this.AgentCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.AgentCityTextBox.TabIndex = 24;
			// 
			// AgentsIATACodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentsIATACodeTextBox, "AWBHeaderManager.EH_AgentIATACodeFormatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AgentIATACodeFormatted)));
			this.AgentsIATACodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|c6cb83ab-bcee-4a40-89dd-fa535d652ebc", "Agents IATA Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AgentsIATACodeTextBox, false);
			this.AgentsIATACodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 400, true);
			this.AgentsIATACodeTextBox.Name = "AgentsIATACodeTextBox";
			this.AgentsIATACodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.AgentsIATACodeTextBox.TabIndex = 26;
			// 
			// AgentsAccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentsAccountNumberTextBox, "AWBHeaderManager.EH_AgentAccountNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AgentAccountNo)));
			this.AgentsAccountNumberTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|318e711c-c630-49a1-9681-11010cea15aa", "Account No");
			this.AgentsAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 400, true);
			this.AgentsAccountNumberTextBox.Name = "AgentsAccountNumberTextBox";
			this.AgentsAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.AgentsAccountNumberTextBox.TabIndex = 28;
			// 
			// AgentNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentNameTextBox, "AWBHeaderManager.EH_AgentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AgentName)));
			this.AgentNameTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|b51a9819-5808-4d51-8ac0-2a5430323c60", "Name");
			this.AgentNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 324, true);
			this.AgentNameTextBox.Name = "AgentNameTextBox";
			this.AgentNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.AgentNameTextBox.TabIndex = 22;
			// 
			// HandlingInformationTextBox
			// 
			this.HandlingInformationTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.HandlingInformationTextBox, "AWBHeaderManager.EH_HandlingInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_HandlingInformation)));
			this.HandlingInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 572, true);
			this.HandlingInformationTextBox.Multiline = true;
			this.HandlingInformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.HandlingInformationTextBox.Name = "HandlingInformationTextBox";
			this.HandlingInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 32, true);
			this.HandlingInformationTextBox.TabIndex = 76;
			// 
			// OverrideRateSectionCheckBox
			//
			this.BindingSource.SetBindingMember(this.OverrideRateSectionCheckBox, "AWBHeaderManager.EH_AreRateLinesOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AreRateLinesOverridden)));
			this.OverrideRateSectionCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|8ff837a4-dd0b-42e0-8373-5d5cb948f8e5", "Override Rate Section");
			this.OverrideRateSectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideRateSectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 604, true);
			this.OverrideRateSectionCheckBox.Name = "OverrideRateSectionCheckBox";
			this.OverrideRateSectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.OverrideRateSectionCheckBox.TabIndex = 78;
			this.OverrideRateSectionCheckBox.UseVisualStyleBackColor = false;
			// 
			// DestinationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationTextBox, "AWBHeaderManager.EH_AirportOfDestinationText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AirportOfDestinationText)));
			this.DestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 532, true);
			this.DestinationTextBox.Name = "DestinationTextBox";
			this.DestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DestinationTextBox.TabIndex = 61;
			// 
			// AirportOfDepartureTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirportOfDepartureTextBox, "AWBHeaderManager.EH_AirportOfDepartureAndRequestRouteText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_AirportOfDepartureAndRequestRouteText)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AirportOfDepartureTextBox, false);
			this.AirportOfDepartureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 444, true);
			this.AirportOfDepartureTextBox.Name = "AirportOfDepartureTextBox";
			this.AirportOfDepartureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.AirportOfDepartureTextBox.TabIndex = 30;
			// 
			// ToFirstTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToFirstTextBox, "AWBHeaderManager.EH_To1st");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_To1st)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToFirstTextBox, false);
			this.ToFirstTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 488, true);
			this.ToFirstTextBox.Name = "ToFirstTextBox";
			this.ToFirstTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ToFirstTextBox.TabIndex = 37;
			// 
			// ToSecondTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToSecondTextBox, "AWBHeaderManager.EH_To2nd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_To2nd)));
			this.ToSecondTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 488, true);
			this.ToSecondTextBox.Name = "ToSecondTextBox";
			this.ToSecondTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ToSecondTextBox.TabIndex = 42;
			// 
			// BySecondTextBox
			// 
			this.BindingSource.SetBindingMember(this.BySecondTextBox, "AWBHeaderManager.EH_By2nd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_By2nd)));
			this.BySecondTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 488, true);
			this.BySecondTextBox.Name = "BySecondTextBox";
			this.BySecondTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.BySecondTextBox.TabIndex = 44;
			// 
			// ToThirdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToThirdTextBox, "AWBHeaderManager.EH_To3rd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_To3rd)));
			this.ToThirdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 488, true);
			this.ToThirdTextBox.Name = "ToThirdTextBox";
			this.ToThirdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ToThirdTextBox.TabIndex = 46;
			// 
			// ByThirdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ByThirdTextBox, "AWBHeaderManager.EH_By3rd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).EH_By3rd)));
			this.ByThirdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 488, true);
			this.ByThirdTextBox.Name = "ByThirdTextBox";
			this.ByThirdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ByThirdTextBox.TabIndex = 48;
			// 
			// AWBTopPanel
			// 
			this.AWBTopPanel.BackColor = System.Drawing.Color.White;
			this.AWBTopPanel.Controls.Add(this.OverrideValuesCheckBox);
			this.AWBTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AWBTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBTopPanel.Name = "AWBTopPanel";
			this.AWBTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 32, true);
			this.AWBTopPanel.TabIndex = 0;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBUserControl|d759c11c-5fbf-42e8-a50b-e6db18ccfce5", "Check this if you want to override default values");
			this.OverrideValuesCheckBox.Click += new System.EventHandler(this.OverrideValuesCheckBox_Click);
			this.OverrideValuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.OverrideValuesCheckBox.Name = "OverrideValuesCheckBox";
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 24, true);
			this.OverrideValuesCheckBox.TabIndex = 0;
			this.OverrideValuesCheckBox.UseVisualStyleBackColor = false;
			// 
			// AWBUserControl
			// 
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "AWBUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 1379, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.AWBBottomPanel.ResumeLayout(false);

			this.ConsigneeNameAndAddressTabControl.ResumeLayout(false);
			this.ConsigneeAddressTabControl.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.asAgreedPanel.ResumeLayout(false);
			this.asAgreedPanel.PerformLayout();
			this.AWBPanel.ResumeLayout(false);
			this.AWBPanel.PerformLayout();
			this.ConsigneeOverrideTabPage.ResumeLayout(false);
			this.ConsigneeOverrideTabPage.PerformLayout();
			this.ShipperAddressTabControl.ResumeLayout(false);
			this.ShipperNameAndAddressTabPage.ResumeLayout(false);
			this.ShippersPanel.ResumeLayout(false);
			this.ShippersPanel.PerformLayout();
			this.ShipperNameAndAddressOverrideTabPage.ResumeLayout(false);
			this.ShipperNameAndAddressOverrideTabPage.PerformLayout();
			this.AccountingInformationTabControl.ResumeLayout(false);
			this.AccountingInformationTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AccountingInformationGrid)).EndInit();
			this.NotifyPartyInfoTabPage.ResumeLayout(false);
			this.NotifyPartyInfoTabPage.PerformLayout();
			this.NotifyAddressOverrideTabPage.ResumeLayout(false);
			this.NotifyAddressOverrideTabPage.PerformLayout();
			this.AWBTopPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
