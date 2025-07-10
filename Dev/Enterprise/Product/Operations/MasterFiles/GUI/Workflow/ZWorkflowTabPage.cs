using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class ZWorkflowTabPage : ZBindingTabPage, IWorkflowTabPage, ILicensedComponent
	{
		public ZWorkflowTabPage()
		{
			Name = "WorkflowTabPage";
			Text = Res.GetString("TemplateTab.Workflow", "Workflow");

			MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(935);
			MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(520);

			TrackingUserControl.Dock = DockStyle.Fill;
			Controls.Add(TrackingUserControl);
		}

		public void Initialize(IWorkflowProvider workflowProvider)
		{
			this.WorkflowProvider = workflowProvider;

			WorkflowDescriptor workflowDescriptor = GetWorkflowDescriptor(workflowProvider.WorkflowType);
			SupportsEventTracking = (workflowDescriptor != null && workflowDescriptor.SupportsEventTracking);

			AddSendUniversalXmlMenuItem(workflowProvider);

			AddReapplyTemplatesMenuItemIfRequired(workflowDescriptor, workflowProvider);

			initialized = true;

			if (FindForm() is IHotkeyProvider form && !form.Hotkeys.IsRegistered(Keys.Control | Keys.W))
			{
				form.Hotkeys.RegisterHotKey(Keys.Control | Keys.W, SelectWorkflowTab, Res.GetString("843717F2-C1FA-488C-BF9B-F14DCD16F8FF", "Select Workflow Tab"));
			}
		}

		bool initialized;
		bool IWorkflowTabPage.Initialized { get { return initialized; } }

		public void SetVisibility(bool visible)
		{
			if (initialized)
			{
				TabVisible = visible;

				if (universalXmlMenuItem != null)
				{
					universalXmlMenuItem.Visible = visible;
				}

				if (reapplyWorkflowMenuItem != null)
				{
					reapplyWorkflowMenuItem.Visible = visible;
				}
			}
		}

		void SelectWorkflowTab()
		{
			var parentTabControl = Parent as ZTabControl;
			if (parentTabControl != null)
			{
				parentTabControl.SelectedTab = this;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool ExcludeFromBindingOnSave
		{
			get { return true; }
		}

		public override void ClearNotificationImage()
		{
			base.ClearNotificationImage();
			UpdateExceptionIcon();
		}

		protected sealed override bool IsAutoSized
		{
			get { return true; }
		}

		public void ConsumeWorkflowLicence()
		{
			if (!isWorkflowLicenceConsumed)
			{
				Env.Licence.Workflow.Login(this);
				isWorkflowLicenceConsumed = true;
			}
		}

		bool isWorkflowLicenceConsumed;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && !DesignModeFinder.IsDesigning)
			{
				Application.Idle -= new EventHandler(WorkflowItems_ListChanged_OnIdle);
				if (WorkflowProvider != null && IsBound)
				{
					((IBindingList)WorkflowProvider.WorkflowItems.ExceptionsIncludingRelated).ListChanged -= new ListChangedEventHandler(WorkflowItems_ListChanged);
				}
				if (isWorkflowLicenceConsumed)
				{
					Env.Licence.Workflow.Logout(this);
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		ZForm Form
		{
			get { return FindForm() as ZForm; }
		}

		#region Hiding Properties

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int ImageIndex
		{
			get { return base.ImageIndex; }
			set { base.ImageIndex = value; }
		}

		#endregion

		#region NavigateToWorkflowItem

		public void NavigateToWorkflowItem(ProcessTask task)
		{
			NavigateToWorkflowItem(() => TrackingUserControl.NavigateToWorkflowItem(task));
		}

		public void NavigateToWorkflowItem(IProcessHeader workflow)
		{
			NavigateToWorkflowItem(() => TrackingUserControl.NavigateToWorkflowItem(workflow));
		}

		void NavigateToWorkflowItem(Action navigationAction)
		{
			var zTabControl = Parent as ZTabControl;

			if (zTabControl != null)
			{
				zTabControl.SelectedTab = this;

				// SelectedTab setter may have been cancelled
				if (zTabControl.SelectedTab == this)
				{
					navigationAction();
				}
			}
			else
			{
				Globals.Message.ShowError(
					Res.GetString("336ecadd-6a55-4fae-aa6a-1b243fed70d3", "Cannot navigate to workflow & tracking tab because it is hidden."),
					Res.GetString("d902273a-c2ce-4110-ae80-4eefe5c59bd7", "Hidden workflow & tracking tab"));
			}
		}

		#endregion

		#region IWorkflowTabPage

		bool IWorkflowTabPage.SupportsEventTracking
		{
			get { return SupportsEventTracking; }
		}

		bool SupportsEventTracking
		{
			get { return supportsEventTracking; }
			set
			{
				supportsEventTracking = value;
				Text = value ? WorkflowAndTrackingTabText : WorkflowTabText;
			}
		}
		bool supportsEventTracking;

		#endregion

		#region ILicensedComponent Members

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return LicensedComponentManager ?? (LicensedComponentManager = new LicensedComponentManager(this)); }
		}
		LicensedComponentManager LicensedComponentManager;

		#endregion

		#region Send Universal XML Actions Menu Item

		void AddSendUniversalXmlMenuItem(IWorkflowProvider workflowProvider)
		{
			if (!(workflowProvider is BusinessObject parentBO)
				|| !(Form is IFileMenuItemsProvider parentForm)
				|| parentForm.ActionsMenuItem is null
				|| !(parentBO.GetUniversalDataContextManager() is IDataContextManager dataContextManager))
			{
				return;
			}

			universalXmlMenuItem = BuildSendUniversalXmlMenuItem(workflowProvider, dataContextManager);

			if (universalXmlMenuItem.MenuItems.Count > 0)
			{
				ZFormMenuStrategy.AddActionsMenuItem(parentForm, universalXmlMenuItem);
			}
		}

		MenuItem universalXmlMenuItem;

		static MenuItem BuildSendUniversalXmlMenuItem(IWorkflowProvider workflowProvider, IDataContextManager dataContextManager)
		{
			MenuItem result = new ZMenuItem(ResString.GetMultilingualString("ZWorkflowTabPage|ActionsMenu|SendUniversalXML", "Send Universal XML"));

			var universalMenuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport, () => new[] { workflowProvider }, dataContextManager);

			foreach (var menuDescriptor in universalMenuBuilder.Build())
			{
				result.MenuItems.Add(new ZMenuItem(menuDescriptor.Caption, menuDescriptor.Handler));
			}

			return result;
		}
		#endregion

		#region Reapply Workflow Templates Menu Item

		void AddReapplyTemplatesMenuItemIfRequired(WorkflowDescriptor workflowDescriptor, IWorkflowProvider workflowProvider)
		{
			if (!workflowDescriptor.SupportsReapplyTemplatesMenuItem
				|| !(workflowProvider is BusinessObject parentBO)
				|| !(Form is IFileMenuItemsProvider parentForm)
				|| parentForm.ActionsMenuItem is null)
			{
				return;
			}

			reapplyWorkflowMenuItem = (ZMenuItem)ObjectFactory.Get<IWorkflowReapplyTemplatesMenuItem>("ReapplyWorkflowTemplateMenuItemProvider").GetReapplyWorkflowTemplateMenuItemForBusinessObjectFrom(parentBO);
			ZFormMenuStrategy.AddActionsMenuItem(parentForm, reapplyWorkflowMenuItem);
		}

		ZMenuItem reapplyWorkflowMenuItem;

		#endregion

		#region Implementation

		static string WorkflowTabText { get { return Res.GetString("ZWorkflowTabPage|WorkflowTabText", "Workflow"); } }
		static string WorkflowAndTrackingTabText { get { return Res.GetString("ZWorkflowTabPage|WorkflowAndTrackingTabText", "Workflow && Tracking"); } }

		internal IWorkflowProvider WorkflowProvider { get; private set; }

		protected override void UpdateInitialTabImageCore()
		{
			base.UpdateInitialTabImageCore();
			var idleQueueState = UserIdleWorkItemOptions.DisableSlowRunningWarning; // Disabling due to intermittent error reports when DB slowness causes unreproducable occurrences.
			UserIdleWorker.QueueWorkItem(this, (NoResString)"Update Workflow Exception Icon", idleQueueState, new MethodInvoker(UpdateExceptionIcon));
		}

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!initialized)
			{
				ErrorReporter.ReportOnce(
					"InitializeMustBeCalledFromForm_" + FindForm().GetType().FullName,
					"You must must add WorkflowTabPage.Initialize() into the constructor of form " + FindForm().GetType().Name + ". This is required to update the tab page caption.");
			}

			IWorkflowProvider provider = GetWorkflowProvider(dataSource, dataMember);
			if (provider != null)
			{
				((IBindingList)provider.WorkflowItems.ExceptionsIncludingRelated).ListChanged -= WorkflowItems_ListChanged;
			}
			base.SetDataBindingCore(dataSource, dataMember);
			provider = GetWorkflowProvider(dataSource, dataMember);
			if (provider != null)
			{
				((IBindingList)provider.WorkflowItems.ExceptionsIncludingRelated).ListChanged += WorkflowItems_ListChanged;
			}
		}

		protected internal virtual ZWorkflowUserControl TrackingUserControl
		{
			get
			{
				if (trackingUserControl == null)
				{
					trackingUserControl = new ZWorkflowUserControl();
				}
				return trackingUserControl;
			}
		}
		ZWorkflowUserControl trackingUserControl;

		IWorkflowProvider GetWorkflowProvider(object dataSource, string dataMember)
		{
			IWorkflowProvider result = null;
			BindingManagerBase bindingManager = BindingContext[dataSource, dataMember];
			CurrencyManager currencyManager = bindingManager as CurrencyManager;
			if (currencyManager != null && currencyManager.List is IWorkflowProvider)
			{
				result = currencyManager.List as IWorkflowProvider;
			}
			else
			{
				result = bindingManager.Position != -1 ? (IWorkflowProvider)bindingManager.GetCurrent() : null;
			}
			return result;
		}

		protected virtual WorkflowDescriptor GetWorkflowDescriptor(ZString workflowType)
		{
			return WorkflowDescriptors.Instance.TryGetValueSafe(workflowType);
		}

		void WorkflowItems_ListChanged(object sender, ListChangedEventArgs e)
		{
			Application.Idle -= new EventHandler(WorkflowItems_ListChanged_OnIdle);
			Application.Idle += new EventHandler(WorkflowItems_ListChanged_OnIdle);
		}

		void WorkflowItems_ListChanged_OnIdle(object sender, EventArgs e)
		{
			Application.Idle -= new EventHandler(WorkflowItems_ListChanged_OnIdle);
			UpdateExceptionIcon();
		}

		void UpdateExceptionIcon()
		{
			if (WorkflowProvider != null)
			{
				bool hasUnactionedExceptions = WorkflowProvider.WorkflowItems.ExceptionsIncludingRelated.Cast<ProcessTask>().Any(e => e.P9_Status == ExceptionStatusCodeList.Codes.Open);

				if (hasUnactionedExceptions)
				{
					if (ImageIndex != Icons.GetImageIndex(IconTypes.Error))
					{
						ImageIndex = Icons.GetImageIndex(IconTypes.RedAlarmBell);
					}
				}
				else if (ImageIndex == Icons.GetImageIndex(IconTypes.RedAlarmBell))
				{
					ImageIndex = -1;
				}
			}
		}

		#endregion
	}
}
