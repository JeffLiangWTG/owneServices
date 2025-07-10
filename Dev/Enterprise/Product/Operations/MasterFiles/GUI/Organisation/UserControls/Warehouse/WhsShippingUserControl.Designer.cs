using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WhsShippingUserControl
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
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.WhsShippingAccountNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WhsShippingAccountNumbersGrid)).BeginInit();
			this.WhsShippingAccountNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// WhsShippingAccountNumbersGrid
			// 
			this.WhsShippingAccountNumbersGrid.AllowNavigation = false;
			this.WhsShippingAccountNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WhsShippingAccountNumbersGrid, "OrgWhsClientAccountAssociations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_OAN_CarrierAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).CarrierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).CarrierName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_WW_Warehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_WSH_SalesChannel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_BillingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_OAN_BillToCarrierAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).BillToCarrierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).BillToCarrierName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).OWC_OAN_DutyBillToCarrierAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).DutyBillToCarrierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWhsClientAccountAssociation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWhsClientAccountAssociations)).SyncRoot)).DutyBillToCarrierName)));
			this.WhsShippingAccountNumbersGrid.CaptionVisible = false;
			zAddressDropEditColumnStyleInfo1.ColumnName = "OWC_OAN_CarrierAccount";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CarrierCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CarrierName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OWC_WW_Warehouse";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OWC_WSH_SalesChannel";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "OWC_BillingType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.ColumnName = "OWC_OAN_BillToCarrierAccount";
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "BillToCarrierCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "BillToCarrierName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo3.ColumnName = "OWC_OAN_DutyBillToCarrierAccount";
			zAddressDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "DutyBillToCarrierCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "DutyBillToCarrierName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo3);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.WhsShippingAccountNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.WhsShippingAccountNumbersGrid.GridId = "321ced68-d51b-aac3-a88e-41f61badc025";
			this.WhsShippingAccountNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WhsShippingAccountNumbersGrid.LayoutKey = "AccountNumbersGrid";
			this.WhsShippingAccountNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.WhsShippingAccountNumbersGrid.Name = "WhsShippingAccountNumbersGrid";
			this.WhsShippingAccountNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 300, true);
			this.WhsShippingAccountNumbersGrid.TabIndex = 4;
			// 
			// WhsShippingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WhsShippingAccountNumbersGrid);
			this.Name = "WhsShippingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 309, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WhsShippingAccountNumbersGrid)).EndInit();
			this.WhsShippingAccountNumbersGrid.ResumeLayout(false);
			this.WhsShippingAccountNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid WhsShippingAccountNumbersGrid;
	}
}
