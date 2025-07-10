namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class LegChargeSummaryControl
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
            this.pnlLeg = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblLeg = new Enterprise.ZArchitecture.ZLabel();
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlLeg.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel);
            // 
            // pnlLeg
            // 
            this.pnlLeg.Controls.Add(this.lblLeg);
            this.pnlLeg.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeg.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlLeg.Name = "pnlLeg";
            this.pnlLeg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 20, true);
            this.pnlLeg.TabIndex = 0;
            // 
            // lblLeg
            // 
            this.BindingSource.SetBindingMember(this.lblLeg, "Leg");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel)(null)).Leg)));
            this.lblLeg.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLeg.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblLeg.IsFontBold = true;
            this.lblLeg.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblLeg.Name = "lblLeg";
            this.lblLeg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
            this.lblLeg.TabIndex = 3;
            this.lblLeg.Text = "Leg";
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoSize = true;
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 0, true);
            this.pnlItemsContainer.TabIndex = 1;
            // 
            // LegChargeSummaryControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.pnlLeg);
            this.Name = "LegChargeSummaryControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 22, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlLeg.ResumeLayout(false);
            this.pnlLeg.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlLeg;
		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
		private ZArchitecture.ZLabel lblLeg;
	}
}
