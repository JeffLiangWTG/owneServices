using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class ExporterSchemeControlUS
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.MajorExporterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryDataCollectionForThisCompanyGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MajorExporterGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryDataCollectionForThisCompanyGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// MajorExporterGroupBox
			// 
			this.MajorExporterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlUS|8f0d105d-b4cc-4a22-b70a-40c5d054c470", "TSA Known Shipper Details");
			this.MajorExporterGroupBox.Controls.Add(this.CountryDataCollectionForThisCompanyGrid);
			this.MajorExporterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MajorExporterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MajorExporterGroupBox.Name = "MajorExporterGroupBox";
			this.MajorExporterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 344, true);
			this.MajorExporterGroupBox.TabIndex = 6;
			this.MajorExporterGroupBox.TabStop = false;
			// 
			// CountryDataCollectionForThisCompanyGrid
			// 
			this.CountryDataCollectionForThisCompanyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryDataCollectionForThisCompanyGrid, "CountryDataCollectionForThisCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_OA_ApprovedLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXApprovedOrMajorExporter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXE3Signed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXSiteInspectionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXExportPermissionDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemLastEditTimeUtc)));
			this.CountryDataCollectionForThisCompanyGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "OV_OA_ApprovedLocation";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "OV_EXApprovedOrMajorExporter";
			zCheckBoxColumnStyleInfo1.ColumnName = "OV_EXE3Signed";
			zDateEditColumnStyleInfo1.ColumnName = "OV_EXSiteInspectionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0cd1c850-41de-4292-a9d5-4185b62593ff", "TSA ID", "TSA ID Number", "");
			zTextBoxColumnStyleInfo1.ColumnName = "OV_EXApprovalNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo1.ColumnName = "OV_EXExportPermissionDetails";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.ColumnName = "OV_SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlUS|afe39ce2-f747-4e81-8328-d9fa9b25218d", "Created On");
			zDateEditColumnStyleInfo2.ColumnName = "OV_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlUS|3571a3a9-36de-4c97-b40c-1a6eccdbbe31", "Last Edit By");
			zTextBoxColumnStyleInfo3.ColumnName = "OV_SystemLastEditUser";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlUS|06aa8712-3d17-4c82-b16d-4a98b90f96dc", "Last Edit On");
			zDateEditColumnStyleInfo3.ColumnName = "OV_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CountryDataCollectionForThisCompanyGrid.GridId = "00db63f1-0c5b-4705-8532-e4f58db1e3a5";
			this.CountryDataCollectionForThisCompanyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryDataCollectionForThisCompanyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryDataCollectionForThisCompanyGrid.LayoutKey = "zGrid1";
			this.CountryDataCollectionForThisCompanyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CountryDataCollectionForThisCompanyGrid.Name = "CountryDataCollectionForThisCompanyGrid";
			this.CountryDataCollectionForThisCompanyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 325, true);
			this.CountryDataCollectionForThisCompanyGrid.TabIndex = 0;
			// 
			// ExporterSchemeControlUS
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MajorExporterGroupBox);
			this.Name = "ExporterSchemeControlUS";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MajorExporterGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CountryDataCollectionForThisCompanyGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox MajorExporterGroupBox;
		private Enterprise.ZArchitecture.ZGrid CountryDataCollectionForThisCompanyGrid;
	}
}
