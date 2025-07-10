namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusMapForm
	{
		private RefCusMapUserControl RefCusMapUserControl;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.RefCusMapUserControl = new Enterprise.Customs.Universal.GUI.RefCusMapUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RefCusMapUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 234, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.RefCusMapUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 164, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 164, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 207, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 234, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusMapCombined);
			// 
			// RefCusMapUserControl
			// 
			this.RefCusMapUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefCusMapUserControl, ".");
			this.RefCusMapUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefCusMapUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefCusMapUserControl.Name = "RefCusMapUserControl";
			this.RefCusMapUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 164, true);
			this.RefCusMapUserControl.TabIndex = 1;
			this.RefCusMapUserControl.TabStop = true;
			// 
			// RefCusMapForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("09e2e5ba-b8cd-4e76-bcc9-14bd0e89ce5e", "Global Data Maps");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 290, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusMapCombined);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RefCusMapForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "RefCusMapForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RefCusMapUserControl.ResumeLayout(true);
			this.RefCusMapUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
