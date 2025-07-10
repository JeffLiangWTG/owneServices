using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBNatureAndQtyOfGoodsOrigin : FWBNatureAndQtyOfGoods
	{
		public override ZString Type
		{
			get { return Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin; }
		}

		public override ZString Text
		{
			get { return string.Format(ZArchitecture.Core.Culture.Invariant, (NoResString)"Goods Origin: {0}", Country); } // Developer Constant
		}

		public ZString Country { get; set; }
	}
}
