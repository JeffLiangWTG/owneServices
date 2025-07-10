using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public static class RateViewModelExtensions
	{
		public static OrgHeader GetCarrierToApplyToJob(this RateViewModel model)
		{
			var cw1Model = model as CW1RateViewModel;

			if (cw1Model != null)
			{
				return cw1Model.CarrierOrg ?? cw1Model.ServiceProviderOrg;
			}

			return model.CarrierOrg;
		}
	}
}
