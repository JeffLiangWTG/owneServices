namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class RoutesControl
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
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.IHasTransportLegs);
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoScroll = true;
            this.pnlItemsContainer.AutoSize = true;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 0, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 0, true);
            this.pnlItemsContainer.TabIndex = 0;
            // 
            // RoutesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Name = "RoutesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 123, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
	}
}
