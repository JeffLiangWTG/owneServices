using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Consol
{
	public class EmailToOverseaAgentExporter : IXmlDataTransferExporter
	{
		public void PromptUserAndExport(IList selectedElements)
		{
			foreach (ForwardingConsol consol in selectedElements)
			{
				PromptUserAndExport(consol);
			}
		}

		public void PromptUserAndExport(ForwardingConsol consol)
		{
			var fileName = new XmlDataTransferExporter(new ForwardingConsolValueObjectDataAdapter(), true).Export(new[] { consol });
			if (!File.Exists(fileName))
			{
				return;
			}
			try
			{
				var mode = XmlCommunicationsMode(consol);
				if (mode != null)
				{
					string recipientEmailAddress = mode.EK_Destination;
					if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(recipientEmailAddress))
					{
						var fileDisplayName = "Consol_" + consol.JK_UniqueConsignRef + "___MasterBill_" + consol.JK_MasterBillNum +
											  ".xml"; // Default File Name
						SendFileAsEmailAttachment(fileName, fileDisplayName, recipientEmailAddress);
						Globals.Message.ShowInformation(Res.GetString("d701d2b1-daf0-4c58-a44c-a5c8a0c421f3",
																	  "Consol is sent to '{0}' as XML file.",
																	  mode.Organisation.OH_FullNameTruncated));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("388f6ffe-ed7b-46e7-8cd4-ab1552655f36",
																"Recipient email address '{0}' is not valid.", recipientEmailAddress));
					}
				}
				else
				{
					var agentPrefix = consol.IsImport()
										? Res.GetString("6d86b020-01ae-41c2-8534-41624a643772", "Sending")
										: Res.GetString("0f63d491-46de-469e-8e4b-9cf6ef56dcf5", "Receiving");
					var error = Res.GetString("86c09c93-1da5-4687-a1d5-199ac2c8bcaf",
											  "You must specify a {0} Agent on the Consol, and they must have an EDI Communications Mode for transport mode 'E-mail Address as Attachment' set up on the Organization -> Config -> EDI Communications tab.",
											  agentPrefix);
					Globals.Message.ShowError(error);
				}
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		public void PromptUserAndExport(ZQuery exportQuery)
		{
			throw new System.NotImplementedException();
		}

		EDICommunicationsMode XmlCommunicationsMode(ForwardingConsol consol)
		{
			var recipientOrg = consol.IsImport() ? consol.SendingForwarder : consol.ReceivingForwarder;

			EDICommunicationsMode result = null;
			if (recipientOrg != null)
			{
				result = recipientOrg.EDICommunicationsModes.GetXmlCommunicationMode(JobInvoicingConsumerTypes.Consol.Code);
			}

			if (result != null && result.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment)
			{
				result = null;
			}

			return result;
		}

		static void SendFileAsEmailAttachment(string fileName, string fileDisplayName, string recipientEmailAddress)
		{
			var attachment = new AttachmentDef(fileDisplayName, fileName);

			var email = new EmailDef();
			email.Subject = (NoResString)"ediEnterprise Consol XML File"; // Email Subject
			email.AddRecipientForUserCommunication(recipientEmailAddress);
			email.Attachments.Add(attachment);
			email.FromAddress = Env.Registry.MailboxEmailAddress;
			email.FromDisplayName = Env.Registry.MailboxDisplayName;

			Env.OutgoingMailManager.CreateAndSave(email);
		}
	}
}
