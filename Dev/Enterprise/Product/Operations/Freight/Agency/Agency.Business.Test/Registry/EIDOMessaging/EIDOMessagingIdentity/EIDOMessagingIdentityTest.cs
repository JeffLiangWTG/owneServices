using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOMessagingIdentity))]
	internal class EIDOMessagingIdentityTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSerialisation()
		{
			EIDOMessagingIdentity identity1 = Messaging.Identities.AddNew();
			identity1.Password = "password1";
			identity1.SenderID = "SenderID1";
			identity1.RecipientID = "RecipientID1";
			string xml;
			using (StringWriter stream = new StringWriter())
			using (XmlWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Identity");
				((IXmlSerializable)identity1).WriteXml(writer);
				writer.WriteEndElement();
				xml = stream.ToString();
			}

			const string ExpectedXml = "<Identity>\n" + "<Principal>00000000-0000-0000-0000-000000000000</Principal>\n" + "<Password>password1</Password>\n" + "<SenderID>SenderID1</SenderID>\n" + "<RecipientID>RecipientID1</RecipientID>\n" + "</Identity>" + "";
			AssertMultilineASCIIEquals("", ExpectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));
			EIDOMessagingIdentity identity2 = new EIDOMessagingIdentity(new EIDOMessagingHeader());
			using (StringReader stream = new StringReader(xml))
			using (XmlReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)identity2).ReadXml(reader);
			}

			AssertEquals("Password", identity1.Password, identity2.Password);
			AssertEquals("SenderID", identity1.SenderID, identity2.SenderID);
			AssertEquals("RecipientID", identity1.RecipientID, identity2.RecipientID);
		}

		#region Implementation
		EIDOMessagingHeader Messaging
		{
			get
			{
				return messaging ?? (messaging = new EIDOMessagingHeader());
			}
		}

		EIDOMessagingHeader messaging;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EIDOMessagingIdentity(new EIDOMessagingHeader());
		}
		#endregion
	}
}
