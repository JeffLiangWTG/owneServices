using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class RelatedBusinessDataValidation : AutoRelatedBusinessDataValidation
	{
		public RelatedBusinessDataValidation(AutoRelatedBusinessData parent)
			: base(parent)
		{
		}

		public new RelatedBusinessData Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RelatedBusinessData)base.Parent; }
		}

		protected override void CheckUS_RelatedBusiness()
		{
			base.CheckUS_RelatedBusiness();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_RelatedBusinessInfo, Parent.Lookups.RelatedBusinessTypes);
		}

		protected override void CheckUS_NameOfEntity()
		{
			base.CheckUS_NameOfEntity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NameOfEntityInfo);
		}

		protected override void CheckUS_Number()
		{
			base.CheckUS_Number();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NumberInfo);

			if (!Parent.US_Number.IsEmpty)
			{
				if (!EmployerIdentificationNumberValidator.IsValidEIN(Parent.US_Number) &&
					!SocialSecurityNumberValidator.IsValidSSN(Parent.US_Number) &&
					!CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.US_Number))
				{
					Parent.US_NumberInfo.AddMessageError(NumberNotInRightFormat);
				}
			}
		}
		internal const string NumberNotInRightFormat = "The number is not in a valid format of EIN or SSN or CBP Assigned Number.\n" +
			EmployerIdentificationNumberValidator.EINNumberRightFormat + "\n" + SocialSecurityNumberValidator.SocialSecurityNumberRightFormat + "\n\n" + CBPAssignedNumberValidator.CBPAssignedNumberRightFormat;
	}
}
