using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWCustomsSubscribersXmlCredentialConfigurationHandlerTest : Customs.Business.XmlCredential.Testing.ConfigurationHandlerTestCase
	{
		IXmlCredentialConfigurationHandler GetXmlCredentialConfigurationHandler(string configurationName, LoggingInformation logger)
		{
			IXmlCredentialConfigurationHandler result = null;
			var types = (Hashtable)ObjectFactory.Get("XmlCredentialConfigurationHandlers");
			var objectHandle = (ObjectHandle)types[configurationName];
			if (objectHandle != null)
			{
				result = (IXmlCredentialConfigurationHandler)objectHandle.GetObject(logger);
			}

			return result;
		}

		[ExpectNoExceptions]
		public void TestXmlCredentialConfigurationHandlerType()
		{
			NUnit.Framework.Assert.That(GetXmlCredentialConfigurationHandler("TWCustomsSubscribers", new LoggingInformation()), NUnit.Framework.Is.TypeOf<TWCustomsSubscribersXmlCredentialConfigurationHandler>());
		}

		[ExpectNoExceptions]
		public void TestProcess()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "TWC";
			glbStaff.GS_EmailAddress = "fish@where.com";
			var wrapper = TWGlbStaffWrapper.Get(glbStaff);
			var tvaPassword = wrapper.TWPasswordCollection.AddNew();
			tvaPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
			tvaPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			tvaPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			tvaPassword.GP_MailBoxID = "CBK0124-0";
			tvaPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			tvaPassword.GP_GS = glbStaff.PK;
			tvaPassword.GP_UserID = "2";
			var uvcPassword = wrapper.TWPasswordCollection.AddNew();
			uvcPassword.GP_PasswordType = PasswordTypesList.Codes.UVC;
			uvcPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			uvcPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			uvcPassword.GP_MailBoxID = "CBK0124-1";
			uvcPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			uvcPassword.GP_GS = glbStaff.PK;
			uvcPassword.GP_UserID = "3";
			Factory.Save();
			var outgoingInterchange = GlbExternalPasswordTestHelper.GetLatestEHubConfigurationInterchanges(Factory, 1, EDIInterchangeTypeList.Codes.Configuration).FirstOrDefault();
			Configuration configuration;
			using (var reader = outgoingInterchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			NUnit.Framework.Assert.That(configuration.Name, NUnit.Framework.Is.EqualTo("TWCustomsSubscribers").Using(CustomComparers.TypeComparison), "configuration.Name");
			var systemGroup = configuration.Group[0];
			NUnit.Framework.Assert.That(systemGroup.Type, NUnit.Framework.Is.EqualTo("System").Using(CustomComparers.TypeComparison), "systemGroup.Type");
			NUnit.Framework.Assert.That(systemGroup.Reference, NUnit.Framework.Is.EqualTo("EDIDAT").Using(CustomComparers.TypeComparison), "systemGroup.Reference");
			var systemGroupCode = systemGroup.Reference;
			var companyGroup = (Group)systemGroup.Items[0];
			NUnit.Framework.Assert.That(companyGroup.Type, NUnit.Framework.Is.EqualTo("Company").Using(CustomComparers.TypeComparison), "companyGroup.Type");
			NUnit.Framework.Assert.That(companyGroup.Reference, NUnit.Framework.Is.EqualTo("EDI").Using(CustomComparers.TypeComparison), "companyGroup.Reference");
			var companyGroupCode = companyGroup.Reference;
			var staffGroup = (Group)companyGroup.Items[0];
			NUnit.Framework.Assert.That(staffGroup.Type, NUnit.Framework.Is.EqualTo("Staff").Using(CustomComparers.TypeComparison), "staffGroup.Type");
			NUnit.Framework.Assert.That(staffGroup.Reference, NUnit.Framework.Is.EqualTo("TWC").Using(CustomComparers.TypeComparison), "staffGroup.Reference");
			var passwordGroup = (Group)staffGroup.Items[0];
			NUnit.Framework.Assert.That(passwordGroup.Type, NUnit.Framework.Is.EqualTo("MailBoxID").Using(CustomComparers.TypeComparison), "passwordGroup.Type");
			NUnit.Framework.Assert.That(passwordGroup.Reference, NUnit.Framework.Is.EqualTo("CBK0124-0").Using(CustomComparers.TypeComparison), "passwordGroup.Reference");
			NUnit.Framework.Assert.That(passwordGroup.Annotations.Count, NUnit.Framework.Is.EqualTo(0), "passwordGroup.Annotations.Count");
			var item = passwordGroup.Annotations.AddNew();
			item.Name = "StatusReason";
			item.Value = "recv file by tymcomm+J fail! return code :6, please refer to tymcomm's log";
			var passwordGroup1 = (Group)staffGroup.Items[1];
			NUnit.Framework.Assert.That(passwordGroup1.Type, NUnit.Framework.Is.EqualTo("MailBoxID").Using(CustomComparers.TypeComparison), "passwordGroup1.Type");
			NUnit.Framework.Assert.That(passwordGroup1.Reference, NUnit.Framework.Is.EqualTo("CBK0124-1").Using(CustomComparers.TypeComparison), "passwordGroup1.Reference");
			NUnit.Framework.Assert.That(passwordGroup1.Annotations.Count, NUnit.Framework.Is.EqualTo(0), "passwordGroup1.Annotations.Count");
			item = passwordGroup1.Annotations.AddNew();
			item.Name = "StatusReason";
			item.Value = "recv file by tymcomm+J fail! return code :6, please refer to tymcomm's log";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var handler = GetXmlCredentialConfigurationHandler(configuration.Name, logger);
			tvaPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			NUnit.Framework.Assert.That(tvaPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.PasswordOK).Using(CustomComparers.TypeComparison), "tvaPassword.Status");
			uvcPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			NUnit.Framework.Assert.That(uvcPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.PasswordOK).Using(CustomComparers.TypeComparison), "uvcPassword.Status");
			passwordGroup.Status = PasswordStatusList.Codes.Invalid;
			passwordGroup1.Status = PasswordStatusList.Codes.Invalid;
			configuration.Timestamp = tvaPassword.GP_SystemLastEditTimeUtc.AddMinutes(-10);
			handler.Process(configuration);
			Factory.Save();
			NUnit.Framework.Assert.That(tvaPassword.GP_StatusReason, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "tvaPassword.StatusReason");
			NUnit.Framework.Assert.That(tvaPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.PasswordOK).Using(CustomComparers.TypeComparison), "tvaPassword.Status");
			NUnit.Framework.Assert.That(uvcPassword.GP_StatusReason, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "uvcPassword.StatusReason");
			NUnit.Framework.Assert.That(uvcPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.PasswordOK).Using(CustomComparers.TypeComparison), "uvcPassword.Status");
			configuration.Timestamp = tvaPassword.GP_SystemLastEditTimeUtc.AddMinutes(10);
			handler.Process(configuration);
			NUnit.Framework.Assert.That(tvaPassword.GP_StatusReason, NUnit.Framework.Is.EqualTo("MailBoxID: recv file by tymcomm+J fail! return code :6, please refer to tymcomm's log").Using(CustomComparers.TypeComparison), "tvaPassword.StatusReason");
			NUnit.Framework.Assert.That(tvaPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "tvaPassword.Status");
			NUnit.Framework.Assert.That(uvcPassword.GP_StatusReason, NUnit.Framework.Is.EqualTo("MailBoxID: recv file by tymcomm+J fail! return code :6, please refer to tymcomm's log").Using(CustomComparers.TypeComparison), "uvcPassword.StatusReason");
			NUnit.Framework.Assert.That(uvcPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "uvcPassword.Status");
			outgoingInterchange.EI_BodyText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.TWCustomsSubscribers.xml");
			using (var reader = outgoingInterchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			systemGroup = configuration.Group[0];
			systemGroup.Reference = systemGroupCode;
			companyGroup = (Group)systemGroup.Items[0];
			companyGroup.Reference = companyGroupCode;
			staffGroup = (Group)companyGroup.Items[0];
			staffGroup.Reference = glbStaff.GS_Code;
			passwordGroup = (Group)staffGroup.Items[0];
			passwordGroup.Reference = "CBK0124-0";
			tvaPassword.GP_StatusReason = ZString.Empty;
			tvaPassword.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			configuration.Timestamp = tvaPassword.GP_SystemLastEditTimeUtc.AddMinutes(10);
			handler.Process(configuration);
			tvaPassword.Reload();
			NUnit.Framework.Assert.That(tvaPassword.GP_StatusReason, NUnit.Framework.Is.EqualTo("MailBoxID: recv file by tymcomm+J fail! return code :6, please refer to tymcomm's log").Using(CustomComparers.TypeComparison), "tvaPassword.StatusReason");
			NUnit.Framework.Assert.That(tvaPassword.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "tvaPassword.Status");
		}
	}
}
