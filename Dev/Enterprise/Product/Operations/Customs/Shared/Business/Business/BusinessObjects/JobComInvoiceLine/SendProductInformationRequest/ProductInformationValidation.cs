using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class ProductInformationValidation : ZValidation
	{
		public ProductInformationValidation(ProductInformation parent) : base(parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.Parent = parent;
			this.ZValidationInternals = this;
		}

		public override Type AutoValidationType => typeof(ProductInformation);

		public override void ValidateAll()
		{
			ValidateSendFrom();
		}

		public void ValidateSendFrom()
		{
			ZValidationInternals.Validate(Parent.SendFromInfo, delegate {
				CheckSendFrom();
			});
		}

		public void CheckSendFrom()
		{
			var propertyInfo = Parent.SendFromInfo;
			if (Parent.SendFrom.IsEmpty)
			{
				var description = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				propertyInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(description));
			}
			else
			{
				EmailAddressValidation.ValidateEmailAddress(propertyInfo);
			}
		}

		#region Implementation

		ProductInformation Parent { get; }
		IValidationInternals ZValidationInternals { get; }

		#endregion
	}
}
