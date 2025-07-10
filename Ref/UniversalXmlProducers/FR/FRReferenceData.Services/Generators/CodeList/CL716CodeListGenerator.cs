using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class CL716CodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportControlResultsTypeCodesFileName;
			}
		}

		protected override string ValidityTag => "CHAMP1";

		protected override string CodeTag => "CHAMP4";

		protected override string StartDateTag => "CHAMP2";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "CL716";

		protected override bool GetStartDateFromInputFile => true;
	}
}
