namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class NatureAndQtyOfGoods : Forwarding.AWB.Business.NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoods(ExportAWBRateLine rateLine) : base(rateLine)
		{
		}

		public new ExportAWBRateLine ParentRateLine => (ExportAWBRateLine)base.ParentRateLine;

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsValidation(this);
		}
	}
}
