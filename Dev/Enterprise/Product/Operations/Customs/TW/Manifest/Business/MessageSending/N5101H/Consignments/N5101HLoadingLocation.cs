using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HLoadingLocation : ILocation
	{
		public N5101HLoadingLocation(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		ZString ILocation.ID => bill?.ABL_RL_NKPortOfLoading ?? ZString.Empty;

		ZString ILocation.Name => bill?.ABL_LocationInformation ?? ZString.Empty;

		ZDate ILocation.LoadingDateTime => ZDate.Empty;

		ZString ILocation.EstimatedLoadingCode => ZString.Empty;
	}
}
