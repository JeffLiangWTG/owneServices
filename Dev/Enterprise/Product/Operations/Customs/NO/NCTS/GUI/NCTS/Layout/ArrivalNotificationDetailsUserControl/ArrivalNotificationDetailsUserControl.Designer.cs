namespace Enterprise.Customs.NO.NCTS.GUI
{
	partial class ArrivalNotificationDetailsUserControl
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
            this.GoodsRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.NCTS.Business.NctsHeader);
			// 
			// GoodsRegistrationNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.GoodsRegistrationNumberTextBox, "ArrivalMovementHeader.GoodsRegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsRegistrationNumber)));
			this.GoodsRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 20, true);
            this.GoodsRegistrationNumberTextBox.Name = "GoodsRegistrationNumberTextBox";
            this.GoodsRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
            this.GoodsRegistrationNumberTextBox.TabIndex = 1;
            // 
            // ArrivalNotificationDetailsUserControl
            // 
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GoodsRegistrationNumberTextBox);
            this.Name = "ArrivalNotificationDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 60, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox GoodsRegistrationNumberTextBox;
	}
}
