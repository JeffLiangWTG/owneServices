using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Tracking.Business
{
	public interface IBizOChangesEmailNotification
	{
		ZGuid PK { get; }
		ZString HumanReadableName { get; }
		ZString Number { get; }
		OrgContact LoggedInContact { get; }
		ZBool IsCancelled { get; }
		GlbBranch EventBranch { get; }
		GuidRegistryItem EmailGroupRegistryItem { get; }
		BusinessObjectFactory Factory { get; }
		bool IsInDatabase { get; }
		bool IsDeleted { get; }
		bool HasChanges { get; }
		void AddPropertiesForEmailReporting(DataState state);
		PropertyChangeInfo[] GetPropertiesForEmailReporting();
		ControllerID ControllerForEnterpriseUrl { get; }
		OrgHeader RelatedOrg { get; }
		ZGuid GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role);
		CodeDescriptionBoolRegistryItem StaffRolesToNotify { get; }
		CodePairRegistryItem NotificationSendingRule { get; }
	}
}
