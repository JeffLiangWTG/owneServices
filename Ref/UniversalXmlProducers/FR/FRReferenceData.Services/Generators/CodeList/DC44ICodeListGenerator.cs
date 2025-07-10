using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class DC44ICodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFileName;
				yield return ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName;
			}
		}

		protected override string ValidityTag => CurrentProcessedFile == ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName ? "CHAMP4" : "CHAMP1";

		protected override string CodeTag => CurrentProcessedFile == ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName ? "CHAMP1" : "CHAMP5";

		protected override string StartDateTag => CurrentProcessedFile == ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName ? "CHAMP3" : "CHAMP2";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => CurrentProcessedFile == ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName ? "CHAMP2" : "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "DC44I";

		protected override bool GetStartDateFromInputFile => true;

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;
	}
}
