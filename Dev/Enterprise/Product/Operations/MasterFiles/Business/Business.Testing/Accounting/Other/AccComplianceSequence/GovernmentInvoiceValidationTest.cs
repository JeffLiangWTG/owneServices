using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GovernmentInvoiceValidationTest : AccTransactionHeaderValidationTest
	{
		public void TestGovernmentInvoice_CheckAH_ComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Peru))
			{
				GovernmentInvoice header = Factory.NewWithValidTestData<GovernmentInvoice>();
				header.AH_ComplianceSubType = "TXI";
				header.AH_TransactionReference = "123456";
				AssertNoErrors("Precondition: SubType should not have errors.", header.AH_ComplianceSubTypeInfo);
				Factory.Save();

				header.AH_ComplianceSubType = "";
				AssertHasErrors("SubType can't be blank if transaction reference is not blank", header.AH_ComplianceSubTypeInfo);

				header.AH_ComplianceSubType = "TXI";
				AssertNoErrors(header.AH_ComplianceSubTypeInfo);

				header.AH_TransactionReference = "";
				header.AH_ComplianceSubType = "";
				AssertNoErrors("SubType can be blank if transaction reference is blank", header.AH_ComplianceSubTypeInfo);

				header.AH_ComplianceSubType = "AAA";
				AssertHasErrors("SubType has invalid code", header.AH_ComplianceSubTypeInfo);
			}
		}

		public void TestCheckAH_ComplianceSubType_China()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			var complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();

			complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns("error message");
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);

			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionReference = "123456";
			var invoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine.AL_AH = invoice.PK;

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				invoice.AH_ComplianceSubType = "TXB";
				Factory.Save();

				Assert("AH_ComplianceSubType HasChanges", !invoice.AH_ComplianceSubTypeInfo.HasChanges);
				AssertHasError(invoice.AH_ComplianceSubTypeInfo, "error message");
			}

			countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();

			complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns(string.Empty);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				invoice.AH_ComplianceSubType = "TXA";
				Factory.Save();

				Assert("AH_ComplianceSubType HasChanges", !invoice.AH_ComplianceSubTypeInfo.HasChanges);
				AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);
			}
		}

		public void TestCheckAH_ComplianceSubType_IsComplianceSubTypeProtected()
		{
			var protectionErrorMessage = "You cannot change the original 'PIN' value of Compliance Sub Type for Payables e-Reporting transactions.";

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				apInvoice.AH_TransactionType = TransactionTypes.Invoice;
				apInvoice.AH_ComplianceSubType = "PIN";
				AssertNoErrors(apInvoice.AH_ComplianceSubTypeInfo);
				Factory.Save();

				var governmentInvoice = Factory.Load<GovernmentInvoice>(apInvoice.PK);

				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
				var complianceSubTypeProtectValidation = new Mock<IProtectComplianceSubTypeForEInvoicingTransactions>();
				var complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();
				complianceSubTypeProtectValidation.Setup(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>())).Returns(protectionErrorMessage);
				complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns("");
				countryComplianceFactoryMock.Setup(c => c.GetIProtectComplianceSubTypeForEInvoicingTransactions(It.IsAny<ZString>())).Returns(complianceSubTypeProtectValidation.Object);
				countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);
				ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

				var mockProxy = new Mock<IEInvoicingTransactionProxyFactory>();
				var eInvoicingTransactionMock = new Mock<IEInvoicingTransaction>();
				mockProxy.Setup(x => x.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(eInvoicingTransactionMock.Object);
				eInvoicingTransactionMock.SetupGet(x => x.CurrentStatus).Returns("SUC");
				ObjectFactory.Substitute(mockProxy.Object);

				governmentInvoice.AH_ComplianceSubType = "PIC";
				AssertHasError(governmentInvoice.AH_ComplianceSubTypeInfo, protectionErrorMessage);

				complianceSubTypeProtectValidation.Verify(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
				complianceSubTypeValidation.Verify(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
				eInvoicingTransactionMock.Verify(x => x.CurrentStatus, Times.Never);

				complianceSubTypeProtectValidation.Setup(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>())).Returns("");

				governmentInvoice.Validation.ValidateAH_ComplianceSubType();
				AssertNoError(governmentInvoice.AH_ComplianceSubTypeInfo, protectionErrorMessage);

				complianceSubTypeProtectValidation.Verify(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>()), Times.Exactly(3));
				complianceSubTypeValidation.Verify(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>()), Times.Exactly(3));
				eInvoicingTransactionMock.Verify(x => x.CurrentStatus, Times.Once);
			}
		}

		public void TestCheckAH_ComplianceSubType_IfGovernmentInvoiceIsSubmittedForEInvoicing()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_ComplianceSubType = "EIN";
				AssertNoErrors(arInvoice.AH_ComplianceSubTypeInfo);
				Factory.Save();

				var mockProxy = new Mock<IEInvoicingTransactionProxyFactory>();
				var eInvoicingTransactionMock = new Mock<IEInvoicingTransaction>();
				mockProxy.Setup(x => x.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(eInvoicingTransactionMock.Object);
				eInvoicingTransactionMock.SetupGet(x => x.CurrentStatus).Returns("SUC");
				ObjectFactory.Substitute(mockProxy.Object);

				var arGovernmentInvoice = Factory.Load<GovernmentInvoice>(arInvoice.PK);
				arGovernmentInvoice.AH_ComplianceSubType = "EAR";
				AssertHasError(arGovernmentInvoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'EIN' as it has already been submitted for E-Invoicing.");
				mockProxy.Verify(x => x.GetProxy(It.IsAny<AccTransactionHeader>()), Times.Once);
				eInvoicingTransactionMock.Verify(x => x.CurrentStatus, Times.Exactly(2));

				var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				apInvoice.AH_TransactionType = TransactionTypes.Invoice;
				apInvoice.AH_ComplianceSubType = "PIN";
				AssertNoErrors(apInvoice.AH_ComplianceSubTypeInfo);
				mockProxy.Verify(x => x.GetProxy(It.IsAny<AccTransactionHeader>()), Times.Once);
				eInvoicingTransactionMock.Verify(x => x.CurrentStatus, Times.Exactly(2));
				Factory.Save();

				var apGovernmentInvoice = Factory.Load<GovernmentInvoice>(apInvoice.PK);
				apGovernmentInvoice.AH_ComplianceSubType = "PAR";
				AssertHasError(apGovernmentInvoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'PIN' as it has already been submitted for E-Invoicing.");
				mockProxy.Verify(x => x.GetProxy(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
				eInvoicingTransactionMock.Verify(x => x.CurrentStatus, Times.Exactly(4));
			}
		}

		public void TestCheckAH_ComplianceSubType_WarningMessage()
		{
			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
				{
					var arInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
					arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
					arInvoice.AH_TransactionType = TransactionTypes.Invoice;
					arInvoice.AH_ComplianceSubType = "101";

					AssertHasWarning(arInvoice.AH_ComplianceSubTypeInfo, "You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.");
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
				{
					var arInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
					arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
					arInvoice.AH_TransactionType = TransactionTypes.Invoice;
					arInvoice.AH_ComplianceSubType = "101";

					AssertNoWarnings(arInvoice.AH_ComplianceSubTypeInfo);
				}
			}
		}

		public void TestGovernmentInvoice_CheckAH_TransactionReference()
		{
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Peru))
			{
				var guid_GC = GlbCompany.CurrentCompany.PK;
				complianceSequence.XD_GC_Company = guid_GC;
				Factory.Save();

				var header = Factory.NewWithValidTestData<GovernmentInvoice>();
				header.AH_GC = guid_GC;
				header.AH_ComplianceSubType = "TXI";
				header.AH_TransactionReference = "abc-0010001";
				Factory.Save();
				AssertEquals("should find sequence", complianceSequence, header.ComplianceSequenceFromTransactionReference);

				header.AH_TransactionReference = "abc-0010077"; //UAT test 7
				AssertHasError(header.AH_TransactionReferenceInfo, "The number overlaps with existing compliance sequence book");

				header.AH_TransactionReference = "abc-0010001";
				AssertNoErrors(header.AH_TransactionReferenceInfo);

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					header.AH_TransactionReference = ""; //UAT test 5
					AssertNoErrors("Transaction Reference can be blank when posting is not ordered by Post Date", header.AH_TransactionReferenceInfo);
				}

				var emptyComplianceSubtypeDisallowedRegistryInstance = GovernmentInvoiceValidation.GetEmptyComplianceSubtypeDisallowedRegistryInstance();

				foreach (var dateOption in new[] { AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code })
				{
					using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
					using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					using (emptyComplianceSubtypeDisallowedRegistryInstance.SetTemporaryValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						header.AH_TransactionReference = "abc-0010001";
						AssertNoErrors(header.AH_TransactionReferenceInfo);

						header.AH_TransactionReference = ""; //UAT test 11
						AssertHasError("Transaction Reference can't be blank when allocation is on posting and ordered by Post Date",
							header.AH_TransactionReferenceInfo, "Please enter a Class A Invoice Number.");
					}
				}
			}
		}

		public void TestCheckSavedAH_TransactionReferenceCannotBeChangedForPortugal()
		{
			var header = Factory.NewWithValidTestData<GovernmentInvoice>();
			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.AH_ComplianceSubType = "TXI";
			header.AH_TransactionReference = "FAC 001/000001";
			AssertNoErrors(header.AH_TransactionReferenceInfo);

			Factory.Save();
			header.AH_TransactionReference = "FAC 001/000002";
			AssertNoErrors(header.AH_TransactionReferenceInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				Assert("Pre-condition", header.Company.Country.SupportDocumentSigning);
				var expectedError = "This number cannot be changed from it's original value 'FAC 001/000001' as it is used for Digital Signature.";
				header.Validation.ValidateAH_TransactionReference();
				AssertHasError(header.AH_TransactionReferenceInfo, expectedError);

				header.AH_TransactionReference = ZString.Empty;
				AssertHasError(header.AH_TransactionReferenceInfo, expectedError);

				Factory.Save();
				header.Validation.ValidateAH_TransactionReference();
				AssertNoErrors(header.AH_TransactionReferenceInfo);

				header.AH_TransactionReference = "FAC 001/000003";
				expectedError = "This number cannot be changed from it's original value '' as it is used for Digital Signature.";
				AssertHasError(header.AH_TransactionReferenceInfo, expectedError);

				Factory.Save();
				header.Validation.ValidateAH_TransactionReference();
				AssertNoError(header.AH_TransactionReferenceInfo, expectedError);

				header.AH_TransactionReference = "FAC 001/000004";
				expectedError = "This number cannot be changed from it's original value 'FAC 001/000003' as it is used for Digital Signature.";
				AssertHasError(header.AH_TransactionReferenceInfo, expectedError);
			}

			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.Validation.ValidateAH_TransactionReference();
			AssertNoErrors(header.AH_TransactionReferenceInfo);
		}

		public void TestAH_ComplianceSubTypeCannotBeChangedForPortugal()
		{
			var header = Factory.NewWithValidTestData<GovernmentInvoice>();
			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.AH_ComplianceSubType = "TXI";
			AssertNoErrors(header.AH_ComplianceSubTypeInfo);

			Factory.Save();
			header.AH_ComplianceSubType = "TCR";
			AssertNoErrors(header.AH_ComplianceSubTypeInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				Assert("Pre-condition", header.Company.Country.SupportDocumentSigning);
				var expectedError = "Compliance Sub Type cannot be changed from it's original value 'TXI' as allocated for it number is used for Digital Signature.";
				header.Validation.ValidateAH_ComplianceSubType();
				AssertHasError(header.AH_ComplianceSubTypeInfo, expectedError);

				header.AH_ComplianceSubType = ZString.Empty;
				AssertHasError(header.AH_ComplianceSubTypeInfo, expectedError);

				Factory.Save();
				header.Validation.ValidateAH_ComplianceSubType();
				AssertNoErrors(header.AH_ComplianceSubTypeInfo);

				header.AH_ComplianceSubType = "TXI";
				AssertNoErrors(header.AH_ComplianceSubTypeInfo);

				Factory.Save();
				header.AH_ComplianceSubType = "TCR";
				AssertHasError(header.AH_ComplianceSubTypeInfo, expectedError);
			}

			Assert("Pre-condition", !header.Company.Country.SupportDocumentSigning);
			header.Validation.ValidateAH_ComplianceSubType();
			AssertNoErrors(header.AH_ComplianceSubTypeInfo);
		}

		AccComplianceSequence sequence;
		readonly ZDate dateStart = new ZDate(2019, 1, 1);
		readonly ZDate date2307 = new ZDate(2019, 7, 23); //1st date used
		readonly ZDate date2407 = new ZDate(2019, 7, 24);
		readonly ZDate date0909 = new ZDate(2019, 9, 9);
		readonly ZDate date1909 = new ZDate(2019, 9, 19);
		readonly ZDate date2109 = new ZDate(2019, 9, 21);
		readonly ZDate date2609 = new ZDate(2019, 9, 26);
		readonly ZDate date0110 = new ZDate(2019, 10, 1);
		readonly ZDate date0812 = new ZDate(2019, 12, 8); //last date used
		readonly ZDate dateExpiry = new ZDate(2019, 12, 31);

		void SetupComplianceSequenceForCountry()
		{
			sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = SpainComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Code = "AR1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 99;
			sequence.XD_MaximumNumberDigits = 2;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_StartDate = dateStart;
			sequence.XD_ExpiryDate = dateExpiry;
			sequence.XD_IsActive = true;
			sequence.XD_Prefix = "00233";
		}

		void SetupForValidateTransactionReferenceInComplianceSequence(string complianceNumberAllocationDateOption)
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Spain);
			var guid_GC = GlbCompany.CurrentCompany.PK;

			SetupComplianceSequenceForCountry();

			CreateInvoice(sequence, "01", date2307, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "03", date2307, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "04", date2407, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "05", date2407, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "06", date0909, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "07", date1909, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "07/A", date1909, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "08", date2109, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "08/Z", date2109, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "09", date2609, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "09/A", date2609, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "15", date0110, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "16", date0110, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "18", date0812, complianceNumberAllocationDateOption);
			CreateInvoice(sequence, "20", date0812, complianceNumberAllocationDateOption);

			sequence.XD_NextNumber = 21;

			AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, complianceNumberAllocationDateOption);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			Factory.Save();
		}

		public void TestValidateTransactionReferenceNotInUse_AR_PST()
		{
			AssertValidateTransactionReferenceNotInUse(LedgerTypes.AccountsReceivable, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidateTransactionReferenceNotInUse_AR_INV()
		{
			AssertValidateTransactionReferenceNotInUse(LedgerTypes.AccountsReceivable, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void TestValidateTransactionReferenceNotInUse_AP_PST()
		{
			AssertValidateTransactionReferenceNotInUse(LedgerTypes.AccountsPayable, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		string GetDateLabel(string dateOption)
		{
			return dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code ?
				AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Description : AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Description;
		}

		void AssertValidateTransactionReferenceNotInUse(string ledger, string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);

			string registryARValue = ledger == LedgerTypes.AccountsReceivable ? dateOption : AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;
			string registryAPValue = ledger == LedgerTypes.AccountsPayable ? dateOption : AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryARValue))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryAPValue))
			{
				var lastDateUsedTransaction = CreateInvoice(sequence, "21", date0812, dateOption);
				lastDateUsedTransaction.AH_Ledger = ledger;
				Factory.Save();

				var prefix = sequence.XD_Prefix;
				var invTest = CreateInvoice(sequence, "00", date2307, dateOption);
				invTest.AH_Ledger = ledger;
				Factory.Save();

				invTest.AH_TransactionReference = prefix + "01"; //UAT test case 10.2
				expectErrorAlreadyUsed(invTest);
				expectSuggestion(invTest, "02");
				invTest.AH_TransactionReference = prefix + "03";
				expectErrorAlreadyUsed(invTest);
				expectSuggestion(invTest, "02");
				invTest.AH_TransactionReference = prefix + "02";
				AssertNoErrors(invTest.AH_TransactionReferenceInfo);

				SetInvoiceDate(invTest, date2407, dateOption);
				invTest.AH_TransactionReference = prefix + "07/A";
				expectErrorAlreadyUsed(invTest);
				expectSuggestion(invTest, "05/A");
				invTest.AH_TransactionReference = prefix + "05/A";
				AssertNoErrors(invTest.AH_TransactionReferenceInfo);

				SetInvoiceDate(invTest, date1909, dateOption);
				invTest.AH_TransactionReference = prefix + "07/A";
				expectErrorAlreadyUsed(invTest);
				expectSuggestion(invTest, "07/B");
				invTest.AH_TransactionReference = prefix + "07/B";
				AssertNoErrors(invTest.AH_TransactionReferenceInfo);

				SetInvoiceDate(invTest, date2109, dateOption);
				invTest.AH_TransactionReference = prefix + "08/A";
				AssertNoErrors(invTest.AH_TransactionReferenceInfo);
				invTest.AH_TransactionReference = prefix + "08/Z"; //UAT test case 10.3 
				expectErrorAlreadyUsed(invTest);
				var dateLabel = GetDateLabel(dateOption);
				AssertHasError(invTest.AH_TransactionReferenceInfo, $"No suggestions have been found for this {dateLabel}");

				SetInvoiceDate(invTest, date0110, dateOption);
				invTest.AH_TransactionReference = prefix + "08"; //UAT test 4.3
				expectErrorAlreadyUsed(invTest);
				expectSuggestion(invTest, "14");
				invTest.AH_TransactionReference = prefix + "14";
				AssertNoErrors(invTest.AH_TransactionReferenceInfo);
			}

			void expectErrorAlreadyUsed(GovernmentInvoice inv)
			{
				AssertHasError(inv.AH_TransactionReferenceInfo, "This Compliance Number is already used for the specified Compliance Sub Type");
			}
		}

		void SetInvoiceDate(GovernmentInvoice invoice, ZDateTime date, string dateOption)
		{
			if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_InvoiceDate = date;
			}
			else if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
			{
				invoice.AH_PostDate = date;
			}
		}

		public void TestValidateTransactionReferenceIsInRange_PST()
		{
			AssertValidateTransactionReferenceIsInRange(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidateTransactionReferenceIsInRange_INV()
		{
			AssertValidateTransactionReferenceIsInRange(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertValidateTransactionReferenceIsInRange(string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);
			var prefix = sequence.XD_Prefix;
			var invTest = CreateInvoice(sequence, "00", date0909, dateOption);
			Factory.Save();

			invTest.AH_TransactionReference = prefix + "999";
			expectErrorNotInRange(invTest);
			expectSuggestion(invTest, "06/A");
			invTest.AH_TransactionReference = prefix + "06/A";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			var invTest2 = CreateInvoice(sequence, "10", date2609, dateOption);
			Factory.Save();

			invTest2.AH_TransactionReference = "AS2-10"; //UAT test case 1 / 7 / 10.4
			expectErrorNotInRange(invTest2);
			expectSuggestion(invTest2, "10");
			invTest2.AH_TransactionReference = prefix + "10";
			AssertNoErrors(invTest2.AH_TransactionReferenceInfo);

			void expectErrorNotInRange(GovernmentInvoice inv)
			{
				var maxNumDigits = sequence.XD_MaximumNumberDigits;
				AssertHasError(inv.AH_TransactionReferenceInfo, string.Format("This Compliance Number is not between Start ({0}{1}) and End number ({0}{2}) for the specified Compliance Sub Type",
					prefix, sequence.XD_StartNumber.ToString().PadLeft(maxNumDigits, '0'), sequence.XD_EndNumber.ToString().PadLeft(maxNumDigits, '0')));
			}
		}

		public void TestValidateTransactionReferenceToNextNumber_PST()
		{
			AssertValidateTransactionReferenceToNextNumber(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidateTransactionReferenceToNextNumber_INV()
		{
			AssertValidateTransactionReferenceToNextNumber(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertValidateTransactionReferenceToNextNumber(string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);
			var prefix = sequence.XD_Prefix;
			var invTest = CreateInvoice(sequence, "00", date0909, dateOption);
			Factory.Save();

			invTest.AH_TransactionReference = prefix + "55/B";
			expectErrorHigherThanNext(invTest);
			expectSuggestion(invTest, "06/A");
			invTest.AH_TransactionReference = prefix + "06/A";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			SetInvoiceDate(invTest, date0110, dateOption);
			invTest.AH_TransactionReference = prefix + "22"; //UAT test 8
			expectErrorHigherThanNext(invTest);
			expectSuggestion(invTest, "14");
			invTest.AH_TransactionReference = prefix + "14";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			void expectErrorHigherThanNext(GovernmentInvoice inv)
			{
				AssertHasError(inv.AH_TransactionReferenceInfo, $"This Compliance Number is higher than the next number ({prefix}{sequence.XD_NextNumber}) for the specified Compliance Sub Type");
			}
		}

		public void TestValidateTransactionReferenceToPostDate()
		{
			AssertValidateTransactionReferenceToAllocationDate(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidateTransactionReferenceToInvoiceDate()
		{
			AssertValidateTransactionReferenceToAllocationDate(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertValidateTransactionReferenceToAllocationDate(string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);
			var prefix = sequence.XD_Prefix;
			var invTest = CreateInvoice(sequence, "00", date0909, dateOption);
			Factory.Save();

			invTest.AH_TransactionReference = prefix + "01/A"; //UAT test 3
			expectErrorNotAllowed(invTest, date0909);
			expectSuggestion(invTest, "06/A");
			invTest.AH_TransactionReference = prefix + "06/A";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			invTest.AH_TransactionReference = prefix + "14"; //UAT test 2
			expectErrorNotAllowed(invTest, date0909);
			expectSuggestion(invTest, "06/A");
			invTest.AH_TransactionReference = prefix + "06/A";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			SetInvoiceDate(invTest, date2609, dateOption);
			invTest.AH_TransactionReference = prefix + "17";
			expectErrorNotAllowed(invTest, date2609);
			expectSuggestion(invTest, "10");
			invTest.AH_TransactionReference = prefix + "10";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			SetInvoiceDate(invTest, date0110, dateOption);
			invTest.AH_TransactionReference = prefix + "19"; //UAT test 4.1
			expectErrorNotAllowed(invTest, date0110);
			expectSuggestion(invTest, "14");
			invTest.AH_TransactionReference = prefix + "14";
			AssertNoErrors(invTest.AH_TransactionReferenceInfo);

			void expectErrorNotAllowed(GovernmentInvoice inv, ZDate date)
			{
				var dateLabel = GetDateLabel(dateOption);
				AssertHasError(inv.AH_TransactionReferenceInfo, $"This Compliance Number is not allowed for this {dateLabel} ({date.ToShortDateString()})");
			}
		}

		public void TestValidatePostDateIsPriorToLastDateUsed_PST()
		{
			AssertValidatePostDateIsPriorToLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidatePostDateIsPriorToLastDateUsed_INV()
		{
			AssertValidatePostDateIsPriorToLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertValidatePostDateIsPriorToLastDateUsed(string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);
			var prefix = sequence.XD_Prefix;
			var invTest = CreateInvoice(sequence, "21", date0812, dateOption);
			Factory.Save();

			invTest.AH_TransactionReference = prefix + "20"; //UAT test 4.2
			expectErrorLaterLastDateUsed(invTest, date0812);
			invTest.AH_TransactionReference = "20";
			expectErrorLaterLastDateUsed(invTest, date0812);

			invTest.AH_TransactionReference = prefix + "22"; //UAT test 9
			expectErrorLaterLastDateUsed(invTest, date0812);
			invTest.AH_TransactionReference = "22";
			expectErrorLaterLastDateUsed(invTest, date0812);

			void expectErrorLaterLastDateUsed(GovernmentInvoice inv, ZDate date)
			{
				var dateLabel = GetDateLabel(dateOption);
				var invDate = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code ? inv.AH_PostDate : inv.AH_InvoiceDate;
				AssertHasError(inv.AH_TransactionReferenceInfo, $"{dateLabel} ({invDate.Date.ToShortDateString()}) is later than or equal to Last Date used ({date.ToShortDateString()}) for this Compliance Book. Please use 'Allocate' action");
			}
		}

		public void TestValidateTransactionReferenceWithOpenRange_PST()
		{
			AssertValidateTransactionReferenceWithOpenRange(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestValidateTransactionReferenceWithOpenRange_INV()
		{
			AssertValidateTransactionReferenceWithOpenRange(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertValidateTransactionReferenceWithOpenRange(string dateOption)
		{
			SetupForValidateTransactionReferenceInComplianceSequence(dateOption);
			var prefix = sequence.XD_Prefix;
			var invTest = CreateInvoice(sequence, "02", date2307, dateOption);

			sequence.XD_StartDate = ZDate.Empty; //UAT test 6
			Factory.Save();
			invTest.AH_TransactionReference = prefix + "01";
			AssertNoExceptionThrown(Factory.Save);

			invTest.AH_TransactionReference = prefix + "02";
			sequence.XD_StartDate = dateStart;
			sequence.XD_ExpiryDate = ZDate.Empty;
			Factory.Save();
			invTest.AH_TransactionReference = prefix + "03";
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestInvalidAvailableTransRef_PST()
		{
			AssertInvalidAvailableTransRef(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestInvalidAvailableTransRef_INV()
		{
			AssertInvalidAvailableTransRef(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertInvalidAvailableTransRef(string dateOption)
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Portugal);
			var guid_GC = GlbCompany.CurrentCompany.PK;

			SetupComplianceSequenceForCountry();

			CreateInvoice(sequence, "04", date2307, dateOption);
			CreateInvoice(sequence, "05", date0909, dateOption);
			CreateInvoice(sequence, "01", date2407, dateOption);
			CreateInvoice(sequence, "02", date2407, dateOption);
			CreateInvoice(sequence, "03", date0909, dateOption);

			sequence.XD_NextNumber = 4;

			AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, dateOption);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(guid_GC.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var invTest = CreateInvoice(sequence, "00", date2407, dateOption);
			Factory.Save();
			var prefix = sequence.XD_Prefix;

			var dateLabel = GetDateLabel(dateOption);
			invTest.AH_TransactionReference = prefix + "04"; //UAT test 10.1 
			AssertHasError(invTest.AH_TransactionReferenceInfo, "This Compliance Number is already used for the specified Compliance Sub Type");
			AssertHasError(invTest.AH_TransactionReferenceInfo, $"No suggestions have been found for this {dateLabel}");
		}

		void expectSuggestion(GovernmentInvoice inv, string number)
		{
			AssertHasError(inv.AH_TransactionReferenceInfo, "You can assign the following number: " + sequence.XD_Prefix + number);
		}

		#region E-Invoicing Related Tests

		public void TestComplianceFields_CannotBeChanged_ForEInvoicingPivotStatus()
		{
			AssertEquals("Precondition: no weird compliance rules apply for AU", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var savedInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			savedInvoice.AH_ComplianceSubType = "ABC";
			savedInvoice.AH_TransactionReference = "1234";
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var possiblePivotStatuses = GetConstantValues(typeof(EInvoicingPivotState)).Concat(new[] { string.Empty });
				foreach (var pivotStatus in possiblePivotStatuses)
				{
					var mockFactory = CreateMockEInvoicingTransactionFactory(pivotStatus);
					using (ObjectFactory.Substitute(mockFactory.Object))
					{
						var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
						var invoice = newFactory.Load<GovernmentInvoice>(savedInvoice.PK);
						AssertEquals("Precondition", savedInvoice.AH_ComplianceSubType, invoice.AH_ComplianceSubType);
						AssertEquals("Precondition", savedInvoice.AH_TransactionReference, invoice.AH_TransactionReference);
						AssertNoError(invoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'ABC' as it has already been submitted for E-Invoicing.");
						AssertNoError(invoice.AH_TransactionReferenceInfo, "Compliance Number cannot be changed from '1234' as it has already been submitted for E-Invoicing.");

						invoice.AH_ComplianceSubType = "DEF";
						invoice.AH_TransactionReference = "5678";

						AssertNoError(invoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'ABC' as it has already been submitted for E-Invoicing.");
						AssertNoError(invoice.AH_TransactionReferenceInfo, "Compliance Number cannot be changed from '1234' as it has already been submitted for E-Invoicing.");
					}
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var possiblePivotStatuses = GetConstantValues(typeof(EInvoicingPivotState))
											.Concat(new[] { string.Empty });
				var invalidPivotStatuses = new[]
				{
					Constants.EInvoicingPivotState.Sent,
					Constants.EInvoicingPivotState.Succeed,
					Constants.EInvoicingPivotState.Delivered,
					Constants.EInvoicingPivotState.Batched,
				};

				foreach (var pivotStatus in possiblePivotStatuses)
				{
					var mockFactory = CreateMockEInvoicingTransactionFactory(pivotStatus);
					using (ObjectFactory.Substitute(mockFactory.Object))
					{
						var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
						var invoice = newFactory.Load<GovernmentInvoice>(savedInvoice.PK);
						AssertEquals("Precondition", savedInvoice.AH_ComplianceSubType, invoice.AH_ComplianceSubType);
						AssertEquals("Precondition", savedInvoice.AH_TransactionReference, invoice.AH_TransactionReference);
						AssertNoError(invoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'ABC' as it has already been submitted for E-Invoicing.");
						AssertNoError(invoice.AH_TransactionReferenceInfo, "Compliance Number cannot be changed from '1234' as it has already been submitted for E-Invoicing.");

						invoice.AH_ComplianceSubType = "DEF";
						invoice.AH_TransactionReference = "5678";

						if (invalidPivotStatuses.Contains(pivotStatus))
						{
							AssertHasError(invoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'ABC' as it has already been submitted for E-Invoicing.");
							AssertHasError(invoice.AH_TransactionReferenceInfo, "Compliance Number cannot be changed from '1234' as it has already been submitted for E-Invoicing.");
						}
						else
						{
							AssertNoError(invoice.AH_ComplianceSubTypeInfo, "Compliance Sub Type cannot be changed from 'ABC' as it has already been submitted for E-Invoicing.");
							AssertNoError(invoice.AH_TransactionReferenceInfo, "Compliance Number cannot be changed from '1234' as it has already been submitted for E-Invoicing.");
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected int invCounter;
		GovernmentInvoice CreateInvoice(AccComplianceSequence sequence, ZString transRef, ZDateTime? allocationDate, string allocationDateOption)
		{
			var invoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GC = sequence.XD_GC_Company;
			invoice.AH_GB = sequence.XD_GB_BranchOwner;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_ComplianceSubType = sequence.XD_SequenceClass;
			invoice.AH_TransactionNum = "INV" + invCounter++.ToString();
			invoice.AH_TransactionReference = sequence.XD_Prefix + transRef;
			invoice.AH_XD_ComplianceBook = sequence.PK;

			if (allocationDate.HasValue)
			{
				if (allocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					invoice.AH_InvoiceDate = allocationDate.Value;
					invoice.AH_PostDate = ZDateTime.Now;
				}
				else if (allocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
				{
					invoice.AH_PostDate = allocationDate.Value;
					invoice.AH_InvoiceDate = ZDateTime.Now;
				}
			}
			return invoice;
		}

		protected override Type HeaderType => typeof(GovernmentInvoice);

		static IEnumerable<string> GetConstantValues(Type t)
			=> t.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
				.Select(fi => (string)fi.GetValue(null));

		static Mock<IEInvoicingTransactionProxyFactory> CreateMockEInvoicingTransactionFactory(string pivotStatus)
		{
			var eInvoicingProxyFactoryMock = new Mock<IEInvoicingTransactionProxyFactory>();

			var transactionMock = new Mock<IEInvoicingTransaction>();
			transactionMock.Setup(x => x.CurrentStatus).Returns(pivotStatus);
			eInvoicingProxyFactoryMock.Setup(c => c.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(transactionMock.Object);

			return eInvoicingProxyFactoryMock;
		}

		#endregion
	}
}
