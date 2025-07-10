namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ForwardingBookingServicesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ServicesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ServicesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.ServicesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// ServicesGroupBox
			// 
			this.ServicesGroupBox.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|5dac4840-12e9-47a6-b0d5-f2e92e1f9d48", "Services");
			this.ServicesGroupBox.Controls.Add(this.ServicesGrid);
			this.ServicesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesGroupBox.Name = "ServicesGroupBox";
			this.ServicesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 160, true);
			this.ServicesGroupBox.TabIndex = 23;
			this.ServicesGroupBox.TabStop = false;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "Booking+DocsAndCartage+Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_Calc_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_OH_Contractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_ServiceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_Calc_LocationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ServiceProviderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_ServiceNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_ServiceId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.DocsAndCartage.Services)).SyncRoot)).ES_ExternalServiceId)));
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|494c8bf0-e9f2-4b87-97ac-a8a1a3242b7c", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ES_Calc_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ES_OH_Contractor";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ES_ServiceCount";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.MaxValue = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ES_Calc_LocationCode";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("NVOCCAdditionalDetailsControl|31afebf6-c40e-492a-a38d-43527a050e48", "Provider", "Service Provider", "Service Location Provider", "");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ServiceProviderPK";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ES_Duration";
			zTimeEditExColumnStyleInfo1.IsVisible = false;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ES_ServiceNote";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "ES_ServiceId";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "ES_ExternalServiceId";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGrid.GridId = "42384f97-4272-4356-ba9a-01dd81259dcb";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 141, true);
			this.ServicesGrid.TabIndex = 0;
			// 
			// ForwardingBookingServicesControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ServicesGroupBox);
			this.Name = "ForwardingBookingServicesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServicesGroupBox.ResumeLayout(false);
			this.ServicesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ServicesGrid.ResumeLayout(false);
			this.ServicesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox ServicesGroupBox;
		internal ZArchitecture.ZGrid ServicesGrid;
		#endregion
	}
}
