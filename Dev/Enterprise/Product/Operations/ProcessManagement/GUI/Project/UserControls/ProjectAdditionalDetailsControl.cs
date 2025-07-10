using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectAdditionalDetailsControl : ZUserControl
	{
		public ProjectAdditionalDetailsControl()
		{
			InitializeComponent();
			Panels.Add(LeftTopPanel);
			Panels.Add(RightTopPanel);
			Panels.Add(LeftBottomPanel);
			Panels.Add(RightBottomPanel);

			VisibilityConfigurationProvider.SetIsVisibilityConfigured(this, true);
			foreach (var panel in Panels)
			{
				VisibilityConfigurationProvider.SetIsVisibilityConfigured(panel, true);
				panel.ControlAdded += new ControlEventHandler(delegate
				{ SetNoPanelsMessageLabelTextIfNeeded(); });
				panel.ControlRemoved += new ControlEventHandler(delegate
				{ SetNoPanelsMessageLabelTextIfNeeded(); });
			}
			// MiddleBottom and MiddleTop Panels are excluded for a 4 panel tab

			SetNoPanelsMessageLabelTextIfNeeded();
		}

		readonly List<ZPanel> Panels = new List<ZPanel>();

		void SetNoPanelsMessageLabelTextIfNeeded()
		{
			if (HasDynamicControl())
			{
				NoPanelsMessageLabel.Visible = false;
				splitContainerMain.Visible = true;
			}
			else
			{
				NoPanelsMessageLabel.Visible = true;
				splitContainerMain.Visible = false;
			}
		}

		bool HasDynamicControl()
		{
			foreach (var panel in Panels)
			{
				if (HasDynamicControl(panel))
				{
					return true;
				}
			}
			return false;
		}

		bool HasDynamicControl(ZPanel panel)
		{
			foreach (var control in panel.Controls)
			{
				if (control is ZDynamicControlCreationUserControl)
				{
					return true;
				}
			}
			return false;
		}

#if DEBUG
		public ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest()
		{
			return VisibilityConfigurationProvider;
		}
#endif
	}
}
