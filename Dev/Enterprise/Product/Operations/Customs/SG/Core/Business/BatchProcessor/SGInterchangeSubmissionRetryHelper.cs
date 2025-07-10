using System.Text;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class SGInterchangeSubmissionRetryHelper
	{
		public SGInterchangeSubmissionRetryHelper()
		{
		}

		public bool IsNextRetryLastRetry(EDIInterchange interchange)
		{
			return interchange.EI_RetryCount.Equals(SGCustomsDataRegistry.Instance.SubmissionRetryLimit.Value);
		}

		public void AddDelayToLinkedEDIMessage(EDIInterchange interchange)
		{
			if (SGCustomsDataRegistry.Instance.SubmissionFinalRetryDelay.Value > 0)
			{
				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_HeldUntilDate = HeldUntilDateWithDelay;
				}
			}
		}

		public bool IsLinkedMessageDelayed(EDIInterchange interchange)
		{
			if (interchange.ContainedMessages.Count == 1)
			{
				var message = interchange.ContainedMessages[0];
				return !message.EM_HeldUntilDate.IsEmpty && message.EM_HeldUntilDate.IsInTheFuture();
			}
			return false;
		}

		public bool MaxRetriesExceeded(EDIInterchange interchange)
		{
			return interchange.EI_RetryCount > SGCustomsDataRegistry.Instance.SubmissionRetryLimit.Value;
		}

		public void OnMaxRetriesExceeded(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Failed;

			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				CusEntryHeader cusEntryHeader = message.EM_LinkedObject as CusEntryHeader;
				if (cusEntryHeader != null)
				{
					cusEntryHeader.CH_Status = cusEntryHeader.SG_PreviousEntryStatus;
				}
			}

			SendSubmissionFailureNotification(interchange);
		}

		void SendSubmissionFailureNotification(EDIInterchange interchange)
		{
			EmailDef email = new EmailDef();
			email.Body = string.Format(@"Attempts to send an outbound declaration message directly were unsuccessful. 
The outbound message/interchange has been marked as failed.  
Once you have investigated the problem, you may need to re-send the interchange or message. 
Please see the Service Task log to see the exact reason for the failures.
The interchange's details are: #{0}, {1}-->{2}.",
			interchange.EI_InterchangeNum, interchange.EI_From, interchange.EI_To);
			email.Subject = "Unable to send SG customs declaration message";

			AttachmentDef att = new AttachmentDef("Failed Interchange.txt", GetBytesFromInterchangeText(interchange));
			email.Attachments.Add(att);
			Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(email, interchange.Factory);
		}

		byte[] GetBytesFromInterchangeText(EDIInterchange interchange)
		{
			StringBuilder text = new StringBuilder();
			text.Append(interchange.EI_HeaderText);
			text.Append(System.Environment.NewLine);
			text.Append(interchange.EI_BodyText);
			text.Append(System.Environment.NewLine);
			text.Append(interchange.EI_FooterText);
			return System.Text.Encoding.ASCII.GetBytes(text.ToString());
		}

		ZDateTime HeldUntilDateWithDelay
		{
			get
			{
				if (heldUntilDateWithDelay.IsEmpty)
				{
					heldUntilDateWithDelay = ZDateTime.Now.AddMinutes(SGCustomsDataRegistry.Instance.SubmissionFinalRetryDelay.Value);
				}
				return heldUntilDateWithDelay;
			}
		}
		ZDateTime heldUntilDateWithDelay;
	}
}
