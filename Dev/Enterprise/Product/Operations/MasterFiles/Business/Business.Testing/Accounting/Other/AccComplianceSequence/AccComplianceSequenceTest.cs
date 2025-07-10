using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequence))]
	class AccComplianceSequenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccComplianceSequence>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestCanReactivate()
		{
			var complianceSequance1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			var complianceSequance2 = Factory.NewWithValidTestData<AccComplianceSequence>();

			var mockProvier = new Mock<IComplianceSequencePresentationProvider>();
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance1))).Returns(ZString.Empty);
			mockProvier.Setup(x => x.CanReactivate(It.Is<AccComplianceSequence>(o => o == complianceSequance2))).Returns("Can not reactive");

			var mockFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockFactory.Setup(x => x.GetComplianceSequencePresentationProvider()).Returns(mockProvier.Object);

			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				AssertNullOrEmpty(complianceSequance1.CanReactivate());
				AssertEquals("Can not reactive", complianceSequance2.CanReactivate());

				mockFactory.Verify(x => x.GetComplianceSequencePresentationProvider(), Times.Exactly(2));
				mockProvier.Verify(x => x.CanReactivate(complianceSequance1), Times.Once);
				mockProvier.Verify(x => x.CanReactivate(complianceSequance2), Times.Once);
			}
		}

		#region TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_INV_ATH_ReturnsTrue()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(true, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_CRD_ATH_ReturnsTrue()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(true, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_ADJ_ATH_ReturnsTrue()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(true, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_JNL_ATH_ReturnsFalse()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Journal);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AP_INV_ATH_ReturnsFalse()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_INV_IRD_ReturnsFalse()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD);
			newFactory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_INV_NoReference_ReturnsFalse()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			newFactory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_AR_INV_ATH_InDifferentCompany_ReturnsFalse()
		{
			var nonCurrentCompanyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(nonCurrentCompanyQuery);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = CreateTransactionHeaderLinkedTo(sequence, newFactory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, company: nonCurrentCompany);
			_ = CreateHeaderReferenceLinkedTo(header, newFactory, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
			newFactory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		public void TestPrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference_WithNoLinkedTransaction_ReturnsFalse()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			AssertEquals(false, sequence.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference);
		}

		#region Test Data Helpers

		AccTransactionHeader CreateTransactionHeaderLinkedTo(AccComplianceSequence sequence, BusinessObjectFactory factory, string ledger, string transactionType, GlbCompany company = null)
		{
			var transactionHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = ledger;
			transactionHeader.AH_TransactionType = transactionType;
			transactionHeader.AH_XD_ComplianceBook = sequence.PK;
			transactionHeader.AH_GC = company?.PK.ToGuid() ?? GlbCompany.CurrentCompany.PK;
			return transactionHeader;
		}

		AccTransactionHeaderReference CreateHeaderReferenceLinkedTo(AccTransactionHeader transactionHeader, BusinessObjectFactory factory, string refType)
		{
			var headerReference = factory.NewWithValidTestData<AccTransactionHeaderReference>();
			headerReference.AH1_AH = transactionHeader.PK;
			headerReference.AH1_Type = refType;
			headerReference.AH1_Reference = Guid.NewGuid().ToString().Substring(0, 10);
			return headerReference;
		}

		#endregion

		#endregion

		public void TestDefaultNumberFormat()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AssertNull(sequence.DefaultMandatoryComplianceNumberSequenceConfiguration);
			AssertEquals("DEF", sequence.XD_NumberFormat);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AssertEquals("MPC", sequence.DefaultMandatoryComplianceNumberSequenceConfiguration.Code);
			AssertEquals("Mandatory Portugal Configuration", sequence.DefaultMandatoryComplianceNumberSequenceConfiguration.Description);
			AssertEquals("MPC", sequence.XD_NumberFormat);
		}

		[TestDate(2012, 11, 11, 1, 1, 1)]
		public void TestFindSuitableSequenceBookOnExpiryDate()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = new ZDateTime(2012, 11, 11);
			Factory.Save();

			var sequenceBooks = AccComplianceSequence.FindSuitableSequenceBook("NTC", ComplianceBookAllocationLevel.Branch, ZDateTime.Today, ZDateTime.Now, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);

			AssertEquals(1, sequenceBooks.Length);
			AssertEquals(complianceSequence.PK, sequenceBooks[0].PK);
		}

		public void TestGetUnableToAllocateNumberMessage()
		{
			AssertEquals("Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has Invoice Date = {1}, that is greater than the current one(s).",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code));

			AssertEquals("Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier Invoice Date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with Invoice Date < {1}.",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code));

			AssertEquals("Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has Post Date = {1}, that is greater than the current one(s).",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code));

			AssertEquals("Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier Post Date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with Post Date < {1}.",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code));

			AssertEquals("Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has allocation date = {1}, that is greater than the current one(s).",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code));

			AssertEquals("Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier allocation date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with allocation date < {1}.",
				ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code));
		}

		public void TestFindSuitableSequenceBookWithNewFactory()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_NextNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			complianceSequence.XD_IsActive = true;
			Factory.Save();

			var sequenceBooks = AccComplianceSequence.FindSuitableSequenceBook("NTC", ComplianceBookAllocationLevel.Branch, ZDateTime.Empty, ZDateTime.Empty, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(1, sequenceBooks.Length);

			complianceSequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2019, 1, 1));
			sequenceBooks = AccComplianceSequence.FindSuitableSequenceBook("NTC", ComplianceBookAllocationLevel.Branch, ZDateTime.Empty, ZDateTime.Empty, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(0, sequenceBooks.Length);
		}

		public void TestFindSuitableSequenceBookOnInvalidExpiryDate()
		{
			AssertNoExceptionThrown(() => { AccComplianceSequence.FindSuitableSequenceBook("NTC", ComplianceBookAllocationLevel.Branch, ZDateTime.Today, ZDateTime.Invalid, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK); });
		}

		public void TestFindSuitableSequenceBookOnInvalidStartDate()
		{
			AssertNoExceptionThrown(() => { AccComplianceSequence.FindSuitableSequenceBook("NTC", ComplianceBookAllocationLevel.Branch, ZDateTime.Invalid, ZDateTime.Now, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK); });
		}

		public void TestXD_Calc_StartNumberStringAndXD_Calc_EndNumberStringMaxLength()
		{
			AssertEquals(AccComplianceSequenceSchema.XD_StartNumber.Precision, sequence.XD_Calc_StartNumberStringInfo.MaxLength);
			AssertEquals(AccComplianceSequenceSchema.XD_EndNumber.Precision, sequence.XD_Calc_EndNumberStringInfo.MaxLength);
		}

		public void TestPreventDelete()
		{
			AccComplianceSequence complianceSequence = Factory.New<AccComplianceSequence>();
			AssertEquals("PreventDelete", true, PreventDeleteAttribute.IsTrue(typeof(AccComplianceSequence)));
		}

		public void TestCanReactivateForMultipleSequences()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal);
			AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var sequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence1.XD_Code = "EXI";
			sequence1.XD_IsActive = false;
			sequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			var sequence2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence2.XD_Code = "TXI";
			sequence2.XD_IsActive = false;
			sequence2.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence2.XD_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			AccComplianceSequence[] sequences = new AccComplianceSequence[] { sequence1, sequence2 };

			var multipleDeleter = new BusinessObjectMultipleDeleter(sequences);
			AssertNullOrEmpty("Should have no message after reactivating action when allow reactivating inactive compliance book", multipleDeleter.Process(BusinessObjectMultipleDeleterAction.Activate));
			AssertEquals("Should reactive successfully for sequence1", true, sequence1.XD_IsActive);
			AssertEquals("Should reactive successfully for sequence2", true, sequence2.XD_IsActive);

			AccountingMasterFilesRegistry.Instance.AllowReActivationOfInactiveComplianceBook.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			sequence1.XD_IsActive = false;
			sequence2.XD_IsActive = false;
			Factory.Save();

			var expectedMessage = "Compliance book EXI cannot be activated due to registry setting.\r\nThis is controlled by this registry: Accounting -> Government Compliance Invoice Document -> Allow re-activation of inactive compliance books.\r\nCompliance book TXI cannot be activated due to registry setting.\r\nThis is controlled by this registry: Accounting -> Government Compliance Invoice Document -> Allow re-activation of inactive compliance books.";
			var message = multipleDeleter.Process(BusinessObjectMultipleDeleterAction.Activate);
			AssertEquals("Should have following message after reactivating action when not allow reactivating inactive compliance book", expectedMessage, message);
			AssertEquals("Should fail to reactive sequence1", false, sequence1.XD_IsActive);
			AssertEquals("Should fail to reactive sequence2", false, sequence2.XD_IsActive);
		}

		public void TestXD_NumberFormat()
		{
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			AssertEquals(AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code, sequence.XD_NumberFormat);
		}

		public void TestXD_SequenceClassDescription()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>("AccComplianceSequenceLookups.XD_SequenceClass_List");
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;

				AssertEquals("Electronic GUI", sequence.XD_Calc_SequenceClassDescription);

				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Triplicate Computerized GUI", sequence.XD_Calc_SequenceClassDescription);
				AssertNotNull(sequence.XD_Calc_SequenceClassDescriptionInfo);
			}
		}

		public void TestXD_AllocationLevel()
		{
			AssertNotNull(sequence.XD_GB_BranchOwner);
			AssertEquals(ZGuid.Empty, sequence.XD_GE_Department);
			AssertEquals(ComplianceBookAllocationLevel.Branch, sequence.XD_AllocationLevel);

			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Counter;
			AssertEquals(ZGuid.Empty, sequence.XD_GB_BranchOwner);
			AssertEquals(ZGuid.Empty, sequence.XD_GE_Department);

			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Company;
			AssertEquals(ZGuid.Empty, sequence.XD_GB_BranchOwner);
			AssertEquals(ZGuid.Empty, sequence.XD_GE_Department);

			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			AssertNotNull(sequence.XD_GB_BranchOwner);
			AssertEquals(ZGuid.Empty, sequence.XD_GE_Department);

			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.BranchDepartment;
			AssertNotNull(sequence.XD_GB_BranchOwner);
			AssertNotNull(sequence.XD_GE_Department);
		}

		public void TestIsConfiguredAsPrePrintedSequence()
		{
			sequence.XD_SO_ComplianceTemplate = ZGuid.Empty;
			sequence.XD_SQ_DocumentPrintQueue = ZGuid.Empty;

			Assert("No Printing Configuration, so this sequence is not configured to be printed to pre-printed paper", !sequence.IsConfiguredAsPrePrintedSequence);

			sequence.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
			Assert("Printing Configuration is 'No Separate Document', so this sequence is not configured to be printed to pre-printed paper", !sequence.IsConfiguredAsPrePrintedSequence);

			sequence.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.SinglePageSummarize;
			Assert("This sequence is configured to print to a pre-printed document, so it should show as having pre-printed configuration", sequence.IsConfiguredAsPrePrintedSequence);

			sequence.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.SinglePageReferAttached;
			Assert("This sequence is configured to print to a pre-printed document, so it should show as having pre-printed configuration", sequence.IsConfiguredAsPrePrintedSequence);
		}

		public void TestXD_Calc_DocumentTitle()
		{
			#region Dominican Republic
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.DominicanRepublic))
			{
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura de Crédito Fiscal", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXF;
				AssertEquals("Factura de Consumo", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VIS;
				AssertEquals("Comprobante de Compras", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VGM;
				AssertEquals("Comprobante Registro de Gastos Menores", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXS;
				AssertEquals("Comprobante de Regímenes Especiales", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXG;
				AssertEquals("Comprobante Gubernamental", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Documento de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Peru
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Peru))
			{
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("FACTURA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("NOTA DE CREDITO", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("NOTA DE DEBITO", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
				AssertEquals("DISBURSEMENT", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TBO;
				AssertEquals("BOLETA DE VENTA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.CAE;
				AssertEquals("CARTA DE PORTE AERO NACIONAL", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.CMA;
				AssertEquals("CONOCIMIENTO DE EMBARQUE MARITIMA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.HON;
				AssertEquals("RECIBO POR HONORARIOS", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.MQR;
				AssertEquals("TICKET O CINTA EMITIDO POR MAQUINA REGISTRADORA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.NXI;
				AssertEquals("COMPROBANTE - NO DOMICILIADO", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.NCR;
				AssertEquals("NOTA DE CREDITO - NO DOMICILIADO", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.NCD;
				AssertEquals("NOTA DE DEBITO - NO DOMICILIADO", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.OTR;
				AssertEquals("OTROS", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.SSP;
				AssertEquals("RECIBO POR SERVICIOS PUBLICOS", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TBC;
				AssertEquals("NOTA DE CRÉDITO DE BOLETA DE VENTA", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region VietNam
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("HÓA ĐƠN GIÁ TRỊ GIA TĂNG", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI;
				AssertEquals("HÓA ĐƠN XUẤT KHẨU", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Indonesia
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Indonesia))
			{
				sequence.XD_SequenceClass = IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Faktur Pajak", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = IndonesiaComplianceInfo.ComplianceSubTypeCodes.BKP;
				AssertEquals("Ekspor Barang Kena Pajak Tidak Berwujud", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = IndonesiaComplianceInfo.ComplianceSubTypeCodes.JKP;
				AssertEquals("Ekspor Jasa Kena Pajak", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Poland
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Poland))
			{
				sequence.XD_SequenceClass = PolandComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("FAKTURA VAT", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PolandComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("FAKTURA KORYGUJĄCA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PolandComplianceInfo.ComplianceSubTypeCodes.DCR;
				AssertEquals("NOTA KREDYTOWA", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PolandComplianceInfo.ComplianceSubTypeCodes.DSB;
				AssertEquals("NOTA OBCIĄŻENIOWA", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Mexico
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Mexico))
			{
				sequence.XD_SequenceClass = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = MexicoComplianceInfo.ComplianceSubTypeCodes.OTR;
				AssertEquals("Otros Comprobantes Nacionales (CFD)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Documento de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Uruguay
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Uruguay))
			{
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("e-Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TKT;
				AssertEquals("e-Ticket", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito de e-Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TKD;
				AssertEquals("Nota de Débito de e-Ticket", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito de e-Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.TKC;
				AssertEquals("Nota de Crédito de e-Ticket", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI;
				AssertEquals("e-Factura Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT;
				AssertEquals("e-Ticket Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD;
				AssertEquals("Nota de Débito de e-Factura Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD;
				AssertEquals("Nota de Débito de e-Ticket Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR;
				AssertEquals("Nota de Crédito de e-Factura Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR;
				AssertEquals("Nota de Crédito de e-Ticket Contingencia", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("DOCUMENTO DE REEMBOLSO", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Portugal
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.LCD;
				AssertEquals("Nota de Débito de Recuperação", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.LCR;
				AssertEquals("Nota de Crédito de Recuperação", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.LTX;
				AssertEquals("Fatura de Recuperação", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.PCD;
				AssertEquals("Nota de Débito de Fornecedor", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.PCR;
				AssertEquals("Nota de Crédito de Fornecedor", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.PTX;
				AssertEquals("Fatura de Fornecedor", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.SBC;
				AssertEquals("Nota de Crédito Autofaturação AP", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.SBD;
				AssertEquals("Nota de Débito Autofaturação AP", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.SBI;
				AssertEquals("Fatura Autofaturação AP", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TCM;
				AssertEquals("Nota de Crédito Manual", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TDM;
				AssertEquals("Nota de Débito Manual", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Fatura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				AssertEquals("Fatura Manual", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Fatura de Reembolso", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.XCR;
				AssertEquals("Nota de Crédito de Reembolso", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.XPC;
				AssertEquals("Nota de Crédito de Reembolso AP", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PortugalComplianceInfo.ComplianceSubTypeCodes.XPI;
				AssertEquals("Fatura de Reembolso AP", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region ElSalvador
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.ElSalvador))
			{
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Debito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Credito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Comprobante de Credito Fiscal", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TXN;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TXE;
				AssertEquals("Factura de Exportacion", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ElSalvadorComplianceInfo.ComplianceSubTypeCodes.TCF;
				AssertEquals("Nota de Credito sin Credito Fiscal", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Ecuador
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ecuador))
			{
				sequence.XD_SequenceClass = EcuadorComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = EcuadorComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = EcuadorComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = EcuadorComplianceInfo.ComplianceSubTypeCodes.TXV;
				AssertEquals("Liquidación de Compras de Bienes y Prestación de Servicios", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = EcuadorComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Liquidación de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Costa Rica
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.CostaRica))
			{
				sequence.XD_SequenceClass = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = CostaRicaComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Documento de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Guatemala
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Guatemala))
			{
				sequence.XD_SequenceClass = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = GuatemalaComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Nota de Remisión", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = GuatemalaComplianceInfo.ComplianceSubTypeCodes.XCR;
				AssertEquals("Nota de Crédito de Nota de Remisión", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Honduras
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Honduras))
			{
				sequence.XD_SequenceClass = HondurasComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = HondurasComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = HondurasComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = HondurasComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Documento de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Chile
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Chile))
			{
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.DXI;
				AssertEquals("Factura Electrónica", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.DEX;
				AssertEquals("Factura No Afecta o Exenta Electrónica", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.DCR;
				AssertEquals("Nota de Crédito Electrónica", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.DCD;
				AssertEquals("Nota de Débito Electrónica", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.TEX;
				AssertEquals("Factura de Ventas Y Servicios no Afectos O Exentos de I.V.A", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChileComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Colombia
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Colombia))
			{
				sequence.XD_SequenceClass = ColombiaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura De Venta", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ColombiaComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota De Credito", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region KoreaSouth
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("일반 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.TZI;
				AssertEquals("영세율 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CTI;
				AssertEquals("수정 일반 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CZI;
				AssertEquals("수정 영세율 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.NTI;
				AssertEquals("일반 계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CNI;
				AssertEquals("수정 일반 계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.ITI;
				AssertEquals("수정 일반 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.IZI;
				AssertEquals("수정 영세율 세금계산서", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = KoreaSouthComplianceInfo.ComplianceSubTypeCodes.INI;
				AssertEquals("수정 일반 계산서", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Taiwan
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("三聯式電子計算機統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				AssertEquals("電子發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
				AssertEquals("二聯式手開統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP;
				AssertEquals("三聯式手開統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
				AssertEquals("三聯式收銀機統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS;
				AssertEquals("海關進口關稅", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("三聯式折讓證明單", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
				AssertEquals("收據", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
				AssertEquals("收據折讓", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("二聯式折讓證明單", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC;
				AssertEquals("二聯式收銀機統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI;
				AssertEquals("二聯式電子計算機統一發票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD;
				AssertEquals("彙總登錄每張稅額伍佰元以下之進項(二聯式)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX;
				AssertEquals("彙總登錄每張稅額伍佰元以下之進項(三聯式)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				AssertEquals("電子發票折讓", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("代收代付", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG;
				AssertEquals("免開零稅率發票", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region China
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				sequence.XD_SequenceClass = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
				AssertEquals("电子增值税普通发票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				AssertEquals("电子增值税专用发票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertEquals("增值税专用发票", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
				AssertEquals("增值税普通发票", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Italy
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				AssertEquals("Vendite - Fattura / Nota di Credito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARS;
				AssertEquals("Fattura / Credito Per Conto Del Fornitore", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				AssertEquals("Acquisti - Fattura / Nota di Credito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				AssertEquals("Acquisti - Autofattura (Extra UE / Conto Del Fornitore)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARE;
				AssertEquals("Vendite - Fattura / Nota di Credito (UE)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARN;
				AssertEquals("Vendite - Fattura / Nota di Credito (Extra UE)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.INI;
				AssertEquals("Acquisti - Integrazione (Italia)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.INT;
				AssertEquals("Acquisti - Integrazione (UE)", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.APV;
				AssertEquals("Acquisti - Fattura Semplificata", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.ARV;
				AssertEquals("Vendite - Fattura Semplificata", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = ItalyComplianceInfo.ComplianceSubTypeCodes.XAP;
				AssertEquals("Acquisti - Escluse - Rimborso Documentato", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Panama
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Panama))
			{
				sequence.XD_SequenceClass = PanamaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PanamaComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PanamaComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = PanamaComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Documento de Reembolso", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Sri Lanka
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SriLanka))
			{
				sequence.XD_SequenceClass = SriLankaComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Tax Invoice", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SriLankaComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Tax Credit Note", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SriLankaComplianceInfo.ComplianceSubTypeCodes.STX;
				AssertEquals("Suspended Tax Invoice", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SriLankaComplianceInfo.ComplianceSubTypeCodes.STC;
				AssertEquals("Suspended Tax Credit Note", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Spain
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			{
				sequence.XD_SequenceClass = SpainComplianceInfo.ComplianceSubTypeCodes.TXI;
				AssertEquals("Factura", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SpainComplianceInfo.ComplianceSubTypeCodes.TCR;
				AssertEquals("Nota de Crédito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SpainComplianceInfo.ComplianceSubTypeCodes.TCD;
				AssertEquals("Nota de Débito", sequence.XD_Calc_DocumentTitle);
				sequence.XD_SequenceClass = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
				AssertEquals("Factura de Suplido", sequence.XD_Calc_DocumentTitle);
			}
			#endregion

			#region Australia
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				sequence.XD_SequenceClass = ZString.Empty;
				AssertEquals("", sequence.XD_Calc_DocumentTitle);
			}
			#endregion
		}

		public void TestFindParentSubTypeInRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				ComplianceSubTypeDependencyConfigurationCollection collection = new ComplianceSubTypeDependencyConfigurationCollection();
				ComplianceSubTypeDependencyConfiguration item = collection.AddNew();
				item.Country = "TW";
				item.ChildSubType = "TXI";
				item.ParentSubType = "TXS";
				item = collection.AddNew();
				item.Country = "TW";
				item.ChildSubType = "TXS";
				item.ParentSubType = "TXP";
				item = collection.AddNew();
				item.Country = "TW";
				item.ChildSubType = "TXP";
				item.ParentSubType = "TXE";
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
				AssertEquals("TXE", AccComplianceSequence.FindParentSubTypeInRegistry("TXI"));
			}
		}

		public void TestLogVoidingInformation()
		{
			Factory.Save();

			StmALog[] logs = sequence.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.VoidingComplianceSequenceNo.Code));
			AssertEquals(0, logs.Length);

			sequence.XD_NextNumber = 20;
			Factory.Save();
			logs = sequence.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.VoidingComplianceSequenceNo.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("sequence number is voided from 1 to 19", logs[0].SL_Reference);
			string expected = logs[0].SL_EventTime.ToString() + " - sequence number is voided from 1 to 19<br />";
			AssertEquals(expected, sequence.VoidedNumbersLogsAsHTML);

			sequence.XD_NextNumber = 30;
			Factory.Save();
			logs = sequence.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.VoidingComplianceSequenceNo.Code));
			AssertEquals(2, logs.Length);
			ZDateTime secondTime = logs.Select(x => x.SL_EventTime).Max();
			expected += secondTime.ToString() + " - sequence number is voided from 20 to 29<br />";
			AssertEquals(expected, sequence.VoidedNumbersLogsAsHTML);
		}

		public void TestNoStmALogs()
		{
			Factory.Save();
			var newFactoryForLoading = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, sequence.PK);
				AssertEquals("Not expecting Add event.", 0, newFactoryForLoading.Load<StmALog>(query).Length);
				sequence.XD_Description = "Test Log";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, newFactoryForLoading.Load<StmALog>(query).Length);
				sequence.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, newFactoryForLoading.Load<StmALog>(query).Length);
			});
		}

		public void TestDatesReadOnlyForPortugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Factory.Save();

				Assert(!sequence.XD_StartDate_ReadOnly_ForTestOnly);
				Assert(!sequence.XD_ExpiryDate_ReadOnly_ForTestOnly);

				sequence.XD_NextNumber += 1;

				Assert(sequence.XD_StartDate_ReadOnly_ForTestOnly);
				Assert(sequence.XD_ExpiryDate_ReadOnly_ForTestOnly);
			}
		}

		#region Test Paddings

		public void TestXD_Calc_StartNumberStringPadding()
		{
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_StartNumber = 1;
			AssertEquals("000000001", sequence.XD_Calc_StartNumberString);
		}

		public void TestXD_Calc_EndNumberStringPadding()
		{
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_EndNumber = 100;
			AssertEquals("000000100", sequence.XD_Calc_EndNumberString);
		}

		public void TestXD_Calc_NextNumberStringPadding()
		{
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_NextNumber = 10;
			AssertEquals("000000010", sequence.XD_Calc_NextNumberString);
		}

		public void TestXD_MaximumNumberDigits()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var newSequence = Factory.New<AccComplianceSequence>();
				AssertEquals((ZByte)8, newSequence.XD_MaximumNumberDigits);
			}
		}

		public void TestXD_SequenceClass()
		{
			Assert(!sequence.IsInDatabase);
			Assert("XD_SequenceClass should not be readonly if not in database", !sequence.XD_SequenceClassInfo.ReadOnly);
			Factory.Save();
			Assert(sequence.IsInDatabase);
			sequence.XD_NextNumber++;
			Assert("XD_SequenceClass should not be readonly if not in Taiwan company", !sequence.XD_SequenceClassInfo.ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				Assert("XD_SequenceClass should always be readonly", sequence.XD_SequenceClassInfo.ReadOnly);
			}
		}

		#endregion

		#region Test ReadOnly

		public void TestXD_Calc_NextNumberString_ReadOnly()
		{
			Assert(!sequence.IsInDatabase);
			Assert("XD_Calc_NextNumberString should always be readonly", sequence.XD_Calc_NextNumberStringInfo.ReadOnly);
			Factory.Save();
			Assert(sequence.IsInDatabase);
			Assert("XD_Calc_NextNumberString should always be readonly", sequence.XD_Calc_NextNumberStringInfo.ReadOnly);
		}

		public void TestXD_Calc_StartNumberString_ReadOnly()
		{
			Assert(!sequence.IsInDatabase);
			Assert("XD_Calc_StartNumberString should not be read only if not in database", !sequence.XD_Calc_StartNumberStringInfo.ReadOnly);
			Factory.Save();
			Assert(sequence.IsInDatabase);
			Assert("XD_Calc_StartNumberString should be read only if it's in database", sequence.XD_Calc_StartNumberStringInfo.ReadOnly);
		}

		public void TestXD_Calc_EndNumberString_ReadOnly()
		{
			Assert(!sequence.IsInDatabase);
			Assert("XD_Calc_EndNumberString should not be read only if not in database", !sequence.XD_Calc_EndNumberStringInfo.ReadOnly);
			Factory.Save();
			Assert(sequence.IsInDatabase);
			Assert("XD_Calc_EndtNumberString should be read only if it's in database", sequence.XD_Calc_EndNumberStringInfo.ReadOnly);
		}

		public void TestIsComplianceNumberAllocationDateMandatory()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				Assert("if AR registry is different from NOT then compliance number allocation date is mandatory", sequence.IsComplianceNumberAllocationDateMandatory);
			}

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				Assert("if AR registry is different from NOT then compliance number allocation date is mandatory", sequence.IsComplianceNumberAllocationDateMandatory);
			}

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				Assert("if AR registry is NOT then compliance number allocation date is not mandatory", !sequence.IsComplianceNumberAllocationDateMandatory);
			}
		}

		public void TestXD_RollupBehaviourWhenMaxExceeded_ReadOnly()
		{
			sequence.XD_SU_MenuItem = ZGuid.Empty;
			Assert(sequence.XD_RollupBehaviourWhenMaxExceededInfo.ReadOnly);
			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			sequence.XD_SU_MenuItem = menu.PK;
			Assert(!sequence.XD_RollupBehaviourWhenMaxExceededInfo.ReadOnly);
		}

		#endregion

		#region Test string manipulation

		public void TestStartNumberIncludingPrefixNumberPart()
		{
			sequence.XD_Prefix = "ABC10";
			sequence.XD_StartNumber = 1;
			sequence.XD_MaximumNumberDigits = 5;
			AssertEquals("1000001", sequence.StartNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC1";
			AssertEquals("100001", sequence.StartNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC";
			AssertEquals("00001", sequence.StartNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC12DE34";
			AssertEquals("3400001", sequence.StartNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC12DE";
			AssertEquals("00001", sequence.StartNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "123";
			AssertEquals("12300001", sequence.StartNumberIncludingPrefixNumberPart);
		}

		public void TestEndNumberIncludingPrefixNumberPart()
		{
			sequence.XD_Prefix = "ABC10";
			sequence.XD_EndNumber = 100;
			sequence.XD_MaximumNumberDigits = 5;
			AssertEquals("1000100", sequence.EndNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC1";
			AssertEquals("100100", sequence.EndNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC";
			AssertEquals("00100", sequence.EndNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC12DE34";
			AssertEquals("3400100", sequence.EndNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "ABC12DE";
			AssertEquals("00100", sequence.EndNumberIncludingPrefixNumberPart);

			sequence.XD_Prefix = "123";
			AssertEquals("12300100", sequence.EndNumberIncludingPrefixNumberPart);
		}

		public void TestPrefixExcludesNumberPart()
		{
			sequence.XD_Prefix = "ABC10";
			AssertEquals("ABC", sequence.PrefixExcludesNumberPart);

			sequence.XD_Prefix = "ABC1";
			AssertEquals("ABC", sequence.PrefixExcludesNumberPart);

			sequence.XD_Prefix = "ABC";
			AssertEquals("ABC", sequence.PrefixExcludesNumberPart);

			sequence.XD_Prefix = "ABC10DE20";
			AssertEquals("ABC10DE", sequence.PrefixExcludesNumberPart);

			sequence.XD_Prefix = "ABC10DE";
			AssertEquals("ABC10DE", sequence.PrefixExcludesNumberPart);

			sequence.XD_Prefix = "1234";
			AssertEquals("", sequence.PrefixExcludesNumberPart);
		}

		#endregion

		#region Test Get Next Number

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberInfo()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_Prefix = "Prefix";
			sequence.XD_ExpiryDate = new ZDateTime(2013, 12, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 9;
			Factory.Save();

			var complianceNumberSequenceMock = new Mock<IComplianceNumberSequence>();
			var result = sequence.GetNextNumberInfo(complianceNumberSequenceMock.Object, new ZDateTime(2012, 06, 15));
			AssertEquals("000000009", result.NextNumber);
			AssertEquals("Prefix000000009", result.NextNumberWithPrefix);

			var reloadedSequence = new BusinessObjectFactory().Load<AccComplianceSequence>(sequence.PK);
			AssertEquals("NextNo should be set to 10", 10m, reloadedSequence.XD_NextNumber);
		}

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_NotLastNo()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2013, 12, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 9;
			Factory.Save();

			var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
			AssertEquals("Should be correct result", 9, result);

			var reloadedSequence = new BusinessObjectFactory().Load<AccComplianceSequence>(sequence.PK);
			AssertEquals("Compliance sequence should not expired", 2013, reloadedSequence.XD_ExpiryDate.Year);
			AssertEquals("NextNo should be set to 10", 10m, reloadedSequence.XD_NextNumber);
			AssertEquals("IsActive should still be true", true, reloadedSequence.XD_IsActive);
		}

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_LastNo()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2013, 12, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 10;
			Factory.Save();

			var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
			AssertEquals("Should be correct result", 10, result);

			var reloadedSequence = new BusinessObjectFactory().Load<AccComplianceSequence>(sequence.PK);
			AssertEquals("Compliance sequence should be expired", 2012, reloadedSequence.XD_ExpiryDate.Year);
			AssertEquals("NextNo should be set to 11", 11m, reloadedSequence.XD_NextNumber);
			AssertEquals("IsActive should still be false", false, reloadedSequence.XD_IsActive);
		}

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_Expired()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2012, 05, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 10;
			Factory.Save();

			try
			{
				var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
				Assert("Test Failed. The exception should have been raised.", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(AllocationComplianceSequenceFullException), ex.GetType());
				AssertEquals("The exception message should be correct", ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, ((AllocationComplianceSequenceFullException)ex).UserFriendlyMessage);
			}
		}

		[TestDate(2012, 06, 15, 12, 50, 40)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLockOnExpiredDate()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2012, 06, 15);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 10;
			Factory.Save();

			var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15, 10, 12, 13));
			AssertEquals(10, result);
		}

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_InActive()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2013, 04, 30);
			sequence.XD_IsActive = false;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 10;
			Factory.Save();

			try
			{
				var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
				Assert("Test Failed. The exception should have been raised.", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(AllocationComplianceSequenceFullException), ex.GetType());
				AssertEquals("The exception message should be correct", ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, ((AllocationComplianceSequenceFullException)ex).UserFriendlyMessage);
			}
		}

		[TestDate(2012, 06, 15)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_NextNumberExceedsEndNumber()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_ExpiryDate = new ZDateTime(2013, 04, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 11;
			Factory.Save();

			try
			{
				var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
				Assert("Test Failed. The exception should have been raised.", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(AllocationComplianceSequenceFullException), ex.GetType());
				AssertEquals("The exception message should be correct", ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, ((AllocationComplianceSequenceFullException)ex).UserFriendlyMessage);
			}
		}

		[TestDate(2018, 08, 07)]
		public void TestGetNextNumberFromComplianceSubTypeSequenceWithLock_StartDate()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_StartDate = new ZDate(2018, 08, 30);
			sequence.XD_IsActive = true;
			sequence.XD_EndNumber = 10;
			sequence.XD_NextNumber = 10;
			Factory.Save();

			try
			{
				var result = sequence.GetNextNumberFromComplianceSubTypeSequenceWithLock(new ZDateTime(2012, 06, 15));
				Assert("Test Failed. The exception should have been raised.", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should be of correct exception type", typeof(AllocationComplianceSequenceFullException), ex.GetType());
				AssertEquals("The exception message should be correct", ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, ((AllocationComplianceSequenceFullException)ex).UserFriendlyMessage);
			}
		}

		#endregion

		#region Test Start Number

		public void TestTemplateCopy_StartNumberForDigitalSignature()
		{
			sequence.XD_NextNumber = 100;
			var copiedSequence = (AccComplianceSequence)((ITemplateCopyable)sequence).TemplateCopy();
			AssertEquals("Start number is 1", new ZDecimal(1), copiedSequence.XD_StartNumber);
			AssertEquals("000000001", copiedSequence.XD_Calc_StartNumberString);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				copiedSequence = (AccComplianceSequence)((ITemplateCopyable)sequence).TemplateCopy();
				AssertEquals("Start number is 100", new ZDecimal(100), copiedSequence.XD_StartNumber);
				AssertEquals("000000100", copiedSequence.XD_Calc_StartNumberString);
			}
		}

		#endregion

		#region Implementation

		protected AccComplianceSequence sequence;
		protected GlbBranch branch;

		protected override void SetUp()
		{
			base.SetUp();

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "AAA";

			Factory.Save();

			sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_Code = "LM1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = branch.PK;
		}

		#endregion
	}
}
