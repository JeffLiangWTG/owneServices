namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class ChargeToggleButton
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
            this.lblChargeCode = new Enterprise.ZArchitecture.ZLabel();
            this.picErrorWarning = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picErrorWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel);
            // 
            // lblChargeCode
            // 
            this.lblChargeCode.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblChargeCode, "ChargeCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel)(null)).ChargeCode)));
            this.lblChargeCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblChargeCode.IsFontBold = true;
            this.lblChargeCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
            this.lblChargeCode.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.lblChargeCode.Name = "lblChargeCode";
            this.lblChargeCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 13, true);
            this.lblChargeCode.TabIndex = 0;
            this.lblChargeCode.Text = "FRT";
            this.lblChargeCode.SizeChanged += new System.EventHandler(this.lblChargeCode_SizeChanged);
            this.lblChargeCode.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lblChargeCode_MouseClick);
            this.lblChargeCode.MouseHover += new System.EventHandler(this.lblChargeCode_MouseHover);
            // 
            // picErrorWarning
            // 
            this.picErrorWarning.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
            this.picErrorWarning.Name = "picErrorWarning";
            this.picErrorWarning.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
            this.picErrorWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picErrorWarning.TabIndex = 1;
            this.picErrorWarning.TabStop = false;
            this.picErrorWarning.VisibleChanged += new System.EventHandler(this.picErrorWarning_VisibleChanged);
            // 
            // ChargeToggleButton
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.picErrorWarning);
            this.Controls.Add(this.lblChargeCode);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "ChargeToggleButton";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picErrorWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel lblChargeCode;
		protected ZArchitecture.GUI.ZPictureBox picErrorWarning;
	}
}
