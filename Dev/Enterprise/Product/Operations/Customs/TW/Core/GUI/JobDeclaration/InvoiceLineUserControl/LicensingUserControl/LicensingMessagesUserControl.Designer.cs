namespace Enterprise.Customs.TW.GUI
{
	//merge this class
	partial class LicensingMessagesUserControl
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
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ControllingAgencyGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ControllingAgencyGrid)).BeginInit();
            this.ControllingAgencyGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
            // 
            // ControllingAgencyGrid
            // 
            this.ControllingAgencyGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ControllingAgencyGrid, "FilteredInvoiceLines.InvoiceLineLinkControllingMsgHeaders");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).IsLinkedCMHeader)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).Sequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).FunctionalReferenceID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).CertificateType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).BusinessType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).MessageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).MessageTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).ControllingAgency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.InvoiceLineLinkControllingMsgHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InvoiceLineLinkControllingMsgHeaders)).SyncRoot)).ControllingAgencyDescription)));
            this.ControllingAgencyGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo1.ColumnName = "IsLinkedCMHeader";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
            zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "FunctionalReferenceID";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
            zTextBoxColumnStyleInfo2.ColumnName = "CertificateType";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "BusinessType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "MessageType";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
            zTextBoxColumnStyleInfo5.ColumnName = "MessageTypeDescription";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(182);
            zTextBoxColumnStyleInfo6.ColumnName = "ControllingAgency";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo7.ColumnName = "ControllingAgencyDescription";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
            this.ControllingAgencyGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.ControllingAgencyGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ControllingAgencyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ControllingAgencyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ControllingAgencyGrid.GridId = "0c3615f7-af88-4403-a59b-aa1f898232d4";
            this.ControllingAgencyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ControllingAgencyGrid.LayoutKey = "ControllingAgencyGrid";
            this.ControllingAgencyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ControllingAgencyGrid.Name = "ControllingAgencyGrid";
            this.ControllingAgencyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 286, true);
            this.ControllingAgencyGrid.TabIndex = 1;
            // 
            // LicensingMessagesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ControllingAgencyGrid);
            this.Name = "LicensingMessagesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 286, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ControllingAgencyGrid)).EndInit();
            this.ControllingAgencyGrid.ResumeLayout(false);
            this.ControllingAgencyGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ControllingAgencyGrid;
	}
}
