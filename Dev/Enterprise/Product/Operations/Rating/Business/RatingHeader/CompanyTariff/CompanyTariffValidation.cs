using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class CompanyTariffValidation : RatingHeaderValidation
	{
		public CompanyTariffValidation(AutoRatingHeader parent)
		: base(parent)
		{ }

		#region Properties

		protected override void CheckTH_OH()
		{
			MandatoryValidation.CheckNotEntered(Parent.TH_OHInfo);

			if (!Parent.TH_OH.IsEmpty)
			{
				Parent.TH_OHInfo.AddError(ErrorMessages.CompanyTariffMustNotHaveOrganisation);
			}
		}

		#endregion
	}
}

