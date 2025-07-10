
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectDetailsControlTest : ProjectControlTest
	{
		protected override ZUserControl ControlForTest
		{
			get { return new ProjectDetailsControl(); }
		}

		protected override ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest(ZUserControl control)
		{
			return ((ProjectDetailsControl)control).GetVisibilityConfigurationProviderForTest();
		}

		protected override string[] ExpectedPanelNames
		{
			get
			{
				return new string[]
				{
					CustomisablePanelNames.LeftTopPanel,
					CustomisablePanelNames.LeftBottomPanel,
					CustomisablePanelNames.MiddleTopPanel,
					CustomisablePanelNames.RightTopPanel,
					CustomisablePanelNames.RightBottomPanel
				};
			}
		}

		protected override bool ControlShouldHaveVisibilityConfigured
		{
			get { return false; }
		}
	}
}
