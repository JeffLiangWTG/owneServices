using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	static class GSSApiHelpers
	{
		public static IEnumerable<string> GetOnlineSchedulesErrors(OnlineSchedules onlineSchedules)
		{
			var errors = onlineSchedules.NotificationsIncludingChildren.Where(notification => notification.Type == CargoWise.ComponentModel.NotificationType.Error);
			return errors.Select(error => error.Message);
		}

		public static List<ISailingModel> BuildSailingModels(List<JobSailing> allSailings, List<JobSailing> newSailings, IConnectionQuery connectionQuery = null)
		{
			var sailingModels = new List<ISailingModel>();
			foreach (var sailing in allSailings)
			{
				if (sailing == null)
				{
					// If the GSS connection is not of type SEA, there is no sailing
					// We use `null` here so that the sailings in the response correspond the the sailings given in the payload (in the same order)
					sailingModels.Add(null);
					continue;
				}

				sailingModels.Add(new SailingModel(newSailings, sailing, connectionQuery));
			}
			return sailingModels;
		}
	}
}
