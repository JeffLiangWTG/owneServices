//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDrawbackOtherFeeAddInfoValidation
//
//    This class should be used for overriding validation in AutoDrawbackOtherFeeAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.Types;

	public class DrawbackOtherFeeAddInfoValidation : AutoDrawbackOtherFeeAddInfoValidation
	{
		public DrawbackOtherFeeAddInfoValidation(AutoDrawbackOtherFeeAddInfo parent) : base(parent)
		{
		}

		DrawbackOtherFee DrawbackOtherFee
		{
			get { return ((DrawbackOtherFeeAddInfo)Parent).Parent as DrawbackOtherFee; }
		}

		protected override void CheckUS_99ClaimAmount()
		{
			base.CheckUS_99ClaimAmount();
			var drawbackOtherFee = DrawbackOtherFee;
			var drw99ClaimAmount = drawbackOtherFee._99ClaimedAmount;
			if (drw99ClaimAmount > ZDecimal.Zero & drw99ClaimAmount > drawbackOtherFee.CalculatedAmount)
			{
				drawbackOtherFee.US_99ClaimAmountInfo.AddMessageError(DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			}
		}

		protected override void CheckUS_CalculatedAmount()
		{
			base.CheckUS_CalculatedAmount();
			ValidateUS_99ClaimAmount();
		}
	}
}
