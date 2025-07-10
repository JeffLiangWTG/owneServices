using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AddressLevelExporterSchemeControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
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
			this.MajorExporterGroupBox.CaptionResourceString = GroupBoxCaption;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXApprovedOrMajorExporter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_OA_ApprovedLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXApprovalExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_EXExportPermissionDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCountryData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryDataCollectionForThisCompany)).SyncRoot)).OV_SystemLastEditUser)));
			this.CountryDataCollectionForThisCompanyGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlHK|7a121664-694d-421e-b5bb-cacac92fe842", "Known/Approved");
			zDropEditColumnStyleInfo1.ColumnName = "OV_EXApprovedOrMajorExporter";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "OV_EXApprovalNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo1.ColumnName = "OV_OA_ApprovedLocation";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExporterSchemeControlHK|14b3545f-6b2d-40dd-9c08-0207f9728fe9", "Expiry Date");
			zDateEditColumnStyleInfo1.ColumnName = "OV_EXApprovalExpiryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zMultiLineTextBoxColumnInfo1.ColumnName = "OV_EXExportPermissionDetails";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo2.ColumnName = "OV_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "OV_SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "OV_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "OV_SystemLastEditUser";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OV_RN_NKIssuingAuthorityCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CountryDataCollectionForThisCompanyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CountryDataCollectionForThisCompanyGrid.CopySelectedRowsAllowed = true;
			this.CountryDataCollectionForThisCompanyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryDataCollectionForThisCompanyGrid.GridId = "00db63f1-0c5b-4705-8532-e4f58db1e3a5";
			this.CountryDataCollectionForThisCompanyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryDataCollectionForThisCompanyGrid.LayoutKey = "zGrid1";
			this.CountryDataCollectionForThisCompanyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CountryDataCollectionForThisCompanyGrid.Name = "CountryDataCollectionForThisCompanyGrid";
			this.CountryDataCollectionForThisCompanyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 325, true);
			this.CountryDataCollectionForThisCompanyGrid.TabIndex = 0;
			// 
			// ExporterSchemeControlHK
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MajorExporterGroupBox);
			this.Name = "ExporterSchemeControlHK";
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
