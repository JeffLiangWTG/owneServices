namespace Enterprise.Rating.GUI.AutoRating
{
	partial class NamedAccountMessageBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		new System.ComponentModel.IContainer components = null;

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
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Button1
			// 
			this.Button1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("5c01eddc-3729-4fb4-a577-017b5a6cace8", "&Apply Rates and Named Account");
			this.Button1.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.Button1.IsCaptionOverridden = true;
			this.Button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 82, true);
			this.Button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 23, true);
			this.Button1.Text = Enterprise.Rating.GUI.Res.GetData("5c01eddc-3729-4fb4-a577-017b5a6cace8", "&Apply Rates and Named Account").Caption;
			// 
			// Button2
			// 
			this.Button2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("e84bce71-1694-4803-8df5-ec112be5d014", "Apply &Rates Only");
			this.Button2.DialogResult = System.Windows.Forms.DialogResult.No;
			this.Button2.IsCaptionOverridden = true;
			this.Button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 82, true);
			this.Button2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.Button2.Text = Enterprise.Rating.GUI.Res.GetData("e84bce71-1694-4803-8df5-ec112be5d014", "Apply &Rates Only").Caption;
			// 
			// Button3
			// 
			this.Button3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("d56bc18a-9338-43de-82b8-135b7e67e6d3", "&Skip Rates and Continue");
			this.Button3.DialogResult = System.Windows.Forms.DialogResult.Ignore;
			this.Button3.IsCaptionOverridden = true;
			this.Button3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 82, true);
			this.Button3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 23, true);
			this.Button3.Text = Enterprise.Rating.GUI.Res.GetData("d56bc18a-9338-43de-82b8-135b7e67e6d3", "&Skip Rates and Continue").Caption;
			// 
			// TextBox
			// 
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 63, true);
			// 
			// NamedAccountMessageBox
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 113, true);
			this.Name = "NamedAccountMessageBox";
			this.Text = "Named Account";
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
