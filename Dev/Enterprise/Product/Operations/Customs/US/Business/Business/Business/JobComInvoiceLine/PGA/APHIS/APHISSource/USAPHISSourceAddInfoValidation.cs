//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISSourceAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISSourceAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USAPHISSourceAddInfoValidation : AutoUSAPHISSourceAddInfoValidation
	{
		public USAPHISSourceAddInfoValidation(AutoUSAPHISSourceAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_SourceTypeCode()
		{
			base.CheckUS_SourceTypeCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_SourceTypeCodeInfo, Parent.Lookups.SourceTypes);
			}
		}

		protected override void CheckUS_ProcessingTypeCode()
		{
			base.CheckUS_ProcessingTypeCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingTypeCodeInfo, Parent.Lookups.ProcessingTypes);
				ValidateUS_ProcessingDescription();
			}
		}

		protected override void CheckUS_ProcessingDescription()
		{
			base.CheckUS_ProcessingDescription();
			if (Source.IsOtherTreatmentType && Parent.US_ProcessingDescription.IsEmpty && IsPGAValidation)
			{
				Parent.US_ProcessingDescriptionInfo.AddMessageError(ValidationConstants.APHIS.ProcessingDescriptionRequiresForOtherTreatmentType);
			}
		}

		protected override void CheckUS_CountryCode()
		{
			base.CheckUS_CountryCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CountryCodeInfo, Parent.Lookups.Countries);
			}
		}

		protected new USAPHISSourceAddInfo Parent
		{
			get { return (USAPHISSourceAddInfo)base.Parent; }
		}

		protected APHISSource Source
		{
			get { return Parent.Parent; }
		}

		protected APHISHeader Header
		{
			get
			{
				var source = Source;
				return source == null ? null : source.Header;
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null)
				{
					var invoiceLine = aphisHeader.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
