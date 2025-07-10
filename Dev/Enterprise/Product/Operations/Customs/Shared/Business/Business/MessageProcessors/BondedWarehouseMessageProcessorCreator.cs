using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors
{
	public class BondedWarehouseMessageProcessorCreator
	{
		public BondedWarehouseMessageProcessorCreator(ILoggingInformation logger, GetNewBondedWarehouseMessageProcessorDelegate getNewBondedWarehouseMessageProcessor, GetFallbackNotificationGroupPKDelegate getFallbackNotificationGroupPK)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.getNewBondedWarehouseMessageProcessor = Argument.NotNull(getNewBondedWarehouseMessageProcessor, "getNewBondedWarehouseMessageProcessor");
			this.getFallbackNotificationGroupPK = Argument.NotNull(getFallbackNotificationGroupPK, "getFallbackNotificationGroupPK");
		}

		public delegate BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessorDelegate(Action<EmailDef, EDIMessage> sendMail);
		public delegate Guid GetFallbackNotificationGroupPKDelegate(Guid companyPK, Guid branchPK, Guid departmentPK);

		public void ProcessBondedWarehouseOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= ProcessBondedWarehouseOnFactorySaved;
			}
			getNewBondedWarehouseMessageProcessor(SendEmail).ProcessAfterSaved(savedSuccessfully);
		}

		public void SendEmail(EmailDef email, EDIMessage message)
		{
			try
			{
				if (email.Recipients.Count > 0)
				{
					Env.OutgoingCustomsMailManager.CreateAndSave(email);
				}
				else
				{
					var provider = message.EM_LinkedObject as IDeclarationProvider;
					var declaration = provider?.Declaration;
					AddFallBackRecipient(email, declaration, message);
					if (email.Recipients.Count > 0)
					{
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}
					else
					{
						var groupPK = getFallbackNotificationGroupPK(declaration?.RegistryCompanyPK ?? Guid.Empty, declaration?.RegistryBranchPK ?? Guid.Empty, declaration?.Job?.Department?.PK.ToGuid() ?? Guid.Empty);
						var groupSourceLocator = GroupSourceLocator.GetFromGroup(message.Factory.Load<GlbGroup>(groupPK));
						if (groupSourceLocator != null)
						{
							Env.OutgoingCustomsMailManager.CreateAndSave(email, groupPK, groupSourceLocator);
						}
					}
				}

				logger.Log("Email sent.\r\n\r\n" +
					"SUBJECT: " + email.Subject + "\r\n");
			}
			catch (EmailSendFailedException e)
			{
				logger.LogError((NoResString)"Couldn't send email: " + e.Message + (NoResString)".  Here are the contents of the email that couldn't be sent:\r\n\r\n" +
					(NoResString)"SUBJECT: " + email.Subject + (NoResString)"\r\n" +
					(NoResString)"BODY: " + email.Body + (NoResString)"\r\n");
			}
		}

		public void AddFallBackRecipient(EmailDef email, BaseJobDeclaration declaration, EDIMessage message)
		{
			var emailAddress = message?.UserWhoQueuedThisRecord?.GS_EmailAddress ?? ZString.Empty;
			if (emailAddress.IsEmpty)
			{
				emailAddress = declaration?.CusAgent?.GS_EmailAddress ?? ZString.Empty;
			}

			if (!emailAddress.IsEmpty)
			{
				email.AddRecipientForSystemCommunication(emailAddress);
			}
		}

		readonly ILoggingInformation logger;
		readonly GetNewBondedWarehouseMessageProcessorDelegate getNewBondedWarehouseMessageProcessor;
		readonly GetFallbackNotificationGroupPKDelegate getFallbackNotificationGroupPK;
	}
}
