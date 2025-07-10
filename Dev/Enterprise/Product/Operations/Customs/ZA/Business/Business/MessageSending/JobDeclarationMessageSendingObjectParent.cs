using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class JobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<MessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;
		public bool HasDutiableSendingObject => SendingObjectsCollection.Cast<MessageSendingObject>().Any(obj => obj.IsDutiable);

		public void UpdateFromDeferredSubmission(DeferredSubmission submission)
		{
			var agentChanged = submission.AgentChanged;
			var submissionDate = submission.SubmissionDate;
			var deferMessages = submission.CanDeferMessages && submissionDate > ZDateTime.Today;

			foreach (MessageSendingObject sendingObject in SendingObjectsCollection)
			{
				if (agentChanged)
				{
					sendingObject.RefreshLocalReferenceNumber();
				}
				if (deferMessages)
				{
					sendingObject.SubmissionDate = sendingObject.PaymentMethodIsDeferOrVAT ? submissionDate : ZDateTime.Empty;
				}
			}

			ParentDeclaration.Factory.Save();
		}

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			if (sendingObjectsCollection == null)
			{
				sendingObjectsCollection = new MessageSendingObjectCollection(Factory);
				foreach (CusEntryHeader header in ParentDeclaration.ActiveEntryHeaders)
				{
					sendingObjectsCollection.Add(new MessageSendingObject(header));
				}
				RegisterEditableChildObject(sendingObjectsCollection);
			}
			return sendingObjectsCollection;
		}
		MessageSendingObjectCollection sendingObjectsCollection;

		internal const string DocumentApprovalReasonDescription = "Request for adjustment to Temporary Credit Limit.";

		public bool IsMessagingPOC { get; set; }
	}
}
