using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class AR44ICodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportAdditionalReferenceCodesFileName;
				yield return ApplicationConfig.Instance.DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName;
			}
		}

		protected override string ValidityTag => "CHAMP1";

		protected override string CodeTag => "CHAMP5";

		protected override string StartDateTag => "CHAMP2";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "AR44I";

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;
	}
}
