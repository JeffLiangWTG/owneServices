using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class TD44ICodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEImportTransportDocumentTypeCodesFileName;
			}
		}
		protected override string ValidityTag => "CHAMP1";

		protected override string StartDateTag => "CHAMP2";

		protected override string DescriptionTag => "CHAMP3";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeTag => "CHAMP5";

		protected override string EndDateTag => string.Empty;

		protected override string CodeType => "TD44I";

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;

		protected override RefCusCodeTypeGenerator CodeTypeGenerator => new TD44ICodeTypeGenerator();
	}
}
