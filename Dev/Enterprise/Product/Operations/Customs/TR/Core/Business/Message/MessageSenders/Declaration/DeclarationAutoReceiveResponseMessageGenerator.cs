using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Customs.TR.MessageDefinitions.Tem;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class DeclarationAutoReceiveResponseMessageGenerator : TRAutoReceiveResponseMessageGenerator<TRImportExportMessage>
	{
		public DeclarationAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage) : base(sender, queryGuid, originalMessage)
		{
		}

		public DeclarationAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage, ZString msgType) : base(sender, queryGuid, originalMessage)
		{
			messageType = msgType;
		}

		readonly ZString messageType;

		public override ZString MessageType => messageType == TRMessageTypes.Codes.DTE ? TRMessageTypes.Codes.DT1 : TRMessageTypes.Codes.DK1;

		protected override ZString MessageText => GetMessageText();

		ZString GetMessageText()
		{
			if (messageTextCache.IsEmpty)
			{
				var envelope = new Envelope();
				envelope.Header = new EnvelopeHeader();
				envelope.Body = new EnvelopeBody
				{
					IslemSonucGetir2 = new IslemSonucGetir2
					{
						GuiDof = queryGuid,
					}
				};

				if (TRBPassword != null)
				{
					var islemSonucGetir2 = envelope.Body.IslemSonucGetir2;
					islemSonucGetir2.KullaniciAdi = TRBPassword.GP_UserID;
					islemSonucGetir2.KullaniciSifre = TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
				}

				var ns = new XmlSerializerNamespaces();
				ns.Add((NoResString)"soapenv", @"http://schemas.xmlsoap.org/soap/envelope/");
				ns.Add((NoResString)"tem", @"http://tempuri.org/");

				messageTextCache = XmlObjectSerializer.SerializeWithAdditionalNamespaces(envelope, ns);
			}

			return messageTextCache;
		}

		ZString messageTextCache;

		protected override Enterprise.MasterFiles.Business.GlbStaff WhoSendThisMessage => originalMessage.UserWhoQueuedThisRecord;
	}
}
