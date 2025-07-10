using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DrawbackJobDeclarationUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestDrawbackJobDeclarationUserControl()
		{
			using (DrawbackJobDeclarationUserControl drawbackJobDeclarationUserControl = new DrawbackJobDeclarationUserControl())
			{
			}
		}

		public void TestControlLabelOnDrawbackJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				using (var declarationUserControl = (DrawbackJobDeclarationUserControl)brokerageControl.DeclarationUserControl)
				{
					var control = declarationUserControl.Controls.Find("ACENAFTADrawbackCountryDropEdit", true)[0] as IResCaptionedControl;
					AssertEquals("US_NAFTADrawbackCountry lable", "USMCA Drawback Ctry/Rgn. Code", control.CaptionResourceString.Caption);
				}
			}
		}

		public void TestControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				using (var declarationUserControl = (DrawbackJobDeclarationUserControl)brokerageControl.DeclarationUserControl)
				{
					Assert("Control is not visible", !declarationUserControl.Controls.Find("AdditionalDetailsGroupBox", true)[0].Visible);
					Assert("Control is not visible", !declarationUserControl.Controls.Find("TransferorOrganizationControl", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("ContractNumberszGroupBox", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("BondGroupBox", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("ImporterOrganisationControl", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("MessageStatusDescriptionTextBox", true)[0].Visible);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					Assert("Control is visible", declarationUserControl.Controls.Find("AdditionalDetailsGroupBox", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("TransferorOrganizationControl", true)[0].Visible);
					Assert("Control is not visible", !declarationUserControl.Controls.Find("ContractNumberszGroupBox", true)[0].Visible);
					Assert("Control is not visible", !declarationUserControl.Controls.Find("BondGroupBox", true)[0].Visible);
					Assert("Control is not visible", !declarationUserControl.Controls.Find("ImporterOrganisationControl", true)[0].Visible);
				}
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				using (var declarationUserControl = (DrawbackJobDeclarationUserControl)brokerageControl.DeclarationUserControl)
				{
					Assert("Control is not visible", !declarationUserControl.Controls.Find("AdditionalDetailsGroupBox", true)[0].Visible);
					Assert("Control is not visible", !declarationUserControl.Controls.Find("TransferorOrganizationControl", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("ContractNumberszGroupBox", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("BondGroupBox", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("MessageStatusDescriptionTextBox", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("ImporterOrganisationControl", true)[0].Visible);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("AdditionalDetailsGroupBox", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("TransferorOrganizationControl", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("ContractNumberszGroupBox", true)[0].Visible);
					Assert("Control is NOT visible", !declarationUserControl.Controls.Find("BondGroupBox", true)[0].Visible);
					Assert("Control is visible", declarationUserControl.Controls.Find("ImporterOrganisationControl", true)[0].Visible);
				}
			}
		}
	}
}
