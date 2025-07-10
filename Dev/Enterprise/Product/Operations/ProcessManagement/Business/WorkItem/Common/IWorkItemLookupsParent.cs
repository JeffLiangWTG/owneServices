using CargoWise.Types;

namespace Enterprise.ProcessManagement.Business
{
	public interface IWorkItemLookupsParent
	{
		ZString WKI_WorkItemType { get; }
		ZString WKI_WorkItemArea { get; }
		ZString WKI_ActivityType { get; }
		ZString WKI_ActivitySubtype { get; }
	}
}
