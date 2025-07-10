using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceDataTransferImporter : XmlDataTransferDirector
	{
		public InvoiceDataTransferImporter(IValueObjectDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		#region Import

		protected override XmlDataImporter NewXmlDataImporter()
		{
			return new InvoiceXmlDataImporter((StandAloneInvoiceValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
