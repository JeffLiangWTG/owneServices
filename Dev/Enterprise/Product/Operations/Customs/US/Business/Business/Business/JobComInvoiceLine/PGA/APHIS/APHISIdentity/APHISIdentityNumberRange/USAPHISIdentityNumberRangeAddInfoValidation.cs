//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISIdentityNumberRangeAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISIdentityNumberRangeAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISIdentityNumberRangeAddInfoValidation : AutoUSAPHISIdentityNumberRangeAddInfoValidation
	{
		public USAPHISIdentityNumberRangeAddInfoValidation(AutoUSAPHISIdentityNumberRangeAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_StartNumber()
		{
			base.CheckUS_StartNumber();
			if (IsACECargoReleaseValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_StartNumberInfo);
				ValidateUS_EndNumber();
			}
		}

		protected override void CheckUS_EndNumber()
		{
			base.CheckUS_EndNumber();
			if (Parent.US_StartNumber.IsEmpty && !Parent.US_EndNumber.IsEmpty && IsACECargoReleaseValidationMode)
			{
				Parent.US_EndNumberInfo.AddMessageError(ValidationConstants.APHIS.IdentityEndNumberRequiresStartNumber);
			}
		}

		protected new USAPHISIdentityNumberRangeAddInfo Parent
		{
			get { return (USAPHISIdentityNumberRangeAddInfo)base.Parent; }
		}

		protected APHISIdentityNumberRange Identity
		{
			get { return Parent.Parent; }
		}

		protected APHISProduct Product
		{
			get
			{
				var identity = Identity;
				return identity == null ? null : identity.Product;
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var result = false;
				var product = Product;
				if (product != null)
				{
					var header = product.Header;
					var invoiceLine = header == null ? null : header.Parent;
					result = invoiceLine != null && invoiceLine.IsACECargoReleaseValidationMode;
				}
				return result;
			}
		}
	}
}
