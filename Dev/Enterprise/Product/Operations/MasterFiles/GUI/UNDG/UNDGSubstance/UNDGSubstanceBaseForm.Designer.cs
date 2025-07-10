namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class UNDGSubstanceBaseForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new protected virtual void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));

			//
			// UNDGSubstanceBaseForm
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "UNDGSubstanceBaseForm";

			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			undgSubstanceControl = GetNewUNDGSubstanceUserControl();

			MainTabPage.SuspendLayout();
			MainTabPage.Controls.Add(undgSubstanceControl);
			//
			// UNDGSubstanceControl
			//
			BindingSource.SetBindingMember(undgSubstanceControl, ".");
			undgSubstanceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			undgSubstanceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			undgSubstanceControl.TabIndex = 0;

			MainTabPage.ResumeLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZUserControl undgSubstanceControl;
	}
}
