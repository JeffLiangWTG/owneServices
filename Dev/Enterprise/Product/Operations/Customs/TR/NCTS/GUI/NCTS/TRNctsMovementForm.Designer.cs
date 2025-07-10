namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class TRNctsMovementForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ManifestsToOpenTabPageUserControl = new Enterprise.Customs.TR.NCTS.GUI.ManifestsToOpenUserControl();
			this.ManifestsToOpenTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.ManifestsToOpenTabPage.SuspendLayout();
			this.ManifestsToOpenTabPageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 514, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 487, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 487, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 487, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 514, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 24, true);
			// 
			// ManifestsToOpenTabPage
			// 
			this.ManifestsToOpenTabPage.Controls.Add(this.ManifestsToOpenTabPageUserControl);
			this.ManifestsToOpenTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.ManifestsToOpenTabPage.Name = "ManifestsToOpenTabPage";
			this.ManifestsToOpenTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ManifestsToOpenTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 548, true);
			this.ManifestsToOpenTabPage.TabIndex = 3;
			this.ManifestsToOpenTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("A6B74648-1FFB-4E68-B5B1-35BF97F0B25D", "Manifests To Open");
			this.ManifestsToOpenTabPage.UseVisualStyleBackColor = true;

			// 
			// ManifestsToOpenTabPageUserControl
			// 
			this.ManifestsToOpenTabPageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestsToOpenTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ManifestsToOpenTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsToOpenTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ManifestsToOpenTabPageUserControl.Name = "ManifestsToOpenTabPageUserControl";
			this.ManifestsToOpenTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 542, true);
			this.ManifestsToOpenTabPageUserControl.TabIndex = 0;

			// 
			// TRNctsMovementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 800, true);
			this.Name = "TRNctsMovementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "TRNctsMovementForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ManifestsToOpenTabPage.ResumeLayout(false);
			this.ManifestsToOpenTabPage.PerformLayout();
			this.ManifestsToOpenTabPageUserControl.ResumeLayout(true);
			this.ManifestsToOpenTabPageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		ZArchitecture.GUI.ZTabPage ManifestsToOpenTabPage;
		ManifestsToOpenUserControl ManifestsToOpenTabPageUserControl;
	}
}
