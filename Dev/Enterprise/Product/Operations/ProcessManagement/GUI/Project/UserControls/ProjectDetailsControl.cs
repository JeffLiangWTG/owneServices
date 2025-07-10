using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectDetailsControl : ZUserControl, IListenForNotifications
	{
		public ProjectDetailsControl()
		{
			InitializeComponent();

			VisibilityConfigurationProvider.SetIsVisibilityConfigured(LeftTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(LeftBottomPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(MiddleTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(RightTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(RightBottomPanel, true);
			// NOTE: MiddleBottom Panel of Details Tab is deliberately excluded for the details group
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			if (pendingChildVisibilityChange)
			{
				pendingChildVisibilityChange = false;
				UpdateTopPanelHeight();
			}
		}

		/// <summary>
		/// Fit the splitter containing the top panels to their visible height.
		/// Avoids lots of blank space in the panels when child controls have been hidden by the workflow template.
		/// </summary>
		void UpdateTopPanelHeight()
		{
			int maxPreferredHeight = 0;
			foreach (var panel in new ZPanel[] { LeftTopPanel, MiddleTopPanel, RightTopPanel })
			{
				var content = PanelDynamicContent(panel);
				var rowPanel = FindRowPanel(content);
				if (rowPanel != null)
				{
					maxPreferredHeight = Math.Max(maxPreferredHeight, rowPanel.PreferredSize.Height);

					foreach (Control child in rowPanel.Controls)
					{
						if (child.Visible)
						{
							maxPreferredHeight = Math.Max(maxPreferredHeight, child.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(22));
						}
					}
				}
			}

			if (maxPreferredHeight >= ControlDpiScalingHelper.ScaleToCurrentDpiY(RowLayout.DefaultRowHeight) && SplitContainerMain.Width != 0)
			{
				SplitContainerMain.SplitterDistance = Math.Max(0, maxPreferredHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(30));
			}
		}

		static RowLayoutPanel FindRowPanel(Control parent)
		{
			if (parent != null)
			{
				foreach (Control child in parent.Controls)
				{
					var found = (child as RowLayoutPanel) ?? FindRowPanel(child);
					if (found != null)
					{
						return found;
					}
				}
			}

			return null;
		}

		static Control PanelDynamicContent(ZPanel panel)
		{
			var child = FirstChild(panel);
			if (child is ZDynamicControlCreationUserControl)
			{
				child = FirstChild(child);
			}

			return child;
		}

		static Control FirstChild(Control parent)
		{
			return parent != null && parent.Controls.Count > 0
				? parent.Controls[0]
				: null;
		}

		void IListenForNotifications.NotifyAboutStateOfChildControl(Control control, INotificationType state)
		{
		}

		bool pendingChildVisibilityChange;

		void IListenForNotifications.NotifyAboutVisibilityChangeOfChildControl(Control control)
		{
			if (!pendingChildVisibilityChange && control.IsHandleCreated)
			{
				control.BeginInvoke(() => { PerformLayout(); });
			}
			if (control.Parent is RowLayoutPanel)
			{
				pendingChildVisibilityChange = true;
			}
		}

#if DEBUG
		public ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest()
		{
			return VisibilityConfigurationProvider;
		}
#endif
	}
}
