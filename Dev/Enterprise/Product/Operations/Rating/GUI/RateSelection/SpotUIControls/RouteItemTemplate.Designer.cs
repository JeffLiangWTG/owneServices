using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class RouteItemTemplate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RouteItemTemplate));
            this.pnlRoute = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlRouteInfo = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.transitInfoCenteringTable = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.transitInfoFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.lblTransitTime = new Enterprise.ZArchitecture.ZLabel();
            this.pbShip = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.pbAirplane = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblFlagCode = new Enterprise.ZArchitecture.ZLabel();
            this.voyageInfoCenteringTable = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.voyageInfoFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.lblVoyageNumber = new Enterprise.ZArchitecture.ZLabel();
            this.pnlDestination = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblPortOfDischargeCode = new Enterprise.ZArchitecture.ZLabel();
            this.portOfDischargeNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.estimatedArrivalLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlOrigin = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblPortOfLoadingCode = new Enterprise.ZArchitecture.ZLabel();
            this.portOfLoadingNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.estimatedDepartureLabel = new Enterprise.ZArchitecture.ZLabel();
            this.cpnlDeadlines = new Enterprise.ZArchitecture.GUI.ZCollapsiblePanel();
            this.pnlDeadlineContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlColumnTitles = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblName = new Enterprise.ZArchitecture.ZLabel();
            this.lblType = new Enterprise.ZArchitecture.ZLabel();
            this.lblDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblCode = new Enterprise.ZArchitecture.ZLabel();
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblTitle = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlRoute.SuspendLayout();
            this.pnlRouteInfo.SuspendLayout();
            this.transitInfoCenteringTable.SuspendLayout();
            this.transitInfoFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbShip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAirplane)).BeginInit();
            this.voyageInfoCenteringTable.SuspendLayout();
            this.voyageInfoFlowLayoutPanel.SuspendLayout();
            this.pnlDestination.SuspendLayout();
            this.pnlOrigin.SuspendLayout();
            this.cpnlDeadlines.SuspendLayout();
            this.pnlColumnTitles.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.TransportLegViewModel);
            // 
            // pnlRoute
            // 
            this.pnlRoute.Controls.Add(this.pnlRouteInfo);
            this.pnlRoute.Controls.Add(this.pnlDestination);
            this.pnlRoute.Controls.Add(this.pnlOrigin);
            this.pnlRoute.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRoute.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlRoute.Name = "pnlRoute";
            this.pnlRoute.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 60, true);
            this.pnlRoute.TabIndex = 0;
            // 
            // pnlRouteInfo
            // 
            this.pnlRouteInfo.Controls.Add(this.zPanel1);
            this.pnlRouteInfo.Controls.Add(this.transitInfoCenteringTable);
            this.pnlRouteInfo.Controls.Add(this.voyageInfoCenteringTable);
            this.pnlRouteInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRouteInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 0, true);
            this.pnlRouteInfo.Name = "pnlRouteInfo";
            this.pnlRouteInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 60, true);
            this.pnlRouteInfo.TabIndex = 2;
            // 
            // zPanel1
            // 
            this.zPanel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
            this.zPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 1, true);
            this.zPanel1.TabIndex = 3;
            // 
            // transitInfoCenteringTable
            // 
            this.transitInfoCenteringTable.ColumnCount = 1;
            this.transitInfoCenteringTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.transitInfoCenteringTable.Controls.Add(this.transitInfoFlowLayoutPanel, 0, 0);
            this.transitInfoCenteringTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.transitInfoCenteringTable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.transitInfoCenteringTable.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.transitInfoCenteringTable.Name = "transitInfoCenteringTable";
            this.transitInfoCenteringTable.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 7, 0, 0, true);
            this.transitInfoCenteringTable.RowCount = 1;
            this.transitInfoCenteringTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.transitInfoCenteringTable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 27, true);
            this.transitInfoCenteringTable.TabIndex = 8;
            // 
            // transitInfoFlowLayoutPanel
            // 
            this.transitInfoFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.transitInfoFlowLayoutPanel.AutoSize = true;
            this.transitInfoFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.transitInfoFlowLayoutPanel.Controls.Add(this.lblTransitTime);
            this.transitInfoFlowLayoutPanel.Controls.Add(this.pbShip);
            this.transitInfoFlowLayoutPanel.Controls.Add(this.pbAirplane);
            this.transitInfoFlowLayoutPanel.Controls.Add(this.lblFlagCode);
            this.transitInfoFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
            this.transitInfoFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.transitInfoFlowLayoutPanel.Name = "transitInfoFlowLayoutPanel";
            this.transitInfoFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 18, true);
            this.transitInfoFlowLayoutPanel.TabIndex = 2;
            this.transitInfoFlowLayoutPanel.WrapContents = false;
            // 
            // lblTransitTime
            // 
            this.lblTransitTime.AutoSize = true;
            this.lblTransitTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTransitTime.IsFontBold = true;
            this.lblTransitTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblTransitTime.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.lblTransitTime.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 18, true);
            this.lblTransitTime.Name = "lblTransitTime";
            this.lblTransitTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
            this.lblTransitTime.TabIndex = 7;
            this.lblTransitTime.Text = "TransitTime";
            this.lblTransitTime.UseMnemonic = false;
            this.lblTransitTime.MouseHover += new System.EventHandler(this.lblTransitTime_MouseHover);
            // 
            // pbShip
            // 
            this.pbShip.Image = ((System.Drawing.Image)(resources.GetObject("pbShip.Image")));
            this.pbShip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 0, true);
            this.pbShip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
            this.pbShip.Name = "pbShip";
            this.pbShip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
            this.pbShip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbShip.TabIndex = 6;
            this.pbShip.TabStop = false;
            // 
            // pbAirplane
            // 
            this.pbAirplane.Image = ((System.Drawing.Image)(resources.GetObject("pbAirplane.Image")));
            this.pbAirplane.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 0, true);
            this.pbAirplane.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
            this.pbAirplane.Name = "pbAirplane";
            this.pbAirplane.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
            this.pbAirplane.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbAirplane.TabIndex = 5;
            this.pbAirplane.TabStop = false;
            // 
            // lblFlagCode
            // 
            this.lblFlagCode.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblFlagCode, "FlagCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegViewModel)(null)).FlagCode)));
            this.lblFlagCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblFlagCode.IsFontBold = true;
            this.lblFlagCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 0, true);
            this.lblFlagCode.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 18, true);
            this.lblFlagCode.Name = "lblFlagCode";
            this.lblFlagCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
            this.lblFlagCode.TabIndex = 4;
            this.lblFlagCode.Text = "FlagCode";
            this.lblFlagCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblFlagCode.UseMnemonic = false;
            // 
            // voyageInfoCenteringTable
            // 
            this.voyageInfoCenteringTable.ColumnCount = 1;
            this.voyageInfoCenteringTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.voyageInfoCenteringTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.voyageInfoCenteringTable.Controls.Add(this.voyageInfoFlowLayoutPanel, 0, 0);
            this.voyageInfoCenteringTable.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.voyageInfoCenteringTable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
            this.voyageInfoCenteringTable.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.voyageInfoCenteringTable.Name = "voyageInfoCenteringTable";
            this.voyageInfoCenteringTable.RowCount = 1;
            this.voyageInfoCenteringTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.voyageInfoCenteringTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
            this.voyageInfoCenteringTable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 31, true);
            this.voyageInfoCenteringTable.TabIndex = 5;
            // 
            // voyageInfoFlowLayoutPanel
            // 
            this.voyageInfoFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.voyageInfoFlowLayoutPanel.AutoSize = true;
            this.voyageInfoFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.voyageInfoFlowLayoutPanel.Controls.Add(this.lblVoyageNumber);
            this.voyageInfoFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 0, true);
            this.voyageInfoFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.voyageInfoFlowLayoutPanel.Name = "voyageInfoFlowLayoutPanel";
            this.voyageInfoFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
            this.voyageInfoFlowLayoutPanel.TabIndex = 0;
            this.voyageInfoFlowLayoutPanel.WrapContents = false;
            // 
            // lblVoyageNumber
            // 
            this.lblVoyageNumber.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblVoyageNumber, "VoyageNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegViewModel)(null)).VoyageNumber)));
            this.lblVoyageNumber.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblVoyageNumber.IsFontBold = true;
            this.lblVoyageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblVoyageNumber.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.lblVoyageNumber.Name = "lblVoyageNumber";
            this.lblVoyageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
            this.lblVoyageNumber.TabIndex = 3;
            this.lblVoyageNumber.Text = "VoyageNumber";
            this.lblVoyageNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblVoyageNumber.UseMnemonic = false;
            // 
            // pnlDestination
            // 
            this.pnlDestination.Controls.Add(this.lblPortOfDischargeCode);
            this.pnlDestination.Controls.Add(this.portOfDischargeNameLabel);
            this.pnlDestination.Controls.Add(this.estimatedArrivalLabel);
            this.pnlDestination.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlDestination.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 0, true);
            this.pnlDestination.Name = "pnlDestination";
            this.pnlDestination.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 60, true);
            this.pnlDestination.TabIndex = 1;
            // 
            // lblPortOfDischargeCode
            // 
            this.BindingSource.SetBindingMember(this.lblPortOfDischargeCode, "PortOfDischargeCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegViewModel)(null)).PortOfDischargeCode)));
            this.lblPortOfDischargeCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPortOfDischargeCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblPortOfDischargeCode.IsFontBold = true;
            this.lblPortOfDischargeCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblPortOfDischargeCode.Name = "lblPortOfDischargeCode";
            this.lblPortOfDischargeCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.lblPortOfDischargeCode.TabIndex = 5;
            this.lblPortOfDischargeCode.Text = "PortOfDischargeCode";
            this.lblPortOfDischargeCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPortOfDischargeCode.UseMnemonic = false;
            // 
            // portOfDischargeNameLabel
            // 
            this.portOfDischargeNameLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.portOfDischargeNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.portOfDischargeNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.portOfDischargeNameLabel.Name = "portOfDischargeNameLabel";
            this.portOfDischargeNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.portOfDischargeNameLabel.TabIndex = 4;
            this.portOfDischargeNameLabel.Text = "PortOfDischargeName";
            this.portOfDischargeNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.portOfDischargeNameLabel.UseMnemonic = false;
            // 
            // estimatedArrivalLabel
            // 
            this.estimatedArrivalLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.estimatedArrivalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.estimatedArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
            this.estimatedArrivalLabel.Name = "estimatedArrivalLabel";
            this.estimatedArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.estimatedArrivalLabel.TabIndex = 3;
            this.estimatedArrivalLabel.Text = "EstimatedArrival";
            this.estimatedArrivalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.estimatedArrivalLabel.UseMnemonic = false;
            // 
            // pnlOrigin
            // 
            this.pnlOrigin.Controls.Add(this.lblPortOfLoadingCode);
            this.pnlOrigin.Controls.Add(this.portOfLoadingNameLabel);
            this.pnlOrigin.Controls.Add(this.estimatedDepartureLabel);
            this.pnlOrigin.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlOrigin.Name = "pnlOrigin";
            this.pnlOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 60, true);
            this.pnlOrigin.TabIndex = 0;
            // 
            // lblPortOfLoadingCode
            // 
            this.BindingSource.SetBindingMember(this.lblPortOfLoadingCode, "PortOfLoadingCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegViewModel)(null)).PortOfLoadingCode)));
            this.lblPortOfLoadingCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPortOfLoadingCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblPortOfLoadingCode.IsFontBold = true;
            this.lblPortOfLoadingCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblPortOfLoadingCode.Name = "lblPortOfLoadingCode";
            this.lblPortOfLoadingCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.lblPortOfLoadingCode.TabIndex = 2;
            this.lblPortOfLoadingCode.Text = "PortOfLoadingCode";
            this.lblPortOfLoadingCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPortOfLoadingCode.UseMnemonic = false;
            // 
            // portOfLoadingNameLabel
            // 
            this.portOfLoadingNameLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.portOfLoadingNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.portOfLoadingNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.portOfLoadingNameLabel.Name = "portOfLoadingNameLabel";
            this.portOfLoadingNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.portOfLoadingNameLabel.TabIndex = 1;
            this.portOfLoadingNameLabel.Text = "PortOfLoadingName";
            this.portOfLoadingNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.portOfLoadingNameLabel.UseMnemonic = false;
            // 
            // estimatedDepartureLabel
            // 
            this.estimatedDepartureLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.estimatedDepartureLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.estimatedDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
            this.estimatedDepartureLabel.Name = "estimatedDepartureLabel";
            this.estimatedDepartureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
            this.estimatedDepartureLabel.TabIndex = 0;
            this.estimatedDepartureLabel.Text = "EstimatedDeparture";
            this.estimatedDepartureLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.estimatedDepartureLabel.UseMnemonic = false;
            // 
            // cpnlDeadlines
            // 
            this.cpnlDeadlines.AutoSize = true;
            this.cpnlDeadlines.Controls.Add(this.pnlDeadlineContainer);
            this.cpnlDeadlines.Controls.Add(this.pnlColumnTitles);
            this.cpnlDeadlines.Controls.Add(this.pnlTop);
            this.cpnlDeadlines.Dock = System.Windows.Forms.DockStyle.Top;
            this.cpnlDeadlines.IsCollapsed = false;
            this.cpnlDeadlines.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
            this.cpnlDeadlines.Name = "cpnlDeadlines";
            this.cpnlDeadlines.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
            this.cpnlDeadlines.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 64, true);
            this.cpnlDeadlines.TabIndex = 1;
            // 
            // pnlDeadlineContainer
            // 
            this.pnlDeadlineContainer.AutoSize = true;
            this.pnlDeadlineContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDeadlineContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
            this.pnlDeadlineContainer.Name = "pnlDeadlineContainer";
            this.pnlDeadlineContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 0, true);
            this.pnlDeadlineContainer.TabIndex = 3;
            // 
            // pnlColumnTitles
            // 
            this.pnlColumnTitles.Controls.Add(this.lblName);
            this.pnlColumnTitles.Controls.Add(this.lblType);
            this.pnlColumnTitles.Controls.Add(this.lblDate);
            this.pnlColumnTitles.Controls.Add(this.lblCode);
            this.pnlColumnTitles.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlColumnTitles.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
            this.pnlColumnTitles.Name = "pnlColumnTitles";
            this.pnlColumnTitles.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 22, true);
            this.pnlColumnTitles.TabIndex = 2;
            // 
            // lblName
            // 
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblName.IsFontBold = true;
            this.lblName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 0, true);
            this.lblName.Name = "lblName";
            this.lblName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 22, true);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "Name";
            this.lblName.UseMnemonic = false;
            // 
            // lblType
            // 
            this.lblType.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblType.IsFontBold = true;
            this.lblType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 0, true);
            this.lblType.Name = "lblType";
            this.lblType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 22, true);
            this.lblType.TabIndex = 7;
            this.lblType.Text = "Type";
            this.lblType.UseMnemonic = false;
            // 
            // lblDate
            // 
            this.lblDate.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblDate.IsFontBold = true;
            this.lblDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 0, true);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 22, true);
            this.lblDate.TabIndex = 6;
            this.lblDate.Text = "Date";
            this.lblDate.UseMnemonic = false;
            // 
            // lblCode
            // 
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCode.IsFontBold = true;
            this.lblCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 22, true);
            this.lblCode.TabIndex = 1;
            this.lblCode.Text = "Code";
            this.lblCode.UseMnemonic = false;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 22, true);
            this.pnlTop.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTitle.IsFontBold = true;
            this.lblTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 22, true);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Deadlines";
            this.lblTitle.UseMnemonic = false;
            // 
            // RouteItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.cpnlDeadlines);
            this.Controls.Add(this.pnlRoute);
            this.Name = "RouteItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 171, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlRoute.ResumeLayout(false);
            this.pnlRoute.PerformLayout();
            this.pnlRouteInfo.ResumeLayout(false);
            this.pnlRouteInfo.PerformLayout();
            this.transitInfoCenteringTable.ResumeLayout(false);
            this.transitInfoCenteringTable.PerformLayout();
            this.transitInfoFlowLayoutPanel.ResumeLayout(false);
            this.transitInfoFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbShip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAirplane)).EndInit();
            this.voyageInfoCenteringTable.ResumeLayout(false);
            this.voyageInfoCenteringTable.PerformLayout();
            this.voyageInfoFlowLayoutPanel.ResumeLayout(false);
            this.voyageInfoFlowLayoutPanel.PerformLayout();
            this.pnlDestination.ResumeLayout(false);
            this.pnlDestination.PerformLayout();
            this.pnlOrigin.ResumeLayout(false);
            this.pnlOrigin.PerformLayout();
            this.cpnlDeadlines.ResumeLayout(false);
            this.cpnlDeadlines.PerformLayout();
            this.pnlColumnTitles.ResumeLayout(false);
            this.pnlColumnTitles.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlRoute;
		private ZArchitecture.GUI.ZCollapsiblePanel cpnlDeadlines;
		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.ZLabel lblTitle;
		private ZArchitecture.GUI.ZPanel pnlRouteInfo;
		private ZArchitecture.GUI.ZPanel pnlDestination;
		private ZArchitecture.GUI.ZPanel pnlOrigin;
		private ZArchitecture.GUI.ZPanel pnlColumnTitles;
		private ZArchitecture.ZLabel lblType;
		private ZArchitecture.ZLabel lblDate;
		private ZArchitecture.ZLabel lblCode;
		private ZArchitecture.ZLabel lblName;
		private ZArchitecture.GUI.ZPanel pnlDeadlineContainer;
		private ZArchitecture.ZLabel estimatedDepartureLabel;
		private ZArchitecture.ZLabel portOfLoadingNameLabel;
		private ZArchitecture.ZLabel lblPortOfLoadingCode;
		private ZArchitecture.ZLabel lblPortOfDischargeCode;
		private ZArchitecture.ZLabel portOfDischargeNameLabel;
		private ZArchitecture.ZLabel estimatedArrivalLabel;
		private CargoWise.Windows.UI.KFlowLayoutPanel voyageInfoFlowLayoutPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel transitInfoFlowLayoutPanel;
		private ZArchitecture.ZLabel lblVoyageNumber;
		private ZArchitecture.ZLabel lblFlagCode;
		private ZArchitecture.GUI.ZPictureBox pbShip;
		private ZArchitecture.GUI.ZPictureBox pbAirplane;
		private ZArchitecture.ZLabel lblTransitTime;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private KTableLayoutPanel transitInfoCenteringTable;
		private KTableLayoutPanel voyageInfoCenteringTable;
	}
}
