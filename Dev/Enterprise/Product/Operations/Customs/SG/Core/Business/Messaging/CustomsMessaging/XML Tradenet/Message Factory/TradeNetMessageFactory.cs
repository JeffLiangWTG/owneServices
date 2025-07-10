using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public abstract class TradeNetMessageFactory<T> : IMessageFactory where T : ITradeNetSectionParent
	{
		public abstract T CreateTradeNetMessageParent();

		public string GetMessageContent(T messageParent) => XMLHelper.MakeDataAlwaysInUppercase(SerializeCore(messageParent));

		#region Implement

		protected const string MessageVersion = "041";

		protected const string DateTimeFormat = "yyyyMMddHHmm";

		protected string SerializeCore(T obj)
		{
			using (var ms = new MemoryStream())
			using (var writer = XmlWriter.Create(ms, writerSettings))
			{
				var qualifiedNames = GetXmlQualifiedNames().ToArray();

				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(writer, obj, new XmlSerializerNamespaces(qualifiedNames));

				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		protected virtual IEnumerable<XmlQualifiedName> GetXmlQualifiedNames()
		{
			yield return new XmlQualifiedName("coo", "urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin");
			yield return new XmlQualifiedName("cac", "urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2");
			yield return new XmlQualifiedName("cbc", "urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2");
			yield return new XmlQualifiedName("inp", "urn:crimsonlogic:tn:schema:xsd:InNonPayment");
			yield return new XmlQualifiedName("ipt", "urn:crimsonlogic:tn:schema:xsd:InPayment");
			yield return new XmlQualifiedName("out", "urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration");
			yield return new XmlQualifiedName("tnp", "urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement");
		}

		readonly XmlWriterSettings writerSettings = new XmlWriterSettings() { Indent = true, Encoding = new UTF8Encoding(false), NewLineChars = System.Environment.NewLine };

		#endregion

		#region IMessageFactory

		public MessageType MessageType => MessageType.XML;

		ICusMessage IMessageFactory.GetOriginalMessage(ICustomsDec cusEntryHeader) => GetOriginalMessageCore(cusEntryHeader);

		ICusMessage IMessageFactory.GetAmendmentMessage(ICustomsDec cusEntryHeader) => GetAmendmentMessageCore(cusEntryHeader);

		ICusMessage IMessageFactory.GetRefundMessage(ICustomsDec cusEntryHeader) => GetRefundMessageCore(cusEntryHeader);

		ICusMessage IMessageFactory.GetCancellationMessage(ICustomsDec cusEntryHeader) => GetCancellationMessageCore(cusEntryHeader);

		protected abstract ICusMessage GetOriginalMessageCore(ICustomsDec cusEntryHeader);

		protected abstract ICusMessage GetAmendmentMessageCore(ICustomsDec cusEntryHeader);

		protected abstract ICusMessage GetRefundMessageCore(ICustomsDec cusEntryHeader);

		protected abstract ICusMessage GetCancellationMessageCore(ICustomsDec cusEntryHeader);

		#endregion
	}
}
