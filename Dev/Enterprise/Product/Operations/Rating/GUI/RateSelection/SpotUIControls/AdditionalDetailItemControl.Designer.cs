namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class AdditionalDetailItemControl
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
            this.lblCode = new Enterprise.ZArchitecture.ZLabel();
            this.lblDescription = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.AdditionalDetailsViewModel);
            // 
            // lblCode
            // 
            this.BindingSource.SetBindingMember(this.lblCode, "Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.AdditionalDetailsViewModel)(null)).Code)));
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 19, true);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "Code";
            // 
            // lblDescription
            // 
            this.BindingSource.SetBindingMember(this.lblDescription, "Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.AdditionalDetailsViewModel)(null)).Description)));
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblDescription.IsFontBold = true;
            this.lblDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 0, true);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 19, true);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AdditionalDetailItemControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.lblCode);
            this.Controls.Add(this.lblDescription);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "AdditionalDetailItemControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel lblCode;
		private ZArchitecture.ZLabel lblDescription;
	}
}
