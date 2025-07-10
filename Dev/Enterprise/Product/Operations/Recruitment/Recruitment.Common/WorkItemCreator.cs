using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Recruitment.Registry;

namespace Enterprise.Recruitment.Common
{
	public static class WorkItemCreator
	{
		public static WorkItem CreateWorkItem(BusinessObjectFactory factory, WorkItemTemplateProperties template, string extraText = "")
		{
			var workItem = factory.New<WorkItem>();

			using (workItem.GetValidationSuspender())
			{
				if (template != null)
				{
					// anything in template longer than 3 chars is invalid according to DNK
					workItem.WKI_WorkItemType = template.Type.Substring(0, 3);
					workItem.WKI_WorkItemArea = template.Area.Substring(0, 3);
					workItem.WKI_ActivityType = template.ActivityType.Substring(0, 3);
					workItem.WKI_ActivitySubtype = template.ActivitySubType.Substring(0, 3);
					workItem.WKI_Priority = template.Priority.Substring(0, 3);
					workItem.WKI_Summary = $"{template.FriendlyName} - {extraText}";

					workItem.ApplyWorkflowTemplates();
				}
			}
			return workItem;
		}
	}
}
