using System.Windows.Forms;

namespace Enterprise.eTail.GUI
{
	partial class HVLVShipmentUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBoxCountProperties = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.rbScannedNoneReported = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbScannedHeld = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbScannedCleared = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbScannedCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbSurplusCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbShortCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbDeliveredCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbImportNoneReportedCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbImportHeldCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbImportClearedCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbExportNoneReportedCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbExportHeldCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbExportClearedCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.rbItemCount = new Enterprise.eTail.GUI.ToggleRadioButton();
			this.lblScannedNoneReported = new Enterprise.ZArchitecture.ZLabel();
			this.lblScannedHeld = new Enterprise.ZArchitecture.ZLabel();
			this.lblScannedCleared = new Enterprise.ZArchitecture.ZLabel();
			this.lblScannedCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblSurplusCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblShortCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblDeliveredCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblImportClearedCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblImportHeldCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblImportNoneReportedCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblExportClearedCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblExportHeldCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblExportNoneReportedCount = new Enterprise.ZArchitecture.ZLabel();
			this.lblItemCount = new Enterprise.ZArchitecture.ZLabel();
			this.consignmentUserControl = new Enterprise.eTail.GUI.HVLVConsignmentUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxCountProperties.SuspendLayout();
			this.consignmentUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVConsignmentHeader);
			// 
			// groupBoxCountProperties
			// 
			this.groupBoxCountProperties.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("8F45EBBA-B01B-4F73-A66E-DE45B4EA14C2", "Shipment Counts");
			this.groupBoxCountProperties.Controls.Add(this.rbScannedNoneReported);
			this.groupBoxCountProperties.Controls.Add(this.rbScannedHeld);
			this.groupBoxCountProperties.Controls.Add(this.rbScannedCleared);
			this.groupBoxCountProperties.Controls.Add(this.rbScannedCount);
			this.groupBoxCountProperties.Controls.Add(this.rbSurplusCount);
			this.groupBoxCountProperties.Controls.Add(this.rbShortCount);
			this.groupBoxCountProperties.Controls.Add(this.rbDeliveredCount);
			this.groupBoxCountProperties.Controls.Add(this.rbItemCount);
			this.groupBoxCountProperties.Controls.Add(this.rbImportNoneReportedCount);
			this.groupBoxCountProperties.Controls.Add(this.rbImportHeldCount);
			this.groupBoxCountProperties.Controls.Add(this.rbImportClearedCount);
			this.groupBoxCountProperties.Controls.Add(this.rbExportNoneReportedCount);
			this.groupBoxCountProperties.Controls.Add(this.rbExportHeldCount);
			this.groupBoxCountProperties.Controls.Add(this.rbExportClearedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblExportClearedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblExportHeldCount);
			this.groupBoxCountProperties.Controls.Add(this.lblExportNoneReportedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblImportClearedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblImportHeldCount);
			this.groupBoxCountProperties.Controls.Add(this.lblImportNoneReportedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblScannedNoneReported);
			this.groupBoxCountProperties.Controls.Add(this.lblScannedHeld);
			this.groupBoxCountProperties.Controls.Add(this.lblScannedCleared);
			this.groupBoxCountProperties.Controls.Add(this.lblScannedCount);
			this.groupBoxCountProperties.Controls.Add(this.lblSurplusCount);
			this.groupBoxCountProperties.Controls.Add(this.lblShortCount);
			this.groupBoxCountProperties.Controls.Add(this.lblDeliveredCount);
			this.groupBoxCountProperties.Controls.Add(this.lblItemCount);
			this.groupBoxCountProperties.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxCountProperties.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxCountProperties.Name = "groupBoxCountProperties";
			this.groupBoxCountProperties.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1381, 125, true);
			this.groupBoxCountProperties.TabIndex = 0;
			this.groupBoxCountProperties.TabStop = false;
			// 
			// rbDeliveredCount
			// 
			this.rbDeliveredCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbDeliveredCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbDeliveredCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbDeliveredCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbDeliveredCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbDeliveredCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbDeliveredCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1249, 23, true);
			this.rbDeliveredCount.Name = "DeliveredCount";
			this.rbDeliveredCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbDeliveredCount.TabIndex = 22;
			this.rbDeliveredCount.UseVisualStyleBackColor = true;
			this.rbDeliveredCount.CheckedChanged += new System.EventHandler(this.rbDeliveredCount_CheckedChanged);
			// 
			// rbShortCount
			// 
			this.rbShortCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbShortCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbShortCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbShortCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbShortCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbShortCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbShortCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1065, 23, true);
			this.rbShortCount.Name = "ShortCount";
			this.rbShortCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbShortCount.TabIndex = 20;
			this.rbShortCount.UseVisualStyleBackColor = true;
			this.rbShortCount.CheckedChanged += new System.EventHandler(this.rbShortCount_CheckedChanged);
			// 
			// rbScannedNoneReported
			// 
			this.rbScannedNoneReported.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbScannedNoneReported.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbScannedNoneReported.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedNoneReported.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedNoneReported.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedNoneReported.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbScannedNoneReported.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 53, true);
			this.rbScannedNoneReported.Name = "ScannedNoneReportedCount";
			this.rbScannedNoneReported.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbScannedNoneReported.TabIndex = 18;
			this.rbScannedNoneReported.UseVisualStyleBackColor = true;
			this.rbScannedNoneReported.CheckedChanged += new System.EventHandler(this.rbScannedNoneReported_CheckedChanged);
			// 
			// rbScannedHeld
			// 
			this.rbScannedHeld.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbScannedHeld.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbScannedHeld.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedHeld.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedHeld.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedHeld.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbScannedHeld.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 53, true);
			this.rbScannedHeld.Name = "ScannedHeldCount";
			this.rbScannedHeld.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbScannedHeld.TabIndex = 16;
			this.rbScannedHeld.UseVisualStyleBackColor = true;
			this.rbScannedHeld.CheckedChanged += new System.EventHandler(this.rbScannedHeld_CheckedChanged);
			// 
			// rbScannedCleared
			// 
			this.rbScannedCleared.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbScannedCleared.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbScannedCleared.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCleared.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCleared.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCleared.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbScannedCleared.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 53, true);
			this.rbScannedCleared.Name = "ScannedClearedCount";
			this.rbScannedCleared.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbScannedCleared.TabIndex = 14;
			this.rbScannedCleared.UseVisualStyleBackColor = true;
			this.rbScannedCleared.CheckedChanged += new System.EventHandler(this.rbScannedCleared_CheckedChanged);
			// 
			// rbScannedCount
			// 
			this.rbScannedCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbScannedCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbScannedCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbScannedCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbScannedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 53, true);
			this.rbScannedCount.Name = "ScannedCount";
			this.rbScannedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbScannedCount.TabIndex = 12;
			this.rbScannedCount.UseVisualStyleBackColor = true;
			this.rbScannedCount.CheckedChanged += new System.EventHandler(this.rbScannedCount_CheckedChanged);
			// 
			// rbSurplusCount
			// 
			this.rbSurplusCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbSurplusCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbSurplusCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbSurplusCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbSurplusCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbSurplusCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbSurplusCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(858, 23, true);
			this.rbSurplusCount.Name = "SurplusCount";
			this.rbSurplusCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbSurplusCount.TabIndex = 10;
			this.rbSurplusCount.UseVisualStyleBackColor = true;
			this.rbSurplusCount.CheckedChanged += new System.EventHandler(this.rbSurplusCount_CheckedChanged);
			// 
			// rbImportNoneReportedCount
			// 
			this.rbImportNoneReportedCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbImportNoneReportedCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbImportNoneReportedCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbImportNoneReportedCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbImportNoneReportedCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbImportNoneReportedCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbImportNoneReportedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 23, true);
			this.rbImportNoneReportedCount.Name = "ImportNoneReportedCount";
			this.rbImportNoneReportedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbImportNoneReportedCount.TabIndex = 8;
			this.rbImportNoneReportedCount.UseVisualStyleBackColor = true;
			this.rbImportNoneReportedCount.CheckedChanged += new System.EventHandler(this.rbImportNoneReportedCount_CheckedChanged);
			// 
			// rbImportHeldCount
			// 
			this.rbImportHeldCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbImportHeldCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbImportHeldCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbImportHeldCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbImportHeldCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbImportHeldCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbImportHeldCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 23, true);
			this.rbImportHeldCount.Name = "ImportHeldCount";
			this.rbImportHeldCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbImportHeldCount.TabIndex = 6;
			this.rbImportHeldCount.UseVisualStyleBackColor = true;
			this.rbImportHeldCount.CheckedChanged += new System.EventHandler(this.rbImportHeldCount_CheckedChanged);
			// 
			// rbImportClearedCount
			// 
			this.rbImportClearedCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbImportClearedCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbImportClearedCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbImportClearedCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbImportClearedCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbImportClearedCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbImportClearedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 23, true);
			this.rbImportClearedCount.Name = "ImportClearedCount";
			this.rbImportClearedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbImportClearedCount.TabIndex = 4;
			this.rbImportClearedCount.UseVisualStyleBackColor = true;
			this.rbImportClearedCount.CheckedChanged += new System.EventHandler(this.rbImportClearedCount_CheckedChanged);
			// 
			// rbExportNoneReportedCount
			// 
			this.rbExportNoneReportedCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbExportNoneReportedCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbExportNoneReportedCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbExportNoneReportedCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbExportNoneReportedCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbExportNoneReportedCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbExportNoneReportedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 23, true);
			this.rbExportNoneReportedCount.Name = "ExportNoneReportedCount";
			this.rbExportNoneReportedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbExportNoneReportedCount.TabIndex = 8;
			this.rbExportNoneReportedCount.UseVisualStyleBackColor = true;
			this.rbExportNoneReportedCount.CheckedChanged += new System.EventHandler(this.rbExportNoneReportedCount_CheckedChanged);
			// 
			// rbExportHeldCount
			// 
			this.rbExportHeldCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbExportHeldCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbExportHeldCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbExportHeldCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbExportHeldCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbExportHeldCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbExportHeldCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 23, true);
			this.rbExportHeldCount.Name = "ExportHeldCount";
			this.rbExportHeldCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbExportHeldCount.TabIndex = 6;
			this.rbExportHeldCount.UseVisualStyleBackColor = true;
			this.rbExportHeldCount.CheckedChanged += new System.EventHandler(this.rbExportHeldCount_CheckedChanged);
			// 
			// rbExportClearedCount
			// 
			this.rbExportClearedCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbExportClearedCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbExportClearedCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbExportClearedCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbExportClearedCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbExportClearedCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbExportClearedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 23, true);
			this.rbExportClearedCount.Name = "ExportClearedCount";
			this.rbExportClearedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbExportClearedCount.TabIndex = 4;
			this.rbExportClearedCount.UseVisualStyleBackColor = true;
			this.rbExportClearedCount.CheckedChanged += new System.EventHandler(this.rbExportClearedCount_CheckedChanged);
			// 
			// rbItemCount
			// 
			this.rbItemCount.Appearance = System.Windows.Forms.Appearance.Button;
			this.rbItemCount.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rbItemCount.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Control;
			this.rbItemCount.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
			this.rbItemCount.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
			this.rbItemCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rbItemCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 23, true);
			this.rbItemCount.Name = "ItemCount";
			this.rbItemCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 25, true);
			this.rbItemCount.TabIndex = 2;
			this.rbItemCount.UseVisualStyleBackColor = true;
			this.rbItemCount.CheckedChanged += new System.EventHandler(this.rbItemCount_CheckedChanged);
			// 
			// lblDeliveredCount
			// 
			this.lblDeliveredCount.AutoSize = true;
			this.lblDeliveredCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("F3EDCBB0-FC2A-43BB-BDFD-249E1BC652B0", "Delivered");
			this.lblDeliveredCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblDeliveredCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1189, 29, true);
			this.lblDeliveredCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblDeliveredCount.Name = "lblDeliveredCount";
			this.lblDeliveredCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 14, true);
			this.lblDeliveredCount.TabIndex = 21;
			this.lblDeliveredCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblShortCount
			// 
			this.lblShortCount.AutoSize = true;
			this.lblShortCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("37962921-2269-4A04-BDBD-F19D7BE6667B", "Short Shipped");
			this.lblShortCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblShortCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(983, 29, true);
			this.lblShortCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblShortCount.Name = "lblShortCount";
			this.lblShortCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 14, true);
			this.lblShortCount.TabIndex = 19;
			this.lblShortCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblScannedNoneReported
			// 
			this.lblScannedNoneReported.AutoSize = true;
			this.lblScannedNoneReported.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("CD2EAE71-105F-428C-ADF4-4C541085A332", "Scanned None Reported");
			this.lblScannedNoneReported.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblScannedNoneReported.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 59, true);
			this.lblScannedNoneReported.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblScannedNoneReported.Name = "lblScannedNoneReported";
			this.lblScannedNoneReported.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 14, true);
			this.lblScannedNoneReported.TabIndex = 17;
			this.lblScannedNoneReported.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblScannedHeld
			// 
			this.lblScannedHeld.AutoSize = true;
			this.lblScannedHeld.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("D60FB06C-3CB9-4ED5-A3F4-BF324CB328CB", "Scanned Held");
			this.lblScannedHeld.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblScannedHeld.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 59, true);
			this.lblScannedHeld.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblScannedHeld.Name = "lblScannedHeld";
			this.lblScannedHeld.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 14, true);
			this.lblScannedHeld.TabIndex = 15;
			this.lblScannedHeld.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblScannedCleared
			// 
			this.lblScannedCleared.AutoSize = true;
			this.lblScannedCleared.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("50724BFC-62F4-4846-ACEC-BFC5F07ED4C0", "Scanned Cleared");
			this.lblScannedCleared.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblScannedCleared.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 59, true);
			this.lblScannedCleared.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblScannedCleared.Name = "lblScannedCleared";
			this.lblScannedCleared.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 14, true);
			this.lblScannedCleared.TabIndex = 13;
			this.lblScannedCleared.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblScannedCount
			// 
			this.lblScannedCount.AutoSize = true;
			this.lblScannedCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("294EED8E-CF9F-4CFE-A8CF-F8127D77D6C0", "Destination Scanned");
			this.lblScannedCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblScannedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.lblScannedCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblScannedCount.Name = "lblScannedCount";
			this.lblScannedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 14, true);
			this.lblScannedCount.TabIndex = 11;
			this.lblScannedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblSurplusCount
			// 
			this.lblSurplusCount.AutoSize = true;
			this.lblSurplusCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("4A2CCCE5-F7ED-4211-BC61-DBEE7B9F7F51", "Surplus");
			this.lblSurplusCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblSurplusCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 29, true);
			this.lblSurplusCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblSurplusCount.Name = "lblSurplusCount";
			this.lblSurplusCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 14, true);
			this.lblSurplusCount.TabIndex = 9;
			this.lblSurplusCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblImportClearedCount
			// 
			this.lblImportClearedCount.AutoSize = true;
			this.lblImportClearedCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("6119ED74-4F88-4F33-AC8F-FA63820EAE8B", "Import Cleared");
			this.lblImportClearedCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblImportClearedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 29, true);
			this.lblImportClearedCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblImportClearedCount.Name = "lblImportClearedCount";
			this.lblImportClearedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.lblImportClearedCount.TabIndex = 3;
			this.lblImportClearedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblImportHeldCount
			// 
			this.lblImportHeldCount.AutoSize = true;
			this.lblImportHeldCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("D826B1C7-D7E6-4DEA-A50B-700FA86B1FA5", "Import Held");
			this.lblImportHeldCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblImportHeldCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 29, true);
			this.lblImportHeldCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblImportHeldCount.Name = "lblImportHeldCount";
			this.lblImportHeldCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 14, true);
			this.lblImportHeldCount.TabIndex = 5;
			this.lblImportHeldCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblImportNoneReportedCount
			// 
			this.lblImportNoneReportedCount.AutoSize = true;
			this.lblImportNoneReportedCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("3A036960-156D-4813-84C1-E95FF92F63EC", "Import None Reported");
			this.lblImportNoneReportedCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblImportNoneReportedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 29, true);
			this.lblImportNoneReportedCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblImportNoneReportedCount.Name = "lblImportNoneReportedCount";
			this.lblImportNoneReportedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 14, true);
			this.lblImportNoneReportedCount.TabIndex = 7;
			this.lblImportNoneReportedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblExportClearedCount
			// 
			this.lblExportClearedCount.AutoSize = true;
			this.lblExportClearedCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("a88b0c08-307e-452f-aa71-dc16d755a0bd", "Export Cleared");
			this.lblExportClearedCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblExportClearedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 29, true);
			this.lblExportClearedCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblExportClearedCount.Name = "lblExportClearedCount";
			this.lblExportClearedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.lblExportClearedCount.TabIndex = 3;
			this.lblExportClearedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblExportHeldCount
			// 
			this.lblExportHeldCount.AutoSize = true;
			this.lblExportHeldCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("d600de5e-756d-469c-a9d6-b0459857fb9c", "Export Held");
			this.lblExportHeldCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblExportHeldCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 29, true);
			this.lblExportHeldCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblExportHeldCount.Name = "lblExportHeldCount";
			this.lblExportHeldCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 14, true);
			this.lblExportHeldCount.TabIndex = 5;
			this.lblExportHeldCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblExportNoneReportedCount
			// 
			this.lblExportNoneReportedCount.AutoSize = true;
			this.lblExportNoneReportedCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("9a59eae7-2f3d-4f76-9de0-44a4d758d26d", "Export None Reported");
			this.lblExportNoneReportedCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblExportNoneReportedCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 29, true);
			this.lblExportNoneReportedCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblExportNoneReportedCount.Name = "lblExportNoneReportedCount";
			this.lblExportNoneReportedCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 14, true);
			this.lblExportNoneReportedCount.TabIndex = 7;
			this.lblExportNoneReportedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblItemCount
			// 
			this.lblItemCount.AutoSize = true;
			this.lblItemCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("7574D0DE-04BB-4351-A55F-E268C9FC665D", "Total Items");
			this.lblItemCount.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.lblItemCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 29, true);
			this.lblItemCount.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.lblItemCount.Name = "lblItemCount";
			this.lblItemCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 14, true);
			this.lblItemCount.TabIndex = 1;
			this.lblItemCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// consignmentUserControl
			// 
			this.consignmentUserControl.AllowDrop = true;
			this.consignmentUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.consignmentUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.eTail.Business.IHVLVConsignmentCollectionParent)(((Enterprise.eTail.Business.HVLVConsignmentHeader)(null)))));
			this.consignmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignmentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.consignmentUserControl.Name = "consignmentUserControl";
			this.consignmentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1381, 791, true);
			this.consignmentUserControl.TabIndex = 23;
			this.consignmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignmentUserControl.ShowAttachDetachButton = true;
			// 
			// HVLVShipmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.consignmentUserControl);
			this.Controls.Add(this.groupBoxCountProperties);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "HVLVShipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1381, 916, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxCountProperties.ResumeLayout(false);
			this.groupBoxCountProperties.PerformLayout();
			this.consignmentUserControl.ResumeLayout(true);
			this.consignmentUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxCountProperties;
		private Enterprise.eTail.GUI.HVLVConsignmentUserControl consignmentUserControl;
		private ZArchitecture.ZLabel lblItemCount;
		private ZArchitecture.ZLabel lblImportNoneReportedCount;
		private ZArchitecture.ZLabel lblImportHeldCount;
		private ZArchitecture.ZLabel lblImportClearedCount;
		private ZArchitecture.ZLabel lblExportNoneReportedCount;
		private ZArchitecture.ZLabel lblExportHeldCount;
		private ZArchitecture.ZLabel lblExportClearedCount;
		private ZArchitecture.ZLabel lblSurplusCount;
		private ZArchitecture.ZLabel lblShortCount;
		private ZArchitecture.ZLabel lblDeliveredCount;
		private ZArchitecture.ZLabel lblScannedCount;
		private ZArchitecture.ZLabel lblScannedCleared;
		private ZArchitecture.ZLabel lblScannedHeld;
		private ZArchitecture.ZLabel lblScannedNoneReported;
		private ToggleRadioButton rbItemCount;
		private ToggleRadioButton rbImportClearedCount;
		private ToggleRadioButton rbImportHeldCount;
		private ToggleRadioButton rbImportNoneReportedCount;
		private ToggleRadioButton rbExportClearedCount;
		private ToggleRadioButton rbExportHeldCount;
		private ToggleRadioButton rbExportNoneReportedCount;
		private ToggleRadioButton rbSurplusCount;
		private ToggleRadioButton rbShortCount;
		private ToggleRadioButton rbDeliveredCount;
		private ToggleRadioButton rbScannedCount;
		private ToggleRadioButton rbScannedCleared;
		private ToggleRadioButton rbScannedHeld;
		private ToggleRadioButton rbScannedNoneReported;
	}
}
