using System;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class EmailAttachmentDelivery : Delivery
	{
		public override IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream, Func<Messaging.Integration.IEDIMessage> getMessageFunc = null)
		{
			stream.PopulateStream(context.Factory);

			var factory = context.Factory;
			var email = new EmailDef();

			var attachment = new AttachmentDef(mode.EK_Filename, stream.Content.ToByteArray());
			email.Attachments.Add(attachment);
			email.Subject = mode.EK_ServerAddressSubject;
			email.AddRecipientForUserCommunication(mode.EK_Destination);

			Env.OutgoingMailManager.Create(factory, email);

			return DeliveryResult.Success;
		}
	}
}
