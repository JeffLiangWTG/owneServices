using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.GUI.US
{
	public class TSAKnownColumnManager
	{
		#region Constructor

		public TSAKnownColumnManager(ZGrid grid, string tSAKnownColumnName)
		{
			this.grid = grid;
			this.approvedKnownColumnName = tSAKnownColumnName;
		}

		#endregion

		#region Column Control

		public void SetColumns(WhsRow row)
		{
			bool showApprovedKnown = (row.Warehouse != null) && row.Warehouse.IsApprovedKnown;

			SetColumn(approvedKnownColumnName, showApprovedKnown);
		}

		// TODO: Make private when ICustomLabelProvider implemented
		public void SetColumn(string columnName, bool used)
		{
			grid.SetColumnVisible(used, columnName);
			ZGridColumn column = grid.Columns[columnName];
			if (column != null)
			{
				column.IsUnavailable = !used;
				column.ErrorMessageWhenUnavailable = (used) ? "" : errorMessageWhenUnavailable;
			}
		}

		#endregion

		#region Implementation

		readonly ZGrid grid;
		readonly string approvedKnownColumnName;
		static string errorMessageWhenUnavailable => Res.GetString("7034B50D-67D3-4598-9569-DA9CF7C75B02", "TSA column can be selected only for TSA Known warehouse");

		#endregion
	}
}
