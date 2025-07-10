//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateAttachmentSetValidation
//
//    This class should be used for overriding validation in AutoRateAttachmentSetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateAttachmentSetValidation : AutoRateAttachmentSetValidation
	{
		public RateAttachmentSetValidation(AutoRateAttachmentSet parent)
			: base(parent)
		{
		}

		RateAttachmentSet Set
		{
			get { return (RateAttachmentSet)Parent; }
		}

		#region TS_IsMandatory

		protected override void CheckTS_IsMandatory()
		{
			base.CheckTS_IsMandatory();

			if (Set.TS_IsMandatory &&
				RatingConstants.DocTemplateTypes.IsPricingPage(Set.TS_TemplateType))
			{
				Set.TS_IsMandatoryInfo.AddError(Res.GetString("350eef1e-c92c-4133-a6f8-806559593f9c", "You cannot make a Pricing Page mandatory"));
			}
		}

		#endregion

		#region TS_AttachmentName

		protected override void CheckTS_AttachmentName()
		{
			base.CheckTS_AttachmentName();
			MandatoryValidation.CheckEntered(Set.TS_AttachmentNameInfo);
			TranslatableDataFieldAttribute.Validate(Set.TS_AttachmentNameInfo);
		}

		#endregion

		#region TS_TemplateType

		protected override void CheckTS_TemplateType()
		{
			base.CheckTS_TemplateType();
			MandatoryValidation.CheckEntered(Set.TS_TemplateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Set.TS_TemplateTypeInfo, Set.Lookups.TemplateTypes);
		}

		#endregion

		#region TS_Sequence

		protected override void CheckTS_Sequence()
		{
			base.CheckTS_Sequence();
			CompareValidation.CheckNumberGreaterThanZero(Set.TS_SequenceInfo);
		}

		#endregion
	}
}

