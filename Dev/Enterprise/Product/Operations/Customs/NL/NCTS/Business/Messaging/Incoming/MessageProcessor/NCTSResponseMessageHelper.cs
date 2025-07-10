using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public static class NCTSResponseMessageHelper
{
	public static TIncomingDataProvider GetCachedInboundProvider<TXmlObject, TIncomingDataProvider>(this EDIMessage message)
	where TIncomingDataProvider : class, INCTSIncomingDataProvider
	{
		return message != null && message.EM_ReceiveTransmit == EDIMessage.Direction.Receive
			? message.Factory.GetCachedValue($"{typeof(TIncomingDataProvider)}_{message.PK}", () =>
			{
				var stringReader = new StringReader(message.EM_MessageText);
				var xmlTextReader = new NamespaceIgnorantXmlTextReader(stringReader);
				var rootElementName = typeof(TXmlObject).GetCustomAttribute<XmlRootAttribute>().ElementName;
				var xmlObject = (TXmlObject)new XmlSerializer(typeof(TXmlObject), new XmlRootAttribute(rootElementName)).Deserialize(xmlTextReader);

				return xmlObject != null
					? (TIncomingDataProvider)Activator.CreateInstance(typeof(TIncomingDataProvider), xmlObject)
					: default;
			})
			: null;
	}

	class NamespaceIgnorantXmlTextReader : XmlTextReader
	{
		public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader) { }

		public override string NamespaceURI
		{
			get { return string.Empty; }
		}
	}

	public static string DiscardedMessageByLogEvent => Res.GetString("8F808A74-C7A0-4ADA-86D0-8143826CC470", "The message is discarded since there used guarantee is already cleared. An event CGU with reference to message CC045C was created.");

	public static ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => linkedObject is NctsDepartureMovementHeader header ? header.RegistryBranchPK : linkedObject is NctsHeader nctsHeader ? nctsHeader.RegistryBranchPK : ZGuid.Empty;
}
