using Enterprise.Rating.GUI.RateSelection.SpotUIControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class BookingEngineRateCardControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblCarrierName = new Enterprise.ZArchitecture.ZLabel();
            this.summaryTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.selectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.pnlCol1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.airlineLogoControl1 = new Enterprise.Rating.GUI.RateSelector.UIControls.AirlineLogoControl();
            this.pnlCol2 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblDepartureTimeDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblDepartureTimeTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblOrigin = new Enterprise.ZArchitecture.ZLabel();
            this.pnlCol3 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.routeViewIconsMode1 = new Enterprise.Rating.GUI.RateSelector.UIControls.RouteViewIconsMode();
            this.pnlCol4 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblArrivalTimeDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblArrivalTimeTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblDestination = new Enterprise.ZArchitecture.ZLabel();
            this.pnlCol5 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblStatusLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblRateLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlCol6 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblMainChargeStatus = new Enterprise.ZArchitecture.ZLabel();
            this.lblMainChargeRate = new Enterprise.ZArchitecture.ZLabel();
            this.pnlCol7 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.expandCollapseToggle1 = new Enterprise.Rating.GUI.RateSelector.UIControls.ExpandCollapseToggle();
            this.lblRemarks = new Enterprise.ZArchitecture.ZLabel();
            this.tblRoot = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.pnlRootLeft = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblRemarksLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlRootRight = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.totalPriceLargeDisplay1 = new Enterprise.Rating.GUI.RateSelector.UIControls.TotalPriceLargeDisplay();
            this.pnlBottomLeft = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.bottomLeftTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.pnlRoutingContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.routesControl1 = new Enterprise.Rating.GUI.RateSelection.SpotUIControls.RoutesControl();
            this.lblRoutingLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlAdditionalDetailsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.additionalDetailsControl1 = new Enterprise.Rating.GUI.RateSelection.SpotUIControls.AdditionalDetailsControl();
            this.lblAdditionalDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlBottomRight = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlCostBreakdownContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.costBreakdownControl = new Enterprise.Rating.GUI.RateSelection.SpotUIControls.CostBreakdownControl();
            this.lblCostBreakdownLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.summaryTableLayoutPanel.SuspendLayout();
            this.pnlCol1.SuspendLayout();
            this.airlineLogoControl1.SuspendLayout();
            this.pnlCol2.SuspendLayout();
            this.pnlCol3.SuspendLayout();
            this.routeViewIconsMode1.SuspendLayout();
            this.pnlCol4.SuspendLayout();
            this.pnlCol5.SuspendLayout();
            this.pnlCol6.SuspendLayout();
            this.pnlCol7.SuspendLayout();
            this.expandCollapseToggle1.SuspendLayout();
            this.tblRoot.SuspendLayout();
            this.pnlRootLeft.SuspendLayout();
            this.pnlRootRight.SuspendLayout();
            this.totalPriceLargeDisplay1.SuspendLayout();
            this.pnlBottomLeft.SuspendLayout();
            this.bottomLeftTableLayoutPanel.SuspendLayout();
            this.pnlRoutingContainer.SuspendLayout();
            this.routesControl1.SuspendLayout();
            this.pnlAdditionalDetailsContainer.SuspendLayout();
            this.pnlBottomRight.SuspendLayout();
            this.pnlCostBreakdownContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel);
            // 
            // lblCarrierName
            // 
            this.lblCarrierName.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblCarrierName, "CarrierName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).CarrierName)));
            this.lblCarrierName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCarrierName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
            this.lblCarrierName.Name = "lblCarrierName";
            this.lblCarrierName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 19, true);
            this.lblCarrierName.TabIndex = 0;
            this.lblCarrierName.Text = "Carrier";
            this.lblCarrierName.UseMnemonic = false;
            // 
            // summaryTableLayoutPanel
            // 
            this.summaryTableLayoutPanel.AutoSize = true;
            this.summaryTableLayoutPanel.ColumnCount = 8;
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.summaryTableLayoutPanel.Controls.Add(this.selectedCheckBox, 0, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol1, 1, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol2, 2, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol3, 3, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol4, 4, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol5, 5, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol6, 6, 0);
            this.summaryTableLayoutPanel.Controls.Add(this.pnlCol7, 7, 0);
            this.summaryTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
            this.summaryTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.summaryTableLayoutPanel.Name = "summaryTableLayoutPanel";
            this.summaryTableLayoutPanel.RowCount = 1;
            this.summaryTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(65)));
            this.summaryTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 65, true);
            this.summaryTableLayoutPanel.TabIndex = 2;
            // 
            // selectedCheckBox
            // 
            this.selectedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.selectedCheckBox.AutoSize = true;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.selectedCheckBox, false);
            this.selectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 25, true);
            this.selectedCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 3, 3, 3, true);
            this.selectedCheckBox.Name = "selectedCheckBox";
            this.selectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.selectedCheckBox.TabIndex = 7;
            this.selectedCheckBox.UseVisualStyleBackColor = true;
            this.selectedCheckBox.CheckedChanged += new System.EventHandler(this.selectedCheckBox_CheckedChanged);
            // 
            // pnlCol1
            // 
            this.pnlCol1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol1.Controls.Add(this.airlineLogoControl1);
            this.pnlCol1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 0, true);
            this.pnlCol1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol1.Name = "pnlCol1";
            this.pnlCol1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 65, true);
            this.pnlCol1.TabIndex = 6;
            // 
            // airlineLogoControl1
            // 
            this.airlineLogoControl1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.airlineLogoControl1, ".");
            this.airlineLogoControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
            this.airlineLogoControl1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.airlineLogoControl1.Name = "airlineLogoControl1";
            this.airlineLogoControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 42, true);
            this.airlineLogoControl1.TabIndex = 0;
            // 
            // pnlCol2
            // 
            this.pnlCol2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol2.Controls.Add(this.lblDepartureTimeDate);
            this.pnlCol2.Controls.Add(this.lblDepartureTimeTime);
            this.pnlCol2.Controls.Add(this.lblOrigin);
            this.pnlCol2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 0, true);
            this.pnlCol2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol2.Name = "pnlCol2";
            this.pnlCol2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 65, true);
            this.pnlCol2.TabIndex = 0;
            // 
            // lblDepartureTimeDate
            // 
            this.lblDepartureTimeDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblDepartureTimeDate.ForeColor = System.Drawing.Color.DimGray;
            this.lblDepartureTimeDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 44, true);
            this.lblDepartureTimeDate.Name = "lblDepartureTimeDate";
            this.lblDepartureTimeDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
            this.lblDepartureTimeDate.TabIndex = 2;
            this.lblDepartureTimeDate.Text = "DepartureTimeDate";
            this.lblDepartureTimeDate.UseMnemonic = false;
            // 
            // lblDepartureTimeTime
            // 
            this.lblDepartureTimeTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblDepartureTimeTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblDepartureTimeTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 26, true);
            this.lblDepartureTimeTime.Name = "lblDepartureTimeTime";
            this.lblDepartureTimeTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
            this.lblDepartureTimeTime.TabIndex = 1;
            this.lblDepartureTimeTime.Text = "DepartureTimeTime";
            this.lblDepartureTimeTime.UseMnemonic = false;
            // 
            // lblOrigin
            // 
            this.BindingSource.SetBindingMember(this.lblOrigin, "Origin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).Origin)));
            this.lblOrigin.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblOrigin.Name = "lblOrigin";
            this.lblOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 19, true);
            this.lblOrigin.TabIndex = 0;
            this.lblOrigin.Text = "Origin";
            this.lblOrigin.UseMnemonic = false;
            // 
            // pnlCol3
            // 
            this.pnlCol3.Controls.Add(this.routeViewIconsMode1);
            this.pnlCol3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCol3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 0, true);
            this.pnlCol3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol3.Name = "pnlCol3";
            this.pnlCol3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 65, true);
            this.pnlCol3.TabIndex = 1;
            // 
            // routeViewIconsMode1
            // 
            this.routeViewIconsMode1.AllowDrop = true;
            this.routeViewIconsMode1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.routeViewIconsMode1, ".");
            this.routeViewIconsMode1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
            this.routeViewIconsMode1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.routeViewIconsMode1.Name = "routeViewIconsMode1";
            this.routeViewIconsMode1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 30, true);
            this.routeViewIconsMode1.TabIndex = 0;
            // 
            // pnlCol4
            // 
            this.pnlCol4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol4.Controls.Add(this.lblArrivalTimeDate);
            this.pnlCol4.Controls.Add(this.lblArrivalTimeTime);
            this.pnlCol4.Controls.Add(this.lblDestination);
            this.pnlCol4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 0, true);
            this.pnlCol4.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol4.Name = "pnlCol4";
            this.pnlCol4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 65, true);
            this.pnlCol4.TabIndex = 2;
            // 
            // lblArrivalTimeDate
            // 
            this.lblArrivalTimeDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArrivalTimeDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblArrivalTimeDate.ForeColor = System.Drawing.Color.DimGray;
            this.lblArrivalTimeDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 44, true);
            this.lblArrivalTimeDate.Name = "lblArrivalTimeDate";
            this.lblArrivalTimeDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
            this.lblArrivalTimeDate.TabIndex = 2;
            this.lblArrivalTimeDate.Text = "ArrivalTimeDate";
            this.lblArrivalTimeDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblArrivalTimeDate.UseMnemonic = false;
            // 
            // lblArrivalTimeTime
            // 
            this.lblArrivalTimeTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArrivalTimeTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblArrivalTimeTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblArrivalTimeTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 26, true);
            this.lblArrivalTimeTime.Name = "lblArrivalTimeTime";
            this.lblArrivalTimeTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
            this.lblArrivalTimeTime.TabIndex = 1;
            this.lblArrivalTimeTime.Text = "ArrivalTimeTime";
            this.lblArrivalTimeTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblArrivalTimeTime.UseMnemonic = false;
            // 
            // lblDestination
            // 
            this.lblDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.lblDestination, "Destination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).Destination)));
            this.lblDestination.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblDestination.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 19, true);
            this.lblDestination.TabIndex = 0;
            this.lblDestination.Text = "Destination";
            this.lblDestination.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDestination.UseMnemonic = false;
            // 
            // pnlCol5
            // 
            this.pnlCol5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol5.Controls.Add(this.lblStatusLabel);
            this.pnlCol5.Controls.Add(this.lblRateLabel);
            this.pnlCol5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 0, true);
            this.pnlCol5.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol5.Name = "pnlCol5";
            this.pnlCol5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 65, true);
            this.pnlCol5.TabIndex = 3;
            // 
            // lblStatusLabel
            // 
            this.lblStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblStatusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 27, true);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
            this.lblStatusLabel.TabIndex = 1;
            this.lblStatusLabel.Text = "Status";
            this.lblStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblStatusLabel.UseMnemonic = false;
            // 
            // lblRateLabel
            // 
            this.lblRateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRateLabel.ForeColor = System.Drawing.Color.DimGray;
            this.lblRateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
            this.lblRateLabel.Name = "lblRateLabel";
            this.lblRateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
            this.lblRateLabel.TabIndex = 0;
            this.lblRateLabel.Text = "Rate";
            this.lblRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRateLabel.UseMnemonic = false;
            // 
            // pnlCol6
            // 
            this.pnlCol6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol6.AutoSize = true;
            this.pnlCol6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlCol6.Controls.Add(this.lblMainChargeStatus);
            this.pnlCol6.Controls.Add(this.lblMainChargeRate);
            this.pnlCol6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 0, true);
            this.pnlCol6.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol6.Name = "pnlCol6";
            this.pnlCol6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 65, true);
            this.pnlCol6.TabIndex = 4;
            // 
            // lblMainChargeStatus
            // 
            this.lblMainChargeStatus.AutoSize = true;
            this.lblMainChargeStatus.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblMainChargeStatus.IsFontBold = true;
            this.lblMainChargeStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 28, true);
            this.lblMainChargeStatus.Name = "lblMainChargeStatus";
            this.lblMainChargeStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.lblMainChargeStatus.TabIndex = 1;
            this.lblMainChargeStatus.UseMnemonic = false;
            // 
            // lblMainChargeRate
            // 
            this.lblMainChargeRate.AutoSize = true;
            this.lblMainChargeRate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblMainChargeRate.IsFontBold = true;
            this.lblMainChargeRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
            this.lblMainChargeRate.Name = "lblMainChargeRate";
            this.lblMainChargeRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.lblMainChargeRate.TabIndex = 0;
            this.lblMainChargeRate.UseMnemonic = false;
            // 
            // pnlCol7
            // 
            this.pnlCol7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCol7.Controls.Add(this.expandCollapseToggle1);
            this.pnlCol7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 0, true);
            this.pnlCol7.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlCol7.Name = "pnlCol7";
            this.pnlCol7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 65, true);
            this.pnlCol7.TabIndex = 5;
            // 
            // expandCollapseToggle1
            // 
            this.expandCollapseToggle1.AllowDrop = true;
            this.expandCollapseToggle1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 26, true);
            this.expandCollapseToggle1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.expandCollapseToggle1.Name = "expandCollapseToggle1";
            this.expandCollapseToggle1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.expandCollapseToggle1.TabIndex = 0;
            // 
            // lblRemarks
            // 
            this.lblRemarks.AutoEllipsis = true;
            this.BindingSource.SetBindingMember(this.lblRemarks, "Remarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).Remarks)));
            this.lblRemarks.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRemarks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 95, true);
            this.lblRemarks.Name = "lblRemarks";
            this.lblRemarks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 13, true);
            this.lblRemarks.TabIndex = 1;
            this.lblRemarks.Text = "Remarks";
            this.lblRemarks.UseMnemonic = false;
            // 
            // tblRoot
            // 
            this.tblRoot.AutoSize = true;
            this.tblRoot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblRoot.ColumnCount = 2;
            this.tblRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblRoot.Controls.Add(this.pnlRootLeft, 0, 0);
            this.tblRoot.Controls.Add(this.pnlRootRight, 1, 0);
            this.tblRoot.Controls.Add(this.pnlBottomLeft, 0, 1);
            this.tblRoot.Controls.Add(this.pnlBottomRight, 1, 1);
            this.tblRoot.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.tblRoot.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.tblRoot.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 115, true);
            this.tblRoot.Name = "tblRoot";
            this.tblRoot.RowCount = 2;
            this.tblRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(115)));
            this.tblRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblRoot.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 198, true);
            this.tblRoot.TabIndex = 3;
            // 
            // pnlRootLeft
            // 
            this.pnlRootLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlRootLeft.AutoSize = true;
            this.pnlRootLeft.BackColor = System.Drawing.Color.White;
            this.pnlRootLeft.Controls.Add(this.lblCarrierName);
            this.pnlRootLeft.Controls.Add(this.summaryTableLayoutPanel);
            this.pnlRootLeft.Controls.Add(this.lblRemarksLabel);
            this.pnlRootLeft.Controls.Add(this.lblRemarks);
            this.pnlRootLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlRootLeft.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlRootLeft.Name = "pnlRootLeft";
            this.pnlRootLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 115, true);
            this.pnlRootLeft.TabIndex = 0;
            // 
            // lblRemarksLabel
            // 
            this.lblRemarksLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblRemarksLabel.ForeColor = System.Drawing.Color.DimGray;
            this.lblRemarksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 95, true);
            this.lblRemarksLabel.Name = "lblRemarksLabel";
            this.lblRemarksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 13, true);
            this.lblRemarksLabel.TabIndex = 3;
            this.lblRemarksLabel.Text = "Remarks";
            this.lblRemarksLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRemarksLabel.UseMnemonic = false;
            // 
            // pnlRootRight
            // 
            this.pnlRootRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlRootRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(187)))), ((int)(((byte)(83)))));
            this.pnlRootRight.Controls.Add(this.totalPriceLargeDisplay1);
            this.pnlRootRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(718, 0, true);
            this.pnlRootRight.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlRootRight.Name = "pnlRootRight";
            this.pnlRootRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 115, true);
            this.pnlRootRight.TabIndex = 1;
            // 
            // totalPriceLargeDisplay1
            // 
            this.totalPriceLargeDisplay1.AllowDrop = true;
            this.totalPriceLargeDisplay1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.totalPriceLargeDisplay1, ".");
            this.totalPriceLargeDisplay1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 43, true);
            this.totalPriceLargeDisplay1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.totalPriceLargeDisplay1.Name = "totalPriceLargeDisplay1";
            this.totalPriceLargeDisplay1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
            this.totalPriceLargeDisplay1.TabIndex = 0;
            // 
            // pnlBottomLeft
            // 
            this.pnlBottomLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBottomLeft.AutoSize = true;
            this.pnlBottomLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBottomLeft.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlBottomLeft.Controls.Add(this.bottomLeftTableLayoutPanel);
            this.pnlBottomLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
            this.pnlBottomLeft.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlBottomLeft.Name = "pnlBottomLeft";
            this.pnlBottomLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 83, true);
            this.pnlBottomLeft.TabIndex = 3;
            // 
            // bottomLeftTableLayoutPanel
            // 
            this.bottomLeftTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLeftTableLayoutPanel.AutoSize = true;
            this.bottomLeftTableLayoutPanel.ColumnCount = 1;
            this.bottomLeftTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bottomLeftTableLayoutPanel.Controls.Add(this.pnlRoutingContainer, 0, 0);
            this.bottomLeftTableLayoutPanel.Controls.Add(this.pnlAdditionalDetailsContainer, 0, 1);
            this.bottomLeftTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
            this.bottomLeftTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
            this.bottomLeftTableLayoutPanel.Name = "bottomLeftTableLayoutPanel";
            this.bottomLeftTableLayoutPanel.RowCount = 2;
            this.bottomLeftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bottomLeftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bottomLeftTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 73, true);
            this.bottomLeftTableLayoutPanel.TabIndex = 7;
            // 
            // pnlRoutingContainer
            // 
            this.pnlRoutingContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlRoutingContainer.AutoSize = true;
            this.pnlRoutingContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlRoutingContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRoutingContainer.Controls.Add(this.routesControl1);
            this.pnlRoutingContainer.Controls.Add(this.lblRoutingLabel);
            this.pnlRoutingContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlRoutingContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlRoutingContainer.Name = "pnlRoutingContainer";
            this.pnlRoutingContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 28, true);
            this.pnlRoutingContainer.TabIndex = 5;
            // 
            // routesControl1
            // 
            this.routesControl1.AllowDrop = true;
            this.routesControl1.AutoScroll = true;
            this.routesControl1.AutoSize = true;
            this.routesControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.routesControl1, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.GUI.RateSelector.Models.IHasTransportLegs)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)))));
            this.routesControl1.ContainerPanelAutoSize = true;
            this.routesControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.routesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
            this.routesControl1.Name = "routesControl1";
            this.routesControl1.SelectedItem = null;
            this.routesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 3, true);
            this.routesControl1.TabIndex = 2;
            // 
            // lblRoutingLabel
            // 
            this.lblRoutingLabel.BackColor = System.Drawing.Color.LightGray;
            this.lblRoutingLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRoutingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblRoutingLabel.IsFontBold = true;
            this.lblRoutingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblRoutingLabel.Name = "lblRoutingLabel";
            this.lblRoutingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 23, true);
            this.lblRoutingLabel.TabIndex = 3;
            this.lblRoutingLabel.Text = "Routing";
            this.lblRoutingLabel.UseMnemonic = false;
            // 
            // pnlAdditionalDetailsContainer
            // 
            this.pnlAdditionalDetailsContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlAdditionalDetailsContainer.AutoSize = true;
            this.pnlAdditionalDetailsContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlAdditionalDetailsContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAdditionalDetailsContainer.Controls.Add(this.additionalDetailsControl1);
            this.pnlAdditionalDetailsContainer.Controls.Add(this.lblAdditionalDetailsLabel);
            this.pnlAdditionalDetailsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
            this.pnlAdditionalDetailsContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
            this.pnlAdditionalDetailsContainer.Name = "pnlAdditionalDetailsContainer";
            this.pnlAdditionalDetailsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 27, true);
            this.pnlAdditionalDetailsContainer.TabIndex = 6;
            // 
            // additionalDetailsControl1
            // 
            this.additionalDetailsControl1.AllowDrop = true;
            this.additionalDetailsControl1.AutoSize = true;
            this.additionalDetailsControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.additionalDetailsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.additionalDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.additionalDetailsControl1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
            this.additionalDetailsControl1.Name = "additionalDetailsControl1";
            this.additionalDetailsControl1.SelectedItem = null;
            this.additionalDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 0, true);
            this.additionalDetailsControl1.TabIndex = 4;
            // 
            // lblAdditionalDetailsLabel
            // 
            this.lblAdditionalDetailsLabel.BackColor = System.Drawing.Color.LightGray;
            this.lblAdditionalDetailsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAdditionalDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblAdditionalDetailsLabel.IsFontBold = true;
            this.lblAdditionalDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblAdditionalDetailsLabel.Name = "lblAdditionalDetailsLabel";
            this.lblAdditionalDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 25, true);
            this.lblAdditionalDetailsLabel.TabIndex = 0;
            this.lblAdditionalDetailsLabel.Text = "Additional Details";
            this.lblAdditionalDetailsLabel.UseMnemonic = false;
            // 
            // pnlBottomRight
            // 
            this.pnlBottomRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBottomRight.AutoSize = true;
            this.pnlBottomRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBottomRight.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlBottomRight.Controls.Add(this.pnlCostBreakdownContainer);
            this.pnlBottomRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(718, 115, true);
            this.pnlBottomRight.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlBottomRight.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 0, true);
            this.pnlBottomRight.Name = "pnlBottomRight";
            this.pnlBottomRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 83, true);
            this.pnlBottomRight.TabIndex = 4;
            // 
            // pnlCostBreakdownContainer
            // 
            this.pnlCostBreakdownContainer.AutoSize = true;
            this.pnlCostBreakdownContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlCostBreakdownContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCostBreakdownContainer.Controls.Add(this.costBreakdownControl);
            this.pnlCostBreakdownContainer.Controls.Add(this.lblCostBreakdownLabel);
            this.pnlCostBreakdownContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
            this.pnlCostBreakdownContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 5, true);
            this.pnlCostBreakdownContainer.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 0, true);
            this.pnlCostBreakdownContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 0, true);
            this.pnlCostBreakdownContainer.Name = "pnlCostBreakdownContainer";
            this.pnlCostBreakdownContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 25, true);
            this.pnlCostBreakdownContainer.TabIndex = 2;
            // 
            // costBreakdownControl
            // 
            this.costBreakdownControl.AllowDrop = true;
            this.costBreakdownControl.AutoSize = true;
            this.BindingSource.SetBindingMember(this.costBreakdownControl, ".");
            this.costBreakdownControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.costBreakdownControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
            this.costBreakdownControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.costBreakdownControl.Name = "costBreakdownControl";
            this.costBreakdownControl.SelectedItem = null;
            this.costBreakdownControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 0, true);
            this.costBreakdownControl.TabIndex = 0;
            // 
            // lblCostBreakdownLabel
            // 
            this.lblCostBreakdownLabel.BackColor = System.Drawing.Color.LightGray;
            this.lblCostBreakdownLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCostBreakdownLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCostBreakdownLabel.IsFontBold = true;
            this.lblCostBreakdownLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblCostBreakdownLabel.Name = "lblCostBreakdownLabel";
            this.lblCostBreakdownLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 23, true);
            this.lblCostBreakdownLabel.TabIndex = 1;
            this.lblCostBreakdownLabel.Text = "Cost Breakdown";
            this.lblCostBreakdownLabel.UseMnemonic = false;
            // 
            // BookingEngineRateCardControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.tblRoot);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 115, true);
            this.Name = "BookingEngineRateCardControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(918, 198, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.summaryTableLayoutPanel.ResumeLayout(false);
            this.summaryTableLayoutPanel.PerformLayout();
            this.pnlCol1.ResumeLayout(false);
            this.pnlCol1.PerformLayout();
            this.airlineLogoControl1.ResumeLayout(true);
            this.airlineLogoControl1.PerformLayout();
            this.pnlCol2.ResumeLayout(false);
            this.pnlCol2.PerformLayout();
            this.pnlCol3.ResumeLayout(false);
            this.pnlCol3.PerformLayout();
            this.routeViewIconsMode1.ResumeLayout(true);
            this.routeViewIconsMode1.PerformLayout();
            this.pnlCol4.ResumeLayout(false);
            this.pnlCol4.PerformLayout();
            this.pnlCol5.ResumeLayout(false);
            this.pnlCol5.PerformLayout();
            this.pnlCol6.ResumeLayout(false);
            this.pnlCol6.PerformLayout();
            this.pnlCol7.ResumeLayout(false);
            this.pnlCol7.PerformLayout();
            this.expandCollapseToggle1.ResumeLayout(true);
            this.expandCollapseToggle1.PerformLayout();
            this.tblRoot.ResumeLayout(false);
            this.tblRoot.PerformLayout();
            this.pnlRootLeft.ResumeLayout(false);
            this.pnlRootLeft.PerformLayout();
            this.pnlRootRight.ResumeLayout(false);
            this.pnlRootRight.PerformLayout();
            this.totalPriceLargeDisplay1.ResumeLayout(true);
            this.totalPriceLargeDisplay1.PerformLayout();
            this.pnlBottomLeft.ResumeLayout(false);
            this.pnlBottomLeft.PerformLayout();
            this.bottomLeftTableLayoutPanel.ResumeLayout(false);
            this.bottomLeftTableLayoutPanel.PerformLayout();
            this.pnlRoutingContainer.ResumeLayout(false);
            this.pnlRoutingContainer.PerformLayout();
            this.routesControl1.ResumeLayout(true);
            this.routesControl1.PerformLayout();
            this.pnlAdditionalDetailsContainer.ResumeLayout(false);
            this.pnlAdditionalDetailsContainer.PerformLayout();
            this.pnlBottomRight.ResumeLayout(false);
            this.pnlBottomRight.PerformLayout();
            this.pnlCostBreakdownContainer.ResumeLayout(false);
            this.pnlCostBreakdownContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel lblCarrierName;
		private CargoWise.Windows.UI.KTableLayoutPanel summaryTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel pnlCol2;
		private ZArchitecture.GUI.ZPanel pnlCol3;
		private ZArchitecture.ZLabel lblDepartureTimeDate;
		private ZArchitecture.ZLabel lblDepartureTimeTime;
		private ZArchitecture.ZLabel lblOrigin;
		private ZArchitecture.ZLabel lblRemarks;
		private CargoWise.Windows.UI.KTableLayoutPanel tblRoot;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private ZArchitecture.GUI.ZPanel pnlRootLeft;
		private ZArchitecture.GUI.ZPanel pnlRootRight;
		private ZArchitecture.GUI.ZPanel pnlCol4;
		private ZArchitecture.ZLabel lblArrivalTimeDate;
		private ZArchitecture.ZLabel lblArrivalTimeTime;
		private ZArchitecture.ZLabel lblDestination;
		private ZArchitecture.GUI.ZPanel pnlCol5;
		private ZArchitecture.GUI.ZPanel pnlCol6;
		private ZArchitecture.GUI.ZPanel pnlCol7;
		private ZArchitecture.GUI.ZPanel pnlCol1;
		private ZArchitecture.ZLabel lblStatusLabel;
		private ZArchitecture.ZLabel lblRateLabel;
		private ZArchitecture.ZLabel lblRemarksLabel;
		private AirlineLogoControl airlineLogoControl1;
		private RouteViewIconsMode routeViewIconsMode1;
		private ZArchitecture.ZLabel lblMainChargeStatus;
		private ZArchitecture.ZLabel lblMainChargeRate;
		private TotalPriceLargeDisplay totalPriceLargeDisplay1;
		private ExpandCollapseToggle expandCollapseToggle1;
		private RateSelection.SpotUIControls.RoutesControl routesControl1;
		private ZArchitecture.GUI.ZPanel pnlBottomLeft;
		private ZArchitecture.ZLabel lblRoutingLabel;
		private ZArchitecture.GUI.ZPanel pnlBottomRight;
		private RateSelection.SpotUIControls.CostBreakdownControl costBreakdownControl;
		private ZArchitecture.ZLabel lblCostBreakdownLabel;
		private AdditionalDetailsControl additionalDetailsControl1;
		private ZArchitecture.GUI.ZPanel pnlCostBreakdownContainer;
		private ZArchitecture.GUI.ZPanel pnlRoutingContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel bottomLeftTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel pnlAdditionalDetailsContainer;
		private ZArchitecture.ZLabel lblAdditionalDetailsLabel;
		private ZArchitecture.GUI.ZCheckBox selectedCheckBox;
	}
}
