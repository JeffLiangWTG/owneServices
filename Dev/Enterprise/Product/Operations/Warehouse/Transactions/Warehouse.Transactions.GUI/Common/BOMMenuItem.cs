using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class BOMMenuItem : ZMenuItem
	{
		#region Construction

		public BOMMenuItem(WhsWorkOrder docket)
		{
			this.docket = docket;
			this.Caption = ResString.GetMultilingualString("64D07596-4E75-4e46-B66E-DEA4D309CB5C", "Expand all Lines by their Bills of Materials");
		}

		public BOMMenuItem(WorkOrderDocketLinesGridUserControl parentControl)
		{
			this.parentControl = parentControl;
			this.Caption = ResString.GetMultilingualString("BF63D6CD-5961-468b-A34A-15D35346D920", "Expand Selected Lines by their Bills of Materials");
		}

		WhsWorkOrder Docket
		{
			get { return IsOnGridSubMenu ? parentControl.Docket : docket; }
		}

		readonly WhsWorkOrder docket;
		readonly WorkOrderDocketLinesGridUserControl parentControl;

		#endregion

		#region OnClick

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (IsOnGridSubMenu)
			{
				ExpandSelectedLines();
			}
			else
			{
				ExpandAllLines();
			}

			Checked = !Checked;
		}

		void ExpandAllLines()
		{
			if (Checked)
			{
				Docket.BOM.CollapseAllLines();
			}
			else
			{
				Docket.BOM.ExpandAllLines();
			}
		}

		void ExpandSelectedLines()
		{
			if (Checked)
			{
				Docket.BOM.CollapseLines(parentControl.GetSelectedLines());
			}
			else
			{
				Docket.BOM.ExpandLines(parentControl.GetSelectedLines());
			}
		}

		bool IsOnGridSubMenu
		{
			get { return parentControl != null; }
		}

		#endregion

		#region Updating Checked / Enabled States

		public void UpdateCheckedAndEnabledStateForAllLines()
		{
			Enabled = Docket.Lines.Count > 0;
			Checked = Enabled && Docket.AllLines.Count == Docket.Lines.Count; // all lines are expanded if this is true.
		}

		public void UpdateCheckedAndEnabledStateForSelectedLines(WhsWorkOrderLine[] selectedLines)
		{
			Enabled = IsSelectedLineBOMProduct(selectedLines);
			Checked = IsSelectedLineFullyExpanded(selectedLines);
		}

		bool IsSelectedLineFullyExpanded(WhsWorkOrderLine[] selectedLines)
		{
			return IsSelectedLineBOMProduct(selectedLines) && IsLineFullyExpanded(selectedLines[0]);
		}

		bool IsLineFullyExpanded(WhsWorkOrderLine parentLine)
		{
			foreach (WhsWorkOrderLine childLine in parentLine.BOM.ChildComponentLines)
			{
				if (!childLine.BOM.IsExpanded || !IsLineFullyExpanded(childLine))
				{
					return false;
				}
			}

			return true;
		}

		bool IsSelectedLineBOMProduct(WhsWorkOrderLine[] selectedLines)
		{
			return selectedLines.Length == 1 && selectedLines[0].IsBOMProduct;
		}

		#endregion
	}
}
