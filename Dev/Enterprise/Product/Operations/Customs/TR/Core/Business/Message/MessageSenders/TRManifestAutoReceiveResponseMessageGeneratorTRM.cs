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
	public class TRManifestAutoReceiveResponseMessageGeneratorTRM : TRBaseMessageGenerator<TRManifestMessage>
	{
		public TRManifestAutoReceiveResponseMessageGeneratorTRM(IMessageSender sender, EDIMessage originalMessage, ZString customsOffice, ZString registrationNumber) : base(sender)
		{
			this.originalMessage = originalMessage;
			this.customsOffice = customsOffice;
			this.registrationNumber = registrationNumber;
		}
		readonly ZString customsOffice;
		readonly ZString registrationNumber;
		protected readonly EDIMessage originalMessage;

		public override ZString MessageType => TRMessageTypes.Codes.TRM;

		protected override ZString MessageText
		{
			get
			{
				var envelope = new Envelope();
				envelope.Header = new EnvelopeHeader();
				envelope.Body = new EnvelopeBody
				{
					OzbyMuayeneMemuruAdiSorgula = new OzbyMuayeneMemuruAdiSorgula()
				};

				if (TRBPassword != null)
				{
					var ozbyMuayeneMemuruAdiSorgula = envelope.Body.OzbyMuayeneMemuruAdiSorgula;
					ozbyMuayeneMemuruAdiSorgula.KullaniciAdi = TRBPassword.GP_UserID;
					ozbyMuayeneMemuruAdiSorgula.KullaniciSifre = TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
					ozbyMuayeneMemuruAdiSorgula.Gumruk = TRMessageHelper.RemoveCountryCodePrefix(customsOffice);
					ozbyMuayeneMemuruAdiSorgula.TescilNo = registrationNumber;
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

		protected override ZString ApplicationReference => Sender.JobReference + "|" + TRBPassword?.GP_UserID;
		protected override Enterprise.MasterFiles.Business.GlbStaff WhoSendThisMessage => originalMessage?.UserWhoQueuedThisRecord;
	}
}
