using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(EHubRegistryUpdateServiceTask))]
	class EHubRegistryUpdateServiceTaskTest : ServiceTaskTestCase<EHubRegistryUpdateServiceTask>
	{
		public void TestProcessIncomingPasswordUpdate()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var sgWrapper = staff.GetSGWrapper();
			var ntpPassword = sgWrapper.SGNationalTradePlatformPassword;
			ntpPassword.GP_UserID = "ABC123";
			ntpPassword.CurrentDecryptedPassword = "HELLO";
			ntpPassword.NextDecryptedPassword = "GOODBYE";
			ntpPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			Factory.Save();

			var outgoingInterchange = ConfigurationTestHelper.GetLatestEHubConfigurationInterchange(Factory);
			Configuration outgoingConfiguration;
			using (var reader = outgoingInterchange.GetEI_BodyTextReader())
			{
				outgoingConfiguration = reader.DeserializeToConfiguration();
			}
			CombineAssertions(() =>
			{
				AssertEquals("configuration.Name", "SGCustomsNTP", outgoingConfiguration.Name);
				var systemGroup = outgoingConfiguration.Group[0];
				AssertEquals("systemGroup.Type", "System", systemGroup.Type);
				AssertEquals("systemGroup.Reference", "EDIDAT", systemGroup.Reference);
				AssertEquals("systemGroup.Status", "", systemGroup.Status);
				AssertEquals("systemGroup.Annotations.Count", 0, systemGroup.Annotations.Count);
				AssertEquals("systemGroup.Items.Length", 1, systemGroup.Items.Length);
				var companyGroup = (Group)systemGroup.Items[0];
				AssertEquals("companyGroup.Type", "Company", companyGroup.Type);
				AssertEquals("companyGroup.Reference", "EDI", companyGroup.Reference);
				AssertEquals("companyGroup.Status", "", companyGroup.Status);
				AssertEquals("companyGroup.Annotations.Count", 0, companyGroup.Annotations.Count);
				AssertEquals("companyGroup.Items.Length", 1, companyGroup.Items.Length);
				var staffGroup = (Group)companyGroup.Items[0];
				AssertEquals("staffGroup.Type", "Staff", staffGroup.Type);
				AssertEquals("staffGroup.Reference", "E", staffGroup.Reference);
				AssertEquals("staffGroup.Status", "", staffGroup.Status);
				AssertEquals("staffGroup.Annotations.Count", 0, staffGroup.Annotations.Count);
				AssertEquals("staffGroup.Items.Length", 1, staffGroup.Items.Length);
				var ntpPasswordGroup = (Group)staffGroup.Items[0];
				AssertEquals("ntpPasswordGroup.Type", "NTP", ntpPasswordGroup.Type);
				AssertEquals("ntpPasswordGroup.Reference", "", ntpPasswordGroup.Reference);
				AssertEquals("ntpPasswordGroup.Status", PasswordStatusList.Codes.Invalid, ntpPasswordGroup.Status);
				AssertEquals("ntpPasswordGroup.Annotations.Count", 0, ntpPasswordGroup.Annotations.Count);
				AssertEquals("ntpPasswordGroup.Items.Length", 2, ntpPasswordGroup.Items.Length);
				var ntpPasswordGroupCredential1 = (Credential)ntpPasswordGroup.Items[0];
				AssertEquals("ntpPasswordGroupCredential1.Name", "Current", ntpPasswordGroupCredential1.Name);
				AssertEquals("ntpPasswordGroupCredential1.UserName", "ABC123", ntpPasswordGroupCredential1.UserName);
				AssertNotEquals("ntpPasswordGroupCredential1.Password", ZString.Empty, ntpPasswordGroupCredential1.Password);
				var ntpPasswordGroupCredential2 = (Credential)ntpPasswordGroup.Items[1];
				AssertEquals("ntpPasswordGroupCredential2.Name", "Next", ntpPasswordGroupCredential2.Name);
				AssertEquals("ntpPasswordGroupCredential2.UserName", "ABC123", ntpPasswordGroupCredential2.UserName);
				AssertNotEquals("ntpPasswordGroupCredential2.Password", ZString.Empty, ntpPasswordGroupCredential2.Password);
				ntpPasswordGroup.Status = PasswordStatusList.Codes.Valid;
				ntpPasswordGroupCredential1.Password = null;
			});
			outgoingConfiguration.Timestamp = ZDateTime.UtcNow;

			TestForInterchangeType(1, EDIInterchangeTypeList.Codes.EHubRegistryUpdate, ntpPassword, outgoingConfiguration);
			TestForInterchangeType(2, EDIInterchangeTypeList.Codes.Configuration, ntpPassword, outgoingConfiguration);
		}

		void TestForInterchangeType(int interchangeNo, string interchangeType, IGlbExternalPassword ntpPassword, Configuration outgoingConfiguration)
		{
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.eHub;
			incomingInterchange.EI_InterchangeType = interchangeType;
			incomingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingInterchange.EI_From = Constants.Configuration.EHubRecipient;
			incomingInterchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			incomingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			incomingInterchange.EI_BodyText = outgoingConfiguration.ToXml();
			Factory.Save();

			var serviceTask = new EHubRegistryUpdateServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertContains("serviceTask.ServiceLogger", $"Interchange '{interchangeNo}' has been processed successfully.", serviceTask.ServiceLogger.ToString());
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var interchangeLoaded = factory.Load<EDIInterchange>(incomingInterchange.PK);
			AssertEquals("interchange status", EDIInterchange.Status.Received, interchangeLoaded.EI_Status);
			var ntpPasswordLoaded = factory.Load<GlbExternalPassword>(ntpPassword.PK);
			AssertEquals("ntpPasswordLoaded.GP_UserID", "ABC123", ntpPasswordLoaded.GP_UserID);
			AssertEquals("ntpPasswordLoaded.CurrentDecryptedPassword", "HELLO", ntpPasswordLoaded.CurrentDecryptedPassword);
			AssertEquals("ntpPasswordLoaded.GP_PasswordStatus", Core.Constants.PasswordOK, ntpPasswordLoaded.GP_PasswordStatus);
			AssertEquals("ntpPasswordLoaded.NextDecryptedPassword", ZString.Empty, ntpPasswordLoaded.NextDecryptedPassword);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
