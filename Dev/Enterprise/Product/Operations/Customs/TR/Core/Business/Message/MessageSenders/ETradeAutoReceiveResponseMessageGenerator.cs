using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeAutoReceiveResponseMessageGenerator : TRAutoReceiveResponseMessageGenerator<ETradeEDIMessage>
	{
		public ETradeAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage) : base(sender, queryGuid, originalMessage)
		{
		}

		public override ZString MessageType => GetMessageType(originalMessage.EM_MessageType);

		ZString GetMessageType(ZString originalMessageType)
		{
			ZString result = ZString.Empty;
			switch (originalMessageType)
			{
				case TRMessageTypes.Codes.TRE:
					result = TRMessageTypes.Codes.T1E;
					break;
				case TRMessageTypes.Codes.TRD:
					result = TRMessageTypes.Codes.T1D;
					break;
				case TRMessageTypes.Codes.TCD:
					result = TRMessageTypes.Codes.T2D;
					break;
				case TRMessageTypes.Codes.TRS:
					result = TRMessageTypes.Codes.T1S;
					break;
			}

			return result;
		}

		protected override ZString MessageText
		{
			get
			{
				var envelope = new Envelope();
				envelope.Header = new EnvelopeHeader();
				envelope.Body = new EnvelopeBody
				{
					ServisCevabiSorgulama = new CargoWise.Customs.TR.MessageDefinitions.TemETrade.ServisCevabiSorgulama
					{
						KayitNo = queryGuid,
					}
				};

				if (TRBPassword != null)
				{
					var servisCevabiSorgulama = envelope.Body.ServisCevabiSorgulama;
					servisCevabiSorgulama.KullaniciAdi = TRBPassword.GP_UserID;
					servisCevabiSorgulama.KullaniciSifre = TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
				}

				var ns = new XmlSerializerNamespaces();
				ns.Add((NoResString)"soapenv", @"http://schemas.xmlsoap.org/soap/envelope/");
				ns.Add("TemETrade", @"http://tempuri.org/");

				return XmlObjectSerializer.SerializeWithAdditionalNamespaces(envelope, ns);
			}
		}
	}
}
