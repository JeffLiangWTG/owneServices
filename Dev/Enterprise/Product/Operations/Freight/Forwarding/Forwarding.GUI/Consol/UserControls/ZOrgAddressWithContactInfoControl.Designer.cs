using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ZOrgAddressWithContactInfoControl
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
			this.addressValidationStatusButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContactControl = new MiniatureContactControl();
			this.OrganisationFindBox.SuspendLayout();
			this.GroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.AddressTab.SuspendLayout();
			this.ContactInfoTab.SuspendLayout();
			this.AddressDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ContactInfoTab
			// 
			this.ContactInfoTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ContactInfoTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 64, true);
			// 
			// FaxLabel
			// 
			this.FaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			// 
			// EmailLabel
			// 
			this.EmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			// 
			// zLabel3
			// 
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			// 
			// zLabel2
			// 
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			// 
			// zLabel1
			// 
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			// 
			// addressValidationStatusButton
			// 
			this.addressValidationStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 15, true);
			this.addressValidationStatusButton.Name = "addressValidationStatusButton";
			this.addressValidationStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.addressValidationStatusButton.TabIndex = 0;
			this.addressValidationStatusButton.Text = " ";
			this.addressValidationStatusButton.Click += new System.EventHandler(this.AddressValidationStatusButton_Click);
			this.addressValidationStatusButton.Visible = true;
			//
			// GroupBox
			//
			this.GroupBox.Controls.Add(this.addressValidationStatusButton);
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 126, true);
			//
			// DetailsTabControl
			//
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 87, true);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 36, true);
			this.DetailsTabControl.Controls.Add(this.ContactTabPage);
			// 
			// ContactTabPage
			// 
			this.ContactTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ContactTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZAddressWithContactInfoControl|464e4552-edd2-c89d-4233-05f42088a940", "Contact");
			this.ContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContactTabPage.Name = "ContactTabPage";
			this.ContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 65, true);
			this.ContactTabPage.Controls.Add(ContactControl);
			// 
			// ContactControl
			// 
			this.ContactControl.DataSourceType = typeof(ZAddressWithContact);
			BindingSource.SetBindingMember(ContactControl, ".");
			this.ContactControl.CaptionRenderingEnabled = true;
			this.ContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 55, true);
			//
			// ZOrgAddressWithContactInfoControl
			// 
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 127, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 127, true);
			this.Name = "ZOrgAddressWithContactInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 127, true);
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.AddressTab.ResumeLayout(false);
			this.AddressTab.PerformLayout();
			this.ContactInfoTab.ResumeLayout(false);
			this.ContactInfoTab.PerformLayout();
			this.AddressDropEdit.ResumeLayout(true);
			this.AddressDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		MiniatureContactControl ContactControl;
		protected ZArchitecture.GUI.ZTabPage ContactTabPage;

		#endregion
	}
}

