using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOMessagingHeader))]
	internal class EIDOMessagingHeaderTest : RegistryBusinessObjectTemplateTestCase<EIDOMessagingHeader>
	{
		public void TestValidateEmail()
		{
			Messaging.Email = "";
			Messaging.ValidateEmail();
			AssertNoNotifications(Messaging.EmailInfo);
			messaging.Identities.AddNew();
			Messaging.ValidateEmail();
			AssertHasError(Messaging.EmailInfo, "Please enter an Email Address.");
			Messaging.Email = "bob@freadnet.org";
			AssertNoErrors(Messaging.EmailInfo);
			Messaging.Email = "bob";
			AssertHasError(Messaging.EmailInfo, "Please enter a valid email address.");
		}

		public void TestSerialisation()
		{
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(EIDOMessagingHeader));
			Messaging.Testing = true;
			Messaging.Email = "bob@freadnet.org";
			EIDOMessagingIdentity identity1A = Messaging.Identities.AddNew();
			identity1A.Password = "password1";
			identity1A.SenderID = "SenderID1";
			identity1A.RecipientID = "RecipientID1";
			EIDOMessagingIdentity identity1B = Messaging.Identities.AddNew();
			identity1B.Password = "password2";
			identity1B.SenderID = "SenderID2";
			identity1B.RecipientID = "RecipientID2";
			string xml;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, Messaging);
				xml = stream.ToString();
			}

			const string ExpectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" + "<EIDOMessagingHeader>\n" + "  <Testing>Y</Testing>\n" + "  <Email>bob@freadnet.org</Email>\n" + "  <Identities>\n" + "    <Identity>\n" + "      <Principal>00000000-0000-0000-0000-000000000000</Principal>\n" + "      <Password>password1</Password>\n" + "      <SenderID>SenderID1</SenderID>\n" + "      <RecipientID>RecipientID1</RecipientID>\n" + "    </Identity>\n" + "    <Identity>\n" + "      <Principal>00000000-0000-0000-0000-000000000000</Principal>\n" + "      <Password>password2</Password>\n" + "      <SenderID>SenderID2</SenderID>\n" + "      <RecipientID>RecipientID2</RecipientID>\n" + "    </Identity>\n" + "  </Identities>\n" + "</EIDOMessagingHeader>\n" + "";
			AssertMultilineASCIIEquals("", ExpectedXml, xml);
			EIDOMessagingHeader messaging2;
			using (StringReader stream = new StringReader(xml))
			{
				messaging2 = (EIDOMessagingHeader)serialiser.Deserialize(stream);
			}

			AssertEquals("Testing", Messaging.Testing, messaging2.Testing);
			AssertEquals("Email", Messaging.Email, messaging2.Email);
			AssertEquals("Identities", Messaging.Identities.Count, messaging2.Identities.Count);
			EIDOMessagingIdentity identity2A = Messaging.Identities[0];
			AssertEquals("Password", identity1A.Password, identity2A.Password);
			AssertEquals("SenderID", identity1A.SenderID, identity2A.SenderID);
			AssertEquals("RecipientID", identity1A.RecipientID, identity2A.RecipientID);
			EIDOMessagingIdentity identity2B = Messaging.Identities[1];
			AssertEquals("Password", identity1B.Password, identity2B.Password);
			AssertEquals("SenderID", identity1B.SenderID, identity2B.SenderID);
			AssertEquals("RecipientID", identity1B.RecipientID, identity2B.RecipientID);
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override EIDOMessagingHeader GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override EIDOMessagingHeader GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(EIDOMessagingHeader originalBusinessObject, EIDOMessagingHeader newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			Converter<EIDOMessagingIdentity, string> converter = (EIDOMessagingIdentity i) =>
			{
				return string.Format("{0}|{1}|{2}|{3}", i.PrincipalPK, i.Password, i.SenderID, i.RecipientID);
			};
			AssertContainsExactElementsInAnyOrder("Identities", Array.ConvertAll(originalBusinessObject.Identities.ToArray<EIDOMessagingIdentity>(), converter), Array.ConvertAll(newBusinessObject.Identities.ToArray<EIDOMessagingIdentity>(), converter));
		}

		EIDOMessagingHeader GetNewPopulatedBusinessObject()
		{
			EIDOMessagingHeader messaging = new EIDOMessagingHeader();
			messaging.Email = "bob@freadnet.org";
			EIDOMessagingIdentity identity = messaging.Identities.AddNew();
			identity.SenderID = "sender";
			identity.RecipientID = "recipient";
			identity.Password = "password";
			return messaging;
		}

		EIDOMessagingHeader Messaging
		{
			get
			{
				return messaging ?? (messaging = new EIDOMessagingHeader());
			}
		}

		EIDOMessagingHeader messaging;
		#endregion
	}
}
