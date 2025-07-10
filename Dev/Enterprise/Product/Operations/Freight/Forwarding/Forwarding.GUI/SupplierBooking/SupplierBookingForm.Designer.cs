
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class SupplierBookingForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldsUserControl = new SupplierBookingCustomFieldsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CustomFieldsUserControl.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 452, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 425, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 425, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 425, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 452, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBooking);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SupplierBookingForm|6a7ce1ab-900a-4ba2-aeda-f8d6ebe70a4f", "Workflow & Tracking");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 4;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// CustomFieldsTabPage
			//
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SupplierBookingUserControl|b77f8ee3-21b7-4309-89f9-327911e88efd", "Additional Details");
			this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsUserControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 500, true);
			this.CustomFieldsTabPage.TabIndex = 1;
			// 
			// CustomFieldsUserControl
			// 
			this.CustomFieldsUserControl.AllowDrop = true;
			this.CustomFieldsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 49, true);
			this.CustomFieldsUserControl.Name = "CustomFieldsUserControl";
			this.CustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 150, true);
			this.CustomFieldsUserControl.TabIndex = 0;
			// 
			// SupplierBookingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 508, true);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBooking);
			this.Name = "SupplierBookingForm";
			this.ShouldSerializeTabPageMethods = true;
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.CustomFieldsUserControl.ResumeLayout(true);
			this.CustomFieldsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private SupplierBookingCustomFieldsUserControl CustomFieldsUserControl;
		#endregion

		private SupplierBookingUserControl supplierBookingUserControl;
	}
}
