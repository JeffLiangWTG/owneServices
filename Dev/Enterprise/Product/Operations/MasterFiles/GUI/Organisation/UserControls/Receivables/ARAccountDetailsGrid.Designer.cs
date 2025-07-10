using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables
{
	partial class ARAccountDetailsGrid
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCompanyData);
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, "ARAccountDetailsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)));

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_IsDefaultAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_RX_NKAccountCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_AccountName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_BankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_BankSwift)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_BankBsb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_BankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_IBANNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARAccountDetailsCollection)).SyncRoot)).A1_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).AccountDetailsCollection)).SyncRoot)).A1_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).AccountDetailsCollection)).SyncRoot)).A1_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).AccountDetailsCollection)).SyncRoot)).A1_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.AccARAccountDetails)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).AccountDetailsCollection)).SyncRoot)).A1_SystemLastEditTimeUtc)));
			this.grid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "A1_IsDefaultAccount";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.ColumnName = "A1_PaymentMethod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "A1_RX_NKAccountCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo1.ColumnName = "A1_AccountName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "A1_BankName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "A1_BankSwift";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "A1_BankBsb";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo5.ColumnName = "A1_BankAccount";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.ColumnName = "A1_IBANNumber";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "A1_RN_NKCountryCode";
			zDateEditColumnStyleInfo1.ColumnName = "A1_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("2b5d3d93-7011-4e72-bfb1-089fabab3add", "Audit Details");
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			((IOverridablePropertyDescriptor)zDateEditColumnStyleInfo1).PropertyDescriptor = new LocalAuditTimePropertyDescriptor(AccAPAccountDetailsSchema.A1_SystemCreateTimeUtc);
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "A1_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo2.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("2b5d3d93-7011-4e72-bfb1-089fabab3add", "Audit Details");
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			((IOverridablePropertyDescriptor)zDateEditColumnStyleInfo2).PropertyDescriptor = new LocalAuditTimePropertyDescriptor(AccAPAccountDetailsSchema.A1_SystemLastEditTimeUtc);
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "A1_SystemCreateUser";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("2b5d3d93-7011-4e72-bfb1-089fabab3add", "Audit Details");
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "A1_SystemLastEditUser";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("2b5d3d93-7011-4e72-bfb1-089fabab3add", "Audit Details");
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.CopySelectedRowsAllowed = true;
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "af88fe9b-f289-4cca-82cd-e7c8b743c7dc";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "zGrid1";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 151, true);
			this.grid.TabIndex = 10;
			// 
			// ARAccountDetailsGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.grid);
			this.Name = "ARAccountDetailsGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid grid;
	}
}
