using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList
{
	public class ENSUBCodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportEntrySubStyleCodesFileName;
			}
		}
		protected override string ValidityTag => "CHAMP1";

		protected override string CodeTag => "CHAMP4";

		protected override string StartDateTag => "CHAMP2";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "ENSUB";
	}
}
