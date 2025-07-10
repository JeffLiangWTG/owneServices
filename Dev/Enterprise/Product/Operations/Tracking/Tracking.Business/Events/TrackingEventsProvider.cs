using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public static class TrackingEventsProvider
	{
		public static StmALogCollection GetTrackingEvents(this ITrackingEventsProvider eventProvider, TrackingSiteUser siteUser)
		{
			var result = new TrackingEventsCollection(eventProvider.Factory);
			if (siteUser != null && siteUser.CanViewEvents && WebDataRegistry.Instance.EventVisibility.Value.Count > 0)
			{
				var filter = new ZQuery();
				filter.AddToFilter(StmALogSchema.SL_Parent, GetEventsParentsPKs(eventProvider));
				filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, GetVisibleEventCodes());
				filter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				if (!WebDataRegistry.Instance.EventIncludeEstimates.Value)
				{
					filter.AddToFilter(StmALogSchema.SL_IsEstimate, false);
				}
				filter.OrderBy = string.Format("{0} {1}", StmALogSchema.SL_EventTime.Name, (WebDataRegistry.Instance.EventSortOrder.Value == EventSortOrderList.Codes.Chronological ? OrderByClause.Ascending : OrderByClause.Descending));
				result.Load(filter);
			}
			return result;
		}

		static string[] GetVisibleEventCodes()
		{
			var result = new List<string>();
			foreach (EventVisibility eventVisibility in WebDataRegistry.Instance.EventVisibility.Value)
			{
				result.Add(eventVisibility.EventCode);
			}
			return result.ToArray();
		}

		static ZGuid[] GetEventsParentsPKs(ITrackingEventsProvider eventProvider)
		{
			var result = new List<ZGuid>();
			result.Add(eventProvider.LogsParentPK);
			if (WebDataRegistry.Instance.EventIncludeRelated.Value)
			{
				foreach (var businessObject in eventProvider.BusinessObjectsWithRelatedEvents)
				{
					if (businessObject != null)
					{
						result.Add(businessObject.PK);
					}
				}
			}
			return result.ToArray();
		}
	}
}
