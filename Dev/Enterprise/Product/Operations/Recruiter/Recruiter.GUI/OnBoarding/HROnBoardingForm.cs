using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class HROnBoardingForm : ZForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.System;

		HROnBoarding hrOnBoarding => (HROnBoarding)DataSource;

		public HROnBoardingForm(HROnBoarding hrOnBoarding) : base(hrOnBoarding)
		{
			ControllerID = ControllerIDs.HROnBoarding;
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			if (!this.IsDesignMode())
			{
				WorkflowTabPage.Initialize(hrOnBoarding);
			}
		}

		void OnBoardingButton_Click(object sender, EventArgs e)
		{
			OpenInBrowser(hrOnBoarding.PK);
		}

		static void OpenInBrowser(ZGuid onboardingPK)
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrEmpty(baseURL))
			{
				var errorMessage = ResString.GetMultilingualString("3EC65C07-E96C-4E7D-B19A-B4AD24C202D2",
@"The HR Onboarding record cannot be opened in a browser as GLOW has not been configured for this client.
Registry: ") + GlowRegistry.Instance.GlowPortalsUri.Category + "/" + GlowRegistry.Instance.GlowPortalsUri.Caption;
				Globals.Message.ShowError(errorMessage);
				return;
			}

			var url = UrlBuilder.GenerateURL(new Uri(baseURL), "goto/onBoarding", additionalQueryStrings: new[] { ("onboardingPK", onboardingPK.ToString()) });
			WebUrlLauncher.Launch(url.ToString());
		}
	}
}
