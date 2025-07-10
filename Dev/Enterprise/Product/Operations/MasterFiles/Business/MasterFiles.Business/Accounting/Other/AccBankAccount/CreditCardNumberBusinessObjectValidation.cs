

namespace Enterprise.MasterFiles.Business
{
	public class CreditCardNumberBusinessObjectValidation : AutoCreditCardNumberBusinessObjectValidation
	{
		public CreditCardNumberBusinessObjectValidation(AutoCreditCardNumberBusinessObject parent)
			: base(parent) { }

		#region Implementation

		public new CreditCardNumberBusinessObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CreditCardNumberBusinessObject)base.Parent; }
		}

		#endregion

		protected override void CheckUnencryptedCreditCardNumber()
		{
			base.CheckUnencryptedCreditCardNumber();
			//ensure it is at least 13 digits long
			//TODO: Should we allow only 13 digits? What does ComPay expect?
			if (Parent.UnencryptedCreditCardNumber.Length < 13)
			{
				Parent.UnencryptedCreditCardNumberInfo.AddError(Res.GetString("ddf59c0b-5498-484c-ac01-928dc656e161", "The credit card number should be at least 13 digits in length."));
			}
			if (!Parent.UnencryptedCreditCardNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.UnencryptedCreditCardNumberInfo.AddError(Res.GetString("11933fb6-939e-445f-83a1-08e0e02985e7", "The credit card number should only consist of numbers.  Do not include hyphens or spaces."));
			}
		}
	}
}
