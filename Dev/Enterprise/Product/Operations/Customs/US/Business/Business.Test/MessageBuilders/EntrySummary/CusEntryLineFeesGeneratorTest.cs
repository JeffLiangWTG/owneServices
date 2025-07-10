using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class CusEntryLineFeesGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2009, 6, 1)]
		public void TestMPFLessThanZeroPointZeroOne()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1.74m;

			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.50m;
			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.50m;
			invoiceLine2.AddSecondaryInvoiceLine().JI_LinePrice = 0.15m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine invoiceLine2Loaded = factory2.Load<JobComInvoiceLine>(invoiceLine2.PK);
			ICusEntryLine entryLine = invoiceLine2Loaded.CusEntryLine;

			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);
			AssertEquals(0m, fees[0].Amount);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);
		}

		public void TestCusEntryLineFeeDeletedForRoundedEmptyMPF()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 0.59m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine invoiceLine2Loaded = factory2.Load<JobComInvoiceLine>(invoiceLine2.PK);
			ICusEntryLine entryLine = invoiceLine2Loaded.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);
			AssertEquals(0m, fees[0].Amount);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);
		}

		[TestDate(2009, 6, 1)]
		public void TestGetFees()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_LinePrice = 2000.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals("PreCondition", invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine);

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);
			AssertEquals(25.20m, fees[0].Amount);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesWhenAmountIsEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "0804.40.0010";//AVOCADOS - Required

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.JI_Tariff = "2008.80.0000";//016/017 - Not required and Excise Fee
			FeeCusCodeData distilled = invoiceLine2.FeeCusCodes.AddNew();
			distilled.CY_IsOverridden = true;
			distilled.CY_FeeAmount = 20m;
			distilled.CY_Code = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(2, fees.Count);

			bool hasSeenMPF = false, hasSeenAvocadoFee = false;
			foreach (IFee fee in fees)
			{
				hasSeenMPF |= fee.Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
				hasSeenAvocadoFee |= fee.Code == Core.Constants.USCustoms.FeeCodes.Avocado;
			}

			Assert("MPF is there", hasSeenMPF);
			Assert("Avocado fee is there", hasSeenAvocadoFee);

			entryLine = invoiceLine2.CusEntryLine;
			fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);//Excise fee should be excluded from Fees
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);

			AssertEquals("But aggregated into the total tax", 20m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesForCottonFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "6117.80.9510";//Cotton Fee - Flagged as Required

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			//Cotton fee has a threshold amount and due to that, Customs does not expect to see an empty 62
			//See a CMR job, B00151469 
			AssertEquals(1, fees.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesForCottonFeeWithXVSets()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "6117.80.9510";//Cotton Fee - Flagged as Required
			invoiceLine.US_SetInd = SetIndicatorList.Codes.X;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.JI_Tariff = "6117.80.9510";//Cotton Fee - Flagged as Required
			invoiceLine2.US_SetInd = SetIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);
			AssertEquals("For X lines, no fees should be mandatory except MPF and HMF", Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);

			ICusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			List<IFee> fees2 = new List<IFee>(entryLine2.Fees);
			AssertEquals("All other fees except for MPF and HMF are calculated at V lines ", Core.Constants.USCustoms.FeeCodes.Cotton, fees2[0].Code);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesForCottonFeeForACE()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "6117.80.9510";//Cotton Fee - Flagged as Required

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(2, fees.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, fees[0].Code);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Cotton, fees[1].Code);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesWhenAmountIsThere()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "0804.40.0010";
			invoiceLine.JI_CustomsQuantity = 5000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(2, fees.Count);
			bool hasSeenMPF = false, hasSeenAvocadoFee = false;
			foreach (IFee fee in fees)
			{
				hasSeenMPF |= fee.Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
				hasSeenAvocadoFee |= fee.Code == Core.Constants.USCustoms.FeeCodes.Avocado;
			}
			Assert("MPF is there", hasSeenMPF);
			Assert("Avocado fee is there", hasSeenAvocadoFee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
