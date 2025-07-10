using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportNMFSUserControl : ZUserControl
	{
		public ExportNMFSUserControl()
		{
			InitializeComponent();
		}

		internal void RemoveIrrevelantColumnsForProduct()
		{
			var columnsToBeRemoved = new[]
			{
				NMFSLine.Schema.US_Quantity,
				NMFSLine.Schema.US_UnitOfMeasure,
				NMFSLine.Schema.US_DISDocumentID
			};
			NMFSHeaderGrid.RemoveFromAvailableColumns(columnsToBeRemoved);
		}
	}
}
