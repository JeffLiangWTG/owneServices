using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class InvalidationMotivationCodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEInvalidationMotivationReferenceCodesFileName;
			}
		}

		protected override string ValidityTag => string.Empty;

		protected override string CodeTag => "CHAMP1";

		protected override string StartDateTag => "CHAMP5";

		protected override string EndDateTag => "CHAMP6";

		protected override string DescriptionTag => "CHAMP2";

		protected override string AdditionalDescriptionTag => "CHAMP3";
	

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;
		protected override string CodeType => "INVMO";
		protected override bool GetStartDateFromInputFile => true;
		protected override bool GetEndDateFromInputFile => true;
	}
}
