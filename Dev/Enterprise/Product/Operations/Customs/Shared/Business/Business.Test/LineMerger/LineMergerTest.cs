using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class LineMergerEntryLineNumberingTest : TestCaseWithFactory
	{
		public void TestSavingHighestLineNumberDoesNotHappenOnMerge()
		{
			merger.DoMerge();
			AssertEquals("Highest line number", (ZShort)0, entryHeader.CH_HighestLineNumber);
		}

		public void TestMergeLogs()
		{
			merger.DoMerge();
			AssertEquals("Entry lines merged", entryHeader.Declaration.Logs.MostRecentLog.SL_Reference);
		}

		public void TestEntryLineNumberedInOrderOfInvoiceLines()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = testDec.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = (short)2;
			line2.JI_LineNo = (short)1;

			AssertEquals("Line no for Line1", (short)2, line1.JI_LineNo);
			AssertEquals("Line no for Line2", (short)1, line2.JI_LineNo);

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("Entry line no matches invoice line no", (short)2, line1.CusEntryLine.CL_LineNumber);
			AssertEquals("Entry line no matches invoice line no", (short)1, line2.CusEntryLine.CL_LineNumber);
		}

		public void TestLineNumbersDoNotGetChangedByLaterMergesAfterSubmissionToCustoms()
		{
			entryHeader.EntryNumber = "B123";
			entryHeader.CH_HighestLineNumber = (short)1;
			invoice1.JobComInvoiceLines.Remove(line1);
			var line2 = invoice1.JobComInvoiceLines.AddNew();//a new invoice line is added
			line2.JI_Tariff = "2.2.2";
			invoice1.JobComInvoiceLines.Add(line1); // change ordering

			merger.DoMerge();
			AssertEquals("Entry Lines count", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line numbers are maintained between merges", (short)1, line1.CusEntryLine.CL_LineNumber);
			AssertEquals("New line gets next line number", (short)2, line2.CusEntryLine.CL_LineNumber);
		}

		public void TestLineNumbersGetResetIfDoesNotSupportAmendmentsAndNotSumittedToCustoms()
		{
			merger.SupportsAmendments = false;
			CheckLineNumberOfNewLineAfterDelete(2);
		}

		public void TestLineNumbersGetResetIfDoesNotSupportAmendmentsAndSumittedToCustoms()
		{
			entryHeader.EntryNumber = "B123";
			merger.SupportsAmendments = false;
			CheckLineNumberOfNewLineAfterDelete(2);
		}

		public void TestLineNumbersDoGetReusedWhenLineIsDeletedBeforeSendingMessageToCustoms()
		{
			CheckLineNumberOfNewLineAfterDelete(2);
		}

		public virtual void TestLineNumbersDoNotGetReusedWhenLineIsDeletedAfterSendingMessageToCustoms()
		{
			entryHeader.EntryNumber = "B123";
			entryHeader.CH_Status = GetClearStatus();
			CheckLineNumberOfNewLineAfterDelete(3);
		}

		protected virtual string GetClearStatus() => Common.Shared.EntryStatusList.Codes.Clear;

		protected void CheckLineNumberOfNewLineAfterDelete(short correctLineNumber)
		{
			var line2 = testDec.FilteredInvoiceLines.AddNew();
			line2.JI_Tariff = "2.2.2";
			merger.DoMerge();
			line1.Delete();
			merger.DoMerge();

			var line3 = testDec.FilteredInvoiceLines.AddNew();
			line3.JI_Tariff = "3.3.3";
			merger.DoMerge();

			AssertEquals("Entry Lines count", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of 'third' line (line 1 has been deleted)", correctLineNumber, line3.CusEntryLine.CL_LineNumber);
		}

		//TODO: check correct behaviour for this situation with Cate/Brett (nick)
		//		public void TestLineNumbersDoNotGetReusedWhenLineIsDeletedAndEntryIsSavedAndReloadedAfterSubmissionToCustoms()
		//		{
		//			EntryHeader.EntryNumber = "B123";
		//			CheckLineNumberOfNewLineAfterDeleteSaveAndReload(2);
		//		}

		public void TestChangingHeaderKeyRecyclesHeaderWithSave()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			var initialGuid = testDec.CustomsEntryHeaders[0].PK;
			testDec.JE_ExportDate = new ZDateTime(2005, 11, 1);
			merger.DoMerge();
			AssertEquals("Entry Header has changed - should have been recycled", initialGuid, testDec.CustomsEntryHeaders[0].PK);
		}

		public void TestChangingHeaderKeyRecyclesHeaderWithoutSave()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var initialGuid = testDec.CustomsEntryHeaders[0].PK;
			testDec.JE_ExportDate = new ZDateTime(2005, 11, 1);
			merger.DoMerge();
			AssertEquals("Entry Header has changed - should have been recycled", initialGuid, testDec.CustomsEntryHeaders[0].PK);
		}

		public void TestChangingLineKeyRecyclesLineWithSave()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			var initialGuid = testDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			testDec.FilteredInvoiceLines[0].JI_Tariff = "12.34.56";
			merger.DoMerge();
			AssertEquals("Entry Line has changed - should have been recycled", initialGuid, testDec.CustomsEntryHeaders[0].MergedLines[0].PK);
		}

		public void TestChangingLineKeyRecyclesLineWithSave_Advanced()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var line2 = testDec.Invoices[0].JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "11.22.33";
			merger.DoMerge();
			Factory.Save();
			var initialGuid = testDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			var nextGuid = testDec.CustomsEntryHeaders[0].MergedLines[1].PK;
			testDec.FilteredInvoiceLines[0].JI_Tariff = "12.34.56";
			testDec.FilteredInvoiceLines[1].JI_Tariff = "10.10.10";
			merger.DoMerge();
			AssertEquals("Entry Line has changed - should have been recycled", initialGuid, testDec.CustomsEntryHeaders[0].MergedLines[0].PK);
			AssertEquals("Entry Line has changed - should have been recycled", nextGuid, testDec.CustomsEntryHeaders[0].MergedLines[1].PK);
		}

		//TODO Joo weird testing
		[ExpectNoExceptions]
		public void TestDeletedLinesAreNotRecycled()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var initialGuid = testDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			testDec.Invoices[0].JobComInvoiceLines.Remove(initialGuid);
			merger.DoMerge();
			foreach (CusEntryHeader entryHeader in testDec.CustomsEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					AssertEquals("Deleted Line should not appear in any entryline", true, entryLine.RandomLine.PK != initialGuid);
				}
			}
		}

		public void TestMergeRecycleBasher()
		{
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var line2 = testDec.Invoices[0].JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "11.22.33";
			merger.DoMerge();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var theSameTestDec = factory2.Load<BaseJobDeclaration>(testDec.PK);
			theSameTestDec.Invoices[0].JobComInvoiceLines[0].JI_Tariff = "10.10.10";
			var newMerger = new LineMerger(theSameTestDec);
			var initialGuid = theSameTestDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			newMerger.DoMerge();
			AssertEquals("Line should have been recycled", initialGuid, theSameTestDec.CustomsEntryHeaders[0].MergedLines[0].PK);
		}

		public void TestLineNumbersGetReusedWhenLineIsDeletedAndEntryIsSavedAndReloadedBeforeSubmissionToCustoms()
		{
			CheckLineNumberOfNewLineAfterDeleteSaveAndReload(1);
		}

		public void TestExistingLinesGetRenumberedBeforeSubmissionToCustoms()
		{
			Create2MoreLinesDeleteExistingMergeAndCheckNumbers(1);
		}

		public virtual void TestExistingLinesKeepNumbersAfterSubmissionToCustoms()
		{
			entryHeader.EntryNumber = "B123";
			entryHeader.CH_Status = GetClearStatus();
			Create2MoreLinesDeleteExistingMergeAndCheckNumbers(2);
		}

		protected void Create2MoreLinesDeleteExistingMergeAndCheckNumbers(short startNumber)
		{
			var line2 = testDec.FilteredInvoiceLines.AddNew();
			line2.JI_Tariff = "2.2.2";

			var line3 = testDec.FilteredInvoiceLines.AddNew();
			line3.JI_Tariff = "3.3.3";

			merger.DoMerge();
			AssertEquals("Entry Lines count", 3, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			line1.Delete();
			merger.DoMerge();
			AssertEquals("Entry Lines count", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of Line2", startNumber, testDec.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			AssertEquals("Line number of Line3", startNumber + 1, testDec.CustomsEntryHeaders[0].MergedLines[1].CL_LineNumber);
		}

		protected void CheckLineNumberOfNewLineAfterDeleteSaveAndReload(short correctLineNumber)
		{
			line1.Delete();
			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadDeclaration = reloadFactory.Load<BaseJobDeclaration>(testDec.PK);

			var line2 = reloadDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2.2.2";

			GetNewLineMerger(reloadDeclaration).DoMerge();
			AssertEquals("Entry Lines count", 1, reloadDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of second line (line 1 has been deleted)", correctLineNumber, reloadDeclaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
		}

		protected virtual LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			invoice1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1.1.1";
			merger = GetNewLineMerger(testDec);

			merger.DoMerge();

			AssertEquals("Entry Lines count", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Line number of first line", (short)1, testDec.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			entryHeader = testDec.CustomsEntryHeaders[0];
		}

		LineMerger merger;
		protected BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice1;
		BaseJobComInvoiceLine line1;
		CusEntryHeader entryHeader;
	}

	public abstract class LineMergerTest : TestCaseWithFactory
	{
		public void TestDutyCalculatorStrategyType()
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			var getNewDutyCalculatorStrategyMethod = merger.GetType().GetMethod("GetNewDutyCalculatorStrategy", BindingFlags.Instance | BindingFlags.NonPublic);
			var strategy = getNewDutyCalculatorStrategyMethod.Invoke(merger, Array.Empty<object>());
			AssertEquals("DutyCalculatorStrategyType", ExpectedDutyCalculatorStrategyType, strategy.GetType());
		}

		protected virtual Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		public void TestGetEntryCreationStrategies()
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			var getEntryCreationStrategiesMethod = merger.GetType().GetMethod("GetEntryCreationStrategies", BindingFlags.Instance | BindingFlags.NonPublic);
			var strategies = getEntryCreationStrategiesMethod.Invoke(merger, Array.Empty<object>()) as EntryCreationStrategy[];
			AssertContainsExactElementsInExactOrder("EntryCreationStrategiesType", ExpectedEntryCreationStrategiesType, strategies.Select(x => x.GetType()));
		}

		protected virtual Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		public void TestLandedCostOnlyConfigurationProviderType()
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			var getNewDutyCalculatorStrategyMethod = merger.GetType().GetMethod("GetLandedCostOnlyConfigurationProvider", BindingFlags.Instance | BindingFlags.NonPublic);
			var strategy = getNewDutyCalculatorStrategyMethod.Invoke(merger, Array.Empty<object>());
			AssertEquals("LandedCostOnlyConfigurationProviderType", ExpectedLandedCostOnlyConfigurationProviderType, strategy.GetType());
		}

		protected virtual Type ExpectedLandedCostOnlyConfigurationProviderType => typeof(LandedCostOnlyConfigurationProviderBase);

		public void TestMergeResumeApportionmentWhenDirty()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration, 1);

			TestJobDeclaration.ApportionmentDirty = true;
			AssertEquals("Apportionment is dirty", true, TestJobDeclaration.ApportionmentDirty);

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();
			AssertEquals("Apportionment is not dirty", false, TestJobDeclaration.ApportionmentDirty);
		}

		public void TestLineMergerBuildInvoiceLinesForEntryLinesAfterSettingJI_CL()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("One Customs Entry Header created", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("one entry line", 1, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Three invoice lines for one entry line", 3, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("EntryHeader.InvoiceLines", 3, new List<BaseJobComInvoiceLine>(TestJobDeclaration.CustomsEntryHeaders[0].InvoiceLines).Count);
		}

		public virtual void TestLineMergerCreateOneEntryHeader()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice1 = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			var invoice2 = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			SetCountrySpecificValueToTheKeyForHeadersToGenerateEntryHeaderPerInvoice(TestJobDeclaration);
			SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(invoice1);
			SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(invoice2);

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("One cus entry header", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Six cus entry lines", 6, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has 'No merge' option which will add invoiceLinePK as KeyForLine merge in shared. Asycuda's KeyForLine is not based on JE_MergeBy.
		/// An invoice header that has three invoice lines that have the same part numbers
		/// Expected result : One cus entry header and three entry lines
		/// </summary>
		public void TestDontMerge()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(invoice);

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("One cus entry header", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Three cus entry lines", 3, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		/// <summary>
		/// A declaration has one invoice header which has three invoice lines.
		/// Those lines have different part numbers
		/// Expected Result : One cus entry header and three cus entry lines
		/// </summary>
		public void TestMergeByPartNumWithDifferentPartNumbers()
		{
			if (ApplyPartNumberToKeyForLineForMerge)
			{
				TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
				var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
				var line1 = invoice.JobComInvoiceLines[0];
				var line2 = invoice.JobComInvoiceLines[1];
				var line3 = invoice.JobComInvoiceLines[2];

				SetSpecificValueToInvoiceLines(line1, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part1");
				SetSpecificValueToInvoiceLines(line2, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part2");
				SetSpecificValueToInvoiceLines(line3, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part3");

				var merger = GetNewLineMerger(TestJobDeclaration);
				merger.DoMerge();
				AssertEquals("One Customs Entry Header created", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
				AssertEquals("Three Lines for the header", 3,
					TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ApplyPartNumberToKeyForLineForMerge => true;

		/// <summary>
		/// A declaration has one invoice header which has three invoice lines.
		/// Those lines have the same part number
		/// Expected Result : One cus entry header and one cus entry lines
		/// </summary>
		public void TestMergByPartNumWithSamePartNumber()
		{
			if (ApplyPartNumberToKeyForLineForMerge)
			{
				TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
				CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);

				var merger = GetNewLineMerger(TestJobDeclaration);
				merger.DoMerge();

				AssertEquals("One Customs Entry Header created", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
				AssertEquals("One Line for the header", 1, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
			else
			{
				Assert(true);
			}
		}

		/// <summary>
		/// A declaration has one invoice header that has three invoice lines
		/// Three lines have the same lookup code, but different part numbers.
		/// Mergeby Lookup Codes should result in one merged line
		/// </summary>
		public void TestMergeByLookupCodeWithSameLookupCodesButWithDifferentPartNums()
		{
			if (ApplyPartNumberToKeyForLineForMerge && ApplyClassificationToKeyForLineForMerge)
			{
				TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
				var line1 = invoice.JobComInvoiceLines[0];
				var line2 = invoice.JobComInvoiceLines[1];
				var line3 = invoice.JobComInvoiceLines[2];

				SetSpecificValueToInvoiceLines(line1, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part1");
				SetSpecificValueToInvoiceLines(line2, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part2");
				SetSpecificValueToInvoiceLines(line3, BaseJobComInvoiceLine.Schema.JI_PartNo, "Part3");

				AssertEquals("Precondition:Classficiation is the same", line1.JI_CC, line2.JI_CC);
				AssertEquals("Precondition:Classficiation is the same", line2.JI_CC, line3.JI_CC);

				var merger = GetNewLineMerger(TestJobDeclaration);
				merger.DoMerge();
				AssertEquals("One Customs Entry Header created", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
				AssertEquals("One line for the header", 1, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
			else
			{
				Assert(true);
			}
		}

		/// <summary>
		/// A declaration has one invoice header that has three invoice lines
		/// Three lines have the different lookup code.
		/// Mergeby Lookup Codes should result in three merged lines
		/// </summary>
		public void TestMergeByLookupCodeWithDifferentLookupCodes()
		{
			if (ApplyClassificationToKeyForLineForMerge)
			{
				TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
				var line1 = invoice.JobComInvoiceLines[0];
				var line2 = invoice.JobComInvoiceLines[1];
				var line3 = invoice.JobComInvoiceLines[2];

				SetClassificationToInvoiceLine(line1);
				SetClassificationToInvoiceLine(line2);
				SetClassificationToInvoiceLine(line3);

				var merger = GetNewLineMerger(TestJobDeclaration);
				merger.DoMerge();

				AssertEquals("One Customs Entry Header created", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
				AssertEquals("Three Lines for the header", 3,
					TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ApplyClassificationToKeyForLineForMerge => true;

		/// <summary>
		/// All invoice lines are to be merged into one cus entry line.
		/// </summary>
		public void TestJI_CLSetWhenEntryLineIdentified()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			var line1 = invoice.JobComInvoiceLines[0];
			var line2 = invoice.JobComInvoiceLines[1];
			var line3 = invoice.JobComInvoiceLines[2];

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("Customs Entry Header", 1, TestJobDeclaration.CustomsEntryHeaders.Count);
			AssertEquals("Merged Line Count", 1, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryLineGuid = TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			AssertEquals("Invoice Line1", entryLineGuid, line1.JI_CL);
			AssertEquals("Invoice Line2", entryLineGuid, line2.JI_CL);
			AssertEquals("Invoice Line3", entryLineGuid, line3.JI_CL);
		}

		public void TestReMergeKeepsOriginalPKOnHeaderAndLine()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			var headerPK = TestJobDeclaration.CustomsEntryHeaders[0].PK;
			var linePK = TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PK;

			merger.DoMerge();

			Assert("Header PK differs after second merge - should be same", headerPK == TestJobDeclaration.CustomsEntryHeaders[0].PK);
			Assert("Line PK differs after second merge - should be same", linePK == TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PK);
		}

		public void TestThrowAwayMergeDeletesReferenceInInvoiceLine()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			var line1 = invoice.JobComInvoiceLines[0];

			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			var entryLineGuid = TestJobDeclaration.CustomsEntryHeaders[0].MergedLines[0].PK;
			AssertEquals("PreCondition:Invoice Line has a reference to entry line", entryLineGuid, line1.JI_CL);

			TestJobDeclaration.ThrowAwayMerge();
			AssertEquals("Reference to CusEntryLine has been deleted", ZGuid.Empty, line1.JI_CL);
		}

		public void TestDisposeOrMarkDeletePendingBeforeEntryIsLodgedSuccessfully()
		{
			var declaration = TestJobDeclaration;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = CreateInvoiceAndInvoiceLineForMerge(declaration, 2);
			SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(invoice);
			var line1 = invoice.JobComInvoiceLines[0];
			var line2 = invoice.JobComInvoiceLines[1];

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("there should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("There should be two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine1 = line1.CusEntryLine;
			var entryLine2 = line2.CusEntryLine;

			entryHeader.CH_HighestLineNumber = 0;
			line1.Delete();
			merger.DoMerge();
			AssertEquals("EntryLine1 should have been deleted", true, entryLine1.IsDeleted);
		}

		public void TestRecalculateBillCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "Master Bill";

			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("there should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("there should be one bill for header", 1, declaration.CustomsEntryHeaders[0].Bills.Count);
			AssertEquals("There should be two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine1 = line1.CusEntryLine;
			var entryLine2 = line2.CusEntryLine;

			entryHeader.CH_HighestLineNumber = 0;
			line1.Delete();
			merger.DoMerge();
			AssertEquals("EntryLine1 should have been deleted", true, entryLine1.IsDeleted);
		}

		public void TestDisposeOrMarkDeletedPendingAfterEntryIsLodged()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("there should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("There should be two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine1 = line1.CusEntryLine;
			var entryLine2 = line2.CusEntryLine;

			entryHeader.CH_HighestLineNumber = 2;
			line1.Delete();
			merger.DoMerge();

			if (declaration.ShouldKeepDeletedLinesOnAmendment)
			{
				AssertEquals("EntryLine1 should not be deleted", false, entryLine1.IsDeleted);
				AssertEquals("EntryLine1 status should be changed", EntryLineStatusList.Codes.DeletePending, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("AllEntryLines has EntryLine1 and EntryLine2", true, entryHeader.AllEntryLines.Contains(entryLine1));
				AssertEquals("AllEntryLines has EntryLine1 and EntryLine2", true, entryHeader.AllEntryLines.Contains(entryLine2));
				AssertEquals("MergedLine has EntryLine2", true, entryHeader.MergedLines.Contains(entryLine2));
				AssertEquals("MergedLine does not have EntryLine1", false, entryHeader.MergedLines.Contains(entryLine1));

				AssertEquals("PendingDeletionLines has EntryLine1", true, entryHeader.PendingDeletionEntryLines.Contains(entryLine1));
				AssertEquals("PendingDeletionLines does not have EntryLine2", false, entryHeader.PendingDeletionEntryLines.Contains(entryLine2));

				var line3 = invoice.JobComInvoiceLines.AddNew();
				merger.DoMerge();

				var entryLine3 = line3.CusEntryLine;
				AssertEquals("AllEntryLines has EntryLine3", true, entryHeader.AllEntryLines.Contains(entryLine3));
				AssertEquals("MergedLines has EntryLine3", true, entryHeader.MergedLines.Contains(entryLine3));
				AssertEquals("PendingDeletion should not have EntryLine3", false, entryHeader.PendingDeletionEntryLines.Contains(entryLine3));
			}
			else
			{
				AssertEquals("EntryLine1 should have been deleted", true, entryLine1.IsDeleted);
			}
		}

		public void TestSetValuesAfterInvoiceLinesAreRebuilt()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "InvoiceLineDescription";
			invoiceLine.JI_Tariff = "0000.00.00 00";

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("CL_Description should be set", "InvoiceLineDescription", entryLine.CL_Description);
			AssertEquals("Tariff should be set", invoiceLine.JI_Tariff, entryLine.CL_AdValoremTariff);
		}

		[ExpectNoExceptions]
		public void TestOnMergedIsCalled()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var mockMerger = new Mock<LineMerger>(new object[] { testDec }) { CallBase = true };
			var merger = mockMerger.Object;
			mockMerger.Protected().Setup("OnMerged");
			merger.DoMerge();
			mockMerger.VerifyAll();
		}

		public void TestReMergeKeepsLineNumbersAfterSubmissionToCustoms()
		{
			TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = CreateInvoiceAndInvoiceLineForMerge(TestJobDeclaration);
			SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(invoice);
			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			var entryHeader = TestJobDeclaration.CustomsEntryHeaders[0];
			CreateCurrentEntrySnapShot();
			SetEntryHeaderToHasBeenLodgedAtCustoms(entryHeader);

			AssertEquals("EntryHeader.MergedLines.Count", 3, entryHeader.MergedLines.Count);
			AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", new ZInt(2), entryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[2].CL_LineNumber", new ZInt(3), entryHeader.MergedLines[2].CL_LineNumber);

			if (AllowDeleteEntryLineForRegistedEntry)
			{
				var line2 = invoice.InvoiceLines[1];
				line2.Delete();
				merger.DoMerge();

				entryHeader = TestJobDeclaration.CustomsEntryHeaders[0];
				AssertEquals("EntryHeader.MergedLines.Count", 2, entryHeader.MergedLines.Count);
				AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);

				AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", TestJobDeclaration.ShouldKeepDeletedLinesOnAmendment || !entryHeader.ShouldCompletelyReassignNumbers ? 3 : 2, entryHeader.MergedLines[1].CL_LineNumber);
			}
		}

		protected virtual void CreateCurrentEntrySnapShot() { }

		protected virtual bool AllowDeleteEntryLineForRegistedEntry => true;

		void SetEntryHeaderToHasBeenLodgedAtCustoms(CusEntryHeader entryHeader)
		{
			entryHeader.EntryNumber = "ABC";
			entryHeader.CH_EntryStatus = entryHeader.CH_Status = GetClearStatus();
		}

		protected virtual string GetClearStatus() => Common.Shared.EntryStatusList.Codes.Clear;

		protected virtual string DefaultMessageTypeForTesting => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override void SetUp()
		{
			base.SetUp();

			TestJobDeclaration = GetJobDeclaration();

			TestJobDeclaration.JE_MessageType = DefaultMessageTypeForTesting;
			aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TEST";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "00000000";
			classification.CC_Description = "TESTDESCRIPTION";
			classification.CC_RN_NKCountryCode = TestJobDeclaration.CountryCode;

			part = MasterFiles.Business.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "PartNum";

			BaseCusClassPartPivot partClassPivot = Factory.New<BaseCusClassPartPivot>();
			partClassPivot.CI_OP = part.PK;
			partClassPivot.CI_CC = classification.PK;
			Factory.Save();
		}

		protected ZGuid GetValidSupplier()
		{
			var result = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			return result.PK;
		}

		BaseJobComInvoiceHeader CreateInvoiceAndInvoiceLineForMerge(BaseJobDeclaration declaration, int invoicelineNumber = 3)
		{
			var invoice = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_OH_Supplier = GetValidSupplier();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			for (int i = 0; i < invoicelineNumber; i++)
			{
				invoice.JobComInvoiceLines.AddNew();
			}
			SetDefaultValuesToInvoiceLines(invoice);

			if (!declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "1";

				foreach (BaseJobComInvoiceLine invoiceLine in invoice.InvoiceLines)
				{
					invoiceLine.JI_CEI = instruction.PK;
				}
			}

			return invoice;
		}

		protected virtual void SetCountrySpecificValueToTheKeyForLinesToGenerateEntryLinePerInvoiceLine(BaseJobComInvoiceHeader header)
		{
		}

		protected virtual void SetCountrySpecificValueToTheKeyForHeadersToGenerateEntryHeaderPerInvoice(BaseJobDeclaration declaration)
		{
			if (!declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0];
				foreach (BaseJobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.JI_CEI = instruction.PK;
				}
			}
		}

		protected void SetDefaultValuesToInvoiceLines(BaseJobComInvoiceHeader header)
		{
			foreach (BaseJobComInvoiceLine line in header.JobComInvoiceLines)
			{
				line.JI_PartNo = part.OP_PartNum;
				line.JI_CustomsQuantity = 1.0m;
				line.JI_LinePrice = 100.0m;
			}
		}

		protected void SetSpecificValueToInvoiceLines(BaseJobComInvoiceLine line, string lineColumn, object lineValue)
		{
			line[lineColumn] = lineValue;
		}

		protected void SetClassificationToInvoiceLine(BaseJobComInvoiceLine line)
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_LookupCode = line.PK.ToString().Substring(0, 10);
			classification.CC_Description = "Description";
			line.JI_CC = classification.PK;
		}

		protected abstract LineMerger GetNewLineMerger(BaseJobDeclaration declaration);

		protected abstract BaseJobDeclaration GetJobDeclaration();

		public BaseJobDeclaration TestJobDeclaration;
		protected RefCurrency aUDCurrency;
		BaseCusClassification classification;
		MasterFiles.Business.OrgSupplierPart part;
	}

	sealed class LineMergerBaseOnlyTest : LineMergerTest
	{
		public void TestInvoiceAmountAndCurrency()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(0m, entryLine.CL_InvoiceAmount);
			AssertEquals("", entryLine.CL_RX_NKInvoiceAmountCurrency);
		}

		public void TestReMergeReAssignsLineNumbersWhenReAssignLineNumbersCompletelyEveryMergeIsOverridden()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);
			mockDeclaration.Setup(m => m.IsCustomsLineAmendmentATotalReplacement).Returns(true);

			var testJobDeclaration = mockDeclaration.Object;
			testJobDeclaration.Invoices.AddNew();
			testJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var header1 = testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_OH_Supplier = GetValidSupplier();
			header1.JZ_IncoTerm = "FOB";
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			var line1 = header1.JobComInvoiceLines.AddNew();
			var line2 = header1.JobComInvoiceLines.AddNew();
			var line3 = header1.JobComInvoiceLines.AddNew();
			SetDefaultValuesToInvoiceLines(header1);

			var merger = new LineMerger(testJobDeclaration);
			merger.DoMerge();

			var entryHeader = testJobDeclaration.CustomsEntryHeaders[0];
			entryHeader.CH_HighestLineNumber = 3;
			AssertEquals("EntryHeader.MergedLines.Count", 3, entryHeader.MergedLines.Count);
			AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", new ZInt(2), entryHeader.MergedLines[1].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[2].CL_LineNumber", new ZInt(3), entryHeader.MergedLines[2].CL_LineNumber);

			line2.Delete();
			merger.DoMerge();

			entryHeader = testJobDeclaration.CustomsEntryHeaders[0];
			AssertEquals("EntryHeader.MergedLines.Count", 2, entryHeader.MergedLines.Count);
			AssertEquals("EntryHeader.MergedLines[0].CL_LineNumber", new ZInt(1), entryHeader.MergedLines[0].CL_LineNumber);
			AssertEquals("EntryHeader.MergedLines[1].CL_LineNumber", new ZInt(2), entryHeader.MergedLines[1].CL_LineNumber);
		}

		public void TestMergeConsideringMaximumEntryLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.Invoices.AddNew();

			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew().JI_Description = "DoNotCountMeForMaximumEntryLine";

			var merger = new TestLineMerger(declaration);
			merger.DoMerge();
			AssertEquals("There should be one entry header", 1, declaration.CustomsEntryHeaders.Count);

			var newLine = declaration.InvoiceLines.AddNew();
			merger.DoMerge();

			AssertEquals("There should be two entry headers now", 2, declaration.CustomsEntryHeaders.Count);

			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();

			merger.DoMerge();
			AssertEquals("There should be three entry headers now", 3, declaration.CustomsEntryHeaders.Count);

			declaration.InvoiceLines.RemoveAndDeleteAll();
			declaration.InvoiceLines.AddNew();
			merger.DoMerge();
			AssertEquals("There should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
		}

		public void TestMergeConsideringMaximumEntryLinesWhenMergingByTariff()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.Invoices.AddNew();

			declaration.InvoiceLines.AddNew().JI_Tariff = "0000000000";
			declaration.InvoiceLines.AddNew().JI_Tariff = "0000000000";
			declaration.InvoiceLines.AddNew().JI_Tariff = "0000000001";
			declaration.InvoiceLines.AddNew().JI_Tariff = "0000000002";
			declaration.InvoiceLines[3].JI_Description = "DoNotCountMeForMaximumEntryLine";

			var merger = new TestLineMerger(declaration);
			merger.DoMerge();
			AssertEquals("There should be one entry header", 1, declaration.CustomsEntryHeaders.Count);

			var newLine = declaration.InvoiceLines.AddNew();
			newLine.JI_Tariff = "0000000000";
			merger.DoMerge();
			AssertEquals("There should be one entry header", 1, declaration.CustomsEntryHeaders.Count);

			newLine.JI_Tariff = "0000000003";
			merger.DoMerge();
			AssertEquals("There should be one entry header", 2, declaration.CustomsEntryHeaders.Count);

			declaration.InvoiceLines.RemoveAndDeleteAll();
			declaration.InvoiceLines.AddNew();
			merger.DoMerge();
			AssertEquals("There should be one entry header", 1, declaration.CustomsEntryHeaders.Count);
		}

		class TestLineMerger : LineMerger
		{
			public TestLineMerger(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override EntryCreationStrategy[] GetEntryCreationStrategies() => new EntryCreationStrategy[] { new TestStrategy(this.Declaration) };
		}

		class TestStrategy : EntryCreationStrategy
		{
			public TestStrategy(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected internal override int MaxLinesBeforeNewEntry => 2;

			protected internal override bool ShouldIncrementNumberOfEntryLinesForMaximumEntryLines(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_Description != "DoNotCountMeForMaximumEntryLine";

			protected internal override bool CreateANewEntryIfMaximumEntryLineExceeded => true;
		}

		public void TestRebuildingMergedLinesDontHappenForDeactivatedEntries()
		{
			var declaration = TestJobDeclaration;

			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockEntry.Setup(m => m.IsActive).Returns(false);
			var entry = mockEntry.Object;

			var mockMergedLines = new Mock<CusEntryLineCollection<CusEntryLine>>(entry) { CallBase = true };
			var mergedLines = mockMergedLines.Object;
			mockEntry
				.Protected()
				.Setup<ICusEntryLineCollection<CusEntryLine>>("GetMergedLineCollection")
				.Returns(mergedLines);

			var entryLine = entry.MergedLines.AddNew();

			declaration.CustomsEntryHeaders.Add(entry);
			declaration.ActiveEntryHeaders.Rebuild();
			AssertEquals("entry is not active", false, declaration.ActiveEntryHeaders.Contains(entry));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			mockMergedLines
				.Protected()
				.Setup("RebuildCore");

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			mockMergedLines.VerifyAll();
		}

		public void TestDeactivadedEntriesWontGetRecalculatedTotalAmountPayable()
		{
			var declaration = TestJobDeclaration;

			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockEntry.Setup(m => m.IsActive).Returns(false);
			mockEntry.Setup(m => m.TotalAmountPayable).Returns(new ZDecimal(2000m));

			var entry = mockEntry.Object;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.CH_TotalPaid = 1000m;

			var entryLine = entry.MergedLines.AddNew();

			declaration.ActiveEntryHeaders.Rebuild();
			AssertEquals("entry is not active", false, declaration.ActiveEntryHeaders.Contains(entry));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("deactivated entry.CH_TotalPaid is retained", 1000m, entry.CH_TotalPaid);
		}

		public void TestDeactivatedEntriesWontGetRecycled()
		{
			var declaration = TestJobDeclaration;

			var entry = Factory.New<CusEntryHeaderForTesting>();
			entry.IsActiveReturns = false;

			declaration.CustomsEntryHeaders.Add(entry);
			var entryLine = entry.MergedLines.AddNew();

			declaration.ActiveEntryHeaders.Rebuild();
			AssertEquals("entry is not active", false, declaration.ActiveEntryHeaders.Contains(entry));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("There should be two entries in CustomsEntryHeaders", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("entry should not be in ActiveEntryHeaders", false, declaration.ActiveEntryHeaders.Contains(entry));

			var entryCreated = declaration.ActiveEntryHeaders[0];
			AssertNotEquals(entryCreated, entry);
		}

		public void TestClearReferenceToEntryLineWhenLineIsNotValidForMerge()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var mergeManager = new Mock<MergeManager>(declaration) { CallBase = true };
			declaration.GetMergeManagerReturns = mergeManager.Object;

			var lineMerger = new Mock<LineMerger>(declaration) { CallBase = true };
			mergeManager
				.Protected()
				.Setup<LineMerger>("GetNewLineMergerCore")
				.Returns(lineMerger.Object);

			var strategyA = new TestPartialCreationStrategy(declaration, "A");
			lineMerger
				.Protected()
				.Setup<EntryCreationStrategy[]>("GetEntryCreationStrategies")
				.Returns(new EntryCreationStrategy[] { strategyA });

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotNull("invoiceLine's CusEntryLine", invoiceLine.CusEntryLine);

			invoiceLine.JI_Description = "DONOTMERGENOW FOR STATEGY A";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNull("JI_CL should have been cleared if not valid", invoiceLine.CusEntryLine);
		}

		public void TestMergeWithMultiStrategies()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var mergeManager = new Mock<MergeManager>(declaration) { CallBase = true };
			declaration.GetMergeManagerReturns = mergeManager.Object;

			var lineMerger = new Mock<LineMerger>(declaration) { CallBase = true };
			mergeManager
				.Protected()
				.Setup<LineMerger>("GetNewLineMergerCore")
				.Returns(lineMerger.Object);

			var strategyA = new TestPartialCreationStrategy(declaration, "A");
			var strategyB = new StrategyUsingAdditionalLink(declaration, "B");
			lineMerger
				.Protected()
				.Setup<EntryCreationStrategy[]>("GetEntryCreationStrategies")
				.Returns(new EntryCreationStrategy[] { strategyB, strategyA });

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_Description = "DONOTMERGENOW FOR STATEGY A";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("two entries created", 2, declaration.CustomsEntryHeaders.Count);
			var existingEntryLineByStategyA = invoiceLine2.CusEntryLine;
			AssertNull("PreCondition:invoiceLine1'CusEntryLine", invoiceLine1.CusEntryLine);

			invoiceLine1.JI_Description = "";//when merged through strategyA again, it should have been sorted again with strategyA and invoiceLine2 should have kept the existing entry line

			strategyA = new TestPartialCreationStrategy(declaration, "A");
			strategyB = new StrategyUsingAdditionalLink(declaration, "B");

			lineMerger
				.Protected()
				.Setup<EntryCreationStrategy[]>("GetEntryCreationStrategies")
				.Returns(new EntryCreationStrategy[] { strategyB, strategyA });

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("invoice line2's entry line is still existingEntryLineByStategyA", existingEntryLineByStategyA, invoiceLine2.CusEntryLine);
			AssertNotNull(invoiceLine1.CusEntryLine);
		}

		public void TestPopulateTotalAmountPayableForCusEntryHeaders()
		{
			var jobDec = Factory.New<BaseJobDeclaration>();

			var entryHeader = jobDec.CustomsEntryHeaders.AddNew();
			var line = entryHeader.AllEntryLines.AddNew();
			var fee = line.Fees.AddNew();
			fee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			fee.CF_ChargeAmount = 50m;
			AssertEquals(0m, entryHeader.CH_TotalPaid);

			line.Fees.RemoveAndDeleteAll();
			var confirmedFee = line.ConfirmedFees.AddNew();
			confirmedFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			confirmedFee.CF_ChargeAmount = 50m;
			new LineMerger(jobDec).PopulateTotalAmountPayableForCusEntryHeaders();
			AssertEquals(entryHeader.TotalAmountPayable, entryHeader.CH_TotalPaid);
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<BaseJobDeclaration>();

		protected override LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger(declaration);

		sealed class TestPartialCreationStrategy : EntryCreationStrategy
		{
			public TestPartialCreationStrategy(BaseJobDeclaration declaration, ZString messageType)
				: base(declaration, messageType)
			{
			}

			public override bool LineIsValidForMerge(BaseJobComInvoiceLine baseInvoiceLine) => baseInvoiceLine.JI_Description != "DONOTMERGENOW FOR STATEGY A";

			protected internal override bool IsEntryHeaderValidToBeReused(CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine) => entry.CH_MessageType == CH_MessageTypeToNewEntryHeader;

			protected internal override IEnumerable<CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					if (entry.CH_MessageType == CH_MessageTypeToNewEntryHeader)
					{
						yield return entry;
					}
				}
			}
		}

		sealed class StrategyUsingAdditionalLink : EntryCreationStrategy
		{
			public StrategyUsingAdditionalLink(BaseJobDeclaration declaration, ZString messageType)
				: base(declaration, messageType)
			{
			}

			protected internal override CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
			{
				var result = invoiceLine.AdditionalEntryLineLinks.GetEntryLineFor(CH_MessageTypeToNewEntryHeader);
				return result.Any() ? result.ElementAt(0) : null;
			}

			protected internal override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
				=> invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);

			protected internal override bool IsEntryHeaderValidToBeReused(CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine) => entry.CH_MessageType == CH_MessageTypeToNewEntryHeader;

			protected internal override IEnumerable<CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					if (entry.CH_MessageType == CH_MessageTypeToNewEntryHeader)
					{
						yield return entry;
					}
				}
			}
		}

		sealed class CusEntryHeaderForTesting : CusEntryHeader
		{
			public CusEntryHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsActiveReturns { get; set; }

			public override bool IsActive => IsActiveReturns;
		}

		sealed class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public MergeManager GetMergeManagerReturns { get; set; }

			protected override MergeManager GetMergeManager() => GetMergeManagerReturns;
		}
	}
}
