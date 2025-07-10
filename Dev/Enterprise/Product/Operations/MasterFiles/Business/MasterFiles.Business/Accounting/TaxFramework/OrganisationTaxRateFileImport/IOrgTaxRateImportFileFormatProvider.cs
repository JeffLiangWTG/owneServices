using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IOrgTaxRateImportFileFormatProvider
	{
		IOrgTaxRateImportFileFormat GetFileFormat(ZString taxAuthorityCode);
	}

	public interface IOrgTaxRateImportFileFormat
	{
		short StartDate { get; }
		short EndDate { get; }
		short RegistrationCode { get; }
		short PerceptionRate { get; }
		char Delimiter { get; }
		short NumberOfColumnsInFormat { get; }
	}
}
