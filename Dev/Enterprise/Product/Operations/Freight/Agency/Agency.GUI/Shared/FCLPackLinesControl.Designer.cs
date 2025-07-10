namespace Enterprise.Freight.Agency.GUI
{
	partial class FCLPackLinesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalsBottomPanel = new CargoWise.Windows.UI.KPanel();
			this.PackLinesTotalPacks = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalPacks = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackLinesTotalWeight = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalWeight = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackLinesTotalVolume = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentTotalVolume = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.harmonisedCodeColumnStyleInfo = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.TotalsBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyShipment);
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "OuterPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_JC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_UnitOfDimension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_DetailedDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ExportRefNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_ImportRefNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentPackLine)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).OuterPackLines)).SyncRoot)).JL_HarmonisedCode)));
			this.PackLinesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "JL_JC";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.ColumnName = "JL_F3_NKPackType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "JL_UnitOfDimension";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo5.IsMandatory = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo6.IsMandatory = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JL_RH_NKCommodityCode";
			zMultiLineTextBoxColumnInfo1.ColumnName = "JL_DetailedDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo2.ColumnName = "JL_MarksAndNumbers";
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo1.ColumnName = "JL_ExportRefNumber";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "JL_ImportRefNumber";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			harmonisedCodeColumnStyleInfo.ColumnName = "JL_HarmonisedCode";
			harmonisedCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PackLinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(harmonisedCodeColumnStyleInfo);
			this.PackLinesGrid.CopySelectedRowsAllowed = true;
			this.PackLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesGrid.GridId = "2e506c1c-e237-46e2-b6f7-9229b1c5434a";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 140, true);
			this.PackLinesGrid.TabIndex = 0;
			// 
			// TotalsBottomPanel
			// 
			this.TotalsBottomPanel.Controls.Add(this.PackLinesTotalPacks);
			this.TotalsBottomPanel.Controls.Add(this.ShipmentTotalPacks);
			this.TotalsBottomPanel.Controls.Add(this.PackLinesTotalWeight);
			this.TotalsBottomPanel.Controls.Add(this.ShipmentTotalWeight);
			this.TotalsBottomPanel.Controls.Add(this.PackLinesTotalVolume);
			this.TotalsBottomPanel.Controls.Add(this.ShipmentTotalVolume);
			this.TotalsBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.TotalsBottomPanel.Name = "TotalsBottomPanel";
			this.TotalsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 56, true);
			this.TotalsBottomPanel.TabIndex = 1;
			// 
			// PackLinesTotalPacks
			// 
			this.PackLinesTotalPacks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackLinesTotalPacks, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksUnit)));
			this.PackLinesTotalPacks.BindToAmount = "TotalOuterPacks";
			this.PackLinesTotalPacks.BindToUnit = "TotalOuterPacksUnit";
			this.PackLinesTotalPacks.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("5658581f-813a-4715-9093-eb18517e2221", "Packs");
			this.PackLinesTotalPacks.Decimals = 2;
			this.PackLinesTotalPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 8, true);
			this.PackLinesTotalPacks.Name = "PackLinesTotalPacks";
			this.PackLinesTotalPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PackLinesTotalPacks.TabIndex = 0;
			this.PackLinesTotalPacks.UnitPreBoundMaxLength = 3;
			// 
			// ShipmentTotalPacks
			// 
			this.ShipmentTotalPacks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalPacks, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_OuterPacksReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksUnit)));
			this.ShipmentTotalPacks.BindToAmount = "JS_OuterPacksReadOnly";
			this.ShipmentTotalPacks.BindToUnit = "TotalOuterPacksUnit";
			this.ShipmentTotalPacks.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("3f52b389-e689-4340-a8d0-b8afd80b8e1e", "Packs");
			this.ShipmentTotalPacks.Decimals = 2;
			this.ShipmentTotalPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 32, true);
			this.ShipmentTotalPacks.Name = "ShipmentTotalPacks";
			this.ShipmentTotalPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipmentTotalPacks.TabIndex = 1;
			this.ShipmentTotalPacks.UnitPreBoundMaxLength = 3;
			// 
			// PackLinesTotalWeight
			// 
			this.PackLinesTotalWeight.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackLinesTotalWeight, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineWeightUnit)));
			this.PackLinesTotalWeight.BindToAmount = "TotalOuterPacksWeight";
			this.PackLinesTotalWeight.BindToUnit = "TotalPackLineWeightUnit";
			this.PackLinesTotalWeight.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("d33e2cac-932e-4715-9f2c-1f1ad0c887c1", "Weight");
			this.PackLinesTotalWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 8, true);
			this.PackLinesTotalWeight.Name = "PackLinesTotalWeight";
			this.PackLinesTotalWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PackLinesTotalWeight.TabIndex = 2;
			this.PackLinesTotalWeight.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentTotalWeight
			// 
			this.ShipmentTotalWeight.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalWeight, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_ActualWeightReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineWeightUnit)));
			this.ShipmentTotalWeight.BindToAmount = "JS_ActualWeightReadOnly";
			this.ShipmentTotalWeight.BindToUnit = "TotalPackLineWeightUnit";
			this.ShipmentTotalWeight.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("e7884455-6205-4d07-a0f9-31c700ac06fd", "Weight");
			this.ShipmentTotalWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 32, true);
			this.ShipmentTotalWeight.Name = "ShipmentTotalWeight";
			this.ShipmentTotalWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentTotalWeight.TabIndex = 3;
			this.ShipmentTotalWeight.UnitPreBoundMaxLength = 2;
			// 
			// PackLinesTotalVolume
			// 
			this.PackLinesTotalVolume.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackLinesTotalVolume, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalOuterPacksVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineVolumeUnit)));
			this.PackLinesTotalVolume.BindToAmount = "TotalOuterPacksVolume";
			this.PackLinesTotalVolume.BindToUnit = "TotalPackLineVolumeUnit";
			this.PackLinesTotalVolume.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("30b89a3d-3af6-486f-82d1-b6eff3d7fd53", "Volume");
			this.PackLinesTotalVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 8, true);
			this.PackLinesTotalVolume.Name = "PackLinesTotalVolume";
			this.PackLinesTotalVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PackLinesTotalVolume.TabIndex = 4;
			this.PackLinesTotalVolume.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentTotalVolume
			// 
			this.ShipmentTotalVolume.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTotalVolume, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).JS_ActualVolumeReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipment)(null)).TotalPackLineVolumeUnit)));
			this.ShipmentTotalVolume.BindToAmount = "JS_ActualVolumeReadOnly";
			this.ShipmentTotalVolume.BindToUnit = "TotalPackLineVolumeUnit";
			this.ShipmentTotalVolume.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("32e7fbee-0b6a-465c-b220-0d0ba89f33e2", "Volume");
			this.ShipmentTotalVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 32, true);
			this.ShipmentTotalVolume.Name = "ShipmentTotalVolume";
			this.ShipmentTotalVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentTotalVolume.TabIndex = 5;
			this.ShipmentTotalVolume.UnitPreBoundMaxLength = 2;
			// 
			// FCLPackLinesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackLinesGrid);
			this.Controls.Add(this.TotalsBottomPanel);
			this.Name = "FCLPackLinesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 196, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.TotalsBottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid PackLinesGrid;
		private CargoWise.Windows.UI.KPanel TotalsBottomPanel;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalPacks;
		private ZArchitecture.GUI.ZCalcDropEdit PackLinesTotalWeight;
		private ZArchitecture.GUI.ZCalcDropEdit PackLinesTotalVolume;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalVolume;
		private ZArchitecture.GUI.ZCalcDropEdit PackLinesTotalPacks;
		private ZArchitecture.GUI.ZCalcDropEdit ShipmentTotalWeight;
		Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo harmonisedCodeColumnStyleInfo;
	}
}
