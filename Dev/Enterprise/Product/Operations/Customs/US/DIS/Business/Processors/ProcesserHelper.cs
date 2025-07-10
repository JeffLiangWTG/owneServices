using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DIS.Business
{
	public static class ProcesserHelper
	{
		public static GlbBranch GetFallbackBranch(IDISHost disHost, Enterprise.Messaging.Business.EDIMessage outgoingMessage, Enterprise.Messaging.Business.EDIMessage incomingMessage)
		{
			GlbBranch result = null;
			if (disHost != null && disHost.BranchPK.IsValid)
			{
				result = disHost.Factory.Load<GlbBranch>(disHost.BranchPK);
			}
			else if (outgoingMessage != null && outgoingMessage.Branch != null)
			{
				result = outgoingMessage.Branch;
			}
			else if (incomingMessage != null && incomingMessage.Branch != null)
			{
				result = incomingMessage.Branch;
			}
			return result;
		}

		public static void SendNotification(BusinessObjectFactory factory, ZString errorMessage, ZString messageText, ZGuid companyPK, ZGuid branchPK)
		{
			var email = new EmailDef();
			email.Subject = "Error processing DIS message";
			email.Body = "A DIS message had the following error during processing."
			+ System.Environment.NewLine
			+ errorMessage;
			email.Attachments.Add(new AttachmentDef("messagetext.xml", Encoding.ASCII.GetBytes(messageText)));
			var emailGroup = USCustomsDataRegistry.Instance.DISMessagesGroup;
			var groupPK = emailGroup.GetFallBackValueAtAllLevels(companyPK.ToGuid(), branchPK.ToGuid(), Guid.Empty);
			var emailRepCal = new EmailRecipientCalculator(GroupNotification.StaffMemberOrNominatedGroup, groupPK, ZString.Empty, ZGuid.Empty);
			emailRepCal.SendNotifications(factory, email, emailGroup);
		}
	}
}
