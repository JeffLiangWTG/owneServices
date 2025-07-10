using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.FormalEntry.Testing
{
	sealed class CusdecMenuTest : Declaration.Testing.NZEDIMenuAbstractTest
	{
		[ExpectNoExceptions]
		public void TestSubmitJob()
		{
			declaration.DeclarationNumber = "";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			menu.SubmitJobMenuItem_Run();
			Assert("Expect No Exceptions", true);
		}

		[ExpectNoExceptions]
		public void TestCancelJob()
		{
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			menu.CancelJobMenuItem_Run();
			Assert("Expect No Exceptions", true);
		}

		public void TestNoCreditCheckWhenCancelJob()
		{
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Factory.Save();
			AssertEquals("PreCondition", 0, declaration.CusEntryHeader.Messages.Count);
			menu.CancelJobMenuItem_Run();
			AssertEquals("Cancellation message queued", 1, declaration.CusEntryHeader.Messages.Count);
		}

		public void TestResetToOriginal()
		{
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Factory.Save();
			menu.ResetToOriginalMenuItem_Run();
			AssertEquals(FormalEntryStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
		}

		public void TestCallMessageSendingFormCanStillSend()
		{
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (ZForm parentForm = new ZForm())
			{
				parentForm.Menu.MenuItems.Add(menu);
				menu.SubmitJobMenuItem_Run();
			}

			AssertEquals("JobDeclaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "Original Entry Message " + MessageManager.MessageReportingImmediateSend, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCallMessageSendingFormWhenMessageIsQueuedAndTheUserDoesntWantToCancelThePendingMessage()
		{
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menu.SubmitJobMenuItem_Run();
			AssertEquals("JobDeclaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "Cannot Send Message - Message is Already Queued to be sent on: " + declaration.CachedTodaysDate.ToShortDateString(), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCallMessageSendingFormWhenMessageIsQueuedAndTheUserWantsToCancelThePendingMessage()
		{
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menu.SubmitJobMenuItem_Run();
			AssertEquals("JobDeclaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
		}

		[TestDate(2010, 09, 01)]
		public void TestSubmitIPIAllowedForNonBroker()
		{
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				//reset declarant to non-broker user
				var declarant = GlbStaff.CurrentUser;
				declarant.GS_EmailAddress = "";
				declarant.GetNZWrapper().NZBPassword.GP_UserID = "";
				Factory.Save();
				declaration.DeclarationNumber = "";
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
				declaration.JE_DateOfArrival = ZDateTime.Today;
				declaration.JE_EntrySubmittedDate = ZDateTime.Today;
				Factory.Save();
				using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
				{
					menu.SubmitJobMenuItem_Run();
					AssertContains("IMPORT + IPI cannot send without a broker ID", BrokerValidation.MissingBrokerIdMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
				{
					menu.SubmitJobMenuItem_Run();
					AssertContains("IMPORT + IPI cannot send without a means of communication", BrokerValidation.MissingDeclarantCommunicationsMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					declarant.GS_EmailAddress = "test.user@company.org";
					Factory.Save();
					menu.SubmitJobMenuItem_Run();
					AssertEquals("Non-Broker is able to send an IPI message", "Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.Text);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
					declaration.ResetToOriginal();
					Factory.Save();
					menu.SubmitJobMenuItem_Run();
					AssertContains("Non-Broker is NOT able to send a Formal message", BrokerValidation.MissingDeclarantIdMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
					Factory.Save();
					menu.SubmitJobMenuItem_Run();
					AssertEquals("Broker IS able to send a Formal message", "Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation
		TestFormalEntryCreator decCreator;
		protected override void SetUp()
		{
			base.SetUp();
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "5837458";
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(declarant.PK.ToGuid()).Encrypt("TEST1");
			declarant.GS_EmailAddress = "test.user@testCompany.org";
			Factory.Save();
			declaration = JobDeclaration.New(Factory);
			decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			menu.Declaration = declaration;
		}

		protected override void SetupValidPINIfRequired()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = currentUser.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "WTFZOMG";
			CurrentUsersPin usersPin = new CurrentUsersPin(Factory, false);
			usersPin.DecryptedPinCode = "CandyMountainAdventure";
			currentUser.GS_EmailAddress = "test.user@testCompany.org";
			Factory.Save();
		}
		#endregion
	}
}
