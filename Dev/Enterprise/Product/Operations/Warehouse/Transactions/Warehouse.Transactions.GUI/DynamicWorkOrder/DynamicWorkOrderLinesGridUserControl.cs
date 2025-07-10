using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	abstract class DynamicWorkOrderLinesGridUserControl : DocketLinesGridUserControl
	{
		public DynamicWorkOrderLinesGridUserControl()
		{
			RemoveBOMColumn();
		}

		void RemoveBOMColumn()
		{
			LinesGrid.GetColumnStyle(WhsDocketLine.Schema.IsBOMProduct).IsUnavailable = true;
		}

		protected override bool SupportsDuplicateMenuItem => false;
	}
}
