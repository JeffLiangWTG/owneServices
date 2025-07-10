using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class GridSelectionManager
	{
		readonly ZGrid grid;
		readonly Func<Form> getParentForm;

		public GridSelectionManager(ZGrid grid, Func<Form> getParentForm, bool alwaysInvoke)
		{
			this.grid = grid;
			this.getParentForm = getParentForm;
			this.alwaysInvoke = alwaysInvoke;
		}

		public void ListChanged(object sender, ListChangedEventArgs e)
		{
			if (ParentForm != null && !ParentForm.IsDisposed && !ParentForm.IsDisposing)
			{
				ListChangedCore(e.ListChangedType);
			}
		}

		protected virtual void ListChangedCore(ListChangedType listChangedType)
		{
			if (listChangedType == ListChangedType.ItemChanged && !IsListNull && !IsFormSaving() && !settingLastSelectedItem.IsSuspended)
			{
				itemToSelectOnBinding = (IBusiness)grid.ListManager.GetCurrent();
			}
			else if (listChangedType == ListChangedType.Reset)
			{
				ReselectItem();
			}
		}

		bool IsFormSaving()
		{
			return ParentForm != null && ParentForm.IsSavingInProgress;
		}

		public void ReselectItem()
		{
			IBusiness item = null;

			if (itemToSelectOnBinding != null && !IsListNull)
			{
				item = itemToSelectOnBinding;
			}

			if (item != null)
			{
				var itemIndex = grid.List.IndexOf(item);

				if (itemIndex >= 0 && itemIndex != grid.ListManager.Position)
				{
					if (alwaysInvoke || grid.InvokeRequired || ParentForm.IsSavingInProgress)
					{
						grid.BeginInvoke(new Action(() => ChangeCurrentPosition(itemIndex)));
					}
					else
					{
						ChangeCurrentPosition(itemIndex);
					}
				}
			}
		}

		void ChangeCurrentPosition(int itemIndex)
		{
			if (grid.ListManager.Position != itemIndex)
			{
				using (settingLastSelectedItem.Suspend())
				{
					grid.ListManager.CancelCurrentEdit();
					grid.ListManager.Position = itemIndex;
				}
			}
		}

		public void SetItemToSelectOnBinding()
		{
			itemToSelectOnBinding = (IBusiness)grid.ListManager.GetCurrent();
		}

		IBusiness itemToSelectOnBinding;
		bool IsListNull => grid?.ListManager?.List == null;

		readonly ActionSuspender settingLastSelectedItem = new ActionSuspender();

		ZForm ParentForm => parentForm ?? (parentForm = getParentForm() as ZForm);
		ZForm parentForm;
		readonly bool alwaysInvoke;
	}
}
