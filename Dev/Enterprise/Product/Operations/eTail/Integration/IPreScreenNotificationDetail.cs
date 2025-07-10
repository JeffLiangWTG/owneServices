using Enterprise.Integration;

namespace Enterprise.eTail.Integration
{
	public interface IPreScreenNotificationDetail
	{
		string Message { get; }

		string ValidationRuleCode { get; }

		IGlbGroup Group { get; }

		bool NotifyStaffMember { get; }

		IHVLVConsignment Consignment { get; }
	}
}
