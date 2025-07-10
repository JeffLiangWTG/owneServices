using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryLineParentLineSetterTest : TestCaseWithFactory
	{
		public void TestSettingParentLinePKFor9802XAndV()
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

			var supLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

			var normalLine3 = invoiceLine3.CusEntryLine;

			AssertEquals("supLine1.ChildLines", 3, supLine1.ChildLines.Count);

			var secondaryLines = ((MessageBuilders.ICusEntryLine)supLine1).SecondaryTariffLines;
			AssertEquals(1, secondaryLines.Count());

			AssertEquals("supLine2.ChildLines", 1, supLine2.ChildLines.Count);
			secondaryLines = ((MessageBuilders.ICusEntryLine)supLine2).SecondaryTariffLines;
			AssertEquals(1, secondaryLines.Count());

			AssertEquals("normalline3 should point to supline1", supLine1, normalLine3.ParentLine);
		}

		public void TestSettingParentLinePKForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6307906800";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6307906800";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "4818200020";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine.ParentLine);
			AssertEquals(invoiceLine.CusEntryLine, invoiceLine3.CusEntryLine.ParentLine);

			Assert(invoiceLine.CusEntryLine.ChildLines.Contains(invoiceLine2.CusEntryLine));
			Assert(invoiceLine.CusEntryLine.ChildLines.Contains(invoiceLine3.CusEntryLine));
		}

		public void TestSettingParentLinePKForCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MergeBy = "TRF";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8407.90.9060";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8407.90.9060";
			invoiceLine2.US_SupTariff = "9802.00.5030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var aceCargoReleaseEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, false);
			var supLine = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			var supLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, true);
			var aceCargoReleaseEntryLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, false);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, aceCargoReleaseEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, supLine.CL_LineNumber);
			AssertEquals((short)2, supLine2.CL_LineNumber);
			AssertEquals((short)2, aceCargoReleaseEntryLine2.CL_LineNumber);

			Assert(supLine.ChildSecondaryEntryLines.Contains(invoiceLine2.CusEntryLine));
			Assert(supLine2.ChildSecondaryEntryLines.Contains(aceCargoReleaseEntryLine2));
		}

		public void TestSettingParentLinePKForCargoReleaseForParentChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MergeBy = "TRF";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var aceCargoReleaseEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, false);
			var entrySummaryEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);

			var aceCargoReleaseEntryLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, false);
			var entrySummaryEntryLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);

			AssertEquals(entrySummaryEntryLine, entrySummaryEntryLine2.ParentLine);
			AssertEquals(aceCargoReleaseEntryLine, aceCargoReleaseEntryLine2.ParentLine);
		}

		public void TestSettingParentLinePKFor9802XAndV_2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6307906800";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6307906800";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.US_SupTariff = "9802008068";
			invoiceLine3.JI_Tariff = "4818200020";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals("MergedLines", 4, entry.MergedLines.Count);

			var normalLine1 = invoiceLine.CusEntryLine;

			var supLine3 = invoiceLine3.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			var normalLine3 = invoiceLine3.CusEntryLine;

			AssertEquals("normalLine1.ChildLines", 2, normalLine1.ChildLines.Count);

			var secondaryLines = ((MessageBuilders.ICusEntryLine)supLine3).SecondaryTariffLines;
			AssertEquals(1, secondaryLines.Count());

			AssertEquals("normalline3 should point to supline1", supLine3, normalLine3.ParentLine);
		}

		public void TestSettingParentLinePKForCombindLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_Tariff = ZString.Empty;
			parentInvoiceLine.US_SupTariff = "99030120";
			parentInvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = parentInvoiceLine.PK;
			childLine.JI_Tariff = "3920992000";
			childLine.US_SupTariff = "99038501";
			childLine.SupFormattedAdditionalTariff1 = "99038802";
			childLine.SupFormattedAdditionalTariff2 = "99030120";
			childLine.SupFormattedAdditionalTariff3 = "99038815";
			childLine.SupFormattedAdditionalTariff4 = "99038821";
			childLine.SupFormattedAdditionalTariff5 = "99038816";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("MergedLines", 9, entry.MergedLines.Count);
			var entryLine99030120 = parentInvoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals(8, entryLine99030120.ChildLines.Count);
		}

		public void TestNoExceptionThrownWhenAdditionalSupTariffSetInsteadOfProvTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.SupFormattedAdditionalTariff1 = "99030120";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNoExceptionThrown(() =>
			{
				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				AssertEquals("MergedLines", 2, entry.MergedLines.Count);
				var entryLine99030120 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030120").FirstOrDefault();
				AssertEquals(ZGuid.Empty, entryLine99030120.US_CL_ParentLine);
				AssertEquals(1, entryLine99030120.ChildLines.Count);
			});
		}

		public void TestClearParentLienOnSupTariffLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3920992000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine3920992000 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "3920992000").FirstOrDefault();
			AssertEquals(ZGuid.Empty, entryLine3920992000.US_CL_ParentLine);
			AssertEquals(0, entryLine3920992000.ChildLines.Count);

			entryLine3920992000.US_CL_ParentLine = ZGuid.NewZGuid();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZGuid.Empty, entryLine3920992000.US_CL_ParentLine);
			AssertEquals(0, entryLine3920992000.ChildLines.Count);

			invoiceLine.US_SupTariff = "99038501";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine99038501 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038501").FirstOrDefault();
			AssertEquals(ZGuid.Empty, entryLine99038501.US_CL_ParentLine);
			AssertEquals(1, entryLine99038501.ChildLines.Count);

			entryLine99038501.US_CL_ParentLine = ZGuid.NewZGuid();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine99038501 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038501").FirstOrDefault();
			AssertEquals(ZGuid.Empty, entryLine99038501.US_CL_ParentLine);
			AssertEquals(1, entryLine99038501.ChildLines.Count);

			invoiceLine.SupFormattedAdditionalTariff1 = "99038802";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine99038802 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038802").FirstOrDefault();
			AssertEquals(ZGuid.Empty, entryLine99038802.US_CL_ParentLine);
			AssertEquals(2, entryLine99038802.ChildLines.Count);

			entryLine99038802.US_CL_ParentLine = entryLine99038501.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine99038802 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038802").FirstOrDefault();
			AssertEquals(ZGuid.Empty, entryLine99038802.US_CL_ParentLine);
			AssertEquals(2, entryLine99038802.ChildLines.Count);
		}
	}
}
