using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class CL214IMCodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportPreviousDocumentTypeCodesFileName;
			}
		}

		protected override string ValidityTag => "CHAMP1";

		protected override string CodeTag => "CHAMP5";

		protected override string StartDateTag => "CHAMP2";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "214IM";

		protected override bool GetStartDateFromInputFile => true;

		protected override string DataGrouping => UniversalDataHelper.Constants.France;

		protected override RefCusCodeTypeGenerator CodeTypeGenerator => new CL214IMCodeTypeGenerator();
	}
}
