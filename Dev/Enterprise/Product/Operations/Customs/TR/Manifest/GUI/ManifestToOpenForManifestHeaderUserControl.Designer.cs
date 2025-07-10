using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class ManifestToOpenForManifestHeaderUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ManifestsToOpenSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AdditionalInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ManifestsToOpenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpeningStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AtWarehouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReOpenedAgainCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillLineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalBoxQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionBoxQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WarehouseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsToOpenSplitContainer)).BeginInit();
			this.ManifestsToOpenSplitContainer.Panel1.SuspendLayout();
			this.ManifestsToOpenSplitContainer.Panel2.SuspendLayout();
			this.ManifestsToOpenSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).BeginInit();
			this.AdditionalInfoGrid.SuspendLayout();
			this.ManifestsToOpenGroupBox.SuspendLayout();
			this.OpeningStyleDropEdit.SuspendLayout();
			this.WarehouseCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader);
			// 
			// ManifestsToOpenSplitContainer
			// 
			this.ManifestsToOpenSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsToOpenSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestsToOpenSplitContainer.Name = "ManifestsToOpenSplitContainer";
			this.ManifestsToOpenSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ManifestsToOpenSplitContainer.Panel1
			// 
			this.ManifestsToOpenSplitContainer.Panel1.Controls.Add(this.AdditionalInfoGrid);
			// 
			// ManifestsToOpenSplitContainer.Panel2
			// 
			this.ManifestsToOpenSplitContainer.Panel2.Controls.Add(this.ManifestsToOpenGroupBox);
			this.ManifestsToOpenSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 589, true);
			this.ManifestsToOpenSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(206);
			this.ManifestsToOpenSplitContainer.TabIndex = 0;
			// 
			// AdditionalInfoGrid
			// 
			this.AdditionalInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfoGrid, "ManifestsToOpenList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).BillNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).AtWarehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_CustomsOffice)));
			this.AdditionalInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.MaxDropDownItems = 3;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BillNo";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CSI_Quantity2";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.ColumnName = "AtWarehouse";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_CustomsOffice";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfoGrid.GridId = "142de25f-601d-4237-90d0-660438aa83e6";
			this.AdditionalInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfoGrid.LayoutKey = "GoodsItemsGrid";
			this.AdditionalInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfoGrid.Name = "AdditionalInfoGrid";
			this.AdditionalInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 206, true);
			this.AdditionalInfoGrid.TabIndex = 1;
			// 
			// ManifestsToOpenGroupBox
			// 
			this.ManifestsToOpenGroupBox.Controls.Add(this.OpeningStyleDropEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.RegistrationNoTextBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.AtWarehouseCheckBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.ReOpenedAgainCheckBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.BillNoTextBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.BillLineNoCalcEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.TotalBoxQuantityCalcEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.DeductionBoxQuantityCalcEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.WarehouseCodeFindBox);
			this.ManifestsToOpenGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsToOpenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestsToOpenGroupBox.Name = "ManifestsToOpenGroupBox";
			this.ManifestsToOpenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 379, true);
			this.ManifestsToOpenGroupBox.TabIndex = 2;
			this.ManifestsToOpenGroupBox.TabStop = false;
			this.ManifestsToOpenGroupBox.CaptionResourceString = Enterprise.Customs.TR.Manifest.GUI.Res.GetData("5b0fb174-dda9-4dfc-99c7-542008a17a64", "Item Details");
			// 
			// OpeningStyleDropEdit
			// 
			this.OpeningStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpeningStyleDropEdit, "ManifestsToOpenList.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_SubType)));
			this.OpeningStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 19, true);
			this.OpeningStyleDropEdit.Name = "OpeningStyleDropEdit";
			this.OpeningStyleDropEdit.PreBoundMaxLength = 1;
			this.OpeningStyleDropEdit.ShouldResizeByMaxLength = true;
			this.OpeningStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.OpeningStyleDropEdit.TabIndex = 1;
			// 
			// RegistrationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNoTextBox, "ManifestsToOpenList.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber2)));
			this.RegistrationNoTextBox.CaptionResourceString = null;
			this.RegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 45, true);
			this.RegistrationNoTextBox.Name = "RegistrationNoTextBox";
			this.RegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.RegistrationNoTextBox.TabIndex = 2;
			// 
			// AtWarehouseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AtWarehouseCheckBox, "ManifestsToOpenList.AtWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).AtWarehouse)));
			this.AtWarehouseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AtWarehouseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AtWarehouseCheckBox.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AtWarehouseCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AtWarehouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 71, true);
			this.AtWarehouseCheckBox.Name = "AtWarehouseCheckBox";
			this.AtWarehouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 22, true);
			this.AtWarehouseCheckBox.TabIndex = 3;
			this.AtWarehouseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AtWarehouseCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReOpenedAgainCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReOpenedAgainCheckBox, "ManifestsToOpenList.Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).Procedure)));
			this.ReOpenedAgainCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ReOpenedAgainCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReOpenedAgainCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 99, true);
			this.ReOpenedAgainCheckBox.Name = "ReOpenedAgainCheckBox";
			this.ReOpenedAgainCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 22, true);
			this.ReOpenedAgainCheckBox.TabIndex = 4;
			this.ReOpenedAgainCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ReOpenedAgainCheckBox.UseVisualStyleBackColor = true;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "ManifestsToOpenList.CSI_Description");// CSI_Description");
																												// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 127, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// BillNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillNoTextBox, "ManifestsToOpenList.BillNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).BillNo)));
			this.BillNoTextBox.CaptionResourceString = null;
			this.BillNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 153, true);
			this.BillNoTextBox.Name = "BillNoTextBox";
			this.BillNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.BillNoTextBox.TabIndex = 6;
			// 
			// BillLineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BillLineNoCalcEdit, "ManifestsToOpenList.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_LineNo)));
			this.BillLineNoCalcEdit.CaptionResourceString = null;
			this.BillLineNoCalcEdit.DecimalPlaces = 0;
			this.BillLineNoCalcEdit.Decimals = 0;
			this.BillLineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 179, true);
			this.BillLineNoCalcEdit.Name = "BillLineNoCalcEdit";
			this.BillLineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.BillLineNoCalcEdit.TabIndex = 7;
			this.BillLineNoCalcEdit.Text = "0";
			this.BillLineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalBoxQuantityCalcEdit
			// 
			this.TotalBoxQuantityCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalBoxQuantityCalcEdit, "ManifestsToOpenList.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity)));
			this.TotalBoxQuantityCalcEdit.CaptionResourceString = null;
			this.TotalBoxQuantityCalcEdit.DecimalPlaces = 2;
			this.TotalBoxQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 205, true);
			this.TotalBoxQuantityCalcEdit.Name = "TotalBoxQuantityCalcEdit";
			this.TotalBoxQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TotalBoxQuantityCalcEdit.TabIndex = 8;
			this.TotalBoxQuantityCalcEdit.Text = "0.00";
			this.TotalBoxQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DeductionBoxQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionBoxQuantityCalcEdit, "ManifestsToOpenList.CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity2)));
			this.DeductionBoxQuantityCalcEdit.CaptionResourceString = null;
			this.DeductionBoxQuantityCalcEdit.DecimalPlaces = 2;
			this.DeductionBoxQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 231, true);
			this.DeductionBoxQuantityCalcEdit.Name = "DeductionBoxQuantityCalcEdit";
			this.DeductionBoxQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DeductionBoxQuantityCalcEdit.TabIndex = 9;
			this.DeductionBoxQuantityCalcEdit.Text = "0.00";
			this.DeductionBoxQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WarehouseCodeFindBox
			// 
			this.WarehouseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseCodeFindBox, "ManifestsToOpenList.CSI_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.ManifestToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_CustomsOffice)));
			this.WarehouseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 257, true);
			this.WarehouseCodeFindBox.Name = "WarehouseCodeFindBox";
			this.WarehouseCodeFindBox.PreBoundMaxLength = 1;
			this.WarehouseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.WarehouseCodeFindBox.TabIndex = 10;
			// 
			// ManifestToOpenForManifestHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestsToOpenSplitContainer);
			this.Name = "ManifestToOpenForManifestHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestsToOpenSplitContainer.Panel1.ResumeLayout(false);
			this.ManifestsToOpenSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ManifestsToOpenSplitContainer)).EndInit();
			this.ManifestsToOpenSplitContainer.ResumeLayout(false);
			this.ManifestsToOpenSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).EndInit();
			this.AdditionalInfoGrid.ResumeLayout(false);
			this.AdditionalInfoGrid.PerformLayout();
			this.ManifestsToOpenGroupBox.ResumeLayout(false);
			this.ManifestsToOpenGroupBox.PerformLayout();
			this.OpeningStyleDropEdit.ResumeLayout(true);
			this.OpeningStyleDropEdit.PerformLayout();
			this.WarehouseCodeFindBox.ResumeLayout(true);
			this.WarehouseCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KSplitContainer ManifestsToOpenSplitContainer;
		public ZArchitecture.ZGrid AdditionalInfoGrid;
		protected ZArchitecture.GUI.ZGroupBox ManifestsToOpenGroupBox;

		#endregion }
		private ZArchitecture.GUI.ZDropEdit OpeningStyleDropEdit;
		private ZArchitecture.GUI.ZCheckBox AtWarehouseCheckBox;
		private ZArchitecture.GUI.ZCheckBox ReOpenedAgainCheckBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.ZTextBox BillNoTextBox;
		private ZArchitecture.ZCalcEdit BillLineNoCalcEdit;
		private ZArchitecture.ZCalcEdit TotalBoxQuantityCalcEdit;
		private ZArchitecture.ZCalcEdit DeductionBoxQuantityCalcEdit;
		private ZArchitecture.GUI.ZCodeFindBox WarehouseCodeFindBox;
		private ZTextBox RegistrationNoTextBox;
	}
}
