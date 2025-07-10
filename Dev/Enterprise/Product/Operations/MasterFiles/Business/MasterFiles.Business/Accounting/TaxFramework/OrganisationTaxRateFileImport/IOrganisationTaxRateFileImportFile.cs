using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	public interface IOrganisationTaxRateFileImportFileDataImporter
	{
		void ImportData(string filename, INotifications notifications, OrganisationTaxRateFileImport bizo);
	}
}
