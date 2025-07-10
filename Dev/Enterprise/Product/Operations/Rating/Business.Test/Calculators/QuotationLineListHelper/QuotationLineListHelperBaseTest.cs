using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.Rating.Business
{
	abstract class QuotationLineListHelperBaseTest : TestCaseWithFactory
	{
		#region Consolidated Lines

		public void TestConsolidatedLines_DifferentOrigin()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_OriginLRC = value,
				value1: "USCHI",
				value2: "IDJKT",
				expectedOutput: TestConsolidatedLines_DifferentOrigin_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentOrigin_ExpectedOutput { get; }

		public void TestConsolidatedLines_DifferentDestination()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_DestinationLRC = value,
				value1: "USCHI",
				value2: "IDJKT",
				expectedOutput: TestConsolidatedLines_DifferentDestination_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentDestination_ExpectedOutput { get; }

		public void TestConsolidatedLines_DifferentCommodity()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_RH_NKCommodityCode = value,
				value1: "HAZ",
				value2: "ALUM",
				expectedOutput: TestConsolidatedLines_DifferentCommodity_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentCommodity_ExpectedOutput { get; }

		public void TestConsolidatedLines_DifferentCarrierServiceLevel()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_PL_NKCarrierServiceLevel = value,
				value1: "AM",
				value2: "XX",
				expectedOutput: TestConsolidatedLines_DifferentCarrierServiceLevel_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentCarrierServiceLevel_ExpectedOutput { get; }

		public void TestConsolidatedLines_DifferentServiceLevel()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_RS_NKServiceLevel_NI = value,
				value1: "AM",
				value2: "XX",
				expectedOutput: TestConsolidatedLines_DifferentServiceLevel_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentServiceLevel_ExpectedOutput { get; }

		public void TestConsolidatedLines_DifferentTransportProvider()
			=> TestConsolidatedLines
			(
				setValue: (rateEntry, value) => rateEntry.TI_OH_TransportProvider = value,
				value1: TransportProvider1.PK,
				value2: TransportProvider2.PK,
				expectedOutput: TestConsolidatedLines_DifferentTransportProvider_ExpectedOutput
			);

		protected abstract string TestConsolidatedLines_DifferentTransportProvider_ExpectedOutput { get; }

		public void TestConsolidatedLines<T>(Action<RateEntry, T> setValue, T value1, T value2, string expectedOutput)
		{
			var rateEntryLCL = RatingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, "SEA", "AUSYD", "NZAKL", "FRT", 10);
			rateEntryLCL.TI_RH_NKCommodityCode = "";

			var rateEntryORG1 = RatingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "SEA", "AUSYD", "NZAKL", "ODOC", 20, commodity: "");
			setValue(rateEntryORG1, value1);

			var rateEntryORG2 = RatingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "SEA", "AUSYD", "NZAKL", "ODOC", 21, commodity: "");
			setValue(rateEntryORG2, value2);

			var set = new PricingPageRateLineList();
			set.Add(rateEntryLCL.RateLines[0]);
			set.Add(rateEntryORG1.RateLines[0]);
			set.Add(rateEntryORG2.RateLines[0]);

			var actual = Render(set);

			AssertContainsExactLinesInExactOrder("", expectedOutput, actual);
		}

		#endregion

		protected abstract RatingHeader RatingHeader { get; }

		#region Implementation

		string Render(PricingPageRateLineList set)
		{
			var list = ListHelper.GetLines(set, false);
			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (var line in list)
			{
				builder.AppendLine(line.ToString());
			}

			return builder.ToString();
		}

		protected override void SetUp()
		{
			base.SetUp();

			TransportProvider1 = TestHelper.NewOrgHeader();
			TransportProvider2 = TestHelper.NewOrgHeader();

			Factory.Save();
		}

		OrgHeader TransportProvider1;
		OrgHeader TransportProvider2;

		protected TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		QuotationLineListHelper ListHelper => listHelper ?? (listHelper = new QuotationLineListHelper());
		QuotationLineListHelper listHelper;

		#endregion
	}
}
