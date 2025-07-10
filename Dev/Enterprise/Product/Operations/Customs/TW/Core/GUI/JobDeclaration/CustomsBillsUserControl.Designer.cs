namespace Enterprise.Customs.TW.GUI
{
	partial class CustomsBillsUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.FilterByPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FilterByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HouseBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterByPanel.SuspendLayout();
			this.FilterByDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Controls.Add(this.FilterByDropEdit);
			this.FilterByPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterByPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterByPanel.Name = "FilterByPanel";
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 45, true);
			this.FilterByPanel.TabIndex = 1;
			// 
			// FilterByDropEdit
			// 
			this.FilterByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterByDropEdit, "JE_BillsFilterBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_BillsFilterBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Lookups.BillFilterByList)));
			this.FilterByDropEdit.BindToList = "Lookups+BillFilterByList";
			this.FilterByDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("128d8582-fe41-49e0-b19a-e4b5fb9f6a9f", "Filter By");
			this.FilterByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 13, true);
			this.FilterByDropEdit.Name = "FilterByDropEdit";
			this.FilterByDropEdit.PreBoundMaxLength = 3;
			this.FilterByDropEdit.ShouldResizeByMaxLength = true;
			this.FilterByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 17, true);
			this.FilterByDropEdit.TabIndex = 1;
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseBillsGrid, "FilteredBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_BillType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).Lookups.CU_BillTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_BillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_ParentBillUniqueCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).Lookups.CU_ParentBillList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_PackType)));
			this.HouseBillsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+CU_BillTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("23becc58-7d48-418d-b826-800e92f0cc23", "Bill Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CU_BillType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2e21cc37-8681-45c1-920d-4c7036c89c2a", "Bill Num.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CU_BillNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5d0b5ee3-7dfa-4a12-a1cb-049d60d9e1d8", "HBL Issue Date");
			zDateEditColumnStyleInfo1.ColumnName = "CU_IssueDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+CU_ParentBillList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8a59e9bb-a39f-4e4e-b974-36148a7c68fb", "Parent Bill ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CU_ParentBillUniqueCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b6a60266-7061-4ecf-af0f-0e3b41cac024", "Manifest Qty");
			zCalcEditColumnStyleInfo1.ColumnName = "CU_NoOfPacks";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("9083ea9f-4b90-4e7c-80bf-cf321ed87aac", "Manifest Qty/UQ");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b887763b-b992-4d28-bf02-dea9e0e6813e", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CU_PackType";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("9083ea9f-4b90-4e7c-80bf-cf321ed87aac", "Manifest Qty/UQ");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.HouseBillsGrid.GridId = "9fdaa7fa-8551-49a5-8c57-8899985df5c3";
			this.HouseBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsGrid.LayoutKey = "HouseBillsGrid";
			this.HouseBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.HouseBillsGrid.Name = "HouseBillsGrid";
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 159, true);
			this.HouseBillsGrid.TabIndex = 2;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("9423e361-8fc4-4c59-971b-7594a34cf181", "Packing Details");
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 204, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 182, true);
			this.PackingDetailsGroupBox.TabIndex = 3;
			this.PackingDetailsGroupBox.TabStop = false;
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.PackingDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackingDetailsGrid, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.LowestBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_MarksAndNos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).PackTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.WeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.WeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.VolumeUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).CW_DimensionUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.Package)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Packages)).SyncRoot)).Lookups.DimensionUQList)));
			this.PackingDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.BindToList = "Lookups+LowestBills";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("21CE06B4-74B3-4B40-9926-1AEB5262C5CC", "Linked Bill(Lowest Bill)");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CW_HouseBill";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("CE2B9453-C472-434B-94F5-533595BD2606", "Marks", "Marks & Numbers");
			zTextBoxColumnStyleInfo2.ColumnName = "CW_MarksAndNos";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("A328EB5A-D3C3-4942-AE66-E4C504B399A2", "Pack Qty");
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "CW_PackQty";
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.BindToList = "PackTypeList";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("76A88520-D03E-4608-B30F-3439AA1BB90E", "Pack Type");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "CW_PackType";
			zDropEditColumnStyleInfo5.IsMandatory = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CW_NetWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|57703896-6eef-4fa9-890b-bb0dd2756a3a", "Net Weight");
			zCalcEditColumnStyleInfo3.ToolTip = "Net Weight for Packing Details";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.BindToList = "Lookups.WeightUQList";
			zDropEditColumnStyleInfo6.ColumnName = "CW_NetWeightUQ";
			zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|57703896-6eef-4fa9-890b-bb0dd2756a3a", "Net Weight");
			zDropEditColumnStyleInfo6.ToolTip = "Net Weight Unit for Invoice Line";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CW_GrossWeight";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|a6fee84f-cc45-4fcf-9618-954919b01720", "Gross Weight");
			zCalcEditColumnStyleInfo4.ToolTip = "Gross Weight for Packing Details";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo7.BindToList = "Lookups.WeightUQList";
			zDropEditColumnStyleInfo7.ColumnName = "CW_GrossWeightUQ";
			zDropEditColumnStyleInfo7.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|a6fee84f-cc45-4fcf-9618-954919b01720", "Gross Weight");
			zDropEditColumnStyleInfo7.ToolTip = "Gross Weight Unit for Invoice Line";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CW_Volume";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|9a4a3dee-d852-4e4f-b834-7e03cd8f035f", "Volume");
			zCalcEditColumnStyleInfo5.ToolTip = "Volume for Packing Details";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo8.BindToList = "Lookups.VolumeUQList";
			zDropEditColumnStyleInfo8.ColumnName = "CW_VolumeUQ";
			zDropEditColumnStyleInfo8.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|9a4a3dee-d852-4e4f-b834-7e03cd8f035f", "Volume");
			zDropEditColumnStyleInfo8.ToolTip = "Volume Unit for Invoice Line";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "CW_Length";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|5a4eecce-d38b-4abc-960d-352940836d63", "Dimension");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "CW_Width";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|5a4eecce-d38b-4abc-960d-352940836d63", "Dimension");
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "CW_Height";
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|5a4eecce-d38b-4abc-960d-352940836d63", "Dimension");
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo9.BindToList = "Lookups.DimensionUQList";
			zDropEditColumnStyleInfo9.ColumnName = "CW_DimensionUQ";
			zDropEditColumnStyleInfo9.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("CustomsPackingUserControl|5a4eecce-d38b-4abc-960d-352940836d63", "Dimension");
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.PackingDetailsGrid.GridId = "d87612d5-13b2-4f15-ab3c-83202d1e2bea";
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 165, true);
			this.PackingDetailsGrid.TabIndex = 0;
			// 
			// CustomsBillsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackingDetailsGroupBox);
			this.Controls.Add(this.HouseBillsGrid);
			this.Controls.Add(this.FilterByPanel);
			this.Name = "CustomsBillsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 386, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterByPanel.ResumeLayout(false);
			this.FilterByPanel.PerformLayout();
			this.FilterByDropEdit.ResumeLayout(true);
			this.FilterByDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel FilterByPanel;
		private ZArchitecture.GUI.ZDropEdit FilterByDropEdit;
		public ZArchitecture.ZGrid HouseBillsGrid;
		protected ZArchitecture.GUI.ZGroupBox PackingDetailsGroupBox;
		protected ZArchitecture.ZGrid PackingDetailsGrid;
	}
}
