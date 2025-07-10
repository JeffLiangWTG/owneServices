using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class MessageChooserValidation : ASYCUDA.Business.MessageChooserValidation
	{
		public MessageChooserValidation(MessageChooser parent)
			: base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateCycleDate();
			ValidateCycleNumber();
		}
		#region CycleDate
		public void ValidateCycleDate()
		{
			ValidateCalculatedProperty(Parent.CycleDateInfo);
		}

		protected virtual void CheckCycleDate()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.CycleDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.CycleDateInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CycleDateInfo);
		}
		#endregion
		#region CycleNumber
		public void ValidateCycleNumber()
		{
			ValidateCalculatedProperty(Parent.CycleNumberInfo);
		}
		protected virtual void CheckCycleNumber()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CycleNumberInfo);
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CycleNumberInfo);
		}
		#endregion

		protected new MessageChooser Parent => (MessageChooser)base.Parent;
	}
}
