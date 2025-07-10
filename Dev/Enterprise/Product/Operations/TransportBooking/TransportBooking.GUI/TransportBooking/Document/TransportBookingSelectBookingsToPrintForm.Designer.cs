namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingSelectBookingsToPrintForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UnSelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BookingsGrid = new Enterprise.TransportBookings.GUI.ZGridThatIsNotModifyingHasChanges();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BookingsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 384, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DocumentDtbBookingCollection);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.UnSelectAllButton);
			this.ButtonsPanel.Controls.Add(this.SelectAllButton);
			this.ButtonsPanel.Controls.Add(this.PrintButton);
			this.ButtonsPanel.Controls.Add(this.CancelPrintingButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 351, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 33, true);
			this.ButtonsPanel.TabIndex = 5;
			// 
			// UnSelectAllButton
			// 
			this.UnSelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnSelectAllButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|55301b45-aa9f-4225-bb93-90bdda0a3997", "Un-Select All");
			this.UnSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 4, true);
			this.UnSelectAllButton.Name = "UnSelectAllButton";
			this.UnSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 26, true);
			this.UnSelectAllButton.TabIndex = 1;
			this.UnSelectAllButton.UseVisualStyleBackColor = true;
			this.UnSelectAllButton.Click += new System.EventHandler(this.UnSelectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|358fe072-dc97-405a-a6d4-946f4b739fab", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 26, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|eb7bcc59-e13a-401f-bb7e-063456112fcf", "Deliver");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 4, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 26, true);
			this.PrintButton.TabIndex = 2;
			this.PrintButton.UseVisualStyleBackColor = true;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// CancelPrintingButton
			// 
			this.CancelPrintingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintingButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|70d8f45a-7189-4788-a1aa-84fb9872cdb2", "Cancel");
			this.CancelPrintingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 4, true);
			this.CancelPrintingButton.Name = "CancelPrintingButton";
			this.CancelPrintingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 26, true);
			this.CancelPrintingButton.TabIndex = 3;
			this.CancelPrintingButton.UseVisualStyleBackColor = true;
			// 
			// BookingsGrid
			// 
			this.BookingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BookingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.KM_JobID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.KM_KT_NKBookingTemplate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.KM_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.KM_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.KM_TransportReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.Address.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.Address.OrganisationDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).Booking.Address.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DocumentDtbBooking)(null)).IncludeInDelivery)));
			this.BookingsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Booking+KM_JobID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "Booking+KM_KT_NKBookingTemplate";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "Booking+KM_Direction";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "Booking+KM_Description";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ColumnName = "Booking+KM_TransportReference";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|be47b894-096c-4a2a-8cad-85d6da72b167", "Transport Co", "Transport Company", "");
			zMultiControlColumnStyleInfo1.ColumnName = "Booking+Address+OrganisationNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "Booking+Address+OrganisationDataFieldType";
			zMultiControlColumnStyleInfo1.IsReadOnly = true;
			zGuidDropEditColumnStyleInfo1.ColumnName = "Booking+Address+E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.IsReadOnly = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInDelivery";
			this.BookingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BookingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BookingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BookingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BookingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BookingsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.BookingsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.BookingsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BookingsGrid.CopySelectedRowsAllowed = true;
			this.BookingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingsGrid.GridId = "508c8a19-8713-49d1-ac35-d8b56d23753d";
			this.BookingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BookingsGrid.LayoutKey = "BookingsGrid";
			this.BookingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingsGrid.Name = "BookingsGrid";
			this.BookingsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.BookingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 351, true);
			this.BookingsGrid.TabIndex = 6;
			// 
			// TransportBookingSelectBookingsToPrintForm
			// 
			this.AcceptButton = this.PrintButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelPrintingButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingSelectBookingsToPrintForm|C068B565-2ACC-45F0-97DF-4583670FFB0E", "Select Bookings to Deliver");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 408, true);
			this.Controls.Add(this.BookingsGrid);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.DocumentDtbBookingCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 200, true);
			this.Name = "TransportBookingSelectBookingsToPrintForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.BookingsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BookingsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZButton PrintButton;
		private ZArchitecture.GUI.ZButton CancelPrintingButton;
		private ZGridThatIsNotModifyingHasChanges BookingsGrid;
		private ZArchitecture.GUI.ZButton UnSelectAllButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
