using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class Utf8StringWriter : StringWriter
	{
		public Utf8StringWriter(IFormatProvider formatProvider)
			: base(formatProvider)
		{
		}

		public override Encoding Encoding => Encoding.UTF8;
	}

	public abstract class WhsXmlExportToEmailDirector<TDocket, TValueObject> : WhsXmlExportDirector<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsXmlExportToEmailDirector(WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override void AddSuccessNotification()
		{
			Globals.Message.ShowInformation(Res.GetString("7a81ca60-ac84-4d75-8e4b-e65ae4facdb5", "Email with {0} XML attachment successfully sent.", GetDocketTypeDescription()), Res.GetString("7b5db75a-6769-4c60-82c9-7c112976d981", "Send email with {0} XML attachment", GetDocketTypeDescription()));
		}

		protected override bool ExportToXmlCore(TDocket docket, INotifications notify)
		{
			using (var writer = new Utf8StringWriter(CultureInfo.CurrentCulture))
			{
				XmlExporter.Export(docket, writer, notify);
				SendEmail(writer.GetStringBuilder().ToString(), docket);
			}
			return true;
		}

		protected override ZString GetCheckExportConditionsAreMet(TDocket docket)
		{
			var result = base.GetCheckExportConditionsAreMet(docket);
			if (result.IsEmpty)
			{
				var client = docket.Client;
				EDICommunicationsMode mode = null;
				if (client != null)
				{
					mode = GetCommunicationsModeForXMLEmailAttachment(client);
				}

				if (client != null && client.MiscServ != null && mode != null)
				{
					if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(mode.EK_Destination))
					{
						result = Res.GetString("4c700c3f-1ccb-473a-97bc-493ec0ad1739", "The Client does not have a valid {0} email address. Please set it on Client's Config->General->Organization Transmission Address", EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment);
					}
				}
				else
				{
					result = Res.GetString("f1d7d087-8013-4492-80c4-757212146326", "Client or its communication email address is not entered.");
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		protected abstract string GetJobInvoicingConsumerType();

		protected virtual string EDICommunicationsModeFileFormat => EDICommunicationsModeFileFormatList.Codes.XML;

		EDICommunicationsMode GetCommunicationsModeForXMLEmailAttachment(OrgHeader client)
		{
			var modes = client.EDICommunicationsModes.FindByModuleAndFileFormat(GetJobInvoicingConsumerType(), EDICommunicationsModeFileFormat);
			foreach (var mode in modes)
			{
				if (mode.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment)
				{
					return mode;
				}
			}

			return null;
		}

		void SendEmail(ZString attachmentContent, TDocket docket)
		{
			var mail = new EmailDef
			{
				Subject = GetEmailSubject()
			};
			mail.AddRecipientForUserCommunication(GetCommunicationsModeForXMLEmailAttachment(docket.Client).EK_Destination);

			var fileName = OrgProxy.OH_Code + "_" + docket.WD_ExternalReference + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture) + ".xml";
			var attachment = new AttachmentDef(fileName, Encoding.ASCII.GetBytes(attachmentContent));
			mail.Attachments.Add(attachment);
			Env.OutgoingMailManager.CreateAndSave(mail);
		}

		protected virtual ZString GetEmailSubject()
		{
			return Res.GetString("a03abe3f-bf94-4bf7-a47e-bd23ed50f445", "{0} Warehouse XML File", Core.Constants.ProductName);
		}

		#endregion
	}
}
