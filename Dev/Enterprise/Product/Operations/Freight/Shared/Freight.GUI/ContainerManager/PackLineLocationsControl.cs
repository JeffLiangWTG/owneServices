using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class PackLineLocationsControl : ZUserControl
	{
		public PackLineLocationsControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ProcessLocationGridColumns();
			}
		}

		public ZGrid Grid
		{
			get { return locationsGrid; }
		}

		void ProcessLocationGridColumns()
		{
			string[] columnNamesToRemove = WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value ?
				new[] { PackLocation.Schema.JQ_WarehouseLocation } :
				new[] { PackLocation.Schema.LocationString, PackLocation.Schema.LocationWhsGuid };

			ZGridColumnInfo[] columnsToRemove = locationsGrid.ColumnStyles
				.Cast<ZGridColumnInfo>()
				.Where(columnInfo => columnNamesToRemove.Contains(columnInfo.ColumnName))
				.ToArray();

			foreach (ZGridColumnInfo columnToRemove in columnsToRemove)
			{
				locationsGrid.ColumnStyles.Remove(columnToRemove);
			}
		}
	}
}
