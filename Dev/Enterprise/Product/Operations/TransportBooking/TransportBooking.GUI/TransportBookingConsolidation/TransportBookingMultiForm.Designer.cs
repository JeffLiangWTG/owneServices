namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingMultiForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.transportBookingsControl = new Enterprise.TransportBookings.GUI.TransportBookingsControl();
			this.AdditionalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalReferencesControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.AdditionalReferencesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 619, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AdditionalReferencesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.transportBookingsControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 592, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 592, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 619, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingConsolidation);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 592, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// transportBookingsControl
			// 
			this.transportBookingsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.transportBookingsControl, ".");
			this.transportBookingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transportBookingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.transportBookingsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 0, true);
			this.transportBookingsControl.Name = "transportBookingsControl";
			this.transportBookingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 592, true);
			this.transportBookingsControl.TabIndex = 0;
			// 
			// AdditionalReferencesTabPage
			// 
			this.AdditionalReferencesTabPage.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("a42dfe78-b142-4506-8f21-bfef0c118fbd", "Additional References");
			this.AdditionalReferencesTabPage.Controls.Add(this.AdditionalReferencesControl);
			this.AdditionalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.AdditionalReferencesTabPage.Name = "AdditionalReferencesTabPage";
			this.AdditionalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 597, true);
			this.AdditionalReferencesTabPage.TabIndex = 4;
			this.AdditionalReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalReferencesControl
			// 
			this.AdditionalReferencesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalReferencesControl, "AdditionalReferenceNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.TransportBookings.Business.DtbBookingConsolidation)(null)).AdditionalReferenceNumbers)));
			this.AdditionalReferencesControl.DisplayDetailPanel = false;
			this.AdditionalReferencesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalReferencesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalReferencesControl.Name = "AdditionalReferencesControl";
			this.AdditionalReferencesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 590, true);
			this.AdditionalReferencesControl.TabIndex = 0;
			// 
			// TransportBookingMultiForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 675, true);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingConsolidation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 713, true);
			this.Name = "TransportBookingMultiForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "TransportBookingMultiForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal TransportBookingsControl transportBookingsControl;
		private ZArchitecture.GUI.ZTabPage AdditionalReferencesTabPage;
		private MasterFiles.GUI.NumbersControl AdditionalReferencesControl;
	}
}
