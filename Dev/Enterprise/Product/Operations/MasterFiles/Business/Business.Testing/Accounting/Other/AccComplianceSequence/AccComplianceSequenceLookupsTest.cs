using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccComplianceSequenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSequenceClassListBaseOnCountryCode()
		{
			AssertType(typeof(CodeDescriptionPairList), AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(Core.Constants.CountryCodes.Australia));
		}

		public void TestSequenceClassListBaseOnCountryCodeInLocalLanguage()
		{
			AssertType(typeof(CodeDescriptionPairList), AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(Core.Constants.CountryCodes.Australia));
		}

		public void TestSequenceClassListBaseOnCurrentCompanyCountryCode()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertType(typeof(CodeDescriptionPairList), AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode());
		}

		public void TestComplianceInvoiceDocumentMenusList()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);
			lookup.ComplianceInvoiceDocumentMenus.Load();

			int originalCount = lookup.ComplianceInvoiceDocumentMenus.Count;

			StmMenuItem menu1 = Factory.NewWithValidTestData(typeof(StmMenuItem)) as StmMenuItem;
			StmMenuItem menu2 = Factory.NewWithValidTestData(typeof(StmMenuItem)) as StmMenuItem;
			menu1.SU_BusinessContext = nameof(BusinessContext.ARInvoice);
			menu2.SU_BusinessContext = "SomethingElse";

			Assert("Precondition: Does not contain Menu 1", !lookup.ComplianceInvoiceDocumentMenus.Contains(menu1));
			lookup.ComplianceInvoiceDocumentMenus.Load();

			AssertEquals(1, lookup.ComplianceInvoiceDocumentMenus.Count - originalCount);
			Assert("Contains Template 1", lookup.ComplianceInvoiceDocumentMenus.Contains(menu1));
			Assert("Does not contain Template 2", !lookup.ComplianceInvoiceDocumentMenus.Contains(menu2));
		}

		public void TestPrintingBranchesList()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);
			lookup.PrintingBranches.Load();

			int originalCount = lookup.PrintingBranches.Count;

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;

			Assert("Precondition: Does not contain Branch 1", !lookup.PrintingBranches.Contains(branch1));
			lookup.PrintingBranches.Load();

			AssertEquals(1, lookup.PrintingBranches.Count - originalCount);
			Assert("Contains branch 1", lookup.PrintingBranches.Contains(branch1));
			Assert("Does not contain branch 2", !lookup.PrintingBranches.Contains(branch2));
		}

		public void TestComplianceDocumentNumberAllocationSettingList()
		{
			AssertEquals(4, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.Count);
			Assert(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.ContainsCode("PRN"));
			Assert(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.ContainsCode("PST"));
			Assert(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.ContainsCode("GVT"));
			Assert(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.ContainsCode("MAN"));
		}

		public void TestXD_RollupBehaviourType_List()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);
			AssertEquals(3, lookup.XD_RollupBehaviourType_List.Count);
			Assert(lookup.XD_RollupBehaviourType_List.ContainsCode("MNL"));
			Assert(lookup.XD_RollupBehaviourType_List.ContainsCode("SRA"));
			Assert(lookup.XD_RollupBehaviourType_List.ContainsCode("SSM"));
		}

		public void TestComplianceNumberFormatDefault()
		{
			AssertEquals("DEF", AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code);
			AssertEquals("Series Prefix + Sequence Number", AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Description);
		}

		public void TestXD_ComplianceNumberFormat_List()
		{
			var configurationCollection = ComplianceNumberSequenceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			var lookup = new AccComplianceSequenceLookups(sequence);

			var list = lookup.XD_ComplianceNumberFormat_List;
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode("DEF"));
			Assert(list.ContainsCode("AAA"));
			Assert(list.ContainsCode("BBB"));
			Assert(list.ContainsCode("CCC"));
		}

		public void TestXD_AllocationLevel_List()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);
			AssertEquals(4, lookup.XD_AllocationLevel_List.Count);
			Assert(lookup.XD_AllocationLevel_List.ContainsCode("CTR"));
			Assert(lookup.XD_AllocationLevel_List.ContainsCode("COM"));
			Assert(lookup.XD_AllocationLevel_List.ContainsCode("BRN"));
			Assert(lookup.XD_AllocationLevel_List.ContainsCode("BDP"));
		}

		public void TestCnComplianceSubTypeContainsETA()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);
			Assert(AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode().ContainsCode(ChinaComplianceInfo.ComplianceSubTypeCodes.ETA));
			Assert(AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage().ContainsCode(ChinaComplianceInfo.ComplianceSubTypeCodes.ETA));
		}

		public void TestXD_SequenceClass_List()
		{
			ComplianceSubTypeList mockedComplianceSubTypeList = new ComplianceSubTypeList();
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXV", () => (NoResString)"Purchase Tax Voucher", () => "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", () => ""));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TCD", () => (NoResString)"Debit Note", () => "Nota de D\u00e9bito", () => "", LedgerOfUse.ALL));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXI", () => (NoResString)"Tax Invoice", () => "Factura", () => "", LedgerOfUse.AR));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("XCL", () => (NoResString)"Reimbursement/Disbursement/Excluded Supply", () => "Liquidaci\u00f3n de Reembolso", () => "", LedgerOfUse.AP));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(x => x.GetComplianceSubTypes()).Returns(mockedComplianceSubTypeList);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);

			var tXVDescription = new CodeDescriptionPair("TXV", "Purchase Tax Voucher");
			var tCDDescription = new CodeDescriptionPair("TCD", "Debit Note");
			var tXIDescription = new CodeDescriptionPair("TXI", "Tax Invoice");
			var xCLDescription = new CodeDescriptionPair("XCL", "Reimbursement/Disbursement/Excluded Supply");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				AccComplianceSequenceLookups lookup = new AccComplianceSequenceLookups(sequence);

				AssertContainsExactElementsInAnyOrder(new[] { xCLDescription, tXIDescription, tXVDescription, tCDDescription }, lookup.XD_SequenceClass_List);
			}
		}

		public void TestComplianceSubTypeFilterByLedger()
		{
			ComplianceSubTypeList mockedComplianceSubTypeList = new ComplianceSubTypeList();
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXV", () => (NoResString)"Purchase Tax Voucher", () => "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", () => ""));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TCD", () => (NoResString)"Debit Note", () => "Nota de D\u00e9bito", () => "", LedgerOfUse.ALL));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXI", () => (NoResString)"Tax Invoice", () => "Factura", () => "", LedgerOfUse.AR));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("XCL", () => (NoResString)"Reimbursement/Disbursement/Excluded Supply", () => "Liquidaci\u00f3n de Reembolso", () => "", LedgerOfUse.AP));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(x => x.GetComplianceSubTypes()).Returns(mockedComplianceSubTypeList);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);

			var tXVDescription = new CodeDescriptionPair("TXV", "Purchase Tax Voucher");
			var tCDDescription = new CodeDescriptionPair("TCD", "Debit Note");
			var tXIDescription = new CodeDescriptionPair("TXI", "Tax Invoice");
			var xCLDescription = new CodeDescriptionPair("XCL", "Reimbursement/Disbursement/Excluded Supply");
			var tXVLocalDescription = new CodeDescriptionPair("TXV", "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios");
			var tCDLocalDescription = new CodeDescriptionPair("TCD", "Nota de D\u00e9bito");
			var tXILocalDescription = new CodeDescriptionPair("TXI", "Factura");
			var xCLLocalDescription = new CodeDescriptionPair("XCL", "Liquidaci\u00f3n de Reembolso");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Ecuador);
				var complianceSubTypeOnCurrentCompanyCountryCodeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode();
				var complianceSubTypeInLocalLanguageOnCurrentCompanyCountryCodeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage();
				AssertContainsExactElementsInAnyOrder(new[] { tXIDescription, xCLDescription, tXVDescription, tCDDescription }, complianceSubTypeOnCurrentCompanyCountryCodeList);
				AssertContainsExactElementsInAnyOrder(new[] { tXILocalDescription, xCLLocalDescription, tXVLocalDescription, tCDLocalDescription }, complianceSubTypeInLocalLanguageOnCurrentCompanyCountryCodeList);

				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, LedgerTypes.AccountsReceivable, new[] { tXIDescription, tXVDescription, tCDDescription }, new[] { tXILocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, LedgerTypes.AccountsPayable, new[] { xCLDescription, tXVDescription, tCDDescription }, new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, LedgerTypes.IncompleteTransactions, new[] { xCLDescription, tXVDescription, tCDDescription }, new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, LedgerTypes.CashBook, new[] { tXIDescription, xCLDescription, tXVDescription, tCDDescription }, new[] { tXILocalDescription, xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, null, new[] { tXIDescription, xCLDescription, tXVDescription, tCDDescription }, new[] { tXILocalDescription, xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByLedger(Core.Constants.CountryCodes.Ecuador, ZString.Empty, new[] { tXIDescription, xCLDescription, tXVDescription, tCDDescription }, new[] { tXILocalDescription, xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });

				void AssertTestComplianceSubTypeFilterByLedger(ZString countryCode, ZString? ledger, CodeDescriptionPair[] complianceSubTypeListExpected, CodeDescriptionPair[] complianceSubTypeInLocalLanguageListExpected)
				{
					var complianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(countryCode, ledger);
					var complianceSubTypeInLocalLanguageList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(countryCode, ledger);
					AssertContainsExactElementsInAnyOrder(complianceSubTypeListExpected, complianceSubTypeList);
					AssertContainsExactElementsInAnyOrder(complianceSubTypeInLocalLanguageListExpected, complianceSubTypeInLocalLanguageList);
				}
			}
		}

		public void TestComplianceSubTypeFilterByTransactionType()
		{
			ComplianceSubTypeList mockedComplianceSubTypeList = new ComplianceSubTypeList();
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXV", () => (NoResString)"Purchase Tax Voucher", () => "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", () => ""));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TCD", () => (NoResString)"Debit Note", () => "Nota de D\u00e9bito", () => "", transactionType: TransactionTypeOfUse.ALL));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("TXI", () => (NoResString)"Tax Invoice", () => "Factura", () => "", transactionType: TransactionTypeOfUse.INV));
			mockedComplianceSubTypeList.Add(new ComplianceSubType("XCL", () => (NoResString)"Reimbursement/Disbursement/Excluded Supply", () => "Liquidaci\u00f3n de Reembolso", () => "", transactionType: TransactionTypeOfUse.CRD));

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(x => x.GetComplianceSubTypes()).Returns(mockedComplianceSubTypeList);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);

			var tXVDescription = new CodeDescriptionPair("TXV", "Purchase Tax Voucher");
			var tCDDescription = new CodeDescriptionPair("TCD", "Debit Note");
			var tXIDescription = new CodeDescriptionPair("TXI", "Tax Invoice");
			var xCLDescription = new CodeDescriptionPair("XCL", "Reimbursement/Disbursement/Excluded Supply");
			var tXVLocalDescription = new CodeDescriptionPair("TXV", "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios");
			var tCDLocalDescription = new CodeDescriptionPair("TCD", "Nota de D\u00e9bito");
			var tXILocalDescription = new CodeDescriptionPair("TXI", "Factura");
			var xCLLocalDescription = new CodeDescriptionPair("XCL", "Liquidaci\u00f3n de Reembolso");

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				AssertTestComplianceSubTypeFilterByTransactionType(Core.Constants.CountryCodes.Ecuador, TransactionTypes.Invoice, new[] { tXIDescription, tXVDescription, tCDDescription }, new[] { tXILocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByTransactionType(Core.Constants.CountryCodes.Ecuador, TransactionTypes.CreditNote, new[] { xCLDescription, tXVDescription, tCDDescription }, new[] { xCLLocalDescription, tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByTransactionType(Core.Constants.CountryCodes.Ecuador, TransactionTypes.AdjustmentNote, new[] { tXVDescription, tCDDescription }, new[] { tXVLocalDescription, tCDLocalDescription });
				AssertTestComplianceSubTypeFilterByTransactionType(Core.Constants.CountryCodes.Ecuador, null, new[] { tXVDescription, tCDDescription, tXIDescription, xCLDescription }, new[] { tXVLocalDescription, tCDLocalDescription, tXILocalDescription, xCLLocalDescription });
				AssertTestComplianceSubTypeFilterByTransactionType(Core.Constants.CountryCodes.Ecuador, ZString.Empty, new[] { tXVDescription, tCDDescription, tXIDescription, xCLDescription }, new[] { tXVLocalDescription, tCDLocalDescription, tXILocalDescription, xCLLocalDescription });

				void AssertTestComplianceSubTypeFilterByTransactionType(ZString countryCode, ZString? transactionType, CodeDescriptionPair[] complianceSubTypeListExpected, CodeDescriptionPair[] complianceSubTypeInLocalLanguageListExpected)
				{
					var complianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(countryCode, transactionType: transactionType);
					var complianceSubTypeInLocalLanguageList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(countryCode, transactionType: transactionType);
					AssertContainsExactElementsInAnyOrder(complianceSubTypeListExpected, complianceSubTypeList);
					AssertContainsExactElementsInAnyOrder(complianceSubTypeInLocalLanguageListExpected, complianceSubTypeInLocalLanguageList);
				}
			}
		}

		public void TestComplianceSubTypeFilterByLedger_Empty()
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns((IComplianceSubTypeCodeProvider)null);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			{
				var complianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(Core.Constants.CountryCodes.Australia);
				var complianceSubTypeInLocalLanguageList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(Core.Constants.CountryCodes.Australia);
				AssertEquals("complianceSubTypeList.Count", 0, complianceSubTypeList.Count);
				AssertEquals("complianceSubTypeLocalLenguageList.Count", 0, complianceSubTypeInLocalLanguageList.Count);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				complianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode();
				complianceSubTypeInLocalLanguageList = AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage();
				AssertEquals("complianceSubTypeList.Count", 0, complianceSubTypeList.Count);
				AssertEquals("complianceSubTypeLocalLenguageList.Count", 0, complianceSubTypeInLocalLanguageList.Count);
			}
		}
	}
}
