using System;
using System.Collections;
using System.Windows.Forms;

using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;

using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class DragAndDropGridManager : IDisposable
	{
		public DragAndDropGridManager(ZGrid grid)
		{
			Argument.NotNull(grid, "Grid");
			Grid = grid;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public void ManageGrid()
		{
			Grid.AllowDrop = true;
			Grid.DragOver += new DragEventHandler(UserControl_DragOver);
			Grid.DragEnter += new DragEventHandler(UserControl_DragEnter);
			Grid.DragDrop += new DragEventHandler(UserControl_DragDrop);
		}

		#region Events

		void UserControl_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ArrayList)))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		void UserControl_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ArrayList)))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		void UserControl_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ArrayList)))
			{
				BusinessObject[] list = Aggregator.Filter<BusinessObject>((ArrayList)e.Data.GetData(typeof(ArrayList)));
				OnDragDropCore(list);
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Grid != null)
				{
					Grid.DragOver -= new DragEventHandler(UserControl_DragOver);
					Grid.DragEnter -= new DragEventHandler(UserControl_DragEnter);
					Grid.DragDrop -= new DragEventHandler(UserControl_DragDrop);
				}
			}

			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

		public void OnDragDropCore(BusinessObject[] list)
		{
			if (Docket is ICreateDocketLineFromInventory docketThatCanCreateDocketLine)
			{
				docketThatCanCreateDocketLine.AcceptInventoryLinesFromSearchGrid(list);
			}
		}

		WhsDocket Docket
		{
			get
			{
				WhsDocket result = null;
				if (Grid != null)
				{
					ZUserControl userControl = Grid.Parent as ZUserControl;
					if (userControl != null)
					{
						result = userControl.CurrentDataItem as WhsDocket;
						WhsPick pick = userControl.CurrentDataItem as WhsPick;
						if (pick != null && pick.Orders.Count > 0)
						{
							result = pick.Orders[0];
						}
					}
				}
				return result;
			}
		}

		readonly ZGrid Grid;

		#endregion
	}
}
