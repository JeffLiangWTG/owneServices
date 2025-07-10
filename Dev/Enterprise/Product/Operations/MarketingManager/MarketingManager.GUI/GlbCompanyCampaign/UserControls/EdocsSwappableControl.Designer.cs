using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	partial class EdocsSwappableControl
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

			if (eDocsPlugIn != null)
			{
				eDocsPlugIn.Dispose();
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
			this.NoContactSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// NoContactSelectedLabel
			// 
			this.NoContactSelectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NoContactSelectedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NoContactSelectedLabel.Text = Res.GetString("{a73f5b0f-3d5c-49fb-8260-63d41860ddd8}", "Select a record from the grid to use this control.");
			this.NoContactSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoContactSelectedLabel.Name = "NoContactSelectedLabel";
			this.NoContactSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			this.NoContactSelectedLabel.TabIndex = 1;
			this.NoContactSelectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// EdocsSwappableControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NoContactSelectedLabel);
			this.Name = "EdocsSwappableControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void InitializeEDocsPlugin()
		{
			eDocsUserControl = new DocumentScanning.GUI.eDocsUserControl(eDocsPlugIn);
			this.SuspendLayout();
			// 
			// eDocsUserControl
			// 
			this.eDocsUserControl.AllowDrop = true;
			this.eDocsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eDocsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.eDocsUserControl.Name = "eDocsUserControl";
			this.eDocsUserControl.SetDataBinding(eDocsPlugIn.BusinessEntity, "");
			this.eDocsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 149, true);
			this.eDocsUserControl.TabIndex = 0;
			this.eDocsUserControl.Visible = false;
			this.eDocsUserControl.OnlyShowGrid = false;
			this.Controls.Add(eDocsUserControl);
			this.ResumeLayout(true);
		}

		#endregion

		public eDocsPlugIn eDocsPlugIn;
		internal ZLabel NoContactSelectedLabel;
		public eDocsUserControl eDocsUserControl;
	}
}
