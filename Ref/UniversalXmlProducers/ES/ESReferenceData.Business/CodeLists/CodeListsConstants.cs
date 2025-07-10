namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class CodeListsConstants
	{
		public static class ImportExport
		{
			public static class CodeTypes
			{
				public const string IE_C44_ADDINFO = "ADDIB";
			}
		}
		public static class Import
		{
			public static class CodeTypes
			{
				public const string IMPORT_C44_DOCUMENTS = "DC44I";
				public const string IMPORT_C44_ADDINFO = "ADDII";
			}
		}

		public static class Export
		{
			public static class CodeTypes
			{
				public const string EXPORT_C44_DOCUMENTS = "DC44E";
				public const string EXPORT_C44_ADDINFO = "ADDIE";
			}
		}

		public static class Ncts
		{
			public static class CodeTypes
			{
				public const string NCTS_C44_DOCUMENTS = "DC44N";
			}
		}

		public static class RefCusCodeListAttributeNames
		{
			public const string LEVEL = "Level";
			public const string COMPLEMENT = "Complement";
			public const string ITEM_NUMBER = "ItemNumber";
			public const string REFERENCE = "Reference";
			public const string IS_NATIONAL = "IsNational";
		}

		public static class RefCusCodeListAttributeValues
		{
			public const string LEVEL_HEADER = "Header";
			public const string LEVEL_HOUSE = "House";
			public const string LEVEL_ITEM = "Item";
			public const string Yes = "Y";
			public const string No = "N";
		}
	}
}
