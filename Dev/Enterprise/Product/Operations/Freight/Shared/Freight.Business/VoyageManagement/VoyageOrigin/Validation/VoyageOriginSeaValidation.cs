using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class VoyageOriginSeaValidation : BaseJobVoyOriginValidation
	{
		public VoyageOriginSeaValidation(AutoJobVoyOrigin parent)
			: base(parent)
		{
		}

		protected override void CheckJA_RL_NKPortOfLoading()
		{
			base.CheckJA_RL_NKPortOfLoading();
			if (Origin.PortOfLoading != null && !Origin.PortOfLoading.RL_HasSeaport)
			{
				Origin.JA_RL_NKPortOfLoadingInfo.AddWarning(Res.GetString("18d19716-3e33-40e2-bd0d-21cfcd7ff5c1", "{0} does not have a sea port.", Origin.JA_RL_NKPortOfLoading));
			}
			if (Voyage != null && Voyage.HasSelectedTradeLanes && !PortOfLoadingExistInTradeLanes)
			{
				Origin.JA_RL_NKPortOfLoadingInfo.AddError(Res.GetString("bbdc1341-8600-414d-8b54-3e411fca17b3", "This port does not match any of the specified trade lanes."));
			}
		}

		protected override bool IsDateGreater(ZDateTime datetime1, ZDateTime datetime2)
		{
			return datetime1.Date > datetime2.Date;
		}

		bool PortOfLoadingExistInTradeLanes
		{
			get
			{
				RefUNLOCO originPort = new RefUNLOCO.Loader(Origin.Factory).Load(Origin.JA_RL_NKPortOfLoading);

				if (originPort != null)
				{
					foreach (JobTradeLaneVoyage tradeLaneVoyage in Voyage.TradeLanes)
					{
						if (tradeLaneVoyage.TradeLane != null)
						{
							ILocation location1 = LocationHelper.GetLocationFromString(tradeLaneVoyage.TradeLane.EJ_Location1, Origin.Factory);
							ILocation location2 = LocationHelper.GetLocationFromString(tradeLaneVoyage.TradeLane.EJ_Location2, Origin.Factory);

							bool isValidPort = (location1 != null && location1.CompletelyCovers(originPort)) ||
								(tradeLaneVoyage.TradeLane.EJ_Direction == DirectionTypeList.Codes.BothWays &&
								location2 != null && location2.CompletelyCovers(originPort));

							if (isValidPort)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}
	}
}
