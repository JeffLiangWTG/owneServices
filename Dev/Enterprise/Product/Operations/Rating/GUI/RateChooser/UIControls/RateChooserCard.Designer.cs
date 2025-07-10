using System;
using System.Windows.Forms;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class RateChooserCard
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RateChooserCard));
            this.CardPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlCommodityInfo = new System.Windows.Forms.TableLayoutPanel();
            this.txtCommodityExcluded = new Enterprise.ZArchitecture.ZTextBox();
            this.lblExcludedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblIncludedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblCommodityType = new Enterprise.ZArchitecture.ZLabel();
            this.lblCommodityName = new Enterprise.ZArchitecture.ZLabel();
            this.txtCommodityIncluded = new Enterprise.ZArchitecture.ZTextBox();
            this.pnlTop = new System.Windows.Forms.TableLayoutPanel();
            this.pnl6thColumn = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnl6thColAutoScrollRegion = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lcsOutlandCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargeSummaryControl();
            this.lcsOceanCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargeSummaryControl();
            this.lcsInlandCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargeSummaryControl();
            this.chargesSummaryBOLCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesSummaryControl();
            this.chargesSummaryCW1FreightCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesSummaryControl();
            this.chargesSummaryCW1BillOfLadingCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesSummaryControl();
            this.lblLclUnit = new Enterprise.ZArchitecture.ZLabel();
            this.pnlTotal = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblTotalCalculatedCharges = new Enterprise.ZArchitecture.ZLabel();
            this.pnlTotalIcon = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pbTotalIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblTotalLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnl5thColumn = new System.Windows.Forms.TableLayoutPanel();
            this.lblAddOnOrCarrier = new Enterprise.ZArchitecture.ZLabel();
            this.lblVesselOrConsigneeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblRateType2OrConsignor = new Enterprise.ZArchitecture.ZLabel();
            this.lblRateType2OrConsignorLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblAddonOrCarrierLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblVesselOrConsignee = new Enterprise.ZArchitecture.ZLabel();
            this.pnl4thColumn = new System.Windows.Forms.TableLayoutPanel();
            this.lblRateType = new Enterprise.ZArchitecture.ZLabel();
            this.lblRateTypeOrBlankLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblServiceStringOrLevel = new Enterprise.ZArchitecture.ZLabel();
            this.lblServiceStringOrLevelLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pbCarrierServiceLevelWarning = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblCarrierServiceLevel = new Enterprise.ZArchitecture.ZLabel();
            this.lblCarrierServiceLevelLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnl3rdColumn = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlRoute = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlRouteInfo = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlTransitInfo = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblSpotVoyageNumber = new Enterprise.ZArchitecture.ZLabel();
            this.lblSpotVesselName = new Enterprise.ZArchitecture.ZLabel();
            this.pbShip = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.pnlVoyageInfo = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblSpotTransitTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblSpotRateID = new Enterprise.ZArchitecture.ZLabel();
            this.pnlDestination = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblDestination = new Enterprise.ZArchitecture.ZLabel();
            this.lblArrivalTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblArrivalDate = new Enterprise.ZArchitecture.ZLabel();
            this.pnlOrigin = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblOrigin = new Enterprise.ZArchitecture.ZLabel();
            this.lblDepartureTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblDepartureDate = new Enterprise.ZArchitecture.ZLabel();
            this.pnlNonSpotInfo = new System.Windows.Forms.TableLayoutPanel();
            this.lblExpiryDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblExpiryDateLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblEffectiveDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblEffectiveDateLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblRoutingOrTransitTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblTransitTimeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnl2ndColumn = new System.Windows.Forms.TableLayoutPanel();
            this.pbCommodityInfo = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblCommodities = new Enterprise.ZArchitecture.ZLabel();
            this.lblAccount = new Enterprise.ZArchitecture.ZLabel();
            this.lblTradeLane = new Enterprise.ZArchitecture.ZLabel();
            this.pnl1stColumn = new System.Windows.Forms.TableLayoutPanel();
            this.lblContractNumberLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.lblServiceProvider = new Enterprise.ZArchitecture.ZLabel();
            this.pbSpotRate = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.pbProviderIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblContractNumber = new Enterprise.ZArchitecture.ZLabel();
            this.pnl7thColumn = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblWarnings = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.pnlRateDetails = new Enterprise.ZArchitecture.GUI.ZCollapsiblePanel();
            this.scrollbarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.verticalLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.pnlChargeDetails = new System.Windows.Forms.TableLayoutPanel();
            this.chargesCW1BillOfLadingCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesControl();
            this.chargesCW1FreightCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesControl();
            this.chargesBOLCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.ChargesControl();
            this.lcInlandCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargesControl();
            this.lcOceanCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargesControl();
            this.lcOutlandCharges = new Enterprise.Rating.GUI.RateChooser.UIControls.LegChargesControl();
            this.pnlThirdRow = new System.Windows.Forms.TableLayoutPanel();
            this.ucRoutes = new Enterprise.Rating.GUI.RateSelection.SpotUIControls.RoutesControl();
            this.ucPenalties = new Enterprise.Rating.GUI.RateSelection.SpotUIControls.PenaltiesControl();
            this.ucBookingTerms = new Enterprise.Rating.GUI.RateSelection.BookingTermsControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CardPanel.SuspendLayout();
            this.pnlCommodityInfo.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnl6thColumn.SuspendLayout();
            this.pnl6thColAutoScrollRegion.SuspendLayout();
            this.lcsOutlandCharges.SuspendLayout();
            this.lcsOceanCharges.SuspendLayout();
            this.lcsInlandCharges.SuspendLayout();
            this.chargesSummaryBOLCharges.SuspendLayout();
            this.chargesSummaryCW1FreightCharges.SuspendLayout();
            this.chargesSummaryCW1BillOfLadingCharges.SuspendLayout();
            this.pnlTotal.SuspendLayout();
            this.pnlTotalIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTotalIcon)).BeginInit();
            this.pnl5thColumn.SuspendLayout();
            this.pnl4thColumn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCarrierServiceLevelWarning)).BeginInit();
            this.pnl3rdColumn.SuspendLayout();
            this.pnlRoute.SuspendLayout();
            this.pnlRouteInfo.SuspendLayout();
            this.pnlTransitInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbShip)).BeginInit();
            this.pnlVoyageInfo.SuspendLayout();
            this.pnlDestination.SuspendLayout();
            this.pnlOrigin.SuspendLayout();
            this.pnlNonSpotInfo.SuspendLayout();
            this.pnl2ndColumn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCommodityInfo)).BeginInit();
            this.pnl1stColumn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSpotRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProviderIcon)).BeginInit();
            this.pnl7thColumn.SuspendLayout();
            this.pnlRateDetails.SuspendLayout();
            this.scrollbarPanel.SuspendLayout();
            this.verticalLayoutPanel.SuspendLayout();
            this.pnlChargeDetails.SuspendLayout();
            this.chargesCW1BillOfLadingCharges.SuspendLayout();
            this.chargesCW1FreightCharges.SuspendLayout();
            this.chargesBOLCharges.SuspendLayout();
            this.lcInlandCharges.SuspendLayout();
            this.lcOceanCharges.SuspendLayout();
            this.lcOutlandCharges.SuspendLayout();
            this.pnlThirdRow.SuspendLayout();
            this.ucRoutes.SuspendLayout();
            this.ucPenalties.SuspendLayout();
            this.ucBookingTerms.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.ChooserRateRow);
            // 
            // CardPanel
            // 
            this.CardPanel.AutoSize = true;
            this.CardPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CardPanel.Controls.Add(this.pnlCommodityInfo);
            this.CardPanel.Controls.Add(this.pnlTop);
            this.CardPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.CardPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CardPanel.Name = "CardPanel";
            this.CardPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2076, 134, true);
            this.CardPanel.TabIndex = 2;
            // 
            // pnlCommodityInfo
            // 
            this.pnlCommodityInfo.BackColor = System.Drawing.SystemColors.Info;
            this.pnlCommodityInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCommodityInfo.ColumnCount = 2;
            this.pnlCommodityInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.pnlCommodityInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.pnlCommodityInfo.Controls.Add(this.txtCommodityExcluded, 1, 3);
            this.pnlCommodityInfo.Controls.Add(this.lblExcludedLabel, 0, 3);
            this.pnlCommodityInfo.Controls.Add(this.lblIncludedLabel, 0, 2);
            this.pnlCommodityInfo.Controls.Add(this.lblCommodityType, 0, 1);
            this.pnlCommodityInfo.Controls.Add(this.lblCommodityName, 0, 0);
            this.pnlCommodityInfo.Controls.Add(this.txtCommodityIncluded, 1, 2);
			this.pnlCommodityInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 5, true);
            this.pnlCommodityInfo.Name = "pnlCommodityInfo";
            this.pnlCommodityInfo.RowCount = 4;
            this.pnlCommodityInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlCommodityInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlCommodityInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlCommodityInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlCommodityInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 120, true);
            this.pnlCommodityInfo.TabIndex = 0;
            this.pnlCommodityInfo.Visible = false;
            // 
            // txtCommodityExcluded
            // 
            this.txtCommodityExcluded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommodityExcluded.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.txtCommodityExcluded, "CommodityExcluded");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityExcluded)));
            this.txtCommodityExcluded.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 79, true);
            this.txtCommodityExcluded.Multiline = true;
            this.txtCommodityExcluded.Name = "txtCommodityExcluded";
            this.txtCommodityExcluded.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCommodityExcluded.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 32, true);
            this.txtCommodityExcluded.TabIndex = 16;
            // 
            // lblExcludedLabel
            // 
            this.BindingSource.SetBindingMember(this.lblExcludedLabel, "ExcludedLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ExcludedLabel)));
            this.lblExcludedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExcludedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 76, true);
            this.lblExcludedLabel.Name = "lblExcludedLabel";
            this.lblExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 42, true);
            this.lblExcludedLabel.TabIndex = 15;
            this.lblExcludedLabel.Text = "EL";
            // 
            // lblIncludedLabel
            // 
            this.BindingSource.SetBindingMember(this.lblIncludedLabel, "IncludedLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).IncludedLabel)));
            this.lblIncludedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIncludedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblIncludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 38, true);
            this.lblIncludedLabel.Name = "lblIncludedLabel";
            this.lblIncludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 38, true);
            this.lblIncludedLabel.TabIndex = 13;
            this.lblIncludedLabel.Text = "IL";
            // 
            // lblCommodityType
            // 
            this.BindingSource.SetBindingMember(this.lblCommodityType, "CommodityType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityType)));
            this.pnlCommodityInfo.SetColumnSpan(this.lblCommodityType, 2);
            this.lblCommodityType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommodityType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCommodityType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.lblCommodityType.Name = "lblCommodityType";
            this.lblCommodityType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 19, true);
            this.lblCommodityType.TabIndex = 12;
            this.lblCommodityType.Text = "CommodityType";
            // 
            // lblCommodityName
            // 
            this.BindingSource.SetBindingMember(this.lblCommodityName, "CommodityName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityName)));
            this.pnlCommodityInfo.SetColumnSpan(this.lblCommodityName, 2);
            this.lblCommodityName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommodityName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCommodityName.IsFontBold = true;
            this.lblCommodityName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblCommodityName.Name = "lblCommodityName";
            this.lblCommodityName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 19, true);
            this.lblCommodityName.TabIndex = 11;
            this.lblCommodityName.Text = "CommodityName";
            // 
            // txtCommodityIncluded
            // 
            this.txtCommodityIncluded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommodityIncluded.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.txtCommodityIncluded, "CommodityIncluded");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityIncluded)));
            this.txtCommodityIncluded.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 41, true);
            this.txtCommodityIncluded.Multiline = true;
            this.txtCommodityIncluded.Name = "txtCommodityIncluded";
            this.txtCommodityIncluded.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCommodityIncluded.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 32, true);
            this.txtCommodityIncluded.TabIndex = 14;
            // 
            // pnlTop
            // 
            this.pnlTop.AutoSize = true;
            this.pnlTop.ColumnCount = 7;
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.pnlTop.Controls.Add(this.pnl6thColumn, 5, 0);
            this.pnlTop.Controls.Add(this.pnl5thColumn, 4, 0);
            this.pnlTop.Controls.Add(this.pnl4thColumn, 3, 0);
            this.pnlTop.Controls.Add(this.pnl3rdColumn, 2, 0);
            this.pnlTop.Controls.Add(this.pnl2ndColumn, 1, 0);
            this.pnlTop.Controls.Add(this.pnl1stColumn, 0, 0);
            this.pnlTop.Controls.Add(this.pnl7thColumn, 6, 0);
            this.pnlTop.Controls.Add(this.pnlRateDetails, 0, 1);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.RowCount = 2;
            this.pnlTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2076, 134, true);
            this.pnlTop.TabIndex = 10;
            // 
            // pnl6thColumn
            // 
            this.pnl6thColumn.Controls.Add(this.pnl6thColAutoScrollRegion);
            this.pnl6thColumn.Controls.Add(this.pnlTotal);
            this.pnl6thColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl6thColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1558, 3, true);
            this.pnl6thColumn.Name = "pnl6thColumn";
            this.pnlTop.SetRowSpan(this.pnl6thColumn, 2);
            this.pnl6thColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 128, true);
            this.pnl6thColumn.TabIndex = 15;
            // 
            // pnl6thColAutoScrollRegion
            // 
            this.pnl6thColAutoScrollRegion.AutoScroll = true;
            this.pnl6thColAutoScrollRegion.Controls.Add(this.lcsOutlandCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.lcsOceanCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.lcsInlandCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.chargesSummaryBOLCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.chargesSummaryCW1FreightCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.chargesSummaryCW1BillOfLadingCharges);
            this.pnl6thColAutoScrollRegion.Controls.Add(this.lblLclUnit);
            this.pnl6thColAutoScrollRegion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl6thColAutoScrollRegion.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnl6thColAutoScrollRegion.Name = "pnl6thColAutoScrollRegion";
            this.pnl6thColAutoScrollRegion.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 110, true);
            this.pnl6thColAutoScrollRegion.TabIndex = 39;
            // 
            // lcsOutlandCharges
            // 
            this.lcsOutlandCharges.AllowDrop = true;
            this.lcsOutlandCharges.AutoSize = true;
            this.lcsOutlandCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.lcsOutlandCharges, "OutlandCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).OutlandCharges)));
            this.lcsOutlandCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.lcsOutlandCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 358, true);
            this.lcsOutlandCharges.Name = "lcsOutlandCharges";
            this.lcsOutlandCharges.SelectedItem = null;
            this.lcsOutlandCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
            this.lcsOutlandCharges.TabIndex = 42;
            // 
            // lcsOceanCharges
            // 
            this.lcsOceanCharges.AllowDrop = true;
            this.lcsOceanCharges.AutoSize = true;
            this.lcsOceanCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.lcsOceanCharges, "OceanCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).OceanCharges)));
            this.lcsOceanCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.lcsOceanCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
            this.lcsOceanCharges.Name = "lcsOceanCharges";
            this.lcsOceanCharges.SelectedItem = null;
            this.lcsOceanCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
            this.lcsOceanCharges.TabIndex = 41;
            // 
            // lcsInlandCharges
            // 
            this.lcsInlandCharges.AllowDrop = true;
            this.lcsInlandCharges.AutoSize = true;
            this.lcsInlandCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.lcsInlandCharges, "InlandCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).InlandCharges)));
            this.lcsInlandCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.lcsInlandCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
            this.lcsInlandCharges.Name = "lcsInlandCharges";
            this.lcsInlandCharges.SelectedItem = null;
            this.lcsInlandCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
            this.lcsInlandCharges.TabIndex = 40;
            // 
            // chargesSummaryBOLCharges
            // 
            this.chargesSummaryBOLCharges.AllowDrop = true;
            this.chargesSummaryBOLCharges.AutoSize = true;
            this.chargesSummaryBOLCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.chargesSummaryBOLCharges, "BOLCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BOLCharges)));
            this.chargesSummaryBOLCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.chargesSummaryBOLCharges.IsSelected = false;
            this.chargesSummaryBOLCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
            this.chargesSummaryBOLCharges.Name = "chargesSummaryBOLCharges";
            this.chargesSummaryBOLCharges.SelectionChanged = null;
            this.chargesSummaryBOLCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 100, true);
            this.chargesSummaryBOLCharges.TabIndex = 39;
            // 
            // chargesSummaryCW1FreightCharges
            // 
            this.chargesSummaryCW1FreightCharges.AllowDrop = true;
            this.chargesSummaryCW1FreightCharges.AutoSize = true;
            this.chargesSummaryCW1FreightCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.chargesSummaryCW1FreightCharges, "CW1FreightCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CW1FreightCharges)));
            this.chargesSummaryCW1FreightCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.chargesSummaryCW1FreightCharges.IsSelected = false;
            this.chargesSummaryCW1FreightCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 118, true);
            this.chargesSummaryCW1FreightCharges.Name = "chargesSummaryCW1FreightCharges";
            this.chargesSummaryCW1FreightCharges.SelectionChanged = null;
            this.chargesSummaryCW1FreightCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 100, true);
            this.chargesSummaryCW1FreightCharges.TabIndex = 38;
            // 
            // chargesSummaryCW1BillOfLadingCharges
            // 
            this.chargesSummaryCW1BillOfLadingCharges.AllowDrop = true;
            this.chargesSummaryCW1BillOfLadingCharges.AutoSize = true;
            this.chargesSummaryCW1BillOfLadingCharges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.chargesSummaryCW1BillOfLadingCharges, "CW1BillOfLadingCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CW1BillOfLadingCharges)));
            this.chargesSummaryCW1BillOfLadingCharges.Dock = System.Windows.Forms.DockStyle.Top;
            this.chargesSummaryCW1BillOfLadingCharges.IsSelected = false;
            this.chargesSummaryCW1BillOfLadingCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 18, true);
            this.chargesSummaryCW1BillOfLadingCharges.Name = "chargesSummaryCW1BillOfLadingCharges";
            this.chargesSummaryCW1BillOfLadingCharges.SelectionChanged = null;
            this.chargesSummaryCW1BillOfLadingCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 100, true);
            this.chargesSummaryCW1BillOfLadingCharges.TabIndex = 37;
            // 
            // lblLclUnit
            // 
            this.BindingSource.SetBindingMember(this.lblLclUnit, "LclUnit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).LclUnit)));
            this.lblLclUnit.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLclUnit.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblLclUnit.IsFontBold = true;
            this.lblLclUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblLclUnit.Name = "lblLclUnit";
            this.lblLclUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 18, true);
            this.lblLclUnit.TabIndex = 36;
            this.lblLclUnit.Text = "LclUnit";
            this.lblLclUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTotal
            // 
            this.pnlTotal.BackColor = System.Drawing.Color.Silver;
            this.pnlTotal.Controls.Add(this.lblTotalCalculatedCharges);
            this.pnlTotal.Controls.Add(this.pnlTotalIcon);
            this.pnlTotal.Controls.Add(this.lblTotalLabel);
            this.pnlTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
            this.pnlTotal.Name = "pnlTotal";
            this.pnlTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 20, true);
            this.pnlTotal.TabIndex = 36;
            // 
            // lblTotalCalculatedCharges
            // 
            this.BindingSource.SetBindingMember(this.lblTotalCalculatedCharges, "TotalCalculatedCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).TotalCalculatedCharges)));
            this.lblTotalCalculatedCharges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalCalculatedCharges.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotalCalculatedCharges.IsFontBold = true;
            this.lblTotalCalculatedCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 0, true);
            this.lblTotalCalculatedCharges.Name = "lblTotalCalculatedCharges";
            this.lblTotalCalculatedCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
            this.lblTotalCalculatedCharges.TabIndex = 17;
            this.lblTotalCalculatedCharges.Text = "TotalCalculatedCharges";
            this.lblTotalCalculatedCharges.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotalCalculatedCharges.MouseHover += new System.EventHandler(this.lblTotalCalculatedCharges_MouseHover);
            // 
            // pnlTotalIcon
            // 
            this.pnlTotalIcon.Controls.Add(this.pbTotalIcon);
            this.pnlTotalIcon.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlTotalIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 0, true);
            this.pnlTotalIcon.Name = "pnlTotalIcon";
            this.pnlTotalIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.pnlTotalIcon.TabIndex = 16;
            // 
            // pbTotalIcon
            // 
            this.pbTotalIcon.Name = "pbTotalIcon";
            this.pbTotalIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.pbTotalIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbTotalIcon.TabIndex = 15;
            this.pbTotalIcon.TabStop = false;
			this.pbTotalIcon.MouseHover += new System.EventHandler(this.pbTotalIcon_MouseHover);
			// 
			// lblTotalLabel
			// 
			this.BindingSource.SetBindingMember(this.lblTotalLabel, "TotalLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).TotalLabel)));
            this.lblTotalLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotalLabel.IsFontBold = true;
            this.lblTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblTotalLabel.Name = "lblTotalLabel";
            this.lblTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 18, true);
            this.lblTotalLabel.TabIndex = 2;
            this.lblTotalLabel.Text = "Total";
            // 
            // pnl5thColumn
            // 
            this.pnl5thColumn.ColumnCount = 2;
            this.pnl5thColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl5thColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl5thColumn.Controls.Add(this.lblAddOnOrCarrier, 1, 2);
            this.pnl5thColumn.Controls.Add(this.lblVesselOrConsigneeLabel, 0, 1);
            this.pnl5thColumn.Controls.Add(this.lblRateType2OrConsignor, 1, 0);
            this.pnl5thColumn.Controls.Add(this.lblRateType2OrConsignorLabel, 0, 0);
            this.pnl5thColumn.Controls.Add(this.lblAddonOrCarrierLabel, 0, 2);
            this.pnl5thColumn.Controls.Add(this.lblVesselOrConsignee, 1, 1);
            this.pnl5thColumn.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl5thColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1247, 3, true);
            this.pnl5thColumn.Name = "pnl5thColumn";
            this.pnl5thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnl5thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnl5thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl5thColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 69, true);
            this.pnl5thColumn.TabIndex = 5;
            // 
            // lblAddOnOrCarrier
            // 
            this.BindingSource.SetBindingMember(this.lblAddOnOrCarrier, "AddOnOrCarrier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).AddOnOrCarrier)));
            this.lblAddOnOrCarrier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddOnOrCarrier.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblAddOnOrCarrier.IsFontBold = true;
            this.lblAddOnOrCarrier.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 46, true);
            this.lblAddOnOrCarrier.Name = "lblAddOnOrCarrier";
            this.lblAddOnOrCarrier.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
            this.lblAddOnOrCarrier.TabIndex = 16;
            this.lblAddOnOrCarrier.Text = "AddOnOrCarrier";
            // 
            // lblVesselOrConsigneeLabel
            // 
            this.BindingSource.SetBindingMember(this.lblVesselOrConsigneeLabel, "VesselOrConsigneeLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).VesselOrConsigneeLabel)));
            this.lblVesselOrConsigneeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVesselOrConsigneeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblVesselOrConsigneeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
            this.lblVesselOrConsigneeLabel.Name = "lblVesselOrConsigneeLabel";
            this.lblVesselOrConsigneeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
            this.lblVesselOrConsigneeLabel.TabIndex = 13;
            this.lblVesselOrConsigneeLabel.Text = "VoCLabel";
            this.lblVesselOrConsigneeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRateType2OrConsignor
            // 
            this.BindingSource.SetBindingMember(this.lblRateType2OrConsignor, "RateType2OrConsignor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).RateType2OrConsignor)));
            this.lblRateType2OrConsignor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRateType2OrConsignor.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblRateType2OrConsignor.IsFontBold = true;
            this.lblRateType2OrConsignor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 0, true);
            this.lblRateType2OrConsignor.Name = "lblRateType2OrConsignor";
            this.lblRateType2OrConsignor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
            this.lblRateType2OrConsignor.TabIndex = 12;
            this.lblRateType2OrConsignor.Text = "RateType2OrConsignor";
            // 
            // lblRateType2OrConsignorLabel
            // 
            this.BindingSource.SetBindingMember(this.lblRateType2OrConsignorLabel, "RateType2OrConsignorLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).RateType2OrConsignorLabel)));
            this.lblRateType2OrConsignorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRateType2OrConsignorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRateType2OrConsignorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblRateType2OrConsignorLabel.Name = "lblRateType2OrConsignorLabel";
            this.lblRateType2OrConsignorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
            this.lblRateType2OrConsignorLabel.TabIndex = 11;
            this.lblRateType2OrConsignorLabel.Text = "RoCLabel";
            this.lblRateType2OrConsignorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAddonOrCarrierLabel
            // 
            this.BindingSource.SetBindingMember(this.lblAddonOrCarrierLabel, "AddonOrCarrierLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).AddonOrCarrierLabel)));
            this.lblAddonOrCarrierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddonOrCarrierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblAddonOrCarrierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
            this.lblAddonOrCarrierLabel.Name = "lblAddonOrCarrierLabel";
            this.lblAddonOrCarrierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
            this.lblAddonOrCarrierLabel.TabIndex = 15;
            this.lblAddonOrCarrierLabel.Text = "AoCLabel";
            this.lblAddonOrCarrierLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblVesselOrConsignee
            // 
            this.BindingSource.SetBindingMember(this.lblVesselOrConsignee, "VesselOrConsignee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).VesselOrConsignee)));
            this.lblVesselOrConsignee.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVesselOrConsignee.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblVesselOrConsignee.IsFontBold = true;
            this.lblVesselOrConsignee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 23, true);
            this.lblVesselOrConsignee.Name = "lblVesselOrConsignee";
            this.lblVesselOrConsignee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
            this.lblVesselOrConsignee.TabIndex = 14;
            this.lblVesselOrConsignee.Text = "VesselOrConsignee";
            // 
            // pnl4thColumn
            // 
            this.pnl4thColumn.ColumnCount = 3;
            this.pnl4thColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.pnl4thColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.pnl4thColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.pnl4thColumn.Controls.Add(this.lblRateType, 1, 2);
            this.pnl4thColumn.Controls.Add(this.lblRateTypeOrBlankLabel, 0, 2);
            this.pnl4thColumn.Controls.Add(this.lblServiceStringOrLevel, 1, 1);
            this.pnl4thColumn.Controls.Add(this.lblServiceStringOrLevelLabel, 0, 1);
            this.pnl4thColumn.Controls.Add(this.pbCarrierServiceLevelWarning, 2, 0);
            this.pnl4thColumn.Controls.Add(this.lblCarrierServiceLevel, 1, 0);
            this.pnl4thColumn.Controls.Add(this.lblCarrierServiceLevelLabel, 0, 0);
            this.pnl4thColumn.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl4thColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(936, 3, true);
            this.pnl4thColumn.Name = "pnl4thColumn";
            this.pnl4thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnl4thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.pnl4thColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl4thColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 69, true);
            this.pnl4thColumn.TabIndex = 4;
            // 
            // lblRateType
            // 
            this.BindingSource.SetBindingMember(this.lblRateType, "RateType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).RateType)));
            this.lblRateType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRateType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblRateType.IsFontBold = true;
            this.lblRateType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 46, true);
            this.lblRateType.Name = "lblRateType";
            this.lblRateType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblRateType.TabIndex = 16;
            this.lblRateType.Text = "RateType";
            // 
            // lblRateTypeOrBlankLabel
            // 
            this.BindingSource.SetBindingMember(this.lblRateTypeOrBlankLabel, "RateTypeOrBlankLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).RateTypeOrBlankLabel)));
            this.lblRateTypeOrBlankLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRateTypeOrBlankLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRateTypeOrBlankLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
            this.lblRateTypeOrBlankLabel.Name = "lblRateTypeOrBlankLabel";
            this.lblRateTypeOrBlankLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblRateTypeOrBlankLabel.TabIndex = 15;
            this.lblRateTypeOrBlankLabel.Text = "RateTypeOrBlankLabel";
            this.lblRateTypeOrBlankLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblServiceStringOrLevel
            // 
            this.BindingSource.SetBindingMember(this.lblServiceStringOrLevel, "ServiceStringOrLevel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ServiceStringOrLevel)));
            this.lblServiceStringOrLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblServiceStringOrLevel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblServiceStringOrLevel.IsFontBold = true;
            this.lblServiceStringOrLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 23, true);
            this.lblServiceStringOrLevel.Name = "lblServiceStringOrLevel";
            this.lblServiceStringOrLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblServiceStringOrLevel.TabIndex = 14;
            this.lblServiceStringOrLevel.Text = "ServiceStringOrLevel";
            // 
            // lblServiceStringOrLevelLabel
            // 
            this.BindingSource.SetBindingMember(this.lblServiceStringOrLevelLabel, "ServiceStringOrLevelLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ServiceStringOrLevelLabel)));
            this.lblServiceStringOrLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblServiceStringOrLevelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblServiceStringOrLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
            this.lblServiceStringOrLevelLabel.Name = "lblServiceStringOrLevelLabel";
            this.lblServiceStringOrLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblServiceStringOrLevelLabel.TabIndex = 13;
            this.lblServiceStringOrLevelLabel.Text = "ServiceStringOrLevelLabel";
            this.lblServiceStringOrLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbCarrierServiceLevelWarning
            // 
            this.pbCarrierServiceLevelWarning.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
            this.pbCarrierServiceLevelWarning.Name = "pbCarrierServiceLevelWarning";
            this.pbCarrierServiceLevelWarning.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 17, true);
            this.pbCarrierServiceLevelWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCarrierServiceLevelWarning.TabIndex = 12;
            this.pbCarrierServiceLevelWarning.TabStop = false;
            this.pbCarrierServiceLevelWarning.MouseHover += new System.EventHandler(this.imgServiceLevelWarning_MouseHover);
            // 
            // lblCarrierServiceLevel
            // 
            this.BindingSource.SetBindingMember(this.lblCarrierServiceLevel, "CarrierServiceLevel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CarrierServiceLevel)));
            this.lblCarrierServiceLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCarrierServiceLevel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCarrierServiceLevel.IsFontBold = true;
            this.lblCarrierServiceLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
            this.lblCarrierServiceLevel.Name = "lblCarrierServiceLevel";
            this.lblCarrierServiceLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblCarrierServiceLevel.TabIndex = 11;
            this.lblCarrierServiceLevel.Text = "CarrierServiceLevel";
            // 
            // lblCarrierServiceLevelLabel
            // 
            this.BindingSource.SetBindingMember(this.lblCarrierServiceLevelLabel, "CarrierServiceLevelLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CarrierServiceLevelLabel)));
            this.lblCarrierServiceLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCarrierServiceLevelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCarrierServiceLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblCarrierServiceLevelLabel.Name = "lblCarrierServiceLevelLabel";
            this.lblCarrierServiceLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 23, true);
            this.lblCarrierServiceLevelLabel.TabIndex = 10;
            this.lblCarrierServiceLevelLabel.Text = "CSLLabel";
            this.lblCarrierServiceLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnl3rdColumn
            // 
            this.pnl3rdColumn.Controls.Add(this.pnlRoute);
            this.pnl3rdColumn.Controls.Add(this.pnlNonSpotInfo);
            this.pnl3rdColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl3rdColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 3, true);
            this.pnl3rdColumn.Name = "pnl3rdColumn";
            this.pnl3rdColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 102, true);
            this.pnl3rdColumn.TabIndex = 3;
            // 
            // pnlRoute
            // 
            this.pnlRoute.Controls.Add(this.pnlRouteInfo);
            this.pnlRoute.Controls.Add(this.pnlDestination);
            this.pnlRoute.Controls.Add(this.pnlOrigin);
            this.pnlRoute.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRoute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
            this.pnlRoute.Name = "pnlRoute";
            this.pnlRoute.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 60, true);
            this.pnlRoute.TabIndex = 18;
            // 
            // pnlRouteInfo
            // 
            this.pnlRouteInfo.Controls.Add(this.zPanel1);
            this.pnlRouteInfo.Controls.Add(this.pnlTransitInfo);
            this.pnlRouteInfo.Controls.Add(this.pnlVoyageInfo);
            this.pnlRouteInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRouteInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 0, true);
            this.pnlRouteInfo.Name = "pnlRouteInfo";
            this.pnlRouteInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 60, true);
            this.pnlRouteInfo.TabIndex = 2;
            // 
            // zPanel1
            // 
            this.zPanel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 5, true);
            this.zPanel1.TabIndex = 3;
            // 
            // pnlTransitInfo
            // 
            this.pnlTransitInfo.Controls.Add(this.lblSpotVoyageNumber);
            this.pnlTransitInfo.Controls.Add(this.lblSpotVesselName);
            this.pnlTransitInfo.Controls.Add(this.pbShip);
            this.pnlTransitInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTransitInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTransitInfo.Name = "pnlTransitInfo";
            this.pnlTransitInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 25, true);
            this.pnlTransitInfo.TabIndex = 2;
            // 
            // lblSpotVoyageNumber
            // 
            this.BindingSource.SetBindingMember(this.lblSpotVoyageNumber, "BookingInfo.VoyageNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo.VoyageNumber)));
            this.lblSpotVoyageNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpotVoyageNumber.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblSpotVoyageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 0, true);
            this.lblSpotVoyageNumber.Name = "lblSpotVoyageNumber";
            this.lblSpotVoyageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 25, true);
            this.lblSpotVoyageNumber.TabIndex = 9;
            this.lblSpotVoyageNumber.Text = "VYN";
            this.lblSpotVoyageNumber.MouseHover += new System.EventHandler(this.lblSpotVoyageNumber_MouseHover);
            // 
            // lblSpotVesselName
            // 
            this.BindingSource.SetBindingMember(this.lblSpotVesselName, "BookingInfo.VesselName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo.VesselName)));
            this.lblSpotVesselName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSpotVesselName.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblSpotVesselName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 0, true);
            this.lblSpotVesselName.Name = "lblSpotVesselName";
            this.lblSpotVesselName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 25, true);
            this.lblSpotVesselName.TabIndex = 8;
            this.lblSpotVesselName.Text = "VN";
            this.lblSpotVesselName.MouseHover += new System.EventHandler(this.lblSpotVesselName_MouseHover);
            // 
            // pbShip
            // 
            this.pbShip.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbShip.Image = ((System.Drawing.Image)(resources.GetObject("pbShip.Image")));
            this.pbShip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pbShip.Name = "pbShip";
            this.pbShip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 25, true);
            this.pbShip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbShip.TabIndex = 6;
            this.pbShip.TabStop = false;
            this.pbShip.MouseHover += new System.EventHandler(this.pbShip_MouseHover);
            // 
            // pnlVoyageInfo
            // 
            this.pnlVoyageInfo.Controls.Add(this.lblSpotTransitTime);
            this.pnlVoyageInfo.Controls.Add(this.lblSpotRateID);
            this.pnlVoyageInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlVoyageInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
            this.pnlVoyageInfo.Name = "pnlVoyageInfo";
            this.pnlVoyageInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 25, true);
            this.pnlVoyageInfo.TabIndex = 0;
            // 
            // lblSpotTransitTime
            // 
            this.lblSpotTransitTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpotTransitTime.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblSpotTransitTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 0, true);
            this.lblSpotTransitTime.Name = "lblSpotTransitTime";
            this.lblSpotTransitTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 25, true);
            this.lblSpotTransitTime.TabIndex = 4;
            this.lblSpotTransitTime.Text = "TT";
            this.lblSpotTransitTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSpotTransitTime.MouseHover += new System.EventHandler(this.lblSpotTransitTime_MouseHover);
            // 
            // lblSpotRateID
            // 
            this.BindingSource.SetBindingMember(this.lblSpotRateID, "BookingInfo.RateId");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo.RateId)));
            this.lblSpotRateID.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSpotRateID.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblSpotRateID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblSpotRateID.Name = "lblSpotRateID";
            this.lblSpotRateID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 25, true);
            this.lblSpotRateID.TabIndex = 3;
            this.lblSpotRateID.Text = "RateID";
            this.lblSpotRateID.MouseHover += new System.EventHandler(this.lblSpotRateID_MouseHover);
            // 
            // pnlDestination
            // 
            this.pnlDestination.Controls.Add(this.lblDestination);
            this.pnlDestination.Controls.Add(this.lblArrivalTime);
            this.pnlDestination.Controls.Add(this.lblArrivalDate);
            this.pnlDestination.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlDestination.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 0, true);
            this.pnlDestination.Name = "pnlDestination";
            this.pnlDestination.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 60, true);
            this.pnlDestination.TabIndex = 1;
            // 
            // lblDestination
            // 
            this.BindingSource.SetBindingMember(this.lblDestination, "BookingInfo.Destination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo.Destination)));
            this.lblDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDestination.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblDestination.IsFontBold = true;
            this.lblDestination.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
            this.lblDestination.TabIndex = 5;
            this.lblDestination.Text = "D";
            this.lblDestination.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblArrivalTime
            // 
            this.lblArrivalTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblArrivalTime.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblArrivalTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.lblArrivalTime.Name = "lblArrivalTime";
            this.lblArrivalTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
            this.lblArrivalTime.TabIndex = 4;
            this.lblArrivalTime.Text = "AT";
            this.lblArrivalTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblArrivalDate
            // 
            this.lblArrivalDate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblArrivalDate.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblArrivalDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
            this.lblArrivalDate.Name = "lblArrivalDate";
            this.lblArrivalDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
            this.lblArrivalDate.TabIndex = 3;
            this.lblArrivalDate.Text = "AD";
            this.lblArrivalDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlOrigin
            // 
            this.pnlOrigin.Controls.Add(this.lblOrigin);
            this.pnlOrigin.Controls.Add(this.lblDepartureTime);
            this.pnlOrigin.Controls.Add(this.lblDepartureDate);
            this.pnlOrigin.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlOrigin.Name = "pnlOrigin";
            this.pnlOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 60, true);
            this.pnlOrigin.TabIndex = 0;
            // 
            // lblOrigin
            // 
            this.BindingSource.SetBindingMember(this.lblOrigin, "BookingInfo.Origin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo.Origin)));
            this.lblOrigin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOrigin.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblOrigin.IsFontBold = true;
            this.lblOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblOrigin.Name = "lblOrigin";
            this.lblOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.lblOrigin.TabIndex = 2;
            this.lblOrigin.Text = "O";
            this.lblOrigin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDepartureTime
            // 
            this.lblDepartureTime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDepartureTime.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblDepartureTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.lblDepartureTime.Name = "lblDepartureTime";
            this.lblDepartureTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.lblDepartureTime.TabIndex = 1;
            this.lblDepartureTime.Text = "DT";
            this.lblDepartureTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDepartureDate
            // 
            this.lblDepartureDate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDepartureDate.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.lblDepartureDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
            this.lblDepartureDate.Name = "lblDepartureDate";
            this.lblDepartureDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.lblDepartureDate.TabIndex = 0;
            this.lblDepartureDate.Text = "DD";
            this.lblDepartureDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlNonSpotInfo
            // 
            this.pnlNonSpotInfo.ColumnCount = 2;
            this.pnlNonSpotInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlNonSpotInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlNonSpotInfo.Controls.Add(this.lblExpiryDate, 1, 2);
            this.pnlNonSpotInfo.Controls.Add(this.lblExpiryDateLabel, 0, 2);
            this.pnlNonSpotInfo.Controls.Add(this.lblEffectiveDate, 1, 1);
            this.pnlNonSpotInfo.Controls.Add(this.lblEffectiveDateLabel, 0, 1);
            this.pnlNonSpotInfo.Controls.Add(this.lblRoutingOrTransitTime, 1, 0);
            this.pnlNonSpotInfo.Controls.Add(this.lblTransitTimeLabel, 0, 0);
            this.pnlNonSpotInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNonSpotInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlNonSpotInfo.Name = "pnlNonSpotInfo";
            this.pnlNonSpotInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.pnlNonSpotInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.pnlNonSpotInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.pnlNonSpotInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 69, true);
            this.pnlNonSpotInfo.TabIndex = 17;
            // 
            // lblExpiryDate
            // 
            this.BindingSource.SetBindingMember(this.lblExpiryDate, "ExpiryDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ExpiryDate)));
            this.lblExpiryDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblExpiryDate.IsFontBold = true;
            this.lblExpiryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 44, true);
            this.lblExpiryDate.Name = "lblExpiryDate";
            this.lblExpiryDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
            this.lblExpiryDate.TabIndex = 19;
            this.lblExpiryDate.Text = "ExpiryDate";
            // 
            // lblExpiryDateLabel
            // 
            this.BindingSource.SetBindingMember(this.lblExpiryDateLabel, "ExpiryDateLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ExpiryDateLabel)));
            this.lblExpiryDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExpiryDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblExpiryDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
            this.lblExpiryDateLabel.Name = "lblExpiryDateLabel";
            this.lblExpiryDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 25, true);
            this.lblExpiryDateLabel.TabIndex = 18;
            this.lblExpiryDateLabel.Text = "ExpiryDateLabel";
            this.lblExpiryDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEffectiveDate
            // 
            this.BindingSource.SetBindingMember(this.lblEffectiveDate, "EffectiveDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).EffectiveDate)));
            this.lblEffectiveDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblEffectiveDate.IsFontBold = true;
            this.lblEffectiveDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 22, true);
            this.lblEffectiveDate.Name = "lblEffectiveDate";
            this.lblEffectiveDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
            this.lblEffectiveDate.TabIndex = 17;
            this.lblEffectiveDate.Text = "EffectiveDate";
            // 
            // lblEffectiveDateLabel
            // 
            this.BindingSource.SetBindingMember(this.lblEffectiveDateLabel, "EffectiveDateLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).EffectiveDateLabel)));
            this.lblEffectiveDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEffectiveDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblEffectiveDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
            this.lblEffectiveDateLabel.Name = "lblEffectiveDateLabel";
            this.lblEffectiveDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 22, true);
            this.lblEffectiveDateLabel.TabIndex = 16;
            this.lblEffectiveDateLabel.Text = "EffectiveDateLabel";
            this.lblEffectiveDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRoutingOrTransitTime
            // 
            this.BindingSource.SetBindingMember(this.lblRoutingOrTransitTime, "RoutingOrTransitTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).RoutingOrTransitTime)));
            this.lblRoutingOrTransitTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblRoutingOrTransitTime.IsFontBold = true;
            this.lblRoutingOrTransitTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 0, true);
            this.lblRoutingOrTransitTime.Name = "lblRoutingOrTransitTime";
            this.lblRoutingOrTransitTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
            this.lblRoutingOrTransitTime.TabIndex = 14;
            this.lblRoutingOrTransitTime.Text = "RoutingOrTransitTime";
            // 
            // lblTransitTimeLabel
            // 
            this.BindingSource.SetBindingMember(this.lblTransitTimeLabel, "TransitTimeLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).TransitTimeLabel)));
            this.lblTransitTimeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTransitTimeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblTransitTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblTransitTimeLabel.Name = "lblTransitTimeLabel";
            this.lblTransitTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 22, true);
            this.lblTransitTimeLabel.TabIndex = 15;
            this.lblTransitTimeLabel.Text = "TransitTimeLabel";
            this.lblTransitTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnl2ndColumn
            // 
            this.pnl2ndColumn.ColumnCount = 3;
            this.pnl2ndColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.pnl2ndColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.pnl2ndColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53F));
            this.pnl2ndColumn.Controls.Add(this.pbCommodityInfo, 1, 2);
            this.pnl2ndColumn.Controls.Add(this.lblCommodities, 0, 2);
            this.pnl2ndColumn.Controls.Add(this.lblAccount, 0, 1);
            this.pnl2ndColumn.Controls.Add(this.lblTradeLane, 0, 0);
            this.pnl2ndColumn.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl2ndColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 3, true);
            this.pnl2ndColumn.Name = "pnl2ndColumn";
            this.pnl2ndColumn.RowCount = 1;
            this.pnl2ndColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.pnl2ndColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.pnl2ndColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.pnl2ndColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 85, true);
            this.pnl2ndColumn.TabIndex = 2;
            // 
            // pbCommodityInfo
            // 
            this.pbCommodityInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbCommodityInfo.Image = ((System.Drawing.Image)(resources.GetObject("pbCommodityInfo.Image")));
            this.pbCommodityInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 61, true);
            this.pbCommodityInfo.Name = "pbCommodityInfo";
            this.pbCommodityInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.pbCommodityInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCommodityInfo.TabIndex = 13;
            this.pbCommodityInfo.TabStop = false;
            this.pbCommodityInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbCommodityInfo_MouseUp);
            // 
            // lblCommodities
            // 
            this.BindingSource.SetBindingMember(this.lblCommodities, "Commodities");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).Commodities)));
            this.lblCommodities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommodities.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCommodities.IsFontBold = true;
            this.lblCommodities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 58, true);
            this.lblCommodities.Name = "lblCommodities";
            this.lblCommodities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 27, true);
            this.lblCommodities.TabIndex = 10;
            this.lblCommodities.Text = "Commodities";
            this.lblCommodities.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAccount
            // 
            this.BindingSource.SetBindingMember(this.lblAccount, "Account");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).Account)));
            this.lblAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAccount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblAccount.IsFontBold = true;
            this.lblAccount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
            this.lblAccount.Name = "lblAccount";
            this.lblAccount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 29, true);
            this.lblAccount.TabIndex = 9;
            this.lblAccount.Text = "Account";
            this.lblAccount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTradeLane
            // 
            this.BindingSource.SetBindingMember(this.lblTradeLane, "TradeLane");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).TradeLane)));
            this.lblTradeLane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTradeLane.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTradeLane.IsFontBold = true;
            this.lblTradeLane.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.lblTradeLane.Name = "lblTradeLane";
            this.lblTradeLane.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 29, true);
            this.lblTradeLane.TabIndex = 8;
            this.lblTradeLane.Text = "TradeLane";
            this.lblTradeLane.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnl1stColumn
            // 
            this.pnl1stColumn.ColumnCount = 2;
            this.pnl1stColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl1stColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnl1stColumn.Controls.Add(this.lblContractNumberLink, 0, 2);
            this.pnl1stColumn.Controls.Add(this.lblServiceProvider, 0, 1);
            this.pnl1stColumn.Controls.Add(this.pbSpotRate, 0, 0);
            this.pnl1stColumn.Controls.Add(this.pbProviderIcon, 1, 0);
            this.pnl1stColumn.Controls.Add(this.lblContractNumber, 0, 2);
            this.pnl1stColumn.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl1stColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.pnl1stColumn.Name = "pnl1stColumn";
            this.pnl1stColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.pnl1stColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.pnl1stColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.pnl1stColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnl1stColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 100, true);
            this.pnl1stColumn.TabIndex = 1;
            // 
            // lblContractNumberLink
            // 
            this.BindingSource.SetBindingMember(this.lblContractNumberLink, "ContractNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ContractNumber)));
            this.pnl1stColumn.SetColumnSpan(this.lblContractNumberLink, 2);
            this.lblContractNumberLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContractNumberLink.IsFontBold = true;
            this.lblContractNumberLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 80, true);
            this.lblContractNumberLink.Name = "lblContractNumberLink";
            this.lblContractNumberLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
            this.lblContractNumberLink.TabIndex = 10;
            this.lblContractNumberLink.Text = "ContractNumberLink";
            this.lblContractNumberLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblContractNumberLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblContractNumberLink_Click);
            this.lblContractNumberLink.MouseHover += new System.EventHandler(this.lblContractNumberLink_MouseHover);
            // 
            // lblServiceProvider
            // 
            this.BindingSource.SetBindingMember(this.lblServiceProvider, "ServiceProviderText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ServiceProviderText)));
            this.pnl1stColumn.SetColumnSpan(this.lblServiceProvider, 2);
            this.lblServiceProvider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblServiceProvider.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblServiceProvider.IsFontBold = true;
            this.lblServiceProvider.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
            this.lblServiceProvider.Name = "lblServiceProvider";
            this.lblServiceProvider.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 24, true);
            this.lblServiceProvider.TabIndex = 9;
            this.lblServiceProvider.Text = "ServiceProviderText";
            this.lblServiceProvider.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblServiceProvider.MouseHover += new System.EventHandler(this.lblServiceProvider_MouseHover);
            // 
            // pbSpotRate
            // 
            this.pbSpotRate.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbSpotRate.Image = ((System.Drawing.Image)(resources.GetObject("pbSpotRate.Image")));
            this.pbSpotRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 3, true);
            this.pbSpotRate.Name = "pbSpotRate";
            this.pbSpotRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 26, true);
            this.pbSpotRate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSpotRate.TabIndex = 8;
            this.pbSpotRate.TabStop = false;
            this.pbSpotRate.MouseHover += new System.EventHandler(this.pbSpotRate_MouseHover);
            // 
            // pbProviderIcon
            // 
            this.pbProviderIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbProviderIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 3, true);
            this.pbProviderIcon.Name = "pbProviderIcon";
            this.pbProviderIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 26, true);
            this.pbProviderIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbProviderIcon.TabIndex = 7;
            this.pbProviderIcon.TabStop = false;
            // 
            // lblContractNumber
            // 
            this.BindingSource.SetBindingMember(this.lblContractNumber, "ContractNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ContractNumber)));
            this.pnl1stColumn.SetColumnSpan(this.lblContractNumber, 2);
            this.lblContractNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContractNumber.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblContractNumber.IsFontBold = true;
            this.lblContractNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
            this.lblContractNumber.Name = "lblContractNumber";
            this.lblContractNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 24, true);
            this.lblContractNumber.TabIndex = 11;
            this.lblContractNumber.Text = "ContractNumber";
            this.lblContractNumber.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblContractNumber.MouseHover += new System.EventHandler(this.lblContractNumber_MouseHover);
            // 
            // pnl7thColumn
            // 
            this.pnl7thColumn.Controls.Add(this.lblWarnings);
            this.pnl7thColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl7thColumn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1973, 3, true);
            this.pnl7thColumn.Name = "pnl7thColumn";
            this.pnlTop.SetRowSpan(this.pnl7thColumn, 2);
            this.pnl7thColumn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 128, true);
            this.pnl7thColumn.TabIndex = 16;
            // 
            // lblWarnings
            // 
            this.BindingSource.SetBindingMember(this.lblWarnings, "WarningLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).WarningLabel)));
            this.lblWarnings.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblWarnings.IsFontBold = true;
            this.lblWarnings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
            this.lblWarnings.Name = "lblWarnings";
            this.lblWarnings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
            this.lblWarnings.TabIndex = 11;
            this.lblWarnings.Text = "Warnings";
            this.lblWarnings.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWarnings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWarnings_LinkClicked);
            // 
            // pnlRateDetails
            // 
            this.pnlRateDetails.AutoSize = true;
            this.pnlTop.SetColumnSpan(this.pnlRateDetails, 5);
            this.pnlRateDetails.Controls.Add(this.scrollbarPanel);
            this.pnlRateDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRateDetails.IsCollapsed = true;
            this.pnlRateDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 111, true);
            this.pnlRateDetails.Name = "pnlRateDetails";
            this.pnlRateDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
            this.pnlRateDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1549, 20, true);
            this.pnlRateDetails.TabIndex = 9;
            // 
            // scrollbarPanel
            // 
            this.scrollbarPanel.AutoScroll = true;
            this.scrollbarPanel.AutoSize = true;
            this.scrollbarPanel.Controls.Add(this.verticalLayoutPanel);
            this.scrollbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.scrollbarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.scrollbarPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 300, true);
            this.scrollbarPanel.Name = "scrollbarPanel";
            this.scrollbarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1549, 300, true);
            this.scrollbarPanel.TabIndex = 17;
            // 
            // verticalLayoutPanel
            // 
            this.verticalLayoutPanel.AutoSize = true;
            this.verticalLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.verticalLayoutPanel.Controls.Add(this.pnlChargeDetails);
            this.verticalLayoutPanel.Controls.Add(this.pnlThirdRow);
            this.verticalLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.verticalLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.verticalLayoutPanel.Name = "verticalLayoutPanel";
            this.verticalLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2166, 592, true);
            this.verticalLayoutPanel.TabIndex = 18;
            // 
            // pnlChargeDetails
            // 
            this.pnlChargeDetails.AutoSize = true;
            this.pnlChargeDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlChargeDetails.ColumnCount = 6;
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlChargeDetails.Controls.Add(this.chargesCW1BillOfLadingCharges, 0, 0);
            this.pnlChargeDetails.Controls.Add(this.chargesCW1FreightCharges, 1, 0);
            this.pnlChargeDetails.Controls.Add(this.chargesBOLCharges, 2, 0);
            this.pnlChargeDetails.Controls.Add(this.lcInlandCharges, 3, 0);
            this.pnlChargeDetails.Controls.Add(this.lcOceanCharges, 4, 0);
            this.pnlChargeDetails.Controls.Add(this.lcOutlandCharges, 5, 0);
            this.pnlChargeDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.pnlChargeDetails.Name = "pnlChargeDetails";
            this.pnlChargeDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlChargeDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2160, 340, true);
            this.pnlChargeDetails.TabIndex = 15;
            // 
            // chargesCW1BillOfLadingCharges
            // 
            this.chargesCW1BillOfLadingCharges.AllowDrop = true;
            this.chargesCW1BillOfLadingCharges.AutoSize = true;
            this.BindingSource.SetBindingMember(this.chargesCW1BillOfLadingCharges, "CW1BillOfLadingCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CW1BillOfLadingCharges)));
            this.chargesCW1BillOfLadingCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.chargesCW1BillOfLadingCharges.IsSelected = false;
            this.chargesCW1BillOfLadingCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 1, true);
            this.chargesCW1BillOfLadingCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.chargesCW1BillOfLadingCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.chargesCW1BillOfLadingCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 100, true);
            this.chargesCW1BillOfLadingCharges.Name = "chargesCW1BillOfLadingCharges";
            this.chargesCW1BillOfLadingCharges.SelectedItem = null;
            this.chargesCW1BillOfLadingCharges.SelectionChanged = null;
            this.chargesCW1BillOfLadingCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.chargesCW1BillOfLadingCharges.TabIndex = 8;
            // 
            // chargesCW1FreightCharges
            // 
            this.chargesCW1FreightCharges.AllowDrop = true;
            this.chargesCW1FreightCharges.AutoSize = true;
            this.BindingSource.SetBindingMember(this.chargesCW1FreightCharges, "CW1FreightCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CW1FreightCharges)));
            this.chargesCW1FreightCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.chargesCW1FreightCharges.IsSelected = false;
            this.chargesCW1FreightCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 1, true);
            this.chargesCW1FreightCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.chargesCW1FreightCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.chargesCW1FreightCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 110, true);
            this.chargesCW1FreightCharges.Name = "chargesCW1FreightCharges";
            this.chargesCW1FreightCharges.SelectedItem = null;
            this.chargesCW1FreightCharges.SelectionChanged = null;
            this.chargesCW1FreightCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.chargesCW1FreightCharges.TabIndex = 10;
            // 
            // chargesBOLCharges
            // 
            this.chargesBOLCharges.AllowDrop = true;
            this.chargesBOLCharges.AutoSize = true;
            this.BindingSource.SetBindingMember(this.chargesBOLCharges, "BOLCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BOLCharges)));
            this.chargesBOLCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.chargesBOLCharges.IsSelected = false;
            this.chargesBOLCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 1, true);
            this.chargesBOLCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.chargesBOLCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.chargesBOLCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 110, true);
            this.chargesBOLCharges.Name = "chargesBOLCharges";
            this.chargesBOLCharges.SelectedItem = null;
            this.chargesBOLCharges.SelectionChanged = null;
            this.chargesBOLCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.chargesBOLCharges.TabIndex = 9;
            // 
            // lcInlandCharges
            // 
            this.lcInlandCharges.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.lcInlandCharges, "InlandCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).InlandCharges)));
            this.lcInlandCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.lcInlandCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1085, 1, true);
            this.lcInlandCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.lcInlandCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 0, true);
            this.lcInlandCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 110, true);
            this.lcInlandCharges.Name = "lcInlandCharges";
            this.lcInlandCharges.SelectedItem = null;
            this.lcInlandCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.lcInlandCharges.TabIndex = 16;
            // 
            // lcOceanCharges
            // 
            this.lcOceanCharges.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.lcOceanCharges, "OceanCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).OceanCharges)));
            this.lcOceanCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.lcOceanCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1445, 1, true);
            this.lcOceanCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.lcOceanCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 0, true);
            this.lcOceanCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 110, true);
            this.lcOceanCharges.Name = "lcOceanCharges";
            this.lcOceanCharges.SelectedItem = null;
            this.lcOceanCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.lcOceanCharges.TabIndex = 15;
            // 
            // lcOutlandCharges
            // 
            this.lcOutlandCharges.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.lcOutlandCharges, "OutlandCharges");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).OutlandCharges)));
            this.lcOutlandCharges.Dock = System.Windows.Forms.DockStyle.Left;
            this.lcOutlandCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1805, 1, true);
            this.lcOutlandCharges.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 1, 5, 1, true);
            this.lcOutlandCharges.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 0, true);
            this.lcOutlandCharges.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 110, true);
            this.lcOutlandCharges.Name = "lcOutlandCharges";
            this.lcOutlandCharges.SelectedItem = null;
            this.lcOutlandCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 338, true);
            this.lcOutlandCharges.TabIndex = 17;
            // 
            // pnlThirdRow
            // 
            this.pnlThirdRow.AutoSize = true;
            this.pnlThirdRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlThirdRow.ColumnCount = 3;
            this.pnlThirdRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlThirdRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlThirdRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlThirdRow.Controls.Add(this.ucRoutes, 0, 0);
            this.pnlThirdRow.Controls.Add(this.ucPenalties, 1, 0);
            this.pnlThirdRow.Controls.Add(this.ucBookingTerms, 2, 0);
            this.pnlThirdRow.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 349, true);
            this.pnlThirdRow.Name = "pnlThirdRow";
            this.pnlThirdRow.RowCount = 1;
            this.pnlThirdRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlThirdRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlThirdRow.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1519, 240, true);
            this.pnlThirdRow.TabIndex = 16;
            this.pnlThirdRow.VisibleChanged += new System.EventHandler(this.pnlThirdRow_VisibleChanged);
            // 
            // ucRoutes
            // 
            this.ucRoutes.AllowDrop = true;
            this.ucRoutes.AutoScroll = true;
            this.BindingSource.SetBindingMember(this.ucRoutes, "BookingInfo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateSelector.Models.BookingInfoViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo)));
            this.ucRoutes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucRoutes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
            this.ucRoutes.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
            this.ucRoutes.Name = "ucRoutes";
            this.ucRoutes.SelectedItem = null;
            this.ucRoutes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 234, true);
            this.ucRoutes.TabIndex = 5;
            // 
            // ucPenalties
            // 
            this.ucPenalties.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ucPenalties, "BookingInfo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateSelector.Models.BookingInfoViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo)));
            this.ucPenalties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucPenalties.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 3, true);
            this.ucPenalties.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
            this.ucPenalties.Name = "ucPenalties";
            this.ucPenalties.SelectedItem = null;
            this.ucPenalties.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 234, true);
            this.ucPenalties.TabIndex = 6;
            // 
            // ucBookingTerms
            // 
            this.ucBookingTerms.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ucBookingTerms, "BookingInfo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateSelector.Models.BookingInfoViewModel)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).BookingInfo)));
            this.ucBookingTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucBookingTerms.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1166, 3, true);
            this.ucBookingTerms.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
            this.ucBookingTerms.Name = "ucBookingTerms";
            this.ucBookingTerms.SelectedItem = null;
            this.ucBookingTerms.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 234, true);
            this.ucBookingTerms.TabIndex = 7;
            // 
            // RateChooserCard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CardPanel);
            this.Name = "RateChooserCard";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2076, 135, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CardPanel.ResumeLayout(false);
            this.CardPanel.PerformLayout();
            this.pnlCommodityInfo.ResumeLayout(false);
            this.pnlCommodityInfo.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnl6thColumn.ResumeLayout(false);
            this.pnl6thColumn.PerformLayout();
            this.pnl6thColAutoScrollRegion.ResumeLayout(false);
            this.pnl6thColAutoScrollRegion.PerformLayout();
            this.lcsOutlandCharges.ResumeLayout(true);
            this.lcsOutlandCharges.PerformLayout();
            this.lcsOceanCharges.ResumeLayout(true);
            this.lcsOceanCharges.PerformLayout();
            this.lcsInlandCharges.ResumeLayout(true);
            this.lcsInlandCharges.PerformLayout();
            this.chargesSummaryBOLCharges.ResumeLayout(true);
            this.chargesSummaryBOLCharges.PerformLayout();
            this.chargesSummaryCW1FreightCharges.ResumeLayout(true);
            this.chargesSummaryCW1FreightCharges.PerformLayout();
            this.chargesSummaryCW1BillOfLadingCharges.ResumeLayout(true);
            this.chargesSummaryCW1BillOfLadingCharges.PerformLayout();
            this.pnlTotal.ResumeLayout(false);
            this.pnlTotal.PerformLayout();
            this.pnlTotalIcon.ResumeLayout(false);
            this.pnlTotalIcon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTotalIcon)).EndInit();
            this.pnl5thColumn.ResumeLayout(false);
            this.pnl4thColumn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCarrierServiceLevelWarning)).EndInit();
            this.pnl3rdColumn.ResumeLayout(false);
            this.pnl3rdColumn.PerformLayout();
            this.pnlRoute.ResumeLayout(false);
            this.pnlRoute.PerformLayout();
            this.pnlRouteInfo.ResumeLayout(false);
            this.pnlRouteInfo.PerformLayout();
            this.pnlTransitInfo.ResumeLayout(false);
            this.pnlTransitInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbShip)).EndInit();
            this.pnlVoyageInfo.ResumeLayout(false);
            this.pnlVoyageInfo.PerformLayout();
            this.pnlDestination.ResumeLayout(false);
            this.pnlDestination.PerformLayout();
            this.pnlOrigin.ResumeLayout(false);
            this.pnlOrigin.PerformLayout();
            this.pnlNonSpotInfo.ResumeLayout(false);
            this.pnl2ndColumn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCommodityInfo)).EndInit();
            this.pnl1stColumn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSpotRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProviderIcon)).EndInit();
            this.pnl7thColumn.ResumeLayout(false);
            this.pnl7thColumn.PerformLayout();
            this.pnlRateDetails.ResumeLayout(false);
            this.pnlRateDetails.PerformLayout();
            this.scrollbarPanel.ResumeLayout(false);
            this.scrollbarPanel.PerformLayout();
            this.verticalLayoutPanel.ResumeLayout(false);
            this.verticalLayoutPanel.PerformLayout();
            this.pnlChargeDetails.ResumeLayout(false);
            this.pnlChargeDetails.PerformLayout();
            this.chargesCW1BillOfLadingCharges.ResumeLayout(true);
            this.chargesCW1BillOfLadingCharges.PerformLayout();
            this.chargesCW1FreightCharges.ResumeLayout(true);
            this.chargesCW1FreightCharges.PerformLayout();
            this.chargesBOLCharges.ResumeLayout(true);
            this.chargesBOLCharges.PerformLayout();
            this.lcInlandCharges.ResumeLayout(true);
            this.lcInlandCharges.PerformLayout();
            this.lcOceanCharges.ResumeLayout(true);
            this.lcOceanCharges.PerformLayout();
            this.lcOutlandCharges.ResumeLayout(true);
            this.lcOutlandCharges.PerformLayout();
            this.pnlThirdRow.ResumeLayout(false);
            this.ucRoutes.ResumeLayout(true);
            this.ucRoutes.PerformLayout();
            this.ucPenalties.ResumeLayout(true);
            this.ucPenalties.PerformLayout();
            this.ucBookingTerms.ResumeLayout(true);
            this.ucBookingTerms.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel CardPanel;
		private ZArchitecture.GUI.ZCollapsiblePanel pnlRateDetails;
		private TableLayoutPanel pnlChargeDetails;
		private ChargesControl chargesCW1FreightCharges;
		private ChargesControl chargesBOLCharges;
		private ChargesControl chargesCW1BillOfLadingCharges;
		private TableLayoutPanel pnlThirdRow;
		private RateSelection.BookingTermsControl ucBookingTerms;
		private RateSelection.SpotUIControls.PenaltiesControl ucPenalties;
		private RateSelection.SpotUIControls.RoutesControl ucRoutes;
		private TableLayoutPanel pnlTop;
		private TableLayoutPanel pnl1stColumn;
		private ZArchitecture.GUI.ZLinkLabel lblContractNumberLink;
		private ZArchitecture.ZLabel lblServiceProvider;
		private ZArchitecture.GUI.ZPictureBox pbSpotRate;
		private ZArchitecture.GUI.ZPictureBox pbProviderIcon;
		private ZArchitecture.ZLabel lblContractNumber;
		private TableLayoutPanel pnl2ndColumn;
		private TableLayoutPanel pnlCommodityInfo;
		private ZArchitecture.ZTextBox txtCommodityExcluded;
		private ZArchitecture.ZLabel lblExcludedLabel;
		private ZArchitecture.ZTextBox txtCommodityIncluded;
		private ZArchitecture.ZLabel lblIncludedLabel;
		private ZArchitecture.ZLabel lblCommodityType;
		private ZArchitecture.ZLabel lblCommodityName;
		private ZArchitecture.GUI.ZPictureBox pbCommodityInfo;
		private ZArchitecture.ZLabel lblCommodities;
		private ZArchitecture.ZLabel lblAccount;
		private ZArchitecture.ZLabel lblTradeLane;
		private ZArchitecture.GUI.ZPanel pnl3rdColumn;
		private ZArchitecture.GUI.ZPanel pnl6thColumn;
		private ZArchitecture.GUI.ZPanel pnl6thColAutoScrollRegion;
		private LegChargeSummaryControl lcsOutlandCharges;
		private LegChargeSummaryControl lcsOceanCharges;
		private LegChargeSummaryControl lcsInlandCharges;
		private ChargesSummaryControl chargesSummaryBOLCharges;
		private ChargesSummaryControl chargesSummaryCW1FreightCharges;
		private ChargesSummaryControl chargesSummaryCW1BillOfLadingCharges;
		private ZArchitecture.ZLabel lblLclUnit;
		private ZArchitecture.GUI.ZPanel pnlTotal;
		private ZArchitecture.ZLabel lblTotalCalculatedCharges;
		private ZArchitecture.GUI.ZPanel pnlTotalIcon;
		private ZArchitecture.GUI.ZPictureBox pbTotalIcon;
		private ZArchitecture.ZLabel lblTotalLabel;
		private TableLayoutPanel pnl5thColumn;
		private ZArchitecture.ZLabel lblAddOnOrCarrier;
		private ZArchitecture.ZLabel lblAddonOrCarrierLabel;
		private ZArchitecture.ZLabel lblVesselOrConsignee;
		private ZArchitecture.ZLabel lblVesselOrConsigneeLabel;
		private ZArchitecture.ZLabel lblRateType2OrConsignor;
		private ZArchitecture.ZLabel lblRateType2OrConsignorLabel;
		private TableLayoutPanel pnl4thColumn;
		private ZArchitecture.ZLabel lblRateType;
		private ZArchitecture.ZLabel lblRateTypeOrBlankLabel;
		private ZArchitecture.ZLabel lblServiceStringOrLevel;
		private ZArchitecture.ZLabel lblServiceStringOrLevelLabel;
		private ZArchitecture.GUI.ZPictureBox pbCarrierServiceLevelWarning;
		private ZArchitecture.ZLabel lblCarrierServiceLevel;
		private ZArchitecture.ZLabel lblCarrierServiceLevelLabel;
		private ZArchitecture.GUI.ZPanel pnl7thColumn;
		private ZArchitecture.GUI.ZLinkLabel lblWarnings;
		private TableLayoutPanel pnlNonSpotInfo;
		private ZArchitecture.ZLabel lblExpiryDate;
		private ZArchitecture.ZLabel lblExpiryDateLabel;
		private ZArchitecture.ZLabel lblEffectiveDate;
		private ZArchitecture.ZLabel lblEffectiveDateLabel;
		private ZArchitecture.ZLabel lblRoutingOrTransitTime;
		private ZArchitecture.ZLabel lblTransitTimeLabel;
		private ZArchitecture.GUI.ZPanel pnlRoute;
		private ZArchitecture.GUI.ZPanel pnlRouteInfo;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZPanel pnlTransitInfo;
		private ZArchitecture.GUI.ZPictureBox pbShip;
		private ZArchitecture.GUI.ZPanel pnlVoyageInfo;
		private ZArchitecture.ZLabel lblSpotTransitTime;
		private ZArchitecture.ZLabel lblSpotRateID;
		private ZArchitecture.GUI.ZPanel pnlDestination;
		private ZArchitecture.ZLabel lblDestination;
		private ZArchitecture.ZLabel lblArrivalTime;
		private ZArchitecture.ZLabel lblArrivalDate;
		private ZArchitecture.GUI.ZPanel pnlOrigin;
		private ZArchitecture.ZLabel lblOrigin;
		private ZArchitecture.ZLabel lblDepartureTime;
		private ZArchitecture.ZLabel lblDepartureDate;
		private ZArchitecture.ZLabel lblSpotVoyageNumber;
		private ZArchitecture.ZLabel lblSpotVesselName;
		private LegChargesControl lcInlandCharges;
		private LegChargesControl lcOceanCharges;
		private LegChargesControl lcOutlandCharges;
		private ZArchitecture.GUI.ZPanel scrollbarPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel verticalLayoutPanel;
	}
}
