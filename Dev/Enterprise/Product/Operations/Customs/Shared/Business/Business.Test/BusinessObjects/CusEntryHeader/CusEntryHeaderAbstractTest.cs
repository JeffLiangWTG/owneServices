using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryHeader))]
	public abstract class CusEntryHeaderAbstractTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			return declaration.CustomsEntryHeaders.AddNew();
		}

		protected abstract Type ExpectedChargeCollectionType { get; }
		protected abstract Type ExpectedChargeType { get; }
		protected abstract BaseJobDeclaration GetNewDeclaration();

		public void TestChargesAndCollectionType()
		{
			var dec = GetNewDeclaration();
			var ceh = dec.CustomsEntryHeaders.AddNew();
			AssertType("CEH needs to have its own type of charges collection", ExpectedChargeCollectionType, ceh.Charges);
			AssertType("CEH.Charges.AddNew() gives correct charge type", ExpectedChargeType, ceh.Charges.AddNew());
			AssertType("CEH.Charges.AddNew(string) gives correct charge type", ExpectedChargeType, ceh.Charges.AddNew("x"));
			AssertType("CEH.Charges.AddNew(string, ZDecimal) gives correct charge type", ExpectedChargeType, ceh.Charges.AddNew("x", 0m));
			AssertType("CEH.Charges[int] gives correct charge type", ExpectedChargeType, ceh.Charges[0]);
			AssertType("CEH.Charges[string] gives correct charge type", ExpectedChargeType, ceh.Charges["x"]);
		}
	}

	public abstract class CusEntryHeaderTest : CusEntryHeaderAbstractTest
	{
		public void TestICustomsChargeEntry_Job()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			header.CH_BGMReference = "EntryRef1";
			header.EntryNumber = "123456";

			var jobHeader = new JobHeader.Loader(dec).TryCreate();
			Factory.Save();
			AssertEquals(jobHeader, ((ICustomsChargeEntry)header).Job);
			AssertEquals(((IAccInvoiceDataProvider)header).UniqueNumber, ((ICustomsChargeEntry)header).UniqueNumber);
			AssertEquals(((IAccInvoiceDataProvider)header).PreviousUniqueNumber, ((ICustomsChargeEntry)header).PreviousUniqueNumber);
		}

		public new void TestWorkflowSupportableBusinessObject()
		{
			Assert("We no longer support workflow on CusEntryHeader", true);
		}

		public void TestGetPermitComment()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var permit = new PermitRecord
			{
				PermitHeader = null,
				Quantity = 0,
				Value = 10
			};
			AssertEquals("Customs Entry", entry.GetPermitComment(permit));

			permit.Procedure = "D48";
			AssertEquals("Customs Entry - D48", entry.GetPermitComment(permit));
		}

		public void TestResetInvoiceHeadersAndLines()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var entryLine12 = entry1.MergedLines.AddNew();
			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine11 = entryLine11.InvoiceLines.AddNew();
			var invoiceLine12 = entryLine12.InvoiceLines.AddNew();
			invoiceLine11.JI_JZ = invoice1.PK;
			invoiceLine12.JI_JZ = invoice2.PK;

			AssertEquals(2, entry1.InvoiceHeaders.Length);
			invoice1.Delete();
			AssertEquals(2, entry1.InvoiceHeaders.Length);

			entry1.ResetInvoiceHeadersAndLines();
			AssertEquals(1, entry1.InvoiceHeaders.Length);

			AssertEquals(2, entry1.InvoiceLines.Count());

			entryLine11.InvoiceLines.Remove(invoiceLine11);
			AssertEquals(2, entry1.InvoiceLines.Count());

			entry1.ResetInvoiceHeadersAndLines();
			AssertEquals(1, entry1.InvoiceLines.Count());
		}

		public void TestDeclarationUCR()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LoadOrCreateUCRNumber("UCR");
			AssertEquals("UCR", entry.DeclarationUCR);

			entry.LoadOrCreateUCRNumber("UCR/001");
			AssertEquals("UCR", entry.DeclarationUCR);
			AssertEquals("001", entry.DeclarationUCRPartSuffix);
		}

		public virtual void TestDuty()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(0.00m, entryHeader.Duty);
			AssertEquals(0, entryHeader.Charges.Count);
			if (!string.IsNullOrEmpty(entryHeader.DutyCode))
			{
				entryHeader.Duty = 12.34m;
				AssertEquals(12.34m, entryHeader.Duty);
				AssertEquals(1, entryHeader.Charges.Count);
				entryHeader.Duty = 34.56m;
				AssertEquals(34.56m, entryHeader.Duty);
			}
		}

		public virtual void TestVAT()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(0.00m, entryHeader.VAT);
			AssertEquals(0, entryHeader.Charges.Count);
			if (!string.IsNullOrEmpty(entryHeader.TaxCode))
			{
				entryHeader.VAT = 12.34m;
				AssertEquals(12.34m, entryHeader.VAT);
				AssertEquals(1, entryHeader.Charges.Count);
				entryHeader.VAT = 34.56m;
				AssertEquals(34.56m, entryHeader.VAT);
			}
		}

		public void TestMovementReferenceNumber_BlankMRNWillBeNotPersistedUponSave()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryHeader1 = dec.CustomsEntryHeaders.AddNew();
			var throwAway = entryHeader1.MovementReferenceNumber;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertEquals("Upon save, the blank MRN is not persisted", 0, newFactory.GetDatabaseCount(typeof(CusEntryNumber), new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber)));
		}

		public void TestMovementReferenceNumber_MovementReferenceNumberSetter()
		{
			var newFactory = new BusinessObjectFactory();
			var dec = Factory.New<BaseJobDeclaration>();
			CombineAssertions("With Setting IssueDate", () =>
			{
				var entryHeader1 = dec.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_BGMReference = "T1";
				entryHeader1.MovementReferenceNumberSetter("TESTMRN", ZDateTime.BrettsBirthday);
				AssertEquals("MovementReferenceNumber", "TESTMRN", entryHeader1.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate", ZDateTime.BrettsBirthday, entryHeader1.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus", ZString.Empty, entryHeader1.MovementReferenceNumberEntryStatus);
				Factory.Save();
				entryHeader1 = newFactory.Load<CusEntryHeader>(entryHeader1.PK);
				AssertEquals("MovementReferenceNumber Reloaded", "TESTMRN", entryHeader1.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate Reloaded", ZDateTime.BrettsBirthday, entryHeader1.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus Reloaded", ZString.Empty, entryHeader1.MovementReferenceNumberEntryStatus);
			});

			CombineAssertions("Without Setting IssueDate", () =>
			{
				var entryHeader2 = dec.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_BGMReference = "T2";
				entryHeader2.MovementReferenceNumberSetter("TESTMRN2");
				AssertEquals("MovementReferenceNumber", "TESTMRN2", entryHeader2.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate", ZDateTime.Empty, entryHeader2.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus", ZString.Empty, entryHeader2.MovementReferenceNumberEntryStatus);
				AssertEquals("MovementReferenceNumberExpiryDate", ZDateTime.Empty, entryHeader2.MovementReferenceNumberExpiryDate);
				Factory.Save();
				entryHeader2 = newFactory.Load<CusEntryHeader>(entryHeader2.PK);
				AssertEquals("MovementReferenceNumber Reloaded", "TESTMRN2", entryHeader2.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate Reloaded", ZDateTime.Empty, entryHeader2.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus Reloaded", ZString.Empty, entryHeader2.MovementReferenceNumberEntryStatus);
				AssertEquals("MovementReferenceNumberExpiryDate Reloaded", ZDateTime.Empty, entryHeader2.MovementReferenceNumberExpiryDate);
			});

			CombineAssertions("With Setting entryStatus", () =>
			{
				var entryHeader3 = dec.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_BGMReference = "T3";
				entryHeader3.MovementReferenceNumberSetter("TESTMRN3", ZDateTime.BrettsBirthday, "203");
				AssertEquals("MovementReferenceNumber", "TESTMRN3", entryHeader3.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate", ZDateTime.BrettsBirthday, entryHeader3.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus", "203", entryHeader3.MovementReferenceNumberEntryStatus);
				Factory.Save();
				entryHeader3 = newFactory.Load<CusEntryHeader>(entryHeader3.PK);
				AssertEquals("MovementReferenceNumber Reloaded", "TESTMRN3", entryHeader3.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate Reloaded", ZDateTime.BrettsBirthday, entryHeader3.MovementReferenceNumberIssueDate);
				AssertEquals("MovementReferenceNumberEntryStatus Reloaded", "203", entryHeader3.MovementReferenceNumberEntryStatus);
			});

			CombineAssertions("Setting expiryDate", () =>
			{
				var entryHeader4 = dec.CustomsEntryHeaders.AddNew();
				entryHeader4.CH_BGMReference = "T4";
				var expiryDate = ZDateTime.BrettsBirthday.AddDays(1);
				entryHeader4.MovementReferenceNumberSetter("TESTMRN4", expiryDate: expiryDate);
				AssertEquals("MovementReferenceNumberExpiryDate", expiryDate, entryHeader4.MovementReferenceNumberExpiryDate);
				Factory.Save();
				entryHeader4 = newFactory.Load<CusEntryHeader>(entryHeader4.PK);
				AssertEquals("MovementReferenceNumberExpiryDate Reloaded", expiryDate, entryHeader4.MovementReferenceNumberExpiryDate);
			});
		}

		public void TestMovementReferenceNumber_SetValueDirectly()
		{
			CombineAssertions(() =>
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				var entryNumMadeDirect = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, entryHeader.CountryCode);
				entryNumMadeDirect.CE_EntryNum = "Directly";
				AssertEquals("MovementReferenceNumber", "Directly", entryHeader.MovementReferenceNumber);
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("MovementReferenceNumber Reloaded", "Directly", entryHeader.MovementReferenceNumber);
			});
		}

		public void TestHasAnyConfirmedFeesOnAnyMergedLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var line1 = header.MergedLines.AddNew();
			var line2 = header.MergedLines.AddNew();
			AssertEquals("No confirmed fees", false, header.HasAnyConfirmedFeesOnAnyMergedLine);
			line1.Fees.AddNew();
			AssertEquals("Has only fee on line1", false, header.HasAnyConfirmedFeesOnAnyMergedLine);
			line2.ConfirmedFees.AddNew();
			AssertEquals("Has fee on line1, confirmed fee on line2", true, header.HasAnyConfirmedFeesOnAnyMergedLine);
			line1.ConfirmedFees.AddNew();
			line2.ConfirmedFees.RemoveAndDeleteAll();
			AssertEquals("Has fee and confirmed fee on line1", true, header.HasAnyConfirmedFeesOnAnyMergedLine);
			line1.Fees.RemoveAndDeleteAll();
			AssertEquals("Has confirmed fee on line1", true, header.HasAnyConfirmedFeesOnAnyMergedLine);
			line1.ConfirmedFees.RemoveAndDeleteAll();
			AssertEquals("All fees deleted", false, header.HasAnyConfirmedFeesOnAnyMergedLine);
		}

		#region IWorkflowTriggerEventSource

		public void TestInheritsIWorkflowTriggerEventSource()
		{
			IWorkflowTriggerEventSource entry = Factory.New<CusEntryHeader>();
			AssertNotNull("Should inherit IWorkflowTriggerEventSource", entry);
		}

		[ExpectNoExceptions]
		public void TestIWorkflowTriggerEventSourceDoesNotThrowIfNoDeclaration()
		{
			IWorkflowTriggerEventSource entry1 = Factory.New<CusEntryHeader>();
			object obj = entry1.JobHeaderCompany;
			obj = entry1.ParentWorkflowProviders;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			IWorkflowTriggerEventSource entry2 = declaration.CustomsEntryHeaders.AddNew();
			obj = entry2.JobHeaderCompany;
		}

		public void TestParentWorkflowProviders()
		{
			IWorkflowTriggerEventSource entry1 = Factory.New<CusEntryHeader>();
			var prov = entry1.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			IWorkflowTriggerEventSource entry2 = declaration.CustomsEntryHeaders.AddNew();
			prov = entry2.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
			AssertEquals(declaration, prov[0]);
		}

		#endregion

		public void TestRecoverFromUnsuccessfulSave()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_BGMReference = "BGMREF";
			entry.CH_Status = "XXX";
			Assert(!entry.CH_BGMReference.IsEmpty);
			Assert(!entry.CH_Status.IsEmpty);
			entry.RecoverFromUnsuccessfulSave();

			if (entry.ShouldResetBGMReferenceOnUnsuccessfulSave)
			{
				Assert(entry.CH_BGMReference.IsEmpty);
			}
			else
			{
				AssertEquals("BGMREF", entry.CH_BGMReference);
			}
			Assert(entry.CH_Status.IsEmpty);
		}

		public void TestDeleteAnyNewMessages()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CanNotBeSavedCusEntryHeader>();
			entry.CanNotBeSaved = false;
			entry.CH_JE = declaration.PK;
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ReceiveTransmit = "RCV";
			entry.Messages.Add(message1);
			Factory.Save();
			var message2 = Factory.New<EDIMessage>();
			entry.Messages.Add(message2);
			message2.EM_ReceiveTransmit = "TRX";
			var message3 = Factory.New<EDIMessage>();
			entry.Messages.Add(message3);
			message3.EM_ReceiveTransmit = "RCV";
			AssertEquals(3, entry.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(!message2.IsDeleted);
			Assert(!message3.IsDeleted);
			entry.CH_TotalPaid = 1;
			entry.CanNotBeSaved = true;
			AssertExceptionThrown<Exception>(() =>
			{
				Factory.Save();
			});
			AssertEquals(2, entry.Messages.Count);
			Assert("Only outgoing message will be deleted", !message1.IsDeleted);
			Assert("Only outgoing message will be deleted", message2.IsDeleted);
			Assert("Only outgoing message will be deleted", !message3.IsDeleted);
		}

		public void TestDeclarationDeletedEntryHeaderPKsInDatabase()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			entry1.Delete();
			AssertEquals(1, declaration.DeletedEntryHeaderPKsInDatabase.Count);
			AssertEquals(entry1.PK, declaration.DeletedEntryHeaderPKsInDatabase[0]);
			entry2.Delete();
			AssertEquals(2, declaration.DeletedEntryHeaderPKsInDatabase.Count);
			AssertEquals(entry2.PK, declaration.DeletedEntryHeaderPKsInDatabase[1]);
		}

		public void TestDeclarationReference()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			AssertEquals(ZString.Empty, entry.DeclarationReference);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration.PK;
			Factory.Save();

			AssertEquals(declaration.JE_DeclarationReference, entry.DeclarationReference);
		}

		public void TestDeclarationIsCachedForCollectionRemoval()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(declaration, entry.Declaration);
			entry.CH_JE = ZGuid.Empty;
			AssertEquals(declaration, entry.Declaration);
			entry.CH_JE = ZGuid.Empty;
			AssertEquals("The previous declaration should have been cached", declaration, entry.Declaration);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration1.PK;
			AssertEquals(declaration1, entry.Declaration);
		}

		public virtual void TestTotalDutyAmount()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 200m;

			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 300m;
			entryLine2.Fees.GetOrAddFeeByFeeType(declaration.GSTOrVATCode).CF_ChargeAmount = 100m;

			AssertEquals("Total Duty Amount", 500m, entry.TotalDutyAmount);
		}

		public virtual void TestTotalTAndI()
		{
			var declaration = ImportJobDeclaration;
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			var (overseasFreightChargeCode, overseasInsuranceChargeCode, notIncludedChargeCode) = GetChargeCodesForTotalTAndI();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var freight = invoiceHeader.Charges.AddNew();
			freight.J7_ChargeType = overseasFreightChargeCode;
			freight.J7_Amount = 120.00m;
			freight.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var insurance = invoiceHeader.Charges.AddNew();
			insurance.J7_ChargeType = overseasInsuranceChargeCode;
			insurance.J7_Amount = 30.00m;
			insurance.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var packing = invoiceHeader.Charges.AddNew();
			packing.J7_ChargeType = notIncludedChargeCode;
			packing.J7_Amount = 70.00m;
			packing.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 500.00m;
			invoiceLine1.JI_Weight = 1;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500.00m;
			invoiceLine2.JI_Weight = 1;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				invoiceLine2.JI_CEI = instruction.PK;
			}

			DoMerge(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("TotalTAndI.Amount", 150.00m, declaration.CustomsEntryHeaders[0].TotalTAndI.Amount);
			});
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public virtual void TestFOBAndCIFFigures()
		{
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(ImportJobDeclaration.CountryCode) || ImportJobDeclaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			var declaration = ImportJobDeclaration;
			var setup = GetChargesCurrencyTestSetup();
			setup.SetupJobDecWithOFTAndCIFCharges(declaration, declaration.LocalCurrencyCode);
			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasFreight.Amount);
				AssertEquals("FOB for the entry", setup.ExpectedFOB, entryHeader.FOB.Amount);
				AssertEquals("CIF for the entry", setup.ExpectedCIF, entryHeader.CIF.Amount);
			});
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public virtual void TestFOBAndCIFInLocalCurrency()
		{
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(ImportJobDeclaration.CountryCode) || ImportJobDeclaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			RefCurrency newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			ZDateTime from = new ZDateTime(2007, 6, 1);
			ZDateTime to = new ZDateTime(2007, 12, 30);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			var setup = GetChargesCurrencyTestSetup();
			setup.SetupJobDecWithOFTAndCIFCharges(declaration, newCurrency.RX_Code);
			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("FOB in local currency", setup.ExpectedFOB * 2, entryHeader.FOBInLocalCurrency.Amount);
				AssertEquals("CIF in local currency", setup.ExpectedCIF * 2, entryHeader.CIFInLocalCurrency.Amount);
			});
		}

		[TestDate(2005, 6, 2)]
		public virtual void TestFOBInLocalCurrency()
		{
			RefCurrency newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			ZDateTime from = new ZDateTime(2005, 6, 1);
			ZDateTime to = new ZDateTime(2005, 6, 5);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

			BaseJobDeclaration declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			// note: setting JZ_ValuationDateOverride is only required for ZA
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			Assert("(pre-condition) invoiceHeader.EffectiveValuationDate.IsValid", invoiceHeader.EffectiveValuationDate.IsValid);
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;
			invoiceHeader.JZ_InvoiceCurrExRateType = "FIX";
			BaseJobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;
			DoMerge(declaration);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("FOB in local currency", 600m, entryHeader.FOBInLocalCurrency.Amount);
		}

		public virtual void TestEntriesOfHouseBillsRefreshed()
		{
			BaseJobDeclaration declaration = ImportJobDeclaration;
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			declaration.FillWithValidTestData();

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			SetInvoicesToResultInTwoEntries(invoice1.JobComInvoiceLines.AddNew(), invoice2.JobComInvoiceLines.AddNew());

			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();

			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			DoMerge(declaration);

			CusEntryHeader entry1 = invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
			CusEntryHeader entry2 = invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
			AssertEquals(true, entry1 != entry2);
			AssertEquals(true, new List<CusEntryHeader>(bill1.Entries).Contains(entry1));
			AssertEquals(true, new List<CusEntryHeader>(bill2.Entries).Contains(entry2));

			invoice1.JZ_CU_RelatedHouseBill = bill2.PK;
			invoice2.JZ_CU_RelatedHouseBill = bill1.PK;

			DoMerge(declaration);

			entry1 = invoice1.JobComInvoiceLines[0].CusEntryLine.Header;
			entry2 = invoice2.JobComInvoiceLines[0].CusEntryLine.Header;
			AssertEquals(true, entry1 != entry2);
			AssertEquals(true, new List<CusEntryHeader>(bill1.Entries).Contains(entry2));
			AssertEquals(true, new List<CusEntryHeader>(bill2.Entries).Contains(entry1));
		}

		public virtual void TestPackages()
		{
			BaseJobDeclaration declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.FillWithValidTestData();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "1";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "2";

			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = bill2.CU_BillUniqueCode;

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = bill.PK;
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			SetInvoicesToResultInTwoEntries(line1, line2);

			DoMerge(declaration);

			CusEntryHeader entry = line1.CusEntryLine.Header;
			CusEntryHeader entry2 = line2.CusEntryLine.Header;

			AssertNotEquals("(pre-condition) merge should result in two entries", entry, entry2);

			AssertEquals("entry's packages", true, entry.Packages.Contains(package));
			AssertEquals("entry's packages", false, entry.Packages.Contains(package2));
			AssertEquals("entry2's packages", false, entry2.Packages.Contains(package));
			AssertEquals("entry2's packages", true, entry2.Packages.Contains(package2));

			package2.CW_HouseBill = bill.CU_BillUniqueCode;

			AssertEquals("entry's packages", true, entry.Packages.Contains(package));
			AssertEquals("entry's packages", true, entry.Packages.Contains(package2));
			AssertEquals("entry2's packages", false, entry2.Packages.Contains(package2));
		}

		public void TestCH_CustomsMessageRemarks()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)GetNewBusinessObject();

			AssertEquals("EntryHeader.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
			AssertEquals("EntryHeader.CH_CustomsMessageRemarks", "", entryHeader.CH_CustomsMessageRemarks);
			AssertEquals("EntryHeader.Notes.HasNotes", false, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsMessageRemarks = "YAYAYA";
			AssertEquals("EntryHeader.CH_CustomsMessageRemarks", "YAYAYA", entryHeader.CH_CustomsMessageRemarks);
			AssertEquals("EntryHeader.Notes.HasNotes", true, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsMessageRemarks = "NANANA";
			AssertEquals("Message.CH_CustomsMessageRemarks", "NANANA", entryHeader.CH_CustomsMessageRemarks);
			AssertEquals("Message.Notes.HasNotes", true, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsMessageRemarks = "";
			AssertEquals("Message.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
			AssertEquals("Message.CH_CustomsMessageRemarks", "", entryHeader.CH_CustomsMessageRemarks);
			AssertEquals("Message.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
		}

		public void TestCH_CustomsDeliveryInstructions()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)GetNewBusinessObject();

			AssertEquals("EntryHeader.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
			AssertEquals("EntryHeader.CH_CustomsDeliveryInstructions", "", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("EntryHeader.Notes.HasNotes", false, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsDeliveryInstructions = "YAYAYA";
			AssertEquals("EntryHeader.CH_CustomsDeliveryInstructions", "YAYAYA", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("EntryHeader.Notes.HasNotes", true, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsDeliveryInstructions = "NANANA";
			AssertEquals("Message.CH_CustomsDeliveryInstructions", "NANANA", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("Message.Notes.HasNotes", true, entryHeader.Notes.HasNotes);

			entryHeader.CH_CustomsDeliveryInstructions = "";
			AssertEquals("Message.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
			AssertEquals("Message.CH_CustomsDeliveryInstructions", "", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("Message.Notes.HasNotes", false, entryHeader.Notes.HasNotes);
		}

		public void TestRandomHeaderWhenInvoiceHeadersAreDeleted()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = testDec.FilteredInvoiceLines.AddNew();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("RandomHeader", invoice, entryHeader.RandomHeader);

			invoice.Delete();
			Assert("RandomHeader", invoice != entryHeader.RandomHeader);
		}

		public void TestClearanceDate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Preassertion", 0, logs.Length);
			entryHeader.CH_Status = "ZZ";
			entryHeader.Logs.AddNew(Events.CustomsCleared);
			logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			var log = logs[0];
			AssertNotNull(log);
			AssertEquals(log.SL_EventTime, entryHeader.ClearanceDate);
		}

		public void TestDeclarationDate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert("Initialy no declaration date", entryHeader.DeclarationDate.IsEmpty);
			var date1 = ZDateTime.Today.AddDays(-1);
			declaration.JE_EntrySubmittedDate = date1;
			AssertEquals("Submitted date if no clearance date", date1, entryHeader.DeclarationDate);
			var logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Preassertion", 0, logs.Length);
			entryHeader.CH_Status = "ZZ";
			entryHeader.Logs.AddNew(Events.CustomsCleared);
			logs = entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			var log = logs[0];
			AssertNotNull(log);
			Assert(!entryHeader.DeclarationDate.IsEmpty);
			AssertEquals("Now clearance date", entryHeader.ClearanceDate, entryHeader.DeclarationDate);
		}

		public void TestAllEntryLinesAreEditibleChild()
		{
			CusEntryHeader header = (CusEntryHeader)GetNewBusinessObject();
			AssertEquals("AllEntryLines are registered as Editible Child", true, header.IsRegisteredEditableChildObject(header.AllEntryLines));
		}

		public void TestPendingDeletionEntryLines()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.DeletePending;

			CusEntryLine entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Active;

			CusEntryLine entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Deleted;

			entryHeader.MergedLines.Rebuild();
			entryHeader.PendingDeletionEntryLines.Rebuild();
			entryHeader.DeletedEntryLines.Rebuild();
			AssertEquals("Merged lines should only have active members", 1, entryHeader.MergedLines.Count);
			AssertEquals("Merged lines should only have active members", entryLine2, entryHeader.MergedLines[0]);

			AssertEquals("PendingDeletionEntryLines should only have pending members", 1, entryHeader.PendingDeletionEntryLines.Count);
			AssertEquals("PendingDeletionEntryLines should only have pending members", entryLine, entryHeader.PendingDeletionEntryLines[0]);

			AssertEquals("DeletedEntryLines should only have deleted members", 1, entryHeader.DeletedEntryLines.Count);
			AssertEquals("DeletedEntryLines should only have deleted members", entryLine3, entryHeader.DeletedEntryLines[0]);
		}

		public void TestDateForDutyRate()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(declaration.DateForDutyRate, entryHeader.DateForDutyRate);
		}

		[TestDate(2004, 12, 12)]
		public void TestDateForDutyRateGetsTodayIfNoDeclaration()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals(new ZDateTime(2004, 12, 12), entryHeader.DateForDutyRate);
		}

		public void TestCountyCode()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entry.CountryCode);
			var declaration = BaseJobDeclaration.New(Factory);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.CountryCode);
			entry.CH_JE = declaration.PK;
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entry.CountryCode);

			var otherCompanyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, declaration.Branch.GB_GC);
			var newCompany = Factory.LoadTop1<GlbCompany>(otherCompanyFilter);
			var branchInAnotherCompany = newCompany.Branches.AddNew();
			declaration.JE_GB = branchInAnotherCompany.PK;
			AssertEquals(newCompany.Country.RN_Code, declaration.CountryCode);
			AssertEquals(newCompany.Country.RN_Code, entry.CountryCode);
		}

		public virtual void TestIsMultiInvoiceCurrency()
		{
			var audCurrency = Core.Constants.CurrencyCodes.Australia;
			var uadCurrency = Core.Constants.CurrencyCodes.UnitedArabEmirates;
			var mockInvoice1 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			mockInvoice1.Setup(m => m.JZ_RX_NKInvoice_Currency).Returns(audCurrency);

			var mockInvoice2 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			mockInvoice2.Setup(m => m.JZ_RX_NKInvoice_Currency).Returns(uadCurrency);

			var multiCurrencyInvoiceHeaders = CreateTypedArray(new object[] { mockInvoice1.Object, mockInvoice2.Object });
			var singleCurrencyInvoiceHeader = CreateTypedArray(new object[] { mockInvoice1.Object });

			var mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])multiCurrencyInvoiceHeaders);
			AssertEquals("IsMultiInvoiceCurrency", true, mockCusEntryHeader.Object.IsMultiInvoiceCurrency);

			mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])singleCurrencyInvoiceHeader);
			AssertEquals("IsMultiInvoiceCurrency", false, mockCusEntryHeader.Object.IsMultiInvoiceCurrency);
		}

		public void TestIsMultiSupplier()
		{
			var mockInvoice1 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var supplier1 = OrgHeader.New(Factory);
			mockInvoice1.Setup(m => m.Supplier).Returns(supplier1);

			var mockInvoice2 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var supplier2 = OrgHeader.New(Factory);
			mockInvoice2.Setup(m => m.Supplier).Returns(supplier2);

			var multiSupplierInvoiceHeaders = CreateTypedArray(new object[] { mockInvoice1.Object, mockInvoice2.Object });
			var singleSupplierInvoiceHeader = CreateTypedArray(new object[] { mockInvoice1.Object });

			var mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])multiSupplierInvoiceHeaders);
			AssertEquals("IsMultiSupplier", true, mockCusEntryHeader.Object.IsMultiSupplier);

			mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])singleSupplierInvoiceHeader);
			AssertEquals("IsMultiSupplier", false, mockCusEntryHeader.Object.IsMultiSupplier);

			mockInvoice1.Verify(m => m.Supplier, Times.Exactly(6));
			mockInvoice2.Verify(m => m.Supplier, Times.Exactly(3));
		}

		public void TestSuppliers()
		{
			var mockInvoice1 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var supplier1 = OrgHeader.New(Factory);
			mockInvoice1.Setup(m => m.Supplier).Returns(supplier1);

			var mockInvoice2 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var supplier2 = OrgHeader.New(Factory);
			mockInvoice2.Setup(m => m.Supplier).Returns(supplier2);

			var singleSupplierInvoiceHeader = CreateTypedArray(new object[] { mockInvoice1.Object });
			var multiSupplierInvoiceHeaders = CreateTypedArray(new object[] { mockInvoice1.Object, mockInvoice2.Object });

			var mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])singleSupplierInvoiceHeader);
			AssertEquals("Suppliers count", 1, mockCusEntryHeader.Object.Suppliers.Count);

			mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])multiSupplierInvoiceHeaders);
			AssertEquals("Suppliers count", 2, mockCusEntryHeader.Object.Suppliers.Count);

			mockInvoice1.Verify(m => m.Supplier, Times.AtMost(8));
			mockInvoice2.Verify(m => m.Supplier, Times.AtMost(8));
			mockCusEntryHeader.Protected().Verify("GetInvoiceHeaders", Times.AtMost(1));
			mockCusEntryHeader.Protected().Verify("GetInvoiceHeaders", Times.AtMost(1));
		}

		public void TestOneSupplierMultiInvoices()
		{
			var mockInvoice1 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var mockInvoice2 = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var supplier1 = OrgHeader.New(Factory);

			mockInvoice1.Setup(m => m.Supplier).Returns(supplier1);
			mockInvoice2.Setup(m => m.Supplier).Returns(supplier1);

			var sameSupplierInvoiceHeaders = CreateTypedArray(new object[] { mockInvoice1.Object, mockInvoice2.Object });
			var mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockCusEntryHeader
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])sameSupplierInvoiceHeaders);
			AssertEquals("Suppliers count", 1, (mockCusEntryHeader.Object).Suppliers.Count);
			mockInvoice1.Verify(m => m.Supplier, Times.AtMost(8));
			mockInvoice2.Verify(m => m.Supplier, Times.AtMost(8));
		}

		public void TestTransactionValueReturnsAggregationOfInvoiceAmountOfMergedHeaders()
		{
			var declaration = GetNewBaseJobDeclaration();
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode))
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals(true, declaration.IsExport);
			var header1 = declaration.Invoices.AddNew();
			var header2 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			header2.JZ_InvoiceAmount = 2000m;
			header2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine1 = header1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			var invoiceLine2 = header2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				invoiceLine2.JI_CEI = instruction.PK;
			}

			DoMerge(declaration);

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(3000m, declaration.CustomsEntryHeaders[0].TransactionValue);
		}

		public virtual BaseJobDeclaration GetNewBaseJobDeclaration() => BaseJobDeclaration.New(Factory);

		public virtual void TestCurrencyConverter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invHeader = declaration.Invoices.AddNew();
			invHeader.JobComInvoiceLines.AddNew().JI_LinePrice = 800m;
			invHeader.JZ_InvoiceAmount = 1000m;
			invHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			OverrideValuationDate(invHeader, new ZDateTime(2005, 8, 1));

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CurrencyConverter.DateForRate", new ZDateTime(2005, 8, 1), entryHeader.CurrencyConverter.DateForRate);
		}

		protected virtual void OverrideValuationDate(BaseJobComInvoiceHeader invoice, ZDateTime date)
		{
			invoice.JZ_ValuationDateOverride = date;
		}

		public void TestBillsReturnsProxiedValuesFromDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_HouseBill = "HBL";
			declaration.JE_MasterBill = "MBL";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(1, entryHeader.Bills.Count);
			AssertEquals("HBL", entryHeader.Bills[0].CU_HouseBill);
			AssertEquals("MBL", entryHeader.Bills[0].CU_MasterBill);
		}

		public void TestHouseBillsCommaSeparated()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration.PK;

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_HouseBill = "HBL1";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "HBL2";

			BillCollectionForEntry billsCollection = new BillCollectionForEntry(entry);
			billsCollection.PopulateBills();

			AssertEquals(2, entry.Bills.Count);
			AssertEquals("HBL1,HBL2", entry.HouseBillsCommaSeparated);
		}

		public void TestMasterBillsCommaSeparated()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration.PK;

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_MasterBill = "MBL1";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_MasterBill = "MBL2";

			AssertEquals(2, entry.Bills.Count);
			AssertEquals("MBL1,MBL2", entry.MasterBillsCommaSeparated);
		}

		public void TestDirectMasterOrLinkedMasterBillsCommaSeparated()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration.PK;

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MBL1";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "MBL2";

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "HBL1";
			bill3.CU_CU_ParentBill = bill2.PK;

			BillCollectionForEntry billsCollection = new BillCollectionForEntry(entry);
			billsCollection.PopulateBills();

			AssertEquals(2, entry.Bills.Count);
			AssertEquals("MBL2,MBL1", entry.DirectMasterOrLinkedMasterBillsCommaSeparated);
		}

		public void TestGrossWeight()
		{
			var entryHeader = Factory.New<WrappedCusEntryHeader>();
			AssertEquals(new ZWeight(-1, "LB"), entryHeader.GrossWeight);
		}

		public void TestCusEntryNumDeleted()
		{
			CombineAssertions("CusEntryNum is deleted", () =>
			{
				var entryHeader = (CusEntryHeader)GetNewBusinessObject();
				entryHeader.EntryNumber = "TSTEntryNum";
				var entryNum = entryHeader.CusEntryNumber;

				AssertEquals("TSTEntryNum", entryNum.CE_EntryNum);

				entryHeader.Delete();
				AssertEquals(true, entryNum.IsDeleted);
			});
		}

		public void TestValueForVAT()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MergedLines.AddNew().CL_ValueForVAT = 1;
			entry.MergedLines.AddNew().CL_ValueForVAT = 2;
			entry.MergedLines.AddNew().CL_ValueForVAT = 3;

			CombineAssertions(() =>
			{
				AssertEquals("Called", 6m, entry.ValueForVAT);
				entry.MergedLines.AddNew().CL_ValueForVAT = 4;
				AssertEquals("Re-Called, unchanged", 6m, entry.ValueForVAT);
			});
		}

		public void TestResetIsValueForVATCalculated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MergedLines.AddNew().CL_ValueForVAT = 1;
			entry.MergedLines.AddNew().CL_ValueForVAT = 2;
			entry.MergedLines.AddNew().CL_ValueForVAT = 3;

			CombineAssertions(() =>
			{
				AssertEquals("Called", 6m, entry.ValueForVAT);
				entry.MergedLines.AddNew().CL_ValueForVAT = 4;
				AssertEquals("Re-Called, unchanged", 6m, entry.ValueForVAT);
				entry.ResetIsValueForVATCalculated();
				AssertEquals("Re-Calculated", 10m, entry.ValueForVAT);
			});
		}

		public void TestResetTotalsAndCachedValues_ResetIsValueForVATCalculated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MergedLines.AddNew().CL_ValueForVAT = 1;
			entry.MergedLines.AddNew().CL_ValueForVAT = 2;
			entry.MergedLines.AddNew().CL_ValueForVAT = 3;

			CombineAssertions(() =>
			{
				AssertEquals("Called", 6m, entry.ValueForVAT);
				entry.MergedLines.AddNew().CL_ValueForVAT = 4;
				AssertEquals("Re-Called, unchanged", 6m, entry.ValueForVAT);
				entry.ResetTotalsAndCachedValues();
				AssertEquals("Re-Calculated", 10m, entry.ValueForVAT);
			});
		}

		public virtual void TestGoodsTypeForDocumentFilter()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertNullOrEmpty(entry.GoodsTypeForDocumentFilter);
		}

		public virtual void TestGetPermitReference()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();

			entry.CH_BGMReference = "PermitTest";
			AssertEquals(entry.CH_BGMReference, entry.GetPermitReference());
		}

		public virtual void TestGetPermitReferenceNumberLine()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertEquals(0, entry.GetPermitReferenceNumberLine());
		}

		public void TestParentWorkflowProviders_ForDeclaration()
		{
			Assert("CusEntryHeader implements IWorkflowTriggerFieldChangeSource",
				typeof(IWorkflowTriggerFieldChangeSource).IsAssignableFrom(typeof(CusEntryHeader)));

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert("Precondition failed", entryHeader != null);

			IWorkflowTriggerFieldChangeSource workflowTriggerChangeSource = entryHeader;
			AssertCollectionContains(declaration, workflowTriggerChangeSource.ParentWorkflowProviders);
		}

		#region ICustomsFileParent

		public void TestICustomsFileParent_GetDeclarationTypeFromInfo()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = (ICustomsFileParent)declaration.ActiveEntryHeaders.AddNew();

			((ICustomsFileParent)declaration).DeclarationTypeInfo.SetValueFromString("AAA");

			AssertEquals("Should get value from the value of Declaration.", "AAA", entryHeader.DeclarationType);
		}

		public void TestICustomsFileParent_BranchPk()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entryHeader = (ICustomsFileParent)declaration.ActiveEntryHeaders.AddNew();

			AssertEquals("Should get value from the branch of Declaration.", branch.PK, entryHeader.BranchPk);
		}

		public void TestICustomsFileParent_IsLocked()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var entryHeader = (ICustomsFileParent)declaration.ActiveEntryHeaders.AddNew();
			var log = entryHeader.Logs.AddNew(AutoEvents.UnlockForEdit);

			AssertEquals("Should not be locked as there is no active LCK log.", false, entryHeader.IsLocked);

			log = entryHeader.Logs.AddNew(AutoEvents.LockForEdit);
			AssertEquals("Should be locked as there is an active LCK log.", true, entryHeader.IsLocked);

			log.Cancel();
			AssertEquals("Should not be locked as there is no active LCK log.", false, entryHeader.IsLocked);
		}

		public void TestICustomsFileParent_LockFile()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var log1 = entryHeader.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = entryHeader.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = entryHeader.Logs.AddNew(AutoEvents.Attached);

			var fileParent = (ICustomsFileParent)entryHeader;
			fileParent.LockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);
		}

		public void TestICustomsFileParent_UnlockFile()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = (ICustomsFileParent)declaration.ActiveEntryHeaders.AddNew();

			var log1 = entryHeader.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = entryHeader.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = entryHeader.Logs.AddNew(AutoEvents.Attached);

			entryHeader.UnlockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = entryHeader.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNotNull("Should add a new UCK event.", newLog);
		}

		#endregion

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = ImportJobDeclaration;
			var result = (CusEntryHeader)base.GetBusinessObjectForFetchForLoad();
			result.CH_JE = declaration.PK;
			return result;
		}
		protected virtual (ZString OverseasFreightChargeCode, ZString OverseasInsuranceChargeCode, ZString NotIncludedChargeCode) GetChargeCodesForTotalTAndI()
			=> (CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Codes.PackingCost);

		protected virtual void DoMerge(BaseJobDeclaration declaration)
		{
			SendsMessagesToCustomsShutterUpperer mergeResult = new SendsMessagesToCustomsShutterUpperer(false);
			mergeResult.AnswerToContinueWithAction = true;
			declaration.DoMerge(mergeResult);
		}

		protected virtual void SetInvoicesToResultInTwoEntries(BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2)
		{
			line1.InvoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			line2.InvoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
		}

		object[] CreateTypedArray(object[] untypedElements)
		{
			Type elementType = untypedElements[0].GetType();
			ArrayList list = new ArrayList(untypedElements);
			return (object[])list.ToArray(elementType);
		}

		protected virtual string DefaultExportMessageType => JobMessageTypeList.Codes.Export;

		protected virtual ZString ImportJobMessage => JobMessageTypeList.Codes.Import;

		protected virtual BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = Factory.New<BaseJobDeclaration>();
				result.JE_MessageType = ImportJobMessage;
				return result;
			}
		}

		protected override BaseJobDeclaration GetNewDeclaration() => ImportJobDeclaration;

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected virtual bool RatesAreReciprocal => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			header.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			header.PendingDeletionEntryLines.AddNew();
			header.AllEntryLines.Load();
			header.PivotsToContainers.RemoveAndDeleteAll();
			header.PivotsToContainers.GetOrCreatePivotFor(header.Declaration.CusContainers.AddNew());
			return header;
		}

		protected interface IChargesCurrencyTestSetup
		{
			void SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode);

			ZDecimal ExpectedFOB { get; }
			ZDecimal ExpectedCIF { get; }
		}

		protected virtual IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				if (declaration.IsDeclarationIntegrated)
				{
					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
				}

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 9800m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10300m;
		}

		class WrappedWeightUQCalculator : WeightUQCalculator
		{
			public override string UQ
			{
				get { return "LB"; }
			}

			public override ZDecimal Weight
			{
				get { return -1m; }
			}
		}

		class WrappedCusEntryHeader : CusEntryHeader
		{
			public WrappedCusEntryHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override WeightUQCalculator GetWeightCalculator()
			{
				return new WrappedWeightUQCalculator();
			}
		}

		class CanNotBeSavedCusEntryHeader : CusEntryHeader
		{
			public CanNotBeSavedCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				if (CanNotBeSaved)
				{
					throw new Exception("Do not save.");
				}
			}

			public bool CanNotBeSaved { get; set; }
		}
	}
}
