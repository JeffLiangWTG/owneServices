using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public class EuropeanRawSupportingDocument : RawSupportingDocument
	{
		public EuropeanRawSupportingDocument(string rawHtml) : base(rawHtml)
		{
		}

		protected override string GetCodeCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[1]//td[2]")?.InnerText?.Replace("Codice:", "")?.Trim() ?? string.Empty;

		protected override string GetDescriptionCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[6]//td[2]//textarea[1]")?.InnerText ?? string.Empty;

		protected override string GetElectronicFolderNoteCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[13]//td[3]//textarea[1]")?.InnerText ?? string.Empty;

		protected override DateTime? GetEndDateCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[4]//td[2]")?.InnerText?.Replace("Fine validità:", "")?.Trim()?.ToDateTime();

		protected override bool GetIsCertificateIdRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[5]");

		protected override bool GetIsCountryRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[4]");

		protected override bool GetIsElectronicFolderRequiredCore() => HasYesValueAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[12]//td[2]//input[1]");

		protected override bool GetIsPaperFolderRequiredCore() => HasYesValueAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[14]//td[2]//input[1]");

		protected override bool GetIsQuantityRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[6]");

		protected override bool GetIsRetroActiveRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[1]");

		protected override bool GetIsUnitOfQuantityRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[7]");

		protected override bool GetIsYearRequiredCore() => HasCheckedAttribute("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[3]");

		protected override string GetPaperFolderNoteCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[15]//td[3]//textarea[1]")?.InnerText ?? string.Empty;

		protected override DateTime? GetStartDateCore() => DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[3]//td[2]")?.InnerText?.Replace("Inizio validità:", "")?.Trim()?.ToDateTime();

		protected override SupportingDocumentType GetSupportingDocumentType() => SupportingDocumentType.European;
	}
}
