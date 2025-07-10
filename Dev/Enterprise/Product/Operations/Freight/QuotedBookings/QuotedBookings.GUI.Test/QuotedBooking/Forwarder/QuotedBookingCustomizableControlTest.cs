using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public abstract class QuotedBookingCustomizableControlTest<T> : TestCaseWithFactory where T : ZUserControl, new()
	{
		public void TestNoControlBoundDirectlyToQuote()
		{
			using (T detailsControl = new T())
			{
				var bindingSource = detailsControl.BindingSource;
				List<Control> listControlsNotBoundDirectly = new List<Control>();
				WalkControls(detailsControl, (ctrl) =>
				{
					var bindingMember = bindingSource.GetFullBindingMember(ctrl);
					if (!String.IsNullOrEmpty(bindingMember) && bindingMember.StartsWith("Quote+"))
					{
						listControlsNotBoundDirectly.Add(ctrl);
					}

					return true;
				}

				);
				AssertEquals(String.Join(",", listControlsNotBoundDirectly.ConvertAll((ctrl) => String.Format("{0} ({1})", ctrl.Name, bindingSource.GetFullBindingMember(ctrl))).ToArray()), 0, listControlsNotBoundDirectly.Count);
			}
		}

		public void TestQuotedBookingFormCustomisationSettingsProviderDisplayFields()
		{
			using (T detailsControl = new T())
			{
				QuotedBookingFormCustomisationSettingsProvider provider = new QuotedBookingFormCustomisationSettingsProvider(new QuotedBookingWorkflowDescriptor());
				var displayFields = (
					from field in provider.DisplayFields.Cast<FormCustomisableElement>()
					where field.DisplayTabCode == TabName && field.Visible && field.ElementGroup != "AdditionalContacts"
					group field by field.Placement.Replace(" ", string.Empty) into grp
					select grp).ToDictionary(g => g.Key, e => e.Cast<FormCustomisableElement>().ToArray());
				var panels = FindCustomizablePanels(detailsControl);
				foreach (var panelName in displayFields.Keys)
				{
					var panel = (
						from p in panels
						where p.Name.StartsWith(panelName)
						select p).FirstOrDefault();
					AssertNotNull(String.Format("Control is missing {0} panel", panelName), panel);
					AssertControlContainsCustomizableElement(panel, displayFields[panelName]);
				}
			}
		}

		public void TestContainsCustomizablePanelNames()
		{
			using (T detailsControl = new T())
			{
				var panelNames =
					from panel in FindCustomizablePanels(detailsControl) select panel.Name;
				AssertContainsExactElementsInAnyOrder(ExpectedPanelNames, panelNames);
			}
		}

		public void TestCustomizablePanelsHasIsVisibilityConfigured()
		{
			using (T detailsControl = new T())
			{
				var visibilityConfigurationProvider = typeof(T).GetField("VisibilityConfigurationProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(detailsControl) as ControlVisibilityConfigurationProvider;
				foreach (var panel in FindCustomizablePanels(detailsControl))
				{
					string message = string.Format("{0}: IsVisibilityConfigured must be set to True", panel.Name);
					AssertEquals(message, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(panel));
				}
			}
		}

		protected void AssertControlContainsCustomizableElement(Control control, FormCustomisableElement[] elements)
		{
			foreach (var element in elements)
			{
				Control ctrlElem = null;
				WalkControls(control, (ctrl) =>
				{
					if (ctrl.Name == element.ElementName)
					{
						ctrlElem = ctrl;
						return false;
					}

					return true;
				}

				);
				AssertNotNull(String.Format("Expected {0} control on {1} control", element.ElementName, control.Name), ctrlElem);
			}
		}

		#region Implementation
		protected abstract string TabName
		{
			get;
		}

		protected abstract string[] ExpectedPanelNames
		{
			get;
		}

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
			}

			);
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
