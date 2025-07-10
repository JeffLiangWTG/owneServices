using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InformalFeeCalculatorTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestExemptForCourierFacility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "8888";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("informal fee applicable", 2m, entryHeader.InformalFee);

			declaration.US_SchDEntry = "4670";//courier facility for UPS
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee not applicable to a courier facility", 0m, entryHeader.InformalFee);

			declaration.US_SchDEntry = "2895";//courier facility for FED EXP
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee not applicable to a courier facility", 0m, entryHeader.InformalFee);
		}

		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestInformalFeeEntryType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("informal fee not applicable to 01 entry type", 0m, entryHeader.InformalFee);

			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee applicable to 11 entry type", 2m, entryHeader.InformalFee);
		}

		public void TestInformalFeeDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_ITDate = new ZDateTime(2017, 12, 31);
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 1, 31);
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("informal fee applicable to 11 entry type", 2.05m, entryHeader.InformalFee);
		}

		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestInformalFeeForIsraelOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Israel;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("informal fee not applicable to Israel Origin", 0m, entryHeader.InformalFee);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Ireland;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee applicable if there is a line from other than IL", 2m, entryHeader.InformalFee);
		}

		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestInformalFeeForLeastDevelopedCountry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cambodia;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("informal fee not applicable to LDDC Origin", 0m, entryHeader.InformalFee);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Ireland;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee applicable if there is a line from other than LDDC", 2m, entryHeader.InformalFee);

			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Israel;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee not applicable as both lines are eligible for exemption", 0m, entryHeader.InformalFee);
		}

		[NUnit.Framework.TestDate(2017, 12, 1)]
		public void TestInformalFeeForReturnedUSGoods()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("9801001035", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "9801001035";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			}
			tariff.UE_ISOCountryofOriginEditCode = "US";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9801.00.10 35";
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("informal fee not applicable to US Origin line", 0m, entryHeader.InformalFee);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3506.99.00 00";
			AssertNotNull(invoiceLine2.ImportTariff);

			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			invoiceLine2.US_SPI = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("informal fee applicable as not all lines are eligible ", 2m, entryHeader.InformalFee);
		}

		public void TestInformalFeeFor9802006000()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9802006000";
			AssertNotNull(invoiceLine.ImportTariff);

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "3506.99.00 00";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var helper = new FeeCalculationHelper(Factory, declaration.DateForFeeCalculation);
			AssertEquals(helper.InformalFeeAmount, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
