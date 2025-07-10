namespace Enterprise.Customs.TR.Business
{
	#region SuppressResourceStringsCheckRegion

	public static class TRMessageConstants
	{
		public const string MessageServiceTaskCategory = "TRC";

		public const string TRRecipient = "Customs";

		public const string TRRecipientTest = "CustomsTest";

		public const string ExportUnion = "Union";

		public const string ExportUnionTest = "UnionTest";

		public const string ExportUnionFtpFileNameKey = "custom.TR.EUT.FileName";

		public const string ExportUnionFtpFileNameExtension = ".txt";

		public const string MessageHeaderActionNodeValue = "Send";

		public const string Test = "Test";

		public const string Live = "Live";

		public const string CountryMapType = "CNTRY";

		public const string ConsolidatedManifest = "GRUPAJ";

		public const string LanguageCode = "TR";

		public const string IncorrectSupportingCodeReceivedByCustoms0103 = "0103";

		public static class GrupajCompanyVatTypes
		{
			public const string Diger = "DIGER";
			public const string VergiNo = "VERGINO";
		}

		public static class Xml
		{
			public const string SOAPNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

			public const string XMLSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

			public const string CustomsNamespace = "http://www.gumruk.gov.tr/";

			public const string TempuriNamespace = "http://tempuri.org/";

			public const string DiffGramNamespace = "urn:schemas-microsoft-com:xml-diffgram-v1";

			public const string CustomsBizTalkNamespace = "http://Gumruk.BizTalk.Integration";

			public const string BizTalk2003AnyNamespace = "http://schemas.microsoft.com/BizTalk/2003/Any";

			public const string SOAPEnvelopeElementName = "Envelope";

			public const string SOAPRootElementName = "Root";

			public const string DiffGramElementName = "diffgram";

			public const string ResultElementName = "Sonuc";

			public const string CustomsResponseElementName = "GidenXML";

			public const string CustomsRequestElementName = "GelenXML";

			public const string SummaryDeclarationResponseElementName = "OzetBeyanResponse";

			public const string InspectionClerkQueryResponse = "ETGBMuayeneMemuruSorgulaResponse";
		}

		public const string RegisteredOrInProcessWithThisReferenceMessage = "Bu referans ile tescil alınmış yada işlemdedir.";

		public const string ReferencePrefix = "ULU-";

		public const string Comma = ",";

		public const string Dot = ".";

		public const string FirmID = "ULUKOM";

		public const int TransactionPollingDelay = 15;

		public const int DT1PollingDelay = 2;
	}

	#endregion
}
