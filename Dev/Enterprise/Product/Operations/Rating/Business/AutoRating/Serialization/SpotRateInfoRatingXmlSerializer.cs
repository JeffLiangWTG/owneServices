using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public static class SpotRateInfoRatingXmlSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var spotRateInfo = (SpotRateInfo)objectToSerialize;

			if (spotRateInfo == null)
			{
				return string.Empty;
			}

			var stringBuilder = new ZStringBuilder();
			var currencyCode = spotRateInfo.Rate.Currency == null ? string.Empty : spotRateInfo.Rate.Currency.Code;
			stringBuilder.Append(ZString.Format((NoResString)"Rate: {0} {1}", spotRateInfo.Rate.Amount, currencyCode)); // displays Object Information
			stringBuilder.Append(ZString.Format((NoResString)"Mode: {0}", spotRateInfo.AutoratedMode));                 // displays Object Information
			stringBuilder.Append(ZString.Format((NoResString)"Type: {0}", spotRateInfo.AutoratedValueType.GetCaption()));   // displays Object Information

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}
	}
}
