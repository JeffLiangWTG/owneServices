using Enterprise.Freight.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class MiscServiceCTOStorageUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo4 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo importFreeDayExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo importDurationExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo exportFreeDayExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo exportDurationExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo();
			this.CTOStorageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.importCTOStorageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ServiceImportCTOStoragesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.exportCTOStorageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ServiceExportCTOStoragesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CTOStorageSplitContainer)).BeginInit();
			this.CTOStorageSplitContainer.Panel1.SuspendLayout();
			this.CTOStorageSplitContainer.Panel2.SuspendLayout();
			this.CTOStorageSplitContainer.SuspendLayout();
			this.importCTOStorageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceImportCTOStoragesGrid)).BeginInit();
			this.ServiceImportCTOStoragesGrid.SuspendLayout();
			this.exportCTOStorageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceExportCTOStoragesGrid)).BeginInit();
			this.ServiceExportCTOStoragesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CTOStorageSplitContainer
			// 
			this.CTOStorageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CTOStorageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CTOStorageSplitContainer.Name = "CTOStorageSplitContainer";
			// 
			// CTOStorageSplitContainer.Panel1
			// 
			this.CTOStorageSplitContainer.Panel1.Controls.Add(this.importCTOStorageGroupBox);
			// 
			// CTOStorageSplitContainer.Panel2
			// 
			this.CTOStorageSplitContainer.Panel2.Controls.Add(this.exportCTOStorageGroupBox);
			this.CTOStorageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 378, true);
			this.CTOStorageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.CTOStorageSplitContainer.TabIndex = 5;
			// 
			// importCTOStorageGroupBox
			// 
			this.importCTOStorageGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6b7e964d-4bfb-48e9-91dc-2b37b6dc0e7c", "Import CTO Storage");
			this.importCTOStorageGroupBox.Controls.Add(this.ServiceImportCTOStoragesGrid);
			this.importCTOStorageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importCTOStorageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importCTOStorageGroupBox.Name = "importCTOStorageGroupBox";
			this.importCTOStorageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 378, true);
			this.importCTOStorageGroupBox.TabIndex = 0;
			this.importCTOStorageGroupBox.TabStop = false;
			// 
			// ServiceImportCTOStoragesGrid
			// 
			this.ServiceImportCTOStoragesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServiceImportCTOStoragesGrid, "ServiceImportCTOStorages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_OH_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceImportCTOStorages)).SyncRoot)).PD_FreeDayType)));
			this.ServiceImportCTOStoragesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("206bb8cc-5c7d-46cd-9782-2f609185c5e3", "Port/Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "PD_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5f9b6101-d648-416a-b1fd-094bf76c28cb", "Class");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e855f89b-ffbc-467d-b296-ae01016717d1", "Client");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "PD_OH_Client";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("63fa09ed-3775-48c4-bb42-a46aa443b4cd", "Free Days");
			zCalcEditColumnStyleInfo1.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6989d5e5-3545-4e52-a28f-f1d8a4ccadd2", "1st Free Day");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			importFreeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			importFreeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			importDurationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			importDurationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(importFreeDayExclusionColumnStyleInfo);
			this.ServiceImportCTOStoragesGrid.ColumnStyles.Add(importDurationExclusionColumnStyleInfo);

			this.ServiceImportCTOStoragesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServiceImportCTOStoragesGrid.GridId = "4f6bd849-44f8-4a7f-b978-1e086b684b1e";
			this.ServiceImportCTOStoragesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceImportCTOStoragesGrid.LayoutKey = "CarrierImportCTOStoragesGrid";
			this.ServiceImportCTOStoragesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ServiceImportCTOStoragesGrid.Name = "ServiceImportCTOStoragesGrid";
			this.ServiceImportCTOStoragesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 359, true);
			this.ServiceImportCTOStoragesGrid.TabIndex = 1;
			// 
			// exportCTOStorageGroupBox
			// 
			this.exportCTOStorageGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2ac17c61-ebd9-4cda-8802-2471d7de23d3", "Export CTO Storage");
			this.exportCTOStorageGroupBox.Controls.Add(this.ServiceExportCTOStoragesGrid);
			this.exportCTOStorageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exportCTOStorageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportCTOStorageGroupBox.Name = "exportCTOStorageGroupBox";
			this.exportCTOStorageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 378, true);
			this.exportCTOStorageGroupBox.TabIndex = 0;
			this.exportCTOStorageGroupBox.TabStop = false;
			// 
			// ServiceExportCTOStoragesGrid
			// 
			this.ServiceExportCTOStoragesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServiceExportCTOStoragesGrid, "ServiceExportCTOStorages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_OH_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ServiceExportCTOStorages)).SyncRoot)).PD_FreeDayType)));
			this.ServiceExportCTOStoragesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ffd407fa-4a52-4114-b6c2-a9ef6638c567", "Port/Country/Region");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "PD_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dc860c64-17a1-408c-904f-bb86740cb03c", "Class");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e15692e7-3af8-4c97-9b8e-548d3745e9c0", "Client");
			zOrganisationFindBoxColumnStyleInfo4.ColumnName = "PD_OH_Client";
			zOrganisationFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("323519ec-00b1-4fd4-94bc-f83fc92c0c39", "Free Days");
			zCalcEditColumnStyleInfo2.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c8fad960-ad09-4989-a859-866fcc2adbe8", "Last Free Day");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			exportFreeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			exportFreeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			exportDurationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			exportDurationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(exportFreeDayExclusionColumnStyleInfo);
			this.ServiceExportCTOStoragesGrid.ColumnStyles.Add(exportDurationExclusionColumnStyleInfo);

			this.ServiceExportCTOStoragesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServiceExportCTOStoragesGrid.GridId = "884a1e83-f7e5-4e8a-95f6-13dd3b153da4";
			this.ServiceExportCTOStoragesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceExportCTOStoragesGrid.LayoutKey = "CarrierExportCTOStoragesGrid";
			this.ServiceExportCTOStoragesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ServiceExportCTOStoragesGrid.Name = "ServiceExportCTOStoragesGrid";
			this.ServiceExportCTOStoragesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 359, true);
			this.ServiceExportCTOStoragesGrid.TabIndex = 2;
			// 
			// MiscServiceCTOStorageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CTOStorageSplitContainer);
			this.Name = "MiscServiceCTOStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 378, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CTOStorageSplitContainer.Panel1.ResumeLayout(false);
			this.CTOStorageSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CTOStorageSplitContainer)).EndInit();
			this.CTOStorageSplitContainer.ResumeLayout(false);
			this.CTOStorageSplitContainer.PerformLayout();
			this.importCTOStorageGroupBox.ResumeLayout(false);
			this.importCTOStorageGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceImportCTOStoragesGrid)).EndInit();
			this.ServiceImportCTOStoragesGrid.ResumeLayout(false);
			this.ServiceImportCTOStoragesGrid.PerformLayout();
			this.exportCTOStorageGroupBox.ResumeLayout(false);
			this.exportCTOStorageGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceExportCTOStoragesGrid)).EndInit();
			this.ServiceExportCTOStoragesGrid.ResumeLayout(false);
			this.ServiceExportCTOStoragesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer CTOStorageSplitContainer;
		private ZArchitecture.GUI.ZGroupBox importCTOStorageGroupBox;
		private ZArchitecture.ZGrid ServiceImportCTOStoragesGrid;
		private ZArchitecture.GUI.ZGroupBox exportCTOStorageGroupBox;
		private ZArchitecture.ZGrid ServiceExportCTOStoragesGrid;
	}
}
