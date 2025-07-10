using Enterprise.Rating.GUI.RateChooser.ViewModel;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class LegChargesControl
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
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblLeg = new Enterprise.ZArchitecture.ZLabel();
            this.pnlItemsContainer = new CargoWise.Windows.UI.KFlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.LegChargesViewModel);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTop.Controls.Add(this.lblLeg);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 25, true);
            this.pnlTop.TabIndex = 0;
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
            this.lblLeg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 25, true);
            this.lblLeg.TabIndex = 1;
            this.lblLeg.Text = "Leg";
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoSize = true;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.pnlItemsContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.pnlItemsContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 1, true);
            this.pnlItemsContainer.TabIndex = 3;
            // 
            // LegChargesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.pnlTop);
            this.Name = "LegChargesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 340, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.ZLabel lblLeg;
		private CargoWise.Windows.UI.KFlowLayoutPanel pnlItemsContainer;
	}
}
