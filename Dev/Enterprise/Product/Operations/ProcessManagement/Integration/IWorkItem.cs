using CargoWise.Types;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IWorkItem
	{
		ZGuid PK { get; }
		ZString WKI_Summary { get; set; }
		ZString WKI_WorkItemType { get; set; }
		ZString WKI_WorkItemArea { get; set; }
		ZString WKI_ActivityType { get; set; }
		ZString WKI_ActivitySubtype { get; set; }
		ZString WKI_Priority { get; set; }
		ZString WKI_SystemCreateUser { get; set; }
		ZString WKI_WorkItemNumber { get; set; }
		ZString WKI_Status { get; set; }
		ZBlob WKI_Details { get; set; }
	}
}
