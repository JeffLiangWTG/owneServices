using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.TR.MessageContracts.MessageBuilders.Declaration;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryHeaderControlMessageGenerator : TRBaseMessageGenerator<TRImportExportMessage>
	{
		public CusEntryHeaderControlMessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.DKO;

		string fMessageText;

		protected override ZString MessageText
		{
			get
			{
				if (fMessageText == null)
				{
					var messageProvider = new CusEntryHeaderMessageProvider((CusEntryHeader)Sender.Parent, MessageType);
					var messageBuilder = new DeclarationMessageBuilder(messageProvider);
					var xmlMessage = messageBuilder.GetXMLMessage(TRBPassword.GP_UserID, TRBPassword.CurrentDecryptedPassword, ApplicationReference);
					xmlMessage = xmlMessage.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", ZString.Empty);
					xmlMessage = xmlMessage.Replace("<Gelen>", "\r\n  <log:Gelen>").Replace("</Gelen>", "  </log:Gelen>\r\n");
					xmlMessage = xmlMessage.Replace("<Sorular_cevaplar />\r\n    ", ZString.Empty);
					xmlMessage = xmlMessage.Replace("    <", "        <");
					xmlMessage = xmlMessage.Replace("  <", "      <");

					var settings = new XmlWriterSettings
					{
						Indent = true,
						Encoding = Encoding.UTF8,
						OmitXmlDeclaration = true
					};

					var envelope = new Envelope()
					{
						Header = new EnvelopeHeader(),
						Body = new EnvelopeBody()
						{
							Kontrol = "CDATA"
						}
					};

					var ns = new XmlSerializerNamespaces();
					ns.Add((NoResString)"soapenv", @"http://schemas.xmlsoap.org/soap/envelope/");
					ns.Add((NoResString)"gum", @"http://Gumruk.BizTalk.Integration");
					ns.Add((NoResString)"log", @"http://LoginKontrol.KontrolGelen");
					ns.Add((NoResString)"tem", @"http://tempuri.org/");
					var soapMessage = TRMessageHelper.SerializeSOAPMessage(envelope, settings, ns);
					fMessageText = soapMessage.Replace("CDATA", xmlMessage);
				}
				return fMessageText;
			}
		}

		protected override ZString ApplicationReference => Sender.JobReference;
	}
}
