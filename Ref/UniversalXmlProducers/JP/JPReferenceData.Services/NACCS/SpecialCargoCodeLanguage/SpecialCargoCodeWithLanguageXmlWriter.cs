using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class SpecialCargoCodeWithLanguageXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Special Cargo Code Language";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_SpecialCargoCodeEngDesc";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.SpecialCargoCodeCsvFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new SpecialCargoCodeWithLanguageParser();

		protected override DateTime UpdatePublicationDateIfNeeded(DateTime publicationDate)
		{
			return DateTime.Parse(AppConfig.NACCS.CodeLists.SpecialCargoCodeEngDescPublicationTime, CultureInfo.InvariantCulture);
		}
	}
}
