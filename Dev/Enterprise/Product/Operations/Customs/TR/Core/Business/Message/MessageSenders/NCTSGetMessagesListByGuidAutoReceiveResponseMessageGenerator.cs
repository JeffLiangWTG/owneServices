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
	public class NCTSGetMessagesListByGuidAutoReceiveResponseMessageGenerator : TRAutoReceiveResponseMessageGenerator<NCTSMessage>
	{
		public NCTSGetMessagesListByGuidAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage) : base(sender, queryGuid, originalMessage)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.T1N;

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
						GetMessagesListByGuid = new GetMessagesListByGuid()
						{
							FirmId = TRMessageConstants.FirmID,
							UserId = userID,
							CorrGuid = queryGuid
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
