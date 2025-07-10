using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class InboundInterchangeProcessor : Enterprise.Messaging.Business.InboundInterchangeProcessor
	{
		protected InboundInterchangeProcessor()
		{
		}

		protected InboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
			Argument.NotNull(logger, "LoggingInformation logger");
		}

		protected override void HandleProcessingException(Exception ex, EDIInterchange interchange, ZGuid interchangePK)
		{
			if (ex is InvalidMessageFormatException)
			{
				SendErrorNotification(interchange, ex.Message);
			}

			base.HandleProcessingException(ex, interchange, interchangePK);
		}

		#region Message Creation Error Handling

		void SendErrorNotification(EDIInterchange interchange, string errorMessage)
		{
			var bodyTextStream = interchange.GetEI_BodyTextReader().GetPaddedMemoryStream();
			var email = new EmailDef();
			email.Body = "A message had the following error during processing.\r\n" + errorMessage;
			email.Subject = "Error processing message";
			email.Attachments.Add(AttachmentDef.CreateZippedAttachment("MessageData.zip", "MessageData.txt", bodyTextStream));
			Env.OutgoingCustomsMailManager.Create(interchange.Factory, email, Env.Registry.PostMasterGroup, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
		}

		#endregion
	}
}
