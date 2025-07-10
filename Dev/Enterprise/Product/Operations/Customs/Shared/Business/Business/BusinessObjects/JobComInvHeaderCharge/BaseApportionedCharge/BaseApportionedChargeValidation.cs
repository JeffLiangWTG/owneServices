
namespace Enterprise.Customs.Business
{
	public class BaseApportionedChargeValidation : JobComInvHeaderChargeValidation
	{
		public BaseApportionedChargeValidation(BaseApportionedCharge apportionedCharge) : base(apportionedCharge)
		{
			this.apportionedCharge = apportionedCharge;
		}

		protected BaseApportionedCharge apportionedCharge;

		#region Overrides

		protected override void CheckJ7_IsIncludedInITOT()
		{
			base.CheckJ7_IsIncludedInITOT();
			ValidateExcludedCharges();
		}

		#endregion

		#region Helping Methods

		protected virtual void ValidateExcludedCharges()
		{
			var invoice = apportionedCharge?.Invoice;
			var incoTermAndChargeFactory = invoice?.IncoTermAndChargeFactory;
			var chargeCode = apportionedCharge?.ChargeCode;
			if (incoTermAndChargeFactory != null
				&& !invoice.IncoTerm.IsEmpty
				&& chargeCode != null
				&& !incoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, chargeCode)
				&& apportionedCharge.J7_IsIncludedInITOT)
			{
				apportionedCharge.J7_IsIncludedInITOTInfo.AddMessageError(Res.GetString("93874722-01b6-4b1e-a440-759ddb3bb401", "{0} cannot include this charge in lines.", apportionedCharge.Invoice.JZ_IncoTerm));
			}
		}
		#endregion
	}
}
