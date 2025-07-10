using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class ClientRateValidation : RatingHeaderValidation
	{
		public ClientRateValidation(AutoRatingHeader parent)
			: base(parent)
		{
		}

		public new ClientRate Parent
		{
			get { return (ClientRate)base.Parent; }
		}

		#region Validation 

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTH_NewRateEndDate();
		}

		#endregion

		#region Properties

		#region TH_NewRateEndDate

		public void ValidateTH_NewRateEndDate()
		{
			ValidateCalculatedProperty(Parent.TH_NewRateEndDateInfo);
		}

		protected void CheckTH_NewRateEndDate()
		{
			if (Parent.TH_NewRateEndDate <= ZDateTime.Today)
			{
				Parent.TH_NewRateEndDateInfo.AddError(ErrorMessages.NewEndDateError);
			}
		}

		#endregion

		protected override void CheckTH_OH()
		{
			MandatoryValidation.CheckEntered(Parent.TH_OHInfo);

			if (!Parent.TH_OH.IsEmpty)
			{
				if (Parent.IsAnyOtherHeaderWithSameOrgAndType())
				{
					if (Parent.IsGlobalClientRate())
					{
						Parent.TH_OHInfo.AddError(ErrorMessages.GlobalRateForThisClientAlreadyExists);
					}
					else
					{
						Parent.TH_OHInfo.AddError(ErrorMessages.RateForThisClientAlreadyExists);
					}
				}

				ListValidation.ErrorIfInvalidPK(Parent.TH_OHInfo, Parent.Lookups.Clients, ErrorMessages.InvalidClientRateHeader);
			}
		}

		#endregion
	}
}

