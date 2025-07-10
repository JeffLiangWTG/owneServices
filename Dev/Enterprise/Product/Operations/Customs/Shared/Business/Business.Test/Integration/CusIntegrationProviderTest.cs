using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.InterfaceImplementations.Testing
{
	sealed class CusIntegrationProviderTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestCusIntegrationProvider()
		{
			var integration = new CusIntegrationProviderTestClass(ZString.Empty);
			var result = integration.Execute(Declaration);
			AssertEquals("UserName of TEST should not be empty or invalid.\n" +
						 "\nThis can be entered in the Registry: TEST Registry.", result);
			AssertEquals("No messages are submitted.", 0, Declaration.Messages.Count);

			var dataExportLog = Declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNull("Precondition", dataExportLog);

			integration = new CusIntegrationProviderTestClass("ABC");

			Declaration.DPSFreightMovementRestricted = true;
			result = integration.Execute(Declaration);
			AssertEquals("Unable to submit message due to Denied Party Screening cancellation.", result);
			AssertEquals("0 messages created.", 0, Declaration.DiscardedMessages.Count);

			Declaration.DPSFreightMovementRestricted = false;
			result = integration.Execute(Declaration);
			AssertEquals("Submit Succeeded.", result);
			AssertEquals("1 message is created.", 1, Declaration.DiscardedMessages.Count);

			var message = (EDIMessage)Declaration.DiscardedMessages.FirstOrDefault();
			AssertEquals("TST", message.EM_ApplicationCode);
			AssertEquals("TST", message.EM_MessageType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("SNT", message.EM_Status);
			Assert("Message is not empty.", !message.EM_MessageText.IsEmpty);

			dataExportLog = Declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNotNull("The exported log should not be null", dataExportLog);
			AssertEquals("The related edimessage should be correct", message, dataExportLog.RelatedEDIMessage.Message);
		}

		#region Implementation

		MessageManageableJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<MessageManageableJobDeclaration>()); }
		}

		MessageManageableJobDeclaration declaration;

		#endregion

		class CusIntegrationProviderTestClass : CusIntegrationProvider
		{
			public CusIntegrationProviderTestClass(ZString userName)
			{
				this.userName = userName;
			}
			readonly ZString userName;

			protected override bool SubmitSucceeded(XElement submissionResult)
			{
				var response = submissionResult.Descendants("RequestResponse").FirstOrDefault();
				return response != null && response.Value == "ok";
			}

			protected override XElement Submit(ZString submittedData)
			{
				var result = submittedData.IsEmpty ? "No data to submit." : "ok";
				return new XElement("TST", new XElement("RequestResponse", result));
			}

			protected override ZString ApplicationCode
			{
				get { return "TST"; }
			}

			protected override ZString ProviderName
			{
				get { return "TEST"; }
			}

			protected override ZString RegistryLocation
			{
				get { return "TEST Registry"; }
			}

			protected override ICollection<SettingDetail> SettingsToValidate
			{
				get
				{
					var result = base.SettingsToValidate;
					result.Add(new SettingDetail(UserName, "UserName"));
					return result;
				}
			}

			ZString UserName
			{
				get { return userName; }
			}
		}
	}
}
