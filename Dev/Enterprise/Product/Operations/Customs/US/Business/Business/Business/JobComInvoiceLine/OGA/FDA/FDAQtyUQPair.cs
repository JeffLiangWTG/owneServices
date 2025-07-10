using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public struct FDAQtyUQPair
	{
		public FDAQtyUQPair(ZDecimal qty, ZString uQ)
		{
			this.Qty = qty;
			this.UQ = uQ;
		}

		public readonly ZDecimal Qty;
		public readonly ZString UQ;
	}
}
