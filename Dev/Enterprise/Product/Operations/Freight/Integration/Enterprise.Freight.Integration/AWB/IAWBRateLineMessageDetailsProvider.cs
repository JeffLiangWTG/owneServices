using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBRateLineMessageDetailsProvider
	{
		ZString WeightInLBsOrKGs { get; }
		ZString NatureAndQtyOfGoodsDescription { get; }
	}
}
