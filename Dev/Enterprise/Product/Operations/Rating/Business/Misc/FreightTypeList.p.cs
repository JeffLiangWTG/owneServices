namespace Enterprise.Rating.Business
{
	partial class FreightTypeList
	{
		public static string CodeFromRateMode(string rateCategory, string rateMode)
		{
			switch (rateCategory)
			{
				case RatingConstants.RateCategory.AIR:
				case RatingConstants.RateCategory.CAI:
					return Codes.Air;

				case RatingConstants.RateCategory.SED:
				case RatingConstants.RateCategory.SID:
				case RatingConstants.RateCategory.SCO:
					return Codes.FCL;

				case RatingConstants.RateCategory.FCL:
				case RatingConstants.RateCategory.CFC:
					switch (rateMode)
					{
						case Core.Constants.RateMode.ROA:
							return Codes.FCL_Road;
						case Core.Constants.RateMode.RAI:
							return Codes.FCL_Rail;
						default:
							return Codes.FCL;
					}

				default:
					switch (rateMode)
					{
						case Core.Constants.RateMode.SEA:
							return Codes.Sea;
						case Core.Constants.RateMode.ROA:
							return Codes.Road;
						case Core.Constants.RateMode.RAI:
							return Codes.Rail;
						case Core.Constants.RateMode.LRO:
							return Codes.LTL_Road;
						case Core.Constants.RateMode.LRA:
							return Codes.LTL_Rail;
						case Core.Constants.RateMode.FTL:
							return Codes.FTL_Road;
						case Core.Constants.RateMode.FWL:
							return Codes.FTL_Rail;
						default:
							return null;
					}
			}
		}

		public static string CodeFromRateMode(bool hasContainers, string rateMode)
		{
			switch (rateMode)
			{
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.COU:
					return Codes.Air;

				case Core.Constants.RateMode.SEA:
					return hasContainers ? Codes.FCL : Codes.Sea;

				case Core.Constants.RateMode.LCL:
					return Codes.Sea;

				case Core.Constants.RateMode.FCL:
					return Codes.FCL;

				case Core.Constants.RateMode.ROA:
					return hasContainers ? Codes.FCL_Road : Codes.Road;

				case Core.Constants.RateMode.LRO:
					return Codes.Road;

				case Core.Constants.RateMode.FRO:
					return Codes.FCL_Road;

				case Core.Constants.RateMode.FTL:
					return Codes.FTL_Road;

				case Core.Constants.RateMode.RAI:
					return hasContainers ? Codes.FCL_Rail : Codes.Rail;

				case Core.Constants.RateMode.LRA:
					return Codes.Rail;

				case Core.Constants.RateMode.FRA:
					return Codes.FCL_Rail;

				case Core.Constants.RateMode.FWL:
					return Codes.FTL_Rail;

				default:
					return null;
			}
		}
	}
}

