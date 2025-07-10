using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBNatureAndQtyOfGoodsSLAC : FWBNatureAndQtyOfGoods
	{
		public override ZString Type
		{
			get { return Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount; }
		}

		public override ZString Text
		{
			get { return string.Format(ZArchitecture.Core.Culture.Invariant, (NoResString)"{0} SLAC", Count); } // Developer Constant
		}

		public ZInt Count { get; set; }
	}
}
