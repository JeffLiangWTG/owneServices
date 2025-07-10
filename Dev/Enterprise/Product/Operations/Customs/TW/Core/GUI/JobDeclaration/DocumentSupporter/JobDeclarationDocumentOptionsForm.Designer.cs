namespace Enterprise.Customs.TW.GUI
{
	partial class JobDeclarationDocumentOptionsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DocumentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HideEXPBuyerEnglishAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideEXPBuyerTradChineseAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideEXPExporterEnglishAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideEXPExporterTradChineseAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GoodsDescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CEI_StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ImportAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HideIMPSellerTradChineseAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideIMPImporterEnglishAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideIMPImporterTradChineseAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CellSettingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportAddressGroupBox.SuspendLayout();
			this.GoodsDescriptionGroupBox.SuspendLayout();
			this.CEI_StyleDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GoodsDescriptionGrid)).BeginInit();
			this.GoodsDescriptionGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.ImportAddressGroupBox.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.CellSettingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 587, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig);
			// 
			// DocumentTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocumentTextBox, "DocumentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).DocumentName)));
			this.DocumentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DocumentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 12, true);
			this.DocumentTextBox.Name = "DocumentTextBox";
			this.DocumentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 21, true);
			this.DocumentTextBox.TabIndex = 0;
			// 
			// ExportAddressGroupBox
			// 
			this.ExportAddressGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d4e8b756-0c51-4813-8c7b-e153d4a54fd7", "Address");
			this.ExportAddressGroupBox.Controls.Add(this.HideEXPBuyerEnglishAddrCheckBox);
			this.ExportAddressGroupBox.Controls.Add(this.HideEXPBuyerTradChineseAddrCheckBox);
			this.ExportAddressGroupBox.Controls.Add(this.HideEXPExporterEnglishAddrCheckBox);
			this.ExportAddressGroupBox.Controls.Add(this.HideEXPExporterTradChineseAddrCheckBox);
			this.ExportAddressGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ExportAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.ExportAddressGroupBox.Name = "ExportAddressGroupBox";
			this.ExportAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 143, true);
			this.ExportAddressGroupBox.TabIndex = 1;
			this.ExportAddressGroupBox.TabStop = false;
			// 
			// HideEXPBuyerEnglishAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideEXPBuyerEnglishAddrCheckBox, "HideEXPBuyerEnglishAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideEXPBuyerEnglishAddr)));
			this.HideEXPBuyerEnglishAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 109, true);
			this.HideEXPBuyerEnglishAddrCheckBox.Name = "HideEXPBuyerEnglishAddrCheckBox";
			this.HideEXPBuyerEnglishAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideEXPBuyerEnglishAddrCheckBox.TabIndex = 3;
			this.HideEXPBuyerEnglishAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideEXPBuyerTradChineseAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideEXPBuyerTradChineseAddrCheckBox, "HideEXPBuyerTradChineseAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideEXPBuyerTradChineseAddr)));
			this.HideEXPBuyerTradChineseAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 79, true);
			this.HideEXPBuyerTradChineseAddrCheckBox.Name = "HideEXPBuyerTradChineseAddrCheckBox";
			this.HideEXPBuyerTradChineseAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideEXPBuyerTradChineseAddrCheckBox.TabIndex = 2;
			this.HideEXPBuyerTradChineseAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideEXPExporterEnglishAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideEXPExporterEnglishAddrCheckBox, "HideEXPExporterEnglishAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideEXPExporterEnglishAddr)));
			this.HideEXPExporterEnglishAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 49, true);
			this.HideEXPExporterEnglishAddrCheckBox.Name = "HideEXPExporterEnglishAddrCheckBox";
			this.HideEXPExporterEnglishAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideEXPExporterEnglishAddrCheckBox.TabIndex = 1;
			this.HideEXPExporterEnglishAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideEXPExporterTradChineseAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideEXPExporterTradChineseAddrCheckBox, "HideEXPExporterTradChineseAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideEXPExporterTradChineseAddr)));
			this.HideEXPExporterTradChineseAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.HideEXPExporterTradChineseAddrCheckBox.Name = "HideEXPExporterTradChineseAddrCheckBox";
			this.HideEXPExporterTradChineseAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideEXPExporterTradChineseAddrCheckBox.TabIndex = 0;
			this.HideEXPExporterTradChineseAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// GoodsDescriptionGroupBox
			// 
			this.GoodsDescriptionGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f09ce1ee-8ad2-4308-8c82-2e43a950bab0", "Goods Description");
			this.GoodsDescriptionGroupBox.Controls.Add(this.CellSettingGroupBox);
			this.GoodsDescriptionGroupBox.Controls.Add(this.GoodsDescriptionGrid);
			this.GoodsDescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsDescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 301, true);
			this.GoodsDescriptionGroupBox.Name = "GoodsDescriptionGroupBox";
			this.GoodsDescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 255, true);
			this.GoodsDescriptionGroupBox.TabIndex = 3;
			this.GoodsDescriptionGroupBox.TabStop = false;
			// 
			// CEI_StyleDropEdit
			// 
			this.CEI_StyleDropEdit.AllowDrop = true;
			this.CEI_StyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CEI_StyleDropEdit, "CustomizeSectionBodyRow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).CustomizeSectionBodyRow)));
			this.CEI_StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 17, true);
			this.CEI_StyleDropEdit.Name = "CEI_StyleDropEdit";
			this.CEI_StyleDropEdit.PreBoundMaxLength = 2;
			this.CEI_StyleDropEdit.ShouldResizeByMaxLength = false;
			this.CEI_StyleDropEdit.ShowDescriptionBox = false;
			this.CEI_StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 21, true);
			this.CEI_StyleDropEdit.TabIndex = 6;
			// 
			// GoodsDescriptionGrid
			// 
			this.GoodsDescriptionGrid.AllowNavigation = false;
			this.GoodsDescriptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GoodsDescriptionGrid.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionGrid, "GoodsDescriptionConfigs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).GoodsDescriptionConfigs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentGoodsDescriptionConfig)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).GoodsDescriptionConfigs)).SyncRoot)).Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentGoodsDescriptionConfig)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).GoodsDescriptionConfigs)).SyncRoot)).Caption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentGoodsDescriptionConfig)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).GoodsDescriptionConfigs)).SyncRoot)).Field)));
			this.GoodsDescriptionGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Position";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "Caption";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zDropEditColumnStyleInfo2.ColumnName = "Field";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.GoodsDescriptionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.GoodsDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GoodsDescriptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GoodsDescriptionGrid.GridId = "a42c4974-2b84-43c1-a31b-d326984bf834";
			this.GoodsDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GoodsDescriptionGrid.LayoutKey = "zGrid1";
			this.GoodsDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.GoodsDescriptionGrid.Name = "GoodsDescriptionGrid";
			this.GoodsDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 195, true);
			this.GoodsDescriptionGrid.TabIndex = 0;
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("04c6d333-2704-4ffc-bf67-b5116ad8be15", "Deliver");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DeliverButton, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DeliverButton.TabIndex = 0;
			this.DeliverButton.ToolTipCaption = null;
			this.DeliverButton.UseVisualStyleBackColor = true;
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// CancelButton1
			// 
			this.CancelButton1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b739a67d-0f57-4355-9d90-6c8129af0069", "Cancel");
			this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 3, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelButton1.TabIndex = 1;
			this.CancelButton1.ToolTipCaption = null;
			this.CancelButton1.UseVisualStyleBackColor = true;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.DocumentTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 49, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ImportAddressGroupBox
			// 
			this.ImportAddressGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("697ea385-f432-4012-89cc-a7979cb8d581", "Address");
			this.ImportAddressGroupBox.Controls.Add(this.HideIMPSellerTradChineseAddrCheckBox);
			this.ImportAddressGroupBox.Controls.Add(this.HideIMPImporterEnglishAddrCheckBox);
			this.ImportAddressGroupBox.Controls.Add(this.HideIMPImporterTradChineseAddrCheckBox);
			this.ImportAddressGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ImportAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 191, true);
			this.ImportAddressGroupBox.Name = "ImportAddressGroupBox";
			this.ImportAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 110, true);
			this.ImportAddressGroupBox.TabIndex = 2;
			this.ImportAddressGroupBox.TabStop = false;
			// 
			// HideIMPSellerTradChineseAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideIMPSellerTradChineseAddrCheckBox, "HideIMPSellerTradChineseAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideIMPSellerTradChineseAddr)));
			this.HideIMPSellerTradChineseAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 79, true);
			this.HideIMPSellerTradChineseAddrCheckBox.Name = "HideIMPSellerTradChineseAddrCheckBox";
			this.HideIMPSellerTradChineseAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideIMPSellerTradChineseAddrCheckBox.TabIndex = 2;
			this.HideIMPSellerTradChineseAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideIMPImporterEnglishAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideIMPImporterEnglishAddrCheckBox, "HideIMPImporterEnglishAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideIMPImporterEnglishAddr)));
			this.HideIMPImporterEnglishAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 49, true);
			this.HideIMPImporterEnglishAddrCheckBox.Name = "HideIMPImporterEnglishAddrCheckBox";
			this.HideIMPImporterEnglishAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideIMPImporterEnglishAddrCheckBox.TabIndex = 1;
			this.HideIMPImporterEnglishAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideIMPImporterTradChineseAddrCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HideIMPImporterTradChineseAddrCheckBox, "HideIMPImporterTradChineseAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig)(null)).HideIMPImporterTradChineseAddr)));
			this.HideIMPImporterTradChineseAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.HideIMPImporterTradChineseAddrCheckBox.Name = "HideIMPImporterTradChineseAddrCheckBox";
			this.HideIMPImporterTradChineseAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.HideIMPImporterTradChineseAddrCheckBox.TabIndex = 0;
			this.HideIMPImporterTradChineseAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.DeliverButton);
			this.zPanel2.Controls.Add(this.CancelButton1);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 557, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 30, true);
			this.zPanel2.TabIndex = 4;
			// 
			// CellSettingGroupBox
			// 
			this.CellSettingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CellSettingGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("fd6fe449-98e3-41d7-8747-8f47d1fad33c", "Cell Setting");
			this.CellSettingGroupBox.Controls.Add(this.CEI_StyleDropEdit);
			this.CellSettingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
			this.CellSettingGroupBox.Name = "CellSettingGroupBox";
			this.CellSettingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 41, true);
			this.CellSettingGroupBox.TabIndex = 3;
			this.CellSettingGroupBox.TabStop = false;
			// 
			// JobDeclarationDocumentOptionsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5b353e1c-8c4f-4044-b4da-755b8a9ceed0", "Customs Declaration Options");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 611, true);
			this.Controls.Add(this.GoodsDescriptionGroupBox);
			this.Controls.Add(this.zPanel2);
			this.Controls.Add(this.ImportAddressGroupBox);
			this.Controls.Add(this.ExportAddressGroupBox);
			this.Controls.Add(this.zPanel1);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 648, true);
			this.Name = "JobDeclarationDocumentOptionsForm";
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.ExportAddressGroupBox, 0);
			this.Controls.SetChildIndex(this.ImportAddressGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			this.Controls.SetChildIndex(this.GoodsDescriptionGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportAddressGroupBox.ResumeLayout(false);
			this.ExportAddressGroupBox.PerformLayout();
			this.GoodsDescriptionGroupBox.ResumeLayout(false);
			this.GoodsDescriptionGroupBox.PerformLayout();
			this.CEI_StyleDropEdit.ResumeLayout(true);
			this.CEI_StyleDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GoodsDescriptionGrid)).EndInit();
			this.GoodsDescriptionGrid.ResumeLayout(false);
			this.GoodsDescriptionGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ImportAddressGroupBox.ResumeLayout(false);
			this.ImportAddressGroupBox.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.CellSettingGroupBox.ResumeLayout(false);
			this.CellSettingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox DocumentTextBox;
		private ZArchitecture.GUI.ZGroupBox ExportAddressGroupBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPBuyerEnglishAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPBuyerTradChineseAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPExporterEnglishAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPExporterTradChineseAddrCheckBox;
		private ZArchitecture.GUI.ZGroupBox GoodsDescriptionGroupBox;
		private ZArchitecture.ZGrid GoodsDescriptionGrid;
		private ZArchitecture.GUI.ZButton DeliverButton;
		private ZArchitecture.GUI.ZButton CancelButton1;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZGroupBox ImportAddressGroupBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPSellerTradChineseAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPImporterEnglishAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPImporterTradChineseAddrCheckBox;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private ZArchitecture.GUI.ZDropEdit CEI_StyleDropEdit;
		private ZArchitecture.GUI.ZGroupBox CellSettingGroupBox;
	}
}
