using System.Linq;
using System.Text;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class ServiceLevelRatingInformationSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var str = string.Empty;
			var information = (ServiceLevelRatingInformation)objectToSerialize;
			if (information != null)
			{
				var serviceLevelInfos = information.ServiceLevelData;
				str = serviceLevelInfos.Aggregate(str, (current, serviceLevelInfo) => current + BuildInfoDetails(serviceLevelInfo));
			}

			return str;
		}

		static string BuildInfoDetails(ServiceLevelInfo info)
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format((NoResString)"Service Level: {0}", info.ServiceLevel));          // displays Object Information
			sb.AppendLine(string.Format((NoResString)"Service Level Type: {0}", info.ServiceLevelType)); // displays Object Information
			sb.AppendLine();

			return sb.ToString();
		}
	}
}
