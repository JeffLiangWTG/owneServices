using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class WorkTaskRelatedItemUserControl : ZUserControl
	{
		public WorkTaskRelatedItemUserControl(bool isViewOrDeleteMode)
			: this()
		{
			if (ObjectFactory.Get<IBMSRegistry>().IsPlanningManagementEnabled)
			{
				ShowNetworkDiagrams();
				ShowParentAndChildWorklows();
			}
			else
			{
				HideNetworkDiagrams();
				HideParentAndChildWorkflows();
			}

			if (isViewOrDeleteMode)
			{
				RelatedItemGrid.Enabled = false;
				NewButton.Enabled = false;
				AttachButton.Enabled = false;
				EditButton.Enabled = false;
				DetachButton.Enabled = false;
			}
		}

		// Seems like we need this parameterless contructor to make the designer work with inherited controls like EDIWorkTaskRelatedItemUserControl
		public WorkTaskRelatedItemUserControl()
		{
			InitializeComponent();
		}

		#region Show / Hide Grids

		void ShowNetworkDiagrams()
		{
			var controlProvider = ObjectFactory.Get<IJobRelatedDiagramsUserControlProvider>();
			networkDiagramsUserControl = controlProvider.GetUserControl();

			var control = networkDiagramsUserControl as Control;

			if (control != null)
			{
				NetworkDiagramGroupBox.Controls.Add(control);
				control.Dock = DockStyle.Fill;
			}
		}

		void HideNetworkDiagrams()
		{
			splitContainerTop.Panel2Collapsed = true;
		}

		void ShowParentAndChildWorklows()
		{
			var controlProvider = ObjectFactory.Get<IJobRelatedWorkflowsControlsProvider>();

			parentWorkflowsUserControl = controlProvider.GetRelatedParentWorkflowsControl();

			if (parentWorkflowsUserControl is Control parentControl)
			{
				ParentWorkflowGroupBox.Controls.Add(parentControl);
				parentControl.Dock = DockStyle.Fill;
			}

			childWorkflowsUserControl = controlProvider.GetRelatedChildWorkflowsControl();

			if (childWorkflowsUserControl is Control childControl)
			{
				ChildWorkflowGroupBox.Controls.Add(childControl);
				childControl.Dock = DockStyle.Fill;
			}
		}

		void HideParentAndChildWorkflows()
		{
			splitContainerMain.Panel2Collapsed = true;
		}

		#endregion

		#region Placing Additional Controls For Extensions

		protected void PlaceAdditionalControlForRelatedItems(Control control)
		{
			AdditionalRelatedItemsPanel.Controls.Add(control);
		}

		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ClearExistingMenuStripItems();
			base.SetDataBinding(dataSource, dataMember);
			InitialiseMenuStripItems();

			if (dataSource is IWorkflowProviderCore workflowProvider
				&& dataSource is BusinessObject bizo
				&& !ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowProvider, bizo.Factory))
			{
				HideNetworkDiagrams();
				HideParentAndChildWorkflows();
			}
		}

		#region Menu Strip Items

		void ClearExistingMenuStripItems()
		{
			foreach (var item in menuStripNew.Items.Cast<ToolStripItem>().ToArray())
			{
				item.Dispose();
			}
			menuStripNew.Items.Clear();

			foreach (var item in menuStripAttach.Items.Cast<ToolStripItem>().ToArray())
			{
				item.Dispose();
			}
			menuStripAttach.Items.Clear();
		}

		void InitialiseMenuStripItems()
		{
			if (RelatedItemSource != null)
			{
				foreach (var relatedItem in RelatedItemSource.SupportedRelatedItemModules)
				{
					if (relatedItem.AllowNew)
					{
						var controllerID = relatedItem.ControllerID;
						var type = relatedItem.Type;
						AddNewMenuItem(menuStripNew, relatedItem.Caption, controllerID,
							delegate(object sender, EventArgs e)
							{
								ShowNewItemForm(controllerID, type, sender);
							});
					}

					if (relatedItem.AllowAttach)
					{
						var info = relatedItem;
						menuStripAttach.Items.Add(relatedItem.Caption, null,
							delegate
							{
								ShowRecordAttacher(info);
							});
					}
				}

				if (menuStripNew.Items.Count < 1)
				{
					NewButton.Visible = false;
				}

				if (menuStripAttach.Items.Count < 1)
				{
					AttachButton.Visible = false;
				}
			}
		}

		protected virtual void AddNewMenuItem(KContextMenuStrip contextMenuStrip, string caption, ControllerID controllerID, EventHandler onClick)
		{
			contextMenuStrip.Items.Add(caption, null, onClick);
		}

		protected virtual void ShowNewItemForm(ControllerID controllerID, string type, object sender)
		{
			LastController = ZControllerFactory.Create(controllerID);
			NewItemHelper.AddNewItem(LastController, RelatedItemSource, RelatedItemSource.RelatedItems, type, sender == null);
		}

		#endregion

		#region Detach

		protected virtual void DetachButton_Click(object sender, EventArgs e)
		{
			if (RelatedItemGrid.ListManager.Position > -1)
			{
				string msg = Res.GetString("0C3C39FC-EDE5-4912-920C-4B38E08FC154", "Are you sure you want to detach the selected related item(s)?");
				string caption = Res.GetString("C42CC04A-7D66-426D-89D4-7DF5AB7A901E", "Confirm Detach...");
				DialogResult dialogResult = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					var relatedItems = RelatedItemGrid.SelectedElements;
					foreach (var relatedItem in relatedItems)
					{
						RelatedItemSource.RelatedItems.Remove(relatedItem);
					}
				}
			}
		}

		#endregion

		#region Open Items

		void RelatedItemGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void OpenRelatedItem()
		{
			if (RelatedItemGrid.ListManager.Position > -1)
			{
				var relatedItem = (IWorkTaskRelatedItem)RelatedItemGrid.ListManager.GetCurrent();
				LastController = ZControllerFactory.Create(relatedItem.ControllerID);
				LastController.ShowEditForm((BusinessObject)relatedItem);
			}
		}

		#endregion

		#region Implementation

		IJobRelatedDiagramsUserControl networkDiagramsUserControl;
		IJobRelatedParentChildWorkflowsControl parentWorkflowsUserControl;
		IJobRelatedParentChildWorkflowsControl childWorkflowsUserControl;

		public new IWorkTaskRelatedItem CurrentDataItem
		{
			get { return (IWorkTaskRelatedItem)base.CurrentDataItem; }
		}

		IWorkTaskRelatedItemSource RelatedItemSource
		{
			get { return (IWorkTaskRelatedItemSource)BindingSource.Current; }
		}

		void ShowRecordAttacher(WorkTaskRelatedItemModuleInfo relatedItem)
		{
			ModuleIdentifier moduleID = relatedItem.ModuleID;

			if (ShouldShowRecordAttacher(moduleID))
			{
				NewItemHelper.SetAdditionalFilter(relatedItem);

				ShowRecordAttacherCore(relatedItem.FindBoxList, moduleID);

				LastAttacher.Show((IZForm)FindForm());
			}
		}

		protected virtual bool ShouldShowRecordAttacher(ModuleIdentifier moduleID)
		{
			return true;
		}

		protected virtual void ShowRecordAttacherCore(IBusinessObjectCollection lookupsCollection, ModuleIdentifier moduleID)
		{
			LastAttacher = new WorkTaskAttacher(RelatedItemSource.RelatedItems, lookupsCollection, moduleID);
		}

		public ZRecordAttacher LastAttacher;
		public ZController LastController;

		#endregion

		void NewButton_Click(object sender, EventArgs e)
		{
			menuStripNew.Show(NewButton, ControlDpiScalingHelper.NewScaledPoint(0, 0), ToolStripDropDownDirection.AboveRight);
		}

		void AttachButton_Click(object sender, EventArgs e)
		{
			menuStripAttach.Show(AttachButton, ControlDpiScalingHelper.NewScaledPoint(0, 0), ToolStripDropDownDirection.AboveRight);
		}

		#region For Test
#if DEBUG
		public KContextMenuStrip MenuStripAttach_ForTest => menuStripAttach;
		public KContextMenuStrip MenuStripNew_ForTest => menuStripNew;
#endif
		#endregion
	}
}
