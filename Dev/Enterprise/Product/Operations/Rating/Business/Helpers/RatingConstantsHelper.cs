using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RatingConstantsHelper : IRatingConstantsHelper
	{
		IEnumerable<ZString> IRatingConstantsHelper.GetRateModes(ZString transportOrContainerMode)
		{
			var rateModes = new CodeDescriptionPairList(OLookUpEditType.RateModes);
			var applicableModes = new HashSet<ZString> { Core.Constants.RateMode.ALL };

			foreach (var rateMode in rateModes.GetAllCodes())
			{
				var transportMode = RatingConstants.RateMode.GetTransportModes(rateMode);
				if (transportOrContainerMode.EqualsIgnoringCase(transportMode))
				{
					applicableModes.Add(rateMode);
				}
				else if (transportOrContainerMode.EqualsIgnoringCase(rateMode))
				{
					applicableModes.Add(transportMode);
					applicableModes.Add(rateMode);
				}
			}

			return applicableModes;
		}
	}
}
