using Enterprise.ZArchitecture.Modules;
namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MultiBookingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SplitBookingDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdditionalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RoutingScheduleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.relatedJobsTabPage = new Enterprise.ZArchitecture.GUI.RelatedJobsTabPage();
			this.AccountingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AccountingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.RoutingScheduleTabPage);
			this.MainTabControl.Controls.Add(this.AdditionalReferencesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.relatedJobsTabPage);
			this.MainTabControl.Controls.Add(this.AccountingTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1471, 630, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RoutingScheduleTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.relatedJobsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AccountingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AdditionalReferencesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 603, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1471, 630, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.SplitBookingDescriptionLabel);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.MultiBookingButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1471, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			// 
			// RoutingScheduleTabPage
			// 
			this.RoutingScheduleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RoutingScheduleTabPage.Name = "RoutingScheduleTabPage";
			this.RoutingScheduleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RoutingScheduleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.RoutingScheduleTabPage.TabIndex = 2;
			this.RoutingScheduleTabPage.Text = ControllerIDs.Routing.Name;
			this.RoutingScheduleTabPage.UseVisualStyleBackColor = true;
			this.RoutingScheduleTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RoutingScheduleTabPage_InitializeTab));
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// MultiBookingButton
			// 
			this.MultiBookingButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("5ed16ffe-79e0-421d-9430-ad1d14de25e2", "Multi Booking");
			this.MultiBookingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.MultiBookingButton.Name = "MultiBookingButton";
			this.MultiBookingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 22, true);
			this.MultiBookingButton.TabIndex = 0;
			this.MultiBookingButton.UseVisualStyleBackColor = true;
			this.MultiBookingButton.Click += new System.EventHandler(this.MultiBookingButton_Click);
			// 
			// SplitBookingDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.SplitBookingDescriptionLabel, "SplitBookingDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).SplitBookingDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SplitBookingDescriptionLabel, false);
			this.SplitBookingDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 6, true);
			this.SplitBookingDescriptionLabel.Name = "SplitBookingDescriptionLabel";
			this.SplitBookingDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 18, true);
			this.SplitBookingDescriptionLabel.TabIndex = 1;
			// 
			// AdditionalReferencesTabPage
			// 
			this.AdditionalReferencesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AdditionalReferencesTabPage.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("18abe398-d456-4ce1-a3b2-861527b38cc8", "Additional Details");
			this.AdditionalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalReferencesTabPage.Name = "AdditionalReferencesTabPage";
			this.AdditionalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 603, true);
			this.AdditionalReferencesTabPage.TabIndex = 4;
			this.AdditionalReferencesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AdditionalReferencesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.TransportBookings.Business.TransportBookingAdditionalReferenceCollection)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).AdditionalReferencesForBinding)));
			// 
			// relatedJobsTabPage
			// 
			this.relatedJobsTabPage.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("1148dccd-283e-474d-9a22-aad191f3072f", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.RelatedJobCollection)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).RelatedJobs)));
			this.relatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.relatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.relatedJobsTabPage.Name = "relatedJobsTabPage";
			this.relatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.relatedJobsTabPage.TabIndex = 4;
			this.relatedJobsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.relatedJobsTabPage_InitializeTab));
			// 
			// AccountingTabPage
			//
			this.AccountingTabPage.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("e808998d-c4d8-4aca-96e9-a9a70b6f334e", "Accounting");
			this.AccountingTabPage.Controls.Add(this.AccountingTabControl);
			this.AccountingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountingTabPage.Name = "AccountingTabPage";
			this.AccountingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 603, true);
			this.AccountingTabPage.TabIndex = 5;
			this.AccountingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AccountingTabPage_InitializeTab));
			//
			// AccountingTabControl
			//
			this.AccountingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AccountingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AccountingTabControl.Name = "AccountingTabControl";
			this.AccountingTabControl.SelectedIndex = 0;
			this.AccountingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1463, 603, true);
			this.AccountingTabControl.TabIndex = 0;
			//
			// TransportBookingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1471, 686, true);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "TransportBookingForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "TransportBookingForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private BookingControl currentBookingPanel;
		private ZArchitecture.GUI.ZButton MultiBookingButton;
		private ZArchitecture.ZLabel SplitBookingDescriptionLabel;
		private ZArchitecture.GUI.ZTabPage AdditionalReferencesTabPage;
		private TransportBookingsAdditionalReferencesUserControl AdditionalReferenceControl;
		private ZArchitecture.GUI.RelatedJobsTabPage relatedJobsTabPage;
		private ZArchitecture.GUI.ZTabPage AccountingTabPage;
		private ZArchitecture.GUI.ZTabControl AccountingTabControl;
		private ScheduleControl SailingDetailsControl;
		private ZArchitecture.GUI.ZTabPage RoutingScheduleTabPage;
		internal DtbInstructionViewsControl InstructionViewsControl;
	}
}
