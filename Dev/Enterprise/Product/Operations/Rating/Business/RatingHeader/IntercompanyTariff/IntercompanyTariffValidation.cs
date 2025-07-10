using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class IntercompanyTariffValidation : RatingHeaderValidation
	{
		public IntercompanyTariffValidation(AutoRatingHeader parent)
			: base(parent)
		{ }

		public new IntercompanyTariff Parent
		{
			get { return (IntercompanyTariff)base.Parent; }
		}

		#region Properties

		#region TH_GlobalRateDescription

		protected override void CheckTH_GlobalRateDescription()
		{
			base.CheckTH_GlobalRateDescription();

			TranslatableDataFieldAttribute.Validate(Parent.TH_GlobalRateDescriptionInfo);
		}

		#endregion

		#region TH_OH

		protected override void CheckTH_OH()
		{
			MandatoryValidation.CheckEntered(Parent.TH_OHInfo);

			if (!Parent.TH_OH.IsEmpty)
			{
				if (Parent.IsAnyOtherHeaderWithSameOrgAndType())
				{
					Parent.TH_OHInfo.AddError(ErrorMessages.IntercompanyTariffForThisSupplierAlreadyExists);
				}

				ListValidation.ErrorIfInvalidPK(Parent.TH_OHInfo, Parent.Lookups.Clients, ErrorMessages.OrgProxyIsMandatoryForIntercompanyTariff);
			}
		}

		#endregion

		#region TH_GC

		protected override void CheckTH_GC()
		{
			base.CheckTH_GC();

			MandatoryValidation.CheckNotEntered(Parent.TH_GCInfo, ErrorMessages.IntercompanyTariffMustBeGlobal);
		}
		#endregion

		#endregion
	}
}


