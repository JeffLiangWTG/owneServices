using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader entry)
			: base(entry)
		{
		}

		public CodeDescriptionPairList TSWMessageStatusList => Factory.GetCachedValue<StatusList>();

		public CodeDescriptionPairList MessageModeList => Factory.GetCachedValue<JobApplicationCodeList>();

		public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<EntryMessageTypeList>();

		public CodeDescriptionPairList TSWMovementStatusList => Factory.GetCachedValue<MovementStatus>();
	}
}
