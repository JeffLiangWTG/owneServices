using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Customs.TR.MessageDefinitions.Tem;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class DeclarationAutoReceiveResponseMessageGeneratorDT3 : TRBaseMessageGenerator<TRImportExportMessage>
	{
		public DeclarationAutoReceiveResponseMessageGeneratorDT3(IMessageSender sender, ZString guID, EDIMessage originalMessage) : base(sender)
		{
			this.originalMessage = originalMessage;
			this.guID = guID;
		}
		readonly ZString guID;
		protected readonly EDIMessage originalMessage;

		public override ZString MessageType => TRMessageTypes.Codes.DT3;

		protected override ZString MessageText
		{
			get
			{
				var envelope = new Envelope();
				envelope.Header = new EnvelopeHeader();
				envelope.Body = new EnvelopeBody();

				envelope.Body = new EnvelopeBody
				{
					IslemSonucGetir4 = new IslemSonucGetir4()
				};

				if (TRBPassword != null)
				{
					var islemSonucGetir4 = envelope.Body.IslemSonucGetir4;
					islemSonucGetir4.KullaniciAdi = TRBPassword.GP_UserID;
					islemSonucGetir4.KullaniciSifre = TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
					islemSonucGetir4.GuiDof = guID;
				}

				var settings = new XmlWriterSettings
				{
					Indent = true,
					Encoding = Encoding.UTF8,
					OmitXmlDeclaration = true
				};

				var ns = new XmlSerializerNamespaces();
				ns.Add((NoResString)"soapenv", @"http://schemas.xmlsoap.org/soap/envelope/");
				ns.Add((NoResString)"tem", @"http://tempuri.org/");

				return TRMessageHelper.SerializeSOAPMessage(envelope, settings, ns);
			}
		}

		protected override ZString ApplicationReference => guID;
		protected override Enterprise.MasterFiles.Business.GlbStaff WhoSendThisMessage => originalMessage.UserWhoQueuedThisRecord;
	}
}
