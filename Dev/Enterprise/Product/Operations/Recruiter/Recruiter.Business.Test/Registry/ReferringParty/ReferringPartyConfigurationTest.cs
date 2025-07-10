using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ReferringPartyConfiguration))]
	public class ReferringPartyConfigurationTest : RegistryBusinessObjectTestCaseBase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new ReferringPartyConfiguration();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override bool IsCodeMandatory => false;

		public void TestValidations()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST_ORG1";
			Factory.Save();
			var cfg = new ReferringPartyConfiguration();
			cfg.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertHasError(cfg.DomainInfo, "Please enter an Email / Domain.");
				cfg.Domain = "@cw1.com";
				AssertNoErrors(cfg.DomainInfo);
				cfg.Domain = "###AAA";
				AssertHasError(cfg.DomainInfo, "Please enter a valid email address or a valid domain name. (e.g. username@example.com / @example.com)");
				cfg.Domain = "user@a.com";
				AssertNoErrors(cfg.DomainInfo);
				cfg.Domain = "user#a.com";
				AssertHasError(cfg.DomainInfo, "Please enter a valid email address or a valid domain name. (e.g. username@example.com / @example.com)");
				cfg.Domain = "@ccc.com";
				AssertNoErrors(cfg.DomainInfo);

				AssertHasError(cfg.ReferringPartyInfo, "Please enter a value.");
				cfg.ReferringParty = "@@";
				AssertHasError(cfg.ReferringPartyInfo, "Enter a valid selection.");
				cfg.ReferringParty = "OH";
				AssertNoErrors(cfg.ReferringPartyInfo);

				cfg.OrganizationPK = ZGuid.NewZGuid();
				AssertHasError(cfg.OrganizationPKInfo, "Enter a valid Organization.");
				cfg.OrganizationPK = org.PK;
				AssertNoErrors(cfg.OrganizationPKInfo);

				AssertHasError(cfg.DefaultReferringSourceInfo, "Please enter a value.");
				cfg.DefaultReferringSource = "@@";
				AssertHasError(cfg.DefaultReferringSourceInfo, "Enter a valid selection.");
				cfg.DefaultReferringSource = "WEB";
				AssertNoErrors(cfg.DefaultReferringSourceInfo);
			});
		}

		public void TestXMLSerialization()
		{
			var org = Factory.New<OrgHeader>();
			var serializer = ZXmlSerializer.New(typeof(ReferringPartyConfiguration));
			var cfg = new ReferringPartyConfiguration();
			cfg.Domain = "@cw1.com";
			cfg.ReferringParty = "OH";
			cfg.OrganizationPK = org.PK;
			cfg.DefaultReferringSource = "WEB";

			string xml;
			using (var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter))
			{
				serializer.Serialize(writer, cfg);
				xml = stringWriter.ToString();
			}

			using (var stringReader = new StringReader(xml))
			using (var reader = new XmlTextReader(stringReader))
			{
				var cfg2 = (ReferringPartyConfiguration)serializer.Deserialize(reader);
				AssertEquals(cfg.Domain, cfg2.Domain);
				AssertEquals(cfg.ReferringParty, cfg2.ReferringParty);
				AssertEquals(cfg.OrganizationPK, cfg2.OrganizationPK);
				AssertEquals(cfg.DefaultReferringSource, cfg2.DefaultReferringSource);
			}
		}
	}
}
