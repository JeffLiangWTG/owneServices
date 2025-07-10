using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBControl runat=server></{0}:AWBControl>")]
	public class AWBControl : AWBBaseControl
	{
		#region Constructors

		public AWBControl()
			: base()
		{
			CssClass = "AWBEditor";
		}

		#endregion
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			AWBHeaderBox = new AWBHeaderControl();
			AWBHeaderBox.ID = "AWBHeader";

			AWBNumberTextBox = new ZTextBox();
			AWBNumberTextBox.ID = "AWBNumber";

			ShipperAddressBox = new AWBAddressControl();
			ShipperAddressBox.ID = "ShipperAddress";

			ConsigneeAddressBox = new AWBAddressControl();
			ConsigneeAddressBox.ID = "ConsigneeAddress";

			IssuedByBox = new AWBIssuedControl();
			IssuedByBox.ID = "IssuedBy";

			NetRateBox = new AWBTextBox();
			NetRateBox.ID = "NetRate";

			AlsoNotifyBox = new AWBAddressControl();
			AlsoNotifyBox.ID = "AlsoNotify";

			CarrierBox = new AWBCarrierControl();
			CarrierBox.ID = (NoResString)"Carrier";

			AgentsIATABox = new AWBTextBox();
			AgentsIATABox.ID = "AgentsIATA";

			AccountNoBox = new AWBTextBox();
			AccountNoBox.ID = "AccountNo";

			SetupGrids();

			RoutingBox = new AWBRoutingControl();
			RoutingBox.ID = (NoResString)"Routing";

			DepartureAirportBox = new AWBTextBox();
			DepartureAirportBox.ID = "DepartureAirport";

			ShippingReferenceBox = new AWBShippingReferenceControl();
			ShippingReferenceBox.ID = "ShippingReference";

			DeclaredValuesBox = new AWBDeclaredValuesControl();
			DeclaredValuesBox.ID = "DeclaredValues";

			DestinationAirportBox = new AWBDestinationControl();
			DestinationAirportBox.ID = "DestinationAirport";

			RequestedFlightBox = new AWBRequestedFlightControl();
			RequestedFlightBox.ID = "RequestedFlight";

			InsuranceAmountBox = new AWBNumericTextBox();
			InsuranceAmountBox.ID = "InsuranceAmount";

			InsuranceInfoBox = new ZTextLabel();

			HandlingInfoBox = new AWBHandlingInfoControl();
			HandlingInfoBox.ID = "HandlingInfo";

			RateLinesBox = new AWBRateLinesControl();
			RateLinesBox.ID = "RateLines";

			BottomBox = new AWBBottomSectionControl();
			EmptyChargesBox = new AWBPrepaidCollectControl();
		}

		protected override void CreateChildControls()
		{
			AWBNumberTextBox.BindTo = "EH_ReferenceNumber";
			AWBNumberTextBox.Width = 150;

			ShipperAddressBox.Caption = Res.GetString("5dc5114a-71c0-411f-83af-de139d2e15c9", "Shipper's Name and Address");
			ShipperAddressBox.AddressType = AWBAddressType.Shipper;
			ShipperAddressBox.AddressLineCssClass = "LeftLine";

			ConsigneeAddressBox.Caption = Res.GetString("aef98e42-db7e-4ffd-9dca-98b3ceeaca36", "Consignee's Name and Address");
			ConsigneeAddressBox.AddressType = AWBAddressType.Consignee;
			ConsigneeAddressBox.AddressLineCssClass = "LeftLine";

			IssuedByBox.Caption = "&nbsp;";
			IssuedByBox.LineCssClass = "RightLine";

			NetRateBox.CaptionVisible = false;
			NetRateBox.BindTo = "EH_NetRateCode";
			NetRateBox.FieldCssClass = "AWBNetRate";
			NetRateBox.FieldCaption = Res.GetString("3ddc25fd-ac60-4002-9ef8-f5314b349f9f", "Net Rate:");

			AlsoNotifyBox.Caption = Res.GetString("b05fa6ad-2038-4eb1-870d-34e04cb98b25", "Also Notify");
			AlsoNotifyBox.AddressType = AWBAddressType.AlsoNotify;
			AlsoNotifyBox.AddressLineCssClass = "RightLine";

			CarrierBox.Caption = Res.GetString("067aaa19-f1a1-4c39-b1d6-0f338fa26fc3", "Issuing Carrier's Agent Name and City");
			CarrierBox.LineCssClass = "LeftLine";

			AgentsIATABox.Caption = Res.GetString("f874852b-d988-450f-9d16-6bfdc976e0e3", "Agents IATA Code");
			AgentsIATABox.BindTo = "EH_AgentIATACodeFormatted";
			AgentsIATABox.FieldCaption = "&nbsp;";
			AgentsIATABox.FieldCssClass = "AgentsIATALine";

			AccountNoBox.Caption = Res.GetString("e5694fa1-31e1-454c-88cd-f04183ca9dec", "Account No.");
			AccountNoBox.BindTo = "EH_AgentAccountNo";
			AccountNoBox.FieldCaption = "&nbsp;";
			AccountNoBox.FieldCssClass = "AccountNoLine";
			AccountNoBox.FieldCaptionVisible = false;
			AccountNoBox.DataFieldAlign = HorizontalAlign.Center;

			AccountingInfoGrid.BindTo = "AWBAccountingInformations";
			AccountingInfoGrid.Caption = Res.GetString("1658d772-f013-4828-a9fd-d18d678a3035", "Accounting Info");
			AccountingInfoGrid.CssClass = "DetailsTable";
			AccountingInfoGrid.ItemStyle.CssClass = "DetailsCell";
			AccountingInfoGrid.HeaderStyle.CssClass = "DetailsHeader";

			SpecialHandlingGrid.BindTo = "AWBSpecialHandlingItems";
			SpecialHandlingGrid.Caption = Res.GetString("6c49fa83-7395-4961-bb72-0c257cf32b34", "Special Handling Code");
			SpecialHandlingGrid.CssClass = "DetailsTable";
			SpecialHandlingGrid.ItemStyle.CssClass = "DetailsCell";
			SpecialHandlingGrid.HeaderStyle.CssClass = "DetailsHeader";

			DepartureAirportBox.Caption = Res.GetString("be8d7693-3206-42db-8acf-2099ad007a2c", "Airport of Departure (Addr. of First Carrier) and Requested Routing");
			DepartureAirportBox.BindTo = "EH_AirportOfDepartureAndRequestRouteText";

			RequestedFlightBox.Caption = Res.GetString("7eea889e-ec16-4dee-bec8-3b744c87ad1c", "Requested Flight");
			RequestedFlightBox.FieldCssClass = "AWBRequestedFlights";
			RequestedFlightBox.FieldCaptionVisible = false;

			InsuranceAmountBox.BindTo = "EH_InsuranceValue";
			InsuranceAmountBox.Caption = Res.GetString("2af447ff-e1b5-4912-bd8c-232af2d6bbf3", "Amount of Insurance");
			InsuranceAmountBox.FieldCaptionVisible = false;
			InsuranceAmountBox.FieldCssClass = "AWBText10";
			InsuranceAmountBox.DataFieldAlign = HorizontalAlign.Center;

			InsuranceInfoBox.Text = Res.GetString("1803f2b1-1e44-4d61-a8b1-91584ba357a0", "INSURANCE - If carrier offers insurance, and such insurance is requested in accordance with the conditions thereof, indicate amount to be insured in figures in box marked 'Amount of Insurance'");
			InsuranceInfoBox.CssClass = "AWBInsuranceInfo";

			EmptyChargesBox.PrepaidFieldVisible = false;
			EmptyChargesBox.CollectFieldVisible = false;
			EmptyChargesBox.PrepaidCaption = Res.GetString("7f487f78-05f1-4532-8935-31d8f3f6f090", "Destination");
			EmptyChargesBox.CollectCaption = Res.GetString("19b2f458-e538-4ccf-a1ab-c5c5dffc088d", "Total Collect");
			EmptyChargesBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder AWBLastChargeField";
			EmptyChargesBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";

			base.CreateChildControls();
			CreateLayoutTable();

			HeaderCellLeft.Controls.Add(AWBHeaderBox);
			HeaderCellRight.Text = "&nbsp;";
											 //HeaderCellRight.Controls.Add(AWBNumberTextBox);

			ShipperAddressCell.Controls.Add(ShipperAddressBox);
			ConsigneeAddressCell.Controls.Add(ConsigneeAddressBox);
			IssuedByCell.Controls.Add(IssuedByBox);

			NetRateCell.Controls.Add(NetRateBox);

			AlsoNotifyCell.Controls.Add(AlsoNotifyBox);
			CarrierCell.Controls.Add(CarrierBox);
			AgentCell.Controls.Add(AgentsIATABox);
			AccountCell.Controls.Add(AccountNoBox);

			AccountingInfoCell.Controls.Add(AccountingInfoGrid);
			SpecialHandlingCell.Controls.Add(SpecialHandlingGrid);

			DepartureAirportCell.Controls.Add(DepartureAirportBox);
			ShippingReferenceCell.Controls.Add(ShippingReferenceBox);

			RoutingCell.Controls.Add(RoutingBox);
			DeclaredValuesCell.Controls.Add(DeclaredValuesBox);

			DestinationAirportCell.Controls.Add(DestinationAirportBox);
			RequestedFlightCell.Controls.Add(RequestedFlightBox);

			InsuranceAmountCell.Controls.Add(InsuranceAmountBox);
			InsuranceInfoCell.Controls.Add(InsuranceInfoBox);

			HandlingInfoCell.Controls.Add(HandlingInfoBox);
			RateLinesCell.Controls.Add(RateLinesBox);

			BottomCell.Controls.Add(BottomBox);

			Controls.Clear();
			Controls.Add(LayoutTable);

			AccountingInfoGrid.ColumnProvider = new TrackingAWBAccountingInfoColumnProvider();
			SpecialHandlingGrid.ColumnProvider = new TrackingAWBSpecialHandlingColumnProvider();

			AWBHandlingCodeGridAddOn handlingCodeGridAddOn = new AWBHandlingCodeGridAddOn(ExportAWBSpecialHandling.Schema.EP_SpecialHandling, "SpecialHandlingDescription");
			Controls.Add(handlingCodeGridAddOn);
			handlingCodeGridAddOn.Grid = SpecialHandlingGrid;
		}

		protected override void BindToAWBHeader(ExportAWBHeader source)
		{
			base.BindToAWBHeader(source);
			BottomBox.Bind(source);
		}

		void CreateLayoutTable()
		{
			LayoutTable = new Table();
			LayoutTable.CellPadding = 0;
			LayoutTable.CellSpacing = 0;

			#region Header

			TableRow headerRow = new TableRow();

			HeaderCellLeft = new TableCell();
			HeaderCellLeft.ColumnSpan = 2;
			HeaderCellLeft.HorizontalAlign = HorizontalAlign.Left;

			HeaderCellRight = new TableCell();
			HeaderCellRight.ColumnSpan = 2;
			HeaderCellRight.HorizontalAlign = HorizontalAlign.Right;

			headerRow.Cells.Add(HeaderCellLeft);
			headerRow.Cells.Add(HeaderCellRight);

			LayoutTable.Rows.Add(headerRow);

			#endregion

			#region Shipper Address

			TableRow shipperRow = new TableRow();
			ShipperAddressCell = new TableCell();
			ShipperAddressCell.CssClass = "AWBSection";
			ShipperAddressCell.RowSpan = 2;
			ShipperAddressCell.ColumnSpan = 2;

			#endregion

			#region Consignee Address

			TableRow consigneeRow = new TableRow();
			ConsigneeAddressCell = new TableCell();
			ConsigneeAddressCell.CssClass = "AWBSection";
			ConsigneeAddressCell.ColumnSpan = 2;

			#endregion

			#region Issued By

			IssuedByCell = new TableCell();
			IssuedByCell.CssClass = "AWBSection";
			IssuedByCell.ColumnSpan = 2;

			#endregion

			#region Net Rate

			TableRow netRateRow = new TableRow();
			NetRateCell = new TableCell();
			NetRateCell.CssClass = (NoResString)"AWBSection NetRateSection";
			NetRateCell.ColumnSpan = 2;

			#endregion

			#region Also Notify

			AlsoNotifyCell = new TableCell();
			AlsoNotifyCell.CssClass = "AWBSection";
			AlsoNotifyCell.ColumnSpan = 2;

			#endregion

			#region Special Handling

			SpecialHandlingCell = new TableCell();
			SpecialHandlingCell.CssClass = "AWBSection";
			SpecialHandlingCell.ColumnSpan = 2;

			#endregion

			#region Accounting Info

			AccountingInfoCell = new TableCell();
			AccountingInfoCell.CssClass = "AWBSection";
			AccountingInfoCell.ColumnSpan = 2;

			#endregion

			#region Carrier

			TableRow carrierRow = new TableRow();
			CarrierCell = new TableCell();
			CarrierCell.CssClass = "AWBSection";
			CarrierCell.ColumnSpan = 2;

			#endregion

			#region Agent and Account

			TableRow agentRow = new TableRow();
			AgentCell = new TableCell();
			AgentCell.CssClass = "AWBSection";

			AccountCell = new TableCell();
			AccountCell.CssClass = "AWBSection";

			#endregion

			shipperRow.Cells.Add(ShipperAddressCell);
			shipperRow.Cells.Add(IssuedByCell);

			netRateRow.Cells.Add(NetRateCell);

			consigneeRow.Cells.Add(ConsigneeAddressCell);
			consigneeRow.Cells.Add(AlsoNotifyCell);

			carrierRow.Cells.Add(CarrierCell);
			carrierRow.Cells.Add(AccountingInfoCell);

			agentRow.Cells.Add(AgentCell);
			agentRow.Cells.Add(AccountCell);
			agentRow.Cells.Add(SpecialHandlingCell);

			#region DepartureAirport

			TableRow departureAirportRow = new TableRow();
			DepartureAirportCell = new TableCell();
			DepartureAirportCell.CssClass = "AWBSection";
			DepartureAirportCell.ColumnSpan = 2;

			#endregion

			#region ShippingReference

			ShippingReferenceCell = new TableCell();
			ShippingReferenceCell.CssClass = "AWBSection";
			ShippingReferenceCell.ColumnSpan = 2;

			#endregion

			departureAirportRow.Cells.Add(DepartureAirportCell);
			departureAirportRow.Cells.Add(ShippingReferenceCell);

			#region Routing

			TableRow routingRow = new TableRow();
			RoutingCell = new TableCell();
			RoutingCell.CssClass = "AWBSection";
			RoutingCell.ColumnSpan = 2;

			#endregion

			#region DeclaredValues

			DeclaredValuesCell = new TableCell();
			DeclaredValuesCell.CssClass = "AWBSection";
			DeclaredValuesCell.ColumnSpan = 2;

			#endregion

			routingRow.Cells.Add(RoutingCell);
			routingRow.Cells.Add(DeclaredValuesCell);

			#region DestinationAirport

			TableRow destinationAirportRow = new TableRow();
			DestinationAirportCell = new TableCell();
			DestinationAirportCell.CssClass = "AWBSectionHighlighted";

			#endregion

			#region RequestedFlight

			RequestedFlightCell = new TableCell();
			RequestedFlightCell.CssClass = "AWBSection";

			#endregion

			#region InsuranceAmount

			InsuranceAmountCell = new TableCell();
			InsuranceAmountCell.CssClass = "AWBSection";

			#endregion

			#region InsuranceInfo

			InsuranceInfoCell = new TableCell();
			InsuranceInfoCell.CssClass = (NoResString)"AWBSection AWBInsuranceInfo";

			#endregion

			destinationAirportRow.Cells.Add(DestinationAirportCell);
			destinationAirportRow.Cells.Add(RequestedFlightCell);
			destinationAirportRow.Cells.Add(InsuranceAmountCell);
			destinationAirportRow.Cells.Add(InsuranceInfoCell);

			#region HandlingInfo

			TableRow handlingInfoRow = new TableRow();
			HandlingInfoCell = new TableCell();
			HandlingInfoCell.CssClass = "AWBSection";
			HandlingInfoCell.ColumnSpan = 4;
			handlingInfoRow.Cells.Add(HandlingInfoCell);

			#endregion

			#region AWBLines

			TableRow aWBLinesRow = new TableRow();
			RateLinesCell = new TableCell();
			RateLinesCell.CssClass = "AWBSection";
			RateLinesCell.ColumnSpan = 4;
			aWBLinesRow.Cells.Add(RateLinesCell);

			#endregion

			#region Bottom Section

			TableRow bottomRow = new TableRow();
			BottomCell = new TableCell();
			BottomCell.CssClass = "AWBSection";
			BottomCell.ColumnSpan = 4;
			bottomRow.Cells.Add(BottomCell);

			#endregion

			Table lastCellTable = new Table();
			lastCellTable.CellSpacing = 0;
			lastCellTable.CellPadding = 0;
			TableCell lastCellTableCell = new TableCell();
			lastCellTableCell.VerticalAlign = VerticalAlign.Top;
			lastCellTableCell.HorizontalAlign = HorizontalAlign.Left;
			lastCellTableCell.CssClass = (NoResString)"AWBLeftBorder AWBRightBorder AWBEmptyCell";
			lastCellTableCell.Controls.Add(EmptyChargesBox);
			lastCellTable.Rows.Add(new TableRow());
			lastCellTable.Rows[0].Cells.Add(lastCellTableCell);

			TableRow lastRow = new TableRow();
			TableCell lastCell = new TableCell();
			lastCell.Controls.Add(lastCellTable);

			lastCell.ColumnSpan = 4;
			lastRow.Cells.Add(lastCell);

			LayoutTable.Rows.Add(shipperRow);
			LayoutTable.Rows.Add(netRateRow);
			LayoutTable.Rows.Add(consigneeRow);
			LayoutTable.Rows.Add(carrierRow);
			LayoutTable.Rows.Add(agentRow);
			LayoutTable.Rows.Add(departureAirportRow);
			LayoutTable.Rows.Add(routingRow);
			LayoutTable.Rows.Add(destinationAirportRow);
			LayoutTable.Rows.Add(handlingInfoRow);
			LayoutTable.Rows.Add(aWBLinesRow);
			LayoutTable.Rows.Add(bottomRow);
			//LayoutTable.Rows.Add(LastRow);
		}

		#endregion

		#region Implementation

		void SetupGrids()
		{
			SetupAccountingGrid();
			SetupSpecialHandlingGrid();
		}

		void SetupAccountingGrid()
		{
			AccountingInfoGrid = new ZGrid();
			AccountingInfoGrid.ID = "AccountingInfo";
			AccountingInfoGrid.DisableCollapsing = true;
			AccountingInfoGrid.AutoGenerateColumns = false;
			AccountingInfoGrid.AllowPaging = false;
			AccountingInfoGrid.AllowAdd = true;
			AccountingInfoGrid.AllowDelete = true;
			AccountingInfoGrid.AllowEdit = true;
			AccountingInfoGrid.InitialRowsToDisplay = 4;
			AccountingInfoGrid.AutoSizeColumns = true;
		}

		void SetupSpecialHandlingGrid()
		{
			SpecialHandlingGrid = new ZGrid();
			SpecialHandlingGrid.ID = "SpecialHandling";
			SpecialHandlingGrid.DisableCollapsing = true;
			SpecialHandlingGrid.AutoGenerateColumns = false;
			SpecialHandlingGrid.AllowPaging = false;
			SpecialHandlingGrid.AllowAdd = true;
			SpecialHandlingGrid.AllowDelete = true;
			SpecialHandlingGrid.AllowEdit = true;
			SpecialHandlingGrid.InitialRowsToDisplay = 1;
			SpecialHandlingGrid.AutoSizeColumns = true;
		}

		AWBHeaderControl AWBHeaderBox;
		ZTextBox AWBNumberTextBox;
		AWBAddressControl ShipperAddressBox;
		AWBAddressControl ConsigneeAddressBox;
		AWBAddressControl AlsoNotifyBox;
		AWBIssuedControl IssuedByBox;
		AWBTextBox NetRateBox;
		AWBCarrierControl CarrierBox;
		AWBTextBox AgentsIATABox;
		AWBTextBox AccountNoBox;
		ZGrid AccountingInfoGrid;
		ZGrid SpecialHandlingGrid;
		AWBTextBox DepartureAirportBox;
		AWBShippingReferenceControl ShippingReferenceBox;
		AWBRoutingControl RoutingBox;
		AWBDeclaredValuesControl DeclaredValuesBox;
		AWBDestinationControl DestinationAirportBox;
		AWBRequestedFlightControl RequestedFlightBox;
		AWBNumericTextBox InsuranceAmountBox;
		ZTextLabel InsuranceInfoBox;
		AWBHandlingInfoControl HandlingInfoBox;
		AWBRateLinesControl RateLinesBox;
		AWBBottomSectionControl BottomBox;
		AWBPrepaidCollectControl EmptyChargesBox;

		Table LayoutTable;
		TableCell HeaderCellRight;
		TableCell HeaderCellLeft;
		TableCell ShipperAddressCell;
		TableCell ConsigneeAddressCell;
		TableCell IssuedByCell;
		TableCell NetRateCell;
		TableCell AlsoNotifyCell;
		TableCell SpecialHandlingCell;
		TableCell AccountingInfoCell;
		TableCell CarrierCell;
		TableCell AgentCell;
		TableCell AccountCell;
		TableCell DepartureAirportCell;
		TableCell ShippingReferenceCell;
		TableCell RoutingCell;
		TableCell DeclaredValuesCell;
		TableCell DestinationAirportCell;
		TableCell RequestedFlightCell;
		TableCell InsuranceAmountCell;
		TableCell InsuranceInfoCell;
		TableCell HandlingInfoCell;
		TableCell RateLinesCell;
		TableCell BottomCell;

		#endregion
	}
}
