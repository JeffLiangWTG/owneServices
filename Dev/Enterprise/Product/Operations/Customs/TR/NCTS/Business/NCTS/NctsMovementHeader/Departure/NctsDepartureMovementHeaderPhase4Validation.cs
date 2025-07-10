using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation
	{
		public NctsDepartureMovementHeaderPhase4Validation(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		protected override void CheckBM_PlaceOfLoading()
		{
			base.CheckBM_PlaceOfLoading();

			var placeOfLoading = Parent.BM_PlaceOfLoading;

			if (!placeOfLoading.IsEmpty && !placeOfLoading.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.BM_PlaceOfLoadingInfo.AddMessageError(Res.GetString("FA777C46-D319-4ABA-A28F-FB79343FB30E", "Place Of Loading should be alphanumeric."));
			}

			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_PlaceOfLoadingInfo);
		}
	}
}
