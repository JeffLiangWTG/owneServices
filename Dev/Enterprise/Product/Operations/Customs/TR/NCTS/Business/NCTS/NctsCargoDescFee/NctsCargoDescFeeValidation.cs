namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsCargoDescFeeValidation : EU.NCTS.Business.NctsCargoDescFeeValidation
	{
		public NctsCargoDescFeeValidation(NctsCargoDescFee parent) : base(parent)
		{
		}

		protected new NctsCargoDescFee Parent => (NctsCargoDescFee)base.Parent;
	}
}
