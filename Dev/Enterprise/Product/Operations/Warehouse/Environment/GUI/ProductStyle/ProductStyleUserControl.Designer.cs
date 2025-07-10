using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class ProductStyleUserControl
	{
		internal ZArchitecture.ZGrid ProductStyleSizesGrid;
		ZGroupBox ProductStyleColoursGroupBox;
		ZGroupBox ProductStyleClassificationsGroupBox;
		ZArchitecture.ZTextBox WST_CodeTextBox;
		ZArchitecture.ZTextBox WST_DescriptionTextBox;
		ZGroupBox ProductStyleSizesGroupBox;
		ZGroupBox ProductStyleGroupBox;
		CargoWise.Windows.UI.KSplitContainer ColoursAndSizesSplitContainer;
		CargoWise.Windows.UI.KSplitContainer ColoursAndClassificationsSplitContainer;
		ZPanel RunSheetButtonsPanel;
		internal ZButton MoveDownButton;
		internal ZButton MoveUpButton;
		ZGuidFindBox OwnerFindBox;
		ZArchitecture.ZGrid ProductStyleColoursGrid;
		ZArchitecture.ZGrid ProductStyleClassificationsGrid;

		void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProductStyleSizesGrid = new ZArchitecture.ZGrid();
			this.ProductStyleColoursGrid = new ZArchitecture.ZGrid();
			this.ProductStyleColoursGroupBox = new ZGroupBox();
			this.ProductStyleClassificationsGrid = new ZArchitecture.ZGrid();
			this.ProductStyleClassificationsGroupBox = new ZGroupBox();
			this.WST_CodeTextBox = new ZArchitecture.ZTextBox();
			this.WST_DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.ProductStyleSizesGroupBox = new ZGroupBox();
			this.RunSheetButtonsPanel = new ZPanel();
			this.MoveDownButton = new ZButton();
			this.MoveUpButton = new ZButton();
			this.ProductStyleGroupBox = new ZGroupBox();
			this.ColoursAndSizesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ColoursAndClassificationsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OwnerFindBox = new ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleSizesGrid)).BeginInit();
			this.ProductStyleSizesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleColoursGrid)).BeginInit();
			this.ProductStyleColoursGrid.SuspendLayout();
			this.ProductStyleColoursGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleClassificationsGrid)).BeginInit();
			this.ProductStyleClassificationsGrid.SuspendLayout();
			this.ProductStyleClassificationsGroupBox.SuspendLayout();
			this.ProductStyleSizesGroupBox.SuspendLayout();
			this.RunSheetButtonsPanel.SuspendLayout();
			this.ProductStyleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColoursAndSizesSplitContainer)).BeginInit();
			this.ColoursAndSizesSplitContainer.Panel1.SuspendLayout();
			this.ColoursAndSizesSplitContainer.Panel2.SuspendLayout();
			this.ColoursAndSizesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColoursAndClassificationsSplitContainer)).BeginInit();
			this.ColoursAndClassificationsSplitContainer.Panel1.SuspendLayout();
			this.ColoursAndClassificationsSplitContainer.Panel2.SuspendLayout();
			this.ColoursAndClassificationsSplitContainer.SuspendLayout();
			this.OwnerFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsProductStyle);
			// 
			// ProductStyleSizesGrid
			// 
			this.ProductStyleSizesGrid.AllowNavigation = false;
			this.ProductStyleSizesGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.ProductStyleSizesGrid, "Sizes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsProductStyle)(null)).Sizes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductStyleSize)(((System.Collections.IList)(((WhsProductStyle)(null)).Sizes)).SyncRoot)).WSZ_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyleSize)(((System.Collections.IList)(((WhsProductStyle)(null)).Sizes)).SyncRoot)).WSZ_Size)));
			this.ProductStyleSizesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WSZ_Sequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WSZ_Size";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductStyleSizesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProductStyleSizesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProductStyleSizesGrid.CopySelectedRowsAllowed = true;
			this.ProductStyleSizesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductStyleSizesGrid.GridId = "c9cff5e7-a810-4fd9-951a-160711bb32f3";
			this.ProductStyleSizesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductStyleSizesGrid.LayoutKey = "ProductStyleSizes";
			this.ProductStyleSizesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.ProductStyleSizesGrid.Name = "ProductStyleSizesGrid";
			this.ProductStyleSizesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 482, true);
			this.ProductStyleSizesGrid.TabIndex = 4;
			// 
			// ProductStyleColoursGrid
			// 
			this.ProductStyleColoursGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductStyleColoursGrid, "Colours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsProductStyle)(null)).Colours)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyleColour)(((System.Collections.IList)(((WhsProductStyle)(null)).Colours)).SyncRoot)).WSC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyleColour)(((System.Collections.IList)(((WhsProductStyle)(null)).Colours)).SyncRoot)).WSC_Description)));
			this.ProductStyleColoursGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WSC_Code";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "WSC_Description";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.ProductStyleColoursGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProductStyleColoursGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProductStyleColoursGrid.CopySelectedRowsAllowed = true;
			this.ProductStyleColoursGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductStyleColoursGrid.GridId = "c9cff5e7-a810-4fd9-951a-160711bb32f3";
			this.ProductStyleColoursGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductStyleColoursGrid.LayoutKey = "ProductStyleColours";
			this.ProductStyleColoursGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.ProductStyleColoursGrid.Name = "ProductStyleColoursGrid";
			this.ProductStyleColoursGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 482, true);
			this.ProductStyleColoursGrid.TabIndex = 3;
			// 
			// ProductStyleClassificationsGrid
			// 
			this.ProductStyleClassificationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductStyleClassificationsGrid, "Classifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsProductStyle)(null)).Classifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyleClassification)(((System.Collections.IList)(((WhsProductStyle)(null)).Classifications)).SyncRoot)).WSS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyleClassification)(((System.Collections.IList)(((WhsProductStyle)(null)).Classifications)).SyncRoot)).WSS_Description)));
			this.ProductStyleClassificationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "WSS_Code";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "WSS_Description";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.ProductStyleClassificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProductStyleClassificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProductStyleClassificationsGrid.CopySelectedRowsAllowed = true;
			this.ProductStyleClassificationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductStyleClassificationsGrid.GridId = "4b300e56-11ad-4615-a9d2-e8f42d9208c2";
			this.ProductStyleClassificationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductStyleClassificationsGrid.LayoutKey = "ProductStyleClassifications";
			this.ProductStyleClassificationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.ProductStyleClassificationsGrid.Name = "ProductStyleClassificationsGrid";
			this.ProductStyleClassificationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 482, true);
			this.ProductStyleClassificationsGrid.TabIndex = 0;
			// 
			// ProductStyleColoursGroupBox
			// 
			this.ProductStyleColoursGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("98d3cfb2-e4a2-4e80-9826-e983394a93d3", "Colors");
			this.ProductStyleColoursGroupBox.Controls.Add(this.ProductStyleColoursGrid);
			this.ProductStyleColoursGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProductStyleColoursGroupBox, false);
			this.ProductStyleColoursGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductStyleColoursGroupBox.Name = "ProductStyleColoursGroupBox";
			this.ProductStyleColoursGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductStyleColoursGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 501, true);
			this.ProductStyleColoursGroupBox.TabIndex = 3;
			this.ProductStyleColoursGroupBox.TabStop = false;
			// 
			// ProductStyleClassificationsGroupBox
			// 
			this.ProductStyleClassificationsGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("bda89108-bdcc-4b57-99aa-0f80bf20d6ce", "Classifications");
			this.ProductStyleClassificationsGroupBox.Controls.Add(this.ProductStyleClassificationsGrid);
			this.ProductStyleClassificationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProductStyleClassificationsGroupBox, false);
			this.ProductStyleClassificationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductStyleClassificationsGroupBox.Name = "ProductStyleClassificationsGroupBox";
			this.ProductStyleClassificationsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductStyleClassificationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 501, true);
			this.ProductStyleClassificationsGroupBox.TabIndex = 4;
			this.ProductStyleClassificationsGroupBox.TabStop = false;
			// 
			// WST_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.WST_CodeTextBox, "WST_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyle)(null)).WST_Code)));
			this.WST_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 17, true);
			this.WST_CodeTextBox.Name = "WST_CodeTextBox";
			this.WST_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 18, true);
			this.WST_CodeTextBox.TabIndex = 1;
			// 
			// WST_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.WST_DescriptionTextBox, "WST_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductStyle)(null)).WST_Description)));
			this.WST_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.WST_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 39, true);
			this.WST_DescriptionTextBox.Name = "WST_DescriptionTextBox";
			this.WST_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 18, true);
			this.WST_DescriptionTextBox.TabIndex = 2;
			// 
			// ProductStyleSizesGroupBox
			// 
			this.ProductStyleSizesGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("ad1bc27e-6bab-4c73-b9fc-d6ce06ae7649", "Sizes");
			this.ProductStyleSizesGroupBox.Controls.Add(this.ProductStyleSizesGrid);
			this.ProductStyleSizesGroupBox.Controls.Add(this.RunSheetButtonsPanel);
			this.ProductStyleSizesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProductStyleSizesGroupBox, false);
			this.ProductStyleSizesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductStyleSizesGroupBox.Name = "ProductStyleSizesGroupBox";
			this.ProductStyleSizesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductStyleSizesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 501, true);
			this.ProductStyleSizesGroupBox.TabIndex = 5;
			this.ProductStyleSizesGroupBox.TabStop = false;
			// 
			// RunSheetButtonsPanel
			// 
			this.RunSheetButtonsPanel.Controls.Add(this.MoveDownButton);
			this.RunSheetButtonsPanel.Controls.Add(this.MoveUpButton);
			this.RunSheetButtonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.RunSheetButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 15, true);
			this.RunSheetButtonsPanel.Name = "RunSheetButtonsPanel";
			this.RunSheetButtonsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.RunSheetButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 482, true);
			this.RunSheetButtonsPanel.TabIndex = 5;
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MoveDownButton.Font = new System.Drawing.Font("Arial", 9F);
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 21, true);
			this.MoveDownButton.TabIndex = 3;
			this.MoveDownButton.Text = "▼";
			this.MoveDownButton.UseVisualStyleBackColor = true;
			this.MoveDownButton.Click += new EventHandler(this.MoveDownButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MoveUpButton.Font = new System.Drawing.Font("Arial", 9F);
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 21, true);
			this.MoveUpButton.TabIndex = 2;
			this.MoveUpButton.Text = "▲";
			this.MoveUpButton.UseVisualStyleBackColor = true;
			this.MoveUpButton.Click += new EventHandler(this.MoveUpButton_Click);
			// 
			// ProductStyleGroupBox
			// 
			this.ProductStyleGroupBox.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("8fa1745c-28c7-4bef-b86f-4a035d5d2ca0", "Product Style");
			this.ProductStyleGroupBox.Controls.Add(this.OwnerFindBox);
			this.ProductStyleGroupBox.Controls.Add(this.WST_CodeTextBox);
			this.ProductStyleGroupBox.Controls.Add(this.WST_DescriptionTextBox);
			this.ProductStyleGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProductStyleGroupBox, false);
			this.ProductStyleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProductStyleGroupBox.Name = "ProductStyleGroupBox";
			this.ProductStyleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 85, true);
			this.ProductStyleGroupBox.TabIndex = 6;
			this.ProductStyleGroupBox.TabStop = false;
			// 
			// ColoursAndSizesSplitContainer
			// 
			this.ColoursAndSizesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColoursAndSizesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 108);
			this.ColoursAndSizesSplitContainer.Name = "ColoursAndSizesSplitContainer";
			// 
			// ColoursAndSizesSplitContainer.Panel1
			// 
			this.ColoursAndSizesSplitContainer.Panel1.Controls.Add(this.ColoursAndClassificationsSplitContainer);
			this.ColoursAndSizesSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(381);
			// 
			// ColoursAndSizesSplitContainer.Panel2
			// 
			this.ColoursAndSizesSplitContainer.Panel2.Controls.Add(this.ProductStyleSizesGroupBox);
			this.ColoursAndSizesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			this.ColoursAndSizesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 589);
			this.ColoursAndSizesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(381);
			this.ColoursAndSizesSplitContainer.TabIndex = 7;
			// 
			// ColoursAndClassificationSplitContainer
			// 
			this.ColoursAndClassificationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColoursAndClassificationsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2);
			this.ColoursAndClassificationsSplitContainer.Name = "ColoursAndClassificationsSplitContainer";
			// 
			// ColoursAndClassificationSplitContainer.Panel1
			// 
			this.ColoursAndClassificationsSplitContainer.Panel1.Controls.Add(this.ProductStyleColoursGroupBox);
			this.ColoursAndClassificationsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(188);
			// 
			// ColoursAndClassificationSplitContainer.Panel2
			// 
			this.ColoursAndClassificationsSplitContainer.Panel2.Controls.Add(this.ProductStyleClassificationsGroupBox);
			this.ColoursAndClassificationsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(188);
			this.ColoursAndClassificationsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 589);
			this.ColoursAndClassificationsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			this.ColoursAndClassificationsSplitContainer.TabIndex = 0;
			// 
			// OwnerFindBox
			// 
			this.OwnerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerFindBox, "WST_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductStyle)(null)).WST_OH_Owner)));
			this.OwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 60, true);
			this.OwnerFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OwnerFindBox.Name = "OwnerFindBox";
			this.OwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 18, true);
			this.OwnerFindBox.TabIndex = 3;
			// 
			// ProductStyleUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ColoursAndSizesSplitContainer);
			this.Controls.Add(this.ProductStyleGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 320, true);
			this.Name = "ProductStyleUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleSizesGrid)).EndInit();
			this.ProductStyleSizesGrid.ResumeLayout(false);
			this.ProductStyleSizesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleColoursGrid)).EndInit();
			this.ProductStyleColoursGrid.ResumeLayout(false);
			this.ProductStyleColoursGrid.PerformLayout();
			this.ProductStyleColoursGroupBox.ResumeLayout(false);
			this.ProductStyleColoursGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductStyleClassificationsGrid)).EndInit();
			this.ProductStyleClassificationsGrid.ResumeLayout(false);
			this.ProductStyleClassificationsGrid.PerformLayout();
			this.ProductStyleClassificationsGroupBox.ResumeLayout(false);
			this.ProductStyleClassificationsGroupBox.PerformLayout();
			this.ProductStyleSizesGroupBox.ResumeLayout(false);
			this.ProductStyleSizesGroupBox.PerformLayout();
			this.RunSheetButtonsPanel.ResumeLayout(false);
			this.RunSheetButtonsPanel.PerformLayout();
			this.ProductStyleGroupBox.ResumeLayout(false);
			this.ProductStyleGroupBox.PerformLayout();
			this.ColoursAndClassificationsSplitContainer.Panel1.ResumeLayout(false);
			this.ColoursAndClassificationsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ColoursAndClassificationsSplitContainer)).EndInit();
			this.ColoursAndClassificationsSplitContainer.ResumeLayout(false);
			this.ColoursAndSizesSplitContainer.Panel1.ResumeLayout(false);
			this.ColoursAndSizesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ColoursAndSizesSplitContainer)).EndInit();
			this.ColoursAndSizesSplitContainer.ResumeLayout(false);
			this.OwnerFindBox.ResumeLayout(true);
			this.OwnerFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
