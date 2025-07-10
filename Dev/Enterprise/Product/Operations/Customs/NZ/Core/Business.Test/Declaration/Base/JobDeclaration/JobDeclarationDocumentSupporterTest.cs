using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	class JobDeclarationDocumentSupportStandaloneDeclarationTest : TestCaseWithFactory
	{
		#region TestGenerateQuestionsToAskUsersBeforeRunningDocument

		public void TestWarningAndQuestionOnDeliveryOrderPrint()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Factory.Save();

			var menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.DeliveryOrder);
			var result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			const string expectedMessage = @"You are trying to print a Delivery Order for a Declaration that is not currently cleared.

Please note that the document produced will not be valid for Customs Clearance processes as it will not have the required Delivery Instructions at the bottom of the document.

Are you sure you want to print a Delivery Order?";
			AssertWarningMessage(result[0], "Warning: Declaration Not Cleared", expectedMessage);
		}

		public void TestNoWarningWhenHasConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<JobDeclaration>(Factory);
			consolidatedDeclaration.JobDeclarations.Add(declaration);

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Factory.Save();

			var menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.EntryPrint);
			var result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			Assert("No WarningMessageIfCurrentEntryTotalAmountAndReturnedOneAreDifferent when is linked to a Consolidated Declaration", result.Count == 0);

			menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.CustomsCertificate);
			result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			Assert("No WarningMessageIfCurrentEntryTotalAmountAndReturnedOneAreDifferent when is linked to a Consolidated Declaration", result.Count == 0);
		}

		public void TestDOWarningNotShownForITAStatus()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Factory.Save();

			var menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.DeliveryOrder);
			var result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			const string expectedMessage = @"You are trying to print a Delivery Order for a Declaration that is not currently cleared.

Please note that the document produced will not be valid for Customs Clearance processes as it will not have the required Delivery Instructions at the bottom of the document.

Are you sure you want to print a Delivery Order?";
			AssertWarningMessage(result[0], "Warning: Declaration Not Cleared", expectedMessage);

			Declaration.JE_EntryStatus = ConsignmentGoodsStatusList.Codes.InternationalTranshipmentApproved;
			result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			Assert("No warning message when printing DO for ITA status", result.Count == 0);
		}

		public void TestGetDataStateBeforeRunningForPrerequisiteMergeCondition()
		{
			var declaration = Factory.New<JobDeclaration>();

			var menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.CustomsCertificate);
			var dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert(!dataState.IsValid);
			AssertContains(Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, dataState.ErrorMessage);
		}

		public void TestWarningOnEntryPrintAndCustomsCertificate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_TotalAmountReturned = 100;
			entryHeader.CH_TotalPaid = 200;

			AssertEntryTotalAmountWarning(JobDeclarationDocumentSupporter.DocNames.EntryPrint);
			AssertEntryTotalAmountWarning(JobDeclarationDocumentSupporter.DocNames.CustomsCertificate);

			var menuItem = GetMenuItem(JobDeclarationDocumentSupporter.DocNames.MAFCoverSheet);
			var result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("No warning messages", 0, result.Count);
		}

		void AssertEntryTotalAmountWarning(string menuName)
		{
			var result = GetDocumentSupporter().GenerateQuestionsToAskUsersBeforeRunningDocument(GetMenuItem(menuName));
			const string areYouSureYouWantToPrint = "{0}\r\n\r\nAre you sure you want to print {1}?";
			var entryTotalAmountMessage = JobDeclarationTest.GetEntryTotalAmountDoesNotMatchReturnedOneMessage(200, 100);
			var expectedMessage = string.Format(areYouSureYouWantToPrint, entryTotalAmountMessage, menuName);
			AssertWarningMessage(result[0], "Warning", expectedMessage);
		}

		static void AssertWarningMessage(DocumentSupporterQuestion question, string expectedTitle, string expectedMessage)
		{
			AssertMultilineASCIIEquals("Warning message should be displayed", expectedMessage, question.QuestionText);
			AssertEquals("QuestionType", QuestionType.Warning, question.QuestionType);
			AssertEquals("Title", expectedTitle, question.Title);
			AssertEquals("DefaultResponse", AnswerType.No, question.DefaultResponse);
		}

		StmMenuItem GetMenuItem(string menuName)
		{
			var menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = menuName;
			menuItem.SU_MenuPath = "Customs\\";
			menuItem.SU_FilterList = "NZ";
			return menuItem;
		}

		#endregion

		public void TestDocumentSupporterWillMergeIfDissectionReportCalled()
		{
			CheckMergeOnDocumentName("<any prefix>Dissection Report<any suffix>");
		}

		public void TestDocumentSupporterWillMergeIfEntryPrintCalled()
		{
			CheckMergeOnDocumentName("NZ Entry Print");
		}

		public void TestDocumentSupporterWillMergeIfCustomsCertificateCalled()
		{
			CheckMergeOnDocumentName("NZ Customs Certificate");
		}

		void CheckMergeOnDocumentName(ZString documentMenuTitle)
		{
			var declaration = Factory.New<JobDeclaration>();
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Factory.Save();

			var documentSupporter1 = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			StmMenuItem menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = documentMenuTitle;
			menuItem.SU_MenuPath = "Customs\\";
			menuItem.SU_FilterList = "NZ";
			AssertEquals("Declaration.MergedLines.Count", 0, declaration.CusEntryHeader.MergedLines.Count);
			documentSupporter1.GetDataStateBeforeRun(menuItem);
			AssertEquals("DocumentSupporter.GetDataStateBeforeRun(MenuItem) is called by ProcessTaskNotificationValidation. We should not change data and save during validation", 0, declaration.CusEntryHeader.MergedLines.Count);

			var args = new DocumentCancelEventArgs(menuItem);

			var documentSupporter = declaration.DocumentSupporter;
			var documentEvents = new IDocumentEventsMock();
			documentSupporter.Initialise(documentEvents);

			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals("Should be merged", 1, declaration.CusEntryHeader.MergedLines.Count);
		}

		public void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
		}

		class IDocumentEventsMock : IDocumentEvents
		{
			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(this, e);
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, e);
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;
			public void NotifyDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(this, e);
				}
			}

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, e);
				}
			}

			#endregion
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = GetNewJobDeclaration()); }
		}
		JobDeclaration declaration;

		protected virtual JobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected virtual DocumentSupporter GetDocumentSupporter()
		{
			return Declaration.DocumentSupporter;
		}

		#endregion
	}

	class JobDeclarationDocumentSupportShipmentLinkedDeclarationTest : JobDeclarationDocumentSupportStandaloneDeclarationTest
	{
		public void TestDeclarationDocumentSupporterInitialised()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "NZ Customs Certificate";
			menuItem.SU_MenuPath = "Customs\\";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Customs);
			menuItem.SU_FilterList = "NZ";

			var declaration = GetNewJobDeclaration();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var args = new DocumentCancelEventArgs(menuItem);
			var documentSupporter = Shipment.DocumentSupporter;
			var dataState = documentSupporter.GetDataStateBeforeRun(menuItem);//This happens during document printing which will instatiate JobDeclaration.DocumentSupporter.

			var mockEvent = new IDocumentEventsMock();
			documentSupporter.Initialise(mockEvent);

			mockEvent.NotifyDocumentPrintRequested(args);
			Assert("declaration is merged", declaration.IsMergeDone);
		}

		class IDocumentEventsMock : IDocumentEvents
		{
			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(this, e);
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, e);
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;
			public void NotifyDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(this, e);
				}
			}

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, e);
				}
			}

			#endregion
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration result = base.GetNewJobDeclaration();
			result.JE_JS = Shipment.PK;
			return result;
		}

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		protected override DocumentSupporter GetDocumentSupporter()
		{
			return Shipment.DocumentSupporter;
		}
	}

	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	public class JobDeclarationDocumentSupporterTest : BaseJobDeclarationDocumentSupportTest
	{
		public void TestGetDocBusinessObjectsForLandedCosting()
		{
			TestDeclaration testDec = Factory.New<TestDeclaration>();
			JobDeclarationDocumentSupporter testSupporter = new JobDeclarationDocumentSupporter(testDec);
			AssertEquals("TestDec.DoStuffCalled", false, testDec.DoStuffCalled);
			DocumentWrapper[] bOAccessed = testSupporter.GetDocumentWrappers(Core.Constants.DataContext.LandedCostHeader, null);
			AssertEquals("TestDec.DoStuffCalled", true, testDec.DoStuffCalled);
		}

		class TestDeclaration : JobDeclaration, ILandedCostHeader
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool DoStuffCalled;
			#region ILandedCostHeader Members

			void ILandedCostHeader.DoStuffBeforeRunningLCDistribution()
			{
				DoStuffCalled = true;
			}

			#endregion
		}

		public override void TestGetFilterValueMSGBKR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			//Filter = MSGBKR
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("For filter 'MSGBKR' result is 'EXP'", "EXP", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("For filter 'MSGBKR' result is 'IMP'", "IMP", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
		}

		public override void TestGetFilterValueMSGBKRCTY()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			//Filter = MSGBKRCTY
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("For filter 'MSGBKR' result is 'EXP' + Current Country/Region Code", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("For filter 'MSGBKR' result is 'IMP' + Current Country/Region Code", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}

		public override void TestGetFilterValueMSGBKRCTYMOD()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			//Filter = MSGBKRCTYMOD
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country/Region Code + \"AIR\"", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageSubType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country/Region Code + \"SEA\"", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country/Region Code + \"AIR\"", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country/Region Code + \"SEA\"", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
		}

		public void TestAutoPrintCustomsClearanceDocsTakesNoteOfBranchOfDeclaration()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);

			GlbBranch otherBranch = Factory.New<GlbBranch>();
			otherBranch.FillWithValidTestData();
			otherBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();
			NZCMessage ediMessage = declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();
			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);

			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);
			AssertEquals("EntryJobs.Count", 0, GetPrintJobs(entryQueue).Count);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);
			AssertEquals("EntryJobs.Count", 1, GetPrintJobs(entryQueue).Count);
		}

		public void TestAutoPrintCustomsClearanceDocsTakesNoteOfDepartmentOfDeclaration()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			StmPrintQueue departmentEntryQueue = GetNewPrintQueue("Departmental Entry Printer");
			GlbDepartment department = Factory.New<GlbDepartment>();
			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), department.PK.ToGuid(), departmentEntryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);
			GlbBranch otherBranch = Factory.New<GlbBranch>();
			otherBranch.FillWithValidTestData();
			otherBranch.GB_RL_NKHomePort = "NZAKL";
			department.GE_Code = "HDP";
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_GE_HomeDepartment = department.PK;
			Factory.Save();

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();
			NZCMessage ediMessage = declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();

			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);
			AssertEquals("departmentEntryQueue.Count", 1, GetPrintJobs(departmentEntryQueue).Count);
			AssertEquals("EntryJobs.Count", 0, GetPrintJobs(entryQueue).Count);
		}

		public void TestAutoPrintCustomsClearanceDocs_BothPaperAndEdocs()
		{
			var departmentEntryQueue = GetNewPrintQueue("Departmental Entry Printer");
			var department = Factory.New<GlbDepartment>();
			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), department.PK.ToGuid(), departmentEntryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);
			NZCustomsDataRegistry.Instance.DeliveryOrderCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.FillWithValidTestData();
			otherBranch.GB_RL_NKHomePort = "NZAKL";
			department.GE_Code = "HDP";
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_GE_HomeDepartment = department.PK;
			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();
			var ediMessage = declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();

			var documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true, printCCAndEP: false);
			var printJobs = GetPrintJobs(declaration);

			AssertEquals(2, printJobs.Count);

			AssertEquals("PRN", printJobs[0].SP_JobType);
			AssertEquals("DDS", printJobs[1].SP_JobType); // eDocsProcessed ?
		}

		public void TestAutoPrintCustomsClearanceDocsWorksForAllDocs()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			StmPrintQueue cusCertQueue = GetNewPrintQueue("CusCert Printer");
			StmPrintQueue dOrderQueue = GetNewPrintQueue("DOrder Printer");

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();
			declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();

			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);

			StmPrintJobCollection cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 0, cusCertJobs.Count);

			StmPrintJobCollection entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 0, entryJobs.Count);

			StmPrintJobCollection dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 0, dOrderJobs.Count);

			StmPrintJobCollection allJobs = GetPrintJobs(declaration);
			AssertEquals("allJobs.count", 3, allJobs.Count);

			NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, cusCertQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.CustomsCertificateCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);

			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 2);

			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dOrderQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 4);

			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);

			cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());

			entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());

			dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 1, dOrderJobs.Count);
			AssertEquals("DOrderJobs[0].SP_Copies", 4, dOrderJobs[0].SP_Copies.ToZInt());

			allJobs = GetPrintJobs(declaration);
			AssertEquals("allJobs.count", 9, allJobs.Count);
		}

		public void TestAutoPrintClearanceDocsWhenNoDO()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			StmPrintQueue cusCertQueue = GetNewPrintQueue("CusCert Printer");
			StmPrintQueue dOrderQueue = GetNewPrintQueue("DOrder Printer");

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();
			NZCMessage ediMessage = declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();

			TestConnection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_EDIMessage_AuditDetailsAreNotMissing_Update')
BEGIN
	DISABLE TRIGGER TG_EDIMessage_AuditDetailsAreNotMissing_Update ON EDIMessage
END");
			ediMessage.EM_SystemCreateUser = "";
			Factory.Save();

			TestConnection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_EDIMessage_AuditDetailsAreNotMissing_Update')
BEGIN
	ENABLE TRIGGER TG_EDIMessage_AuditDetailsAreNotMissing_Update ON EDIMessage
END");

			NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, cusCertQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.CustomsCertificateCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);

			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 2);

			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dOrderQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 4);

			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: false);

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());

			var dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count - No Delivery Order - should not be printed", 0, dOrderJobs.Count);
		}

		public void TestAutoEDocsOnlyCustomsClearanceDocsWorksForAllDocs()
		{
			StmPrintQueue entryQueue = GetNewPrintQueue("Entry Printer");
			StmPrintQueue cusCertQueue = GetNewPrintQueue("CusCert Printer");
			StmPrintQueue dOrderQueue = GetNewPrintQueue("DOrder Printer");

			JobDeclaration declaration = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();
			NZCMessage ediMessage = declaration.CusEntryHeader.Messages.AddNewTestTransmitMessage();
			Factory.Save();

			NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);

			NZCustomsDataRegistry.Instance.EntryPrintCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);

			NZCustomsDataRegistry.Instance.DeliveryOrderCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);

			JobDeclarationDocumentSupporter documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);

			StmPrintJobCollection cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 0, cusCertJobs.Count);

			StmPrintJobCollection entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 0, entryJobs.Count);

			StmPrintJobCollection dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 0, dOrderJobs.Count);

			StmPrintJobCollection allJobs = GetPrintJobs(declaration);
			AssertEquals("allJobs.count", 0, allJobs.Count);

			NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);

			NZCustomsDataRegistry.Instance.EntryPrintCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);

			NZCustomsDataRegistry.Instance.DeliveryOrderCopyToEDocs.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);

			documentSupporter.AutoPrintCustomsClearanceDocs(declaration.CusEntryHeader, printDO: true);

			allJobs = GetPrintJobs(declaration);
			AssertEquals("allJobs.count", 3, allJobs.Count);

			bool customsEntryFound = false;
			bool customsCertificateFound = false;
			bool deliveryOrderFound = false;
			foreach (StmPrintJob printJob in allJobs)
			{
				if (printJob.SP_EmailSubjectLine.Contains("Customs Entry for"))
				{
					customsEntryFound = true;
				}
				if (printJob.SP_EmailSubjectLine.Contains("Customs Certificate for"))
				{
					customsCertificateFound = true;
				}
				if (printJob.SP_EmailSubjectLine.Contains("Delivery Order"))
				{
					deliveryOrderFound = true;
				}
			}
			Assert("Customs Entry not found in EDocs", customsEntryFound);
			Assert("Customs Certificate not found in EDocs", customsCertificateFound);
			Assert("Delivery Order not found in EDocs", deliveryOrderFound);
		}

		public void TestDoNotCheckPreRequisiteMergeConditionsForECIWriteOff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var entry = declaration.CusEntryHeader;

			var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = "NZ Entry Print";
			menuItem.SU_MenuPath = "Customs\\";
			menuItem.SU_FilterList = "NZ";

			var dataState = documentSupporter.GetDataStateBeforeRun(menuItem);
			Assert("Should not require invoices for ECI write-off", dataState.IsValid);
		}

		protected override ZString GetMainNameSpace()
		{
			return "Enterprise.DocumentWrappers.Customs.NZ.";
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return GetDocumentSupportBusinessObjectWithLandedCosting();
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNZ");
			if (branch == null)
			{
				var company = Factory.New<GlbCompany>();
				company.GC_Code = "CNZ";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				branch = Factory.New<GlbBranch>();
				branch.GB_Code = "BNZ";
				branch.GB_GC = company.PK;
				branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				Factory.Save();
			}
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			return declaration;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return !documentCommand.SU_MenuName.StartsWith("Landed Costing") && base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;

				maxHits["EDIMessage"] = 2;

				return maxHits;
			}
		}

		StmPrintJobCollection GetPrintJobs(StmPrintQueue printQueue)
		{
			ZQuery filter = new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK);
			StmPrintJobCollection entryPrintJobs = new StmPrintJobCollection(Factory, filter);
			entryPrintJobs.Load();
			return entryPrintJobs;
		}

		StmPrintJobCollection GetPrintJobs(JobDeclaration declaration)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmPrintJobSchema.SP_ParentGuid, declaration.PK);
			StmPrintJobCollection entryPrintJobs = new StmPrintJobCollection(Factory, filter);
			entryPrintJobs.Load();
			return entryPrintJobs;
		}

		StmPrintQueue GetNewPrintQueue(ZString displayName)
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = true;
			printQueue.SQ_DisplayName = displayName;
			printQueue.SQ_QueueName = @"\\PrintServer\" + displayName;
			printQueue.SQ_PrintLanguage = "ESP";
			printQueue.SQ_Scale = 100m;
			printQueue.SQ_RowScale = 100m;
			printQueue.SQ_ColumnScale = 100m;
			printQueue.SQ_ServerName = "PRINTSERVER";
			return printQueue;
		}
	}
}
