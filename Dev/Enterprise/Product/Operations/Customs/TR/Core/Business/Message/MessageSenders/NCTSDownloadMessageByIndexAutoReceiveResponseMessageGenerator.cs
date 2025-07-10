using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Customs.TR.MessageDefinitions.Ws;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSDownloadMessageByIndexAutoReceiveResponseMessageGenerator : TRAutoReceiveResponseMessageGenerator<NCTSMessage>
	{
		public NCTSDownloadMessageByIndexAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage) : base(sender, queryGuid, originalMessage)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.T2N;

		protected override ZString MessageText
		{
			get
			{
				var userID = Sender.JobReference + "," + TRBPassword.GP_UserID + "," + TRManifestMessageBuilderHelper.MD5Hash(TRBPassword.CurrentDecryptedPassword);
				var envelope = new Envelope()
				{
					Header = new EnvelopeHeader(),
					Body = new EnvelopeBody()
					{
						Downloadmessagebyindex = new Downloadmessagebyindex()
						{
							FirmId = TRMessageConstants.FirmID,
							UserId = userID,
							Index = queryGuid
						}
					}
				};

				var ns = new XmlSerializerNamespaces();
				ns.Add((NoResString)"soapenv", @"http://schemas.xmlsoap.org/soap/envelope/");
				ns.Add((NoResString)"ws", @"http://ws/");
				var setting = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = true
				};
				return XmlObjectSerializer.SerializeWithNamespaces(envelope, Encoding.UTF8, setting, ns);
			}
		}
	}
}
