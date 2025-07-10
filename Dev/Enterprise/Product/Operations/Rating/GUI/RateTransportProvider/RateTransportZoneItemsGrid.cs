using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	internal class RateTransportZoneItemsGrid : ZGrid
	{
		internal RateTransportProvider TransportProvider { get; set; }

		protected override bool IsAbleToImportData
		{
			get
			{
				var ableToImportData = true;
				if (TransportProvider != null)
				{
					if (TransportProvider.CountryCode.IsEmpty || TransportProvider.CountryCodeInfo.HasNotifications())
					{
						Globals.Message.Show(Res.GetString("243AF1B0-8B2F-41D9-90D8-3F590509BF8C", "Please enter a valid Zone Country/Region before importing data."));
						ableToImportData = false;
					}
				}

				return ableToImportData;
			}
		}
	}
}

