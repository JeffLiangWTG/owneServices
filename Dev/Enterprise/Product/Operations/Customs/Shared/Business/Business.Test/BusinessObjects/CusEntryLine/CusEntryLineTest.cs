using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryLine))]
	public abstract class CusEntryLineTest<TCusEntryLine, TJobComInvoiceLine> : EnterpriseBusinessObjectTestCase
		where TCusEntryLine : CusEntryLine
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public void TestTotalDutyAndTaxesAmount()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeAmount = 100m;
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeAmount = 200m;
			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeAmount = 400m;
			fee3.CF_IsLandedCostOnly = true;
			AssertEquals("TotalDutyAndTaxesAmount should be the sum amount of fees whose CF_IsLandedCostOnly is false", 300m, entryLine.TotalDutyAndTaxesAmount);
		}

		public void TestTypeOfFees()
		{
			AssertEquals("Fees' type should be expected.", ExpectedTypeOfFees, Factory.New<CusEntryLine>().Fees.GetType());
		}

		public virtual void TestDescriptionFromLines()
		{
			var declaration = ImportJobDeclaration;
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "LINE";

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				AssertEquals("Description before Merge", "LINE", entryLine.Description);

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_Description = "LINE 2";

				DoMerge(declaration);
				AssertEquals("Description after Merge", ExpectedFallbackEntrylineDescription, entryLine.Description);
			});
		}

		public void TestHeaderIsLoadedCorrectly()
		{
			var declaration = ImportJobDeclaration;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			AssertType(entry.GetType(), entryLine.Header);
		}

		public void TestDescriptionFromClassWhenMergedByClassification()
		{
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(ImportJobDeclaration.CountryCode) || ImportJobDeclaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			var tarrifCode = "0000.00.00.00Y";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = ImportJobDeclaration;
			declaration.FillWithValidTestData();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.JI_CC = classification.PK;
			line1.JI_Description = "LINE";

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				AssertEquals("Description before Merge", ExpectedClassificationDescription, entryLine.Description);

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_Tariff = tarrifCode;
				line2.JI_CC = classification.PK;
				line2.JI_Description = "LINE 2";

				DoMerge(declaration);
				AssertEquals("Description after Merge", ExpectedClassificationDescriptionMultipleLines, entryLine.Description);
			});
		}

		public void TestCL_CustomsPostedStatus()
		{
			CusEntryLine entryLine = (CusEntryLine)GetNewBusinessObject();
			AssertEquals("CustomsPostedStatus should be active when initialised", EntryLineStatusList.Codes.Active, entryLine.CL_CustomsPostedStatus);
			AssertEquals("IsActive", true, entryLine.IsActive);

			entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Deleted;
			AssertEquals("Is not active", false, entryLine.IsActive);
		}

		public void TestReadOnlyProperties()
		{
			CusEntryLine entryLine = (CusEntryLine)GetNewBusinessObject();
			AssertEquals("CL_Tariff is readonly", true, entryLine.CL_AdValoremTariffInfo.ReadOnly);
			AssertEquals("EffectiveDescriptionInfo is readonly", true, entryLine.EffectiveDescriptionInfo.ReadOnly);
		}

		public void TestLineSubmissionStatusDescription()
		{
			CusEntryLine entryLine = (CusEntryLine)GetNewBusinessObject();
			entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.DeletePending;
			AssertEquals("EntryLine Status Desc", EntryLineStatusList.Descriptions.DeletePending, entryLine.LineSubmissionStatusDescription);

			entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Deleted;
			AssertEquals("EntryLine Status Desc", EntryLineStatusList.Descriptions.Deleted, entryLine.LineSubmissionStatusDescription);

			entryLine.CL_CustomsPostedStatus = EntryLineStatusList.Codes.Active;
			AssertEquals("EntryLine Status Desc", EntryLineStatusList.Descriptions.Active, entryLine.LineSubmissionStatusDescription);
		}

		public void TestTariffAndDescriptionAreSetAfterMerge()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "InvoiceLineDescription";
			invoiceLine.JI_ExtraInfoForClassification = "Extended Description";
			invoiceLine.JI_Tariff = "0000.00.00";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Tariff is set", invoiceLine.JI_Tariff, entryLine.CL_AdValoremTariff);
			AssertEquals("Description is set", invoiceLine.JI_Description, entryLine.CL_Description);
			AssertEquals("ExtraInfoForClassification is set", "Extended Description", entryLine.CL_ExtraInfoForClassification);
		}

		public void TestExtendedCommercialDescription()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			string extendedCommercialDescription = "Extended Description";
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "000.00.00";
			invoiceLine.JI_ExtraInfoForClassification = extendedCommercialDescription;
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "000.00.00";
			invoiceLine2.JI_ExtraInfoForClassification = extendedCommercialDescription;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("ExtendedCommercialDescription", extendedCommercialDescription, entryLine.ExtendedCommercialDescription);

			invoiceLine2.JI_ExtraInfoForClassification = "Extended 2 Description";
			merger.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "", entryLine.ExtendedCommercialDescription);

			invoiceLine.JI_ExtraInfoForClassification = "Extended 2 Description";
			merger.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "Extended 2 Description", entryLine.ExtendedCommercialDescription);

			invoiceLine.JI_ExtraInfoForClassification = "";
			merger.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "", entryLine.ExtendedCommercialDescription);

			invoiceLine2.JI_ExtraInfoForClassification = "";
			merger.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "", entryLine.ExtendedCommercialDescription);
		}

		public void TestFormattedTariff()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "InvoiceLineDescription";
			invoiceLine.JI_Tariff = "0000.00.00";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("FormattedTariff", invoiceLine.JI_FormattedTariff, entryLine.FormattedTariff);
		}

		public virtual void TestDutyRateDescription()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_DutyPercent = 5m;
			AssertEquals("Duty rate description", "5.00%", entryLine.DutyRateDescription);

			entryLine.CL_FlatAmount = 10.45m;
			entryLine.CL_FlatAmountUQ = "LA";
			AssertEquals("Flat rate description", "5.00%+10.45/LA", entryLine.DutyRateDescription);

			entryLine.CL_DutyPercent = 0m;
			AssertEquals("Flat rate description", "10.45/LA", entryLine.DutyRateDescription);
		}

		[TestDate(2005, 6, 2)]
		public virtual void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			var line2ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;

			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				line1.JI_CEI = instruction.PK;
				line2.JI_CEI = instruction.PK;
			}

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("FOB", 600.0m, entryLine.FOBInLocalCurrency.Amount);
				AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
				AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
				AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
				AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
			});
		}

		public void TestBondedWarehouseTransactionLineProvider()
		{
			var declaration = ImportJobDeclaration;
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			DoMerge(declaration);

			var txLineProvider = (IBondedWarehouseTransactionLineProvider)declaration.CustomsEntryHeaders[0].MergedLines[0];
			var line1 = txLineProvider.TransactionLine;
			var line2 = txLineProvider.TransactionLine;

			CombineAssertions(() =>
			{
				AssertSame("Same value returned each call", line1, line2);
				AssertNotNull("BondedWarehouseTransaction provided", line1);
			});
		}

		public virtual void TestICusEntryLineInterface()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "A";
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1.1m;
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_InvoiceUQ = "ZZ";
			invoiceLine1.JI_CustomsUnitQty = "PK";
			invoiceLine1.JI_CustomsQuantity = 3.2m;

			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1.0m;
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.JI_Description = "DESCRIPTION";
			invoiceLine2.JI_InvoiceUQ = "ZZ";
			invoiceLine2.JI_CustomsUnitQty = "PK";
			invoiceLine2.JI_CustomsQuantity = 2.1m;

			Assert("Precondition: !InvoiceLine1.JI_Tariff.IsEmpty", !invoiceLine1.JI_Tariff.IsEmpty);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = invoiceLine1.JI_Tariff;

			ICusEntryLine iEntryLine = entryLine;
			AssertEquals("IEntryLine.CL_LineNumber", (short)1, iEntryLine.CL_LineNumber);
			AssertEquals("IEntryLine.Description", "DESCRIPTION", iEntryLine.Description);
			AssertEquals("IEntryLine.Tariff", invoiceLine1.JI_Tariff, iEntryLine.Tariff);
			AssertEquals("IEntryLine.CustomsQuantity", 5.3m, iEntryLine.CustomsQuantity);
			AssertEquals("IEntryLine.CustomsUnitQty", "PK", iEntryLine.CustomsUnitQty);
			AssertEquals("IEntryLine.BondedWarehouseQuantity", 2.1m, iEntryLine.BondedWarehouseQuantity);
			AssertEquals("IEntryLine.CustomsUnitQty", "ZZ", iEntryLine.BondedWarehouseUnitQuantity);
		}

		public void TestConstructor()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertNotNull(entryLine);
		}

		public void TestStaticNewMethod()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertNotNull(entryLine);
		}

		public void TestFeesCollection()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertNotNull(entryLine.Fees);
		}

		public virtual void TestGSTRate()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Default Value for GST Rate should be 0", 0m, entryLine.GSTRate);
		}

		public virtual void TestCustomsQuantities()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertNotNull(entryLine.CustomsQuantities);
		}

		public void TestDelete()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			BaseJobComInvoiceLine invLine = testDec.InvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			entryLine.Delete();
			AssertEquals("InvLine.JI_CL", ZGuid.Empty, invLine.JI_CL);
		}

		public void TestSupportNotes()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			Assert("Support Notes = false", !entryLine.SupportsNotes);
		}

		public void TestCustomsValueIsInLocalCurrency()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals(entryLine.CustomsValue.Currency.Code, testDec.LocalCurrencyCode);
		}

		public void TestEffectiveNetWeight()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			var mock1 = Factory.NewMoq<TJobComInvoiceLine>();
			mock1.Object.JI_CL = entryLine.PK;

			mock1.Setup(m => m.JI_NetWeight).Returns(100);
			mock1.Setup(m => m.JI_NetWeightUQ).Returns("KG");
			Assert(new ZWeight(100m, "KG").Equals(entryLine.EffectiveNetWeight));
		}

		public void TestEffectiveGrossWeight()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var mock1 = Factory.NewMoq<TJobComInvoiceLine>();
			mock1.Object.JI_CL = entryLine.PK;
			mock1.Setup(m => m.EffectiveGrossWeight).Returns(new ZWeight(100, "KG"));
			var mock2 = Factory.NewMoq<TJobComInvoiceLine>();
			mock2.Object.JI_CL = entryLine.PK;
			mock2.Setup(m => m.EffectiveGrossWeight).Returns(new ZWeight(200, "KG"));
			AssertEquals(new ZWeight(300m, "KG"), entryLine.EffectiveGrossWeight);
		}

		public void TestEffectiveVolume()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var mock1 = Factory.NewMoq<TJobComInvoiceLine>();
			mock1.Object.JI_CL = entryLine.PK;
			mock1.Setup(m => m.EffectiveVolume).Returns(new ZVolume(100, "M3"));

			var mock2 = Factory.NewMoq<TJobComInvoiceLine>();
			mock2.Object.JI_CL = entryLine.PK;
			mock2.Setup(m => m.EffectiveVolume).Returns(new ZVolume(200, "M3"));

			AssertEquals(new ZVolume(300m, "M3"), entryLine.EffectiveVolume);
		}

		public virtual void TestEffectiveCustomsWeight()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_CustomsQuantity = 100m;
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 200m;
			AssertEquals(new ZWeight(300m, "KG"), entryLine.EffectiveCustomsWeight);

			invoiceLine2.JI_CustomsUnitQty = "G";
			invoiceLine2.JI_CustomsQuantity = 2000m;
			AssertEquals(new ZWeight(102m, "KG"), entryLine.EffectiveCustomsWeight);
		}

		public void TestGetInvoiceLinesReference()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceNumber = "A";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JZ_InvoiceNumber = "B";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[1].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[2].JI_CL = entryLine.PK;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[0].JI_Description = "C";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[1].JI_Description = "B";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[2].JI_Description = "A";
			AssertEquals("invoice A(line 1), invoice B(line 1, 2, 3)", entryLine.GetInvoiceLinesReference());
		}

		public void TestPrice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 1000m;

			AssertEquals("Price for the base", 1000m, entryLine.Price.Amount);
			AssertEquals("Currency", testDec.LocalCurrencyCode, entryLine.Price.Currency.Code);
		}

		public void TestPriceShouldBeCached()
		{
			var times = 0;
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<Money>("PriceCore").Returns(Money.Empty).Callback(() => ++times);
			var entryLine = entryLineMock.Object;
			AssertEquals(Money.Empty, entryLine.Price);
			AssertEquals("The calculation should be cached so the times is 1.", 1, times);
		}

		public void TestTotalLinePrice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MergeBy = "TRF";
			BaseJobComInvoiceHeader header = testDec.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			header.JobComInvoiceLines.AddNew().JI_LinePrice = 100m;
			header.JobComInvoiceLines.AddNew().JI_LinePrice = 200m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("Amount", 300m, testDec.CustomsEntryHeaders[0].MergedLines[0].TotalLinePrice.Amount);
			AssertEquals("Currency", header.JZ_RX_NKInvoice_Currency, testDec.CustomsEntryHeaders[0].MergedLines[0].TotalLinePrice.Currency.Code);
		}

		public void TestTotalLinePriceInLocalCurrency()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MergeBy = "TRF";
			BaseJobComInvoiceHeader header = testDec.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			header.JobComInvoiceLines.AddNew().JI_LinePrice = 100m;
			header.JobComInvoiceLines.AddNew().JI_LinePrice = 200m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("Amount in Local Currency", 300m, testDec.CustomsEntryHeaders[0].MergedLines[0].TotalLinePriceInLocalCurrency);
		}

		public void TestResetTotalsAndCachedValues()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CustomsValue = 10;
			entryLine.CL_DutyPercent = 20;
			entryLine.CL_WarehouseUnitValue = 30;

			var feeMock = Factory.NewMoq<CusEntryLineFee>();
			feeMock.Protected().Setup<bool>("ShouldResetDataOnMergingCore").Returns(false);
			var fee = feeMock.Object;
			entryLine.Fees.Add(fee);
			fee.CF_ChargeAmount = 500m;
			fee.CF_ChargeType = "TTT";
			fee.CF_IsLandedCostOnly = true;
			AssertEquals("Should not clear amount", false, fee.ShouldResetDataOnMerging);

			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("Customs value is cleared", 0m, entryLine.CL_CustomsValue);
			AssertEquals("CL_DutyPercent is cleared", 0m, entryLine.CL_DutyPercent);
			AssertEquals("CL_WarehouseUnitValue is cleared", 0m, entryLine.CL_WarehouseUnitValue);
			AssertEquals("Charge amount is not cleared. AU has a charge amount which brokers manually enters after contacting Customs and Re-merging should not clear those amounts as system wont calculate this back", 500m, fee.CF_ChargeAmount);
			AssertEquals("fee.CF_IsLandedCostOnly should be reset to false as it is going to be properly set at the end of merge", false, fee.CF_IsLandedCostOnly);

			feeMock.Reset();
			feeMock.Protected().Setup<bool>("ShouldResetDataOnMergingCore").Returns(true);
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("Charge amount is cleared", 0m, fee.CF_ChargeAmount);
			AssertEquals("fee.CF_IsLandedCostOnly", false, fee.CF_IsLandedCostOnly);
		}

		public void TestInvoiceQuantity()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_InvoiceQuantity = 1M;

			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.RefreshInvoiceLines();

			invoiceLine2.JI_InvoiceQuantity = 2M;
			AssertEquals("Total Invoice Quantity", 3m, entryLine.InvoiceQuantity);
		}

		[ExpectNoExceptions]
		public void TestCachedPropertiesAreClearedByResetTotalsAndCachedValues()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader header = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryLine entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			Type cusEntryLineType = entryLine.GetType();

			foreach (PropertyInfo info in cusEntryLineType.GetProperties())
			{
				if (info.CanRead && info.GetIndexParameters().Length == 0)
				{
					if (info.Name != "LightValidationIsValid")
					{
						info.GetValue(entryLine, null);
					}
				}
			}

			foreach (FieldInfo info in cusEntryLineType.GetFields())
			{
				if (info.Name != "TypeDecider" && info.Name != "CustomNoteTypesDelegate")
				{
					AssertNotNull(info.Name, info.GetValue(entryLine));
				}
			}

			entryLine.ResetTotalsAndCachedValues();

			foreach (FieldInfo info in cusEntryLineType.GetFields())
			{
				if (info.Name != "TypeDecider" && info.Name != "CustomNoteTypesDelegate")
				{
					AssertNull(info.Name, info.GetValue(entryLine));
				}
			}
		}

		public void TestGetFirstLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceNumber = "A";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JZ_InvoiceNumber = "B";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[0].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[1].JI_CL = entryLine.PK;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[2].JI_CL = entryLine.PK;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[0].JI_Description = "C";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[1].JI_Description = "B";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[2].JI_Description = "A";
			AssertEquals("invoice A(line 1), invoice B(line 1, 2, 3)", entryLine.GetInvoiceLinesReference());
			AssertEquals("Get FirstLine", invLine1.PK, entryLine.FirstLine.PK);

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			entryHeader.MergedLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CL = entryLine.PK;
			AssertEquals("Get FirstLine", invLine1.PK, entryLine.FirstLine.PK);

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines.AddNew();
			entryHeader.MergedLines.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1].JobComInvoiceLines[3].JI_CL = entryLine.PK;
			AssertEquals("Get FirstLine", invLine1.PK, entryLine.FirstLine.PK);
		}

		public virtual void TestFirstLine()
		{
			const string tarrifCode = "0000.00.00.00Y";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tarrifCode;
			line1.JI_CC = classification.PK;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = tarrifCode;
			line2.JI_CC = classification.PK;

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_Description = "Line 3 Desc";

			var line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = tarrifCode;
			line4.JI_CC = classification.PK;

			var line5 = invoice.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = tarrifCode;
			line5.JI_CC = classification.PK;

			var line6 = invoice.JobComInvoiceLines.AddNew();
			line6.JI_Tariff = tarrifCode;
			line6.JI_CC = classification.PK;
			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				line1.JI_CEI = instruction.PK;
				line2.JI_CEI = instruction.PK;
				line3.JI_CEI = instruction.PK;
				line4.JI_CEI = instruction.PK;
				line5.JI_CEI = instruction.PK;
				line6.JI_CEI = instruction.PK;
			}
			if (declaration.Branch.Country.Code != "US")
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			}

			if (declaration.CustomsEntryHeaders.Count > 0)
			{
				var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
				AssertEquals("Get FirstLine", line1.PK, entryLine1.FirstLine.PK);

				var entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[1];
				AssertEquals("Get FirstLine", line3.PK, entryLine2.FirstLine.PK);
			}
			else
			{
				Assert(declaration.Branch.Country.Code == "US" || declaration.IsDeclarationIntegrated);
			}
		}

		public virtual void TestDescriptionWhenMergedByClassification()
		{
			const string tariffCode = "0000.00.00.00Y";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tariffCode;
			line1.JI_CC = classification.PK;
			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				line1.JI_CEI = instruction.PK;
			}
			AssertEquals("Line1 Description should have defaulted from classification", classification.CC_Description, line1.JI_Description);
			if (declaration.Branch.Country.Code != "US")
			{
				AssertDescriptionAfterMerge(declaration, classification.CC_Description, 0);
			}

			if (declaration.CustomsEntryHeaders.Count > 0)
			{
				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_Tariff = tariffCode;
				line2.JI_CC = classification.PK;
				AssertEquals("Line2 Description should have defaulted from classification", classification.CC_Description, line2.JI_Description);
				AssertDescriptionAfterMerge(declaration, classification.CC_Description, 0);
				AssertEquals("Merge Lines Count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_Description = "LINE 3 DESC";
				AssertEquals("Line3 Description should have remained as entered", "LINE 3 DESC", line3.JI_Description);
				AssertDescriptionAfterMerge(declaration, "LINE 3 DESC", 1);
				AssertEquals("Merge Lines Count", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return entryLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var line = (CusEntryLine)GetNewBusinessObject();
			line.Fees.AddNew();
			line.Header.PivotsToContainers.RemoveAndDeleteAll();
			line.Header.PivotsToContainers.GetOrCreatePivotFor(line.Header.Declaration.CusContainers.AddNew());
			return line;
		}

		protected virtual void DoMerge(BaseJobDeclaration declaration)
		{
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected virtual BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = Factory.New<BaseJobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				return result;
			}
		}

		protected virtual ZString ExpectedFallbackEntrylineDescription => ZString.Empty;

		protected virtual ZString ExpectedClassificationDescription => "CUCKOO SQUEAKERS";

		protected virtual ZString ExpectedClassificationDescriptionMultipleLines => ExpectedClassificationDescription;

		protected virtual bool RatesAreReciprocal => false;

		protected virtual Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		void AssertDescriptionAfterMerge(BaseJobDeclaration declaration, string expectedDescription, int mergedLineIndex)
		{
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			if (declaration.CustomsEntryHeaders.Count > 0)
			{
				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[mergedLineIndex];
				AssertEquals("Description when merge by " + declaration.JE_MergeBy, expectedDescription, entryLine.Description);
			}
		}
	}
}
