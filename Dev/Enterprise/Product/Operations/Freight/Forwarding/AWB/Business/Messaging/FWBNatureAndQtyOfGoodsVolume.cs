using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBNatureAndQtyOfGoodsVolume : FWBNatureAndQtyOfGoods
	{
		public override ZString Type
		{
			get { return Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume; }
		}

		public override ZString Text
		{
			get { return string.Format(ZArchitecture.Core.Culture.Invariant, (NoResString)"VOL {0} {1}", Value, Unit); } // Developer Constant
		}

		public ZDecimal Value { get; set; }

		public ZString Unit { get; set; }
	}
}
