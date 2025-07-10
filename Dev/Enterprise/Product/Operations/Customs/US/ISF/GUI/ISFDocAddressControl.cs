using System;
using System.Windows.Forms;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public class ISFDocAddressControl : ZDocAddressControl, Integration.Customs.US.ISF.IISFDocAddressControl
	{
		public ISFDocAddressControl()
		{
			SetupSocialSecurityNumberFields();
		}

		void SetupSocialSecurityNumberFields()
		{
			this.socialSecurityNumbertDataTextBox = new ZTextBox();
			this.socialSecurityNumbertDataEditButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)this.BindingSource).BeginInit();
			this.DetailsTabControl.SuspendLayout();
			this.GovernmentRegistrationTabPage.SuspendLayout();
			this.OverrideGroupBox.SuspendLayout();
			this.SuspendLayout();

			this.GovernmentRegistrationTabPage.Controls.Add(this.socialSecurityNumbertDataTextBox);
			this.GovernmentRegistrationTabPage.Controls.Add(this.socialSecurityNumbertDataEditButton);
			// 
			// SocialSecurityNumbertDataTextBox
			// 
			this.BindingSource.SetBindingMember(this.socialSecurityNumbertDataTextBox, "E2_SocialSecurityNumberDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ISFDocAddress)null).E2_SocialSecurityNumberDetails);
			this.socialSecurityNumbertDataTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 8);
			this.socialSecurityNumbertDataTextBox.Name = "SocialSecurityNumbertDataTextBox";
			this.socialSecurityNumbertDataTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20);
			this.socialSecurityNumbertDataTextBox.TabIndex = 10;
			// 
			// SocialSecurityNumbertDataEditButton
			// 
			this.socialSecurityNumbertDataEditButton.CaptionResourceString = Res.GetData("ZDocAddressControl|3a5627f0-d184-4c64-bc38-b953abeca309", "&Edit");
			this.socialSecurityNumbertDataEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 5);
			this.socialSecurityNumbertDataEditButton.Name = "SocialSecurityNumbertDataEditButton";
			this.socialSecurityNumbertDataEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23);
			this.socialSecurityNumbertDataEditButton.TabIndex = 11;
			this.socialSecurityNumbertDataEditButton.UseVisualStyleBackColor = true;
			this.socialSecurityNumbertDataEditButton.Click += new EventHandler(this.SocialSecurityNumbertDataEditButton_Click);
			((System.ComponentModel.ISupportInitialize)this.BindingSource).EndInit();
			this.DetailsTabControl.ResumeLayout(false);
			this.GovernmentRegistrationTabPage.ResumeLayout(false);
			this.GovernmentRegistrationTabPage.PerformLayout();
			this.OverrideGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected new ISFDocAddress DocAddress
		{
			get { return (ISFDocAddress)base.DocAddress; }
		}

		protected override Control ValidationReferenceControl => this;

		protected override void UpdateDetailsVisibility()
		{
			var docAddress = DocAddress;
			if (docAddress != null)
			{
				bool isSocialSecurityNumberGovRegNumType = docAddress.IsSocialSecurityNumberGovRegNumType;
				GovernmentRegistrationNumberTextBox.Visible = GovernmentRegistrationTypeDropEdit.Visible && docAddress != null && !isSocialSecurityNumberGovRegNumType && !docAddress.IsPassportIDGovRegNumType;
				bool socialSecurityNumbertDataIsVisible = GovernmentRegistrationTypeDropEdit.Visible && docAddress != null && isSocialSecurityNumberGovRegNumType;
				socialSecurityNumbertDataEditButton.Visible = socialSecurityNumbertDataIsVisible;
				socialSecurityNumbertDataTextBox.Visible = socialSecurityNumbertDataIsVisible;
				bool passportDataIsVisible = GovernmentRegistrationTypeDropEdit.Visible && docAddress != null && docAddress.IsPassportIDGovRegNumType;
				PassportDataEditButton.Visible = passportDataIsVisible;
				PassportDataTextBox.Visible = passportDataIsVisible;
			}
		}

		ZTextBox socialSecurityNumbertDataTextBox;
		ZButton socialSecurityNumbertDataEditButton;

		void SocialSecurityNumbertDataEditButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(new DocAddressSocialSecurityNumberDetailForm(DocAddress), ParentForm);
		}
	}
}
