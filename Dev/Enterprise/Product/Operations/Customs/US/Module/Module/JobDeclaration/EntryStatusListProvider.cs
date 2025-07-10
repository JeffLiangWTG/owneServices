using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override CodeDescriptionPairList EntryStatus_Import_List => new ImportEntryStatusList();

		protected override CodeDescriptionPairList EntryStatus_Export_List => new AESDirectCustomsEntryStatus();

		protected override CodeDescriptionPairList EntryStatus_ImportExport_List
		{
			get
			{
				var result = new CodeDescriptionPairList(EntryStatus_Export_List);
				result.AddRange(EntryStatus_Import_List);
				return result;
			}
		}
	}
}
