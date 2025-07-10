
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	partial class SubmissionTypeList : Integration.Customs.US.ISF.IEntryTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsISF10Entry(string entryType)
		{
			return entryType == SubmissionTypeList.Codes.ISF10
				|| entryType == SubmissionTypeList.Codes.ISF5ToISF10
				|| entryType == SubmissionTypeList.Codes.LateISF10;
		}

		public static bool IsISF5Entry(string entryType)
		{
			return entryType == SubmissionTypeList.Codes.ISF5
				|| entryType == SubmissionTypeList.Codes.ISF10ToISF5
				|| entryType == SubmissionTypeList.Codes.LateISF5;
		}

		public static bool IsLateEntry(string entryType)
		{
			return entryType == SubmissionTypeList.Codes.LateISF5
				|| entryType == SubmissionTypeList.Codes.LateISF10;
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => this;
	}
}
