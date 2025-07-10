using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyConfigurationToSender))]
	sealed class GlbCompanyConfigurationToSenderTest : TestCaseWithFactory
	{
		public void TestSendingRegistration() => CombineAssertions(() =>
		{
			GlbCompanyConfigurationToSender.RegisterForSending(Factory, null, null, null);
			var senders = GlbCompanyConfigurationToSenderForTest.SendersExposed;
			AssertEquals(0, senders.Count);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "A", null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(1, senders[Factory].ConfigurationsToBeSend.Count);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "A", null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(2, senders[Factory].ConfigurationsToBeSend.Count);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "B", null, null);
			AssertEquals(1, senders.Count);
			AssertEquals(3, senders[Factory].ConfigurationsToBeSend.Count);

			var configurationsToBeSend = senders[Factory].ConfigurationsToBeSend;
			Assert(configurationsToBeSend.Where(x => x.ConfigurationName == "B").Count() == 1);
			Assert(configurationsToBeSend.Where(x => x.ConfigurationName == "A").Count() == 2);
			Factory.Save();
			senders = GlbCompanyConfigurationToSenderForTest.SendersExposed;
			AssertEquals(0, senders.Count);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "A", null, null);

			var newFactory = new BusinessObjectFactory();
			GlbCompanyConfigurationToSender.RegisterForSending(newFactory, "A", null, null);

			senders = GlbCompanyConfigurationToSenderForTest.SendersExposed;
			AssertEquals(2, senders.Count);

			Factory.Save();

			newFactory.Save();
			senders = GlbCompanyConfigurationToSenderForTest.SendersExposed;
			AssertEquals(0, senders.Count);
		});

		public void TestRegisterForSending() => CombineAssertions(() =>
		{
			const string configurationName = "BB";
			const string companyCode = "CMP";
			var company = CreateCompany(companyCode, "321");
			company.CredentialDataCreator = () => new GlbCompanyCredentialData("Id", "CHC", new[] { company.GC_CustomsRegistrationNoInfo }, CreateCredential);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, ZString.Empty, company);
			AssertEquals(0, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			Factory.Save();
			AssertEquals(0, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			var interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNull(interchange);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, configurationName, null);
			AssertEquals(1, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			AssertEquals(1, GlbCompanyConfigurationToSenderForTest.SendersExposed[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNull(interchange);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, configurationName, company);
			AssertEquals(1, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			AssertEquals(1, GlbCompanyConfigurationToSenderForTest.SendersExposed[Factory].ConfigurationsToBeSend.Count);
			Factory.Save();
			AssertEquals(0, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, configurationName, company, Status);
			ClearInterchange(interchange);
		});

		public void TestRegisterForSendingOrder() => CombineAssertions(() =>
		{
			const string companyCode = "CMP";
			var company = CreateCompany(companyCode, "321");
			company.CredentialDataCreator = () => new GlbCompanyCredentialData("TEST1", "CHC", new[] { company.GC_CustomsRegistrationNoInfo }, CreateCredential);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST1", company);
			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST2", company);
			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST3", company);
			AssertEquals(1, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);
			AssertEquals(3, GlbCompanyConfigurationToSenderForTest.SendersExposed[Factory].ConfigurationsToBeSend.Count);
			var configurationsToBeSend = GlbCompanyConfigurationToSenderForTest.SendersExposed[Factory].ConfigurationsToBeSend;
			Assert(configurationsToBeSend.Where(x => x.ConfigurationName == "TEST1" && x.CompanyCode == companyCode).Count() == 1);
			Assert(configurationsToBeSend.Where(x => x.ConfigurationName == "TEST2" && x.CompanyCode == companyCode).Count() == 1);
			Assert(configurationsToBeSend.Where(x => x.ConfigurationName == "TEST3" && x.CompanyCode == companyCode).Count() == 1);

			Factory.Save();
			AssertEquals(0, GlbCompanyConfigurationToSenderForTest.SendersExposed.Count);

			var interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST3", company, Status);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST2", company, Status);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", company, Status);

			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST3", company);
			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST2", company);
			GlbCompanyConfigurationToSender.RegisterForSending(Factory, "TEST1", company);
			Factory.Save();

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST1", company, Status);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST2", company, Status);

			ClearInterchange(interchange);

			interchange = Factory.GetLatestDxTConfigurationInterchange();
			AssertNotNull(interchange);
			AssertConfiguration(interchange, "TEST3", company, Status);

			ClearInterchange(interchange);
		});

		GlbCompany CreateCompany(string code, string customsRegistrationNo = "")
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = code;
			company.GC_CustomsRegistrationNo = customsRegistrationNo;
			return company;
		}

		void ClearInterchange(IEDIInterchange interchange)
		{
			interchange.Delete();
			Factory.Save();
		}

		void AssertConfiguration(IEDIInterchange interchange, ZString configurationName, GlbCompany company, ZString status)
		{
			using var reader = interchange.GetEI_BodyTextReader();
			var configuration = reader.DeserializeToConfiguration();
			AssertEquals("Configuration Name", configurationName, configuration.Name);
			AssertEquals("Configuration Group.Count", 1, configuration.Group.Count);

			var systemGroup = configuration.Group[0];
			AssertEquals("System Type", "System", systemGroup.Type);
			AssertEquals("System Reference", "EDIDAT", systemGroup.Reference);
			AssertEquals("System Items.Length", 1, systemGroup.Items.Length);

			var companyGroup = systemGroup.Items[0] as Group;
			AssertEquals("Company type", "Company", companyGroup.Type);
			AssertEquals("Company reference", company.GC_Code, companyGroup.Reference);
			AssertEquals("Company Status", status, companyGroup.Status);
			AssertEquals("Company Items.Length", 1, companyGroup.Items.Length);

			var credential = companyGroup.Items[0] as Credential;
			AssertEquals("Credential Username", company.GC_CustomsRegistrationNo, credential.UserName);
		}

		object CreateCredential(GlbCompany company)
		{
			return new Group()
			{
				Type = Constants.GroupTypes.CompanyType,
				Reference = company.GC_Code,
				Status = Status,
				Items = new object[] { CredentialSender.CreateCredential(string.Empty, company.GC_CustomsRegistrationNo, string.Empty) }
			};
		}

		const string Status = "VAL";

		class GlbCompanyConfigurationToSenderForTest : GlbCompanyConfigurationToSender
		{
			protected GlbCompanyConfigurationToSenderForTest(ZString interchangeType) : base(interchangeType)
			{
			}

			internal static Dictionary<BusinessObjectFactory, GlbCompanyConfigurationToSender> SendersExposed => Senders;
		}
	}
}
