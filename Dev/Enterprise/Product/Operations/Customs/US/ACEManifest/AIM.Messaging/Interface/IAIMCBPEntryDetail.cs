using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMCBPEntryDetail
	{
		ZString EntryType { get; }
		ZString EntryNumber { get; }
	}
}
