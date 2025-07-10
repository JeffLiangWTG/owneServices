namespace Enterprise.Customs.US.Business
{
	partial class DEAFormTypeList
	{
		public static string GetDocumentIdentifierFromFormType(string formTypeId)
		{
			switch (formTypeId)
			{
				case DEAFormTypeList.Codes.DEA35:
					return DocumentIdentifierList.Codes.ImportLicense;
				case DEAFormTypeList.Codes.DEA236:
					return DocumentIdentifierList.Codes.DEA236;
				case DEAFormTypeList.Codes.DEA486:
					return DocumentIdentifierList.Codes.DEA486;
				case DEAFormTypeList.Codes.DEA486A:
					return DocumentIdentifierList.Codes.DEA486A;
				default:
					return string.Empty;
			}
		}

		public static string GetFormTypeFromDocumentIdentifier(string documentIdentifier)
		{
			switch (documentIdentifier)
			{
				case DocumentIdentifierList.Codes.ImportLicense:
					return DEAFormTypeList.Codes.DEA35;
				case DocumentIdentifierList.Codes.DEA236:
					return DEAFormTypeList.Codes.DEA236;
				case DocumentIdentifierList.Codes.DEA486:
					return DEAFormTypeList.Codes.DEA486;
				case DocumentIdentifierList.Codes.DEA486A:
					return DEAFormTypeList.Codes.DEA486A;
				default:
					return string.Empty;
			}
		}
	}
}
