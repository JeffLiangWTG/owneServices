using System;
using Enterprise.Core;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(SGCustomsDataRegistry))]
	sealed class SGCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<SGCustomsDataRegistry>
	{
		public void TestPermitPrintingCertificate()
		{
			TestRegistryItem(ItemSet.PermitPrintingCertificate, "PermitPrintingCertificate", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4, "Permit Printing Certificate", "Specify the Permit Printing Certificate", RegistryStorageFlags.System, RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, SingatureRepository.Instance.DefaultPermitPrintCertificateData);
		}

		public void TestPermitPrintingCertificatePassword()
		{
			TestRegistryItem(ItemSet.PermitPrintingCertificatePassword, "PermitPrintingCertificatePassword", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4, "Permit Printing Certificate Password", "Specify the Password for Permit Printing Certificate", RegistryStorageFlags.System, TextEditorType.Password, RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, SingatureRepository.Instance.DefaultPermitPrintCertificatePassword);
		}

		public void TestEnableXMLTradeNetMessaging()
		{
			TestRegistryItem(ItemSet.EnableXMLTradeNetMessaging, "EnableXML", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4, "Enable XML TradeNet Messaging", "The system will send and receive XML TradeNet messages when enabled.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, true);
		}

		public void TestOriginCriterion()
		{
			AssertEquals(60, ItemSet.OriginCriterion.Value.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "CTC", "CTH", "OTHER", "PE", "PSR(CTC)", "PSR(Other)", "PSR(RVC)", "RVC", "RVC + CC", "RVC + CTH", "RVC + CTSH", "WO", "ACFTA, CUMULATIVE, CONTENT", "PSR", "SINGLE, COUNTRY, CONTENT", "X", "RVC + CTSH", "WO", "CTC", "CTH", "CTH, DMI", "PE", "RVC", "RVC, ACU", "SP", "WO", "CTC", "CTH", "CTH + RVC", "RVC", "SPECIFIC, PROCESSES", "WO", "WO-AK", "ISECA, CUMULATIVE, CONTENT", "PRODUCT, SPECIFIC, RULES", "SINGLE, COUNTRY, CONTENT", "X", "CC", "RVC", "SP", "WO", "F", "G", "P", "PK", "W", "Y", "ASEAN, CUMULATIVE, CONTENT", "ACU", "ASEAN", "CC", "CONTENT", "COUNTRY", "CTSH", "CUMULATIVE", "DMI", "NON ASEAN", "PC", "SINGLE", "Z" }, ItemSet.OriginCriterion.Value.GetAllCodes());
		}

		public void TestISGCustomsRegistry_CycleNumbers()
		{
			AssertEquals(ItemSet.CycleNumbers, ((Integration.Customs.SG.ISGCustomsRegistry)ItemSet).CycleNumbers);
		}

		public void TestMaximumMessageSize()
		{
			TestRegistryItem(ItemSet.MaximumMessageSize, "MaximumMessageSize", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB, "Maximum Message Size", "Maximum Message Size.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, 10, 10, 100);
		}

		public void TestSendImpediments()
		{
			TestRegistryItem(ItemSet.SendImpediments, "SendImpediments", SGCustomsDataRegistry.Categories.Customs_Singapore_Emails, "Send Impediment Queries", "Send Customs or Controlling Agency query instructions to staff member, nominated group or combination of both", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendImpedimentsToGroup()
		{
			TestRegistryItem(ItemSet.SendImpedimentsToGroup, "SendImpedimentsToGroup", SGCustomsDataRegistry.Categories.Customs_Singapore_Emails, "Send Impediment Queries To Group", "Send Customs or Controlling Agency query instructions to selected group", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.IsValueMandatory, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestACCESSEnable()
		{
			TestRegistryItem(ItemSet.ACCESSEnable, "ACCESSEnable", SGCustomsDataRegistry.Categories.Customs_Singapore_ACCESS, (NoResString)"Enabled", (NoResString)"Company listed is enabled for ACCESS system.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestCycleNumbers()
		{
			TestGenericRegistryItem(ItemSet.CycleNumbers, "CycleNumbers", SGCustomsDataRegistry.Categories.Customs_Singapore_ACCESS, "Cycle Numbers", "This is a list of Cycle Numbers applicable for the ACCESS system.", RegistryStorageFlags.System, RegistryOptions.Default);
		}

		public void TestSubmissionRetryLimit()
		{
			TestRegistryItem(ItemSet.SubmissionRetryLimit, "SubmissionRetryLimit", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4, "Submission Retry Limit", "Please enter the number of submission retries before failing a TradeNet interchange.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, 3, 1, 5);
		}

		public void TestSubmissionFinalRetryDelay()
		{
			TestRegistryItem(ItemSet.SubmissionFinalRetryDelay, "SubmissionFinalRetryDelay", SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4, "Submission Final Retry Delay", "Please enter the number of minutes to delay the final submission retry to TradeNet. Maximum of 60 minutes.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, 30, 0, 60);
		}

		public void TestSynchronousMHUBTimeout()
		{
			TestRegistryItem(ItemSet.SynchronousMHUBTimeout,
				"SynchronousMHUBTimeout",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Synchronous Timeout",
				"How many milliseconds should be waited before initial connection is cancelled",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				300000);
		}

		public void TestSynchronousMHUBReadWriteTimeout()
		{
			TestRegistryItem(ItemSet.SynchronousMHUBReadWriteTimeout,
				"SynchronousMHUBReadWriteTimeout",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Synchronous ReadWrite Timeout",
				"How many milliseconds should be waited before between packets before connection is cancelled",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				600000);
		}

		public void TestMessageNumberOffset()
		{
			TestRegistryItem(ItemSet.MessageNumberOffset,
				"MessageNumberOffset",
				SGCustomsDataRegistry.Categories.Customs_Singapore,
				"Message Number Offset",
				"Offset value used to generate a different URN number range.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				0);
		}

		public void TestSendTestMessages()
		{
			TestRegistryItem(ItemSet.SendTestMessages,
				"SendTestMessages",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Send Test Messages to TradeNet Version 4",
				"Setting this flag on will mean TradeNet messages generated will be sent into the MHub Version 4 Test System.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestCargoAgentCode()
		{
			TestRegistryItem(ItemSet.CargoAgentCode,
				"CargoAgentCode",
				SGCustomsDataRegistry.Categories.Customs_Singapore_CMD,
				"Cargo Agent Code",
				"Cargo Agent Code required for sending CMD Messages",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty);
		}

		public void TestCargoAgentRef()
		{
			TestRegistryItem(ItemSet.CargoAgentRef,
				"CargoAgentRef",
				SGCustomsDataRegistry.Categories.Customs_Singapore_CMD,
				"Cargo Agent Reference",
				"Cargo Agent Reference required for sending CMD Messages",
				RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty);
		}

		public void TestGHAList()
		{
			CodeDescriptionPairList testValues = new CodeDescriptionPairList();
			testValues.AddPair(Constants.SGCustoms.GHA.DNAT, "DNATA GHA");
			testValues.AddPair(Constants.SGCustoms.GHA.SATS, "SATS GHA");

			TestRegistryItem(ItemSet.GHAList,
				"GHAList",
				SGCustomsDataRegistry.Categories.Customs_Singapore_CMD,
				"GHA List",
				"List of Ground Handling Agents operating in Singapore",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				4,
				2,
				testValues[0], testValues[1]);
		}

		public void TestSendViaEHub()
		{
			TestRegistryItem(ItemSet.SendViaEHub,
				"SendViaEHub",
				SGCustomsDataRegistry.Categories.Customs_Singapore_CMD,
				"Send via eHub",
				"Send CMD Messages through eHub",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				true);
		}

		public void TestSendAcknowledgements()
		{
			TestRegistryItem(ItemSet.SendAcknowledgements,
				"SendAcknowledgements",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Emails,
				"Send Declaration Acknowledgements",
				"Send message acknowledgements to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendAcknowledgementsToGroup,
				"SendAcknowledgementsToGroup",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Emails,
				"Send Declaration Acknowledgements to Group",
				"Send message acknowledgements to selected group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestSendErrors()
		{
			TestRegistryItem(ItemSet.SendErrors,
				"SendErrors",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Emails,
				"Send Declaration Errors",
				"Send message errors to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendErrorsToGroup,
				"SendErrorsToGroup",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Emails,
				"Send Declaration Errors To Group",
				"Send message errors to selected group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				RegistryFindBoxCollection.GlbGroup,
				Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestWebAddress()
		{
			TestGenericRegistryItem(ItemSet.WebAddress,
				"WebAddress",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Web Address",
				"The web address of the TradeNet applet server",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
			AssertEquals("Previous value", "https://www.tradexchange.gov.sg", SGCustomsDataRegistry.Instance.WebAddress.Value.PreviousValue);
			AssertEquals("New value", "https://www.tradenet.gov.sg", SGCustomsDataRegistry.Instance.WebAddress.Value.NewValue);
			AssertEquals("Effective Date", SGCustomsDataRegistry.Instance.MHUBAccessUpgradeEffectiveDate.Value, SGCustomsDataRegistry.Instance.WebAddress.Value.EffectiveDate);
		}

		public void TestWebAddressTrial()
		{
			TestGenericRegistryItem(ItemSet.WebAddressTrial,
				"WebAddressTrial",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Web Address (trial MHX4)",
				"The web address of the TRIAL TradeNet applet server. Do not edit this without instruction from WTG.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
			AssertEquals("Previous value", "https://trial.tradexchange.gov.sg", SGCustomsDataRegistry.Instance.WebAddressTrial.Value.PreviousValue);
			AssertEquals("New value", "https://trial.tradenet.gov.sg", SGCustomsDataRegistry.Instance.WebAddressTrial.Value.NewValue);
			AssertEquals("Effective Date", SGCustomsDataRegistry.Instance.MHUBAccessUpgradeEffectiveDate.Value, SGCustomsDataRegistry.Instance.WebAddressTrial.Value.EffectiveDate);
		}

		public void TestMhx4EndpointPartialPathForTestConnectionServlet()
		{
			TestRegistryItem(ItemSet.Mhx4EndpointPartialPathForTestConnectionServlet,
				"Mhx4EndpointPartialPathForTestConnectionServlet",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Endpoint - Test Connection Servlet",
				"Relative URI for Test Connection Servlet endpoint. Do not edit this without instruction from WTG.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				"/txmhbweb/mhb/TestConnectionServlet");
		}

		public void TestMHUBAccessUpgradeEffectiveDate()
		{
			TestGenericRegistryItem(ItemSet.MHUBAccessUpgradeEffectiveDate,
				"MHUBAccessUpgradeEffectiveDate",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"MHAccess upgrade effective date",
				"Effective Date when the MHAccess upgrade comes into effect.\r\nSG Customs are upgrading their MHAccess software to utilise JRE 1.8 and improved GCM (Galois Counter Mode) cipher.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				new DateTime(2017, 07, 27));
		}

		public void TestJREVersion()
		{
			TestGenericRegistryItem(ItemSet.JREVersion,
				"JREVersion",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Java version",
				"The current Java version for MHAccess software to utilise.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
			AssertEquals("Previous value", "1.6.0_121", SGCustomsDataRegistry.Instance.JREVersion.Value.PreviousValue);
			AssertEquals("New value", "1.8.0_131", SGCustomsDataRegistry.Instance.JREVersion.Value.NewValue);
			AssertEquals("Effective Date", SGCustomsDataRegistry.Instance.MHUBAccessUpgradeEffectiveDate.Value, SGCustomsDataRegistry.Instance.JREVersion.Value.EffectiveDate);
		}

		public void TestMhaccessVersionString()
		{
			TestGenericRegistryItem(ItemSet.MhaccessVersionString,
				"MhaccessVersionString",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Version Number",
				"Version of MHAccess. Do not edit this without instruction from WTG.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
			AssertEquals("Previous value", "4.0.3", SGCustomsDataRegistry.Instance.MhaccessVersionString.Value.PreviousValue);
			AssertEquals("New value", "4.0.4", SGCustomsDataRegistry.Instance.MhaccessVersionString.Value.NewValue);
			AssertEquals("Effective Date", SGCustomsDataRegistry.Instance.MHUBAccessUpgradeEffectiveDate.Value, SGCustomsDataRegistry.Instance.MhaccessVersionString.Value.EffectiveDate);
		}

		public void TestDigestForLibs()
		{
			TestGenericRegistryItem(ItemSet.DigestForLibs,
				"DigestForLibs",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Digest",
				"Digest of MHAccess libraries. Do not edit this without instruction from WTG.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
			AssertEquals("Previous value", "6DB57667814ABDD96DE73FED486095F0F0947F1E7FB64DA8538FCAA50CA40BDB", SGCustomsDataRegistry.Instance.DigestForLibs.Value.PreviousValue);
			AssertEquals("New value", "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D", SGCustomsDataRegistry.Instance.DigestForLibs.Value.NewValue);
			AssertEquals("Effective Date", SGCustomsDataRegistry.Instance.MHUBAccessUpgradeEffectiveDate.Value, SGCustomsDataRegistry.Instance.DigestForLibs.Value.EffectiveDate);
		}

		public void TestHomeDirectory()
		{
			TestRegistryItem(ItemSet.HomeDirectory,
				"HomeDirectory",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Server Home Directory",
				"The home directory on the TradeNet server",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				"/fshome/sftp/cwise");
		}

		public void TestRecipientMailbox()
		{
			TestRegistryItem(ItemSet.RecipientMailbox,
				"RecipientMailbox",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB,
				"Recipient Mailbox",
				"The mailbox that TradeNet receive/send messages to",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				"DCS4001");
		}

		public void TestAttachPermitToAcknowledgementEmail()
		{
			TestRegistryItem(ItemSet.AttachPermitToAcknowledgementEmail,
				"AttachPermitToAcknowledgementEmail",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4,
				"Attach Permit To Acknowledgement Email",
				"A PDF version of the permit is attached to acknowledgement emails",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestLastSentEmailWithBrokerErrors()
		{
			TestGenericRegistryItem(ItemSet.LastSentEmailWithBrokerErrors,
				"LastSentEmailWithBrokerErrors",
						SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4,
						"Last Sent Email With Broker Errors",
						"Date of last sent email with broker errors",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsHidden,
				DateTime.MinValue);
		}

		public void TestRecipientMailboxTN4Live()
		{
			TestRegistryItem(ItemSet.RecipientIDLive,
				"RecipientIDLive",
				SGCustomsDataRegistry.Categories.Customs_Singapore_Tradenetv4_MHUB_Mhaccess4,
				"Recipient ID (live)",
				"Recipient ID (live). Do not edit this without instruction from WTG.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				"DCS4001");
		}

		protected override bool IsCountrySpecificRegistrySet
		{
			get { return true; }
		}
	}
}
