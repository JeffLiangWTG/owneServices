using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeList
{
	public class AHIPCCodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName;
			}
		}
		protected override string ValidityTag => string.Empty;

		protected override string CodeTag => "CHAMP1";

		protected override string StartDateTag => "CHAMP3";

		protected override string EndDateTag => string.Empty;

		protected override string DescriptionTag => "CHAMP2";

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override string CodeType => "AHIPC";

		protected override bool GetStartDateFromInputFile => true;

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;

		protected override RefCusCodeTypeGenerator CodeTypeGenerator => new AHIPCCodeTypeGenerator();

		protected override Dependency[] GetDependencies(DateTime publicationTime)
		{
			return new[]
			{
				new Dependency($"{CodeTypeGenerator.DataGrouping} {CodeTypeGenerator.Description} Code Type", publicationTime, DependencyType.Required)
			};
		}
	}
}
