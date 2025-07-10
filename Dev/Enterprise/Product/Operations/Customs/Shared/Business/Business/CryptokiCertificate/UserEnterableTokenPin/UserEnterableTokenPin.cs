using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class UserEnterableTokenPin : NonPersistentBusinessObject<UserEnterableTokenPinValidation>
	{
		static class Schema
		{
			public const int PinMaxLength = 30;
		}

		[MaxLength(Schema.PinMaxLength)]
		public ZString Pin
		{
			get { return pin; }
			set
			{
				var oldValue = Pin;
				SetNonPersistentPropertyValue(PinInfo, ref pin, value);

				if (oldValue != value && !IsValidationSuspended)
				{
					Validation.ValidatePin();
				}
			}
		}
		ZString pin;

		public ZPropertyInfo PinInfo => GetZPropertyInfo(nameof(Pin));

		public override UserEnterableTokenPinValidation GetNewValidation() => new UserEnterableTokenPinValidation(this);
	}
}
