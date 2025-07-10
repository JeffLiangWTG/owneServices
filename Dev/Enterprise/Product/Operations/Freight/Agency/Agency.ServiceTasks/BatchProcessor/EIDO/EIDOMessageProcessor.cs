using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal sealed class EIDOMessageProcessor : MessageProcessor
	{
		public EIDOMessageProcessor(LoggingInformation logger)
			: base(logger, EDIInterchange.ApplicationCodes.EIDO, Res.GetString("9a6efd40-e6ce-4c6d-bd0d-7a63a2a95e2b", "Electronic Import Delivery Order")) { }

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			IEIDOResponseMessage response = null;
			EIDOMessage originalMessage = null;
			BillOfLading shipment = null;
			BillOfLadingContainer container = null;

			try
			{
				response = EIDOResponseDecoder.Parse(message.EM_MessageText);

				originalMessage = FindOriginalMessage(message.Factory, response);
				if (originalMessage == null)
				{
					throw new InvalidFormatException("Received a response for a message we have no record of sending.");
				}

				message.EM_LinkTable = originalMessage.EM_LinkTable;
				message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;

				container = message.Factory.Load<BillOfLadingContainer>(message.EM_LinkUniqueID);
				if (container == null)
				{
					throw new InvalidFormatException("Received a response for a message that is not attached to a container.");
				}

				shipment = container.Booking;
				if (shipment == null)
				{
					throw new InvalidFormatException("Received a response for a message that is not attached to a shipment.");
				}

				switch (response.ResponseType)
				{
					case EIDOResponseType.Accepted:
						this.SendAcknowledgementReport(container, CreateProcessEmail(Res.GetString("ef71aa84-29f5-4457-8b78-8c359987ffa7", "Accepted"), shipment.JS_UniqueConsignRef, container.JC_ContainerNum));
						container.Logs.AddNew(Events.MessageAccepted, string.Format(CultureInfo.InvariantCulture, "E-IDO Interchange Number {0}", originalMessage.EM_InterchangeNumber), GetParamtersForEvent());
						originalMessage.EM_Status = EDIMessage.Status.Received;
						return EDIMessage.Status.Recognised;

					case EIDOResponseType.Received:
						this.SendAcknowledgementReport(container, CreateProcessEmail(Res.GetString("47674e7f-46ee-4375-b1b6-1f776219e5c0", "Received"), shipment.JS_UniqueConsignRef, container.JC_ContainerNum));
						container.Logs.AddNew(Events.InterchangeReceiptAcknowledged, string.Format(CultureInfo.InvariantCulture, "E-IDO Interchange Number {0}", originalMessage.EM_InterchangeNumber), GetParamtersForEvent());
						if (originalMessage.EM_Status != EDIMessage.Status.Received)
						{
							originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
						}
						return EDIMessage.Status.Recognised;

					case EIDOResponseType.Rejected:
						this.SendErrorReport(container, CreateProcessEmail(Res.GetString("1e063d22-1290-40af-80f6-ee857207ff4d", "Rejected"), shipment.JS_UniqueConsignRef, container.JC_ContainerNum, response));
						container.Logs.AddNew(Events.MessageRejected, string.Format(CultureInfo.InvariantCulture, "E-IDO Interchange Number {0}", originalMessage.EM_InterchangeNumber), GetParamtersForEvent());
						originalMessage.EM_Status = EDIMessage.Status.Rejected;
						return EDIMessage.Status.Recognised;

					default:
						// Dont use a InvalidFormatException as this indicates a bug in the handling code and not a problem in the received message.
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Dont know how to process this response type ({0})", response.ResponseType));
				}
			}
			catch (InvalidFormatException ex)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine(ex.Message);
				builder.AppendLine();

				if (container != null)
				{
					if (shipment != null)
					{
						builder.Append(Res.GetString("5cea36fc-b702-45c9-b497-fa2665d4ee3c", "Shipment Number: {0}", shipment.JS_UniqueConsignRef) + "\r\n");
						builder.Append(Res.GetString("ef44ec1c-52d2-4060-8ce4-ee9122a9f101", "Ocean Bill Of Lading: {0}", shipment.JS_HouseBill) + "\r\n");
					}
					builder.Append(Res.GetString("2d457019-703f-4dc9-bcf1-ad7aca7e22a3", "Container Number: {0}", container.JC_ContainerNum) + "\r\n\r\n");
				}

				if (response != null)
				{
					Format(builder, response);
				}

				EmailDef email = new EmailDef();
				email.Subject = Res.GetString("49a51968-ffe9-4dfe-82e1-b12cd1c6e462", "Error processing an E-IDO response.");
				email.ContentType = EmailContentTypes.PlainText;
				email.Body = builder.ToString();
				email.Attachments.Add(MessageAsAttachment(message, "response.edi"));

				SendErrorReport(container, email);
				return EIDOMessage.Status.Failed;
			}
		}

		static KeyValuePair<string, string> GetParamtersForEvent()
		{
			return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "1-stop");
		}

		#region CreateProcessEmail

		static EmailDef CreateProcessEmail(string verb, string shipmentNumber, string containerNumber)
		{
			EmailDef email = new EmailDef();
			email.Subject = Res.GetString("2a1d838e-d34e-4d9a-92d6-8aad052a3dc2", "{0} E-IDO for container {1} on {2}", verb, containerNumber, shipmentNumber);

			return email;
		}
		static EmailDef CreateProcessEmail(string verb, string shipmentNumber, string containerNumbers, IEIDOResponseMessage response)
		{
			StringBuilder builder = new StringBuilder();
			Format(builder, response);

			EmailDef email = CreateProcessEmail(verb, shipmentNumber, containerNumbers);
			email.ContentType = EmailContentTypes.PlainText;
			email.Body = builder.ToString();
			return email;
		}

		#endregion

		#region Implementation

		static void Format(StringBuilder builder, IEIDOResponseMessage response)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			if (response == null)
			{
				throw new ArgumentNullException(nameof(response));
			}

			builder.Append(
				Res.GetString("f3bac4af-acf8-4b18-aa6f-2dd42c643f69", "Received a '{0}' response to E-IDO message '{1}' ({2:yyyy-MM-dd hh:mm})",
				response.ResponseType, response.DocumentReference, response.DocumentIssuedDate) + "\r\n");

			foreach (IEIDOResponseError error in response.Errors)
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, "\r\n{0}: {1}\r\n", error.Code, error.Description);

				if (error.ErrorRefType != EIDOResponseErrorRefType.None)
				{
					builder.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}\r\n", error.ErrorRefType, error.Reference);
				}
			}
		}

		static EIDOMessage FindOriginalMessage(BusinessObjectFactory factory, IEIDOResponseMessage response)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.EIDO);
			filter.AddToFilter(EDIMessageSchema.EM_MessageNum, response.DocumentReference);
			return factory.LoadTop1<EIDOMessage>(filter);
		}

		static AttachmentDef MessageAsAttachment(EDIMessage message, string filename)
		{
			string contentString = message.EM_MessageText;
			byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(contentString);
			return new AttachmentDef(filename, contentBytes);
		}

		#endregion

		#region Email Groups & Modes

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return AgencyRegistry.Instance.EIDOAcknowledgementEmailGroup.Value; }
		}
		protected override ZString AcknowledgementEmailMode
		{
			get { return AgencyRegistry.Instance.EIDOAcknowledgementEmailMode.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return AgencyRegistry.Instance.EIDOErrorEmailGroup.Value; }
		}
		protected override ZString ErrorEmailMode
		{
			get { return AgencyRegistry.Instance.EIDOErrorEmailMode.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { throw new NotSupportedException(); }
		}
		protected override ZString ImpedimentEmailMode
		{
			get { throw new NotSupportedException(); }
		}

		#endregion
	}
}
