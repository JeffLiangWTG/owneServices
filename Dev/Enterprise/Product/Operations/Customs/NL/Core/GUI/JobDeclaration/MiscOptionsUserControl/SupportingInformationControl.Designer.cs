using System.Windows.Forms;

namespace Enterprise.Customs.NL.GUI
{
	partial class SupportingInformationControl
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
			this.FiscalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FiscalReferencesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.FiscalReferencesUserControl.SuspendLayout();
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

			// 
			// FiscalReferencesTabPage
			// 
			this.FiscalReferencesTabPage.Controls.Add(this.FiscalReferencesUserControl);
			this.FiscalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FiscalReferencesTabPage.Name = "FiscalReferencesTabPage";
			this.FiscalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FiscalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 244, true);
			this.FiscalReferencesTabPage.TabIndex = 3;
			this.FiscalReferencesTabPage.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLMiscOptionsUserControl|359A0451-3D81-47A3-AA56-C94DD045DF56", "Fiscal References");

			// 
			// FiscalReferencesUserControl
			// 
			this.FiscalReferencesUserControl.AllowDrop = true;
			this.FiscalReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FiscalReferencesUserControl.Name = "FiscalReferencesUserControl";
			this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 238, true);
			this.FiscalReferencesUserControl.TabIndex = 0;
			this.FiscalReferencesUserControl.UserControlType = typeof(Enterprise.Customs.NL.GUI.NLFiscalReferencesUserControl);
			this.FiscalReferencesUserControl.CaptionRenderingEnabled = true;

			this.SupportingInformationTabControl.Controls.Add(this.FiscalReferencesTabPage);
			this.SupportingInformationTabControl.Controls.SetChildIndex(this.FiscalReferencesTabPage, 0);

			//
			// AdditionalInfoTabPage
			//
			this.AdditionalInfoTabPage.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("11E75C13-E915-4E9E-8B13-4A28D98BA0CB", "Additional documents");

			this.FiscalReferencesTabPage.ResumeLayout(false);
			this.FiscalReferencesTabPage.PerformLayout();
			this.FiscalReferencesUserControl.ResumeLayout(true);
			this.FiscalReferencesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabPage FiscalReferencesTabPage;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl FiscalReferencesUserControl;
	}
}
