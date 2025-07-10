using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	// Note: this class ideally should be abstract, but in some cases that
	// causes the WinForms designer to fail because it can't create an instance
	// of this class
	public class TemplateBasedControl : ViewModelBasedControl
	{
		protected virtual IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ItemTemplateControlBase();
		}

		protected virtual Type GetItemControlType(object data)
		{
			return typeof(ItemTemplateControlBase);
		}

		protected IItemTemplateControl GetItemControl(object data)
		{
			// 1st attempt: reuse control already bound to this data
			if (DataToChildControlMap.TryGetValue(data, out var alreadyBound))
			{
				return alreadyBound;
			}

			// 2nd attempt: retrieve from object pool
			IItemTemplateControl itemCtrl = null;
			if (ReuseExistingItemControls)
			{
				var itemType = GetItemControlType(data);
				if (ControlObjectPool.ContainsKey(itemType) && ControlObjectPool[itemType].Count > 0)
				{
					itemCtrl = (IItemTemplateControl)ControlObjectPool[itemType].Dequeue();
				}
			}

			// 3rd attempt: create a new one
			itemCtrl = itemCtrl ?? CreateNewItemControl(data);
			itemCtrl.DataBind(data);
			itemCtrl.SelectionChanged += ChildControlSelectionChanged;

			return itemCtrl;
		}

		public object SelectedItem
		{
			get
			{
				var selectedControl = ChildControlToDataMap.Keys.FirstOrDefault(c => c.IsSelected);
				if (selectedControl != null)
				{
					return ChildControlToDataMap[selectedControl];
				}

				return null;
			}
			set
			{
				if (value is null)
				{
					DataToChildControlMap.Values.ForEach(x => x.IsSelected = false);
				}
				else if (DataToChildControlMap.ContainsKey(value))
				{
					DataToChildControlMap[value].IsSelected = true;
				}
			}
		}

		Panel containerPanel;
		Panel ContainerPanel => containerPanel ?? (containerPanel = GetContainerPanel());

		protected virtual Panel GetContainerPanel()
		{
			var mainPanel = new ZPanel();
			Controls.Add(mainPanel);
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.AutoScroll = true;

			return mainPanel;
		}

		protected virtual IEnumerable GetItemsDataCore()
		{
			if (CurrentDataItem != null && CurrentDataItem is IEnumerable enumerableData)
			{
				return enumerableData;
			}

			return null;
		}

		protected virtual IEnumerable<object> GetItemsData()
		{
			var itemsData = GetItemsDataCore();
			if (itemsData == null)
			{
				return null;
			}

			return itemsData.Cast<object>().Reverse();
		}

		protected virtual void RenderLayout()
		{
			var panel = ContainerPanel;
			if (panel == null)
			{
				return;
			}

			var data = GetItemsData() ?? Array.Empty<object>();

			ClearMainPanelItemControls(panel, data);

			panel.SuspendLayout();

			var controlsToAdd = new List<Control>(); // bulk add, better performance than one-by-one
			foreach (var itemData in data)
			{
				if (!ShouldRenderItem(itemData))
				{
					continue;
				}

				var itemCtl = GetItemControl(itemData);
				controlsToAdd.Add((Control)itemCtl);
				ApplyExtraSettingsOnEachControl((Control)itemCtl);

				DataToChildControlMap[itemData] = itemCtl;
				ChildControlToDataMap[itemCtl] = itemData;
			}

			panel.Controls.AddRange(controlsToAdd.ToArray());
			panel.ResumeLayout(false);
			panel.PerformLayout();
		}

		protected virtual void ApplyExtraSettingsOnEachControl(Control control)
		{
			control.Dock = DockStyle.Top;
		}

		protected virtual bool ShouldRenderItem(object data) => data != null;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (!IsDisposing && RenderLayoutOnCurrentDataItemChanged)
			{
				RenderLayout();
			}
		}

		protected virtual bool RenderLayoutOnCurrentDataItemChanged => true;

		// Inherited controls can set this property to "true".
		// When it's true, we don't dispose the controls on subsequent data bounds.
		// We keep the items in a queue and reuse them in next data bound.
		protected virtual bool ReuseExistingItemControls => false;

		protected virtual void SetSelectedItemOnViewModel(object selectedData)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (IsDisposing && ReuseExistingItemControls)
			{
				foreach (var queue in ControlObjectPool.Values)
				{
					while (queue.Count > 0)
					{
						var control = (Control)queue.Dequeue();
						if (!control.IsDisposed)
						{
							control.Dispose();
						}
					}
				}
			}
		}

		#region implementation
		void ClearMainPanelItemControls(Panel panel, IEnumerable<object> incomingData)
		{
			panel.SuspendLayout();

			var incomingDataHash = new HashSet<object>(incomingData);

			var controlsToRemove = panel.Controls.Cast<Control>().ToArray();
			panel.Controls.Clear(); // bulk clear, better performance than one-by-one
			foreach (var control in controlsToRemove)
			{
				if (control is IItemTemplateControl itemControl)
				{
					var controlData = ChildControlToDataMap[itemControl];

					if (incomingDataHash.Contains(controlData))
					{
						// Control is about to be re-added. We can reuse this
						// control without re-binding

						// Leave it in our mapping dictionaries for later
						// retrieval
						continue;
					}

					// If we reach this point, the item will not be re-added.
					// Return the control to the object pool

					DataToChildControlMap.Remove(controlData);
					ChildControlToDataMap.Remove(itemControl);

					if (ReuseExistingItemControls)
					{
						itemControl.ClearSelection();
						itemControl.SelectionChanged -= ChildControlSelectionChanged;
						itemControl.DataBind(null);

						var controlObjectPool = ControlObjectPool.GetOrAdd(control.GetType(), () => new Queue());
						controlObjectPool.Enqueue(control);
					}
					else if (!control.Disposing || control.IsDisposed)
					{
						control.Dispose();
					}
				}
			}

			panel.ResumeLayout();
		}

		readonly Dictionary<object, IItemTemplateControl> DataToChildControlMap = new Dictionary<object, IItemTemplateControl>();
		readonly Dictionary<IItemTemplateControl, object> ChildControlToDataMap = new Dictionary<IItemTemplateControl, object>();

		readonly Dictionary<Type, Queue> ControlObjectPool = new Dictionary<Type, Queue>();

		void ChildControlSelectionChanged(object sender, EventArgs e)
		{
			if (sender is IItemTemplateControl itemTemplateControl)
			{
				if (itemTemplateControl.IsSelected)
				{
					foreach (var item in ChildControlToDataMap.Keys)
					{
						if (item != itemTemplateControl)
						{
							item.ClearSelection();
						}
					}
				}

				var selected = ChildControlToDataMap.FirstOrDefault(c => c.Key.IsSelected);
				if (selected.Key != null)
				{
					SetSelectedItemOnViewModel(selected.Value);
				}
				else
				{
					SetSelectedItemOnViewModel(null);
				}
			}
		}
		#endregion
	}
}
