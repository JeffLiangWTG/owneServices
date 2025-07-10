
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectAdditionalDetailsControlTest : ProjectControlTest
	{
		protected override ZUserControl ControlForTest
		{
			get { return new ProjectAdditionalDetailsControl(); }
		}

		protected override ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest(ZUserControl control)
		{
			return ((ProjectAdditionalDetailsControl)control).GetVisibilityConfigurationProviderForTest();
		}

		protected override string[] ExpectedPanelNames
		{
			get
			{
				return new string[]
				{
					CustomisablePanelNames.LeftTopPanel,
					CustomisablePanelNames.LeftBottomPanel,
					CustomisablePanelNames.RightTopPanel,
					CustomisablePanelNames.RightBottomPanel
				};
			}
		}

		protected override bool ControlShouldHaveVisibilityConfigured
		{
			get { return true; }
		}

		public void TestNoPanelsMessageLabelControlCaption()
		{
			using (var importUserControl = new ProjectAdditionalDetailsControl())
			{
				var control = importUserControl.FindSingle<ZLabel>("NoPanelsMessageLabel");
				AssertEquals("To make use of the Additional Details Tab, panels may be moved from the Main Details Tab. This can be done from the Workflow Template > Project > Screen Layout Tab.", control.CaptionResourceString.Caption);
			}
		}
	}
}
