using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBNatureAndQtyOfGoodsDimensions : FWBNatureAndQtyOfGoods
	{
		public override ZString Type
		{
			get { return Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions; }
		}

		public override ZString Text
		{
			get { return string.Format(ZArchitecture.Core.Culture.Invariant, (NoResString)"DIMS {0}x{1}x{2} {3} x {4}", Length, Width, Height, Unit, Count); } // Developer Constant
		}

		public ZInt Length { get; set; }

		public ZInt Width { get; set; }

		public ZInt Height { get; set; }

		public ZInt Count { get; set; }

		public ZString Unit { get; set; }
	}
}
