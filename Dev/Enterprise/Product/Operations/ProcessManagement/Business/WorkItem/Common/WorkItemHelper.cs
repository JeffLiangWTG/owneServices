using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemHelper : IWorkItemHelper
	{
		public void CreateWorkItem(BusinessObjectFactory factory, bool checkExistingWorkItem, ZString type, ZString area, ZString activityType, ZString activitySubtype, ZString priority, ZString summary, ZString details)
		{
			if (checkExistingWorkItem && FindExistingOpenWorkItem(factory, summary))
			{
				return;
			}

			var workItem = factory.New<IWorkItem>();
			workItem.WKI_WorkItemType = type;
			workItem.WKI_WorkItemArea = area;
			workItem.WKI_ActivityType = activityType;
			workItem.WKI_ActivitySubtype = activitySubtype;
			workItem.WKI_Priority = priority;
			workItem.WKI_Summary = summary;
			workItem.WKI_Details = ZBlob.FromUTF8(details);

			factory.Save();
		}

		bool FindExistingOpenWorkItem(BusinessObjectFactory factory, ZString summary)
		{
			var query = new ZDBOnlyQuery(typeof(WorkItem));
			query.AddToFilter(WorkItemSchema.WKI_Summary, summary);
			query.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
			query.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
			return factory.LoadTop1<WorkItem>(query) != null;
		}
	}
}
