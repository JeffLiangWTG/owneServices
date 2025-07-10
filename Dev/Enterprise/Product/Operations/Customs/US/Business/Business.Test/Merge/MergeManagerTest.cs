using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public void TestWhenAnEntryWithCustomsTransactionsExists()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals(true, entry.HasTransactionsWithCustoms);

			var entryNumber = entry.EntryNumber;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Merge fails as there is a message error on JE_MessageType", false, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false)));

			AssertNotNull("ENS entry should not have been deactivated at this point", declaration.ActiveEntryHeaders.EntrySummaryEntry);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			AssertEquals("Import ENS merge works", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("should be the same entry because it was lodged at Customs.", entry, declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals("Same entry number", entryNumber, declaration.ImportEntryNumber);
		}

		public void TestFTZMergeOKWithoutEnabling()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			bool merged = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			Assert("Not merged because no invoices or lines", !merged);

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			merged = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			Assert("Should have merged OK ", merged);
		}

		public void TestDoNotProceedMergeWhenEntryNotSupposedToBeDiscarded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_EnableENS = false;
			Assert(declaration.IsBorderMovement);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			Assert(!declaration.IsBorderMovement);
			Assert("Should be merged OK", declaration.DoMerge());

			var entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			Assert(declaration.IsBorderMovement);
			Assert("Should NOT be merged again and deactivated entry", !declaration.DoMerge());
			AssertEquals("Invalid Operation text",
				MergeManager.EntriesAboutToBeDiscarded + MergeManager.CargoReleaseEntriesAboutToBeDiscarded,
				declaration.MergeManager.InvalidOperationText);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			Assert(declaration.DoMerge());

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Assert("Should be merged again and deactivated entry as it is withdrawn", declaration.DoMerge());

			declaration.US_EnableENS = true;
			AssertEquals("DoMerge()", true, declaration.DoMerge());
			declaration.ActiveEntryHeaders[1].CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			declaration.US_EnableENS = false;
			AssertEquals("DoMerge()", false, declaration.DoMerge());

			AssertEquals("Invalid Operation text",
				MergeManager.EntriesAboutToBeDiscarded, declaration.MergeManager.InvalidOperationText);
		}

		public void TestUS_TaxDeferredIndicator()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge();
			AssertHasMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo,
				FormalImportAddInfoJobDeclarationValidation.TaxIsToBeDeferredOnlyOnFormalEntries);

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			AssertNoMessageErrorContaining(declaration.US_TaxDeferIndicatorInfo,
				FormalImportAddInfoJobDeclarationValidation.TaxIsToBeDeferredOnlyOnFormalEntries);
		}

		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestUS_BondAmount()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond Amount should be calculated", 10002m, declaration.US_BondAmount);

			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Bond amount calculated should be cleared out", 0m, declaration.US_BondAmount);
		}

		public void TestCannotMergeReasonForIMX()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode =
				""; //no entry filer code is needed to merge because no need to generate entry number

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			AssertNoExceptionThrown(delegate
			{
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			});

			declaration.Invoices.DeleteAll();
			AssertExceptionThrown(typeof(ApplicationException), delegate
			{
				try
				{
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}
				catch
				{
					declaration.UnlockDoMergeMutex();
					throw;
				}
			});
		}

		public void TestCannotMergeReasonForFTZ()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode =
				""; //no entry filer code is needed to merge because no need to generate entry number

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			AssertNoExceptionThrown(delegate
			{
				try
				{
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}
				catch
				{
					declaration.UnlockDoMergeMutex();
					throw;
				}
			});

			declaration.Invoices.DeleteAll();
			AssertExceptionThrown(typeof(ApplicationException), delegate
			{
				try
				{
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}
				catch
				{
					declaration.UnlockDoMergeMutex();
					throw;
				}
			});
		}

		public void TestCannotMergeReasonForDetailedInBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;

			declaration.DoMerge();
			AssertEquals("No invoice lines",
				Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines,
				declaration.MergeManager.InvalidOperationText);

			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			AssertEquals("invoice line is entered", "", declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeWithoutLinesForENS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;

			declaration.DoMerge();
			AssertEquals("No invoice lines, and ENS is enabled",
				Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines,
				declaration.MergeManager.InvalidOperationText);

			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			AssertEquals("invoice lines are added", "", declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeifNoEntryFilerCode()
		{
			DeclarationTestHelper.SetEntryFilerCode("");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			AssertEquals(MergeManager.NoEntryFilerCodeForBranch, declaration.MergeManager.InvalidOperationText);
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge();
			AssertNotEquals(MergeManager.NoEntryFilerCodeForBranch, declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeIfBranchEntryRangeIsNotProperlySetup()
		{
			var branchStmNums = DeclarationTestHelper.SetupBranchSpecificFormalEntryNumber("XJ5");
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = otherBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			string branchErrorMessage = string.Format(ACEEntryStmNumsSetting.EntryNumberRangeNotSetup,
				otherBranch.GB_Code, "XJ5", currentCompany.GC_Code);
			AssertEquals(branchErrorMessage, declaration.MergeManager.InvalidOperationText);
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.DoMerge();
			AssertNotEquals(branchErrorMessage, declaration.MergeManager.InvalidOperationText);
			AssertNotEquals(
				string.Format(ACEEntryStmNumsSetting.EntryNumberRangeNotSetup, GlbBranch.CurrentBranch.GB_Code, "XJ5",
					GlbCompany.CurrentCompany.GC_Code), declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeIfNotEnoughAvailableEntryNumbers()
		{
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue);
			companyStmNums.GenerateNextCustomsNumber(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

			var invoice = declaration.Invoices.AddNew();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;
			invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge();
			var companyMessage = string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForCompany,
				GlbCompany.CurrentCompany.GC_Code, "XJ5");
			AssertEquals(companyMessage, declaration.MergeManager.InvalidOperationText);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.DoMerge();
			AssertEquals(companyMessage, declaration.MergeManager.InvalidOperationText);

			var branchStmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			var branchStmNums = branchStmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 20000, 30000).StmNums;
			branchStmNums.SetNextNumber(branchStmNums.SN_MaximumValue);
			branchStmNums.GenerateNextCustomsNumber(Factory);
			Factory.InvalidateCachedProperties();
			var branchMessage = string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForBranch,
				GlbBranch.CurrentBranch.GB_Code, "XJ5");
			declaration.DoMerge();
			AssertEquals(branchMessage, declaration.MergeManager.InvalidOperationText);

			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.DoMerge();
			AssertEquals(branchMessage, declaration.MergeManager.InvalidOperationText);

			declaration.ImportEntryNumber = "10203120";
			declaration.DoMerge();
			AssertEquals("", declaration.MergeManager.InvalidOperationText);
			declaration.ImportEntryNumber = "";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			AssertEquals("", declaration.MergeManager.InvalidOperationText);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.DoMerge();
			AssertEquals(branchMessage, declaration.MergeManager.InvalidOperationText);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			branchStmNums.SetNextNumber(branchStmNums.SN_MaximumValue - 1);
			Factory.InvalidateCachedProperties();
			declaration.DoMerge();
			AssertEquals("", declaration.MergeManager.InvalidOperationText);

			branchStmNums.Delete();
			branchStmNums.Factory.Save();
			Factory.InvalidateCachedProperties();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals(companyMessage, declaration.MergeManager.InvalidOperationText);
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue - 10);
			Factory.InvalidateCachedProperties();
			declaration.DoMerge();
			AssertEquals("", declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeIfCompanyOrBranchInBondNumberRangeIsNotProperlySetup()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			InBondNumberRange numberRange =
				USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty,
					otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = otherBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			string branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(
				((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			AssertEquals(branchErrorMessage, declaration.MergeManager.InvalidOperationText);
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.DoMerge();
			AssertNotEquals(branchErrorMessage, declaration.MergeManager.InvalidOperationText);
		}

		public void TestCannotMergeIfNotEnoughAvailableInBondNumbers()
		{
			DbConnection connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableENS = true;
				declaration.US_EnableINB = true;
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

				Bill bill = declaration.Bills.AddNew();
				bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

				Bill houseBill = declaration.Bills.AddNew();
				houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill.CU_CU_ParentBill = bill.PK;
				invoice.JobComInvoiceLines.AddNew();

				InBondNumberRange numberRange =
					USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty,
						GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(Env.CurrentBranch.PK);

				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
				{
				}

				var existingDeclaration = Factory.New<JobDeclaration>();
				existingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				existingDeclaration.US_EnableINB = true;
				var inBondHeader = existingDeclaration.CustomsEntryHeaders.AddNew();
				inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				ZString number = numberRange.LastNumber.ToString().PadLeft(8, '0');
				number += InBondNumberCheckDigitCalculator.GetCheckDigit(number);
				inBondHeader.EntryNumber = number;

				declaration.DoMerge();
				AssertEquals(
					InBondNumberAvailabilityChecker.NotEnoughAvailableInBondNumbersForBranchForJob(
						GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName),
					declaration.MergeManager.InvalidOperationText);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.DoMerge();
				AssertEquals("", declaration.MergeManager.InvalidOperationText);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EnableENS = true;
				declaration.US_EnableINB = false;
				declaration.DoMerge();
				AssertEquals("", declaration.MergeManager.InvalidOperationText);
				declaration.US_EnableINB = true;
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				long numberToCheck = (long)(numberRange.LastNumber - 1);
				while (numberToCheck != branchNumberFountain.PeekPreliminary(Factory))
				{
					branchNumberFountain.GetNext(Factory);
				}

				declaration.DoMerge();
				AssertEquals("", declaration.MergeManager.InvalidOperationText);
			}
			catch
			{
				throw;
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestMergeWithoutLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;
			declaration.US_EnableENS = false;
			declaration.DoMerge();
			AssertEquals("One inbond entry is created", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			Factory.Save();

			AssertEquals("entry number is assigned", false, entry.EntryNumber.IsEmpty);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			AssertEquals("The entry created without invoice lines should still be used when invoice lines are created",
				entry, invoiceLine.CusEntryLine.Header);
		}

		public void TestMergeCallsValidateEntryNumberForAllEntries()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("HasBeenLodged", true, entry.HasBeenLodgedAtCustoms);
			AssertEquals("HasBeenWithdrawn", false, entry.HasBeenWithdrawn);

			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;

			bool successful = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			Assert(!successful);

			AssertEquals("not merged and the entry is still active", true, entry.IsActive);

			//with this error, they cannot save the changes
			AssertHasErrorContaining(declaration.US_EnableENSInfo,
				FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);

			declaration.US_EnableENS = true;
			AssertNoErrorContaining(declaration.US_EnableENSInfo,
				FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);

			declaration.US_EnableENS = false;
			AssertHasErrorContaining(declaration.US_EnableENSInfo,
				FormalImportAddInfoJobDeclarationValidation.CustomsTransactionsExist);
		}

		public void TestGetReasonForCannotMerge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);

			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.US_EnableENS = false;
			AssertEquals("DoMerge() fails", false, declaration.DoMerge());
			AssertEquals("Cannot merge as there are no messaging mode enabled",
				MergeManager.NoImportMessagingModeEnabled, declaration.MergeManager.InvalidOperationText);
		}

		public void TestLineMerger()
		{
			AssertEquals(typeof(LineMerger),
				new MergeManager(Factory.New<JobDeclaration>()).GetNewLineMerger().GetType());
		}

		public void TestTypesThatDoNotAffectMerge()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var result = MergeManagerTestHelper.GetTypesWhichDoNotEffectMerge(declaration.MergeManager);
			Assert("house bill should affect merge due to IT No for formal entries",
				!result.Exists(details => details.TypeToExclude == typeof(Customs.Business.Bill)));
			Assert("house bill should affect merge due to IT No for formal entries",
				!result.Exists(details => details.TypeToExclude == typeof(Customs.Business.BillCollection<Bill, JobDeclaration>)));
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			result = MergeManagerTestHelper.GetTypesWhichDoNotEffectMerge(declaration.MergeManager);
			Assert("house bill should affect merge due to IT No for formal entries",
				result.Exists(details => details.TypeToExclude == typeof(Customs.Business.Bill)));
			Assert("house bill should affect merge due to IT No for formal entries",
				result.Exists(details => details.TypeToExclude == typeof(Customs.Business.BillCollection<Customs.Business.Bill, Customs.Business.BaseJobDeclaration>)));
		}

		protected override bool DoesHouseBillAffectMerge => true;

		protected override bool DoesPackingGroupAffectMerge => false;

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Customs.Business.BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = (JobDeclaration)GetJobDeclaration();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				result.US_EnableENS = true;
				return result;
			}
		}

		protected override void SetUp()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			base.SetUp();
		}
		CustomsNumberViewStmNums companyStmNums;
	}
}
