using System;
using System.Linq;
using CargoWise.Common.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	/// <summary>
	/// Test is on OrderEntryForm.
	/// </summary>
	class ShortfallGridManager : IDisposable
	{
		public ShortfallGridManager(ZGrid grid, IShortfallGridManagerUpdateStrategy updateStrategy)
		{
			Grid = grid;
			UpdateStrategy = updateStrategy;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public void ManageGrid()
		{
			Grid.AfterBind += new EventHandler(Grid_AfterBind);
			Grid.Leave += new EventHandler(Grid_Leave);
			Grid.Enter += new EventHandler(Grid_Enter);
		}

		readonly ZGrid Grid;
		readonly IShortfallGridManagerUpdateStrategy UpdateStrategy;

		#region Changing Rows

		void Grid_AfterBind(object sender, EventArgs e)
		{
			Grid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
			SetPreviouslySelectedRow();
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			OnOrderLineChanged();
		}

		void OnOrderLineChanged()
		{
			// when changing rows, update the row we just moved off of.
			if (PreviouslySelectedRow != null && !PreviouslySelectedRow.IsDeleted)
			{
				PreviouslySelectedRow.Shortfall.ResumeShortfallCalculation();
				if (PreviouslySelectedRow.Shortfall.HasProductUnitsOrAttribsChanged && PreviouslySelectedRow.Docket != null)
				{
					foreach (var line in PreviouslySelectedRow.Docket.Lines.Cast<WhsPickableDocketLine>().Where(l => l.Shortfall.HasProductUnitsOrAttribsChanged))
					{
						UpdateStrategy.Update(line);
					}
				}
			}

			// wait until user leaves the row to calculate shortfall because thats
			// the only way we can be sure they have entered all optional fields like attributes
			SuspendShortfallCalculation();

			SetPreviouslySelectedRow();
		}

		void SuspendShortfallCalculation()
		{
			var currentRow = CurrentRow;
			if (currentRow != null && !currentRow.IsDeleted && !currentRow.Shortfall.IsShortfallCalculationSuspended)
			{
				currentRow.Shortfall.SuspendShortfallCalculation();
			}
		}

		void SetPreviouslySelectedRow()
		{
			var currentRow = CurrentRow;
			PreviouslySelectedRow = currentRow != null && !currentRow.IsDeleted ? currentRow : null;
		}

		WhsPickableDocketLine PreviouslySelectedRow;

		#endregion

		#region Entering the Grid

		void Grid_Enter(object sender, EventArgs e)
		{
			if (Grid.ListManager != null && CurrentRow != null)
			{
				SuspendShortfallCalculation();
			}
		}

		#endregion

		#region Leaving the Grid

		void Grid_Leave(object sender, EventArgs e)
		{
			if (Grid?.ListManager != null)
			{
				// when leaving the grid, the selected row's values will need updating.
				var selectedRow = CurrentRow;
				if (selectedRow != null)
				{
					selectedRow.Shortfall.ResumeShortfallCalculation();
					if (selectedRow.Shortfall.HasProductUnitsOrAttribsChanged && PreviouslySelectedRow?.Docket != null && !PreviouslySelectedRow.IsDeleted)
					{
						foreach (var line in PreviouslySelectedRow.Docket.Lines.Cast<WhsPickableDocketLine>().Where(l => l.Shortfall.HasProductUnitsOrAttribsChanged))
						{
							line.Validation.ValidateWE_ShortfallQuantityCached();

							// if user changes the shortfall, presses tab, then leaves the grid, the shortfall qty is not updated without this line.
							line.WE_ShortfallQuantityCachedInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (Grid != null)
			{
				Grid.AfterBind -= new EventHandler(Grid_AfterBind);
				Grid.Leave -= new EventHandler(Grid_Leave);
				if (Grid.ListManager != null)
				{
					Grid.ListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
				}
			}

			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

		WhsPickableDocketLine CurrentRow
		{
			get { return (Grid.ListManager.Position >= 0) ? Grid.ListManager.GetCurrent() as WhsPickableDocketLine : null; }
		}

		#endregion
	}
}
