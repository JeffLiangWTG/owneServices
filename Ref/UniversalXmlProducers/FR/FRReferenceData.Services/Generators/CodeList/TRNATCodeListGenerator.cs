
using System.Collections.Generic;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class TRNATCodeListGenerator : XmlDrivenCodeListDataFileGenerator
	{
		protected override IEnumerable<string> InputFileNames
		{
			get
			{
				yield return ApplicationConfig.Instance.DeltaIETransactionNatureCodesFileName;
			}
		}

		protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;

		protected override string CodeType => "TRNAT";

		protected override string CodeTag => "CHAMP1";

		protected override string DescriptionTag => "CHAMP2";

		protected override string StartDateTag => "CHAMP3";

		protected override bool GetStartDateFromInputFile => true;

		protected override string EndDateTag => "CHAMP4";

		protected override bool GetEndDateFromInputFile => true;

		protected override string ValidityTag => string.Empty;

		protected override string AdditionalDescriptionTag => string.Empty;

		protected override bool HasAttributes => true;

		protected override RefCusCodeListAttribute[] GetCusCodeListAttributesFromNode(XmlNode node) => [UniversalDataHelper.CreateRefCusCodeListAttribute("IsImport", UniversalDataHelper.Constants.Yes)];
	}
}
