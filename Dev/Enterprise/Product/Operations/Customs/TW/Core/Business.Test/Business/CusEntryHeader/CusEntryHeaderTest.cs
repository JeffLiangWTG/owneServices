using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		[ExpectNoExceptions]
		public void TestTotalNetWeightInKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TEST2";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;
			invoiceLine1.JI_NetWeight = 2031.1m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Grams;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice2.JZ_IncoTerm = "FOB";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.20.00.00-5";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 7738.2m;
			invoiceLine2.JI_NetWeight = 1.125m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var resStrings = DataBoundResourceStrings.GetDataForProperty(entryHeader.CH_TotalNetWeightInKilogramsInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.CH_TotalNetWeightInKilograms, NUnit.Framework.Is.EqualTo(3.156m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(typeof(CusEntryHeader), CustomConstraints.HasCustomAttribute<DecimalPlacesAttribute>(nameof(CusEntryHeader.CH_TotalNetWeightInKilograms), false, x => x.DecimalPlaces == 3));
				NUnit.Framework.Assert.That(resStrings.Caption, NUnit.Framework.Is.EqualTo("Total Net Weight (KG)"));
			});
		}

		[TestDate(2021, 3, 11)]
		[ExpectNoExceptions]
		public void TestEntryNumberShouldBeRemovedWhenSaveFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";

			var entryHeader = Factory.New<CusEntryHeaderThrowingExceptionAfterOnFactorySaving>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			Factory.Save();

			entryHeader.AllocateEntryNumber();
			NUnit.Framework.Assert.That(entryHeader.IsInDatabase, NUnit.Framework.Is.True, "Should be true.");
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  1012300001").Using(CustomComparers.TypeComparison));

			entryHeader.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "EntryNumber is removed as save failed.");
		}

		[ExpectNoExceptions]
		public void TestIsCurrencySameAsFirstInvoiceCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 1;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			NUnit.Framework.Assert.That(entryHeader.IsCurrencySameAsFirstInvoiceCurrency("USD"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be true");
			NUnit.Framework.Assert.That(!entryHeader.IsCurrencySameAsFirstInvoiceCurrency("EUR"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be false");
			NUnit.Framework.Assert.That(!entryHeader.IsCurrencySameAsFirstInvoiceCurrency("TWD"), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should be false");
		}

		[ExpectNoExceptions]
		public void TestFirstInvoiceCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TEST2";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST1";
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice2.JZ_IncoTerm = "FOB";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.20.00.00-5";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 7738.2m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.FirstInvoiceCurrency.Code, NUnit.Framework.Is.EqualTo("JPY"));
		}

		[ExpectNoExceptions]
		public void TestFirstInvoice_HandleDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV2";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line.JI_CEI = entryInstruction.PK;
			invoice1Line.JI_CL = entryLine.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_InvoiceNumber = "INV1";
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_CEI = entryInstruction.PK;
			invoice2Line.JI_CL = entryLine.PK;
			NUnit.Framework.Assert.That(entry.FirstInvoiceCurrencyCode, NUnit.Framework.Is.EqualTo(Core.Constants.CurrencyCodes.UnitedStates).Using(CustomComparers.TypeComparison), "FirstInvoiceCurrencyCode");
			invoice2.Delete();
			NUnit.Framework.Assert.That(entry.FirstInvoiceCurrencyCode, NUnit.Framework.Is.EqualTo(Core.Constants.CurrencyCodes.Australia).Using(CustomComparers.TypeComparison), "FirstInvoiceCurrencyCode");
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalInternationalInsuranceAmountInInvoiceCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(50m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(250.80m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportCostAndFreightEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(250.80m).Using(CustomComparers.TypeComparison));
		}

		CusEntryHeader GetExportCostAndFreightEntryHeaderTestCase()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 24;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 16;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 3605.33m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 3;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 7395.55m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 3;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 15282.23m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_InvoiceQuantity = 6;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 2205.37m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_InvoiceQuantity = 6;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 13982.23m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalInternationalFreightAmountInInvoiceCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(2300m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(18898m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalAdditionsInInvoiceCurrency()
		{
			var entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalAdditionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(18700m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalAdditionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(214.77m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalDeductionsInInvoiceCurrency()
		{
			var entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalDeductionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(2839.02m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalDeductionsInInvoiceCurrency, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalIMPFOBAmountInInvoiceCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(16000m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalEXPDisbursedAmountInInvoiceCurrency()
		{
			var entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(50758.98m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(42187.02m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalEXPDisbursedAmountInInvoiceCurrencyWhenInProcessOfMerging()
		{
			void JI_CLInfo_ValueChanged(object sender, EventArgs e)
			{
				var line = (JobComInvoiceLine)sender;
				var decl = line.Declaration;
				NUnit.Framework.Assert.That(decl.IsMergeInProgress, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(decl.EntryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			}

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			invoice.JZ_InvoiceAmount = 64156.48m;

			var charges = invoice.Charges;
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var dedCharge = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			dedCharge.J7_IsIncludedInITOT = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 32;
			invoiceLine.JI_InvoiceUQ = "PCE";
			invoiceLine.JI_EnteredUnitPrice = 2004.89m;
			invoiceLine.JI_CLInfo.ValueChanged += JI_CLInfo_ValueChanged;
			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(98915.46m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalInvoiceAmountInInvoiceCurrency()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CusEntryInstruction;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = "FOB";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice2.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 145.8m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency, NUnit.Framework.Is.EqualTo(800.04m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalRorCustomsValueInInvoiceCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalRorCustomsValueInInvoiceCurrency, NUnit.Framework.Is.EqualTo(18350m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalCustomsValueInInvoiceCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInInvoiceCurrency, NUnit.Framework.Is.EqualTo(18350m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInInvoiceCurrency, NUnit.Framework.Is.EqualTo(15749.20m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInInvoiceCurrency, NUnit.Framework.Is.EqualTo(42401.79m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_TotalCustomsValueInLocalCurrency()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInLocalCurrency, NUnit.Framework.Is.EqualTo(524260m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInLocalCurrency, NUnit.Framework.Is.EqualTo(474523m).Using(CustomComparers.TypeComparison));

			entryHeader = GetExportAndEXWEntryHeaderTestCase();
			NUnit.Framework.Assert.That(entryHeader.CH_TotalCustomsValueInLocalCurrency, NUnit.Framework.Is.EqualTo(1277566m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_CustomsFactor()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_CustomsFactor, NUnit.Framework.Is.EqualTo(1.146875m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCH_RorCustomsFactor()
		{
			var entryHeader = GetImportFobEntryHeaderTestCase(Factory);
			NUnit.Framework.Assert.That(entryHeader.CH_RorCustomsFactor, NUnit.Framework.Is.EqualTo(1.146875m).Using(CustomComparers.TypeComparison));
		}

		internal static CusEntryHeader GetImportFobEntryHeaderTestCase(BusinessObjectFactory factory)
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 16000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 2300m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 50m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "8419.20.00.00-5";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "IL";
			invoiceLine3.JI_Procedure = "50";
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 7738.2m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "4819.10.00.00-1";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "IL";
			invoiceLine4.JI_Procedure = "50";
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 108m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_Tariff = "84199020006";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "IL";
			invoiceLine5.JI_Procedure = "50";
			invoiceLine5.JI_InvoiceQuantity = 1m;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 145.8m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_Tariff = "84199020006";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "IL";
			invoiceLine6.JI_Procedure = "50";
			invoiceLine6.JI_InvoiceQuantity = 2m;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 160m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "84199020006";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "IL";
			invoiceLine7.JI_Procedure = "50";
			invoiceLine7.JI_InvoiceQuantity = 2m;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 88m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		internal static CusEntryHeader GetExportCostAndInsuranceEntryHeaderTestCase(BusinessObjectFactory factory)
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 16000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;

			var charges = invoice.Charges;
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var dedCharge = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			dedCharge.J7_IsIncludedInITOT = true;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 24;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 16;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 3605.33m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 3;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 7395.55m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 3;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 15282.23m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_InvoiceQuantity = 6;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 2205.37m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_InvoiceQuantity = 6;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 13982.23m;

			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		CusEntryHeader GetExportAndEXWEntryHeaderTestCase()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 34.58m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_InvoiceAmount = 41925m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

			var invoice1Charges = invoice1.Charges;
			invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 20m, Core.Constants.CurrencyCodes.UnitedStates);
			invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 10m, Core.Constants.CurrencyCodes.EuropeanUnion);
			var deductionCharge = invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			deductionCharge.J7_IsDutiable = false;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 165;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 185m;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 50;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 21m;

			var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 450;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 23m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_InvoiceAmount = 100m;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

			var invoice2Charges = invoice2.Charges;
			invoice2Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoice2Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 60m, Core.Constants.CurrencyCodes.EuropeanUnion);

			var groupCharge = invoice2.GroupHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.PackingCost, 67m, Core.Constants.CurrencyCodes.EuropeanUnion);
			groupCharge.J7_IsIncludedInITOT = false;
			groupCharge.J7_IsDutiable = true;

			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 100m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		[ExpectNoExceptions]
		public void TestShouldLogEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_IncoTerm = "CIF";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_LinePrice = 1000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decl = newFactory.Load<JobDeclaration>(declaration.PK);

			var entryHeader = decl.EntryHeader;
			var logs = entryHeader.Logs;

			entryHeader.CH_EntryStatus = "A28";
			newFactory.Save();
			var entryLog = logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			NUnit.Framework.Assert.That(entryLog.SL_Reference, NUnit.Framework.Is.EqualTo("A28").Using(CustomComparers.TypeComparison), "Has been logged");

			entryHeader.CH_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			newFactory.Save();
			entryLog = logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			NUnit.Framework.Assert.That(entryLog.SL_Reference, NUnit.Framework.Is.EqualTo(ClearanceStatusCodeList.Codes.C1).Using(CustomComparers.TypeComparison), "Has been logged");
		}

		[ExpectNoExceptions]
		public void TestIsEntryStatusCleared()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			NUnit.Framework.Assert.That(entryHeader.IsEntryStatusCleared, NUnit.Framework.Is.EqualTo(false), "IsEntryStatusCleared should be false");

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1111";
			cusNum.CE_EntryStatus = ZString.Empty;
			NUnit.Framework.Assert.That(entryHeader.IsEntryStatusCleared, NUnit.Framework.Is.EqualTo(false), "IsEntryStatusCleared should be false");

			cusNum.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			NUnit.Framework.Assert.That(entryHeader.IsEntryStatusCleared, NUnit.Framework.Is.EqualTo(true), "IsEntryStatusCleared should be true");
		}

		[ExpectNoExceptions]
		public void TestLogCustomsClearedToDeclarationOrShipment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryReleaseDate = DateTime.Now;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryReleaseDate = ZDateTime.Now;
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryReleaseDate = ZDateTime.Now;

			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = entryHeader1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "IMP";
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "1111";

			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = entryHeader2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "IMP";
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "2222";

			var cusNum3 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = entryHeader3.PK;
			cusNum3.CE_Category = "CUS";
			cusNum3.CE_EntryType = "IMP";
			cusNum3.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = "3333";
			Factory.Save();

			cusNum1.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			Factory.Save();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "CLR Log Entry event should not be created.");

			cusNum2.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			Factory.Save();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "CLR Log Entry event should not be created.");

			cusNum3.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			Factory.Save();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(true), "CLR Log Entry created");
		}

		[ExpectNoExceptions]
		public void TestCustomsClearedEventLoggedForImportCLR()
		{
			AssertCustomsClearedEventLoggedForImportCLR(ClearanceStatusCodeList.Codes.C1, true);
			AssertCustomsClearedEventLoggedForImportCLR(ClearanceStatusCodeList.Codes.C2, true);
			AssertCustomsClearedEventLoggedForImportCLR(ClearanceStatusCodeList.Codes.C3M, true);
			AssertCustomsClearedEventLoggedForImportCLR(ClearanceStatusCodeList.Codes.C3X, true);
			AssertCustomsClearedEventLoggedForImportCLR("CT2", true);
		}

		[ExpectNoExceptions]
		void AssertCustomsClearedEventLoggedForImportCLR(string entryStatusCode, bool expected)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryReleaseDate = DateTime.Now;

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1111";
			cusNum.CE_EntryStatus = ZString.Empty;
			Factory.Save();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "CLR Log Entry event should not be created.");

			cusNum.CE_EntryStatus = entryStatusCode;
			Factory.Save();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(expected), "CLR Log Entry created");

			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "ECC Log Entry event should not be created.");
		}

		[ExpectNoExceptions]
		public void TestCustomsClearedEventLoggedForExportECC()
		{
			AssertCustomsClearedEventLoggedForExportECC(ClearanceStatusCodeList.Codes.C1, true);
			AssertCustomsClearedEventLoggedForExportECC(ClearanceStatusCodeList.Codes.C2, true);
			AssertCustomsClearedEventLoggedForExportECC(ClearanceStatusCodeList.Codes.C3M, true);
			AssertCustomsClearedEventLoggedForExportECC(ClearanceStatusCodeList.Codes.C3X, true);
			AssertCustomsClearedEventLoggedForExportECC("C2T", true);
		}

		[ExpectNoExceptions]
		void AssertCustomsClearedEventLoggedForExportECC(string entryStatusCode, bool expected)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryReleaseDate = DateTime.Now;

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "EXP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1111";
			cusNum.CE_EntryStatus = ZString.Empty;
			Factory.Save();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "ECC Log Entry event should not be created.");

			cusNum.CE_EntryStatus = entryStatusCode;
			Factory.Save();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(expected), "ECC Log Entry created");

			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			NUnit.Framework.Assert.That(logEntries.Any(), NUnit.Framework.Is.EqualTo(false), "CLR Log Entry event should not be created.");
		}

		[TestDate(2020, 8, 5)]
		public void TestFillEntryNumberPlaceHolder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (entryHeader.GetGenerateEntryNumberExceptionSupporter())
			{
				AssertExceptionThrown<GenerateEntryNumberException>(() => entryHeader.AllocateEntryNumber());
			}
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			entryHeader.EntryNumber = "AAA";
			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			Factory.Save();
			NUnit.Framework.Assert.That(new BusinessObjectFactory().Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK)).Length, NUnit.Framework.Is.EqualTo(0));

			entryHeader.EntryNumber = "";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";

			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";

			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
			Factory.Save();
			NUnit.Framework.Assert.That(new BusinessObjectFactory().Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK)).Length, NUnit.Framework.Is.EqualTo(1));

			declaration.EntryNumber = "AAA";
			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			Factory.Save();
			NUnit.Framework.Assert.That(new BusinessObjectFactory().Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK)).Length, NUnit.Framework.Is.EqualTo(1));

			declaration.EntryNumber = ZString.Empty;
			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BB  0912300002").Using(CustomComparers.TypeComparison));

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.EntryNumber = "BBB";
			AssertNoExceptionThrown(() => entryHeader.AllocateEntryNumber());
			NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
			Factory.Save();
			NUnit.Framework.Assert.That(new BusinessObjectFactory().Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK)).Length, NUnit.Framework.Is.EqualTo(1));
		}

		[TestDate(2020, 8, 5)]
		[ExpectNoExceptions]
		public void TestDeclarationNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";

			NUnit.Framework.Assert.That(entry.EntryNumberForSendingObject, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.DeclarationNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entry.EntryNumber = "AA  0912300002";
			NUnit.Framework.Assert.That(entry.DeclarationNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.DeclarationEntryNumber, NUnit.Framework.Is.EqualTo("AA/  /09/123/00002").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(entry.EntryNumberForSendingObject, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
			entry.CH_Status = "AWO";
			NUnit.Framework.Assert.That(entry.DeclarationNumber, NUnit.Framework.Is.EqualTo("AA  0912300002").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.EntryNumberForSendingObject, NUnit.Framework.Is.EqualTo("AA  0912300002").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.DeclarationEntryNumber, NUnit.Framework.Is.EqualTo("AA/  /09/123/00002").Using(CustomComparers.TypeComparison));

			entry.CH_Status = "";
			entry.EntryNumber = "BB  0912300001";
			NUnit.Framework.Assert.That(entry.DeclarationNumber, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.EntryNumberForSendingObject, NUnit.Framework.Is.EqualTo("BB  0912300001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.DeclarationEntryNumber, NUnit.Framework.Is.EqualTo("BB/  /09/123/00001").Using(CustomComparers.TypeComparison));

			declaration.EntryNumber = "CC  0912300003";
			NUnit.Framework.Assert.That(entry.DeclarationNumber, NUnit.Framework.Is.EqualTo("CC  0912300003").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.DeclarationEntryNumber, NUnit.Framework.Is.EqualTo("CC/  /09/123/00003").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBusinessTaxBaseAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine3 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.InvoiceLines.Add(invoiceLine3);

			invoiceLine1.JI_VatPymntMthd = "CAS";
			invoiceLine2.JI_VatPymntMthd = "CAS";
			invoiceLine3.JI_VatPymntMthd = "CAS";
			entryLine1.CL_ValueForVAT = 150.2m;
			entryLine2.CL_ValueForVAT = 600m;
			entryLine3.CL_ValueForVAT = 12.4m;
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(762m).Using(CustomComparers.TypeComparison), "Business Tax Amount should be ");

			entryHeader.MergedLines.RemoveAndDelete(entryLine3);
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(750m).Using(CustomComparers.TypeComparison), "Business Tax Amount should be ");

			entryLine2.CL_ValueForVAT = 1000m;
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(1150m).Using(CustomComparers.TypeComparison), "Business Tax Amount should be ");

			invoiceLine1.JI_VatPymntMthd = "ROR";
			invoiceLine2.JI_VatPymntMthd = "ROR";
			invoiceLine3.JI_VatPymntMthd = "ROR";
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(1150m).Using(CustomComparers.TypeComparison), "Business Tax Amount should be ");

			invoiceLine1.JI_VatPymntMthd = "DEF";
			NUnit.Framework.Assert.That(entryHeader.BusinessTaxBaseAmount, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Business Tax Amount should be ");
		}

		[ExpectNoExceptions]
		public void TestTotalTaxAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var charge1 = entryHeader.Charges.AddNew();
			var charge2 = entryHeader.Charges.AddNew();
			var charge3 = entryHeader.Charges.AddNew();

			charge1.C1_ChargeAmount = 1250.9m;
			charge2.C1_ChargeAmount = 100.01m;
			charge3.C1_ChargeAmount = 500m;
			NUnit.Framework.Assert.That(entryHeader.TotalTaxAmount, NUnit.Framework.Is.EqualTo(1850m).Using(CustomComparers.TypeComparison), "Total Tax Amount should be ");

			entryHeader.Charges.RemoveAndDelete(charge3);
			NUnit.Framework.Assert.That(entryHeader.TotalTaxAmount, NUnit.Framework.Is.EqualTo(1350m).Using(CustomComparers.TypeComparison), "Total Tax Amount should be ");

			charge2.C1_ChargeAmount = 50.01m;
			NUnit.Framework.Assert.That(entryHeader.TotalTaxAmount, NUnit.Framework.Is.EqualTo(1300m).Using(CustomComparers.TypeComparison), "Total Tax Amount should be ");

			var line = entryHeader.MergedLines.AddNew();
			var lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee.CF_Rate = 1m;
			lineFee.CF_MethodOfPayment = "A";
			lineFee.CF_ChargeAmount = 2M;
			NUnit.Framework.Assert.That(entryHeader.TotalTaxAmount, NUnit.Framework.Is.EqualTo(1302m).Using(CustomComparers.TypeComparison), "Total Tax Amount should be ");
		}

		[ExpectNoExceptions]
		public void TestTotalCashTaxAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var charge1 = entryHeader.Charges.AddNew();
			var charge2 = entryHeader.Charges.AddNew();
			var charge3 = entryHeader.Charges.AddNew();

			charge1.C1_ChargeAmount = 1250.9m;
			charge1.C1_MethodOfPayment = "AAA";
			charge2.C1_ChargeAmount = 100.01m;
			charge2.C1_MethodOfPayment = "CAS";
			charge3.C1_ChargeAmount = 500m;
			charge3.C1_MethodOfPayment = "DEF";
			NUnit.Framework.Assert.That(entryHeader.TotalCashTaxAmount, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Cash) should be ");

			var line = entryHeader.MergedLines.AddNew();
			var lineFee1 = line.Fees.AddNew();
			lineFee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee1.CF_Rate = 1m;
			lineFee1.CF_MethodOfPayment = "A";
			lineFee1.CF_ChargeAmount = 2m;
			var lineFee2 = line.Fees.AddNew();
			lineFee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			lineFee2.CF_Rate = 1m;
			lineFee2.CF_MethodOfPayment = "CAS";
			lineFee2.CF_ChargeAmount = 4m;
			var lineFee3 = line.Fees.AddNew();
			lineFee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
			lineFee3.CF_Rate = 1m;
			lineFee3.CF_MethodOfPayment = "DEF";
			lineFee3.CF_ChargeAmount = 8m;
			NUnit.Framework.Assert.That(entryHeader.TotalCashTaxAmount, NUnit.Framework.Is.EqualTo(100m + 4m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Cash) should be ");
			lineFee1.CF_MethodOfPayment = "CAS";
			NUnit.Framework.Assert.That(entryHeader.TotalCashTaxAmount, NUnit.Framework.Is.EqualTo(100m + 6m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Cash) should be ");
		}

		[ExpectNoExceptions]
		public void TestTotalNonCashTaxAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var charge1 = entryHeader.Charges.AddNew();
			var charge2 = entryHeader.Charges.AddNew();
			var charge3 = entryHeader.Charges.AddNew();

			charge1.C1_ChargeAmount = 1250.9m;
			charge1.C1_MethodOfPayment = "AAA";
			charge2.C1_ChargeAmount = 100.01m;
			charge2.C1_MethodOfPayment = "CAS";
			charge3.C1_ChargeAmount = 500m;
			charge3.C1_MethodOfPayment = "DEF";
			NUnit.Framework.Assert.That(entryHeader.TotalNonCashTaxAmount, NUnit.Framework.Is.EqualTo(500m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Non-Cash) should be ");

			var line = entryHeader.MergedLines.AddNew();
			var lineFee1 = line.Fees.AddNew();
			lineFee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee1.CF_Rate = 1m;
			lineFee1.CF_MethodOfPayment = "A";
			lineFee1.CF_ChargeAmount = 2m;
			var lineFee2 = line.Fees.AddNew();
			lineFee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			lineFee2.CF_Rate = 1m;
			lineFee2.CF_MethodOfPayment = "CAS";
			lineFee2.CF_ChargeAmount = 4m;
			var lineFee3 = line.Fees.AddNew();
			lineFee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
			lineFee3.CF_Rate = 1m;
			lineFee3.CF_MethodOfPayment = "DEF";
			lineFee3.CF_ChargeAmount = 8m;
			NUnit.Framework.Assert.That(entryHeader.TotalNonCashTaxAmount, NUnit.Framework.Is.EqualTo(500m + 8m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Non-Cash) should be ");
			lineFee1.CF_MethodOfPayment = "DEF";
			NUnit.Framework.Assert.That(entryHeader.TotalNonCashTaxAmount, NUnit.Framework.Is.EqualTo(500m + 10m).Using(CustomComparers.TypeComparison), "Total Tax Amount (Non-Cash) should be ");
		}

		[ExpectNoExceptions]
		public void TestCH_ConfirmedBusinessTaxBaseAndCH_ConfirmedTotalDutyTaxFee()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271911", "B40", 100m, 360493m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271911", "B51", 100m, 360493m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271911", "A10", 100m, 360493m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271911", "A09", 100m, 360493m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271912", "B40", 100m, 260491m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271912", "B59", 100m, 260491m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271912", "A20", 100m, 260491m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271913", "A08", 100m, 150213m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271913", "A09", 100m, 150213m);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedTotalDutyTaxFee, NUnit.Framework.Is.EqualTo(600m).Using(CustomComparers.TypeComparison), "CH_ConfirmedTotalDutyTaxFee");
				NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedBusinessTaxBase, NUnit.Framework.Is.EqualTo(620984m).Using(CustomComparers.TypeComparison), "CH_ConfirmedBusinessTaxBase");
			});

			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271914", "B69", 100m, 3m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, "CGI31131271914", "B79", 100m, 3m);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedTotalDutyTaxFee, NUnit.Framework.Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "CH_ConfirmedTotalDutyTaxFee");
				NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedBusinessTaxBase, NUnit.Framework.Is.EqualTo(620987m).Using(CustomComparers.TypeComparison), "CH_ConfirmedBusinessTaxBase");
			});
		}

		[ExpectNoExceptions]
		public void TestCH_ConfirmedTotalDutyTaxFeeDeferred()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var incomingPayResponseNo1 = "ABI32100355835";
			var incomingPayResponseNo2 = "ABI32100355836";
			var payInfos = entryHeader.EntryPayInfos;
			var payInfo1 = payInfos.AddNew();
			var receiptDate = new ZDateTime(2021, 8, 3);
			payInfo1.C9_IncomingPayResponseNo = incomingPayResponseNo1;
			payInfo1.C9_PaymentAmount = 100m;
			payInfo1.C9_TransactionType = "D10";
			payInfo1.C9_PaymentDate = receiptDate.AddDays(14);
			payInfo1.C9_PaymentReference = "88888888";
			payInfo1.C9_ReceiptDate = receiptDate.Date;
			payInfo1.C9_BankAccount = "3070500001";
			payInfo1.C9_PaymentReasonCode = "2";

			NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedTotalDutyTaxFeeDeferred, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));

			var payInfo2 = payInfos.AddNew();
			receiptDate = new ZDateTime(2021, 8, 4);
			payInfo2.C9_IncomingPayResponseNo = incomingPayResponseNo2;
			payInfo2.C9_PaymentAmount = 200m;
			payInfo2.C9_TransactionType = "D10";
			payInfo2.C9_PaymentDate = receiptDate.AddDays(14);
			payInfo2.C9_PaymentReference = "88888888";
			payInfo2.C9_ReceiptDate = receiptDate.Date;
			payInfo2.C9_BankAccount = "3070500001";
			payInfo2.C9_PaymentReasonCode = "2";
			NUnit.Framework.Assert.That(entryHeader.CH_ConfirmedTotalDutyTaxFeeDeferred, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReadOnlyFields()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.CH_EntryReleaseDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "Release Date should be readonly");
				NUnit.Framework.Assert.That(entryHeader.TotalTaxAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "Total tax amount should be readonly");
			});
		}

		[ExpectNoExceptions]
		public void TestStaffCode()
		{
			var twMessageInfoProvider = entryHeader as ITWMessageInfoProvider;
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			NUnit.Framework.Assert.That(ZString.Empty, NUnit.Framework.Is.EqualTo(twMessageInfoProvider.StaffCode));
			declaration.JE_GS_NKCusAgent = "111";
			NUnit.Framework.Assert.That(twMessageInfoProvider.StaffCode, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberType()
		{
			var twMessageInfoProvider = entryHeader as ITWMessageInfoProvider;
			declaration.JE_MessageType = "EXP";
			NUnit.Framework.Assert.That(twMessageInfoProvider.EntryNumberType, NUnit.Framework.Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));

			declaration.JE_MessageType = "IMP";
			NUnit.Framework.Assert.That(twMessageInfoProvider.EntryNumberType, NUnit.Framework.Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEntryNumber()
		{
			var twMessageInfoProvider = entryHeader as ITWMessageInfoProvider;
			declaration.MarkLightValidationAsValidForTesting();

			NUnit.Framework.Assert.That(declaration.LightValidationIsValid, NUnit.Framework.Is.True);

			entryHeader.EntryNumber = "111";

			NUnit.Framework.Assert.That(twMessageInfoProvider.EntryNumber, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(!declaration.LightValidationIsValid, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestMessageInfoProvider_CompanyCode()
		{
			if (entryHeader is ITWMessageInfoProvider messageInfoProvider)
			{
				NUnit.Framework.Assert.That(messageInfoProvider.CompanyID, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_Code));
			}
		}

		[ExpectNoExceptions]
		public void TestMessageInfoProvider_Platform()
		{
			var newFactory = new BusinessObjectFactory();
			var staff = newFactory.New<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword = newFactory.New<GlbExternalPassword>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_MailBoxID = "123-3";
			extPassword.GP_UserID = "001";
			extPassword.GP_GS = staff.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			newFactory.Save();

			if (entryHeader is ITWMessageInfoProvider messageInfoProvider)
			{
				NUnit.Framework.Assert.That(messageInfoProvider.PasswordType, NUnit.Framework.Is.EqualTo(PasswordTypesList.Codes.TVA).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestHasBeenLodgedAtCustoms()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var heard = declaration.CustomsEntryHeaders.AddNew();
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			var disposition = heard.CusDispositions.AddNew();
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_Status = "A01";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			disposition.CDI_Status = "B01";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			disposition.CDI_Status = "F01";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			disposition.CDI_Status = "A01";
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ERM;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});

			heard.CH_EntryStatus = ZString.Empty;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!heard.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!heard.IsRejectedByCustoms, NUnit.Framework.Is.True);
			});
		}

		readonly ZString[] awaitingStatusList = new ZString[] { JobDeclarationMessageStatusList.Codes.AWO, JobDeclarationMessageStatusList.Codes.AWC, JobDeclarationMessageStatusList.Codes.AWG, JobDeclarationMessageStatusList.Codes.AWE };
		[ExpectNoExceptions]
		public void TestIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var heard = declaration.CustomsEntryHeaders.AddNew();
			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = heard.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1111";

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			var disposition = heard.CusDispositions.AddNew();
			disposition.CDI_StatusDate = new DateTime(2022, 1, 1);
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Status = "A01";
			NUnit.Framework.Assert.That(!heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);

			foreach (var status in awaitingStatusList)
			{
				heard.CH_Status = status;
				NUnit.Framework.Assert.That(heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
			}
			heard.CH_Status = ZString.Empty;
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			NUnit.Framework.Assert.That(!heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
			disposition.CDI_Status = "F88";
			NUnit.Framework.Assert.That(heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
			heard.CH_EntryStatus = ZString.Empty;
			NUnit.Framework.Assert.That(!heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);

			heard.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			NUnit.Framework.Assert.That(heard.IsWaitingForResponseOrHasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestDeclarationType()
		{
			var declartion = Factory.New<JobDeclaration>();
			var entryInstruction = declartion.CusEntryInstruction;

			var entryHeader1 = declartion.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

			entryInstruction.CEI_Style = "G1";
			NUnit.Framework.Assert.That(entryHeader1.DeclarationType, NUnit.Framework.Is.EqualTo("G1").Using(CustomComparers.TypeComparison));

			var entryHeader2 = declartion.CustomsEntryHeaders.AddNew();
			NUnit.Framework.Assert.That(entryHeader2.DeclarationType, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestUCRNumber()
		{
			var declartion = Factory.New<JobDeclaration>();
			var entryInstruction = declartion.CusEntryInstruction;

			var entryHeader1 = declartion.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction.PK;

			entryInstruction.UCRNumber = "111";
			NUnit.Framework.Assert.That(entryHeader1.UCRNumber, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison));

			var entryHeader2 = declartion.CustomsEntryHeaders.AddNew();
			NUnit.Framework.Assert.That(entryHeader2.UCRNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestIsWaitingForResponse()
		{
			var heard = Factory.NewWithValidTestData<CusEntryHeader>();
			foreach (var status in awaitingStatusList)
			{
				heard.CH_Status = status;
				NUnit.Framework.Assert.That(heard.IsWaitingForResponse, NUnit.Framework.Is.True);
			}
			heard.CH_Status = "AA";
			NUnit.Framework.Assert.That(!heard.IsWaitingForResponse, NUnit.Framework.Is.True);
			heard.CH_Status = ZString.Empty;
			NUnit.Framework.Assert.That(!heard.IsWaitingForResponse, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public override void TestGoodsTypeForDocumentFilter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			NUnit.Framework.Assert.That(entryHeader.GoodsTypeForDocumentFilter.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.ResetTotalsAndCachedValues();
			NUnit.Framework.Assert.That(entryHeader.GoodsTypeForDocumentFilter.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			var invoice0 = declaration.Invoices.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInstruction.PK;
			line0.JI_Description = @"1";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line0.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;

			var chassis1 = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			entryHeader.ResetTotalsAndCachedValues();
			NUnit.Framework.Assert.That(entryHeader.GoodsTypeForDocumentFilter, NUnit.Framework.Is.EqualTo("VHC").Using(CustomComparers.TypeComparison));

			entryHeader.ResetTotalsAndCachedValues();
			chassis1.JG_ReferenceNumber = "";
			NUnit.Framework.Assert.That(entryHeader.GoodsTypeForDocumentFilter.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestDocumentSupporter()
		{
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter, NUnit.Framework.Is.TypeOf(typeof(CusEntryHeaderDocumentSupporter)));
		}

		[ExpectNoExceptions]
		public void TestChassisNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.ResetTotalsAndCachedValues();

			var invoice0 = declaration.Invoices.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInstruction.PK;
			line0.JI_Description = @"1";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line0.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;

			NUnit.Framework.Assert.That(entryHeader.ChassisNumbers.Count, NUnit.Framework.Is.EqualTo(0));
			var chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			NUnit.Framework.Assert.That(entryHeader.ChassisNumbers.Count, NUnit.Framework.Is.EqualTo(1));
			chassis.JG_ReferenceNumber = "";
			NUnit.Framework.Assert.That(entryHeader.ChassisNumbers.Count, NUnit.Framework.Is.EqualTo(0));

			chassis.JG_ReferenceNumber = "123456";
			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			NUnit.Framework.Assert.That(entryHeader.ChassisNumbers.Count, NUnit.Framework.Is.EqualTo(1));

			chassis.JG_ReferenceNumber = "1234567";
			NUnit.Framework.Assert.That(entryHeader.ChassisNumbers.Count, NUnit.Framework.Is.EqualTo(2));

			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "";

			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "422211";
			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "111111";

			var chassisNumbers = entryHeader.ChassisNumbers;
			NUnit.Framework.Assert.That(chassisNumbers[0], NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chassisNumbers[1], NUnit.Framework.Is.EqualTo("1234567").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chassisNumbers[2], NUnit.Framework.Is.EqualTo("422211").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chassisNumbers[3], NUnit.Framework.Is.EqualTo("111111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeCharges()
		{
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee.CF_Rate = 1m;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			lineFee.CF_ChargeAmount = 2M;

			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee.CF_Rate = 1m;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			lineFee.CF_ChargeAmount = 2M;

			var charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 2M;

			charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 2M;

			var dutyTaxFeeCharges = header.DutyTaxFeeCharges;
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A10" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 6M), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A19" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 2M), NUnit.Framework.Is.True);

			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "TAT";
			lineFee.CF_Rate = 1m;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			lineFee.CF_ChargeAmount = 2M;

			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(2));
			dutyTaxFeeCharges = header.DutyTaxFeeCharges;
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B69" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 2M), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestFirstInvoiceCurrencyCode()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = entryLine11.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice1.PK;

			var freight = invoiceLine.Charges.AddNew();
			freight.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_RX_NKCurrency = "USD";
			freight.J7_Amount = 120.00m;

			NUnit.Framework.Assert.That(entry1.FirstInvoiceCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));

			freight.J7_RX_NKCurrency = "RMB";
			NUnit.Framework.Assert.That(entry1.FirstInvoiceCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFirstInvoiceCurrencyExRate()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_InvoiceCurrExRate = 0.5M;

			var invoiceLine = entryLine11.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice1.PK;

			var freight = invoiceLine.Charges.AddNew();
			freight.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			freight.J7_RX_NKCurrency = "USD";
			freight.J7_Amount = 120.00m;

			NUnit.Framework.Assert.That(entry1.FirstInvoiceCurrencyExRate, NUnit.Framework.Is.EqualTo(0.5M).Using(CustomComparers.TypeComparison));

			freight.J7_RX_NKCurrency = "RMB";
			NUnit.Framework.Assert.That(entry1.FirstInvoiceCurrencyExRate, NUnit.Framework.Is.EqualTo(0.5M).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestOverseasFreight()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 29.925m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 29.925m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceChargeCollection = invoice.Charges;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_PartNo = "PARTNO1";
			invoiceLine3.JI_InvoiceQuantity = 2;
			invoiceLine3.JI_InvoiceUQ = "PCE";

			var oFTCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			oFTCharge.J7_IsNotIncludedInInvoice = false;

			var oNSCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			oNSCharge.J7_IsNotIncludedInInvoice = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.OverseasFreight.Amount, NUnit.Framework.Is.EqualTo(50m).Using(CustomComparers.TypeComparison), "Freight");

			invoiceChargeCollection.RemoveAll();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			oFTCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			oFTCharge.J7_IsNotIncludedInInvoice = false;

			oNSCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			oNSCharge.J7_IsNotIncludedInInvoice = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.OverseasFreight.Amount, NUnit.Framework.Is.EqualTo(50m).Using(CustomComparers.TypeComparison), "Freight");
		}

		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestOverseasInsurance()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 29.925m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 29.925m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 300m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceChargeCollection = invoice.Charges;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_PartNo = "PARTNO1";
			invoiceLine3.JI_InvoiceQuantity = 2;
			invoiceLine3.JI_InvoiceUQ = "PCE";

			var oFTCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			oFTCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			oFTCharge.J7_IsNotIncludedInInvoice = false;

			var oNSCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			oNSCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			oNSCharge.J7_IsNotIncludedInInvoice = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.OverseasInsurance.Amount, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison), "Freight");

			invoiceChargeCollection.RemoveAll();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			oFTCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			oFTCharge.J7_IsNotIncludedInInvoice = false;

			oNSCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedStates);
			oNSCharge.J7_IsNotIncludedInInvoice = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.OverseasInsurance.Amount, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison), "Freight");
		}

		[ExpectNoExceptions]
		public void TestApplicationAndCertificateDocumentWrappers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.JI_NetWeight = 10m;
			line.JI_NetWeightUQ = "KG";
			line.JI_CEI = entryInstruction.PK;
			line.JI_DeclarationGoodsDescription = "test declaration goods description";
			var chassis = line.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			var wrapper = (ApplicationAndCertificateDocumentWrapper)entryHeader.ApplicationAndCertificateDocumentWrappers.First();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeader.ApplicationAndCertificateDocumentWrappers.Count, NUnit.Framework.Is.EqualTo(1), "Count");
				NUnit.Framework.Assert.That(wrapper.Lines[0].Line1, NUnit.Framework.Is.EqualTo("test declaration goods description").Using(CustomComparers.TypeComparison), "Line1");
				NUnit.Framework.Assert.That(wrapper.NetWeightInKG, NUnit.Framework.Is.EqualTo(10m).Using(CustomComparers.TypeComparison), "NetWeightInKG");
			});
		}

		[ExpectNoExceptions]
		public void TestDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var type = entryHeader.GetType();
			NUnit.Framework.Assert.That(type, CustomConstraints.HasCustomAttribute<DecimalPlacesAttribute>("CustomsValue", true, attrib => attrib.DecimalPlaces == 0));
			NUnit.Framework.Assert.That(type, CustomConstraints.HasCustomAttribute<DecimalPlacesAttribute>("TotalTaxAmount", true, attrib => attrib.DecimalPlaces == 0));
		}

		[ExpectNoExceptions]
		public void TestLastSentOutgoingInterchangeNumber()
		{
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "NUM1";
			interchange1.EI_From = "TWCustoms.TEST";
			interchange1.EI_To = "TEST";
			interchange1.EI_ApplicationCode = "TWC";
			interchange1.EI_InterchangeType = "TWC";
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_IsActive = true;
			interchange1.EI_BodyText = "AAA";

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "NUM2";
			interchange2.EI_From = "TWCustoms.TEST";
			interchange2.EI_To = "TEST";
			interchange2.EI_ApplicationCode = "TWC";
			interchange2.EI_InterchangeType = "TWC";
			interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange2.EI_Status = EDIInterchange.Status.Queued;
			interchange2.EI_IsActive = true;
			interchange2.EI_BodyText = "AAA";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "111";
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message1.EM_Status = EDIMessage.Status.Sent;
			message1.EM_EI = interchange1.PK;
			message1.EM_MessageType = "A1";
			message1.EM_MessageText = "AA";

			var message2 = entryHeader.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message2.EM_Status = EDIMessage.Status.Sent;
			message2.EM_EI = interchange2.PK;
			message2.EM_MessageType = "A1";
			message2.EM_MessageText = "AA";

			var message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.EM_MessageType = "A1";
			message.EM_MessageText = "AA";
			Factory.Save();

			message.EM_MessageType = "A1";
			message1.EM_MessageType = "A1";
			message2.EM_MessageType = "A2";
			NUnit.Framework.Assert.That(entryHeader.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType), NUnit.Framework.Is.EqualTo("NUM1").Using(CustomComparers.TypeComparison));

			message.EM_MessageType = "A2";
			message1.EM_MessageType = "A1";
			message2.EM_MessageType = "A2";
			NUnit.Framework.Assert.That(entryHeader.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType), NUnit.Framework.Is.EqualTo("NUM2").Using(CustomComparers.TypeComparison));

			message.EM_MessageType = "A3";
			message1.EM_MessageType = "A1";
			message2.EM_MessageType = "A2";
			NUnit.Framework.Assert.That(entryHeader.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			message.EM_MessageType = "A1";
			message1.EM_MessageType = "A1";
			message2.EM_MessageType = "A1";
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2020, 01, 01);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2020, 01, 02);
			NUnit.Framework.Assert.That(entryHeader.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType), NUnit.Framework.Is.EqualTo("NUM2").Using(CustomComparers.TypeComparison));

			message1.EM_SystemCreateTimeUtc = new ZDateTime(2020, 01, 02);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2020, 01, 01);
			NUnit.Framework.Assert.That(entryHeader.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType), NUnit.Framework.Is.EqualTo("NUM1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotes = shipment.Notes.VisibleNotes;
			var noteD = shipmentNotes.AddNew();
			noteD.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteD.ST_NoteContextModule = nameof(StmNoteContextModule.D);
			var d600 = new ZString('D', 600);
			noteD.ST_NoteText = d600;

			var noteA = shipmentNotes.AddNew();
			var a600 = new ZString('A', 600);
			noteA.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteA.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteA.ST_NoteText = a600;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_JS = shipment.PK;

			var invoiceHeader1 = CreateInvoiceHeader(declaration, "INV1");
			var invoiceHeader2 = CreateInvoiceHeader(declaration, "INV2");
			var invoiceHeader3 = CreateInvoiceHeader(declaration, "INV3");
			var invoiceHeader4 = CreateInvoiceHeader(declaration, "INV4");

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var j600 = new ZString('J', 600);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			invoiceHeader1.TW_MarksAndNumbers = "MARKS AND NUMBER 1";
			invoiceHeader2.TW_MarksAndNumbers = "MARKS AND NUMBER 1";
			invoiceHeader3.TW_MarksAndNumbers = "MARKS AND NUMBER 3";
			invoiceHeader4.TW_MarksAndNumbers = j600;

			NUnit.Framework.Assert.That(entryHeader.MarksAndNumbers, NUnit.Framework.Is.EqualTo(d600));

			shipmentNotes.Remove(noteD);
			NUnit.Framework.Assert.That(entryHeader.MarksAndNumbers, NUnit.Framework.Is.EqualTo("MARKS AND NUMBER 1\r\nMARKS AND NUMBER 3\r\n" + j600).Using(CustomComparers.TypeComparison));

			invoiceHeader1.TW_MarksAndNumbers = ZString.Empty;
			invoiceHeader2.TW_MarksAndNumbers = ZString.Empty;
			invoiceHeader3.TW_MarksAndNumbers = ZString.Empty;
			invoiceHeader4.TW_MarksAndNumbers = ZString.Empty;
			NUnit.Framework.Assert.That(entryHeader.MarksAndNumbers, NUnit.Framework.Is.EqualTo(a600));

			shipmentNotes.Remove(noteA);
			NUnit.Framework.Assert.That(entryHeader.MarksAndNumbers, NUnit.Framework.Is.EqualTo("N/M").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEntryHeaderStatusDescription()
		{
			entryHeader.CH_EntryStatus = ZString.Empty;
			NUnit.Framework.Assert.That(entryHeader.EntryHeaderStatusDescription, NUnit.Framework.Is.EqualTo(EntryStatusCodeList.Descriptions.NotReceive).Using(CustomComparers.TypeComparison));
			entryHeader.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			NUnit.Framework.Assert.That(entryHeader.EntryHeaderStatusDescription, NUnit.Framework.Is.EqualTo(EntryStatusCodeList.Descriptions.ARM).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultStatusDescription()
		{
			entryHeader.CH_EntryStatus = ZString.Empty;
			NUnit.Framework.Assert.That(entryHeader.DefaultStatusDescription, NUnit.Framework.Is.EqualTo(EntryStatusCodeList.Descriptions.NotReceive).Using(CustomComparers.TypeComparison));
			entryHeader.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			NUnit.Framework.Assert.That(entryHeader.DefaultStatusDescription, NUnit.Framework.Is.EqualTo(EntryStatusCodeList.Descriptions.NotReceive).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLogEventWhenAllocateNewEntryNumber()
		{
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_BoxNumber = "AAA";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var logs = entryHeader.Logs;
			var filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(0));
			entryHeader.AllocateEntryNumber();
			Factory.Save();
			filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(filtered.First().SL_Reference, NUnit.Framework.Is.EqualTo(entryHeader.EntryNumber));

			var firstEntryNumber = entryHeader.EntryNumber;

			entryHeader.ThrowAwayEntryNumber();
			filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(1));

			entryHeader.AllocateEntryNumber();
			Factory.Save();
			filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(filtered.ElementAt(1).SL_Reference, NUnit.Framework.Is.EqualTo(entryHeader.EntryNumber));

			entryHeader.EntryNumber = firstEntryNumber;
			filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(filtered.ElementAt(2).SL_Reference, NUnit.Framework.Is.EqualTo(entryHeader.EntryNumber));

			filtered = logs.Find(x => x.Event.SE_Code == Events.CustomsNumberEntered.Code && x.SL_Reference == firstEntryNumber);
			NUnit.Framework.Assert.That(filtered.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[TestDate(2021, 07, 14)]
		[ExpectNoExceptions]
		public void TestPopulateEntrySubmittedDateIfRequired()
		{
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;

			var submittedDateTime = ZDateTime.Today.AddDays(-1);
			entryHeader.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			var expectedDate = new ZDateTime(2021, 7, 13);
			NUnit.Framework.Assert.That(entryHeader.CH_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));

			submittedDateTime = ZDateTime.Today.AddDays(-2);
			entryHeader.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			expectedDate = new ZDateTime(2021, 7, 12);
			NUnit.Framework.Assert.That(entryHeader.CH_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));

			entryHeader.PopulateEntrySubmittedDateIfRequired();
			expectedDate = new ZDateTime(2021, 7, 14);
			NUnit.Framework.Assert.That(entryHeader.CH_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));
			NUnit.Framework.Assert.That(declaration.JE_EntrySubmittedDate, NUnit.Framework.Is.EqualTo(expectedDate));
		}

		class CusEntryHeaderThrowingExceptionAfterOnFactorySaving : CusEntryHeader
		{
			public CusEntryHeaderThrowingExceptionAfterOnFactorySaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowException;

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				if (ShouldThrowException)
				{
					throw new ApplicationException("intended");
				}
			}
		}

		JobComInvoiceHeader CreateInvoiceHeader(JobDeclaration declaration, ZString invoiceNumber)
		{
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceNumber = invoiceNumber;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var line = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.JI_CEI = entryInstruction.PK;
			return invoiceHeader;
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			base.SetUp();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;

		[ExpectNoExceptions]
		public void TestMessageCusEntryNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			warehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "00612348", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration1.PK;
			var cusHead2 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_JE = declaration2.PK;
			var cusHead3 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead3.CH_JE = declaration3.PK;
			var entryInstruction1 = declaration1.CusEntryInstruction;
			entryInstruction1.CEI_Style = "G2";
			entryInstruction1.CEI_CustomsOffice = "BA";
			entryInstruction1.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			cusHead1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryInstruction2 = declaration2.CusEntryInstruction;
			entryInstruction2.CEI_Style = "G2";
			entryInstruction2.CEI_CustomsOffice = "BA";
			entryInstruction2.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			cusHead2.CH_CEI_Instruction = entryInstruction2.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusHead2.PK;
			cusNum2.CE_Category = "XXX";
			cusNum2.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			Factory.Save();
			NUnit.Framework.Assert.That(cusHead1.CusEntryNumber.PK, NUnit.Framework.Is.EqualTo(cusNum1.PK));
			NUnit.Framework.Assert.That(cusHead1.EntryNumber, NUnit.Framework.Is.EqualTo(cusNum1.CE_EntryNum));
			NUnit.Framework.Assert.That(cusHead2.EntryNumber, NUnit.Framework.Is.EqualTo(cusNum2.CE_EntryNum));
			NUnit.Framework.Assert.That(cusHead3.EntryNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.AddNew();
			NUnit.Framework.Assert.That(entryHeader.Messages, NUnit.Framework.Is.TypeOf<TWMessageCollection>());
		}

		[ExpectNoExceptions]
		public void TestEntryPayInfos()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			NUnit.Framework.Assert.That(entryHeader.EntryPayInfos, NUnit.Framework.Is.TypeOf<CusEntryPayInfoCollection<CusEntryPayInfo>>());
		}

		[ExpectNoExceptions]
		public void TestCusDispositionCollection()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			NUnit.Framework.Assert.That(entryHeader.CusDispositions, NUnit.Framework.Is.TypeOf<CusDispositionCollection>());
		}

		[ExpectNoExceptions]
		public void TestCusDispositionOrder()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

			var disposition1 = entryHeader.CusDispositions.AddNew();
			disposition1.CDI_StatusDate = new DateTime(2022, 1, 1);
			disposition1.CDI_Type = "CUS";
			disposition1.CDI_StatusKey = "RFM";
			disposition1.CDI_Status = "A01";

			var disposition2 = entryHeader.CusDispositions.AddNew();
			disposition2.CDI_StatusDate = new DateTime(2022, 1, 2);
			disposition2.CDI_Type = "CUS";
			disposition2.CDI_StatusKey = "RFM";
			disposition2.CDI_Status = "A02";

			var disposition3 = entryHeader.CusDispositions.AddNew();
			disposition3.CDI_StatusDate = new DateTime(2022, 1, 3);
			disposition3.CDI_Type = "CUS";
			disposition3.CDI_StatusKey = "RFM";
			disposition3.CDI_Status = "A03";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadEntryHeader = factory.Load<CusEntryHeader>(entryHeader.PK);
			NUnit.Framework.Assert.That(loadEntryHeader.CusDispositions.Select(c => c.PK), NUnit.Framework.Is.EqualTo(new[] { disposition3.PK, disposition2.PK, disposition1.PK }));
		}

		[ExpectNoExceptions]
		public void TestICusDispositionParent()
		{
			var entryHeaderICusDispositionParent = (ICusDispositionParent)Factory.New<CusEntryHeader>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryHeaderICusDispositionParent.Type, NUnit.Framework.Is.EqualTo(ZString.Empty), "Type");
				NUnit.Framework.Assert.That(entryHeaderICusDispositionParent.ParentTableCode, NUnit.Framework.Is.EqualTo(CusEntryHeaderSchema.Constants.Prefix).Using(CustomComparers.TypeComparison), "ParentTableCode");
				NUnit.Framework.Assert.That(entryHeaderICusDispositionParent.CollectionMaster, NUnit.Framework.Is.SameAs(entryHeaderICusDispositionParent), "CollectionMaster");
				NUnit.Framework.Assert.That(entryHeaderICusDispositionParent.GetStatusDescription("A5"), NUnit.Framework.Is.EqualTo(ZString.Empty), "GetStatusDescription");
			});
		}

		[TestDate(2005, 6, 2)]
		[ExpectNoExceptions]
		public override void TestFOBInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			var from = new ZDateTime(2005, 6, 1);
			var to = new ZDateTime(2005, 6, 5);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			NUnit.Framework.Assert.That(invoiceHeader.EffectiveValuationDate.IsValid, NUnit.Framework.Is.True, "(pre-condition) invoiceHeader.EffectiveValuationDate.IsValid");
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;
			invoiceHeader.JZ_InvoiceCurrExRateType = "FIX";
			var line1 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_EnteredUnitPrice = 100m;
			line2.JI_InvoiceQuantity = 1m;
			line2.JI_EnteredUnitPrice = 200m;
			DoMerge(declaration);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.FOBInLocalCurrency.Amount, NUnit.Framework.Is.EqualTo(600m).Using(CustomComparers.TypeComparison), "FOB in local currency");
		}

		[ExpectNoExceptions]
		public void TestBusinessTaxBaseAmount_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.BusinessTaxBaseAmountInfo, "Business Tax Base", "Business Tax Base", "The base amount for business tax calculation.");
		}

		[ExpectNoExceptions]
		public void TestTotalCashTaxAmount_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.TotalCashTaxAmountInfo, "Total Tax Amount (Cash)", "Total Tax Amount (Cash)", "The total amount (Cash) of the duties, taxes, and fees.");
		}

		[ExpectNoExceptions]
		public void TestTotalNonCashTaxAmount_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.TotalNonCashTaxAmountInfo, "Total Tax Amount (Non-Cash)", "Total Tax Amount (Non-Cash)", "The total amount (Non-Cash) of the duties, taxes, and fees.");
		}

		[ExpectNoExceptions]
		public void TestCH_TotalEXPDisbursedAmountInInvoiceCurrency_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrencyInfo, "Total Inv. Amt. (16)", "The total amount of the commercial invoice.");
		}

		[ExpectNoExceptions]
		public void TestCH_TotalIMPFOBAmountInInvoiceCurrency_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrencyInfo, "FOB (17)", "The total FOB value of this entry.");
		}

		[ExpectNoExceptions]
		public void TestCH_DeclarationIncoterm_Caption()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(entryHeader.CH_DeclarationIncotermInfo, "Declaration Incoterm", "The code of the commercial terms published by the International Chamber of Commerce.");
		}

		[ExpectNoExceptions]
		public void TestRorTPFCashAmountCalculation()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var dateForDuty = new ZDateTime(2019, 9, 19);
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Japan);

			var sellRate = currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			sellRate.RE_StartDate = dateForDuty.AddDays(-5);
			sellRate.RE_ExpiryDate = dateForDuty.AddDays(5);
			sellRate.RE_SellRate = 0.2209m;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = dateForDuty;
			entryInstruction.CEI_Style = "G1";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice.JZ_InvoiceAmount = 6234000m;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 6234000m;
			invoiceLine1.JI_UseOneTenthCV = true;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 1377090.6m;
			invoiceLine1.JI_CL = entryLine1.PK;
			NUnit.Framework.Assert.That(entryHeader.RorTPFCashAmountCalculation, NUnit.Framework.Is.EqualTo(55.084m).Using(CustomComparers.TypeComparison));

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "3E";
			invoiceLine2.JI_UseOneTenthCV = false;
			invoiceLine2.JI_RAPPrice = 1170494.107m;
			invoiceLine2.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			NUnit.Framework.Assert.That(entryHeader.RorTPFCashAmountCalculation, NUnit.Framework.Is.EqualTo(158.509m).Using(CustomComparers.TypeComparison));

			invoiceLine1.JI_Procedure = "31";
			NUnit.Framework.Assert.That(entryHeader.RorTPFCashAmountCalculation, NUnit.Framework.Is.EqualTo(103.425m).Using(CustomComparers.TypeComparison));
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();
		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_InvoiceQuantity = 10m;
				invoiceLine.JI_EnteredUnitPrice = 1000m;
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

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10300m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10800m;
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => entryHeader;

		[ExpectNoExceptions]
		public void TestIEntryNumberGeneratorProviderMembers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_DateForDuty = ZDateTime.BrettsBirthday;
			entryInstruction.CEI_CustomsOffice = "A";
			entryInstruction.CEI_BoxNumber = "ABC";
			entryInstruction.CEI_Style = "G1";
			CombineAssertions(() =>
			{
				var provider = (IEntryNumberGeneratorProvider)entryHeader;
				NUnit.Framework.Assert.That(provider.EntryNumberDate, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_DateForDuty), "EntryNumberDate");
				NUnit.Framework.Assert.That(provider.EntryNumberType, NUnit.Framework.Is.EqualTo(declaration.JE_MessageType), "EntryNumberType");
				NUnit.Framework.Assert.That(provider.Company, NUnit.Framework.Is.EqualTo(declaration.Company), "Company");
				NUnit.Framework.Assert.That(provider.ShipmentType, NUnit.Framework.Is.EqualTo(declaration.JE_MessageType), "ShipmentType");
				NUnit.Framework.Assert.That(provider.EntryNumberPart1Info.Value, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_CustomsOffice).Using(CustomComparers.TypeComparison), "EntryNumberPart1Info");
				NUnit.Framework.Assert.That(provider.CustomsBrokerageBoxNumberInfo.Value, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_BoxNumber).Using(CustomComparers.TypeComparison), "CustomsBrokerageBoxNumberInfo");
				NUnit.Framework.Assert.That(provider.EntryNumberPart2Info.Value, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_Style).Using(CustomComparers.TypeComparison), "EntryNumberPart2Info");
				NUnit.Framework.Assert.That(provider.GetEntryNumberGeneratorCategory(), NUnit.Framework.Is.EqualTo(entryInstruction.GetEntryNumberGeneratorCategory()), "GetEntryNumberGeneratorCategory");
				NUnit.Framework.Assert.That(provider.EntryNumberGeneratorProviderBusinessObject, NUnit.Framework.Is.EqualTo(entryHeader).Using(CustomComparers.TypeComparison), "EntryNumberGeneratorProviderBusinessObject");
			});
		}
	}
}
