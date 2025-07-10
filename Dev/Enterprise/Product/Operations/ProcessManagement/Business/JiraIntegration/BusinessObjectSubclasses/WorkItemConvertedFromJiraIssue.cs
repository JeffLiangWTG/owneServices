using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemConvertedFromJiraIssue : WorkItem
	{
		public WorkItemConvertedFromJiraIssue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override WorkRequest[] GetRelatedWorkRequests()
		{
			return System.Array.Empty<WorkRequest>();
		}

		protected override JobConversation GetOrCreateConversation()
		{
			return conversation ?? (conversation = CreateNewConversation());
		}

		JobConversation conversation;

		JobConversation CreateNewConversation()
		{
			var newConversation = JobConversation.CreateWithoutCheckingForExistingConversation(this, Factory);
			RegisterEditableChildObject(newConversation);

			return newConversation;
		}

		protected override bool SendEmailNotificationsOnSaveCore => false;
	}
}
