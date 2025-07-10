using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportDEAUserControl : ZUserControl
	{
		public ExportDEAUserControl()
		{
			InitializeComponent();
		}

		internal void RemoveIrrevelantColumnsForProduct()
		{
			var columnsToBeRemoved = new[]
			{
				DEAHeader.Schema.US_Weight,
				DEAHeader.Schema.US_UnitOfMeasure
			};

			DEAGrid.RemoveFromAvailableColumns(columnsToBeRemoved);
		}
	}
}
