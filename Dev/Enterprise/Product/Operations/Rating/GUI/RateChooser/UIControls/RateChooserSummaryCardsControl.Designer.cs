namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class RateChooserSummaryCardsControl
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
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooserViewModel);
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoScroll = true;
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 147, true);
            this.pnlItemsContainer.TabIndex = 0;
            // 
            // RateChooserSummaryCardsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Name = "RateChooserSummaryCardsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 147, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
	}
}
