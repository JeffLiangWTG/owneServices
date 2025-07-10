namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ConsolNatureAndQtyOfGoodsLithiumBattery : Forwarding.AWB.Business.NatureAndQtyOfGoodsLithiumBattery
	{
		public ConsolNatureAndQtyOfGoodsLithiumBattery(ConsolExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new ConsolNatureAndQtyOfGoodsLithiumBatteryValidation(this);
		}
	}
}
