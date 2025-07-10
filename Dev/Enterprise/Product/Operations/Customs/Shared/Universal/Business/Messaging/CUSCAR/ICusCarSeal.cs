using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarSeal
	{
		ZString SealNumber { get; }
		ZString SealingParty { get; }
		ZString SealType { get; }
	}
}
