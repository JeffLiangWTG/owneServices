using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class LineNumberAssignerTest : TestCaseWithFactory
	{
		[TestDate(2018, 11, 20)]
		public void TestEntryLineNumberForChildrenLines()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2001.10.0000";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2101.11.2126";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2201.10.0000";

			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2301.10.0000";
			invoiceLine4.JI_ParentID = invoiceLine1.PK;

			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "2401.10.2120";
			invoiceLine5.JI_ParentID = invoiceLine1.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine1 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "2301100000");
			AssertEquals((short)1, entryLine1.CL_LineNumber);
			AssertEquals((short)1, entryLine1.US_ChildLineNum);

			var entryLine2 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "2401102120");
			AssertEquals((short)1, entryLine2.CL_LineNumber);
			AssertEquals((short)2, entryLine2.US_ChildLineNum);

			var invoiceLine6 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "7001.00.1000";
			invoiceLine6.US_SupTariff = "9817.00.2000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine3 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "7001001000");
			AssertEquals((short)4, entryLine3.CL_LineNumber);

			var entryLine4 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "9817002000");
			AssertEquals((short)4, entryLine4.CL_LineNumber);

			var invoiceLine7 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine7.US_SupTariff = "9903.02.21";

			var invoiceLine8 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine8.US_SupTariff = "9801.00.1010";
			invoiceLine8.JI_Tariff = "9001.10.0030";
			invoiceLine8.JI_ParentID = invoiceLine7.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine5 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99030221");
			AssertEquals((short)5, entryLine5.CL_LineNumber);

			var entryLine6 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "9001100030");
			AssertEquals((short)5, entryLine6.CL_LineNumber);

			var entryLine7 = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "9001100030");
			AssertEquals((short)5, entryLine7.CL_LineNumber);
		}

		public void TestEntryLineNumberAssignment()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine secondary1 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine secondary2 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine1.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("ParentLine", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("SecondaryLine # same as ParentLine", (short)1, secondary1.CusEntryLine.CL_LineNumber);
			AssertEquals("SecondaryLine # same as ParentLine", (short)1, secondary2.CusEntryLine.CL_LineNumber);

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, secondary1.US_SecondarySPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, secondary2.US_SecondarySPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SecondarySPI);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("X Line", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("V Line", (short)2, secondary1.CusEntryLine.CL_LineNumber);
			AssertEquals("V Line", (short)3, secondary2.CusEntryLine.CL_LineNumber);

			invoiceLine1.US_SecondarySPI = ZString.Empty;
			secondary1.JI_ParentID = ZGuid.Empty;
			secondary2.JI_ParentID = ZGuid.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("normal line 1", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("normal line 2", (short)2, secondary1.CusEntryLine.CL_LineNumber);
			AssertEquals("normal line 3", (short)3, secondary2.CusEntryLine.CL_LineNumber);

			secondary2.JI_ParentID = secondary1.PK;
			invoiceLine3.JI_ParentID = secondary2.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Normal Line", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("ParentLine", (short)2, secondary1.CusEntryLine.CL_LineNumber);
			AssertEquals("SecondaryLine", (short)2, secondary2.CusEntryLine.CL_LineNumber);
			AssertEquals("VV Line", (short)2, invoiceLine3.CusEntryLine.CL_LineNumber);
		}

		public void TestAssignSecondaryLineNumbersFor9802XAndV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "6307906800";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_SupTariff = "9802008068";
			invoiceLine2.JI_Tariff = "6307906800";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "4818200020";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals("MergedLines", 5, entry.MergedLines.Count);

			var supLine1 = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			var normalLine1 = invoiceLine.CusEntryLine;

			var supLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			var normalLine2 = invoiceLine2.CusEntryLine;

			var normalLine3 = invoiceLine3.CusEntryLine;

			AssertEquals("ChildLineNum", (short)0, supLine1.US_ChildLineNum);
			AssertEquals("ChildLineNum", (short)1, normalLine1.US_ChildLineNum);
			AssertEquals("ChildLineNum", (short)1, supLine2.US_ChildLineNum);
			AssertEquals("ChildLineNum", (short)1, normalLine2.US_ChildLineNum);
			AssertEquals("ChildLineNum", (short)2, normalLine3.US_ChildLineNum);
		}

		public void TestWatchRepairEntryChildLineNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var line = invoice.JobComInvoiceLines.AddNew();
				line.US_SupTariff = "9802004040";
				line.JI_Tariff = "0000000000";

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;
				line1.US_SupTariff = "9802004040";

				var line2 = line1.AddSecondaryInvoiceLine();
				line2.JI_Tariff = "9102111020";
				line2.JI_CustomsQuantity = 1000m;
				line2.JI_LinePrice = 1609m;
				line2.US_SupTariff = "9802004040";
				line2.US_98GoodsValue = 500m;

				var line3 = line1.AddSecondaryInvoiceLine();
				line3.JI_Tariff = "9102111030";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1345m;
				line3.US_SupTariff = "9802004040";

				var line4 = line1.AddSecondaryInvoiceLine();
				line4.JI_Tariff = "9102111040";
				line4.JI_CustomsQuantity = 1000m;
				line4.JI_LinePrice = 204m;
				line4.US_SupTariff = "9802004040";

				var line5 = invoice.JobComInvoiceLines.AddNew();

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals((short)0, line.CusEntryLine.ParentLine.US_ChildLineNum);
				AssertEquals((short)1, line.CusEntryLine.US_ChildLineNum);

				AssertEquals((short)0, line1.CusEntryLine.ParentLine.US_ChildLineNum);
				AssertEquals((short)1, line1.CusEntryLine.US_ChildLineNum);
				AssertEquals((short)2, line2.GetEntryLineFor("ENS", true).US_ChildLineNum);
				AssertEquals((short)3, line2.CusEntryLine.US_ChildLineNum);
				AssertEquals((short)4, line3.GetEntryLineFor("ENS", true).US_ChildLineNum);
				AssertEquals((short)5, line3.CusEntryLine.US_ChildLineNum);
				AssertEquals((short)6, line4.GetEntryLineFor("ENS", true).US_ChildLineNum);
				AssertEquals((short)7, line4.CusEntryLine.US_ChildLineNum);

				AssertEquals((short)0, line5.CusEntryLine.US_ChildLineNum);
			}
		}

		public void TestEntryLineNumberAssignmentForDomesticStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9802004040";
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine1.JI_CustomsQuantity = 1000m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoiceLine2.JI_Tariff = "9802004041";
			invoiceLine2.JI_CustomsQuantity = 2000m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9802004042";
			invoiceLine3.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine3.JI_CustomsQuantity = 3000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLines = declaration.ActiveEntryHeaders[0].EntryLines.ToArray();
			AssertEquals(3, entryLines.Length);
			AssertEquals((ZShort)1, entryLines.First(line => line.CustomsQuantity == 1000m).CL_LineNumber);
			AssertEquals((ZShort)2, entryLines.First(line => line.CustomsQuantity == 2000m).CL_LineNumber);
			AssertEquals((ZShort)3, entryLines.First(line => line.CustomsQuantity == 3000m).CL_LineNumber);

			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.ActiveEntryHeaders[1].EntryLines.ToArray();
			AssertEquals(3, entryLines.Length);
			AssertEquals((ZShort)1, entryLines.First(line => line.CustomsQuantity == 1000m).CL_LineNumber);
			AssertEquals((ZShort)0, entryLines.First(line => line.CustomsQuantity == 2000m).CL_LineNumber);
			AssertEquals((ZShort)2, entryLines.First(line => line.CustomsQuantity == 3000m).CL_LineNumber);
		}
	}
}
