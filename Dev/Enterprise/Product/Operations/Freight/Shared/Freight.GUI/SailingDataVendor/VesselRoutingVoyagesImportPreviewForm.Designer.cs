using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	partial class VesselRoutingVoyagesImportPreviewForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			VendorDataStatusLabel.AllowOutsideOfParent();
			NoPortPairsAvailableLabel.AllowOverlap(PortPairsGrid);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.VesselNameAndLloydsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.E9_RV_NKVesselBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.E9_LloydsNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.E9_LineOperatorBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.VesselSchedulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SearchResultsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VoyagesSelectNoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GridsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			this.PortPairsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoPortPairsAvailableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ResetPortPairsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveSelectedForeignPortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddForeignPortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortPairsSelectNoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ForeignPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortPairsSelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortPairsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VendorDataStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DataSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselNameAndLloydsPanel.SuspendLayout();
			this.E9_RV_NKVesselBoundFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VoyagesGrid)).BeginInit();
			this.VoyagesGrid.SuspendLayout();
			this.VesselSchedulesGroupBox.SuspendLayout();
			this.GridsPanel.SuspendLayout();
			this.PortPairsGroupBox.SuspendLayout();
			this.ForeignPortCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PortPairsGrid)).BeginInit();
			this.PortPairsGrid.SuspendLayout();
			this.PortCodeFindBox.SuspendLayout();
			this.DateTypeDropEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.DataSourceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 540, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 24, true);
			this.MainStatusBar.TabIndex = 15;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter);
			// 
			// VesselNameAndLloydsPanel
			// 
			this.VesselNameAndLloydsPanel.Controls.Add(this.E9_RV_NKVesselBoundFindBox);
			this.VesselNameAndLloydsPanel.Controls.Add(this.E9_LloydsNumberBoundTextBox);
			this.VesselNameAndLloydsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 62, true);
			this.VesselNameAndLloydsPanel.Name = "VesselNameAndLloydsPanel";
			this.VesselNameAndLloydsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 25, true);
			this.VesselNameAndLloydsPanel.TabIndex = 6;
			// 
			// E9_RV_NKVesselBoundFindBox
			// 
			this.E9_RV_NKVesselBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.E9_RV_NKVesselBoundFindBox, "E9_RV_NKVesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_RV_NKVesselName)));
			this.E9_RV_NKVesselBoundFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|d3defcf7-bc72-4864-9f95-0232600776db", "Vessel/Lloyds");
			this.E9_RV_NKVesselBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 0, true);
			this.E9_RV_NKVesselBoundFindBox.Name = "E9_RV_NKVesselBoundFindBox";
			this.E9_RV_NKVesselBoundFindBox.PreBoundMaxLength = 35;
			this.E9_RV_NKVesselBoundFindBox.ShowDescriptionBox = false;
			this.E9_RV_NKVesselBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 19, true);
			this.E9_RV_NKVesselBoundFindBox.TabIndex = 0;
			// 
			// E9_LloydsNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.E9_LloydsNumberBoundTextBox, "E9_LloydsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_LloydsNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.E9_LloydsNumberBoundTextBox, false);
			this.E9_LloydsNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 1, true);
			this.E9_LloydsNumberBoundTextBox.Name = "E9_LloydsNumberBoundTextBox";
			this.E9_LloydsNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 19, true);
			this.E9_LloydsNumberBoundTextBox.TabIndex = 1;
			// 
			// E9_LineOperatorBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.E9_LineOperatorBoundTextBox, "E9_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_Carrier)));
			this.E9_LineOperatorBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|50f7600b-face-4224-9753-a85bb6eed49d", "Carrier");
			this.E9_LineOperatorBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 88, true);
			this.E9_LineOperatorBoundTextBox.Name = "E9_LineOperatorBoundTextBox";
			this.E9_LineOperatorBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.E9_LineOperatorBoundTextBox.TabIndex = 8;
			// 
			// VoyagesGrid
			// 
			this.VoyagesGrid.AllowNavigation = false;
			this.VoyagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.VoyagesGrid, "Voyages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_LloydsNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_Voyage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_LineOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_OperatorsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_OH_LineOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_DataProvider)));
			this.VoyagesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|6daf587e-f926-4002-9e3b-f935863a5b7e", "Selected");
			zCheckBoxColumnStyleInfo5.ColumnName = "E8_IsSelected";
			zCheckBoxColumnStyleInfo5.IsMandatory = true;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.ColumnName = "E8_VesselName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.ColumnName = "E8_LloydsNumber";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "E8_Voyage";
			zTextBoxColumnStyleInfo11.IsMandatory = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("6c2676a6-6a58-402d-a485-1bdd2a287e6d", "Code", "Operator Code", "");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "E8_LineOperator";
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|7bd14bc3-4ee0-4f16-aaf1-b6a1e4896349", "Carrier");
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("de07487b-22f6-4f1c-bcfd-634778e9a4e9", "Description", "Operator Description", "");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "E8_OperatorsDescription";
			zTextBoxColumnStyleInfo13.GroupName = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|7bd14bc3-4ee0-4f16-aaf1-b6a1e4896349", "Carrier");
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|69162e63-242a-4447-b37e-884e755dff74", "Carrier", "Carrier", "");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "E8_OH_LineOperator";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|7bd14bc3-4ee0-4f16-aaf1-b6a1e4896349", "Carrier");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("a9109ecf-eea6-4a9e-879b-2e7366c38cb5", "Data Source");
			zTextBoxColumnStyleInfo14.ColumnName = "E8_DataProvider";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.VoyagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.VoyagesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.VoyagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.VoyagesGrid.CopySelectedRowsAllowed = true;
			this.VoyagesGrid.GridId = "97105359-8856-4810-85c9-56df5502556e";
			this.VoyagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VoyagesGrid.LayoutKey = "VoyagesGrid";
			this.VoyagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.VoyagesGrid.Name = "VoyagesGrid";
			this.VoyagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 132, true);
			this.VoyagesGrid.TabIndex = 0;
			// 
			// VesselSchedulesGroupBox
			// 
			this.VesselSchedulesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|d093e779-59d6-4bf0-8c2c-eb95cab09a6e", "Vessel Schedules");
			this.VesselSchedulesGroupBox.Controls.Add(this.SearchResultsLabel);
			this.VesselSchedulesGroupBox.Controls.Add(this.VoyagesGrid);
			this.VesselSchedulesGroupBox.Controls.Add(this.VoyagesSelectNoneButton);
			this.VesselSchedulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VesselSchedulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselSchedulesGroupBox.Name = "VesselSchedulesGroupBox";
			this.VesselSchedulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 181, true);
			this.VesselSchedulesGroupBox.TabIndex = 0;
			this.VesselSchedulesGroupBox.TabStop = false;
			// 
			// SearchResultsLabel
			// 
			this.SearchResultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SearchResultsLabel.AutoSize = true;
			this.SearchResultsLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SearchResultsLabel, false);
			this.SearchResultsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 154, true);
			this.SearchResultsLabel.Name = "SearchResultsLabel";
			this.SearchResultsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 14, true);
			this.SearchResultsLabel.TabIndex = 2;
			this.SearchResultsLabel.Text = "SearchResultsLabel";
			// 
			// VoyagesSelectNoneButton
			// 
			this.VoyagesSelectNoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.VoyagesSelectNoneButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|a69ad38f-3a75-4927-8bf4-2bd71c05675b", "Select None");
			this.VoyagesSelectNoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 152, true);
			this.VoyagesSelectNoneButton.Name = "VoyagesSelectNoneButton";
			this.VoyagesSelectNoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.VoyagesSelectNoneButton.TabIndex = 1;
			this.VoyagesSelectNoneButton.UseVisualStyleBackColor = true;
			this.VoyagesSelectNoneButton.Click += new System.EventHandler(this.OnVoyagesSelectNone_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|fc8eb689-980d-4bf5-ac5a-9e4762a0f69a", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(692, 511, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 14;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnClose_Click);
			// 
			// GridsPanel
			// 
			this.GridsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GridsPanel.Controls.Add(this.VesselSchedulesGroupBox);
			this.GridsPanel.Controls.Add(this.Splitter);
			this.GridsPanel.Controls.Add(this.PortPairsGroupBox);
			this.GridsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 115, true);
			this.GridsPanel.Name = "GridsPanel";
			this.GridsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 390, true);
			this.GridsPanel.TabIndex = 9;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.Splitter.MinExtra = 80;
			this.Splitter.MinSize = 80;
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 4, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			// 
			// PortPairsGroupBox
			// 
			this.PortPairsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|5916bd2b-785a-4a2d-9875-9454f340d75f", "Port Pairs");
			this.PortPairsGroupBox.Controls.Add(this.NoPortPairsAvailableLabel);
			this.PortPairsGroupBox.Controls.Add(this.ResetPortPairsButton);
			this.PortPairsGroupBox.Controls.Add(this.RemoveSelectedForeignPortButton);
			this.PortPairsGroupBox.Controls.Add(this.AddForeignPortButton);
			this.PortPairsGroupBox.Controls.Add(this.PortPairsSelectNoneButton);
			this.PortPairsGroupBox.Controls.Add(this.ForeignPortCodeFindBox);
			this.PortPairsGroupBox.Controls.Add(this.PortPairsSelectAllButton);
			this.PortPairsGroupBox.Controls.Add(this.PortPairsGrid);
			this.PortPairsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PortPairsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 185, true);
			this.PortPairsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 205, true);
			this.PortPairsGroupBox.Name = "PortPairsGroupBox";
			this.PortPairsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 205, true);
			this.PortPairsGroupBox.TabIndex = 1;
			this.PortPairsGroupBox.TabStop = false;
			// 
			// NoPortPairsAvailableLabel
			// 
			this.NoPortPairsAvailableLabel.AutoSize = true;
			this.NoPortPairsAvailableLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|09acc64d-a2a1-408a-82c1-059b325ee6d2", "No port pairs available for the current port pair type");
			this.NoPortPairsAvailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 81, true);
			this.NoPortPairsAvailableLabel.Name = "NoPortPairsAvailableLabel";
			this.NoPortPairsAvailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 14, true);
			this.NoPortPairsAvailableLabel.TabIndex = 1;
			// 
			// ResetPortPairsButton
			// 
			this.ResetPortPairsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResetPortPairsButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|6ed3c3d9-ba6f-4933-9662-e316aecd125b", "Reset");
			this.ResetPortPairsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 176, true);
			this.ResetPortPairsButton.Name = "ResetPortPairsButton";
			this.ResetPortPairsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.ResetPortPairsButton.TabIndex = 7;
			this.ResetPortPairsButton.UseVisualStyleBackColor = true;
			this.ResetPortPairsButton.Click += new System.EventHandler(this.OnResetPortPairs_Click);
			// 
			// RemoveSelectedForeignPortButton
			// 
			this.RemoveSelectedForeignPortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveSelectedForeignPortButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|88f72886-248c-4195-85d6-27e3b90f3699", "Remove Selected");
			this.RemoveSelectedForeignPortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 176, true);
			this.RemoveSelectedForeignPortButton.Name = "RemoveSelectedForeignPortButton";
			this.RemoveSelectedForeignPortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.RemoveSelectedForeignPortButton.TabIndex = 6;
			this.RemoveSelectedForeignPortButton.UseVisualStyleBackColor = true;
			this.RemoveSelectedForeignPortButton.Click += new System.EventHandler(this.OnRemoveForeignPort_Click);
			// 
			// AddForeignPortButton
			// 
			this.AddForeignPortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddForeignPortButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|31fe5b44-bbe4-42a6-b530-18029023505e", "Add");
			this.AddForeignPortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 176, true);
			this.AddForeignPortButton.Name = "AddForeignPortButton";
			this.AddForeignPortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.AddForeignPortButton.TabIndex = 5;
			this.AddForeignPortButton.UseVisualStyleBackColor = true;
			this.AddForeignPortButton.Click += new System.EventHandler(this.OnAddForeignPort_Click);
			// 
			// PortPairsSelectNoneButton
			// 
			this.PortPairsSelectNoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PortPairsSelectNoneButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|ce3ed6eb-cdfb-4ca3-a4bf-cdb56c1ecce6", "Select None");
			this.PortPairsSelectNoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 177, true);
			this.PortPairsSelectNoneButton.Name = "PortPairsSelectNoneButton";
			this.PortPairsSelectNoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PortPairsSelectNoneButton.TabIndex = 3;
			this.PortPairsSelectNoneButton.UseVisualStyleBackColor = true;
			this.PortPairsSelectNoneButton.Click += new System.EventHandler(this.OnPortPairsSelectNone_Click);
			// 
			// ForeignPortCodeFindBox
			// 
			this.ForeignPortCodeFindBox.AllowDrop = true;
			this.ForeignPortCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ForeignPortCodeFindBox, "Voyages.E8_ForeignPortToAdd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).E8_ForeignPortToAdd)));
			this.ForeignPortCodeFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|8f91a998-eb04-4519-bb75-ed170f1aec20", "Foreign Port to Add");
			this.ForeignPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 178, true);
			this.ForeignPortCodeFindBox.Name = "ForeignPortCodeFindBox";
			this.ForeignPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 19, true);
			this.ForeignPortCodeFindBox.TabIndex = 4;
			// 
			// PortPairsSelectAllButton
			// 
			this.PortPairsSelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PortPairsSelectAllButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|95882de0-1953-491b-a540-b032c585f66f", "Select All");
			this.PortPairsSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 177, true);
			this.PortPairsSelectAllButton.Name = "PortPairsSelectAllButton";
			this.PortPairsSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PortPairsSelectAllButton.TabIndex = 2;
			this.PortPairsSelectAllButton.UseVisualStyleBackColor = true;
			this.PortPairsSelectAllButton.Click += new System.EventHandler(this.OnPortPairsSelectAll_Click);
			// 
			// PortPairsGrid
			// 
			this.PortPairsGrid.AllowNavigation = false;
			this.PortPairsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PortPairsGrid, "Voyages.PortPairs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_IsRegistered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_Publish)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ATD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ATA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_CargoCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ImportAvailability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ImportStorageCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingPortPair)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyage)(((System.Collections.IList)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).Voyages)).SyncRoot)).PortPairs)).SyncRoot)).E9_ExportReceivalCommences)));
			this.PortPairsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|fe7769bb-9263-4dd3-bb5c-9ac462a780f8", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "E9_IsSelected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|32e11b32-e795-43f2-9153-a2810bdb8cb9", "Registered");
			zCheckBoxColumnStyleInfo6.ColumnName = "E9_IsRegistered";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.ColumnName = "E9_RL_NKLoadPort";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo2.ColumnName = "E9_RL_NKDischargePort";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|10c54819-a544-42c6-9677-2b8a6475ebd1", "Publish");
			zCheckBoxColumnStyleInfo7.ColumnName = "E9_Publish";
			zCheckBoxColumnStyleInfo7.IsMandatory = true;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDateEditColumnStyleInfo9.ColumnName = "E9_ETD";
			zDateEditColumnStyleInfo9.IsMandatory = true;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo10.ColumnName = "E9_ETA";
			zDateEditColumnStyleInfo10.IsMandatory = true;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo11.ColumnName = "E9_ATD";
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo12.ColumnName = "E9_ATA";
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo13.ColumnName = "E9_CargoCutOff";
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo14.ColumnName = "E9_ImportAvailability";
			zDateEditColumnStyleInfo14.IsVisible = false;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo15.ColumnName = "E9_ImportStorageCommences";
			zDateEditColumnStyleInfo15.IsVisible = false;
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo16.ColumnName = "E9_ExportReceivalCommences";
			zDateEditColumnStyleInfo16.IsVisible = false;
			zDateEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.PortPairsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PortPairsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.PortPairsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PortPairsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PortPairsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.PortPairsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.PortPairsGrid.CopySelectedRowsAllowed = true;
			this.PortPairsGrid.GridId = "35459434-b01a-4680-aa92-231cb6dae71b";
			this.PortPairsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PortPairsGrid.LayoutKey = "PortPairsGrid";
			this.PortPairsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PortPairsGrid.Name = "PortPairsGrid";
			this.PortPairsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 157, true);
			this.PortPairsGrid.TabIndex = 0;
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|078b5efd-b616-4a35-951f-5b3a650a3bdc", "&Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 57, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.FindButton.TabIndex = 11;
			this.FindButton.UseVisualStyleBackColor = true;
			// 
			// VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageTextBox, "E9_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_Voyage)));
			this.VoyageTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|902caeb9-9900-40b0-9347-65ecd4b2ff2d", "Voyage");
			this.VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 88, true);
			this.VoyageTextBox.Name = "VoyageTextBox";
			this.VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.VoyageTextBox.TabIndex = 7;
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|a107624b-64fc-4b64-a717-34f3705e1313", "&Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 86, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.ClearButton.TabIndex = 12;
			this.ClearButton.UseVisualStyleBackColor = true;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|4491f795-a9fd-40cf-8917-e5a14e6b18a2", "Import");
			this.ImportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 511, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 13;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.OnImport_Click);
			// 
			// VendorDataStatusLabel
			// 
			this.VendorDataStatusLabel.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VendorDataStatusLabel, false);
			this.VendorDataStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 15, true);
			this.VendorDataStatusLabel.Name = "VendorDataStatusLabel";
			this.VendorDataStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 14, true);
			this.VendorDataStatusLabel.TabIndex = 9;
			this.VendorDataStatusLabel.Text = "<schedule data vendor status>";
			// 
			// PortCodeFindBox
			// 
			this.PortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortCodeFindBox, "E9_Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_Port)));
			this.PortCodeFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|ef7f340e-fe81-49da-9f95-46175a1f3c49", "Port");
			this.PortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 12, true);
			this.PortCodeFindBox.Name = "PortCodeFindBox";
			this.PortCodeFindBox.ShowDescriptionBox = false;
			this.PortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 19, true);
			this.PortCodeFindBox.TabIndex = 0;
			// 
			// DateTypeDropEdit
			// 
			this.DateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateTypeDropEdit, "E9_DateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_DateType)));
			this.DateTypeDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|64f3e9e0-0a46-4e28-a759-384220341be0", "Dates");
			this.DateTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 36, true);
			this.DateTypeDropEdit.Name = "DateTypeDropEdit";
			this.DateTypeDropEdit.PreBoundMaxLength = 20;
			this.DateTypeDropEdit.ShowDescriptionBox = false;
			this.DateTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 19, true);
			this.DateTypeDropEdit.TabIndex = 3;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "E9_DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_DateFrom)));
			this.FromDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|8c323221-cb34-48eb-a1d5-462252db24e5", "From");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 36, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 4;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "E9_DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_DateTo)));
			this.ToDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|8a3d7296-c928-477d-8f21-749ccaa663a5", "To");
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 37, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 5;
			// 
			// DataSourceDropEdit
			// 
			this.DataSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DataSourceDropEdit, "E9_DataProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter)(null)).E9_DataProvider)));
			this.DataSourceDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("728bbfc1-dcf7-4b8a-aa29-2c8add85fb32", "Data Source");
			this.DataSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 12, true);
			this.DataSourceDropEdit.Name = "DataSourceDropEdit";
			this.DataSourceDropEdit.PreBoundMaxLength = 3;
			this.DataSourceDropEdit.ShowDescriptionBox = false;
			this.DataSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 19, true);
			this.DataSourceDropEdit.TabIndex = 2;
			// 
			// VesselRoutingVoyagesImportPreviewForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyagesImportPreviewForm|ad31c341-c20f-45a8-b01f-7c4765c6a1d3", "Sailing Schedule Feed");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 564, true);
			this.Controls.Add(this.DataSourceDropEdit);
			this.Controls.Add(this.ToDateEdit);
			this.Controls.Add(this.FromDateEdit);
			this.Controls.Add(this.DateTypeDropEdit);
			this.Controls.Add(this.PortCodeFindBox);
			this.Controls.Add(this.VendorDataStatusLabel);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.VoyageTextBox);
			this.Controls.Add(this.FindButton);
			this.Controls.Add(this.GridsPanel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.VesselNameAndLloydsPanel);
			this.Controls.Add(this.E9_LineOperatorBoundTextBox);
			this.DataSourceType = typeof(Enterprise.Freight.SailingDataVendor.Business.VesselRoutingVoyagesFilter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 600, true);
			this.Name = "VesselRoutingVoyagesImportPreviewForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.E9_LineOperatorBoundTextBox, 0);
			this.Controls.SetChildIndex(this.VesselNameAndLloydsPanel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.GridsPanel, 0);
			this.Controls.SetChildIndex(this.FindButton, 0);
			this.Controls.SetChildIndex(this.VoyageTextBox, 0);
			this.Controls.SetChildIndex(this.ClearButton, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.VendorDataStatusLabel, 0);
			this.Controls.SetChildIndex(this.PortCodeFindBox, 0);
			this.Controls.SetChildIndex(this.DateTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.DataSourceDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VesselNameAndLloydsPanel.ResumeLayout(false);
			this.VesselNameAndLloydsPanel.PerformLayout();
			this.E9_RV_NKVesselBoundFindBox.ResumeLayout(true);
			this.E9_RV_NKVesselBoundFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.VoyagesGrid)).EndInit();
			this.VoyagesGrid.ResumeLayout(false);
			this.VoyagesGrid.PerformLayout();
			this.VesselSchedulesGroupBox.ResumeLayout(false);
			this.VesselSchedulesGroupBox.PerformLayout();
			this.GridsPanel.ResumeLayout(false);
			this.GridsPanel.PerformLayout();
			this.PortPairsGroupBox.ResumeLayout(false);
			this.PortPairsGroupBox.PerformLayout();
			this.ForeignPortCodeFindBox.ResumeLayout(true);
			this.ForeignPortCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PortPairsGrid)).EndInit();
			this.PortPairsGrid.ResumeLayout(false);
			this.PortPairsGrid.PerformLayout();
			this.PortCodeFindBox.ResumeLayout(true);
			this.PortCodeFindBox.PerformLayout();
			this.DateTypeDropEdit.ResumeLayout(true);
			this.DateTypeDropEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.DataSourceDropEdit.ResumeLayout(true);
			this.DataSourceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private Enterprise.ZArchitecture.GUI.ZPanel VesselNameAndLloydsPanel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox E9_RV_NKVesselBoundFindBox;
		private Enterprise.ZArchitecture.ZTextBox E9_LloydsNumberBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox E9_LineOperatorBoundTextBox;
		protected Enterprise.ZArchitecture.ZGrid VoyagesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox VesselSchedulesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZPanel GridsPanel;
		private CargoWise.Windows.UI.KSplitter Splitter;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
		private Enterprise.ZArchitecture.ZTextBox VoyageTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		protected Enterprise.ZArchitecture.GUI.ZButton VoyagesSelectNoneButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PortPairsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZButton RemoveSelectedForeignPortButton;
		protected Enterprise.ZArchitecture.GUI.ZButton AddForeignPortButton;
		protected Enterprise.ZArchitecture.GUI.ZButton PortPairsSelectNoneButton;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ForeignPortCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZButton PortPairsSelectAllButton;
		protected Enterprise.ZArchitecture.ZGrid PortPairsGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton ResetPortPairsButton;
		protected Enterprise.ZArchitecture.ZLabel NoPortPairsAvailableLabel;
		protected Enterprise.ZArchitecture.ZLabel SearchResultsLabel;
		protected Enterprise.ZArchitecture.ZLabel VendorDataStatusLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PortCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DateTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private ZArchitecture.GUI.ZDropEdit DataSourceDropEdit;
	}
}

