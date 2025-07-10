using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public abstract class ShipmentControlTest<T> : TestCaseWithFactory where T : ZUserControl, new()
	{
		public void TestCustomizablePanelsHasIsVisibilityConfigured()
		{
			using (T detailsControl = new T())
			{
				var visibilityConfigurationProvider = typeof(T)
					.GetField("VisibilityConfigurationProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					.GetValue(detailsControl) as ControlVisibilityConfigurationProvider;

				foreach (var panel in FindCustomizablePanels(detailsControl))
				{
					string message = string.Format("{0}: IsVisibilityConfigured must be set to True", panel.Name);
					AssertEquals(message, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(panel));
				}
			}
		}

		#region Implementation

		protected abstract string[] ExpectedPanelNames { get; }

		public static class CustomizablePanelNames
		{
			public const string LeftTopPanel = "LeftTopPanel";
			public const string LeftBottomPanel = "LeftBottomPanel";
			public const string MiddleTopPanel = "MiddleTopPanel";
			public const string MiddleBottomPanel = "MiddleBottomPanel";
			public const string RightTopPanel = "RightTopPanel";
			public const string RightBottomPanel = "RightBottomPanel";
		}

		protected List<ZPanel> FindCustomizablePanels(Control control)
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

		#endregion
	}
}
