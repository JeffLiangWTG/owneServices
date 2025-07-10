using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public interface IWriteOffStatus : ITSWStatus
	{
		CusEntryHeader EntryHeader { get; }
		ZString CustomsStatus { get; }
		ZString GoodsClearanceStatus { get; }
		ZString CombinedStatus { get; }
	}
}
