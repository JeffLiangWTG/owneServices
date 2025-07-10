namespace Enterprise.Rating.Business
{
	using System.Collections.Generic;

	public class TransportZoneSetComparer : IComparer<RateTransportProvider>
	{
		public int Compare(RateTransportProvider transportZoneSet1, RateTransportProvider transportZoneSet2)
		{
			var result = 0;

			if (transportZoneSet1.TP_ZoneType != transportZoneSet2.TP_ZoneType)
			{
				switch (transportZoneSet1.TP_ZoneType)
				{
					case RatingConstants.RatingZoneTypes.All:
						result += 1;
						break;
					case RatingConstants.RatingZoneTypes.Rating:
						result += transportZoneSet2.TP_ZoneType != RatingConstants.RatingZoneTypes.All ? 1 : -1;
						break;
					default:
						result += -1;
						break;
				}
			}

			if (transportZoneSet1.TP_RN_NKCountry != transportZoneSet2.TP_RN_NKCountry || transportZoneSet1.TP_R9_ZoneHubLocation != transportZoneSet2.TP_R9_ZoneHubLocation)
			{
				if (transportZoneSet1.TP_R9_ZoneHubLocation != transportZoneSet2.TP_R9_ZoneHubLocation)
				{
					result += transportZoneSet1.TP_R9_ZoneHubLocation.IsEmpty ? 64 : -64;
				}
				else
				{
					result += transportZoneSet1.TP_RN_NKCountry.IsEmpty ? 32 : -32;
				}
			}

			if (transportZoneSet1.TP_OH_RelatedParty != transportZoneSet2.TP_OH_RelatedParty)
			{
				result += transportZoneSet1.TP_OH_RelatedParty.IsEmpty ? 128 : -128;
			}

			if (result >= 0)
			{
				result += CompareZonesModes(transportZoneSet1, transportZoneSet2);
			}

			return result;
		}

		int CompareZonesModes(RateTransportProvider transportZoneSet1, RateTransportProvider transportZoneSet2)
		{
			var result = 0;

			if (transportZoneSet1.TP_ZoneMode != transportZoneSet2.TP_ZoneMode)
			{
				switch (transportZoneSet1.TP_ZoneMode)
				{
					case Core.Constants.RateMode.ALL:
						result += 4;
						break;

					case Core.Constants.RateMode.AIR:
					case Core.Constants.RateMode.SEA:
					case Core.Constants.RateMode.ROA:
					case Core.Constants.RateMode.RAI:
						result += transportZoneSet2.TP_ZoneMode != Core.Constants.RateMode.ALL ? 8 : -8;
						break;

					default:
						result += -16;
						break;
				}
			}

			return result;
		}
	}
}

