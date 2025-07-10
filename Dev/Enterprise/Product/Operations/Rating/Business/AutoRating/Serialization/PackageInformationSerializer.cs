using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class PackageInformationSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var information = (PackageInformation)objectToSerialize;
			if (information != null)
			{
				return new[]
				{
					ZString.Format((NoResString)"Commodity Code: {0}", information.CommodityCode), // displays Object Information
					ZString.Format((NoResString)"Count: {0}", information.Count), // displays Object Information
					ZString.Format((NoResString)"Volume: {0}", information.Volume), // displays Object Information
					ZString.Format((NoResString)"Volume Unit: {0}", information.VolumeUnit), // displays Object Information
					ZString.Format((NoResString)"Identifier: {0}", information.Identifier), // displays Object Information
					ZString.Format((NoResString)"Description: {0}", information.Description) // displays Object Information
				}
					.ToStringWithNewLineBetweenStrings();
			}

			return string.Empty;
		}
	}
}
