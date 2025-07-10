using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class BookingTermsItemTemplate
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
            this.pnlMain = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblFee = new Enterprise.ZArchitecture.ZLabel();
            this.lblName = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingTermViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lblCurrency);
            this.pnlMain.Controls.Add(this.lblFee);
            this.pnlMain.Controls.Add(this.lblName);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 19, true);
            this.pnlMain.TabIndex = 0;
            // 
            // lblCurrency
            // 
            this.BindingSource.SetBindingMember(this.lblCurrency, "Currency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingTermViewModel)(null)).Currency)));
            this.lblCurrency.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 0, true);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 19, true);
            this.lblCurrency.TabIndex = 2;
            this.lblCurrency.Text = "Currency";
            // 
            // lblFee
            // 
            this.BindingSource.SetBindingMember(this.lblFee, "Fee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingTermViewModel)(null)).Fee)));
            this.lblFee.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 0, true);
            this.lblFee.Name = "lblFee";
            this.lblFee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 19, true);
            this.lblFee.TabIndex = 1;
            this.lblFee.Text = "Fee";
            // 
            // lblName
            // 
            this.BindingSource.SetBindingMember(this.lblName, "Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingTermViewModel)(null)).Name)));
            this.lblName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblName.Name = "lblName";
            this.lblName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 19, true);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name";
            // 
            // BookingTermsItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlMain);
            this.Name = "BookingTermsItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlMain;
		private ZArchitecture.ZLabel lblName;
		private ZArchitecture.ZLabel lblFee;
		private ZArchitecture.ZLabel lblCurrency;
	}
}
