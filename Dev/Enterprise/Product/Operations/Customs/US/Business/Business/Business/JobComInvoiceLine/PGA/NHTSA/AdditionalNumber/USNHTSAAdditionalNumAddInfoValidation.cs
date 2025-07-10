//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSAAdditonalNumAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNHTSAAdditonalNumAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USNHTSAAdditionalNumAddInfoValidation : AutoUSNHTSAAdditionalNumAddInfoValidation
	{
		public USNHTSAAdditionalNumAddInfoValidation(AutoUSNHTSAAdditionalNumAddInfo parent) : base(parent)
		{
		}

		new USNHTSAAdditionalNumAddInfo Parent
		{
			get { return (USNHTSAAdditionalNumAddInfo)base.Parent; }
		}

		NHTSAAdditionalNum Number
		{
			get { return Parent.AdditionalNum; }
		}

		protected override void CheckUS_NHTAdditionalIdentityNumQualifier()
		{
			base.CheckUS_NHTAdditionalIdentityNumQualifier();

			IdentityNumberAndLPCODetailsValidator.ValidateAdditionalIdentityNumQualifier(Parent.US_NHTAdditionalIdentityNumQualifierInfo, Number.Header, Parent.Lookups.NumberTypes);
			ValidateUS_NHTAdditionalIdentityNumber();
		}

		protected override void CheckUS_NHTAdditionalIdentityNumber()
		{
			base.CheckUS_NHTAdditionalIdentityNumber();
			var header = Number.Header;
			if (header != null && header.IsPGAValidationOn)
			{
				var numberType = Parent.US_NHTAdditionalIdentityNumQualifier;
				var number = Parent.US_NHTAdditionalIdentityNumber;

				IdentityNumberAndLPCODetailsValidator.ValidateAdditionalIdentityNumber(Parent.US_NHTAdditionalIdentityNumberInfo, number, numberType, Number.IsVehicleIdentificationNumber, header.US_NHTBoxNumber);

				if (!number.IsEmpty && !numberType.IsEmpty)
				{
					var nhtsaDetails = Number.Details;
					if (nhtsaDetails != null && nhtsaDetails.AdditionalNumbers.HasMultipleNumbersWithSameTypeAndNumber(numberType, number))
					{
						Parent.US_NHTAdditionalIdentityNumberInfo.AddMessageError(ValidationConstants.NHTSA.YouHaveEnteredMultipleNumbersWithSameTypeAndNumber);
					}
				}
			}
		}
	}
}
