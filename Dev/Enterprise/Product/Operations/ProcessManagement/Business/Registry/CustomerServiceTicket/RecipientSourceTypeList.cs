using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business
{
	public class RecipientSourceTypeList : CodeDescriptionPairList
	{
		public RecipientSourceTypeList()
		{
			AddPair(Codes.LastCompletedTaskResource, ResString.GetMultilingualString("7fae7e9d-d71e-4c77-9d2b-625953df5fad", "Last completed task resource"));
			AddPair(Codes.JobLevelWorkflowReleaseGroup, ResString.GetMultilingualString("93319e15-835f-445d-9db5-63468dc0b0b3", "Job-level workflow Release Group members"));
			AddPair(Codes.NotificationGroup, ResString.GetMultilingualString("f2fc17e1-c5d7-4816-b1ca-3ccc2f0b0d72", "Notification group members"));
		}

		public static class Codes
		{
			public const string LastCompletedTaskResource = MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource;
			public const string JobLevelWorkflowReleaseGroup = MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup;
			public const string NotificationGroup = MessageRecipientPartyTypeList.Codes.NotificationGroup;
		}
	}
}
