namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class ChargesChargeGroupTemplate
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
            this.lblChargeGroup = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotal = new Enterprise.ZArchitecture.ZLabel();
            this.pnlContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel.ChargeGroupView);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Silver;
            this.pnlTop.Controls.Add(this.lblChargeGroup);
            this.pnlTop.Controls.Add(this.lblTotal);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            this.pnlTop.TabIndex = 1;
            // 
            // lblChargeGroup
            // 
            this.BindingSource.SetBindingMember(this.lblChargeGroup, "ChargeGroup");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel.ChargeGroupView)(null)).ChargeGroup)));
            this.lblChargeGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChargeGroup.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Small | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblChargeGroup.IsFontBold = true;
            this.lblChargeGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblChargeGroup.Name = "lblChargeGroup";
            this.lblChargeGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
            this.lblChargeGroup.TabIndex = 4;
            this.lblChargeGroup.Text = "Charge Group";
            // 
            // lblTotal
            // 
            this.lblTotal.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotal.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Small | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTotal.IsFontBold = true;
            this.lblTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 0, true);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
            this.lblTotal.TabIndex = 3;
            this.lblTotal.Text = "Total";
			this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pnlContainer
			// 
			this.pnlContainer.AutoSize = true;
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 10, true);
            this.pnlContainer.TabIndex = 2;
            // 
            // ChargesChargeGroupTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.pnlContainer);
            this.Controls.Add(this.pnlTop);
            this.Name = "ChargesChargeGroupTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 25, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.ZLabel lblTotal;
		private ZArchitecture.ZLabel lblChargeGroup;
		private ZArchitecture.GUI.ZPanel pnlContainer;
	}
}
