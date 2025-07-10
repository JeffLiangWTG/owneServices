using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARPaymentCycleValidation : OrgARTermsCycleValidation
	{
		public OrgARPaymentCycleValidation(OrgARPaymentCycle parent)
			: base(parent)
		{
		}

		protected override void CheckP5_ToDay()
		{
			if (!OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(Parent.P5_ToDay))
			{
				Parent.P5_ToDayInfo.AddError(Res.GetString("80CB5296-1DC8-40CC-9D9B-54BD119D40B4", "The value must be 1 or greater."));
			}
			if (Parent.ARTerms != null)
			{
				if (Parent.ARTerms.ARPaymentCycles.Find((paymentCycle) => paymentCycle.P5_ToDay == Parent.P5_ToDay && paymentCycle.PK != Parent.PK).Any())
				{
					Parent.P5_ToDayInfo.AddError(Res.GetString("FE602F75-2821-41A4-B8AD-C4FFC8E3F3EE", "Payment cycle with the same 'Cycle #' already exists."));
				}
			}
		}

		protected override void CheckP5_PaymentDay()
		{
			base.CheckP5_PaymentDay();

			if (Parent.ARTerms != null)
			{
				if (Parent.ARTerms.ARPaymentCycles.Find((paymentCycle) => paymentCycle.P5_PaymentDay == Parent.P5_PaymentDay && paymentCycle.PK != Parent.PK).Any())
				{
					Parent.P5_PaymentDayInfo.AddError(Res.GetString("139294A0-C1A8-43AE-8167-31742BF4461A", "Payment cycle with the same 'Payment Day' value already exists."));
				}
			}
		}
	}
}
