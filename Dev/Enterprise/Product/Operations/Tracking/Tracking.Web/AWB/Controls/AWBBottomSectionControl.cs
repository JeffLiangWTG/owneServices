using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBBottomSectionControl runat=server></{0}:AWBBottomSectionControl>")]
	public class AWBBottomSectionControl : AWBBaseControl
	{
		#region Constructors

		public AWBBottomSectionControl()
			: base() { }

		#endregion
		#region Overrides

		protected override void BindToAWBHeader(ExportAWBHeader source)
		{
			base.BindToAWBHeader(source);
			OtherChargesGrid.Bind(source);
			DataBind();

			WeightBox.Bind(source);
			ValuationBox.Bind(source);
			TaxBox.Bind(source);
			AgentChargesBox.Bind(source);
			CarrierChargesBox.Bind(source);
			TotalChargesBox.Bind(source);
		}

		protected override void OnPreRender(EventArgs e)
		{
			Bind(BusinessEntity);
			base.OnPreRender(e);
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			WeightBox = new AWBPrepaidCollectControl();
			WeightBox.ID = (NoResString)"Weight";

			ValuationBox = new AWBPrepaidCollectControl();
			ValuationBox.ID = (NoResString)"Valuation";

			TaxBox = new AWBPrepaidCollectControl();
			TaxBox.ID = (NoResString)"Tax";

			SetupGrids();

			AgentChargesBox = new AWBPrepaidCollectControl();
			CarrierChargesBox = new AWBPrepaidCollectControl();

			ShipperSignatureBox = new AWBShipperSignatureControl();
			ShipperSignatureBox.ID = "ShipperSignature";

			TotalChargesBox = new AWBPrepaidCollectControl();
			EmptyCharges1Box = new AWBPrepaidCollectControl();
			CarrierSignatureBox = new AWBCarrierSignatureControl();
			CarrierSignatureBox.ID = "CarrierSignature";
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			WeightBox.PrepaidBindTo = "EH_TotalWeightPPD";
			WeightBox.CollectBindTo = "EH_TotalWeightCOL";
			WeightBox.Caption = Res.GetString("a76fffa4-b007-4ad6-8660-04a793965002", "Weight Charge");
			WeightBox.PrepaidCaption = Res.GetString("6b8b3d2b-2cf0-4484-b55d-3e187a1114bd", "Prepaid");
			WeightBox.CollectCaption = Res.GetString("8e54f983-d15e-4a42-b92f-35988a452fd6", "Collect");
			WeightBox.PrepaidCssClass = (NoResString)"AWBRightBorder AWBBottomBorder";
			WeightBox.CollectCssClass = "AWBBottomBorder";
			WeightBox.PrepaidBoxCssClass = "AWBText10";
			WeightBox.CollectBoxCssClass = "AWBText10";

			ValuationBox.PrepaidBindTo = "EH_ValuationPPD";
			ValuationBox.CollectBindTo = "EH_ValuationCOL";
			ValuationBox.Caption = Res.GetString("b764cf5b-e72f-44bc-ae20-08d69b0d4aad", "Valuation Charge");
			ValuationBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder";
			ValuationBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";
			ValuationBox.PrepaidBoxCssClass = "AWBText10";
			ValuationBox.CollectBoxCssClass = "AWBText10";

			TaxBox.PrepaidBindTo = "EH_TaxesPPD";
			TaxBox.CollectBindTo = "EH_TaxesCOL";
			TaxBox.Caption = Res.GetString("149187ad-ad2b-445e-8851-a942dc0c0f1f", "Tax");
			TaxBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder";
			TaxBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";
			TaxBox.PrepaidBoxCssClass = "AWBText10";
			TaxBox.CollectBoxCssClass = "AWBText10";

			OtherChargesGrid.BindTo = "AWBOtherCharges";
			OtherChargesGrid.Caption = Res.GetString("05ab098f-1b3c-47f9-b7f6-6eb7519d43f1", "Other Charges");
			OtherChargesGrid.CssClass = "DetailsTable";
			OtherChargesGrid.ItemStyle.CssClass = "DetailsCell";
			OtherChargesGrid.HeaderStyle.CssClass = "DetailsHeader";

			AgentChargesBox.PrepaidBindTo = "EH_OtherChargesDueAgentPPD";
			AgentChargesBox.CollectBindTo = "EH_OtherChargesDueAgentCOL";
			AgentChargesBox.Caption = Res.GetString("c7cb5ce5-d8d2-4074-b167-4011b6f72399", "Total Other Charges Due Agent");
			AgentChargesBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder";
			AgentChargesBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";
			AgentChargesBox.PrepaidBoxCssClass = "AWBText10";
			AgentChargesBox.CollectBoxCssClass = "AWBText10";

			CarrierChargesBox.PrepaidBindTo = "EH_OtherChargesDueCarrierPPD";
			CarrierChargesBox.CollectBindTo = "EH_OtherChargesDueCarrierCOL";
			CarrierChargesBox.Caption = Res.GetString("46277d10-6f59-4d36-86fd-952e489290fd", "Total Other Charges Due Carrier");
			CarrierChargesBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder";
			CarrierChargesBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";
			CarrierChargesBox.PrepaidBoxCssClass = "AWBText10";
			CarrierChargesBox.CollectBoxCssClass = "AWBText10";

			TotalChargesBox.PrepaidBindTo = "EH_TotalPPD";
			TotalChargesBox.CollectBindTo = "EH_TotalCOL";
			TotalChargesBox.PrepaidCaption = Res.GetString("3c469d9c-37da-498a-93da-177749e8de98", "Total Prepaid");
			TotalChargesBox.CollectCaption = Res.GetString("8fc80040-fd1d-4136-b3da-f3f23c31b4ed", "Total Collect");
			TotalChargesBox.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder AWBBottomBorder";
			TotalChargesBox.CollectCssClass = (NoResString)"AWBBottomPadding AWBBottomBorder";
			TotalChargesBox.PrepaidBoxCssClass = "AWBText10";
			TotalChargesBox.CollectBoxCssClass = "AWBText10";

			EmptyCharges1Box.PrepaidFieldVisible = false;
			EmptyCharges1Box.CollectFieldVisible = false;
			EmptyCharges1Box.PrepaidCaption = Res.GetString("d11d8de5-c4fc-4668-9682-7af0532f0d8a", "Curr. Conversion Rates");
			EmptyCharges1Box.CollectCaption = Res.GetString("79b0be37-df24-4d59-8872-0587460f459f", "CC Charges in Dest. Curr.");
			EmptyCharges1Box.PrepaidCssClass = (NoResString)"AWBBottomPadding AWBRightBorder";
			EmptyCharges1Box.CollectCssClass = "AWBBottomPadding";

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;
			layoutTable.Width = Unit.Percentage(100);

			#region Weight Valuation Tax Charges

			Table charges1Table = new Table();
			charges1Table.CellPadding = 0;
			charges1Table.CellSpacing = 0;
			charges1Table.Width = Unit.Percentage(100);

			TableRow weightRow = new TableRow();
			TableCell weightCell = new TableCell();
			weightCell.Controls.Add(WeightBox);
			weightRow.Cells.Add(weightCell);

			TableRow valuationRow = new TableRow();
			TableCell valuationCell = new TableCell();
			valuationCell.Controls.Add(ValuationBox);
			valuationRow.Cells.Add(valuationCell);

			TableRow taxRow = new TableRow();
			TableCell taxCell = new TableCell();
			taxCell.Controls.Add(TaxBox);
			taxRow.Cells.Add(taxCell);

			charges1Table.Style.Add("background-color", (NoResString)"#ffffff");
			charges1Table.Rows.Add(weightRow);
			charges1Table.Rows.Add(valuationRow);
			charges1Table.Rows.Add(taxRow);

			#endregion

			TableRow charges1Row = new TableRow();
			TableCell charges1Cell = new TableCell();
			charges1Cell.VerticalAlign = VerticalAlign.Top;
			charges1Cell.CssClass = (NoResString)"AWBEmptyCell AWBRightBorder";
			charges1Cell.Controls.Add(charges1Table);
			charges1Row.Cells.Add(charges1Cell);

			TableRow empty1Row = new TableRow();
			empty1Row.Cells.Add(new TableCell()
			{
				Text = "&nbsp;",
				CssClass = (NoResString)"AWBEmptyCell AWBBottomBorder AWBRightBorder"
			});

			TableCell otherChargesCell = new TableCell();
			otherChargesCell.HorizontalAlign = HorizontalAlign.Left;
			otherChargesCell.VerticalAlign = VerticalAlign.Top;
			otherChargesCell.RowSpan = 2;
			otherChargesCell.CssClass = "AWBBottomBorder";
			otherChargesCell.Controls.Add(OtherChargesGrid);
			charges1Row.Cells.Add(otherChargesCell);

			#region Agent Carrier Charges

			Table charges2Table = new Table();
			charges2Table.CellPadding = 0;
			charges2Table.CellSpacing = 0;
			charges2Table.Width = Unit.Percentage(100);

			TableRow agentChargesRow = new TableRow();
			TableCell agentChargesCell = new TableCell();
			agentChargesCell.Controls.Add(AgentChargesBox);
			agentChargesRow.Cells.Add(agentChargesCell);

			TableRow carrierChargesRow = new TableRow();
			TableCell carrierChargesCell = new TableCell();
			carrierChargesCell.Controls.Add(CarrierChargesBox);
			carrierChargesRow.Cells.Add(carrierChargesCell);

			charges2Table.Style.Add("background-color", (NoResString)"#ffffff");
			charges2Table.Rows.Add(agentChargesRow);
			charges2Table.Rows.Add(carrierChargesRow);

			#endregion

			TableRow charges2Row = new TableRow();
			TableCell charges2Cell = new TableCell();
			charges2Cell.VerticalAlign = VerticalAlign.Top;
			charges2Cell.CssClass = (NoResString)"AWBEmptyCell AWBRightBorder";
			charges2Cell.Controls.Add(charges2Table);
			charges2Row.Cells.Add(charges2Cell);

			TableCell shipperSignatureCell = new TableCell();
			shipperSignatureCell.HorizontalAlign = HorizontalAlign.Center;
			shipperSignatureCell.VerticalAlign = VerticalAlign.Top;
			shipperSignatureCell.RowSpan = 2;
			shipperSignatureCell.CssClass = "AWBBottomBorder";
			shipperSignatureCell.Controls.Add(ShipperSignatureBox);
			charges2Row.Cells.Add(shipperSignatureCell);

			TableRow empty2Row = new TableRow();
			empty2Row.Cells.Add(new TableCell()
			{
				Text = "&nbsp;",
				CssClass = (NoResString)"AWBEmptyCell AWBBottomBorder AWBRightBorder"
			});

			#region Total Charges

			Table charges3Table = new Table();
			charges3Table.CellPadding = 0;
			charges3Table.CellSpacing = 0;
			charges3Table.Width = Unit.Percentage(100);

			TableRow totalChargesRow = new TableRow();
			TableCell totalChargesCell = new TableCell();
			totalChargesCell.Controls.Add(TotalChargesBox);
			totalChargesRow.Cells.Add(totalChargesCell);

			TableRow emptyCharges1Row = new TableRow();
			TableCell emptyCharges1Cell = new TableCell();
			emptyCharges1Cell.CssClass = "AWBEmptyCell";
			emptyCharges1Cell.Controls.Add(EmptyCharges1Box);
			emptyCharges1Row.Cells.Add(emptyCharges1Cell);

			charges3Table.Style.Add("background-color", (NoResString)"#ffffff");
			charges3Table.Rows.Add(totalChargesRow);
			charges3Table.Rows.Add(emptyCharges1Row);

			#endregion

			TableRow charges3Row = new TableRow();
			TableCell charges3Cell = new TableCell();
			charges3Cell.VerticalAlign = VerticalAlign.Top;
			charges3Cell.CssClass = (NoResString)"AWBEmptyCell AWBRightBorder";
			charges3Cell.Controls.Add(charges3Table);
			charges3Row.Cells.Add(charges3Cell);

			TableCell carrierSignatureCell = new TableCell();
			carrierSignatureCell.HorizontalAlign = HorizontalAlign.Center;
			carrierSignatureCell.VerticalAlign = VerticalAlign.Bottom;
			carrierSignatureCell.Controls.Add(CarrierSignatureBox);
			charges3Row.Cells.Add(carrierSignatureCell);

			layoutTable.Rows.Add(charges1Row);
			layoutTable.Rows.Add(empty1Row);
			layoutTable.Rows.Add(charges2Row);
			layoutTable.Rows.Add(empty2Row);
			layoutTable.Rows.Add(charges3Row);

			Controls.Clear();
			Controls.Add(layoutTable);

			OtherChargesGrid.ColumnProvider = new TrackingAWBOtherChargesColumnProvider();

			AWBOtherChargeCodeGridAddOn otherChargeCodeGridAddOn = new AWBOtherChargeCodeGridAddOn(ExportAWBOtherCharges.Schema.EO_ChargeCode, ExportAWBOtherCharges.Schema.EO_ChargeDescription, ExportAWBOtherCharges.Schema.EO_EntitlementCode, ExportAWBHeader.Schema.EH_OtherPrepaidCollect);
			Controls.Add(otherChargeCodeGridAddOn);
			otherChargeCodeGridAddOn.Grid = OtherChargesGrid;
		}

		#endregion

		#region Implementation

		void SetupGrids()
		{
			SetupOtherChargesGrid();
		}

		void SetupOtherChargesGrid()
		{
			OtherChargesGrid = new ZGrid();
			OtherChargesGrid.ID = "OtherCharges";
			OtherChargesGrid.DisableCollapsing = true;
			OtherChargesGrid.AutoGenerateColumns = false;
			OtherChargesGrid.AllowPaging = false;
			OtherChargesGrid.AllowAdd = true;
			OtherChargesGrid.AllowDelete = true;
			OtherChargesGrid.AllowEdit = true;
			OtherChargesGrid.InitialRowsToDisplay = 4;
			OtherChargesGrid.AutoSizeColumns = true;
		}

		AWBPrepaidCollectControl WeightBox;
		AWBPrepaidCollectControl ValuationBox;
		AWBPrepaidCollectControl TaxBox;
		ZGrid OtherChargesGrid;

		AWBPrepaidCollectControl AgentChargesBox;
		AWBPrepaidCollectControl CarrierChargesBox;
		AWBShipperSignatureControl ShipperSignatureBox;

		AWBPrepaidCollectControl TotalChargesBox;
		AWBPrepaidCollectControl EmptyCharges1Box;
		AWBCarrierSignatureControl CarrierSignatureBox;

		#endregion
	}
}
