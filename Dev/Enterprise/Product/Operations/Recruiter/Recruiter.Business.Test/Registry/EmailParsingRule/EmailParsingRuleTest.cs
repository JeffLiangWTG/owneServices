using System.IO;
using System.Xml;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(EmailParsingRule))]
	public class EmailParsingRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new EmailParsingRule();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		public void TestXMLSerialization()
		{
			var serializer = ZXmlSerializer.New(typeof(EmailParsingRule));
			var rule = new EmailParsingRule();
			rule.ReferringPartyCode = "OH";
			rule.AllowParseAttachments = true;
			rule.AllowFallbackToEmailBody = true;

			string xml;
			using (var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter))
			{
				serializer.Serialize(writer, rule);
				xml = stringWriter.ToString();
			}

			using (var stringReader = new StringReader(xml))
			using (var reader = new XmlTextReader(stringReader))
			{
				var rule2 = (EmailParsingRule)serializer.Deserialize(reader);
				AssertEquals(rule.ReferringPartyCode, rule2.ReferringPartyCode);
				AssertEquals(rule.AllowParseAttachments, rule2.AllowParseAttachments);
				AssertEquals(rule.AllowFallbackToEmailBody, rule2.AllowFallbackToEmailBody);
				AssertEquals("Organization", rule.ReferringPartyDescription);
				AssertEquals(rule.ReferringPartyDescription, rule2.ReferringPartyDescription);
			}
		}

		public void TestFields()
		{
			var rule = new EmailParsingRule();
			rule.AllowParseAttachments = true;
			rule.AllowFallbackToEmailBody = true;

			AssertEquals(true, rule.AllowParseAttachments);
			AssertEquals(true, rule.AllowFallbackToEmailBody);
			AssertEquals(false, rule.AllowFallbackToEmailBody_ReadOnly);

			rule.AllowParseAttachments = false;
			AssertEquals(false, rule.AllowParseAttachments);
			AssertEquals(false, rule.AllowFallbackToEmailBody);
			AssertEquals(true, rule.AllowFallbackToEmailBody_ReadOnly);
		}
	}
}
