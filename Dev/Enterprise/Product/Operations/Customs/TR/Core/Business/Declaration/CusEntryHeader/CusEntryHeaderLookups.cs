using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(EU.Business.Declaration.CusEntryHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<TRMessageStatusCodeList>();
	}
}
