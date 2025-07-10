namespace Enterprise.ProcessManagement.GUI
{
	partial class ReOpenProjectPopupForm : BaseProjectPopupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.BindingSource.SetBindingMember(this.MessageLabel, "MessageReOpen");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectAction)(null)).MessageReOpen)));
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 28, true);
			this.MessageLabel.Text = "";
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 266, true);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 266, true);
			// 
			// Comment
			// 
			this.Comment.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 59, true);
			this.Comment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 201, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 24, true);
			// 
			// ReOpenProjectPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("D80EF00E-3E9F-47A4-B6AD-E1D5662B418B", "Re-Open Project");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 319, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.Name = "ReOpenProjectPopupForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}