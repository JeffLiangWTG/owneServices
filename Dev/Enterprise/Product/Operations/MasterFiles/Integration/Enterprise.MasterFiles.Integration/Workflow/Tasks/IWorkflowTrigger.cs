using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowTrigger : IBaseTrigger, IMilestoneDateDefaultable
	{
		ZDateTimeOffset LastFiredTime { get; }
		ZGuid ParentTemplateID { get; }
		bool TrySetActualDateForEvent(IWorkflowTriggerSource eventSource, BusinessObject job, ZDateTimeOffset actualDate);

		void UpdateTriggerActionNotifications(NotificationCollection notifications);
	}
}
