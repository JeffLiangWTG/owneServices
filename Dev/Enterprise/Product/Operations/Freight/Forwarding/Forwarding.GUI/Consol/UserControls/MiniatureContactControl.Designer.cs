using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class MiniatureContactControl : ContactControl
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ContactGuidDropEditControl.MaximumSize = ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.ContactGuidDropEditControl.SetControlWidth(230);
			this.ContactGuidDropEditControl.CodeBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);

			this.FaxLabel.Size = ControlDpiScalingHelper.NewScaledSize(24, 10, true);
			this.PhoneLabel.Size = ControlDpiScalingHelper.NewScaledSize(80, 10, true);
			this.EmailLabel.Visible = false;
			this.WebLabel.Visible = false;
			this.WebLinkLabel.Visible = false;
		}

		#endregion
	}
}
