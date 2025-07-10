using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	abstract class ProjectControlTest : TestCase
	{
		public class CustomisablePanelNames
		{
			public const string LeftTopPanel = "LeftTopPanel";
			public const string LeftBottomPanel = "LeftBottomPanel";
			public const string MiddleTopPanel = "MiddleTopPanel";
			public const string MiddleBottomPanel = "MiddleBottomPanel";
			public const string RightTopPanel = "RightTopPanel";
			public const string RightBottomPanel = "RightBottomPanel";
		}

		public void TestCustomisablePanelsHasIsVisibilityConfigured()
		{
			// SetIsVisibilityConfigured should be set after InitializeComponent (i.e. not in the designer)
			// since all controls in ControlForTest (i.e. main control) need to be contructed first.
			// E.g., can't move a control from its default panel to another panel if the other panel doesn't exist yet.
			// So use the constructor for the designer and verify visibility not configured.
			using (var control = ControlForTest)
			{
				var visibilityConfigurationProvider = GetVisibilityConfigurationProviderForTest(control);

				foreach (var panel in FindCustomisablePanels(control))
				{
					AssertEquals(panel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(panel));
				}

				if (ControlShouldHaveVisibilityConfigured)
				{
					AssertEquals(control.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(control));
				}
			}
		}

		protected abstract bool ControlShouldHaveVisibilityConfigured { get; }
		protected abstract string[] ExpectedPanelNames { get; }
		protected abstract ZUserControl ControlForTest { get; }
		protected abstract ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest(ZUserControl control);

		protected List<ZPanel> FindCustomisablePanels(Control control)
		{
			List<ZPanel> listPanels = new List<ZPanel>();

			WalkControls(control, (ctrl) =>
			{
				if (ctrl is ZPanel && ExpectedPanelNames.Contains(ctrl.Name))
				{
					listPanels.Add(ctrl as ZPanel);
				}
				return true;
			});

			return listPanels;
		}

		protected void WalkControls(Control ctrl, Func<Control, bool> func)
		{
			if (!func(ctrl))
			{
				return;
			}

			foreach (Control childCtrl in ctrl.Controls)
			{
				WalkControls(childCtrl, func);
			}
		}
	}
}
