using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GUI
{
	public class CustomsInvoiceLinesGridColorSchemaManager : GridColourSchemeManager
	{
		public CustomsInvoiceLinesGridColorSchemaManager(ZGrid grid) : base(grid)
		{
			this.grid = grid;
		}

		readonly ZGrid grid;

		protected override FilterStripBusinessObject GetFilterBusinessObjectForGrid()
		{
			return new InvoiceLinesGridFilterStripBusinessObject(grid);
		}
	}
}
