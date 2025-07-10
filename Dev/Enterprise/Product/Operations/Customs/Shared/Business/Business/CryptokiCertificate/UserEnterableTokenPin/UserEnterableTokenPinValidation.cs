using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class UserEnterableTokenPinValidation : ZValidation
	{
		public UserEnterableTokenPinValidation(UserEnterableTokenPin parent) : base(parent)
		{
		}

		public override Type AutoValidationType => typeof(UserEnterableTokenPinValidation);

		UserEnterableTokenPin Parent => (UserEnterableTokenPin)ParentFilter;

		public override void ValidateAll()
		{
			ValidatePin();
		}

		public void ValidatePin()
		{
			ValidateCalculatedProperty(Parent.PinInfo);
		}

		protected void CheckPin()
		{
			MandatoryValidation.CheckEntered(Parent.PinInfo);
		}
	}
}
