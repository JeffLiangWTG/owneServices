using System;
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
	public class TRManifestAutoReceiveResponseMessageGeneratorT2O : TRBaseMessageGenerator<TRManifestMessage>
	{
		public TRManifestAutoReceiveResponseMessageGeneratorT2O(IMessageSender sender, EDIMessage originalMessage, ZDateTime processStartDate) : base(sender)
		{
			this.originalMessage = originalMessage;
			this.processStartDate = processStartDate;
		}
		readonly ZDateTime processStartDate;
		protected readonly EDIMessage originalMessage;

		public override ZString MessageType => TRMessageTypes.Codes.T2O;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		protected override ZString MessageText
		{
			get
			{
				var envelope = new Envelope();
				envelope.Header = new EnvelopeHeader();
				envelope.Body = new EnvelopeBody();

				envelope.Body = new EnvelopeBody
				{
					IslemSorgula3 = new IslemSorgula3()
				};

				if (TRBPassword != null)
				{
					var islemSorgula3 = envelope.Body.IslemSorgula3;
					islemSorgula3.RefId = ApplicationReference;
					islemSorgula3.KullaniciAdi = TRBPassword.GP_UserID;
					islemSorgula3.KullaniciSifre = TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
					islemSorgula3.BasIslemGunu = DateTime.Parse(processStartDate.IsValid ? processStartDate.ToISO8601ShortDateString() : ZDateTime.Now.ToISO8601ShortDateString());
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

		protected override ZString ApplicationReference => Sender.JobReference + "|" + TRBPassword.GP_UserID;
		protected override Enterprise.MasterFiles.Business.GlbStaff WhoSendThisMessage => originalMessage.UserWhoQueuedThisRecord;
	}
}
