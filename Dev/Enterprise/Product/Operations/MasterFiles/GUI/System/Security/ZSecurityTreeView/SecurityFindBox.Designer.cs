using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SecurityFindBox
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
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
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
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox.Bare();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HumanReadableNameTextBox = new Enterprise.ZArchitecture.ZTextBox.Bare();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.CodeTextBox.TabIndex = 0;
			this.CodeTextBox.Validated += new System.EventHandler(this.CodeTextBox_Validated);
			// 
			// SelectButton
			// 
			this.SelectButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 0, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.SelectButton.TabIndex = 1;
			this.SelectButton.Text = "...";
			this.SelectButton.UseVisualStyleBackColor = true;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// HumanReadableNameTextBox
			// 
			this.HumanReadableNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HumanReadableNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 0, true);
			this.HumanReadableNameTextBox.Name = "HumanReadableNameTextBox";
			this.HumanReadableNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.HumanReadableNameTextBox.ReadOnly = true;
			this.HumanReadableNameTextBox.TabIndex = 2;
			// 
			// SecurityFindBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HumanReadableNameTextBox);
			this.Controls.Add(this.SelectButton);
			this.Controls.Add(this.CodeTextBox);
			this.Name = "SecurityFindBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZButton SelectButton;
		internal ZTextBox.Bare CodeTextBox;
		internal ZTextBox.Bare HumanReadableNameTextBox;
	}
}
