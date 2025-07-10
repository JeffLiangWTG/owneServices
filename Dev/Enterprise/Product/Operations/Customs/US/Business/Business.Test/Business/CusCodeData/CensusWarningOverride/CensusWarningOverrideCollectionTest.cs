using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CensusWarningOverrideCollection))]
	sealed class CensusWarningOverrideCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CensusWarningOverride>
	{
		public void TestEntryLineLookupsExcludedSupTariffLine()
		{
			var testHelper = new Chapter98HelperTest();

			var job = Factory.NewWithValidTestData<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			job.US_EntryFilerCode = "XJ5";
			job.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			job.US_EnableENS = true;
			job.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = job.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "test1";
			invoice.JZ_InvoiceAmount = 10000m;
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "8215.20.0000";
			parentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			var childLine1 = invoice.JobComInvoiceLines.AddNew();
			childLine1.JI_ParentID = parentLine.PK;
			childLine1.JI_Tariff = "8215.99.3500";
			childLine1.JI_LinePrice = 2000;
			childLine1.US_SupTariff = ZString.Empty;

			job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			List<EntryCensusWarningOverride> list = new List<EntryCensusWarningOverride>();
			var entryHeader = job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("3 entryLines", 3, entryHeader.MergedLines.Count);
			AssertEquals("one is SupTariffLine", 1, entryHeader.MergedLines.OfType<CusEntryLine>().Count(x => x.IsSupTariffLine()));

			var coll = new EntryCensusWarningOverrideCollection(entryHeader);
			var censusWarningOverride = new EntryCensusWarningOverride(entryHeader, coll);
			AssertEquals("SupTariffLine Excluded.", 2, censusWarningOverride.Lookups.EntryLines.Count);
		}

		public void TestCopyFrom()
		{
			List<EntryCensusWarningOverride> list = new List<EntryCensusWarningOverride>();
			var coll = new EntryCensusWarningOverrideCollection(InvoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry);
			var one = new EntryCensusWarningOverride(InvoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, coll);
			one.ConditionCode = CensusWarningCodeList.Codes.ChargesDividedByValue;
			one.OverrideCode = CensusOverrideCodeList.Codes._05;
			list.Add(one);
			var two = new EntryCensusWarningOverride(InvoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, coll);
			two.ConditionCode = CensusWarningCodeList.Codes.GrossWeightAir;
			two.OverrideCode = CensusOverrideCodeList.Codes._06;
			list.Add(two);
			InvoiceLine.CensusWarningOverrides.CopyFrom(list);
			AssertEquals(2, InvoiceLine.CensusWarningOverrides.Count);
			AssertEquals(CensusWarningCodeList.Codes.ChargesDividedByValue, InvoiceLine.CensusWarningOverrides[0].CY_Code);
			AssertEquals(CensusOverrideCodeList.Codes._05, InvoiceLine.CensusWarningOverrides[0].CY_Data);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightAir, InvoiceLine.CensusWarningOverrides[1].CY_Code);
			AssertEquals(CensusOverrideCodeList.Codes._06, InvoiceLine.CensusWarningOverrides[1].CY_Data);
			list.Remove(one);
			two.OverrideCode = CensusOverrideCodeList.Codes._07;
			InvoiceLine.CensusWarningOverrides.CopyFrom(list);
			AssertEquals(1, InvoiceLine.CensusWarningOverrides.Count);
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightAir, InvoiceLine.CensusWarningOverrides[0].CY_Code);
			AssertEquals(CensusOverrideCodeList.Codes._07, InvoiceLine.CensusWarningOverrides[0].CY_Data);
		}

		protected override Customs.Business.CusCodeDataCollection<CensusWarningOverride> GetCusCodeDataCollection() => InvoiceLine.CensusWarningOverrides;

		protected override Type GetExpectedCollectionType() => typeof(CensusWarningOverrideCollection);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CensusWarningOverride>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = "CL";
			return result;
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.ImportEntryNumber = "ENT3242";
					declaration.Invoices.AddNew();
					invoiceLine = declaration.InvoiceLines.AddNew();
					declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				}

				return invoiceLine;
			}
		}
	}
}
