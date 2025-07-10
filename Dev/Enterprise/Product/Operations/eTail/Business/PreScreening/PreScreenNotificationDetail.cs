using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Newtonsoft.Json;

namespace Enterprise.eTail.Business
{
	public class PreScreenNotificationDetail : IPreScreenNotificationDetail
	{
		public PreScreenNotificationDetail(string message, IGlbGroup group, bool notifyStaffMember, IHVLVConsignment consignment, string validationRuleCode)
		{
			Message = message;
			Group = group;
			NotifyStaffMember = notifyStaffMember;
			Consignment = consignment;
			ValidationRuleCode = validationRuleCode;
		}

		public string Message { get; }

		public IGlbGroup Group { get; }

		public bool NotifyStaffMember { get; }

		[JsonIgnore]
		public IHVLVConsignment Consignment { get; }

		public string ValidationRuleCode { get; }
	}
}
