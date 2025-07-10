using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Testing
{
	sealed class CuscarMenuTest : Declaration.Testing.NZEDIMenuAbstractTest
	{
		public void TestSubmitJob()
		{
			declaration.DeclarationNumber = "";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			menu.SubmitJobMenuItem_Run();
			Assert("Expect No Exceptions", true);
		}

		public void TestSubmitTSWWriteOffNotAllowedForNonBroker()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			//reset declarant to non-broker user
			declarant.GetNZWrapper().NZBPassword.GP_UserID = "";
			declarant.GS_EmailAddress = "";
			Factory.Save();
			declaration.DeclarationNumber = "";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			Factory.Save();
			menu.SubmitJobMenuItem_Run();
			Assert("Expect No Exceptions", true);
			AssertContains("Non-Broker should NOT be able to send a TSW write-off message", BrokerValidation.MissingDeclarantIdMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSubmitTSWWriteOffRequiresBroker()
		{
			var declarant = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			declarant.GS_EmailAddress = "";
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_EntrySubmittedDate = ZDateTime.Today;
			Factory.Save();
			menu.SubmitJobMenuItem_Run();
			Assert("Expect No Exceptions", true);
			AssertContains("Broker when details not fully set up should NOT be able to send a TSW write-off message", BrokerValidation.MissingDeclarantCommunicationsMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			declarant.GS_EmailAddress = "staff.user@company.org";
			Factory.Save();
			menu.SubmitJobMenuItem_Run();
			AssertEquals("Broker can send TSW write-off messages", "Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCancelJob()
		{
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.DeclarationNumber = "12345678";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_EntrySubmittedDate = ZDateTime.Today;
			Factory.Save();
			menu.CancelJobMenuItem_Run();
			AssertEquals("JobDeclaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
		}

		public void TestResetToOriginal()
		{
			declaration.DeclarationNumber = "12345678";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Factory.Save();
			menu.ResetToOriginalMenuItem_Run();
			AssertEquals("JobDeclaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
		}

		public void TestCallMessageSendingFormCanStillSend()
		{
			declaration.DeclarationNumber = "12345678";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_EntrySubmittedDate = ZDateTime.Today;
			Factory.Save();
			using (ZForm parentForm = new ZForm())
			{
				parentForm.Menu.MenuItems.Add(menu);
				menu.SubmitJobMenuItem_Run();
			}

			AssertEquals("JobDeclaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "ICR Original Entry Message " + MessageManager.MessageReportingImmediateSend, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation
		TestECIWriteOffCreator decCreator;

		protected override void SetUp()
		{
			base.SetUp();
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(declarant.PK.ToGuid()).Encrypt("NN12WW");
			declarant.GS_EmailAddress = "staff.user@company.org";
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			decCreator = new TestECIWriteOffCreator(declaration);
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestForAir();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			menu.Declaration = declaration;
		}
		#endregion
	}
}
