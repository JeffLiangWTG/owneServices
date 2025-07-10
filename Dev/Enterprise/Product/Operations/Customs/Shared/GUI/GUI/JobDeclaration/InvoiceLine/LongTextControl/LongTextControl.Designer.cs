namespace Enterprise.Customs.GUI
{
	partial class LongTextControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LongTextTextBox = new Enterprise.ZArchitecture.ZTextBox.Bare();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MoreButton
			// 
			this.MoreButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0162f1e5-422b-483e-8efc-c0cbf3a90888", "More ...");
			this.MoreButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.MoreButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 0, true);
			this.MoreButton.Name = "MoreButton";
			this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.MoreButton.TabIndex = 1;
			this.MoreButton.ToolTipCaption = null;
			// 
			// LongTextTextBox
			// 
			this.LongTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LongTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LongTextTextBox.Name = "LongTextTextBox";
			this.LongTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.LongTextTextBox.TabIndex = 0;
			// 
			// LongTextControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LongTextTextBox);
			this.Controls.Add(this.MoreButton);
			this.Name = "LongTextControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

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

		#endregion

	}
}
