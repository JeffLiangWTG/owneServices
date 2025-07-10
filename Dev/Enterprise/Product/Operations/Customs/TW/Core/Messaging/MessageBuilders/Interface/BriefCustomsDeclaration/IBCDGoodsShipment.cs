using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IBCDGoodsShipment : IGoodsShipment
	{
		ZInt SequenceNumeric { get; }

		ZDecimal TotalGrossMassMeasure { get; }

		new IBCDConsignment Consignment { get; }
	}
}
