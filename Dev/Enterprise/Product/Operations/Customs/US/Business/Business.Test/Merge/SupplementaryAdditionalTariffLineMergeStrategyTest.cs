using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SupplementaryAdditionalTariffLineMergeStrategyTest : TestCaseWithFactory
	{
		public void TestLineIsValidForMergeWithSupAdditionalTariff1() => TestLineIsValidForMergeWithSupAdditionalTariffCore<SupplementaryAdditionalTariff1LineMergeStrategy>((line, tariff) => line.SupFormattedAdditionalTariff1 = tariff);

		public void TestLineIsValidForMergeWithSupAdditionalTariff2() => TestLineIsValidForMergeWithSupAdditionalTariffCore<SupplementaryAdditionalTariff2LineMergeStrategy>((line, tariff) => line.SupFormattedAdditionalTariff2 = tariff);

		public void TestLineIsValidForMergeWithSupAdditionalTariff3() => TestLineIsValidForMergeWithSupAdditionalTariffCore<SupplementaryAdditionalTariff3LineMergeStrategy>((line, tariff) => line.SupFormattedAdditionalTariff3 = tariff);

		public void TestLineIsValidForMergeWithSupAdditionalTariff4() => TestLineIsValidForMergeWithSupAdditionalTariffCore<SupplementaryAdditionalTariff4LineMergeStrategy>((line, tariff) => line.SupFormattedAdditionalTariff4 = tariff);

		public void TestLineIsValidForMergeWithSupAdditionalTariff5() => TestLineIsValidForMergeWithSupAdditionalTariffCore<SupplementaryAdditionalTariff5LineMergeStrategy>((line, tariff) => line.SupFormattedAdditionalTariff5 = tariff);

		void TestLineIsValidForMergeWithSupAdditionalTariffCore<TMergeStrategy>(Action<JobComInvoiceLine, string> tariffSetter) where TMergeStrategy : SupplementaryTariffLineMergeStrategy
		{
			AssertNotNull(tariffSetter);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var strategy = Activator.CreateInstance(typeof(TMergeStrategy), declaration, ZString.Empty, null) as TMergeStrategy;
			Assert("Line is invalid for merge", !strategy.LineIsValidForMerge(invoiceLine));

			tariffSetter(invoiceLine, TariffViewAsCodeDescription.NotApplicableCode);
			Assert("Line is invalid for merge", !strategy.LineIsValidForMerge(invoiceLine));

			tariffSetter(invoiceLine, "9999999999");
			Assert("Line is valid for merge", strategy.LineIsValidForMerge(invoiceLine));
		}

		public void TestMergeSupAdditionalTariffsForEntrySummary()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "3920992000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(1, entryLines.Count);
			AssertEquals("Entry line for tariff 3920992000 is created", "3920992000", entryLines[0].CL_AdValoremTariff);

			invoiceLine.US_SupTariff = "99038501";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(2, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000").First());
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501").First());

			invoiceLine.SupFormattedAdditionalTariff1 = "99038502";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(3, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));

			invoiceLine.SupFormattedAdditionalTariff2 = "99038801";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(4, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));

			invoiceLine.SupFormattedAdditionalTariff3 = "99038502";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(5, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));

			invoiceLine.SupFormattedAdditionalTariff4 = "99038815";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(6, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038815 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038815"));

			invoiceLine.SupFormattedAdditionalTariff5 = "99038816";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals(7, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038815 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038815"));
			AssertNotNull("Entry line for tariff 99038816 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038816"));
		}

		public void TestMergeSupAdditionalTariffsForCargoRelease()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "3920992000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(1, entryLines.Count);
			AssertEquals("Entry line for tariff 3920992000 is created", "3920992000", entryLines[0].CL_AdValoremTariff);

			invoiceLine.US_SupTariff = "99038501";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(2, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));

			invoiceLine.SupFormattedAdditionalTariff1 = "99038502";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(3, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));

			invoiceLine.SupFormattedAdditionalTariff2 = "99038801";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(4, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));

			invoiceLine.SupFormattedAdditionalTariff3 = "99038502";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(5, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));

			invoiceLine.SupFormattedAdditionalTariff4 = "99038815";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(6, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038815 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038815"));

			invoiceLine.SupFormattedAdditionalTariff5 = "99038816";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
			AssertEquals(7, entryLines.Count);
			AssertNotNull("Entry line for tariff 3920992000 is created", entryLines.Find(x => x.CL_AdValoremTariff == "3920992000"));
			AssertNotNull("Entry line for tariff 99038501 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038501"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038801 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038801"));
			AssertNotNull("Entry line for tariff 99038502 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038502"));
			AssertNotNull("Entry line for tariff 99038815 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038815"));
			AssertNotNull("Entry line for tariff 99038816 is created", entryLines.Find(x => x.CL_AdValoremTariff == "99038816"));
		}
	}
}
