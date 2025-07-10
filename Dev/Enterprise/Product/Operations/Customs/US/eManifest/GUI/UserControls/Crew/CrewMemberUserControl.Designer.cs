namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class CrewMemberUserControl
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
		void InitializeComponent()
		{
			this.CrewMemberTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.PersonalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PersonalDetailsUserControl = new Enterprise.Customs.US.eManifest.GUI.PersonalDetailsUserControl();
			this.TravelDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TravelDocumentsUserControl = new Enterprise.MasterFiles.GUI.CertificatesUserControl();
			this.UsAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.USAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CrewMemberTabControl.SuspendLayout();
			this.PersonalDetailsTabPage.SuspendLayout();
			this.PersonalDetailsUserControl.SuspendLayout();
			this.TravelDocumentsTabPage.SuspendLayout();
			this.TravelDocumentsUserControl.SuspendLayout();
			this.UsAddressTabPage.SuspendLayout();
			this.USAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.CrewMember);
			// 
			// CrewMemberTabControl
			// 
			this.CrewMemberTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CrewMemberTabControl.Controls.Add(this.PersonalDetailsTabPage);
			this.CrewMemberTabControl.Controls.Add(this.TravelDocumentsTabPage);
			this.CrewMemberTabControl.Controls.Add(this.UsAddressTabPage);
			this.CrewMemberTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrewMemberTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrewMemberTabControl.Name = "CrewMemberTabControl";
			this.CrewMemberTabControl.SelectedIndex = 0;
			this.CrewMemberTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 241, true);
			this.CrewMemberTabControl.TabIndex = 0;
			// 
			// PersonalDetailsTabPage
			// 
			this.PersonalDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMemberUserControl|e01ddcb0-7530-4799-8476-633d7c88f33f", "Personal Details");
			this.PersonalDetailsTabPage.Controls.Add(this.PersonalDetailsUserControl);
			this.PersonalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.PersonalDetailsTabPage.Name = "PersonalDetailsTabPage";
			this.PersonalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PersonalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 218, true);
			this.PersonalDetailsTabPage.TabIndex = 0;
			// 
			// PersonalDetailsUserControl
			// 
			this.PersonalDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PersonalDetailsUserControl, ".");
			this.PersonalDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.PersonalDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PersonalDetailsUserControl.Name = "PersonalDetailsUserControl";
			this.PersonalDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 150, true);
			this.PersonalDetailsUserControl.TabIndex = 2;
			// 
			// TravelDocumentsTabPage
			// 
			this.TravelDocumentsTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMemberUserControl|7b36ef3d-6521-49e0-aaa0-8a62ab2d2fb2", "Travel Certificates");
			this.TravelDocumentsTabPage.Controls.Add(this.TravelDocumentsUserControl);
			this.TravelDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.TravelDocumentsTabPage.Name = "TravelDocumentsTabPage";
			this.TravelDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TravelDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 218, true);
			this.TravelDocumentsTabPage.TabIndex = 1;
			// 
			// TravelDocumentsUserControl
			// 
			this.TravelDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TravelDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ICertificatesProvider)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)))));
			this.TravelDocumentsUserControl.BindToForDescription = "Certificates.XZ_TypeDescription";
			this.TravelDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TravelDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TravelDocumentsUserControl.Name = "TravelDocumentsUserControl";
			this.TravelDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 211, true);
			this.TravelDocumentsUserControl.TabIndex = 0;
			// 
			// UsAddressTabPage
			// 
			this.UsAddressTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UsAddressTabPage.Controls.Add(this.USAddressControl);
			this.UsAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.UsAddressTabPage.Name = "UsAddressTabPage";
			this.UsAddressTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UsAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 218, true);
			this.UsAddressTabPage.TabIndex = 2;
			this.UsAddressTabPage.Text = "US Organization";
			// 
			// USAddressControl
			// 
			this.USAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.USAddressControl, "USAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.eManifest.Business.CrewMember)(null)).USAddress)));
			this.USAddressControl.BindToOrganisations = "Lookups.Organizations";
			this.USAddressControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("CrewMemberUserControl|USAddress", "US Organization");
			this.USAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.USAddressControl.Name = "USAddressControl";
			this.USAddressControl.ReadOnly = false;
			this.USAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.USAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.USAddressControl.TabIndex = 4;
			this.USAddressControl.ValidationJustForced = false;
			// 
			// CrewMemberUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CrewMemberTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.Name = "CrewMemberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 241, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CrewMemberTabControl.ResumeLayout(false);
			this.CrewMemberTabControl.PerformLayout();
			this.PersonalDetailsTabPage.ResumeLayout(false);
			this.PersonalDetailsTabPage.PerformLayout();
			this.PersonalDetailsUserControl.ResumeLayout(true);
			this.PersonalDetailsUserControl.PerformLayout();
			this.TravelDocumentsTabPage.ResumeLayout(false);
			this.TravelDocumentsTabPage.PerformLayout();
			this.TravelDocumentsUserControl.ResumeLayout(true);
			this.TravelDocumentsUserControl.PerformLayout();
			this.UsAddressTabPage.ResumeLayout(false);
			this.UsAddressTabPage.PerformLayout();
			this.USAddressControl.ResumeLayout(true);
			this.USAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl CrewMemberTabControl;
		private ZArchitecture.GUI.ZTabPage PersonalDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage TravelDocumentsTabPage;
		private PersonalDetailsUserControl PersonalDetailsUserControl;
		private Enterprise.MasterFiles.GUI.CertificatesUserControl TravelDocumentsUserControl;
		private ZArchitecture.GUI.ZTabPage UsAddressTabPage;
		private MasterFiles.GUI.ZDocAddressControl USAddressControl;
	}
}
