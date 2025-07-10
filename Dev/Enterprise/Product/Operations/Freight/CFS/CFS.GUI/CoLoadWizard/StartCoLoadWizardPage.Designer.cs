using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for StartCoLoadWizardPage.
	/// </summary>
	public partial class StartCoLoadWizardPage : WizardPage
	{
		MasterFiles.GUI.ZOrganisationControl CoLoadForwarderOrganisationControl;
		ZArchitecture.GUI.ZPictureBox StartWizardPictureBox;
		ZLabel WelcomeLabel;
		ZLabel DirectionsLabel;
		System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.StartWizardPictureBox = new ZArchitecture.GUI.ZPictureBox();
			this.CoLoadForwarderOrganisationControl = new MasterFiles.GUI.ZOrganisationControl();
			this.WelcomeLabel = new ZLabel();
			this.DirectionsLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StartWizardPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CoLoadWizardShipment);
			// 
			// StartWizardPictureBox
			// 
			this.StartWizardPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(2, 11, true);
			this.StartWizardPictureBox.Name = "StartWizardPictureBox";
			this.StartWizardPictureBox.Size = ControlDpiScalingHelper.NewScaledSize(144, 320, true);
			this.StartWizardPictureBox.TabIndex = 0;
			this.StartWizardPictureBox.TabStop = false;
			this.StartWizardPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.StartWizardPictureBox_Paint);
			// 
			// CoLoadForwarderOrganisationControl
			// 
			this.BindingSource.SetBindingMember(this.CoLoadForwarderOrganisationControl, "CW_OH_CoLoadForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CoLoadWizardShipment)(null)).CW_OH_CoLoadForwarder)));
			this.CoLoadForwarderOrganisationControl.BindToOrganisations = "Forwarders";
			this.CoLoadForwarderOrganisationControl.CaptionResourceString = Res.GetData("StartCoLoadWizardPage|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Co-load Forwarder");

/* Unmerged change from project 'Enterprise.Freight.CFS.GUI.Winzor'
Before:
			this.CoLoadForwarderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 184, true);
After:
			this.CoLoadForwarderOrganisationControl.Location = ControlDpiScalingHelper.NewScaledPoint(152, 184, true);
*/
			this.CoLoadForwarderOrganisationControl.Location = ControlDpiScalingHelper.NewScaledPoint(152, 184, true);
			this.CoLoadForwarderOrganisationControl.Name = "CoLoadForwarderOrganisationControl";
			this.CoLoadForwarderOrganisationControl.PopupCaption = "";
			this.CoLoadForwarderOrganisationControl.Size = ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.CoLoadForwarderOrganisationControl.TabIndex = 2;
			// 
			// WelcomeLabel
			// 
			this.WelcomeLabel.Location = ControlDpiScalingHelper.NewScaledPoint(149, 23, true);
			this.WelcomeLabel.Name = "WelcomeLabel";
			this.WelcomeLabel.Size = ControlDpiScalingHelper.NewScaledSize(249, 61, true);
			this.WelcomeLabel.TabIndex = 3;
			this.WelcomeLabel.TextAlign = ContentAlignment.TopLeft;
			// 
			// DirectionsLabel
			// 
			this.DirectionsLabel.Location = ControlDpiScalingHelper.NewScaledPoint(152, 84, true);
			this.DirectionsLabel.Name = "DirectionsLabel";
			this.DirectionsLabel.Size = ControlDpiScalingHelper.NewScaledSize(249, 97, true);
			this.DirectionsLabel.TabIndex = 4;
			this.DirectionsLabel.TextAlign = ContentAlignment.TopLeft;
			// 
			// StartCoLoadWizardPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DirectionsLabel);
			this.Controls.Add(this.WelcomeLabel);
			this.Controls.Add(this.CoLoadForwarderOrganisationControl);
			this.Controls.Add(this.StartWizardPictureBox);
			this.Name = "StartCoLoadWizardPage";
			this.Size = ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StartWizardPictureBox)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
