using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class CloseProjectPopupForm : BaseProjectPopupForm
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
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.BindingSource.SetBindingMember(this.MessageLabel, "MessageCloseOrCancel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.ProjectAction)(null)).MessageCloseOrCancel)));
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 23, true);
			this.MessageLabel.Text = "";
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 266, true);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 266, true);
			// 
			// Comment
			// 
			this.Comment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 166, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 24, true);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEdit1, "CloseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.ProjectAction)(null)).CloseType)));
			this.zDropEdit1.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("b72c3d1f-78b1-423a-90ed-3dfc478d61c0", "Method");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 44, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.zDropEdit1.TabIndex = 2;
			// 
			// CloseProjectPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("04838a6f-3e85-4a84-886a-be69a78a7762", "Close/Cancel Project");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 319, true);
			this.Controls.Add(this.zDropEdit1);
			this.Name = "CloseProjectPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.Comment, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZDropEdit zDropEdit1;

		#endregion
	}
}