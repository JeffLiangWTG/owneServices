using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.MasterFiles.GUI
{
	partial class TaskOwnerPasswordRequestForm
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
		private void InitializeComponent()
		{
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.btnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 60, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 20, true);
			this.PasswordTextBox.TabIndex = 1;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			this.PasswordTextBox.Extensions.Get<CargoWise.Windows.UI.ILabelCaptionRenderer>().Visible = false;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 86, true);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnOK.TabIndex = 2;
			this.btnOK.CaptionResourceString = Res.GetData("TaskOwnerPasswordRequestForm|OK", "OK");
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 86, true);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.CaptionResourceString = Res.GetData("TaskOwnerPasswordRequestForm|Cancel", "Cancel");
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 48, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.CaptionResourceString = Res.GetData("TaskOwnerPasswordRequestForm|018b65f8-8527-414e-9e6e-4c8be90a4dc7", "You are not the owner of this task. To close a task assigned to another user, they will need to enter their password.");
			// 
			// TaskOwnerPasswordRequestForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 118, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.PasswordTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "TaskOwnerPasswordRequestForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton btnOK;
		private Enterprise.ZArchitecture.GUI.ZButton btnCancel;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}