namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class CommoditiesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CommoditiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommoditiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CommodityUserControl = new Enterprise.Customs.US.eManifest.GUI.CommodityUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.CommoditiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommoditiesGrid)).BeginInit();
			this.CommoditiesGrid.SuspendLayout();
			this.CommodityUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Shipment);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.CommoditiesGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 294, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.CommodityUserControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(430);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(216);
			this.SplitContainer.TabIndex = 1;
			// 
			// CommoditiesGroupBox
			// 
			this.CommoditiesGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|7a46ee3b-122b-44a9-a82e-43a99b752d59", "Commodities");
			this.CommoditiesGroupBox.Controls.Add(this.CommoditiesGrid);
			this.CommoditiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommoditiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommoditiesGroupBox.Name = "CommoditiesGroupBox";
			this.CommoditiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 294, true);
			this.CommoditiesGroupBox.TabIndex = 0;
			this.CommoditiesGroupBox.TabStop = false;
			// 
			// CommoditiesGrid
			// 
			this.CommoditiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommoditiesGrid, "Commodities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_PieceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_ManifestUnitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_GrossWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_BJ_Equipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_RN_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_HarmonizedNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_HazardousGoodsIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_HazardousGoodsContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_HazardousGoodsContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_VehicleIdentificationNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)).BY_C4Codes)));
			this.CommoditiesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|b9692c45-12b7-40f0-8d54-e0a0744deb87", "Packages", "Number Of Packages", "Total Number shown on Bill of Lading for this specific commodity.");
			zCalcEditColumnStyleInfo1.ColumnName = "BY_PieceCount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|f33bb082-77b2-4ffb-a3d8-8693ec54b5fd", "Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|408a7e60-04b9-46f1-b7c9-e0ab2abec2b7", "Type", "Type Of Packages", "");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BY_ManifestUnitCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|f33bb082-77b2-4ffb-a3d8-8693ec54b5fd", "Packages");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|cb986322-5335-43a8-bb38-8ab5f354b037", "Weight", "Gross Weight", "Cargo Gross Weight", "Weight of the listed cargo plus any packaging, but excluding weight of the carrier\'s equipment.");
			zCalcEditColumnStyleInfo2.ColumnName = "BY_GrossWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|d659beb5-8add-4ff1-afed-9831d005f6f2", "Gross Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|784edc5b-4208-4516-a3f4-328feb71a774", "Unit", "Weight Unit", "Cargo Gross Weight Unit", "");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "BY_GrossWeightUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|d659beb5-8add-4ff1-afed-9831d005f6f2", "Gross Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|42e2ba9d-62a6-4f33-bcb7-3561a6412d5c", "Description", "Description of Cargo", "A description of the cargo in common trade terms. \"No Freight of All Kinds\" or \"Said to Contain\" will be accepted.");
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "BY_Description";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|b506958c-076a-4cd8-bcbf-851c0d66795d", "Equipment", "The goods are loaded into or on to equipment.");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "BY_BJ_Equipment";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|61ca49c0-8591-4c1f-b004-b4d557d2f172", "Marks", "Marks And Numbers", "The shipping marks & numbers found on the outside of packaging units.");
			zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo2.ColumnName = "BY_MarksAndNumbers";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|6f42ebc5-1570-4c47-acec-7c8b7ff3dfd1", " Value", "Customs Value", "Customs shipment value. In whole dollars. For Sec 321 releases this will be the actual value. The estimated value will be used for IE and TE.");
			zCalcEditColumnStyleInfo3.ColumnName = "BY_MonetaryValue";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|04d4cb27-bcfa-414f-8e85-64e5fdbe3ea6", "Customs Value");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|fa147938-5dfe-4aa7-a649-da2a29eed726", "Origin", "Country Of Origin", "The country of manufacture, production, or growth of any article of foreign origin entering the U.S.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BY_RN_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("8ada832e-5e6e-4c97-b1f5-2905795e8717", "Harmonized Numbers");
			zTextBoxColumnStyleInfo1.ColumnName = "BY_HarmonizedNumbers";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("8dbb3f04-bd08-4ae2-8522-f1e70e3d160c", "DG Code", "Hazardous Materials Code", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BY_HazardousGoodsIdentifier";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|fd6164bb-2665-46ac-ab85-f1bd78e94965", "Hazardous Materials");
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("85e6b8f9-a58f-4e18-ab39-1974e8f0cb56", "Hazmat Contact", "Hazardous Materials Contact", "");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BY_HazardousGoodsContact";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|fd6164bb-2665-46ac-ab85-f1bd78e94965", "Hazardous Materials");
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c15d9608-3f6b-473c-8c5d-5e90ec5cc755", "Phone", "Contact Phone", "Hazardous Materials Contact Phone", "");
			zTextBoxColumnStyleInfo2.ColumnName = "BY_HazardousGoodsContactPhone";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CommoditiesUserControl|fd6164bb-2665-46ac-ab85-f1bd78e94965", "Hazardous Materials");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("202f3aae-4026-4299-adb5-e12c1fd846cc", "VINs", "Vehicle Identification Numbers", "");
			zTextBoxColumnStyleInfo3.ColumnName = "BY_VehicleIdentificationNumbers";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("7988d01e-15f7-42d1-a45c-ea81f949f2eb", "C4 Codes");
			zTextBoxColumnStyleInfo4.ColumnName = "BY_C4Codes";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CommoditiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CommoditiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommoditiesGrid.GridId = "0083fb4d-a4db-48e5-985c-0d45e22681a5";
			this.CommoditiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommoditiesGrid.LayoutKey = "CommoditiesGrid";
			this.CommoditiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CommoditiesGrid.Name = "CommoditiesGrid";
			this.CommoditiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 275, true);
			this.CommoditiesGrid.TabIndex = 0;
			// 
			// CommodityUserControl
			// 
			this.CommodityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityUserControl, "Commodities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.eManifest.Business.Commodity)(((Enterprise.Customs.US.eManifest.Business.Commodity)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Commodities)).SyncRoot)))));
			this.CommodityUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityUserControl.Name = "CommodityUserControl";
			this.CommodityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 294, true);
			this.CommodityUserControl.TabIndex = 0;
			// 
			// CommoditiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "CommoditiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 294, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.CommoditiesGroupBox.ResumeLayout(false);
			this.CommoditiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommoditiesGrid)).EndInit();
			this.CommoditiesGrid.ResumeLayout(false);
			this.CommoditiesGrid.PerformLayout();
			this.CommodityUserControl.ResumeLayout(true);
			this.CommodityUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox CommoditiesGroupBox;
		private CommodityUserControl CommodityUserControl;
		private ZArchitecture.ZGrid CommoditiesGrid;

	}
}
