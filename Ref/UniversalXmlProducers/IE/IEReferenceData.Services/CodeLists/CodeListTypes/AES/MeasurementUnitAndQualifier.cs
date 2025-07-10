using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class MeasurementUnitAndQualifier : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.MeasurementUnitAndQualifierType;

		public string NameInFile => "CL349 - CL Measurement unit and qualifier";

		public string CodeFormattingRegularExpression => "([A-Z]{3})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
