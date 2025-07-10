using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal abstract class ShippingManagerInterchangeSender : BaseInterchangeSender
	{
		protected static bool SendInterchangeEmail(LoggingInformation logger, EDIInterchange interchange, ZString emailAddress, ZString subject, string[] ccEmailAddresses)
		{
			bool success = false;

			try
			{
				SendInterchangeEmailCore(interchange, emailAddress, subject, ccEmailAddresses);
				success = true;
			}
			catch (Exception ex)
			{
				if (logger == null || ex.IsCriticalException())
				{
					throw;
				}

				logger.Log("Failed to send: " + ex.Message); // Error log message
			}

			return success;
		}

		static void SendInterchangeEmailCore(EDIInterchange interchange, ZString emailAddress, ZString subject, string[] ccEmailAddresses)
		{
			byte[] content = System.Text.Encoding.ASCII.GetBytes(interchange.EI_InterchangeText);

			EmailDef email = new EmailDef();
			email.AddRecipientForSystemCommunication(emailAddress);
			email.AddRecipientForSystemCommunication(ccEmailAddresses, RecipientDef.RecipientTypes.CC);
			email.Attachments.Add(new AttachmentDef(GetFileName(interchange), content));
			email.Priority = EmailDef.PriorityFlag.High;
			email.Subject = subject;

			Env.OutgoingMailManager.CreateAndSave(email);
		}

		static string GetFileName(EDIInterchange interchange)
		{
			return interchange.EI_InterchangeNum.PadLeft(8, '0') + ".edi";
		}
	}
}


