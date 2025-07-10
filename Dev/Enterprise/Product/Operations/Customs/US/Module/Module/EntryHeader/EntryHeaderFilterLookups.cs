using Enterprise.Customs.Common.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class EntryHeaderFilterLookups : Customs.Module.EntryHeaderFilterLookups
	{
		public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList MessageStatusListForENS
		{
			get
			{
				return Factory.GetCachedValue("EntrySummaryStatusListForFilter", delegate
				{
					var result = new MessageStatusListENS();

					result.RemoveCode(MessageStatusListENS.Codes.NotSent);
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}
	}
}
