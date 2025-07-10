using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.Data.FlatFileImporter
{
	public class DataTransferImplementation : DataTransfer.DataTransferImpl
	{
		public DataTransferImplementation()
			: base()
		{
		}

		protected override DataTransfer.FlatFileInvoiceDataImporter GetFlatFileImporter(string fileName, Customs.Business.BaseJobDeclaration declaration)
		{
			return new FlatFileInvoiceDataImporter(fileName, (JobDeclaration)declaration);
		}
	}
}
