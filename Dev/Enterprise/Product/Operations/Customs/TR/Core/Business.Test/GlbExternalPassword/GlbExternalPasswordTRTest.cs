using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Testing.Helpers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_TR))]
	public class GlbExternalPasswordTRTest : Enterprise.MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbExternalPassword_TR>
	{
		public void TestTRChipset()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			externalPassword.GP_UserID = "";
			externalPassword.CurrentDecryptedPassword = "1";
			externalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EGUVEN;
			externalPassword.TR_Chipset = ChipsetList.Descriptions.GEMPLUS;
			externalPassword.GP_CertificateSerialNumber = "1";
			Factory.Save();

			var reLoadExternalPassword = Factory.Load<GlbExternalPassword_TR>(externalPassword.PK);
			AssertEquals(ChipsetList.Descriptions.GEMPLUS, reLoadExternalPassword.TR_Chipset);
		}

		public void TestLookupsAndValidationType()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			Assert(externalPassword.Lookups.GetType().FullName.Contains("GlbExternalPasswordLookups_TR"));
			Assert(externalPassword.Validation.GetType().FullName.Contains("GlbExternalPasswordValidation_TR"));
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.TRK, GlbExternalPassword.GP_PasswordType);
		}

		public void TestCurrentDecryptedPasswordMaxLength()
		{
			AssertEquals(15, GlbExternalPassword.CurrentDecryptedPasswordInfo.MaxLength);
		}

		public void TestSetNewBrokerPassword()
		{
			GlbExternalPassword.GP_GS = Staff.PK;
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.GP_UserID = "MMM";

			Factory.Save();

			TwoWayEncoder encoder = new TwoWayEncoder(Staff.PK.ToGuid());
			AssertEquals("MMM", GlbExternalPassword.GP_UserID);
			AssertEquals("Broker password should be encrypted", encoder.Encrypt("TEST"), GlbExternalPassword.GP_CurrentPassword);
		}
		public void TestPasswordTypesList()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			var list1 = externalPassword.Lookups.PasswordTypeList;
			AssertEquals(PasswordTypesList.Descriptions.TRK, list1.GetDescriptionFromCode("TRK"));
		}

		public override void TestOnlySendCredentialIfNeeded()
		{
			var credential = CreateNewGlbExternalPassword(Factory);
			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());
			credential.Delete();
			Factory.Save();
			AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());

			credential = CreateNewGlbExternalPassword(Factory);
			credential.GP_UserID = "USERID";
			credential.CurrentDecryptedPassword = "PASSWORD";
			Factory.Save();
			var interchange1 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotNull("Credential Send", interchange1);
			credential.Delete();
			Factory.Save();
			var interchange2 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotEquals("Should send credential on delete", interchange1, interchange2);
			interchange1.Delete();
			interchange2.Delete();
			Factory.Save();

			foreach (var data in new[]
			{
				new Tuple<string, IZType>(Enterprise.MasterFiles.Business.GlbExternalPassword.Schema.GP_UserID, (ZString)"USERID"),
				new Tuple<string, IZType>(Enterprise.MasterFiles.Business.GlbExternalPassword.Schema.CurrentDecryptedPassword, (ZString)"PASSWORD")
			})
			{
				credential = CreateNewGlbExternalPassword(Factory);
				credential.GP_UserID = ZString.Empty;
				credential.CurrentDecryptedPassword = ZString.Empty;
				Factory.Save();
				AssertNull("No credential Send", Factory.GetLatestEHubConfigurationInterchange());

				var info = credential.ZPropertyInfoHash.GetPropertySafe(data.Item1);
				info.Value = data.Item2;
				Factory.Save();
				interchange1 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotNull("Credential Send", interchange1);

				info.Value = info.Value.Default;
				System.Threading.Thread.Sleep(1);
				Factory.Save();
				interchange2 = Factory.GetLatestEHubConfigurationInterchange();
				AssertNotEquals("Credential Send", interchange1, interchange2);

				info.Value = data.Item2;
				info.Value = info.Value.Default;
				System.Threading.Thread.Sleep(1);
				Factory.Save();
				AssertEquals("No new send as data hasn't changed seen saving", interchange2, Factory.GetLatestEHubConfigurationInterchange());

				credential.Delete();
				Factory.Save();
				AssertEquals("No new send as data hasn't changed seen saving", interchange2, Factory.GetLatestEHubConfigurationInterchange());
				interchange1.Delete();
				interchange2.Delete();
				Factory.Save();
			}
		}
		public void TestProcessForInterchangeType()
		{
			GlbExternalPassword.GP_UserID = "USERID";
			GlbExternalPassword.CurrentDecryptedPassword = "PASSWORD";
			GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorities.Codes.EGUVEN;
			GlbExternalPassword.TR_Chipset = ChipsetList.Codes.AKIS;
			GlbExternalPassword.GP_CertificateSerialNumber = "1234567890";

			Factory.Save();

			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			var expectedInterchangeBody = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TRCustomsSubscribers"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ZAC"">
        <Group Type=""TRK"" Status=""VAL"">
          <Credential Name=""Current"">
            <UserName>USERID</UserName>
            <Password>";
			AssertContains(XmlHelper.IgnoreXmlnsAttrOrder(expectedInterchangeBody), interchange.EI_BodyText);

			GlbExternalPassword.GP_UserID = ZString.Empty;
			GlbExternalPassword.CurrentDecryptedPassword = "";
			GlbExternalPassword.GP_CertificateAuthority = ZString.Empty;
			GlbExternalPassword.TR_Chipset = ZString.Empty;
			GlbExternalPassword.GP_CertificateSerialNumber = ZString.Empty;
			Factory.Save();

			interchange = Factory.GetLatestEHubConfigurationInterchange();
			expectedInterchangeBody = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TRCustomsSubscribers"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""EDI"">
      <Group Type=""Staff"" Reference=""ZAC"">
        <Group Type=""TRK"" />
      </Group>
    </Group>
  </Group>
</Configuration>";
			AssertEquals(XmlHelper.IgnoreXmlnsAttrOrder(expectedInterchangeBody), interchange.EI_BodyText);
		}

		public void TestPINCode()
		{
			var externalPassword = Factory.New<GlbExternalPassword_TR>();
			externalPassword.PINCode = "123456";
			Factory.Save();
			var reLoadExternalPassword = Factory.Load<GlbExternalPassword_TR>(externalPassword.PK);
			AssertEquals("123456", reLoadExternalPassword.PINCode);
		}

		public void TestGetMessageAttrDictionary()
		{
			GlbExternalPassword.GP_UserID = "UserId";
			GlbExternalPassword.GP_CurrentPassword = "Password";

			var expected = new Dictionary<string, string>
			{
				{ "httpclient.user", "UserId" },
				{ "httpclient.password", "Password" },
			};

			AssertContainsExactElementsInAnyOrder(expected, GlbExternalPassword.GetMessageAttrDictionary());
		}
	}
}
