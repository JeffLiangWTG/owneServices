using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.AES;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	public class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public void TestPrintingCustomsDeliveryOrderAndFSISForm9540()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = DocumentNames.CustomsDeliveryOrder;
			var order = declaration.DeliveryOrderHeaders.AddNew();
			order.US_OrderReference = "ORD23";
			order.US_ShouldPrint = true;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			var decSupporter = declaration.DocumentSupporter;
			var handler = decSupporter.DocumentEventsHandlers.FirstOrDefault(x => x.CanHandleMenuItem(menuItem));
			AssertNotNull("Customs Delivery Order should be able to handle", handler);
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("Cancel", true, args.Cancel);

			var supporter = shipment.DocumentSupporter;
			handler = supporter.DocumentEventsHandlers.FirstOrDefault(x => x.CanHandleMenuItem(menuItem));
			AssertNotNull("Customs Delivery Order should be able to handle", handler);
			args.Cancel = false;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("Cancel", true, args.Cancel);
		}

		public void TestGenerateQuestionsToAskUsersBeforeRunningDocumentCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = declaration.DocumentSupporter;
			var menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = DocumentNames.PPQForm368NoticeOfArrival;
			menuItem.SU_MenuPath = Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments;
			var result = supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);

			Assert("Pre-condition", !declaration.IsPPQForm368Box13Compatible);
			AssertEquals("No PPQ Data", 1, result.Count);
			declaration.US_PPQForm368Box13A = "TEST";
			Assert("Pre-condition", declaration.IsPPQForm368Box13Compatible);
			result = supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("No PPQ Data", 0, result.Count);
			menuItem.SU_MenuPath = ZString.Empty;
			result = supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem);
			AssertEquals("New PPQ Form", 1, result.Count);
		}

		#region PrintSSNTestWithSecurityRight

		public void TestPrintSSNTestWithSecurityRight()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Env.Security.USCustomsPrintSSN.IsAllowed = true;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";

				AssertEquals("Print SSN allowed", "Y", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.PrintSocialSecurityNumberAllowed));

				Env.Security.USCustomsPrintSSN.IsAllowed = false;
				AssertEquals("Print SSN not allowed", "N", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.PrintSocialSecurityNumberAllowed));
			}
		}

		#endregion

		public void TestEventTriggerWhenWhenClickOnNoticeOfItentent()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = drawback.Invoices.AddNew();

			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForExportSection = true;
			var line2 = invoice.InvoiceLines.AddNew();
			line2.US_DRWIsForExportSection = true;

			Factory.Save();

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = DocumentNames.DrawbackNoticeOfIntentMenuItem;

			EventHandler<System.ComponentModel.CancelEventArgs> func = (s, e) => e.Cancel = true;
			drawback.OnGetLinesToPrint += func;
			var dataState = drawback.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Print is cancelled", false, dataState.IsValid);
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoiceHeader = declaration.Invoices.AddNew();

			var documentWrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", "Enterprise.Customs.US.DocumentWrappers.DocDeclaration", documentWrappers[0].GetType().ToString());

			declaration.CustomsEntryHeaders.AddNew();
			documentWrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Document wrapper for data context of CusEntryHeader is of type DocDeclaration", "Enterprise.Customs.US.DocumentWrappers.DocCusEntryHeader", documentWrappers[0].GetType().ToString());

			declaration.CustomsEntryHeaders[0].CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeaderENS, null);
			AssertEquals("Data context for CusEntryHeaderENS is of type DocDeclaration", "Enterprise.Customs.US.DocumentWrappers.DocCusEntryHeader", documentWrappers[0].GetType().ToString());

			documentWrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertEquals("Document wrapper for data context of GenericCommercialInvoice for US Export is of type USExportCommercialInvoiceWrapper", "Enterprise.Customs.US.DocumentWrappers.USExportCommercialInvoiceWrapper", documentWrappers[0].GetType().ToString());

			declaration.JE_MessageType = "IMP";
			documentWrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertEquals("Document wrapper for data context of GenericCommercialInvoice for US Import is of type CommercialInvoiceWrapper", "Enterprise.DocumentWrappers.GenericWrappers.CommercialInvoiceWrapper", documentWrappers[0].GetType().ToString());
		}

		public void TestDataContext()
		{
			AssertEquals("DataContext.CusEntryHeaderENS is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeaderENS)));
		}

		public void TestFTZ214IsSupported()
		{
			var ftzAdmission = Factory.New<JobDeclaration>();
			ftzAdmission.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var ftzEntry = ftzAdmission.ActiveEntryHeaders.AddNew();
			ftzEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone;

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(ftzAdmission);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.FTZ214DataContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.FTZ214DataContextValue), null);
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestPGARecapIsSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines.US_NHTSAIndicator = "D";
			var nhtsa = invoiceLines.NHTSALines.AddNew();
			nhtsa.US_CertifyingIndividual = "A";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(declaration);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.PGARecapContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.PGARecapContextValue), null);
			AssertEquals(1, docDataProviders.Length);
			var provider = docDataProviders[0].ParentBusinessObject as DeclarationMessageDataPrint;
			var entryMessageBlockCollection = provider.EntryLineMessageBlockCollection;
			AssertEquals(1, entryMessageBlockCollection.Count());
			var printResult = new ZStringBuilder(provider.PGARecapLines).ToString();
			Assert(printResult.Contains("Entity Role: Certifying Individual   Declaration: NH1 - NHTSA"));

			declaration.US_EnableCRL = true;
			invoiceLines.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLines.ACE_FDALines.AddNew();
			fda.US_Description = "KNZ";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.PGARecapContextValue), null);
			AssertEquals(1, docDataProviders.Length);
			provider = docDataProviders[0].ParentBusinessObject as DeclarationMessageDataPrint;
			entryMessageBlockCollection = provider.EntryLineMessageBlockCollection;
			AssertEquals(1, entryMessageBlockCollection.Count());
			printResult = new ZStringBuilder(provider.PGARecapLines).ToString();
			AssertContains("Entry Line: 1", printResult);
			AssertContains("Entity Role: Certifying Individual   Declaration: NH1 - NHTSA", printResult);
			AssertContains("Commodity Description: KNZ", printResult);
		}

		public void TestFSISImportInspectionApplicationIsSupported()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var fsisLine = invoiceLine.FSISLines.AddNew();
			var fsisLot = fsisLine.Lots.AddNew();

			Factory.Save();

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(dec);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.FSISImportInspectionApplicationDataContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.FSISImportInspectionApplicationDataContextValue), null);
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestIT7512DepartureIsSupported()
		{
			var dataSourceType = new DataContextValue(JobDeclarationDocumentSupporter.IT7512DepartureDataContextValue);
			var docSupporter = Declaration.DocumentSupporter;
			AssertEquals("IT7512Departure is supported", true, docSupporter.IsDataContextSupported(dataSourceType));
			var boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("no inbond", 0, boDocDataProviders.Length);

			var ams = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			ams.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			ams.BH_ParentID = Declaration.PK;
			ams.BH_ParentTableCode = Declaration.TablePrefix;
			var amsMoveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			amsMoveHeader.BM_BH = ams.PK;

			var inbond = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = Declaration.PK;
			inbond.BH_ParentTableCode = Declaration.TablePrefix;
			var inbondMoveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			inbondMoveHeader.BM_BH = inbond.PK;
			Factory.Save();
			AssertEquals("IT7512Departure is supported", true, docSupporter.IsDataContextSupported(dataSourceType));
			boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("boDocDataProviders.Length with a InBond", 1, boDocDataProviders.Length);
		}

		public void TestCusEntryHeaderENSIsSupported()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, docDataProviders.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			jobDeclaration.US_EnableENS = true;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestCusEntryHeaderENSIsSupportedForACE()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, docDataProviders.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			jobDeclaration.US_EnableENS = false;
			jobDeclaration.US_EnableCRL = true;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestACECusEntryHeaderENSIsSupported()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			var jobDeclarationSupporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, jobDeclarationSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.ACEEntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.ACEEntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, docDataProviders.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			jobDeclaration.US_EnableENS = false;
			jobDeclaration.US_EnableCRL = true;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			docDataProviders = jobDeclarationSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.ACEEntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestCusEntryHeader7501IsSupported()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();

			JobDeclarationDocumentSupporter supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue)));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(0, result.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			jobDeclaration.US_EnableENS = true;
			CusEntryHeader entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, result.Length);
		}

		public void TestACSCusEntryHeader7501()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var supporter = new JobDeclarationDocumentSupporter(jobDeclaration);

			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be Business Object", typeof(EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>), providers[0].BusinessObjectToLogAgainst.GetType());
		}

		public void TestACECusEntryHeader7501IsSupported()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue)));

			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be Business Object", typeof(EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>), providers[0].BusinessObjectToLogAgainst.GetType());

			var entrySummaryMessage = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entrySummaryMessage.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   1104-34759790004-347597900                     081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C04-347597900                                                                 47S04-347597900                                                                 503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entrySummaryMessage.EM_MessageNum = "~15000";

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be Business Object", typeof(EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>), providers[0].BusinessObjectToLogAgainst.GetType());

			entrySummaryMessage = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entrySummaryMessage.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   1104-34759790004-347597900                     081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C04-347597900                                                                 47S04-347597900                                                                 503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";
			Factory.Save();
			entrySummaryMessage.EM_MessageNum = "~15000";

			var entrySummaryResponse = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B00                                                        B                    X0 BLOCK       1 REF ID: 8888 XJ5    AE 6009071                                 X1 FX20   FILER NOT AUTHORIZED FOR APPLICATION ID                               X1RF999   BATCH REJECTED                                                        Y           00003";
			Factory.Save();
			entrySummaryResponse.EM_MessageNum = "~15000";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be Business Object, because transmitted messages count > response messages count and response message is rejected",
						typeof(EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>), providers[0].BusinessObjectToLogAgainst.GetType());

			entrySummaryResponse = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B001101SV9AX                                               ~15000               E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 E0 LINITM 000001 REF ID: 001                                                    E0 TARIFF 000001 REF ID: 4703110000                                             E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   Y  1101SV9AX00005";
			Factory.Save();
			entrySummaryResponse.EM_MessageNum = "~15000";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			entrySummaryMessage = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();
			entrySummaryMessage.EM_MessageNum = "~15001";

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			entrySummaryResponse = CreateMessage(entryHeader.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B003902SV9AX                                               ~15000               E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984                                 E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   E0 LINITM 000001 REF ID: 001                                                    E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   E0 TARIFF 000001 REF ID: 8471704065                                             E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022098     B00153984   E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   Y  3902SV9AX00008";
			Factory.Save();
			entrySummaryResponse.EM_MessageNum = "~15001";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());
		}

		EDIMessage CreateMessage(EDIMessageCollection messages, string direction, ZString messageType)
		{
			var message = messages.AddNew(typeof(MQEDIMessage));
			message.EM_ReceiveTransmit = direction;
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = messageType;
			message.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			return (EDIMessage)message;
		}

		public void TestStandAloneCRLSIsHandled()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();

			JobDeclarationDocumentSupporter supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, result.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EnableENS = false;
			jobDeclaration.US_EnableCRL = true;
			CusEntryHeader entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, result.Length);
		}

		public void TestStandAloneBorderCargoReleaseIsAlsoHandled()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();

			JobDeclarationDocumentSupporter supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, result.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EnableENS = false;
			jobDeclaration.US_EnableCRL = true;
			CusEntryHeader entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;

			result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, result.Length);
		}

		public void TestMenuWontRunWhenNo7501Exists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supporter = new JobDeclarationDocumentSupporter(declaration);

			var lcMenu = Factory.New<StmMenuItem>();
			lcMenu.SU_MenuName = Customs.Business.BaseJobDeclarationDocumentSupporter.LandedCostingMenuText;
			AssertEquals(false, supporter.GetDataStateBeforeRun(lcMenu).IsValid);
			AssertEquals(JobDeclarationDocumentSupporter.No7501EntryExist, supporter.GetDataStateBeforeRun(lcMenu).ErrorMessage);

			var fdaRecapMenu = Factory.New<StmMenuItem>();
			fdaRecapMenu.SU_MenuName = DocumentNames.FDARecap;
			AssertEquals(false, supporter.GetDataStateBeforeRun(fdaRecapMenu).IsValid);
			AssertEquals(JobDeclarationDocumentSupporter.No7501EntryExist, supporter.GetDataStateBeforeRun(fdaRecapMenu).ErrorMessage);

			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(JobDeclarationDocumentSupporter.No7501EntryExist, supporter.GetDataStateBeforeRun(lcMenu).ErrorMessage);
			AssertNotEquals(JobDeclarationDocumentSupporter.No7501EntryExist, supporter.GetDataStateBeforeRun(fdaRecapMenu).ErrorMessage);
		}

		public void TestMenuXXXPrintSSN()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			declaration.Logs.AddNew(ZArchitecture.Business.Events.DataImport, "BR:7501");
			AssertEquals("PreCondition", true, declaration.CreatedViaBIRD("7501"));

			var supporter = new JobDeclarationDocumentSupporter(declaration);

			StmMenuItem menuEntrySummary = Factory.New<StmMenuItem>();
			menuEntrySummary.SU_MenuName = "7501 Entry Summary";
			supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), menuEntrySummary);
			AssertEquals("XXX menu", false, declaration.PrintSocialSecurityNumberOnDocument);

			StmMenuItem menuEntrySummarySSN = Factory.New<StmMenuItem>();
			menuEntrySummarySSN.SU_MenuName = "7501 Entry Summary (SSN)";
			supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), menuEntrySummarySSN);
			AssertEquals("XXX menu (SSN)", true, declaration.PrintSocialSecurityNumberOnDocument);
		}

		public void Test7501EntrySummaryNotSupportedForIMXJobsCreatedViaBIRD()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			declaration.Logs.AddNew(ZArchitecture.Business.Events.DataImport, "BR:7501");
			AssertEquals("PreCondition", true, declaration.CreatedViaBIRD("7501"));

			JobDeclarationDocumentSupporter supporter = new JobDeclarationDocumentSupporter(declaration);

			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "7501 Entry Summary";
			AssertEquals(false, supporter.GetDataStateBeforeRun(menu).IsValid);
			AssertEquals(JobDeclarationDocumentSupporter.YouCannotPrint7501ForJobsDoneByExternalBroker, supporter.GetDataStateBeforeRun(menu).ErrorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, supporter.GetDataStateBeforeRun(menu).IsValid);
		}

		public void TestDataSourceForENSEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			AssertEquals("Precondition: ENS Entry is null", null, declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var lcMenu = Factory.New<StmMenuItem>();
			lcMenu.SU_MenuName = DocumentNames.FDARecap;

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.ENSEntryHeader), lcMenu);
			AssertEquals(@"Should return 0 providers if ENS Entry is null. Otherwise, if we will return null object, 
				it will cause null reference exception in Document Engine", 0, providers.Length);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			supporter = new JobDeclarationDocumentSupporter(declaration);

			lcMenu = Factory.New<StmMenuItem>();
			lcMenu.SU_MenuName = Customs.Business.BaseJobDeclarationDocumentSupporter.LandedCostingMenuText;

			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.ENSEntryHeader), lcMenu);

			AssertEquals(1, providers.Length);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, providers[0].ParentBusinessObject);
		}

		public void TestCRLSIsUsedFor3461Print()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();

			JobDeclarationDocumentSupporter supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue)));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(0, result.Length);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			jobDeclaration.US_EnableENS = true;
			jobDeclaration.US_EnableCRL = true;
			CusEntryHeader entryENSHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryENSHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader entryCRLHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryCRLHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			result = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntryImmediateDeliveryDataContextValue), null);
			AssertEquals(1, result.Length);
			AssertEquals("Business object to base document on should be Cargo Release entry", entryCRLHeader.PK, result[0].ParentBusinessObject.PK);
		}

		public void TestCusEntryHeaderAESIsSupported()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals("AES Data Context supported", true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue)));

			var docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue), null);
			AssertEquals("No Doc Data Providers found for this declaration", 0, docDataProviders.Length);

			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;

			docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue), null);
			AssertEquals("One Doc Data Provider exists for declaration", 1, docDataProviders.Length);
			AssertEquals("Data Provider Business Object Type", typeof(AESHeaderPrint), docDataProviders[0].BusinessObjectToLogAgainst.GetType());
			AssertEquals("No Commodity Lines created for AES Header", 0, ((AESHeaderPrint)docDataProviders[0].BusinessObjectToLogAgainst).Commodities.Count);

			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			var outgoingAESTIRMessage = Factory.New<AESTIREDIMessage>();
			outgoingAESTIRMessage.EM_Status = MQEDIMessage.Status.Sent;
			outgoingAESTIRMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingAESTIRMessage);

			var responseAESTIRMessage = Factory.New<AESTIREDIMessage>();
			responseAESTIRMessage.EM_Status = MQEDIMessage.Status.Received;
			responseAESTIRMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			entryHeader.Messages.Add(responseAESTIRMessage);

			docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue), null);
			AssertEquals("One Doc Data Provider exists for declaration", 1, docDataProviders.Length);
			AssertEquals("Data Provider Business Object Type", typeof(AESHeaderPrint), docDataProviders[0].BusinessObjectToLogAgainst.GetType());
			AssertEquals("No Commodity Lines created for AES Header", 0, ((AESHeaderPrint)docDataProviders[0].BusinessObjectToLogAgainst).Commodities.Count);
		}

		public void TestAESMessagesDataProviderSupportedForClearedEntry()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var supporter = new JobDeclarationDocumentSupporter(jobDeclaration);
			AssertEquals("AES Data Context supported", true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue)));

			var docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue), null);
			AssertEquals("No Doc Data Providers for declaration", 0, docDataProviders.Length);

			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			var outgoingAESTIRMessage = Factory.New<AESTIREDIMessage>();
			outgoingAESTIRMessage.EM_Status = MQEDIMessage.Status.Sent;
			outgoingAESTIRMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Transmit;
			outgoingAESTIRMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			outgoingAESTIRMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingAESTIRMessage.EM_MessageText = "B  91013199000E          ABC EXPORTS USA                                        SC1N11HKILKKLUB00153967        AKAGA                   2 58201270920100817 N    SC270                                   N                                       SC3                             OB903847                                        SC3APLU0398476   K9378                                                          N0191013199000EEABC EXPORTS USA               GARY         ODEA                 N02ALTERNATIVE PICKUP ADDRESS                                      6452535520   N03MADISON                  WIUS53562                                           N0156999999900EFCARGOWISE INC                                                   N021699 WALL STREET                                                8475551212   N03MOUNTPROSPECT            ILUS60056                                           N01            CMUSIC TRADING ONLINE                                           NN0214TH FLOOR, LU PLAZA            2 WING YUP STREET, KWUN TONG                 N03HONGKONG                   HK                                                CL1OS 0001MISC HABERDASERY ITEMS                                  AC33D         CL26302600020NO 00000029400000117590KG 00000003960000000400     NLR             Y  91013199000E          ABC EXPORTS USA";
			entryHeader.Messages.Add(outgoingAESTIRMessage);

			var responseAESTIRMessage = Factory.New<AESTIREDIMessage>();
			responseAESTIRMessage.EM_Status = MQEDIMessage.Status.Received;
			responseAESTIRMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			responseAESTIRMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			responseAESTIRMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entryHeader.Messages.Add(responseAESTIRMessage);

			docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.AESDataContextValue), null);
			AssertEquals("Should be 1 Doc Data Provider", 1, docDataProviders.Length);
			AssertEquals("Data Provider Business Object Type", typeof(AESMessagePrint), docDataProviders[0].BusinessObjectToLogAgainst.GetType());
			AssertEquals("One Commodity Line created from AES TIR Message", 1, ((AESMessagePrint)docDataProviders[0].BusinessObjectToLogAgainst).Commodities.Count);
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			if (command.SU_MenuName.Contains(DocumentNames.CustomsDeliveryOrder))
			{
				var deliveryOrderHeader = ((JobDeclaration)businessObject).DeliveryOrderHeaders.AddNew();
				deliveryOrderHeader.US_DeliveryInstructions = "INSTRUCTION";
				deliveryOrderHeader.US_ShouldPrint = true;
				var deliveryOrderLine = deliveryOrderHeader.DeliveryOrderLines.AddNew();
				deliveryOrderLine.US_GoodsDescription = "GOODS";
			}
			else if (command.SU_MenuName.Contains("7512 Departure"))
			{
				var declaration = (JobDeclaration)businessObject;
				var factory = declaration.Factory;
				var inbond = factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
				inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
				inbond.BH_ParentID = declaration.PK;
				inbond.BH_ParentTableCode = declaration.TablePrefix;
				var inbondMoveHeader = factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
				inbondMoveHeader.BM_BH = inbond.PK;
			}
			else
			{
				base.DoSetupForDocument(command, businessObject);
			}
		}

		protected override void RemoveMessageTypesNotInvolvedInTesting(CodeDescriptionPairList messageTypeList, ZString msgType)
		{
			messageTypeList.RemoveCode(JobMessageTypeList.Codes.Miscellaneous);
			messageTypeList.RemoveCode(JobMessageTypeList.Codes.ImportByExternalBroker);
			base.RemoveMessageTypesNotInvolvedInTesting(messageTypeList, msgType);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = (JobDeclaration)base.GetDocumentSupportableBusinessObject();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_EnableAII = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EnableINB = true;
			declaration.ImportEntryNumber = "ENT234322";

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "Bill1";

			var fsisLine = declaration.Invoices[0].InvoiceLines[0].FSISLines.AddNew();
			var fsisLot = fsisLine.Lots.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = entry.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText =
				"B011101SV9AE                                               HYEDUSCMT_162367     " +
				"OI        YUMMY TUNA                                                            " +
				"PG01001NMF370YFTYY                                                              " +
				"PG02P                                                                           " +
				"Y  1101SV9AE00015";

			return declaration;
		}

		protected override void MakeDeclarationExWarehouse(Customs.Business.BaseJobDeclaration baseDeclaration)
		{
			base.MakeDeclarationExWarehouse(baseDeclaration);

			var declaration = (JobDeclaration)baseDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith(Customs.Business.BaseJobDeclarationDocumentSupporter.LandedCostingMenuText))
			{
				return false;//Do not exclude Landed Costing for US
			}
			return base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;

				maxHits["EDIMessage"] = 4;
				maxHits["RefDbEntUS_USCScheduleB"] = 3;
				maxHits["RefDbEntUS_USCTariff"] = 6;
				maxHits["RefDatabase_RefSysConfig"] = 3;
				maxHits["RefDatabase_RefCusTaxOrFee"] = 2;
				maxHits["RefDatabase_RefCusCodeListLanguage"] = 3;
				maxHits["ZZRefCusCodeListCombined"] = 4;
				maxHits["TariffView"] = 3;
				maxHits["CusInBondHeader"] = 3;

				return maxHits;
			}
		}

		public void TestENSWithACECargoReleaseBLU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var entrySummary = CreateMessage(entry.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entrySummary.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_149008     10ASV9  71003428 1101B00160988   0140 XY          2112913                       1113-14792700013-147927000                     111813       TX                  20AA  1101111813A001                                                            21008U                                                                          2200000007PC                                                                    23M    52937373733                                                              318B 037                                                                        40  001 CHCH111813                       0000000045    Y                        47MCHHARWIN8PLA                                                                 5098010040   0000000000 0000000100                                              508703105060 0000000000 0000000100 000000001000NO                               9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			entrySummary.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-10);
			Factory.Save();
			entrySummary.EM_MessageNum = "HYEDUSCMT_149008";

			var entrySummaryResponse = CreateMessage(entry.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_149008     E0 SUMMRY 000001 REF ID: SV9 71003428 B00160988                                 E0 LINITM 000001 REF ID: 001                                                    E1 I628020DUTY ACCEPTED; COMPLEX LINE             SV9  71003428     B00160988   E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7100342800100B00160988   Y  1101SV9AX00004";
			entrySummaryResponse.EM_MessageNum = "HYEDUSCMT_149008";
			entrySummaryResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-9);

			var legacyBLU = CreateMessage(declaration.Messages, EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate);
			legacyBLU.EM_MessageText = "B018888XJ5LN                                               33439                L18888XJ5 70021452N            KKLU334  072709X111                              L3             KKLUSUPERCALA                                   00000050CT       Y  8888XJ5LN00002";
			legacyBLU.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-8);
			Factory.Save();
			legacyBLU.EM_MessageNum = "33439";

			var legacyBLUResponse = CreateMessage(declaration.Messages, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse);
			legacyBLUResponse.EM_MessageText = "B018888XJ5LS                                               33439                L18888XJ5 70021452N            KKLU334  072709X111                              L78888XJ5 700214528WANO ENTRY EXISTS OR ENTRY CLOSED                            L78888XJ5 700214528WBENTRY BELONGS TO ANOTHER DD/PP                             L78888XJ5 70021452524TRANSACTION DATA REJECTED                                  Y  8888XJ5LS00004";
			legacyBLUResponse.EM_MessageNum = "33439";
			legacyBLUResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-7);

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			var printBO = (ACEEntryMessage7501Print)providers[0].ParentBusinessObject;
			AssertEquals("Should be printed from entry summary message, because BLU message rejected", "52937373733", printBO.SCACAndMBillNumber);

			var legacyBLU2 = CreateMessage(declaration.Messages, EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.BillofLadingUpdate);
			legacyBLU2.EM_MessageText = "B018888XJ5LN                                               33442                L18888XJ5 70021452N            KKLU334  072709X111                              L3             KKLUSUPERCALA                                   00000050CT       Y  8888XJ5LN00002";
			legacyBLU2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-6);
			Factory.Save();
			legacyBLU2.EM_MessageNum = "33442";

			var legacyBLUResponse2 = CreateMessage(declaration.Messages, EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse);
			legacyBLUResponse2.EM_MessageText = "B018888XJ5LS                                               33442                L78888XJ5 700214528VZBILL DATA UPDATED AS REQUESTED                             Y  8888XJ5LS00001";
			legacyBLUResponse2.EM_MessageNum = "33442";
			legacyBLUResponse2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-5);

			supporter = new JobDeclarationDocumentSupporter(declaration);
			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			printBO = (ACEEntryMessage7501Print)providers[0].ParentBusinessObject;
			AssertEquals("Should be printed from latest accepted legacy BLU message", "KKLUSUPERCALA", printBO.SCACAndMBillNumber);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var message2 = CreateMessage(seEntry.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
			message2.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148662     SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE20CR B00160830                                                                Y  1101SV9SE00003";
			message2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-4);
			message2.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			Factory.Save();
			message2.EM_MessageNum = "HYEDUSCMT_148662";

			var response2 = CreateMessage(seEntry.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			response2.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00003";
			response2.EM_MessageNum = "HYEDUSCMT_148662";
			response2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-3);

			var statusNotification = CreateMessage(seEntry.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			statusNotification.EM_MessageText = "B001101SV9SO                                              HYEDUSCMT_148662      SO101101SV9  71002677 0123-456789012                                   1        SO40     APLUFSDFS324234                                   00000015CS   00000000SO50040814004691NO BILL MATCH                                                   Y  1101SV9SO00000";
			statusNotification.EM_MessageNum = "HYEDUSCMT_148662";
			statusNotification.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);

			AssertEquals("We've seen the case when Customs sent Status Notification Message with the same message number as ACE Cargo Release Update. However, Message.ResponseMessage should be Cargo Release Response 'SX' message",
				ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse, message2.ResponseMessage.EM_MessageType);

			supporter = new JobDeclarationDocumentSupporter(declaration);
			providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			printBO = (ACEEntryMessage7501Print)providers[0].ParentBusinessObject;
			AssertEquals("Should be printed from latest ACE BLU message", "ALP61325205", printBO.SCACAndMBillNumber);
		}

		public void TestENSWithACECargoReleaseBLU_2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "SV9";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var aceBLUMessage = CreateMessage(declaration.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.CargoRelease);
			aceBLUMessage.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148662     SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE20CR B00160830                                                                Y  1101SV9SE00003";
			aceBLUMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-4);
			Factory.Save();
			aceBLUMessage.EM_MessageNum = "HYEDUSCMT_148662";

			var aceBLUResponse = CreateMessage(declaration.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			aceBLUResponse.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00003";
			aceBLUResponse.EM_MessageNum = "HYEDUSCMT_148662";
			aceBLUResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-3);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var entrySummary = CreateMessage(entry.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entrySummary.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_149008     10ASV9  71003428 1101B00160988   0140 XY          2112913                       1113-14792700013-147927000                     111813       TX                  20AA  1101111813A001                                                            21008U                                                                          2200000007PC                                                                    23M    52937373733                                                              318B 037                                                                        40  001 CHCH111813                       0000000045    Y                        47MCHHARWIN8PLA                                                                 5098010040   0000000000 0000000100                                              508703105060 0000000000 0000000100 000000001000NO                               9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			entrySummary.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-10);
			Factory.Save();
			entrySummary.EM_MessageNum = "HYEDUSCMT_149008";

			var entrySummaryResponse = CreateMessage(entry.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_149008     E0 SUMMRY 000001 REF ID: SV9 71003428 B00160988                                 E0 LINITM 000001 REF ID: 001                                                    E1 I628020DUTY ACCEPTED; COMPLEX LINE             SV9  71003428     B00160988   E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7100342800100B00160988   Y  1101SV9AX00004";
			entrySummaryResponse.EM_MessageNum = "HYEDUSCMT_149008";
			entrySummaryResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-9);

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			AssertEquals("Data Provider should be ACE ENS Message", typeof(ACEEntryMessage7501Print), providers[0].BusinessObjectToLogAgainst.GetType());

			var printBO = (ACEEntryMessage7501Print)providers[0].ParentBusinessObject;
			AssertEquals("Should be printed from entry summary message, because latest accepted BLU message has been sent before ENS message", "52937373733", printBO.SCACAndMBillNumber);
		}

		public void TestENSWhenJobComInvoiceDetached()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MASTER";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entrySummary = CreateMessage(entry.Messages, EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			entrySummary.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_149008     10ASV9  71003428 1101B00160988   0140 XY          2112913                       1113-14792700013-147927000                     111813       TX                  20AA  1101111813A001                                                            21008U                                                                          2200000007PC                                                                    23M    52937373733                                                              318B 037                                                                        40  001 CHCH111813                       0000000045    Y                        47MCHHARWIN8PLA                                                                 5098010040   0000000000 0000000100                                              508703105060 0000000000 0000000100 000000001000NO                               9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			entrySummary.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			Factory.Save();
			entrySummary.EM_MessageNum = "HYEDUSCMT_149008";

			var entrySummaryResponse = CreateMessage(entry.Messages, EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse);
			entrySummaryResponse.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_149008     E0 SUMMRY 000001 REF ID: SV9 71003428 B00160988                                 E0 LINITM 000001 REF ID: 001                                                    E1 I628020DUTY ACCEPTED; COMPLEX LINE             SV9  71003428     B00160988   E1AI995   SUMMARY HAS BEEN ADDED                  SV9  7100342800100B00160988   Y  1101SV9AX00004";
			entrySummaryResponse.EM_MessageNum = "HYEDUSCMT_149008";
			entrySummaryResponse.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);

			var printing1 = entry.US7501DocPrintingData.AddNew();
			printing1.US_LineNo = 1;
			printing1.US_InvoicePK = invoice1.PK;
			printing1.US_MsgPK = entrySummary.PK;

			invoice1.CleanUpInvoiceAfterDetachedOrDeleted("Detached");
			declaration.Invoices.RemoveFromRelationship(invoice1);

			Factory.Save();

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);

			var newPrintBO = (EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>)providers[0].ParentBusinessObject;
			AssertEquals("Print from Business Object when invoice is detahced.", "APLUMASTER", newPrintBO.SCACAndMBillNumber);
		}

		public void TestBillQuantityForSplitShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "73000273";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBillIssuerSCAC = "A2";
			declaration.JE_MasterBill = "00112612224";

			var bill = declaration.Bills.PrimaryMasterBill;
			bill.US_SESplitShip = true;
			bill.CU_NoOfPacks = 10;
			bill.CU_PackType = "CS";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var outgoing7501 = Factory.New<MQEDIMessage>();
			outgoing7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing7501.EM_MessageNum = "HYEDUSCMT_199790";
			outgoing7501.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_199790     10ASV9  73000273 1101B00174214   0140 XA      001 3011519         Y             1158-12345678958-123456789                     010319       TN                  20A2  1101010319B815                                                            SE13IAN CHEN                                0412096252                          21001A                                                                          2200000010CS                                                                    23M    00112612224                                                              318B 891                                                                        SE30SE ACE TEST SUPPLIER HK                                                     SE3515172 GLOUCESTER ROAD                15WAN CHAI DISTRICT                    SE36HONG KONG                                                  HK               SE30ST ACE TEST IMPORTER 1                                                      SE3515123 MADISON AVE                                                           SE36NEW YORK                                    10016          US               40  001 HKHK122918        0000001000     0000000500    N                        47MHKACETES172HON                                                               47S58-123456789                                                                 47EHKACETES172HON                                                               SE50MF ACE TEST SUPPLIER HK                                                     SE5515172 GLOUCESTER ROAD                15WAN CHAI DISTRICT                    SE56HONG KONG                                                  HK               500302420000 0000000000 0000010000 000000100000KG                               OI        IAN TEST SPLIT                                                        PG01001FDAFOONSF                                                                PG02PFDP 16AGC01                                                                PG06262HK                                                                       PG06CSHHK                                                                       PG10                   FRESH FISH                                               PG19FDC                  ACE TEST SUPPLIER HK            172 GLOUCESTER ROAD    PG20WAN CHAI DISTRICT                    HONG KONG               HK             PG19DEQ                  ACE TEST SUPPLIER HK            172 GLOUCESTER ROAD    PG20WAN CHAI DISTRICT                    HONG KONG               HK             PG19FD116 129090909      ACE TEST IMPORTER 1             123 MADISON AVE        PG20                                     NEW YORK             NY US10016        PG21FD1IAN CHOI               13035551212    IAN.CHOI@WISETECHGLOBAL.COM        PG19UC 16 129090909      ACE TEST IMPORTER 1             123 MADISON AVE        PG20                                     NEW YORK             NY US10016        PG19PNS16 129090909      ACE TEST IMPORTER 1             1 EXECUTIVE DR 2ND FL  PG20SUITE 200                            CHELMSFORD           MA US01824        PG21PNSIAN CHOI               13035551212    IAN.CHOI@WISETECHGLOBAL.COM        PG19PNT                  USA LOGISTICS COMPANY           1501 WOODFIELD RD      PG20                                     SCHAUMBURG           IL US60173        PG21PNTUS CHICAGO FACILITATOR 3125551212     ENTERPRISE_CMT@CARGOWISE.COM       PG19DFP                  ACE TEST SUPPLIER HK            172 GLOUCESTER ROAD    PG20WAN CHAI DISTRICT                    HONG KONG               HK             PG19PK                   USA LOGISTICS COMPANY           1501 WOODFIELD RD      PG20                                     SCHAUMBURG           IL US60173        PG21PK US CHICAGO FACILITATOR 3125551212     ENTERPRISE_CMT@CARGOWISE.COM       PG23VFT  001A                                                                   PG23CFR  19148237698                                                            PG25                                                    000000010000            PG261000000008000KG                                                             PG30A0103201901002   1101                                                       6249900003464                                                                   8949900000003464                                                                9000000000000 00000003464 00000000000 00000000000 00000000000 00000000000       Y  1101SV9AE";
			outgoing7501.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entrySummaryEntry.Messages.Add(outgoing7501);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_MessageNum = "HYEDUSCMT_199790";
			incoming7501.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_199790     E0 SUMMRY 000001 REF ID: SV9 73000273 B00174214    171                          E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7300027300100B00174214   Y  1101SV9AX00002";
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryEntry.Messages.Add(incoming7501);
			entrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var cargoReleaseEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var cargoReleaseUpdateOutgoing = Factory.New<MQEDIMessage>();
			cargoReleaseUpdateOutgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseUpdateOutgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			cargoReleaseUpdateOutgoing.EM_MessageNum = "HYEDUSCMT_199792";
			cargoReleaseUpdateOutgoing.EM_MessageText = "B  1101SV9SE                                               HYEDUSCMT_199792     SE10USV9  73000273 01EI 58-12345678940800000100001101  1101                     SE11       B815                        001A                                     SE13IAN CHEN                                0412096252         1                SE15R    00112612224                                                    N       SE20CR B00174214                                                                SE20KIIY                                                                        Y  1101SV9SE";
			cargoReleaseUpdateOutgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			cargoReleaseEntry.Messages.Add(cargoReleaseUpdateOutgoing);

			var cargoReleaseUpdateResponse = Factory.New<MQEDIMessage>();
			cargoReleaseUpdateResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cargoReleaseUpdateResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			cargoReleaseUpdateResponse.EM_MessageNum = "HYEDUSCMT_199792";
			cargoReleaseUpdateResponse.EM_MessageText = "B001101SV9SX                                               HYEDUSCMT_199792     SE10USV9  73000273 01EI 58 - 12345678940800000100001101  1101SE15R    00112612224                                                    NSE20CR B00174214SE9002   SE DATA ACCEPTEDY  1101SV9SX00004";
			cargoReleaseUpdateResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseEntry.Messages.Add(cargoReleaseUpdateResponse);
			cargoReleaseEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseUpdate;

			var supporter = new JobDeclarationDocumentSupporter(declaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue)));

			var providers = supporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.EntrySummary7501DataContextValue), null);
			AssertEquals(1, providers.Length);
			var printBO = (ACEEntryMessage7501Print)providers[0].ParentBusinessObject;
			AssertEquals(1, printBO.EntryPrintBills.Count);
			var bill1 = printBO.EntryPrintBills[0];
			AssertEquals(ZString.Empty, bill1.ITNO);
			AssertEquals("00112612224", bill1.MasterBill);
			AssertEquals(ZString.Empty, bill1.HouseBill);
			AssertEquals(ZString.Empty, bill1.SubHouseBill);
			AssertEquals(10, bill1.PkgQty);
			AssertEquals("CS", bill1.PkgType);
		}

		public new void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Rail, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
