using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business
{
	class EntryMessageTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string ECIWriteOff = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOff;
			public const string ECIWriteOffManifest = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			public const string FormalEntry = CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry;
			public const string Completion = CusEntryHeader.EntryHeaderTypes.NZ.Completion;
			public const string Original = CusEntryHeader.EntryHeaderTypes.NZ.Original;
			public const string PrimaryIndustries = CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries;
		}

		public static class Descriptions
		{
			public const string ECIWriteOff = "ECI Write-Off";
			public const string ECIWriteOffManifest = "ECI Manifest";
			public const string FormalEntry = "Formal Entry";
			public const string Completion = "Completion Entry";
			public const string Original = "Original Entry";
			public const string PrimaryIndustries = "IPI Entry";
		}

		public EntryMessageTypeList()
		{
			AddPair(Codes.ECIWriteOff, Descriptions.ECIWriteOff);
			AddPair(Codes.ECIWriteOffManifest, Descriptions.ECIWriteOffManifest);
			AddPair(Codes.FormalEntry, Descriptions.FormalEntry);
			AddPair(Codes.Completion, Descriptions.Completion);
			AddPair(Codes.Original, Descriptions.Original);
			AddPair(Codes.PrimaryIndustries, Descriptions.PrimaryIndustries);
		}
	}
}


