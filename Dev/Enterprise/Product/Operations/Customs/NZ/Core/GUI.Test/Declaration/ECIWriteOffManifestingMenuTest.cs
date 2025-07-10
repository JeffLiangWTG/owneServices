using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Testing
{
	sealed class ECIWriteOffManifestingMenuTest : Declaration.Testing.NZEDIMenuAbstractTest
	{
		public void TestSubmittingManifestFromSecondDeclarationDoesntBarf()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			declaration.JE_HouseBill = "";
			AssertHasMessageErrors(declaration.JE_HouseBillInfo);
			Customs.Business.SendsMessagesToCustomsShutterUpperer shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			shutterUpperer.AnswerToContinueWithAction = true;
			declaration2.MessageInitiator = shutterUpperer;
			menu.Declaration = declaration2;
			Factory.Save();
			bool gotApplicationExceptionOnPollingNullMessageInitiator = false;
			try
			{
				AssertNull("Precondition: Declaration1.MessageInitiator", declaration.MessageInitiator);
			}
			catch (ApplicationException)
			{
				gotApplicationExceptionOnPollingNullMessageInitiator = true;
			}

			AssertEquals("gotApplicationExceptionOnPollingNullMessageInitiator", true, gotApplicationExceptionOnPollingNullMessageInitiator);
			menu.SubmitJobMenuItem_Run();
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public void TestSubmitJobCannotBeRunWithAResponsePending()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration;
			entryHeader.EntryNumber = "";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			menu.SubmitJobMenuItem_Run();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "Cannot Send Message - Job is Waiting for a Response", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public void TestSubmitJobCannotSendOnceManifested()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration;
			entryHeader.EntryNumber = "";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			menu.SubmitJobMenuItem_Run();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "You cannot submit a declaration entry as a stand-alone Brokerage entry once it has been Manifested.\r\n\r\nOnce any relevant changes have been made and saved to the declaration here,\r\nyou need to action (submit) this job via the Manifest menu options.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus has not changed", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus has not changed", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus has not changed", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}

		public void TestQueueJobCannotQueueOnceManifested()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration;
			entryHeader.EntryNumber = "";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			menu.QueueJobMenuItem_Run();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "This declaration has already been Manifested.\r\nYou need to take any further action on this job via the Manifest menu options.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus has not changed", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus has not changed", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus has not changed", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}

		public void TestCancelJob()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration2;
			entryHeader.EntryNumber = "12345678";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			menu.CancelJobMenuItem_Run();
			AssertEquals("A Manifested entry cannot be cancelled via the brokerage job menu", "You cannot cancel a declaration entry as a stand-alone Brokerage entry once it has been Manifested.\r\nYou need to action (cancel) this job via the Manifest menu options.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}

		public void TestResetToOriginal()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration;
			entryHeader.EntryNumber = "12345678";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			menu.ResetToOriginalMenuItem_Run();
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "You cannot reset a declaration entry to original once it has been Manifested", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}

		public void TestCallMessageSendingFormCanStillSend()
		{
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			menu.Declaration = declaration2;
			entryHeader.EntryNumber = "12345678";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			using (ZForm parentForm = new ZForm())
			{
				parentForm.Menu.MenuItems.Add(menu);
				menu.SubmitJobMenuItem_Run();
			}

			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "ICR Original Entry Message " + MessageManager.MessageReportingImmediateSend, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public void TestCallMessageSendingFormWithInvalidGB()
		{
			var declarant = GlbStaff.CurrentUser;
			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";
			declarant.GS_EmailAddress = "test.user@company.org";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.InvoiceLines.AddNew().JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			menu.Declaration = declaration;
			var rule = new JobBranchDefaultOrderRule { DefaultToBlank = 1, DefaultToBranchRelatedToPortOrWarehouseBranch = 2, DefaultToBranchOfOrganisation = 3, DefaultToLoginUserDefault = 4 };
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, rule);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			declaration.JE_JS = shipment.PK;
			TestCaseHelper.ClearTable(NZCCustomsExchangeRate.Schema.TableName);
			var endDate = new ZDateTime(2014, 3, 12);
			NZCCustomsExchangeRate exchangeRate = Factory.New<NZCCustomsExchangeRate>();
			exchangeRate.U7_CurrencyCode = "USD";
			exchangeRate.U7_DateActiveFrom = endDate.AddDays(-14);
			exchangeRate.U7_DateActiveTo = endDate;
			exchangeRate.U7_Rate = 1.12m;
			AssertEquals(ZGuid.Empty, shipment.Job.JH_GB);
			using (var form = new DeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Menu.MenuItems.Add(menu);
				menu.SubmitJobMenuItem_Run();
			}
		}

		public void TestUrlCustomsFindVesselOrFlightConstant()
		{
			AssertEquals("URL link is current as at Nov 2017 change", "https://www.customs.govt.nz/business/import/lodge-your-import-entry/craft-names-and-flight-numbers/", NZEDIMenu.UrlCustomsFindVesselOrFlight);
		}

		#region Implementation

		Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader entryHeader;
		TestManifestCreator manifestCreator;
		JobDeclaration declaration2;

		protected override void SetUp()
		{
			base.SetUp();
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF253", "QF253", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PK", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			entryHeader = Factory.New<Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader>();
			manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF253", "USSFO", "NZWLG", ZDateTime.Today.AddDays(-1), ZDateTime.Today);
			declaration = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			entryHeader.CH_CustomsMessageRemarks = "RIGHT HANDED SCREWDRIVERS HAVE A LIMITED USE";
		}

		#endregion
	}
}
