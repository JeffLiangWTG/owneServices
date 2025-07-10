using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntrySummaryEntryLineComparerForNumberingTest : TestCaseWithFactory
	{
		public void TestXAndVLinesNoLongerNeedToComeFirst_ButXLinesShouldStillPreceedVLines()
		{
			var (normalLine, normalLine2, xLine, vLine) = SetupData();

			AssertEquals("Normal line should come first, because JI_LineNo = 1", (short)1, normalLine.CusEntryLine.CL_LineNumber);
			AssertEquals("Then normal line2 due to JI_LineNo", (short)2, normalLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("xLine should come first", (short)3, xLine.CusEntryLine.CL_LineNumber);
			AssertEquals("vLine should follow xLine, because xLine is the vLine's parent", (short)4, vLine.CusEntryLine.CL_LineNumber);
		}

		public void TestIrreflexivity()
		{
			var (normalLine, normalLine2, xLine, vLine) = SetupData();
			var comparer = new EntrySummaryEntryLineComparerForNumbering();

			AssertEquals("normalLine", 0, comparer.Compare(normalLine.CusEntryLine, normalLine.CusEntryLine));
			AssertEquals("normalLine2", 0, comparer.Compare(normalLine2.CusEntryLine, normalLine2.CusEntryLine));
			AssertEquals("xLine", 0, comparer.Compare(xLine.CusEntryLine, xLine.CusEntryLine));
			AssertEquals("vLine", 0, comparer.Compare(vLine.CusEntryLine, vLine.CusEntryLine));
		}

		public void TestAsymmetry()
		{
			var (normalLine, normalLine2, xLine, vLine) = SetupData();
			var comparer = new EntrySummaryEntryLineComparerForNumbering();

			CombineAssertions("normalLine < vLine => vLine > normalLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, vLine.CusEntryLine), 0);
				AssertGreaterThan("normalLines has lower JI_LineNo", comparer.Compare(vLine.CusEntryLine, normalLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine < normalLine2 => normalLine2 > normalLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, normalLine2.CusEntryLine), 0);
				AssertGreaterThan("normalLine has lower JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, normalLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine < xLine => xLine > normalLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, xLine.CusEntryLine), 0);
				AssertGreaterThan("normalLine has lower JI_LineNo", comparer.Compare(xLine.CusEntryLine, normalLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine2 < vLine => vLine > normalLine2", () =>
			{
				AssertLessThan("vLine's parent (xLine) has a higher JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, vLine.CusEntryLine), 0);
				AssertGreaterThan("vLine's parent (xLine) has a higher JI_LineNo", comparer.Compare(vLine.CusEntryLine, normalLine2.CusEntryLine), 0);
			});

			CombineAssertions("normalLine2 < xLine => xLine > normalLine2", () =>
			{
				AssertLessThan("normalLine2 has lower JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, xLine.CusEntryLine), 0);
				AssertGreaterThan("normalLine2 has lower JI_LineNo", comparer.Compare(xLine.CusEntryLine, normalLine2.CusEntryLine), 0);
			});

			CombineAssertions("xLine < vLine => vLine > xLine", () =>
			{
				AssertLessThan("xLine is parent of vLine, so should come before it", comparer.Compare(xLine.CusEntryLine, vLine.CusEntryLine), 0);
				AssertGreaterThan("xLine is parent of vLine, so should come before it", comparer.Compare(vLine.CusEntryLine, xLine.CusEntryLine), 0);
			});
		}

		public void TestTransitivity()
		{
			var (normalLine, normalLine2, xLine, vLine) = SetupData();
			var comparer = new EntrySummaryEntryLineComparerForNumbering();

			CombineAssertions("normalLine < normalLine2 & normalLine2 < xLine => normalLine < xLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, normalLine2.CusEntryLine), 0);
				AssertLessThan("normalLine2 has lower JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, xLine.CusEntryLine), 0);
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, xLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine < normalLine2 & normalLine2 < vLine => normalLine < vLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, normalLine2.CusEntryLine), 0);
				AssertLessThan("vLine's parent (xLine) has a higher JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, vLine.CusEntryLine), 0);
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, vLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine < xLine & xLine < vLine => normalLine < vLine", () =>
			{
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, xLine.CusEntryLine), 0);
				AssertLessThan("xLine is parent of vLine, so should come before it", comparer.Compare(xLine.CusEntryLine, vLine.CusEntryLine), 0);
				AssertLessThan("normalLine has a lower JI_LineNo", comparer.Compare(normalLine.CusEntryLine, vLine.CusEntryLine), 0);
			});

			CombineAssertions("normalLine2 < xLine & xLine < vLine => normalLine2 < vLine", () =>
			{
				AssertLessThan("normalLine2 has lower JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, xLine.CusEntryLine), 0);
				AssertLessThan("xLine is parent of vLine, so should come before it", comparer.Compare(xLine.CusEntryLine, vLine.CusEntryLine), 0);
				AssertLessThan("vLine's parent (xLine) has a higher JI_LineNo", comparer.Compare(normalLine2.CusEntryLine, vLine.CusEntryLine), 0);
			});
		}

		public void TestLineNumberingFor99XAndV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var line = invoice.JobComInvoiceLines.AddNew();
				line.US_SupTariff = "9822.05.01";
				line.JI_Tariff = "6205202031";
				line.US_SecondarySPI = "X";

				var vLine = line.AddSecondaryInvoiceLine();
				vLine.US_SupTariff = "9822.05.01";
				vLine.JI_Tariff = "6205202031";
				vLine.US_SecondarySPI = "V";

				var vLine2 = line.AddSecondaryInvoiceLine();
				vLine2.US_SupTariff = "9822.05.01";
				vLine2.JI_Tariff = "6215200000";
				vLine2.US_SecondarySPI = "V";

				var normalLine = invoice.JobComInvoiceLines.AddNew();
				normalLine.JI_Tariff = "6205202031";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("LineNumber", (ZShort)1, line.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)2, vLine.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)3, vLine2.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)4, normalLine.CusEntryLine.CL_LineNumber);
			}
		}

		public void TestLineNumberingFor99XAndV2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var normalLine = invoice.JobComInvoiceLines.AddNew();
				normalLine.JI_Tariff = "6205202031";

				var line = invoice.JobComInvoiceLines.AddNew();
				line.US_SupTariff = "9822.05.01";
				line.JI_Tariff = "6205202031";
				line.US_SecondarySPI = "X";

				var vLine = line.AddSecondaryInvoiceLine();
				vLine.US_SupTariff = "9822.05.01";
				vLine.JI_Tariff = "6205202031";
				vLine.US_SecondarySPI = "V";

				var vLine2 = line.AddSecondaryInvoiceLine();
				vLine2.US_SupTariff = "9822.05.01";
				vLine2.JI_Tariff = "6215200000";
				vLine2.US_SecondarySPI = "V";

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("LineNumber", (ZShort)1, normalLine.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)2, line.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)3, vLine.CusEntryLine.CL_LineNumber);
				AssertEquals("LineNumber", (ZShort)4, vLine2.CusEntryLine.CL_LineNumber);
			}
		}

		public void TestSortNormalLinesThenChangeFirstLineToVAndSecondLineToX()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";

			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0712311000";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("PreCondition:EntryLine.CL_LineNumber", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("PreCondition:EntryLine.CL_LineNumber", (short)2, invoiceLine2.CusEntryLine.CL_LineNumber);

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1.JI_ParentID = invoiceLine2.PK;

			declaration.DoMerge();
			AssertEquals("PreCondition:EntryLine.CL_LineNumber", (short)2, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("PreCondition:EntryLine.CL_LineNumber", (short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
		}

		public void TestSortXAndVLinesBeforeNumbering()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";

			JobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "INV3";

			//X and V line
			JobComInvoiceLine invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1_1.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_ParentID = ZGuid.Empty;//normal line in-between
			invoiceLine2_1.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine3_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3_1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3_1.JI_Tariff = "0712311000";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_1.PK, invoiceLine3_1.JI_ParentID);

			//another set of X and V and normal line with the same tariff with X line sits in-between
			JobComInvoiceLine invoiceLine1_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1_2.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine2_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2_2.JI_Tariff = "0712311000";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_2.PK, invoiceLine2_2.JI_ParentID);

			JobComInvoiceLine invoiceLine3_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3_2.JI_ParentID = ZGuid.Empty;//normal line
			invoiceLine3_2.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine4_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4_2.JI_Tariff = "2002908020";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_2.PK, invoiceLine4_2.JI_ParentID);

			//normal secondary tariffs where parent and children gets mapped to different CusEntryLine's, but share the same entry line number
			JobComInvoiceLine invoiceLine1_3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine1_3.JI_Tariff = "1902194000";//normal

			JobComInvoiceLine invoiceLine2_3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine2_3.JI_Tariff = "0712311000";//normal
			invoiceLine2_3.JI_ParentID = invoiceLine1_3.PK;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			//currently X and V lines and secondary tariff lines dont get merged with other invoice lines
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);

			//X and V lines should be in sequence
			AssertEquals(invoiceLine3_1.CusEntryLine.CL_LineNumber, invoiceLine1_1.CusEntryLine.CL_LineNumber + 1);

			AssertEquals(invoiceLine2_2.CusEntryLine.CL_LineNumber, invoiceLine1_2.CusEntryLine.CL_LineNumber + 1);
			AssertEquals(invoiceLine4_2.CusEntryLine.CL_LineNumber, invoiceLine2_2.CusEntryLine.CL_LineNumber + 1);
		}

		public void TestSortXAndVLinesBeforeNumberingWhenNoInvoiceNumbers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();

			//X and V line
			JobComInvoiceLine invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1_1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1_1.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2_1.JI_ParentID = ZGuid.Empty;//normal line in-between
			invoiceLine2_1.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine3_1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine3_1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3_1.JI_Tariff = "0712311000";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_1.PK, invoiceLine3_1.JI_ParentID);

			//another set of X and V and normal line with the same tariff with X line sits in-between
			JobComInvoiceLine invoiceLine1_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1_2.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine2_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2_2.JI_Tariff = "0712311000";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_2.PK, invoiceLine2_2.JI_ParentID);

			JobComInvoiceLine invoiceLine3_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3_2.JI_ParentID = ZGuid.Empty;//normal line
			invoiceLine3_2.JI_Tariff = "1902194000";

			JobComInvoiceLine invoiceLine4_2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4_2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4_2.JI_Tariff = "2002908020";
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1_2.PK, invoiceLine4_2.JI_ParentID);

			//normal secondary tariffs where parent and children gets mapped to different CusEntryLine's, but share the same entry line number
			JobComInvoiceLine invoiceLine1_3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine1_3.JI_Tariff = "1902194000";//normal

			JobComInvoiceLine invoiceLine2_3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine2_3.JI_Tariff = "0712311000";//normal
			invoiceLine2_3.JI_ParentID = invoiceLine1_3.PK;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			//currently X and V lines and secondary tariff lines dont get merged with other invoice lines
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);

			//X and V lines should be in sequence
			AssertEquals(invoiceLine3_1.CusEntryLine.CL_LineNumber, invoiceLine1_1.CusEntryLine.CL_LineNumber + 1);

			AssertEquals(invoiceLine2_2.CusEntryLine.CL_LineNumber, invoiceLine1_2.CusEntryLine.CL_LineNumber + 1);
			AssertEquals(invoiceLine4_2.CusEntryLine.CL_LineNumber, invoiceLine2_2.CusEntryLine.CL_LineNumber + 1);
		}

		public void TestLineComparerChecksInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1_1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2_1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3_1 = invoice1.JobComInvoiceLines.AddNew();

			JobComInvoiceLine invoiceLine1_2 = invoice2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2_2 = invoice2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3_2 = invoice2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4_2 = invoice2.JobComInvoiceLines.AddNew();

			JobComInvoiceLine invoiceLine1_3 = invoice3.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2_3 = invoice3.JobComInvoiceLines.AddNew();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("lines merging for US should include being based on Invoices", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestOrderedChildLinesForInvoiceLineHasMultipleSupplementaryTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_FormattedTariff = "8424.20.1000";
			invoiceLine.SupTariffFormatted = "9903.88.03";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.16";
			invoiceLine.SupFormattedAdditionalTariff3 = "9903.88.25";
			invoiceLine.SupFormattedAdditionalTariff4 = "9903.01.23";
			invoiceLine.SupFormattedAdditionalTariff5 = "9903.88.28";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var parentEntryLine = invoiceLine.GetEntryLineFor("ENS", true, (x) => x.US_SupAdditionalLine);
			AssertEquals("99030120", parentEntryLine.CL_AdValoremTariff);
			AssertEquals(6, parentEntryLine.ChildLines.Count);
			AssertEquals("99038816", parentEntryLine.ChildLines[0].CL_AdValoremTariff);
			AssertEquals("99038825", parentEntryLine.ChildLines[1].CL_AdValoremTariff);
			AssertEquals("99030123", parentEntryLine.ChildLines[2].CL_AdValoremTariff);
			AssertEquals("99038828", parentEntryLine.ChildLines[3].CL_AdValoremTariff);
			AssertEquals("99038803", parentEntryLine.ChildLines[4].CL_AdValoremTariff);
			AssertEquals("8424201000", parentEntryLine.ChildLines[5].CL_AdValoremTariff);

			invoiceLine.SupFormattedAdditionalTariff3 = ZString.Empty;
			invoiceLine.SupFormattedAdditionalTariff4 = ZString.Empty;
			invoiceLine.SupFormattedAdditionalTariff5 = ZString.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			parentEntryLine = invoiceLine.GetEntryLineFor("ENS", true, (x) => x.US_SupAdditionalLine);
			AssertEquals("99030120", parentEntryLine.CL_AdValoremTariff);
			AssertEquals(3, parentEntryLine.ChildLines.Count);
			AssertEquals("99038816", parentEntryLine.ChildLines[0].CL_AdValoremTariff);
			AssertEquals("99038803", parentEntryLine.ChildLines[1].CL_AdValoremTariff);
			AssertEquals("8424201000", parentEntryLine.ChildLines[2].CL_AdValoremTariff);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		(JobComInvoiceLine normalLine, JobComInvoiceLine normalLine2, JobComInvoiceLine xLine, JobComInvoiceLine vLine) SetupData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine normalLine = invoice.JobComInvoiceLines.AddNew();
			normalLine.JI_Tariff = "1";
			JobComInvoiceLine vLine = invoice.JobComInvoiceLines.AddNew();
			vLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			JobComInvoiceLine normalLine2 = invoice.JobComInvoiceLines.AddNew();
			normalLine2.JI_Tariff = "2";

			JobComInvoiceLine xLine = invoice.JobComInvoiceLines.AddNew();
			xLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			vLine.JI_ParentID = xLine.PK;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			return (normalLine, normalLine2, xLine, vLine);
		}
	}
}
