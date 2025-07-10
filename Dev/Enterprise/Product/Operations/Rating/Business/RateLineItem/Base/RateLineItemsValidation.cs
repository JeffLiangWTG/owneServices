using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateLineItemsValidation : AutoRateLineItemsValidation
	{
		public RateLineItemsValidation(AutoRateLineItems parent) : base(parent)
		{
		}

		RateLineItem LineItem => (RateLineItem)Parent;

		Calculator GetCalculator(ZPropertyInfo info) =>
			info.HasChangesOrIsNew() || !IsValidatingAll
				? LineItem.Parent?.Calculator // This forces to initialize the Calculator if hasn't initialized yet
				: LineItem.Calculator(); // This returns the Calculator if it has been initialized

		#region TM_Type

		protected override void CheckTM_Type()
		{
			base.CheckTM_Type();
			GetCalculator(LineItem.TM_TypeInfo)?.ValidateTM_Type(LineItem);
		}

		#endregion

		#region TM_Value

		protected override void CheckTM_Value()
		{
			base.CheckTM_Value();
			GetCalculator(LineItem.TM_ValueInfo)?.ValidateTM_Value(LineItem);
		}

		#endregion

		#region TM_AgentDeclaredRate

		protected override void CheckTM_AgentDeclaredRate()
		{
			base.CheckTM_AgentDeclaredRate();
			GetCalculator(LineItem.TM_AgentDeclaredRateInfo)?.ValidateTM_AgentDeclaredRate(LineItem);
		}

		#endregion

		#region TM_Text

		protected override void CheckTM_Text()
		{
			base.CheckTM_Text();
			GetCalculator(LineItem.TM_TextInfo)?.ValidateTM_Text(LineItem);
		}

		#endregion

		#region TM_Break

		protected override void CheckTM_Break()
		{
			base.CheckTM_Break();
			GetCalculator(LineItem.TM_BreakInfo)?.ValidateTM_Break(LineItem);
		}

		#endregion

		#region TM_BreakWeightVolume

		protected override void CheckTM_BreakWeightVolume()
		{
			base.CheckTM_BreakWeightVolume();
			GetCalculator(LineItem.TM_BreakWeightVolumeInfo)?.ValidateTM_BreakWeightVolume(LineItem);
		}

		#endregion

		#region TM_BreakMinimum

		protected override void CheckTM_BreakMinimum()
		{
			base.CheckTM_BreakMinimum();
			GetCalculator(LineItem.TM_BreakMinimumInfo)?.ValidateTM_BreakMinimum(LineItem);
		}

		#endregion

		#region TM_AC

		protected override void CheckTM_AC()
		{
			base.CheckTM_AC();
			GetCalculator(LineItem.TM_ACInfo)?.ValidateTM_AC(LineItem);
		}

		#endregion

		#region TM_FlatAmount

		protected override void CheckTM_FlatAmount()
		{
			base.CheckTM_FlatAmount();
			GetCalculator(LineItem.TM_FlatAmountInfo)?.ValidateTM_FlatAmount(LineItem);
		}

		#endregion

		public sealed override void ValidateAll()
		{
			if (!Parent.IsDeleted)
			{
				try
				{
					IsValidatingAll = true;
					base.ValidateAll();
				}
				finally
				{
					IsValidatingAll = false;
				}
			}
		}

		public bool IsValidatingAll { get; private set; }
	}
}

