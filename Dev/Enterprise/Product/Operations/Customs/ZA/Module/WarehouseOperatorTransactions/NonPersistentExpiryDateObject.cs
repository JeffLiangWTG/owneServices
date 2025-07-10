namespace Enterprise.Customs.ZA.Module
{
	public class NonPersistentExpiryDateObject : AutoNonPersistentExpiryDateObject
	{
	}

	public class NonPersistentExpiryDateObjectValidation : AutoNonPersistentExpiryDateObjectValidation
	{
		public NonPersistentExpiryDateObjectValidation(AutoNonPersistentExpiryDateObject parent) : base(parent)
		{
		}

		protected override void CheckDateIsValidZDateTimeRange()
		{
		}
	}
}
