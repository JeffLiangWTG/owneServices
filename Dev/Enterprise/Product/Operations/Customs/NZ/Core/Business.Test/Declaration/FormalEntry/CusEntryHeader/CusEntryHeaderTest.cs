using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderBaseOnlyTest : CusEntryHeaderTest<CusEntryHeader>
	{
	}

	public abstract class CusEntryHeaderTest<T> : Declaration.Testing.CusEntryHeaderTest<T>
		where T : CusEntryHeader
	{
		public void TestIntegrateIfNecessary_KeepResultOfAccountingIntegrationConsistentWithAutoRating_WI00256676()
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification { SendGroupPK = @group.PK };
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "44051192";
			entry.CH_MessageType = EntryMessageTypeList.Codes.FormalEntry;
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 7.27m);
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFee, 48.44m);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GST, 600m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());

			// AutoRating
			new AutoRatingStarter(declaration, null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Invoicing));

			var job = new Job.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 655.71m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", "", charges[0].JR_APInvoiceNum);
				Assert("Cost should be not posted", !charges[0].IsCostPosted);
				Assert("Revenue should be not posted", !charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
  Entry Fee                                  48.44
  + GST                                       7.27
  GST                                       600.00", charges[0].JR_Desc);
			});

			// Post All Revenue Charges
			charges[0].JR_APInvoiceNum = "44051192";
			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 655.71m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", "44051192", charges[0].JR_APInvoiceNum);
				Assert("Cost should be not posted", !charges[0].IsCostPosted);
				Assert("Revenue should be not posted", charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
  Entry Fee                                  48.44
  + GST                                       7.27
  GST                                       600.00", charges[0].JR_Desc);
			});

			declaration.Reload();

			//trigger Accounting Integration
			entry.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			Factory.Save();

			job = new Job.Loader(declaration).Load();
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 655.71m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", "44051192", charges[0].JR_APInvoiceNum);
				Assert("Cost should have been posted", charges[0].IsCostPosted);
				Assert("Revenue should have been posted", charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
  Entry Fee                                  48.44
  + GST                                       7.27
  GST                                       600.00", charges[0].JR_Desc);
			});
		}

		public void TestGetUniqueNumberForAccountingIntegration() => CombineAssertions(() =>
		{
			EntryHeader.EntryNumber = "28578139";
			EntryHeader.CH_ConsolidatedEntryMemberID = 1;
			AssertEquals("UniqueNumber for non-consolidated entry", "28578139", ((IAccInvoiceDataProvider)EntryHeader).UniqueNumber);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.CusEntryHeader;
			AssertEquals("Consolidated entry doesn't have EntryNumber", "", ((IAccInvoiceDataProvider)leadDeclarationEntryHeader).UniqueNumber);

			leadDeclarationEntryHeader.EntryNumber = "28578139";
			leadDeclarationEntryHeader.CH_ConsolidatedEntryMemberID = 1;
			AssertEquals("UniqueNumber for consolidated entry", "28578139-1", ((IAccInvoiceDataProvider)leadDeclarationEntryHeader).UniqueNumber);
		});

		public void TestIntegrateIfNecessary()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = (CusEntryHeader)declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "44051192";
			AssertCostAndRevenuePostedByAccountingIntegration(entry, "44051192");
		}

		public void TestIntegrateIfNecessary_ConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var entry = (CusEntryHeader)declaration.CusEntryHeader;
			entry.EntryNumber = "44051192";
			entry.CH_ConsolidatedEntryMemberID = 1;
			AssertEquals("Declaration is consolidated", true, ConsolidatedDeclaration.IsConsolidated(declaration));
			AssertCostAndRevenuePostedByAccountingIntegration(entry, "44051192-1");
		}

		void AssertCostAndRevenuePostedByAccountingIntegration(CusEntryHeader entry, ZString expectedAPInvoiceNum)
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification { SendGroupPK = @group.PK };
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = entry.Declaration;
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			entry.CH_MessageType = EntryMessageTypeList.Codes.FormalEntry;
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 7.27m);
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFee, 48.44m);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GST, 600m);
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());

			//trigger Accounting Integration
			entry.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			Factory.Save();

			var job = new Job.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 655.71m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", expectedAPInvoiceNum, charges[0].JR_APInvoiceNum);
				Assert("Cost should have been posted", charges[0].IsCostPosted);
				Assert("Revenue should have been posted", charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
Current Amounts
  Entry Fee                                  48.44
  + GST                                       7.27
  GST                                       600.00", charges[0].JR_Desc);
			});
		}

		public void TestIntegrateIfNecessary_OnlyEntryFeeGST_AccountingIntegration()
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification { SendGroupPK = @group.PK };
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "44051192";
			entry.CH_MessageType = EntryMessageTypeList.Codes.FormalEntry;
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 7.27m);
			var entryLine = entry.MergedLines.AddNew();
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());

			//trigger Accounting Integration
			entry.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
			Factory.Save();

			var job = new Job.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 0m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", "", charges[0].JR_APInvoiceNum);
				Assert("Cost should have been posted", !charges[0].IsCostPosted);
				Assert("Revenue should have been posted", !charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
  + GST                                       7.27", charges[0].JR_Desc);
			});
		}

		public void TestIntegrateIfNecessary_OnlyEntryFeeGST_AutoRating()
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification { SendGroupPK = group.PK };
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<JobDeclaration>();
			var importer = testHelper.Importer;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "44051192";
			entry.CH_MessageType = EntryMessageTypeList.Codes.FormalEntry;
			entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 7.27m);
			var entryLine = entry.MergedLines.AddNew();
			Factory.Save();
			AssertNull("No invoicing job should have been created as not lodged yet", new JobHeader.Loader(declaration).Load());

			declaration.Reload();
			// AutoRating
			new AutoRatingStarter(declaration, null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue.With(billingType: BillingType.Invoicing));
			Factory.Save();

			var job = new Job.Loader(declaration).Load();
			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			CombineAssertions(() =>
			{
				AssertEquals("one charge", 1, charges.Length);
				AssertEquals("Charge amount", 0m, charges[0].JR_LocalCostAmt);
				AssertEquals("Invoice No", "", charges[0].JR_APInvoiceNum);
				Assert("Cost should be not posted", !charges[0].IsCostPosted);
				Assert("Revenue should be not posted", !charges[0].IsRevenuePosted);
				AssertEquals(@"Customs Disbursements
  + GST                                       7.27", charges[0].JR_Desc);
			});
		}

		public void TestSyntheticGreenhouseGasesLevyAmount()
		{
			var entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.SyntheticGreenhouseGasesLevyAmount = 10m;

			entryLine = EntryHeader.MergedLines.AddNew();
			entryLine.SyntheticGreenhouseGasesLevyAmount = 20m;

			AssertEquals(30m, EntryHeader.SyntheticGreenhouseGasesLevyAmount);
		}

		public void TestLastCustomsStatusIsImpediment()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", true, EntryHeader.LastCustomsStatusIsImpediment);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", false, EntryHeader.LastCustomsStatusIsImpediment);
		}

		public void TestEntryLinesExistWithoutFreight()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = currentUser.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "WTFZOMG";
			CurrentUsersPin usersPin = new CurrentUsersPin(Factory, false);
			usersPin.DecryptedPinCode = "CandyMountainAdventure";
			Factory.Save();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CandyMountainAdventure");

			JobDeclaration jobDec = JobDeclaration.New(Factory);

			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(jobDec);

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			ErrorReporter.Clear();
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var header = jobDec.CusEntryHeader as CusEntryHeader;

			Assert("Should start with no freight", header.EntryLinesExistWithoutFreight);
			jobDec.FilteredInvoiceLines[0].Charges.AddNew(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 5m);
			Assert("Now we have freight!", !header.EntryLinesExistWithoutFreight);
		}

		public virtual void TestSetDefaultValues()
		{
			AssertEquals("EntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, EntryHeader.CH_EntryStatus);
			AssertEquals("EntryHeader.CH_MessageType", ExpectedCusEntryHeaderType, EntryHeader.CH_MessageType);
		}

		public void TestSettingEntryNumberSetsRightType()
		{
			EntryHeader.EntryNumber = "NZ111";
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, EntryHeader.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, ExpectedCusEntryNumType);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber[] entryNums = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), filter);
			AssertEquals("One entry number", 1, entryNums.Length);
			AssertEquals("Entry number", "NZ111", entryNums[0].CE_EntryNum);
		}

		public void TestPackageCountIsOverriddenToReturnTotalNumberOfPackagesFromPAckagesCollection()
		{
			Customs.Business.BasePackage package1 = Declaration.Packages.AddNew();
			package1.CW_PackQty = 11;
			Customs.Business.BasePackage package2 = Declaration.Packages.AddNew();
			package2.CW_PackQty = 13;
			AssertEquals("EntryHeader.Packages", 24, EntryHeader.PackagesCount);
		}

		public void TestIDocManagerSupportWillFindeDocAttachedToDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <PackingList> %EOF\n"), "PackingList.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <Invoice> %EOF\n"), "Invoice.pdf", "INV");
			AssertEquals("Shipment documents", 2, shipment.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Declaration & Shipment respective document collections should now be available for selection in message sending attachment drop down control", 2, declaration.eDocsForSelection.Count);

			int docsInCollection = 0;
			foreach (IStorageDocsBaseCollection docCollection in declaration.eDocsForSelection)
			{
				foreach (IeDoc doc in docCollection)
				{
					docsInCollection++;
				}
			}

			AssertEquals("There should be 3 documents in total available for selection", 3, docsInCollection);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;

			var additionalMessageInformation = new TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[0];
			IeDoc attachment = documentCollection[0];
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = cusAttachment.FileName;
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			var attachedDoc = messageAttached.GetAttachment();
			AssertEquals("Should find document attached to declaration that was selected for inclusion with this message", attachment, attachedDoc);
			AssertEquals("attached eDoc file name", "IM1_Delivery_Order.pdf", attachedDoc.FileName);
		}

		public void TestIDocManagerSupportWillFindeDocAttachedToShipment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <PackingList> %EOF\n"), "PackingList.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <Invoice> %EOF\n"), "Invoice.pdf", "INV");
			AssertEquals("Shipment documents", 2, shipment.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Declaration & Shipment respective document collections should now be available for selection in message sending attachment drop down control", 2, declaration.eDocsForSelection.Count);

			Factory.Save();
			shipment.DocManagerInfo.MasterFactory.Save();

			int docsInCollection = 0;
			foreach (IStorageDocsBaseCollection docCollection in declaration.eDocsForSelection)
			{
				foreach (IeDoc doc in docCollection)
				{
					docsInCollection++;
				}
			}

			AssertEquals("There should be 3 documents in total available for selection", 3, docsInCollection);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;

			var additionalMessageInformation = new TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[1];
			IeDoc attachment = documentCollection[1]; // second document on Shipment eDocs
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = cusAttachment.FileName;
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			var attachedDoc = messageAttached.GetAttachment();
			AssertEquals("Should find document attached to shipment that was selected for inclusion with this message", attachment.UniqueKey, attachedDoc.UniqueKey);
			AssertEquals("attached eDoc file name", "Invoice.pdf", attachedDoc.FileName);
		}

		public void TestFileNamePeriodsAreReplacedExceptForFileExtension()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "sharp@company.co.nz_20160203_133507.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <PackingList> %EOF\n"), "PackingList.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <Invoice> %EOF\n"), "Invoice.pdf", "INV");
			AssertEquals("Shipment documents", 2, shipment.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Declaration & Shipment respective document collections should now be available for selection in message sending attachment drop down control", 2, declaration.eDocsForSelection.Count);

			int docsInCollection = 0;
			foreach (IStorageDocsBaseCollection docCollection in declaration.eDocsForSelection)
			{
				foreach (IeDoc doc in docCollection)
				{
					docsInCollection++;
				}
			}

			AssertEquals("There should be 3 documents in total available for selection", 3, docsInCollection);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;

			var additionalMessageInformation = new TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[0];
			IeDoc attachment = documentCollection[0];
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = TSWMessageFormatter.FormatAcceptableFileNameForNZC(cusAttachment.FileName);
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			AssertEquals("attached eDoc file name should have replaced all periods except for file extension", "sharp@company_co_nz_20160203_133507.pdf", messageAttached.EG_FileName);
		}

		public void TestInvalidCharactersInFileNameRemoved()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "sharp#tag@company~.co.nz_20160203_133507.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;

			var additionalMessageInformation = new TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[0];
			IeDoc attachment = documentCollection[0];
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = TSWMessageFormatter.FormatAcceptableFileNameForNZC(cusAttachment.FileName);
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			AssertEquals("attached eDoc file name should have replaced all periods except for file extension", "sharptag@company_co_nz_20160203_133507.pdf", messageAttached.EG_FileName);
		}

		public void TestAttachedDocumentNameAllows256Chars()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "NZC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order_WithVeryLongFileNamePurelyForTestingPurposesAsARealFileNameWouldSurelyNeverActuallyBeThisLongAsItWouldBeRediculousToNameAFileWithSuchAStupidName.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = declaration.IsInTestMode;
			message.EM_LinkedObject = entryHeader;

			var additionalMessageInformation = new TradeSingleWindow.AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			IStorageDocsBaseCollection documentCollection = declaration.eDocsForSelection[0];
			IeDoc attachment = documentCollection[0];
			additionalMessageInformation.SupportingDocuments.Add((BusinessObject)attachment);

			foreach (IeDoc cusAttachment in additionalMessageInformation.SupportingDocuments)
			{
				Assert("FileName must be longer than 128 characters", cusAttachment.FileName.Length > 128);
				EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
				ediMessageAttach.EG_FileName = cusAttachment.FileName;
				ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueKey;
				ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
			}

			var messageAttached = message.MessageAttachments[0];
			AssertEquals("attachment is linked to message", messageAttached.EG_EM, message.PK);
			var attachedDoc = messageAttached.GetAttachment();
			AssertEquals("Should find document attached to declaration that was selected for inclusion with this message", attachment, attachedDoc);
			Assert("FileName must be longer than 128 characters", attachedDoc.FileName.Length > 128);
			AssertEquals("attached eDoc file name", "IM1_Delivery_Order_WithVeryLongFileNamePurelyForTestingPurposesAsARealFileNameWouldSurelyNeverActuallyBeThisLongAsItWouldBeRediculousToNameAFileWithSuchAStupidName.pdf", attachedDoc.FileName);
		}

		public void TestGetNewLookups()
		{
			AssertType<CusEntryHeaderLookups>(EntryHeader.Lookups);
		}

		#region Event Logging Tests

		#region Cleared Event logging

		public void TestCustomsClearedEventLoggedImport()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001980";
			Declaration.JE_MasterBill = "09811111111";
			Declaration.JE_HouseBill = "HOUSETEST123";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.SaveHandlingSaveExceptions();

			var entryHeader = Declaration.ActiveEntryHeaders[0];

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00001980";
			outgoingMessage.EM_SystemCreateUser = "GAZ";

			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseB00001980;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mpifoodMessage.EM_SystemCreateUser = "~BP";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "73218058", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, Declaration.JE_TSWCombinedStatus);

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSResponseB00001980;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			nzcsMessage.EM_SystemCreateUser = "~BP";

			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & NZCS should now all be 0 for cleared.", "090", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", Declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "1 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect a Delivery Order has been received from Customs", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect 1 agency response is still pending", TSWEntryStatusList.Codes.CPC, Declaration.JE_TSWCombinedStatus);

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOResponseB00001980;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			bioMessage.EM_SystemCreateUser = "~BP";

			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood, NZCS & MPIBio should now all be 0 for cleared.", "000", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect that a Delivery Order has already been received", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared", TSWEntryStatusList.Codes.CCC, Declaration.JE_TSWCombinedStatus);

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been created", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCustomsClearedEventLoggedForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CLR;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("'ECC' Log Entry event has been created for this cleared/completed export declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));
		}

		public void TestClearanceEventIsNotTriggeredForIPIEntry()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = MessageTypeList.Codes.IPI;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("'CLR' Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CLR;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("'CLR' Log Entry event should not be created for IPI declarations", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCustomsClearedEventLoggedForExportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CLR;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("'ECC' Log Entry event has been created for this cleared/completed export declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));
		}

		public void TestCustomsClearedEventLoggedOnParentShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event for CLR on parent Shipment for the completed entry", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestEventLoggingForDOP()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00003583";
			Declaration.JE_MasterBill = "BKGTESTOBL1";
			Declaration.JE_HouseBill = "BKGTERSTHBL1";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.SaveHandlingSaveExceptions();

			var entryHeader = Declaration.ActiveEntryHeaders[0];

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00003583";
			outgoingMessage.EM_SystemCreateUser = "GAZ";

			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodB00003583;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mpifoodMessage.EM_SystemCreateUser = "~BP";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "52789560", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, Declaration.JE_TSWCombinedStatus);

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOB00003583;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			bioMessage.EM_SystemCreateUser = "~BP";

			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now be 0 for cleared. Customs response Pending", "900", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "52789560", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect that not all responses have been received", FormalEntryStatusList.Codes.AgencyResponsePending, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared - customs pending", TSWEntryStatusList.Codes.PCC, Declaration.JE_TSWCombinedStatus);

			// cleared Delivery on Payment (DOP) customs response should not generate SCM event.
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSB00003583DOP;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			nzcsMessage.EM_SystemCreateUser = "~BP";

			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should now also reflect the DOP response from Customs, entryStatus components representing MPIFood & NZCS should still show be 0 for cleared.", "600", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "52789560", Declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "822", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect a Delivery On Payment has been received", FormalEntryStatusList.Codes.DeliveryOnPayment, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agency responses have cleared - waiting on payment", TSWEntryStatusList.Codes.YCC, Declaration.JE_TSWCombinedStatus);

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event should not have been created for Delivery On Payment status", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			// subsequent DOR message response from Customs should generate the SCM event.
			var nzcsDORMessage = Factory.New<TSWMessage>();
			nzcsDORMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsDORMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsDORMessage.EM_MessageText = NZCSB00003583DOR;
			nzcsDORMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			nzcsDORMessage.EM_SystemCreateUser = "~BP";

			processor.ProcessMessage(nzcsDORMessage);
			AssertEquals(entryHeader, nzcsDORMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should now reflect the cleared DOR response from Customs, entryStatus components representing MPIFood & NZCS should still show be 0 for cleared.", "000", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "52789560", Declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.\r\n1 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect a Delivery On Payment has been received", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agency responses have been cleared", TSWEntryStatusList.Codes.CCC, Declaration.JE_TSWCombinedStatus);

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event should now have been created for Delivery Order received status", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCLREventIsLoggedForClearedWithBioB06Status()
		{
			/*
			 *  set Customs status to cleared, set MPI Food status to cleared
			 *  process MPI Bio B06 (MPI Biosecurity - Directions Given (Final)) response.
			 *  ensure SCM event has been created
			 */
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00004120";
			Declaration.JE_MasterBill = "G82938279";
			Declaration.JE_HouseBill = "G82938279";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CIC;
			Declaration.SaveHandlingSaveExceptions();

			var entryHeader = Declaration.ActiveEntryHeaders[0];
			entryHeader.CH_MPIFoodStatus = "F04";
			entryHeader.CH_MPIBioStatus = "B05";
			entryHeader.CH_NZCSStatus = "819";

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00004120";
			outgoingMessage.EM_SystemCreateUser = "GAZ";

			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry has not been created yet", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOB00004120;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			bioMessage.EM_SystemCreateUser = "~BP";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should be 0 for cleared. Customs response Pending", "010", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number on the declaration", "76437451", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B06", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should show DOR", FormalEntryStatusList.Codes.DeliveryOrderReceived, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Customs have cleared - Biosecurity inspection", TSWEntryStatusList.Codes.CIC, Declaration.JE_TSWCombinedStatus);

			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry has been created when Biosecurity 'B06' status is received with other agencies cleared.", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCLREventIsNotLoggedForNonClearedWithBioB06Status()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00004120";
			Declaration.JE_MasterBill = "G82938279";
			Declaration.JE_HouseBill = "G82938279";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.EPC;
			Declaration.SaveHandlingSaveExceptions();

			var entryHeader = Declaration.ActiveEntryHeaders[0];
			entryHeader.CH_MPIFoodStatus = "F04";
			entryHeader.CH_MPIBioStatus = "";
			entryHeader.CH_NZCSStatus = "ERR";

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00004120";
			outgoingMessage.EM_SystemCreateUser = "GAZ";

			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry has not been created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOB00004120;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			bioMessage.EM_SystemCreateUser = "~BP";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus", "710", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number on the declaration", "76437451", Declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B06", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI Food has cleared, Biosecurity inspection and Customs error", TSWEntryStatusList.Codes.EIC, Declaration.JE_TSWCombinedStatus);

			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry has NOT been created when Biosecurity 'B06' status is received as other agencies are not all cleared.", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		#endregion

		#region Customs Impediment Event logging

		public void TestCustomsImpedimentEventLoggedImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			var customsImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			var customsClearedImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsClearedImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsClearedImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsClearedImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
		}

		public void TestCustomsImpedimentEventLoggedExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			var customsImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			var customsClearedImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsClearedImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsClearedImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsClearedImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
		}

		public void TestCustomsImpedimentEventLoggedImportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			var customsImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			var customsClearedImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsClearedImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsClearedImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsClearedImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
		}

		public void TestCustomsImpedimentEventLoggedExportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			var customsImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			var customsClearedImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsClearedImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsClearedImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsClearedImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
		}

		public void TestCustomsImpedimentEventLoggedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry event gets created on the declaration regardless of if a parent Shipment exists for this held entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			var decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log event should therefore be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry is not on parent Shipment", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
			decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log event should therefore be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - NZC")));
		}

		public void TestCIPEventNotLoggedOnSendOfAmendment()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			var customsImpedimentMsg = Declaration.CusEntryHeader.Messages.AddNew();
			customsImpedimentMsg.EM_ReceiveTransmit = "RCV";
			customsImpedimentMsg.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			customsImpedimentMsg.EM_SystemCreateUser = "~BP";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry should  have been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));

			Declaration.JE_TSWCombinedStatus = ZString.Empty;
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry should not have been created for declaration when sending amendment", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR -")));
		}

		#endregion

		#region MPI Biosecurity Impediment Event logging

		public void TestMPIBIOImpedimentEventLoggedImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PIE;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCE;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
		}

		public void TestMPIBIOImpedimentEventLoggedExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CIA;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCA;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
		}

		public void TestMPIBIOImpedimentEventLoggedImportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AIA;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ACA;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
		}

		public void TestMPIBIOImpedimentEventLoggedExportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.IAR;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CLR;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
		}

		public void TestMPIBIOImpedimentEventLoggedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CIC;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry event for MPI Bio has only been created on the declaration for this held entry, not on the parent shipment", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			var decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log event should be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry should not be updated for parent Shipment", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
			decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log event should be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));
		}

		#endregion

		#region MPI Food Impediment Event logging

		public void TestMPIFoodImpedimentEventLoggedImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCI;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Food has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
		}

		public void TestMPIFoodImpedimentEventLoggedExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAI;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Food has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
		}

		public void TestMPIFoodImpedimentEventLoggedImportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAI;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Food has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
		}

		public void TestMPIFoodImpedimentEventLoggedExportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAI;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Food has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.AAC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
		}

		public void TestMPIFoodImpedimentEventLoggedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCI;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry event for MPI Food has should not have been created on the parent Shipment for this held entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			var decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log event should be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;

			Declaration.SaveHandlingSaveExceptions();
			logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry should not be updated for parent Shipment", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
			decLogEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log event should be showing against the Declaration", true, decLogEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - MPI")));
		}

		#endregion

		public void TestCIPEventRecordsForDifferentAgencies()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "081000548347";
			Declaration.JE_HouseBill = "HB-TEST";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			// MPI Bio response HLD status
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PIE;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			var logProvider = Declaration as IStmALogProvider;
			var impedimentLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("HLD - BIO", impedimentLogs[0].SL_Reference);

			// MPI Food response HLD status
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PII;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Food has also been created for this held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));
			impedimentLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("log count", 2, impedimentLogs.Length);
			AssertEquals("HLD - BIO", impedimentLogs[0].SL_Reference);
			AssertEquals("Impediment log for MPI Food should generate even though the impediment log for MPI Biosecurity already exists", "HLD - MPI", impedimentLogs[1].SL_Reference);

			// MPI Bio response HLD status Cleared
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIBIO;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCI;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));

			// MPI Food response HLD status again
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.MPIFOOD;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCI;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("There should still only be 1 Impediment Log Entry for MPI Food for this declaration", 1, logEntries.Count(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - MPI")));
			impedimentLogs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Subsequent impediment status update for the same agency should not generate another CIP log", 3, impedimentLogs.Length);
			AssertEquals("Impediment log for MPI Biosecurity", "HLD - BIO", impedimentLogs[0].SL_Reference);
			AssertEquals("Impediment log for MPI Food", "HLD - MPI", impedimentLogs[1].SL_Reference);
			AssertEquals("Cleared Impediment log for MPI Biosecurity", "CLR - BIO", impedimentLogs[2].SL_Reference);

			// NZCS HOLD response now recieved
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.ICI;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry has been created for NZCS agency held declaration", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - NZC")));
		}

		#endregion

		#region B00001980 Response

		const string MPIFoodResponseB00001980 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20150925161022</IssueDateTime>
    <FunctionalReferenceID>3157</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161022</AcceptanceDateTime>
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161022</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161022</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string MPIBIOResponseB00001980 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20150925161034</IssueDateTime>
    <FunctionalReferenceID>3159</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161034</AcceptanceDateTime>
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161034</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161037</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string NZCSResponseB00001980 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20150925161034</IssueDateTime>
    <FunctionalReferenceID>3158</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_CA7C4566-8C86-4E79-B77D-7874B1B34BC3"" filename=""IM1_Delivery_Order-73218058-2015-09-25-161045713.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161034</AcceptanceDateTime>
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>502.99</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161034</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161034</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region B00003583 Responses

		const string MPIFoodB00003583 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180501101154</IssueDateTime>
    <FunctionalReferenceID>2688</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>52789560</ID>
        <AcceptanceDateTime formatCode=""204"">20180501101154</AcceptanceDateTime>
        <FunctionalReferenceID>B00003583</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180501101154</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180501101154</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string MPIBIOB00003583 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180501101211</IssueDateTime>
    <FunctionalReferenceID>2690</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>52789560</ID>
        <AcceptanceDateTime formatCode=""204"">20180501101211</AcceptanceDateTime>
        <FunctionalReferenceID>B00003583</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180501101211</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180501101211</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string NZCSB00003583DOP =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180501101209</IssueDateTime>
    <FunctionalReferenceID>2689</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>52789560</ID>
        <AcceptanceDateTime formatCode=""204"">20180501101209</AcceptanceDateTime>
        <FunctionalReferenceID>B00003583</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">606.54</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180501101209</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20180501101209</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string NZCSB00003583DOR =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180503101209</IssueDateTime>
    <FunctionalReferenceID>2745</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_CA7C4566-8C86-4E79-B77D-7874B1B34BC3"" filename=""IM1_Delivery_Order-73218058-2015-09-25-161045713.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>52789560</ID>
        <AcceptanceDateTime formatCode=""204"">20180503101209</AcceptanceDateTime>
        <FunctionalReferenceID>B00003583</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>606.54</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180503101209</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20180503101209</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string MPIBIOB00004120 =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181128155707</IssueDateTime>
    <FunctionalReferenceID>4768</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_4D906E05-E0AE-48D8-B2D1-236B218ADFED"" filename=""PDF9-76437451-2018-11-28-1511189057.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>MPI APPROVAL TO MOVE TO ATF 108-124 Pah Road Royal Oak Auckland NEW ZEALAND</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>76437451</ID>
        <AcceptanceDateTime formatCode=""204"">20181128155707</AcceptanceDateTime>
        <FunctionalReferenceID>B00004120</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181128155707</EffectiveDateTime>
      <NameCode>B06</NameCode>
      <ReleaseDateTime formatCode=""204"">20181128155707</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region Implementation

		protected CusEntryNumber GetCusEntryNumber(CusEntryHeader parent, ZString entryType, ZBool isSystemGenerated)
		{
			ZQuery queryForCusEntryNumber = new ZQuery();
			queryForCusEntryNumber.AddToFilter(CusEntryNumSchema.CE_ParentID, parent.PK);
			queryForCusEntryNumber.AddToFilter(CusEntryNumSchema.CE_ParentTable, parent.TableName);
			queryForCusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			queryForCusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			queryForCusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, isSystemGenerated);
			return Factory.LoadTop1<CusEntryNumber>(queryForCusEntryNumber);
		}

		protected new T EntryHeader
		{
			get { return (T)base.EntryHeader; }
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		protected virtual string ExpectedCusEntryNumType
		{
			get { return CusEntryNumberTypeList.Codes.FormalEntry; }
		}

		protected virtual string ExpectedCusEntryHeaderType
		{
			get { return CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry; }
		}

		#endregion
	}

	public class NZCusEntryHeaderCustomsChargesTest : CusEntryHeaderCustomsChargesTest
	{
		public void TestIsFeePaidByBroker()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.ClientDeferred;
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			AssertEquals("IsFeePaidByBroker", false, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));

			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.BrokerDeferred;
			AssertEquals("IsFeePaidByBroker", true, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			AssertEquals("IsFeePaidByBroker", true, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByClient;
			AssertEquals("IsFeePaidByBroker", false, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.ClientDeferred;
			entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			AssertEquals("IsFeePaidByBroker: Export entries are paid by broker. JE_PaymentMethod is not sent in export messages", true, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.ClientDeferred;
			entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			AssertEquals("IsFeePaidByBroker: export entries for completion", true, entryHeader.IsFeePaidByBroker(ZString.Empty, ZString.Empty, null));
		}

		public override void AddCountrySpecificCharges(Customs.Business.CusEntryHeader entryHeader)
		{
			CusEntryHeader header = entryHeader as CusEntryHeader;
			ICustomsCharges decAsICustomsCharges = ServiceLocator.GetService<ICustomsCharges>(header.Declaration);
			CustomsCharge[] charges = decAsICustomsCharges.GetCustomsCharges(null);

			AssertEquals("charges.CustomsCharges.Length before adding charges", 0, charges.Length);

			header.EntryFeeAmount = 23m;
			header.EntryFeeGST = 44m;

			CusEntryLine entryLine = header.MergedLines.AddNew();

			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.Duty, 100m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GST, 101m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.ACCFuelLevy, 102m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.ALACLevy, 103m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.AntiDumpingDuty, 104m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.CountervailingDuty, 105m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.DutyCredit, 106m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.HERALevy, 107m);

			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.ALACLevyCredit, 200m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.DepositRefund, 300m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.ExciseDutyCredit, 400m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GSTCredit, 500m);
		}

		public override void AssertCustomsCharges(CustomsCharge[] charges)
		{
			AssertEquals("charges.CustomsCharges.Length after adding charges", 9, charges.Length);
			AssertCustomsCharge(charges[0], 23m, 44m, EntryChargeTypeList.Descriptions.EntryFee);
			AssertCustomsCharge(charges[1], 100m, EntryChargeTypeList.Descriptions.Duty);
			AssertCustomsCharge(charges[2], 101m, EntryChargeTypeList.Descriptions.GST);
			AssertCustomsCharge(charges[3], 102m, EntryChargeTypeList.Descriptions.ACCFuelLevy);
			AssertCustomsCharge(charges[4], 103m, EntryChargeTypeList.Descriptions.ALACLevy);
			AssertCustomsCharge(charges[5], 104m, EntryChargeTypeList.Descriptions.AntiDumpingDuty);
			AssertCustomsCharge(charges[6], 105m, EntryChargeTypeList.Descriptions.CountervailingDuty);
			AssertCustomsCharge(charges[7], 106m, EntryChargeTypeList.Descriptions.DutyCredit);
			AssertCustomsCharge(charges[8], 107m, EntryChargeTypeList.Descriptions.HERALevy);
		}

		public override Customs.Business.CusEntryHeader GetEntryHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.BrokerDeferred;
			return declaration.CusEntryHeader;
		}
	}
}
