using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Customs.DataTransfer
{
	class CustomsOrganizationXmlDataTransferExporter : OrganisationXmlDataTransferExporter
	{
		public CustomsOrganizationXmlDataTransferExporter(StandardManualAndBatchImportOrganisationValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		public CustomsOrganizationXmlDataTransferExporter()
			: base(new StandardManualAndBatchImportOrganisationValueObjectDataAdapter())
		{
		}
	}
}
