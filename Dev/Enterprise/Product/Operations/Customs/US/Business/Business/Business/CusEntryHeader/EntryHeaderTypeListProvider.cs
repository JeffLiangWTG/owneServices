using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class EntryHeaderTypeListProvider : Integration.Customs.IUSCusEntryHeaderTypeListProvider
	{
		public ICodeDescriptionPairList GetEntryTypes()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(CusEntryHeaderMessageTypeList.Codes.EntrySummary, CusEntryHeaderMessageTypeList.Descriptions.EntrySummary);
			result.AddPair(CusEntryHeaderMessageTypeList.Codes.Export, CusEntryHeaderMessageTypeList.Descriptions.Export);

			return result;
		}
	}
}
