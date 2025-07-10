using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public interface IMovementStatus
	{
		TranshipmentRequest UnderbondMovement { get; }
		ZString CustomsStatus { get; }
		ZString GoodsClearanceStatus { get; }
		ZString CombinedStatus { get; }
		ZString Agency { get; }
	}
}
