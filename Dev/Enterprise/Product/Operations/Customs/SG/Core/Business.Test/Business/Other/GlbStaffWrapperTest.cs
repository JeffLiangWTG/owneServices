using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.SG;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGGlbStaffWrapper))]
	public class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<SGGlbStaffWrapper>
	{
		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = SGGlbStaffWrapper.Get(staff);
			ISGGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.AccessPassword, iWrapper.AccessPassword);
			AssertEquals(wrapper.SGNationalTradePlatformPassword, iWrapper.SGNationalTradePlatformPassword);
			AssertEquals(wrapper.Tradenetv4Password, iWrapper.Tradenetv4Password);
		}

		public void TestAccessPassword()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_SGA>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;
			Factory.Save();
			var wrapper1 = SGGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.AccessPassword.PK);
			var wrapper2 = SGGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.AccessPassword;
			AssertNotNull(password2);
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.SGA, password2.GP_PasswordType);
		}

		public void TestTradenetv4Password()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_SGv4>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;
			Factory.Save();
			var wrapper1 = SGGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.Tradenetv4Password.PK);
			var wrapper2 = SGGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.Tradenetv4Password;
			AssertNotNull(password2);
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.SG4, password2.GP_PasswordType);
		}

		public void TestSGNationalTradePlatformPassword()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_SGNTP>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;
			Factory.Save();
			var wrapper1 = SGGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.SGNationalTradePlatformPassword.PK);
			var wrapper2 = SGGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.SGNationalTradePlatformPassword;
			AssertNotNull(password2);
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.NTP, password2.GP_PasswordType);
		}

		protected override SGGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return SGGlbStaffWrapper.Get(staff);
		}

		public void TestCreateCredentialXml()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = CreateNewWrapper(glbStaff);
			var password1 = wrapper.SGNationalTradePlatformPassword;
			password1.GP_UserID = "1";
			password1.GP_MailBoxID = "CBK0124-0";
			password1.CurrentDecryptedPassword = "Cw1123456789";
			var password2 = wrapper.AccessPassword;
			password2.GP_UserID = "2";
			password2.GP_MailBoxID = "CBK0125-0";
			password2.CurrentDecryptedPassword = "Cw1123456789";
			var password3 = wrapper.Tradenetv4Password;
			password3.GP_UserID = "3";
			password3.GP_MailBoxID = "CBK0123-0";
			password3.CurrentDecryptedPassword = "Cw1123456789";
			Factory.Save();
			var interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
			AssertEquals(2, interchanges.Length);
			var interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsNTP\""));
			AssertConfigurationValues(interchange, "SGCustomsNTP");
			AssertContains("<UserName>1</UserName>", interchange.EI_BodyText);
			interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsConfiguration\""));
			AssertConfigurationValues(interchange, "SGCustomsConfiguration");
			AssertContains("<UserName>2</UserName>", interchange.EI_BodyText);
			interchanges.ForEach(x => x.Delete());
			Factory.Save();
			password1.Delete();
			Factory.Save();
			interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
			AssertEquals(1, interchanges.Length);
			interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsNTP\""));
			AssertConfigurationValues(interchange, "SGCustomsNTP");
			AssertNotContains("<UserName>1</UserName>", interchange.EI_BodyText);
			interchanges.ForEach(x => x.Delete());
			Factory.Save();
			password2.Delete();
			password3.Delete();
			Factory.Save();
			interchanges = GetLatestEHubConfigurationInterchanges(Factory, 3).ToArray();
			AssertEquals(1, interchanges.Length);
			interchange = interchanges.FirstOrDefault(x => x.EI_BodyText.Contains("Name=\"SGCustomsConfiguration\""));
			AssertConfigurationValues(interchange, "SGCustomsConfiguration");
			AssertNotContains("<UserName>2</UserName>", interchange.EI_BodyText);
		}

		static IEnumerable<EDIInterchange> GetLatestEHubConfigurationInterchanges(BusinessObjectFactory factory, int count)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(IEDIInterchange));
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.Configuration);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_To, MasterFiles.Business.Customs.XmlCredential.Constants.Configuration.EHubRecipient);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued);
			dbQuery.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.eHub);
			dbQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			return factory.Load<EDIInterchange>(dbQuery).Take(count);
		}

		void AssertConfigurationValues(IEDIInterchange interchange, ZString configurationName)
		{
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var configuration = reader.DeserializeToConfiguration();
				AssertEquals("Configuration name should be set", configurationName, configuration.Name);
				var systemGroup = configuration.Group[0];
				AssertEquals("System type should be set", "System", systemGroup.Type);
				var company = (Group)systemGroup.Items[0];
				AssertEquals("Company type should be set", "Company", company.Type);
				var group = (Group)company.Items[0];
				AssertEquals("Staff type should be set", "Staff", group.Type);
			}
		}
	}
}
