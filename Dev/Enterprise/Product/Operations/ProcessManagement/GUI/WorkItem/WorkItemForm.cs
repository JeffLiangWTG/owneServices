using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class WorkItemForm : RelatedItemsSupportableFormBase, IListenForNotifications
	{
		public WorkItemForm()
		{
			InitializeComponent();
			SetUpRelatedTabPage();
		}

		public WorkItemForm(WorkItem workItem, ZWorkflowTabPage workflowTab = null)
			: base(workItem)
		{
			InitializeComponent();
			SetUpRelatedTabPage();

			if (workflowTab != null)
			{
				int index = MainTabControl.TabPages.IndexOf(WorkflowTabPage);
				MainTabControl.TabPages.Remove(WorkflowTabPage);
				WorkflowTabPage.Dispose();
				WorkflowTabPage = workflowTab;
				MainTabControl.TabPages.Insert(WorkflowTabPage, index);
			}

			VisibilityConfigurationProvider.SetIsVisibilityConfigured(LeftTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(MiddleTopPanel, true);
			VisibilityConfigurationProvider.SetIsVisibilityConfigured(RightTopPanel, true);

			ReleaseSequence.Visible = ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled;

			// NOTE: BottomLeft panel deliberately excluded since its too big for anything but the details text

			PlugIns.Add(ControllerIDs.eConversationPlugIn);
			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Audit);

			WorkflowTabPage.Initialize(workItem);
			SetupActionsMenu();
			SetUpEConversationPlugIn();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type StateUserControlType
		{
			get { return State.UserControlType; }
			set { State.UserControlType = value; }
		}

		#region Layout

		protected override void OnLayout(LayoutEventArgs args)
		{
			base.OnLayout(args);
			UpdateRightTopPanelSplitter();

			if (pendingChildVisibilityChange)
			{
				pendingChildVisibilityChange = false;
				UpdateTopPanelHeight();
			}

			if (splitContainerTop != null && splitContainerTopLeft != null && splitContainerTop.SplitterDistance > splitContainerTopLeft.Width)
			{
				splitContainerTop.SplitterDistance = splitContainerTopLeft.Width;
			}
		}

		/// <summary>
		/// Collapse/Expand the splitter pane containing RightTopPanel if there is no/some visible content.
		/// </summary>
		void UpdateRightTopPanelSplitter()
		{
			var splitter = splitContainerTop;
			if (RightTopPanel != null && splitter != null && !IsDisposed && Visible)
			{
				bool visible = !IsBlank(RightTopPanel);
				if (visible == splitter.Panel2Collapsed)
				{
					splitter.Panel2Collapsed = !visible;
				}
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
							maxPreferredHeight = Math.Max(maxPreferredHeight, child.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(12));
						}
					}
				}
			}

			if (maxPreferredHeight >= ControlDpiScalingHelper.ScaleToCurrentDpiY(RowLayout.DefaultRowHeight) && splitContainerMain.Width != 0)
			{
				splitContainerMain.SplitterDistance = Math.Max(0, maxPreferredHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(20));
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

		static bool IsBlank(Control parent)
		{
			bool result = true;
			foreach (Control child in parent.Controls)
			{
				if (ControlVisibleCalculator.IsSetVisible(child))
				{
					result = false;
					break;
				}
			}
			return result;
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

		#endregion

		#region WorkItem

		protected WorkItem WorkItem
		{
			get { return (WorkItem)DataSource; }
		}

		#endregion

		#region Menu

		void SetupActionsMenu()
		{
			var items = new List<MenuItem>();

			MenuItem cancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("B46C546E-C714-4C27-A99A-98C4081CD7D9", "&Cancel"), delegate
			{ WorkItem.Cancel(); });
			items.Add(cancelMenuItem);

			ActionsMenuItem.MenuItems.AddRange(items.ToArray());
		}

		#endregion

		#region Plugins Setup

		protected virtual void SetUpEConversationPlugIn()
		{
			var eConversation = PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);
			((EConversationFullControl)eConversation.UserControl).ShouldShowAddInternalCommentButton = true;
		}

		#endregion

		public override string FormCaption
		{
			get
			{
				var workItem = WorkItem;
				if (workItem != null && workItem.WKI_WorkItemNumber != string.Empty)
				{
					var caption = new StringBuilder();
					caption.Append(workItem.WKI_WorkItemNumber);
					caption.Append(" - ");
					caption.Append(workItem.WKI_Summary);
					return caption.ToString();
				}
				else
				{
					return Res.GetString("ProcessManagement|WorkItemForm|FromCaptionPrefix", "Work Item");
				}
			}
		}

		public override bool IsResizableByTabPageAllowed => true;

#if DEBUG
		public ControlVisibilityConfigurationProvider GetVisibilityConfigurationProviderForTest()
		{
			return VisibilityConfigurationProvider;
		}
#endif

		protected override void OnMainTabControlSelecting(int tabPageIndex)
		{
			base.OnMainTabControlSelecting(tabPageIndex);

			if (tabPageIndex == 0 && WorkItemCustomFields != null)
			{
				WorkItemCustomFields.Visible = true;
			}
		}
	}
}
