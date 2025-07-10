
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingUserControl
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
			this.components = new System.ComponentModel.Container();
			this.LicensingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingMessagesUserControl = new Enterprise.Customs.TW.GUI.LicensingMessagesUserControl();
			this.CertificateOfOriginTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingCertificateOfOriginUserControl = new Enterprise.Customs.TW.GUI.LicensingCertificateOfOriginUserControl();
			this.CommonTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingCommonUserControl = new Enterprise.Customs.TW.GUI.LicensingCommonUserControl();
			this.AnimalAndPlantTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingAnimalAndPlantUserControl = new Enterprise.Customs.TW.GUI.LicensingAnimalAndPlantUserControl();
			this.FoodAndDrugTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingFoodAndDrugUserControl = new Enterprise.Customs.TW.GUI.LicensingFoodAndDrugUserControl();
			this.AlcoholTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingAlcoholUserControl = new Enterprise.Customs.TW.GUI.LicensingAlcoholUserControl();
			this.TypeApprovalTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingTypeApprovalUserControl = new Enterprise.Customs.TW.GUI.LicensingTypeApprovalUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicensingTabControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.LicensingMessagesUserControl.SuspendLayout();
			this.CertificateOfOriginTabPage.SuspendLayout();
			this.LicensingCertificateOfOriginUserControl.SuspendLayout();
			this.CommonTabPage.SuspendLayout();
			this.LicensingCommonUserControl.SuspendLayout();
			this.AnimalAndPlantTabPage.SuspendLayout();
			this.LicensingAnimalAndPlantUserControl.SuspendLayout();
			this.FoodAndDrugTabPage.SuspendLayout();
			this.LicensingFoodAndDrugUserControl.SuspendLayout();
			this.AlcoholTabPage.SuspendLayout();
			this.LicensingAlcoholUserControl.SuspendLayout();
			this.TypeApprovalTabPage.SuspendLayout();
			this.LicensingTypeApprovalUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// LicensingTabControl
			// 
			this.LicensingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LicensingTabControl.Controls.Add(this.MessagesTabPage);
			this.LicensingTabControl.Controls.Add(this.CertificateOfOriginTabPage);
			this.LicensingTabControl.Controls.Add(this.CommonTabPage);
			this.LicensingTabControl.Controls.Add(this.AnimalAndPlantTabPage);
			this.LicensingTabControl.Controls.Add(this.FoodAndDrugTabPage);
			this.LicensingTabControl.Controls.Add(this.AlcoholTabPage);
			this.LicensingTabControl.Controls.Add(this.TypeApprovalTabPage);
			this.LicensingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicensingTabControl.Name = "LicensingTabControl";
			this.LicensingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 314, true);
			this.LicensingTabControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("57a1ffef-1a5f-4737-bcac-02a32fec24b7", "Messages");
			this.MessagesTabPage.Controls.Add(this.LicensingMessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.MessagesTabPage.TabIndex = 0;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingMessagesUserControl
			// 
			this.LicensingMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicensingMessagesUserControl, ".");
			this.LicensingMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingMessagesUserControl.Name = "LicensingMessagesUserControl";
			this.LicensingMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingMessagesUserControl.TabIndex = 0;
			// 
			// CertificateOfOriginTabPage
			// 
			this.CertificateOfOriginTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e19bbfe6-e2f6-432c-a62f-5be21e7fedf9", "Certificate of Origin");
			this.CertificateOfOriginTabPage.Controls.Add(this.LicensingCertificateOfOriginUserControl);
			this.CertificateOfOriginTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CertificateOfOriginTabPage.Name = "CertificateOfOriginTabPage";
			this.CertificateOfOriginTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CertificateOfOriginTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.CertificateOfOriginTabPage.TabIndex = 6;
			this.CertificateOfOriginTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingCertificateOfOriginUserControl
			// 
			this.LicensingCertificateOfOriginUserControl.AllowDrop = true;
			this.LicensingCertificateOfOriginUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LicensingCertificateOfOriginUserControl, ".");
			this.LicensingCertificateOfOriginUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingCertificateOfOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingCertificateOfOriginUserControl.Name = "LicensingCertificateOfOriginUserControl";
			this.LicensingCertificateOfOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingCertificateOfOriginUserControl.TabIndex = 0;
			// 
			// CommonTabPage
			// 
			this.CommonTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CommonTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("095ce5c4-6c87-4004-b382-0b0d1bf7d8bb", "Common");
			this.CommonTabPage.Controls.Add(this.LicensingCommonUserControl);
			this.CommonTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CommonTabPage.Name = "CommonTabPage";
			this.CommonTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommonTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.CommonTabPage.TabIndex = 1;
			// 
			// LicensingCommonUserControl
			// 
			this.LicensingCommonUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicensingCommonUserControl, ".");
			this.LicensingCommonUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingCommonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingCommonUserControl.Name = "LicensingCommonUserControl";
			this.LicensingCommonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingCommonUserControl.TabIndex = 0;
			// 
			// AnimalAndPlantTabPage
			// 
			this.AnimalAndPlantTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("133cb9cc-8ade-408e-ab08-252cccc19d76", "Animal and Plant");
			this.AnimalAndPlantTabPage.Controls.Add(this.LicensingAnimalAndPlantUserControl);
			this.AnimalAndPlantTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AnimalAndPlantTabPage.Name = "AnimalAndPlantTabPage";
			this.AnimalAndPlantTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AnimalAndPlantTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.AnimalAndPlantTabPage.TabIndex = 2;
			this.AnimalAndPlantTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingAnimalAndPlantUserControl
			// 
			this.LicensingAnimalAndPlantUserControl.AllowDrop = true;
			this.LicensingAnimalAndPlantUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LicensingAnimalAndPlantUserControl, ".");
			this.LicensingAnimalAndPlantUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingAnimalAndPlantUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingAnimalAndPlantUserControl.Name = "LicensingAnimalAndPlantUserControl";
			this.LicensingAnimalAndPlantUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingAnimalAndPlantUserControl.TabIndex = 0;
			// 
			// FoodAndDrugTabPage
			// 
			this.FoodAndDrugTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("3cb679c1-72fe-439b-936c-6a274b7f52cb", "Food and Drug");
			this.FoodAndDrugTabPage.Controls.Add(this.LicensingFoodAndDrugUserControl);
			this.FoodAndDrugTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FoodAndDrugTabPage.Name = "FoodAndDrugTabPage";
			this.FoodAndDrugTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FoodAndDrugTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.FoodAndDrugTabPage.TabIndex = 3;
			this.FoodAndDrugTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingFoodAndDrugUserControl
			// 
			this.LicensingFoodAndDrugUserControl.AllowDrop = true;
			this.LicensingFoodAndDrugUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LicensingFoodAndDrugUserControl, ".");
			this.LicensingFoodAndDrugUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingFoodAndDrugUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingFoodAndDrugUserControl.Name = "LicensingFoodAndDrugUserControl";
			this.LicensingFoodAndDrugUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingFoodAndDrugUserControl.TabIndex = 0;
			// 
			// AlcoholTabPage
			// 
			this.AlcoholTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("af055796-0949-4e6a-b818-01b497e08a49", "Alcohol");
			this.AlcoholTabPage.Controls.Add(this.LicensingAlcoholUserControl);
			this.AlcoholTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AlcoholTabPage.Name = "AlcoholTabPage";
			this.AlcoholTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AlcoholTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.AlcoholTabPage.TabIndex = 4;
			this.AlcoholTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingAlcoholUserControl
			// 
			this.LicensingAlcoholUserControl.AllowDrop = true;
			this.LicensingAlcoholUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LicensingAlcoholUserControl, ".");
			this.LicensingAlcoholUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingAlcoholUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingAlcoholUserControl.Name = "LicensingAlcoholUserControl";
			this.LicensingAlcoholUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingAlcoholUserControl.TabIndex = 0;
			// 
			// TypeApprovalTabPage
			// 
			this.TypeApprovalTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("17b371da-beab-4f18-85d6-b137f0bc0c85", "Type Approval");
			this.TypeApprovalTabPage.Controls.Add(this.LicensingTypeApprovalUserControl);
			this.TypeApprovalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TypeApprovalTabPage.Name = "TypeApprovalTabPage";
			this.TypeApprovalTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TypeApprovalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 287, true);
			this.TypeApprovalTabPage.TabIndex = 5;
			this.TypeApprovalTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingTypeApprovalUserControl
			// 
			this.LicensingTypeApprovalUserControl.AllowDrop = true;
			this.LicensingTypeApprovalUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.LicensingTypeApprovalUserControl, ".");
			this.LicensingTypeApprovalUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingTypeApprovalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LicensingTypeApprovalUserControl.Name = "LicensingTypeApprovalUserControl";
			this.LicensingTypeApprovalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 281, true);
			this.LicensingTypeApprovalUserControl.TabIndex = 0;
			// 
			// LicensingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LicensingTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 314, true);
			this.Name = "LicensingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicensingTabControl.ResumeLayout(false);
			this.LicensingTabControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.LicensingMessagesUserControl.ResumeLayout(true);
			this.LicensingMessagesUserControl.PerformLayout();
			this.CertificateOfOriginTabPage.ResumeLayout(false);
			this.CertificateOfOriginTabPage.PerformLayout();
			this.LicensingCertificateOfOriginUserControl.ResumeLayout(true);
			this.LicensingCertificateOfOriginUserControl.PerformLayout();
			this.CommonTabPage.ResumeLayout(false);
			this.CommonTabPage.PerformLayout();
			this.LicensingCommonUserControl.ResumeLayout(true);
			this.LicensingCommonUserControl.PerformLayout();
			this.AnimalAndPlantTabPage.ResumeLayout(false);
			this.AnimalAndPlantTabPage.PerformLayout();
			this.LicensingAnimalAndPlantUserControl.ResumeLayout(true);
			this.LicensingAnimalAndPlantUserControl.PerformLayout();
			this.FoodAndDrugTabPage.ResumeLayout(false);
			this.FoodAndDrugTabPage.PerformLayout();
			this.LicensingFoodAndDrugUserControl.ResumeLayout(true);
			this.LicensingFoodAndDrugUserControl.PerformLayout();
			this.AlcoholTabPage.ResumeLayout(false);
			this.AlcoholTabPage.PerformLayout();
			this.LicensingAlcoholUserControl.ResumeLayout(true);
			this.LicensingAlcoholUserControl.PerformLayout();
			this.TypeApprovalTabPage.ResumeLayout(false);
			this.TypeApprovalTabPage.PerformLayout();
			this.LicensingTypeApprovalUserControl.ResumeLayout(true);
			this.LicensingTypeApprovalUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZTabControl LicensingTabControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZArchitecture.GUI.ZTabPage CommonTabPage;
		private ZArchitecture.GUI.ZTabPage AnimalAndPlantTabPage;
		private ZArchitecture.GUI.ZTabPage FoodAndDrugTabPage;
		private ZArchitecture.GUI.ZTabPage AlcoholTabPage;
		private ZArchitecture.GUI.ZTabPage TypeApprovalTabPage;
		private LicensingMessagesUserControl LicensingMessagesUserControl;
		private LicensingCommonUserControl LicensingCommonUserControl;
		private LicensingAnimalAndPlantUserControl LicensingAnimalAndPlantUserControl;
		private LicensingFoodAndDrugUserControl LicensingFoodAndDrugUserControl;
		private LicensingAlcoholUserControl LicensingAlcoholUserControl;
		private LicensingTypeApprovalUserControl LicensingTypeApprovalUserControl;
        private ZArchitecture.GUI.ZTabPage CertificateOfOriginTabPage;
		private LicensingCertificateOfOriginUserControl LicensingCertificateOfOriginUserControl;
	}
}
