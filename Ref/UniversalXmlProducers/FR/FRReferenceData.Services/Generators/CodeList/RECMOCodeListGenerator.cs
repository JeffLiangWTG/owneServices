using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services;

public class RECMOCodeListGenerator : XmlDrivenCodeListDataFileGenerator
{
	protected override IEnumerable<string> InputFileNames
	{
		get
		{
			yield return ApplicationConfig.Instance.DeltaIEMotivationForRectificationRequestCodesFileName;
		}
	}
	protected override string ValidityTag => string.Empty;

	protected override string CodeTag => "CHAMP1";

	protected override string StartDateTag => "date_table";

	protected override string EndDateTag => string.Empty;

	protected override string DescriptionTag => "CHAMP2";

	protected override string AdditionalDescriptionTag => "CHAMP3";

	protected override string CodeType => "RECMO";

	protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;

	protected override RefCusCodeTypeGenerator CodeTypeGenerator => new RECMOCodeTypeGenerator();
}
