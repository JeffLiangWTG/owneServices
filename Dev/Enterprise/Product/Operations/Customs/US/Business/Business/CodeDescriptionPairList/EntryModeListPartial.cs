using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class EntryModeList : CodeDescriptionPairList,
		Integration.Customs.US.IEntryModeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static EntryModeList GetRelevantListFor(ZDateTime dateOfArrival)
		{
			var result = new EntryModeList();

			if (dateOfArrival.Date >= USConstants.PairedPortProgramEndDate)
			{
				result.RemoveCode(EntryModeList.Codes.Paired);
			}

			return result;
		}

		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
