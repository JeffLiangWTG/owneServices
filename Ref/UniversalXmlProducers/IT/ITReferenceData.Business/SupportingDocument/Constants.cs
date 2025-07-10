namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public static class Constants
	{
		public static class RefCusCodeListAttributeNames
		{
			public const string Retroactive = "Retroactive";
			public const string Year = "Year";
			public const string Country = "Country";
			public const string ReferenceNumber = "ReferenceNumber";
			public const string Quantity = "Quantity";
			public const string UnitOfQuantity = "UnitOfQuantity";
			public const string ElectronicFolder = "ElectronicFolder";
			public const string ElectronicFolderNote = "ElectronicFolderNote";
			public const string PaperFolder = "PaperFolder";
			public const string PaperFolderNote = "PaperFolderNote";
		}

		public static class RefCusCodeListAttributeValues
		{
			public const string Yes = "Y";
		}

		public static class RefCusCodeListCodeTypes
		{
			public const string SupportingDocumentImport = "DC44I";
			public const string SupportingDocumentExport = "DC44E";
			public const string SupportingDocumentNcts = "DC44N";
			public const string SupportingDocumentTemporaryStorage = "DC44T";
			public const string SupportingDocumentAdditionalReference = "AR44E";
		}

		public static class Regex
		{
			public const string NationalCertificateLink = @"<a href=\""javascript:linkToPostKeyCert\('CertificatoNazServlet','(?<UC>.*)','(?<SC>.*)','(?<ST>.*)','(?<Label>.*)','(?<Suffix>.*)','(?<ProgressiveNumber>.*)','(?<DescriptionValidityStartDate>.*)'\)\"">";
			public const string EuropeanCertificateType = @"<OPTION.*value=\""(?<Type>.+)\"">";
			public const string EuropeanCertificateLink = @"<a href=\""javascript:linkToPostKey\('DatiGeneraliServlet',(?<UC>.*),(?<SC>.*),'(?<ST>.*)','(?<Label>.*)','(?<DatiGeneraliTipoCertificato>.*)','(?<DatiGeneraliNumeroCertificato>.*)','(?<DatiGeneraliDataIniValDesCertificato>.*)','(?<CodPaeseRegGrp>.*)'\)\"">";
		}

		public static class Categories
		{
			public const string UnitedNationEdifactPrefix = "N";
		}
	}
}
