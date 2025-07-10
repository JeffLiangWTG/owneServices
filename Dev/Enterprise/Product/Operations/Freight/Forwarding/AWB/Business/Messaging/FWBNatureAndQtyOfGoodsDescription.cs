using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBNatureAndQtyOfGoodsDescription : FWBNatureAndQtyOfGoods
	{
		public FWBNatureAndQtyOfGoodsDescription(ZString type, ZString description)
		{
			this.type = type;
			this.description = description;
		}

		readonly ZString type;
		readonly ZString description;

		public override ZString Type
		{
			get { return type; }
		}

		public override ZString Text
		{
			get { return description; }
		}
	}
}
