using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class CYDInvoicingUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo yardColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo transportModeColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo unitTypeColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo unitLengthColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo containerClassColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo unitLoadColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo freeDaysColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AutoRatingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreeStorageDaysGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreeStorageDaysGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RatingPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StorageCalcMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoPeriodicInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AutoCreateAndRatePeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoPostPeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoDeliverPeriodicInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AutoRatingGroupBox.SuspendLayout();
			this.FreeStorageDaysGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FreeStorageDaysGrid)).BeginInit();
			this.FreeStorageDaysGrid.SuspendLayout();
			this.RatingPeriodDropEdit.SuspendLayout();
			this.StorageCalcMethodDropEdit.SuspendLayout();
			this.AutoPeriodicInvoiceGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// AutoRatingGroupBox
			// 
			this.AutoRatingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDInvoicingUserControl|3326c8b0-2d5e-4d26-97d0-ee5fead8f81e", "Auto Rating");
			this.AutoRatingGroupBox.Controls.Add(this.FreeStorageDaysGroupBox);
			this.AutoRatingGroupBox.Controls.Add(this.RatingPeriodDropEdit);
			this.AutoRatingGroupBox.Controls.Add(this.StorageCalcMethodDropEdit);
			this.AutoRatingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AutoRatingGroupBox.Name = "AutoRatingGroupBox";
			this.AutoRatingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 266, true);
			this.AutoRatingGroupBox.TabIndex = 0;
			this.AutoRatingGroupBox.TabStop = false;
			// 
			// FreeStorageDaysGroupBox
			// 
			this.FreeStorageDaysGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDInvoicingUserControl|85775e17-9e48-4022-9dab-69b4e8c09cf7", "Free Storage Days");
			this.FreeStorageDaysGroupBox.Controls.Add(this.FreeStorageDaysGrid);
			this.FreeStorageDaysGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FreeStorageDaysGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 121, true);
			this.FreeStorageDaysGroupBox.Name = "FreeStorageDaysGroupBox";
			this.FreeStorageDaysGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 193, true);
			this.FreeStorageDaysGroupBox.TabIndex = 0;
			this.FreeStorageDaysGroupBox.TabStop = false;
			// 
			// FreeStorageDaysGrid
			// 
			this.FreeStorageDaysGrid.AllowNavigation = false;
			this.FreeStorageDaysGrid.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
			this.BindingSource.SetBindingMember(this.FreeStorageDaysGrid, "YardStorageFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_WW_Yard)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_UnitType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_YardUnitLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_ContainerClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_UnitLoad)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((CYDYardStorageFreeDays)(((System.Collections.IList)(((OrgHeader)(null)).YardStorageFreeDays)).SyncRoot)).YFD_FreeDays)));
			this.FreeStorageDaysGrid.CaptionVisible = false;
			yardColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_WW_Yard;
			yardColumnStyleInfo.DefaultCollectionIndex = 0;
			yardColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			transportModeColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_TransportMode;
			transportModeColumnStyleInfo.DefaultCollectionIndex = 1;
			transportModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			unitTypeColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_UnitType;
			unitTypeColumnStyleInfo.DefaultCollectionIndex = 2;
			unitTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			unitLengthColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YardUnitLength;
			unitLengthColumnStyleInfo.DefaultCollectionIndex = 3;
			unitLengthColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			containerClassColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_ContainerClass;
			containerClassColumnStyleInfo.DefaultCollectionIndex = 4;
			containerClassColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			unitLoadColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_UnitLoad;
			unitLoadColumnStyleInfo.DefaultCollectionIndex = 5;
			unitLoadColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			freeDaysColumnStyleInfo.ColumnName = CYDYardStorageFreeDays.Schema.YFD_FreeDays;
			freeDaysColumnStyleInfo.DefaultCollectionIndex = 6;
			freeDaysColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.FreeStorageDaysGrid.ColumnStyles.Add(yardColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(transportModeColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(unitTypeColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(unitLengthColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(containerClassColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(unitLoadColumnStyleInfo);
			this.FreeStorageDaysGrid.ColumnStyles.Add(freeDaysColumnStyleInfo);
			this.FreeStorageDaysGrid.GridId = "115c57cc-6ca9-41c3-9755-f718fa2388d4";
			this.FreeStorageDaysGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FreeStorageDaysGrid.LayoutKey = "FreeStorageDaysGrid";
			this.FreeStorageDaysGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 16, true);
			this.FreeStorageDaysGrid.Name = "FreeStorageDaysGrid";
			this.FreeStorageDaysGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 172, true);
			this.FreeStorageDaysGrid.TabIndex = 7;
			// 
			// RatingPeriodDropEdit
			// 
			this.RatingPeriodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RatingPeriodDropEdit, "CompanyData.OB_ARYardStorageRatingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARYardStorageRatingPeriod)));
			this.RatingPeriodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDInvoicingUserControl|fedbbb31-50db-4a33-b8ff-84eb7129a4a0", "Storage Rating Period");
			this.RatingPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 20, true);
			this.RatingPeriodDropEdit.Name = "RatingPeriodDropEdit";
			this.RatingPeriodDropEdit.PreBoundMaxLength = 3;
			this.RatingPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 22, true);
			this.RatingPeriodDropEdit.TabIndex = 4;
			// 
			// StorageCalcMethodDropEdit
			// 
			this.StorageCalcMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StorageCalcMethodDropEdit, "CompanyData.OB_ARYardStorageCalcMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARYardStorageCalcMethod)));
			this.StorageCalcMethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDInvoicingUserControl|ba58e25a-1e4d-49c4-8928-3876b20ceed9", "Storage Calculation Method");
			this.StorageCalcMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 47, true);
			this.StorageCalcMethodDropEdit.Name = "StorageCalcMethodDropEdit";
			this.StorageCalcMethodDropEdit.PreBoundMaxLength = 3;
			this.StorageCalcMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 22, true);
			this.StorageCalcMethodDropEdit.TabIndex = 5;
			// 
			// AutoPeriodicInvoiceGroupBox
			// 
			this.AutoPeriodicInvoiceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CYDInvoicingUserControl|AutoPeriodicInvoiceGroupBox", "Periodic Invoice");
			this.AutoPeriodicInvoiceGroupBox.Controls.Add(this.AutoCreateAndRatePeriodicInvoiceCheckBox);
			this.AutoPeriodicInvoiceGroupBox.Controls.Add(this.AutoPostPeriodicInvoiceCheckBox);
			this.AutoPeriodicInvoiceGroupBox.Controls.Add(this.AutoDeliverPeriodicInvoiceCheckBox);
			this.AutoPeriodicInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 280, true);
			this.AutoPeriodicInvoiceGroupBox.Name = "AutoPeriodicInvoiceGroupBox";
			this.AutoPeriodicInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 101, true);
			this.AutoPeriodicInvoiceGroupBox.TabIndex = 2;
			this.AutoPeriodicInvoiceGroupBox.TabStop = false;
			// 
			// AutoCreateAndRatePeriodicInvoiceCheckBox
			// 
			this.AutoCreateAndRatePeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoCreateAndRatePeriodicInvoiceCheckBox, "CompanyData.OB_YardAutoCreateAndRatePeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_YardAutoCreateAndRatePeriodicInvoice)));
			this.AutoCreateAndRatePeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
			this.AutoCreateAndRatePeriodicInvoiceCheckBox.Name = "AutoCreateAndRatePeriodicInvoiceCheckBox";
			this.AutoCreateAndRatePeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 21, true);
			this.AutoCreateAndRatePeriodicInvoiceCheckBox.TabIndex = 0;
			// 
			// AutoPostPeriodicInvoiceCheckBox
			// 
			this.AutoPostPeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPostPeriodicInvoiceCheckBox, "CompanyData.OB_YardAutoPostPeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_YardAutoPostPeriodicInvoice)));
			this.AutoPostPeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 47, true);
			this.AutoPostPeriodicInvoiceCheckBox.Name = "AutoPostPeriodicInvoiceCheckBox";
			this.AutoPostPeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 21, true);
			this.AutoPostPeriodicInvoiceCheckBox.TabIndex = 1;
			// 
			// AutoDeliverPeriodicInvoiceCheckBox
			// 
			this.AutoDeliverPeriodicInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoDeliverPeriodicInvoiceCheckBox, "CompanyData.OB_YardAutoDeliverPeriodicInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_YardAutoDeliverPeriodicInvoice)));
			this.AutoDeliverPeriodicInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 73, true);
			this.AutoDeliverPeriodicInvoiceCheckBox.Name = "AutoDeliverPeriodicInvoiceCheckBox";
			this.AutoDeliverPeriodicInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 21, true);
			this.AutoDeliverPeriodicInvoiceCheckBox.TabIndex = 2;
			// 
			// CYDInvoicingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AutoPeriodicInvoiceGroupBox);
			this.Controls.Add(this.AutoRatingGroupBox);
			this.Name = "CYDInvoicingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 565, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FreeStorageDaysGroupBox.ResumeLayout(false);
			this.FreeStorageDaysGrid.PerformLayout();
			this.AutoRatingGroupBox.ResumeLayout(false);
			this.AutoRatingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FreeStorageDaysGrid)).EndInit();
			this.FreeStorageDaysGrid.ResumeLayout(false);
			this.FreeStorageDaysGrid.PerformLayout();
			this.RatingPeriodDropEdit.ResumeLayout(true);
			this.RatingPeriodDropEdit.PerformLayout();
			this.StorageCalcMethodDropEdit.ResumeLayout(true);
			this.StorageCalcMethodDropEdit.PerformLayout();
			this.AutoPeriodicInvoiceGroupBox.ResumeLayout(false);
			this.AutoPeriodicInvoiceGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox AutoRatingGroupBox;
		private ZGroupBox FreeStorageDaysGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RatingPeriodDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StorageCalcMethodDropEdit;
		private ZGroupBox AutoPeriodicInvoiceGroupBox;
		private ZCheckBox AutoCreateAndRatePeriodicInvoiceCheckBox;
		private ZCheckBox AutoPostPeriodicInvoiceCheckBox;
		private ZCheckBox AutoDeliverPeriodicInvoiceCheckBox;
		private Enterprise.ZArchitecture.ZGrid FreeStorageDaysGrid;
	}
}
