using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CostsComparer))]
	public class CostsComparerTest : NonPersistentBusinessObjectTestCase
	{
		#region TestTransportModes

		public void TestTransportModes()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				RateMode.ULD,
				RateMode.LSE,
				RateMode.LCL,
				RateMode.FCL,
				RateMode.LRO,
				RateMode.FRO,
				RateMode.FTL,
				RateMode.LRA,
				RateMode.FRA,
				RateMode.FWL,
				RateMode.BLK,
				RateMode.BBK,
				RateMode.ROR,
				RateMode.BCN,
				RateMode.SCN,
				RateMode.COU,
				RateMode.UNA,
				RateMode.OBC
			}, TestComparer.TransportModes.Cast<ICodeDescription>().Select(codeDescription => codeDescription.Code));
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			AssertEquals(ZDateTime.Today, TestComparer.ValidFromDate);
			Assert(TestComparer.ShowOriginDestination);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestComparer.Currency);
		}

		#endregion

		#region Has Changes

		public void TestHasChanges()
		{
			AssertEquals(false, TestComparer.HasChanges);
			TestComparer.HasChanges = true;
			AssertEquals("Should not be changed", false, TestComparer.HasChanges);
		}

		#endregion

		#region Read-Only

		public void TestContainerTypeReadOnly()
		{
			TestComparer.Mode = "LSE";
			Assert(TestComparer.ContainerTypeInfo.ReadOnly);

			TestComparer.Mode = "FCL";
			Assert(!TestComparer.ContainerTypeInfo.ReadOnly);

			TestComparer.Mode = "LCL";
			Assert(TestComparer.ContainerTypeInfo.ReadOnly);

			TestComparer.Mode = "ULD";
			Assert(!TestComparer.ContainerTypeInfo.ReadOnly);
		}

		#endregion

		#region Validation

		public void TestValidateMode()
		{
			TestComparer.RunPreSaveValidation();
			AssertHasError(TestComparer.ModeInfo, "Please enter a value.");
			TestComparer.Mode = "FCL";
			AssertNoErrors(TestComparer.ModeInfo);
			TestComparer.Mode = "ALL";
			AssertHasError(TestComparer.ModeInfo, "Enter a valid selection.");
			TestComparer.Mode = "LSE";
			AssertNoErrors(TestComparer.ModeInfo);
			TestComparer.Mode = "RAI";
			AssertHasError(TestComparer.ModeInfo, "Enter a valid selection.");
		}

		public void TestValidateOrigin()
		{
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.OriginInfo);
			TestComparer.Origin = "AUSYD";
			AssertNoErrors(TestComparer.OriginInfo);
			TestComparer.Origin = "#####";
			AssertHasError(TestComparer.OriginInfo, "Enter a valid selection.");
			TestComparer.Origin = "US";
			AssertNoErrors(TestComparer.OriginInfo);
			TestComparer.Origin = "";
			AssertNoErrors(TestComparer.OriginInfo);
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.Origin = "";
			AssertHasError(TestComparer.OriginInfo, "Please enter a value.");
		}

		public void TestValidateDestination()
		{
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.DestinationInfo);
			TestComparer.Destination = "AUEC";
			AssertNoErrors(TestComparer.DestinationInfo);
			TestComparer.Destination = "#####";
			AssertHasError(TestComparer.DestinationInfo, "Enter a valid selection.");
			TestComparer.Destination = "GBLON";
			AssertNoErrors(TestComparer.DestinationInfo);
			TestComparer.Destination = "";
			AssertNoErrors(TestComparer.DestinationInfo);
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Destination = "";
			AssertHasError(TestComparer.DestinationInfo, "Please enter a value.");
		}

		public void TestValidateCommodityCode()
		{
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.CommodityCodeInfo);
			TestComparer.CommodityCode = "GEN";
			AssertNoErrors(TestComparer.CommodityCodeInfo);
			TestComparer.CommodityCode = "###";
			AssertHasError(TestComparer.CommodityCodeInfo, "Enter a valid selection.");
			TestComparer.CommodityCode = "HAZ";
			AssertNoErrors(TestComparer.CommodityCodeInfo);
		}

		public void TestValidateContainerType()
		{
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.ContainerTypeInfo);
			TestComparer.ContainerType = ZGuid.Invalid;
			AssertHasError(TestComparer.ContainerTypeInfo, "Enter a valid selection.");
			TestComparer.ContainerType = ZGuid.NewZGuid();
			AssertNoErrors(TestComparer.ContainerTypeInfo);
			TestComparer.ContainerType = ZGuid.Missing;
			AssertHasError(TestComparer.ContainerTypeInfo, "The selected selection is no longer valid. Please choose a new selection from the list.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestValidateDate()
		{
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.ValidFromDateInfo);
			AssertNoErrors(TestComparer.ValidToDateInfo);

			TestComparer.ValidFromDate = ZDateTime.Invalid;
			AssertHasError(TestComparer.ValidFromDateInfo, "Enter a valid selection.");
			TestComparer.ValidFromDate = ZDateTime.Today;
			AssertNoErrors(TestComparer.ValidFromDateInfo);
			TestComparer.ValidFromDate = ZDateTime.Today.AddYears(-3);
			AssertHasWarning(TestComparer.ValidFromDateInfo, string.Format("The date '{0}' is more than 1 year old.", TestComparer.ValidFromDate.ToString("dd-MMM-yyyy")));

			TestComparer.ValidToDate = ZDateTime.Invalid;
			AssertHasError(TestComparer.ValidToDateInfo, "Enter a valid selection.");
			TestComparer.ValidToDate = ZDateTime.Today;
			AssertNoErrors(TestComparer.ValidToDateInfo);
			TestComparer.ValidToDate = ZDateTime.Today.AddYears(-3);
			AssertHasWarning(TestComparer.ValidToDateInfo, string.Format("The date '{0}' is more than 1 year old.", TestComparer.ValidToDate.ToString("dd-MMM-yyyy")));
		}

		public void TestValidateCurrency()
		{
			#region Create AUD - USD Exchange Rate

			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("Local Currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			Helper.NewExchangeRate("USD", ExchangeRateTypes.Code.BuyRate, 0.7m);
			Factory.Save();

			#endregion

			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.CurrencyInfo);
			TestComparer.Currency = "UAH";
			AssertHasError(TestComparer.CurrencyInfo, "UAH has no current Buy exchange rate to local currency.");
			TestComparer.Currency = "USD";
			AssertNoErrors(TestComparer.CurrencyInfo);
			TestComparer.Currency = "###";
			AssertHasError(TestComparer.CurrencyInfo, "Enter a valid selection.");
			TestComparer.Currency = "AUD";
			AssertNoErrors(TestComparer.CurrencyInfo);
			TestComparer.Currency = "";
			AssertHasError(TestComparer.CurrencyInfo, "Please enter a value.");
		}

		public void TestValidateChargeCodePK()
		{
			Assert(TestComparer.ChargeCodePKInfo.ReadOnly);
			TestComparer.RunPreSaveValidation();
			AssertNoErrors(TestComparer.ChargeCodePKInfo);
			TestComparer.ChargeCodePK = ZGuid.Invalid;
			AssertNoErrors(TestComparer.ChargeCodePKInfo);

			TestComparer.SingleChargeCodeComparisonOnly = true;
			Assert(!TestComparer.ChargeCodePKInfo.ReadOnly);
			TestComparer.RunPreSaveValidation();
			AssertHasError(TestComparer.ChargeCodePKInfo, "Enter a valid selection.");
			TestComparer.ChargeCodePK = ZGuid.Empty;
			AssertHasError(TestComparer.ChargeCodePKInfo, "Please enter a value.");
			TestComparer.ChargeCodePK = Helper.ChargeCodes["ODOC"].PK;
			AssertNoErrors(TestComparer.ChargeCodePKInfo);
		}

		public void TestRowWarnings()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1a.AddRateLine("WAR").TL_WeightVolume = "M3";
			var entry1b = costing1.AddRateEntry("ORG", "LSE", "AUSYD", "");
			entry1b.AddRateLine("ODOC").TL_RateCalculator = NoteCalculator.Code;
			var entry1c = costing1.AddRateEntry("DST", "LSE", "", "USLAX");
			entry1c.AddRateLine("DDOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)5m;
			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(4, TestComparer.Costs[0].RateLines.Count);

			foreach (RateLine line in TestComparer.Costs[0].RateLines)
			{
				switch (line.ChargeCode.AC_Code)
				{
					case "WAR":
						AssertEquals("This charge is expressed in a unit that is different to the Freight charge. Therefore it cannot be used in a cost comparison.", line.RowWarnings.GetFirstMessage());
						break;

					case "ODOC":
						AssertEquals("This calculator cannot be logically used in a cost comparison due to its complex nature.", line.RowWarnings.GetFirstMessage());
						break;

					case "DDOC":
						AssertEquals("USD has no current Buy exchange rate to local currency.", line.RowWarnings.GetFirstMessage());
						break;
				}
			}
		}

		#endregion

		#region Load Costs

		/// <summary>
		/// Integration test
		/// </summary>
		public void TestLoadCostsWithMoreThanCostComparisonLinesNumber()
		{
			RatingDataRegistry.Instance.CostComparisonLinesNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 100);

			var reducedMaxNumberOfRecords = RatingDataRegistry.Instance.CostComparisonLinesNumber.Value / 5;

			for (var i = 0; i < reducedMaxNumberOfRecords + 5; i++)
			{
				var costing = Helper.NewCosting(Helper.NewOrgHeader());

				var entry1 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
				var entry4 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "20GP");
				var entry3 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
				entry3.TI_RH_NKCommodityCode = "HAZ";
				var entry5 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "20GP");
				entry5.TI_RH_NKCommodityCode = "HAZ";
				var entry2 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO", "", "");
				var entry6 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
				var entry7 = costing.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "", "");
				entry7.TI_RH_NKCommodityCode = "HAZ";

				entry1.RateLines[0].TL_AC = Helper.ChargeCodes["BAF"].PK;
				entry2.RateLines[0].TL_AC = Helper.ChargeCodes["WAR"].PK;
				entry3.RateLines[0].TL_AC = Helper.ChargeCodes["FRT"].PK;
				entry4.RateLines[0].TL_AC = Helper.ChargeCodes["FRT"].PK;
				entry5.RateLines[0].TL_AC = Helper.ChargeCodes["BAF"].PK;
				entry6.RateLines[0].TL_AC = Helper.ChargeCodes["FRT"].PK;
				entry7.RateLines[0].TL_AC = Helper.ChargeCodes["BAF"].PK;
			}

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();

			AssertEquals(RatingDataRegistry.Instance.CostComparisonLinesNumber.Value, TestComparer.Costs.Count);
		}

		public void TestLoadCosts()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1a.RateLines[0].TL_AC = Helper.ChargeCodes["BAF"].PK;

			var entry1b = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO", "", "");

			var entry1c = costing1.AddRateEntry("AIR", "ULD", "AUSYD", "USLAX", "", "");
			entry1c.TI_RH_NKCommodityCode = "HAZ";
			var entry1d = costing1.AddRateEntry("AIR", "ULD", "AUSYD", "USSFO", "", "");

			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2a = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry2b = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			entry2b.TI_ContractNumber = "A00004231";
			var entry2c = costing2.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX", "", "");
			entry2c.TI_RH_NKCommodityCode = "HAZ";

			var costing3 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry3a = costing3.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry3a.RateLines[0].TL_AC = Helper.ChargeCodes["WAR"].PK;
			entry3a.RateLines[0].ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;
			var entry3b = costing3.AddRateEntry("LCL", "LCL", "AUMEL", "USSFO");
			entry3b.TI_ContractNumber = "A00004231";

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1a, entry1b });

			TestComparer.Mode = "ULD";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1c, entry1d });

			TestComparer.CommodityCode = "HAZ";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1c });

			TestComparer.CommodityCode = "";
			TestComparer.Mode = "LCL";
			TestComparer.Origin = "AUSYD";
			TestComparer.Destination = "USLAX";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2c });

			TestComparer.Origin = "AUEC";
			TestComparer.Destination = "USCA";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2c, entry3b });

			TestComparer.CommodityCode = "HAZ";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2c });

			TestComparer.CommodityCode = "";
			TestComparer.Mode = "FCL";
			TestComparer.Origin = "AU";
			TestComparer.Destination = "US";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2a, entry2b });

			TestComparer.ContainerType = Helper.Containers["40GP"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2b });

			TestComparer.CommodityCode = "";
			TestComparer.Mode = "";
			TestComparer.Origin = "";
			TestComparer.Destination = "";
			TestComparer.ContainerType = ZGuid.Empty;
			TestComparer.ContractNumber = "A00004231";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2b, entry3b });
		}

		public void TestLoadCostsContainerClass()
		{
			var container1 = Helper.Containers["20GP"];
			container1.RC_FreightRateClass = "20GN";
			var container2 = Helper.Containers["20FR"];
			container2.RC_FreightRateClass = "20GN";

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry2 = costing.AddRateEntry("FCL", "SEA", "AUMEL", "USLAX", "", "20FR");
			entry2.TI_MatchContainerRateClass = true;
			var entry3 = costing.AddRateEntry("FCL", "SEA", "AUBNE", "USLAX", "", "20FR");

			var entry4 = costing.AddRateEntry("ORG", "FCL", "NZAKL", "", "", "20GP");
			var entry5 = costing.AddRateEntry("ORG", "FCL", "NZWLG", "", "", "20FR");
			entry5.TI_MatchContainerRateClass = true;
			entry5.AddRateLine("ODOC");
			var entry6 = costing.AddRateEntry("ORG", "FCL", "NZCHC", "", "", "20FR");

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ContainerType = container1.PK;
			TestComparer.ShowAllCharges = true;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1, entry2 });

			container1.RC_FreightRateClass = "20GN";
			container1.RC_HandlingRateClass = "20GN";
			container2.RC_FreightRateClass = "";
			container2.RC_HandlingRateClass = "20GN";
			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowOriginDestination = false;
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry4, entry5 });

			TestComparer.ShowOriginChargesOnly = false;
			TestComparer.SingleChargeCodeComparisonOnly = true;
			TestComparer.ChargeCodePK = Helper.ChargeCodes["ODOC"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry5 });

			TestComparer.ChargeCodePK = Helper.ChargeCodes["FRT"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1 });
		}

		public void TestLoadCostsContainerClass_IncludeOriginDestination()
		{
			var containerWithFreightRateClass = Helper.Containers["20GP"];
			containerWithFreightRateClass.RC_FreightRateClass = "20GN";

			var containerWithFreightAndHandlingRateClass = Helper.Containers["20FR"];
			containerWithFreightAndHandlingRateClass.RC_HandlingRateClass = "20GN";
			containerWithFreightAndHandlingRateClass.RC_FreightRateClass = "20GN";

			var containerWithHandlingRateClass = Helper.Containers["20HC"];
			containerWithHandlingRateClass.RC_HandlingRateClass = "20GN";

			var costing = Helper.NewCosting(null);
			var entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry2 = costing.AddRateEntry("FCL", "SEA", "AUMEL", "USLAX", "", "20GP");
			entry2.TI_MatchContainerRateClass = true;
			var entry3 = costing.AddRateEntry("FCL", "SEA", "AUBNE", "USLAX", "", "20GP");

			var entry4 = costing.AddRateEntry("ORG", "FCL", "AUMEL", "USLAX", "", "20GP");
			entry4.AddRateLine("ODOC");
			var entry5 = costing.AddRateEntry("ORG", "FCL", "AUMEL", "USLAX", "", "20FR");
			entry5.TI_MatchContainerRateClass = true;
			entry5.AddRateLine("ODOC");
			var entry6 = costing.AddRateEntry("ORG", "FCL", "AU", "USLAX", "", "20FR");
			entry6.AddRateLine("ODOC");

			var entry7 = costing.AddRateEntry("DST", "FCL", "", "USLAX", "", "20GP");
			entry7.AddRateLine("DDOC");
			var entry8 = costing.AddRateEntry("DST", "FCL", "", "USLAX", "", "20HC");
			entry8.TI_MatchContainerRateClass = true;
			entry8.AddRateLine("DDOC");

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ShowAllCharges = true;
			TestComparer.ShowOriginDestination = true;

			TestComparer.ContainerType = containerWithFreightAndHandlingRateClass.PK;
			TestComparer.LoadCosts();

			var expectedLines258 = new List<RateLine>();
			expectedLines258.AddRange(entry2.RateLines.Cast<RateLine>().ToArray());
			expectedLines258.AddRange(entry5.RateLines.Cast<RateLine>().ToArray());
			expectedLines258.AddRange(entry8.RateLines.Cast<RateLine>().ToArray());

			var expectedEntries = new[] {
				new DummyCostsComparisonEntry
				{
					Entry = entry2,
					RateLines = expectedLines258,
				}
			};
			AssertCostsComparer(expectedEntries, "Entry 2 + related entries 5,8");

			TestComparer.ContainerType = containerWithFreightRateClass.PK;
			TestComparer.LoadCosts();

			var expectedLines17 = new List<RateLine>();
			expectedLines17.AddRange(entry1.RateLines.Cast<RateLine>().ToArray());
			expectedLines17.AddRange(entry7.RateLines.Cast<RateLine>().ToArray());

			var expectedLines247 = new List<RateLine>();
			expectedLines247.AddRange(entry2.RateLines.Cast<RateLine>().ToArray());
			expectedLines247.AddRange(entry4.RateLines.Cast<RateLine>().ToArray());
			expectedLines247.AddRange(entry7.RateLines.Cast<RateLine>().ToArray());

			var expectedLines37 = new List<RateLine>();
			expectedLines37.AddRange(entry3.RateLines.Cast<RateLine>().ToArray());
			expectedLines37.AddRange(entry7.RateLines.Cast<RateLine>().ToArray());

			expectedEntries = new[] {
				new DummyCostsComparisonEntry
				{
					Entry = entry1,
					RateLines = expectedLines17,
				},
				new DummyCostsComparisonEntry
				{
					Entry = entry2,
					RateLines = expectedLines247,
				},
				new DummyCostsComparisonEntry
				{
					Entry = entry3,
					RateLines = expectedLines37,
				}
			};
			AssertCostsComparer(expectedEntries, "Entry 1 + related entry 7; entry 2 + related entries 4,7; entry 3 + related entry 7");

			TestComparer.ContainerType = containerWithHandlingRateClass.PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(Enumerable.Empty<DummyCostsComparisonEntry>(), "No entry");
		}

		public void TestLoadCostsDifferentDates()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2011, 5, 15);
			entry1.TI_RateEndDate = ZDate.Empty;

			var entry2 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USCHI");
			entry2.TI_RateStartDate = new ZDate(2011, 6, 5);
			entry2.TI_RateEndDate = ZDate.Empty;

			var entry3 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "CNSHA");
			entry3.TI_RateStartDate = new ZDate(2011, 7, 1);
			entry3.TI_RateEndDate = ZDate.Empty;

			var entry4 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "AUBNE");
			entry4.TI_RateStartDate = new ZDate(2011, 5, 15);
			entry4.TI_RateEndDate = new ZDate(2011, 6, 5);

			var entry5 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			entry5.TI_RateStartDate = new ZDate(2011, 5, 15);
			entry5.TI_RateEndDate = new ZDate(2011, 7, 1);

			var entry6 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "AUPER");
			entry6.TI_RateStartDate = new ZDate(2011, 5, 15);
			entry6.TI_RateEndDate = new ZDate(2011, 5, 31);

			var entry7 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "UAODS");
			entry7.TI_RateStartDate = new ZDate(2011, 6, 1);
			entry7.TI_RateEndDate = new ZDate(2011, 6, 30);

			var entry8 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NLAMS");
			entry8.TI_RateStartDate = new ZDate(2011, 6, 5);
			entry8.TI_RateEndDate = new ZDate(2011, 7, 5);

			var entry9 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			entry9.TI_RateStartDate = new ZDate(2011, 7, 1);
			entry9.TI_RateEndDate = new ZDate(2011, 7, 5);

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.ValidFromDate = ZDateTime.Empty;
			TestComparer.ValidToDate = ZDateTime.Empty;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1, entry2, entry3, entry4, entry5, entry6, entry7, entry8, entry9 });

			TestComparer.ValidFromDate = new ZDateTime(2011, 6, 1);
			TestComparer.ValidToDate = new ZDateTime(2011, 6, 30);
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1, entry2, entry4, entry5, entry7, entry8 });

			TestComparer.ValidFromDate = new ZDateTime(2011, 6, 1);
			TestComparer.ValidToDate = ZDateTime.Empty;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1, entry2, entry3, entry4, entry5, entry7, entry8, entry9 });

			TestComparer.ValidFromDate = ZDateTime.Empty;
			TestComparer.ValidToDate = new ZDateTime(2011, 6, 30);
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1, entry2, entry4, entry5, entry6, entry7, entry8 });
		}

		public void TestLoadCostsOriginDestinationOnly()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var entry1b = costing1.AddRateEntry("ORG", "LSE", "AUSYD", "");
			var entry1c = costing1.AddRateEntry("DST", "LSE", "", "USLAX");

			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2a = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry2b = costing2.AddRateEntry("ORG", "FCL", "AUSYD", "");
			var entry2c = costing2.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			var entry2d = costing2.AddRateEntry("ORG", "SEA", "AUSYD", "");
			var entry2e = costing2.AddRateEntry("ORG", "ALL", "AUSYD", "");

			var costing3 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry3a = costing3.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			var entry3b = costing3.AddRateEntry("DST", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.Mode = "LSE";
			TestComparer.Origin = "AUSYD";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1b });

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry2b, entry2c });

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			AssertEquals(0, TestComparer.Costs.Count);

			TestComparer.ShowOriginChargesOnly = false;
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Mode = "LSE";
			TestComparer.Origin = "";
			TestComparer.Destination = "US";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1c });

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			AssertEquals(0, TestComparer.Costs.Count);

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry3b });
		}

		public void TestLoadCostsForRegionsCountries()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var entry1b = costing1.AddRateEntry("AIR", "LSE", "AUEC", "USCA");
			var entry1c = costing1.AddRateEntry("AIR", "LSE", "AU", "US");

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.Origin = "AUSYD";
			TestComparer.Destination = "USLAX";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1a, entry1b, entry1c });

			TestComparer.Origin = "AUMEL";
			TestComparer.Destination = "USSFO";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1b, entry1c });

			TestComparer.Origin = "AUPER";
			TestComparer.Destination = "USNYC";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1c });
		}

		public void TestLoadCostsForSingleChargeCodeComparisonOnly()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var entry1b = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO");
			entry1b.AddRateLine("WAR");
			var entry1c = costing1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1c.AddRateLine("ODOC");
			var entry1d = costing1.AddRateEntry("DST", "AIR", "", "USSFO");
			entry1d.AddRateLine("DDOC");

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.ShowAllCharges = false;
			TestComparer.SingleChargeCodeComparisonOnly = true;

			TestComparer.ChargeCodePK = Helper.ChargeCodes["FRT"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1a, entry1b });
			AssertEquals(1, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(1, TestComparer.Costs[1].RateLines.Count);

			TestComparer.ChargeCodePK = Helper.ChargeCodes["WAR"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1b });
			AssertEquals(1, TestComparer.Costs[0].RateLines.Count);

			TestComparer.ChargeCodePK = Helper.ChargeCodes["ODOC"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1c });

			TestComparer.ChargeCodePK = Helper.ChargeCodes["DDOC"].PK;
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1d });
		}

		public void TestLoadCosts_OverrridenRateLines()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1a.RateLines[0].Calculator["UNT"] = (ZDecimal)1000m;
			entry1a.TI_TransitTime = "10";

			var entry1b = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1b.TI_ViaLRC = "SGSIN";
			entry1b.RateLines[0].Calculator["UNT"] = (ZDecimal)500m;
			entry1b.TI_TransitTime = "20";

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertCostsComparer(new[] { entry1a, entry1b });
			var comparer1a = TestComparer.Costs.Cast<CostsComparerEntry>().First(x => x.Entry.PK == entry1a.PK);
			var comparer1b = TestComparer.Costs.Cast<CostsComparerEntry>().First(x => x.Entry.PK == entry1b.PK);
			AssertEquals("rate line count", 1, comparer1a.RateLines.Count);
			AssertEquals("rate line count", 1, comparer1b.RateLines.Count);
			AssertEquals("correct rate line was matched", entry1a.RateLines[0].PK, comparer1a.RateLines[0].PK);
			AssertEquals("correct rate line was matched", entry1b.RateLines[0].PK, comparer1b.RateLines[0].PK);
		}

		public void TestLoadCosts_RelatedEntriesShouldBeFiltered()
		{
			var container1 = Helper.Containers["20GP"];
			container1.RC_FreightRateClass = "20GN";
			var container2 = Helper.Containers["20FR"];
			container2.RC_FreightRateClass = "20GN";
			container2.RC_HandlingRateClass = "20GN";

			var costing = Helper.NewCosting(null);

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "IT", "AU", ZString.Empty, "20GP");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 1;

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "IT", "AU", ZString.Empty, "20FR");
			entry2.TI_MatchContainerRateClass = true;
			entry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = entry2.AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 2;

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "IT", "AU", ZString.Empty, "20GP");
			var rateLine3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 3;

			var entry4 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "IT", "AU");
			var rateLine4 = entry4.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine4.GetCalculator<FlatCalculator>().BaseRate = 4;

			var entry5 = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "IT", "AU");
			var rateLine5 = entry5.AddRateLine("DDOC", FlatCalculator.Code);
			rateLine5.GetCalculator<FlatCalculator>().BaseRate = 5;

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.Origin = "IT";
			TestComparer.Destination = "AU";
			TestComparer.ContainerType = container1.PK;
			TestComparer.ShowOriginDestination = true;

			TestComparer.LoadCosts();
			var expectedEntries = new[] { entry1, entry2 };
			AssertCostsComparer(expectedEntries);
		}

		#endregion

		#region Summary Items

		public void TestSummaryItems()
		{
			#region Create AUD - USD Exchange Rate

			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("Local Currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			Helper.NewExchangeRate("USD", ExchangeRateTypes.Code.BuyRate, 0.7m);

			#endregion

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			entry1a.RateLines[0].Calculator["MIN"] = (ZDecimal)100m;
			entry1a.RateLines[0].Calculator["-45"] = (ZDecimal)5m;
			entry1a.RateLines[0].Calculator["+45"] = (ZDecimal)4m;
			entry1a.RateLines[0].Calculator["+100"] = (ZDecimal)3m;
			entry1a.RateLines[0].Calculator["+250"] = (ZDecimal)2m;
			entry1a.RateLines[0].Calculator["+500"] = (ZDecimal)1.9m;
			entry1a.RateLines[0].Calculator["+1000"] = (ZDecimal)1.85m;
			entry1a.AddRateLine("WAR");
			entry1a.RateLines[1].TL_RateCalculator = UnitCalculator.Code;
			entry1a.RateLines[1].Calculator["UNT"] = (ZDecimal)0.1m;

			var entry1b = costing1.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			entry1b.RateLines[0].Calculator["MIN"] = (ZDecimal)90m;
			entry1b.RateLines[0].Calculator["UNT"] = (ZDecimal)90m;

			var entry1c = costing1.AddRateEntry("FCL", "SEA", "USLAX", "AUSYD", "", "20GP");
			entry1c.RateLines[0].Calculator["UNT"] = (ZDecimal)1000m;
			entry1c.AddRateLine("BAF");
			entry1c.RateLines[1].TL_RateCalculator = UnitCalculator.Code;
			entry1c.RateLines[1].Calculator["UNT"] = (ZDecimal)50m;

			var entry1d = costing1.AddRateEntry("ORG", "ALL", "USLAX", "", "", "");
			entry1d.AddRateLine("ODOC").Calculator["BAS"] = (ZDecimal)15m;

			Factory.Save();

			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			TestComparer.Currency = "USD";
			AssertEquals(8, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "MIN", 100m);
			AssertSummaryItem(2, "BAS", 15m);
			AssertSummaryItem(3, "-45", 5.1m);
			AssertSummaryItem(4, "+45", 4.1m);
			AssertSummaryItem(5, "+100", 3.1m);
			AssertSummaryItem(6, "+250", 2.1m);
			AssertSummaryItem(7, "+500", 2m);
			AssertSummaryItem(8, "+1000", 1.95m);
			AssertSummaryItem(9, "", 0m);
			AssertSummaryItem(10, "", 0m);

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			AssertEquals(3, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "MIN", 90m);
			AssertSummaryItem(2, "BAS", 15m);
			AssertSummaryItem(3, "UNT", 90m);

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			AssertEquals(2, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "BAS", 15m);
			AssertSummaryItem(2, "UNT", 1050m);
		}

		void AssertSummaryItem(int columnNumber, ZString rateItemType, ZDecimal value)
		{
			if (columnNumber <= TestComparer.SummaryItems.Count)
			{
				var rateItem = TestComparer.SummaryItems.Keys[columnNumber - 1];
				var actualType = rateItem.Type;
				var breakValue = rateItem.Break; //caching to avoid impure method call
				if (!breakValue.IsEmpty)
				{
					actualType += breakValue.ToString("f0");
				}

				AssertEquals(rateItemType, actualType);
				AssertEquals(value.ToString("f2"), TestComparer.Costs[0].SummaryColumn("SummaryColumn" + columnNumber.ToString("f0"), typeof(ZString)));
			}
			else
			{
				AssertEquals("", rateItemType);
				AssertEquals("", TestComparer.Costs[0].SummaryColumn("SummaryColumn" + columnNumber.ToString("f0"), typeof(ZString)));
			}
		}

		#endregion

		#region Container classes

		public void TestGetContainerClasses()
		{
			var container1 = Helper.Containers["20GP"];
			container1.RC_FreightRateClass = "22GP";

			var container2 = Helper.Containers["40GP"];
			container2.RC_HandlingRateClass = "42PS";

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ContainerType = container1.PK;
			AssertEquals("22GP - General purpose cont.", TestComparer.ContainerFreightRatingClass);
			AssertEquals(ZString.Empty, TestComparer.ContainerHandlingRatingClass);

			TestComparer.ContainerType = container2.PK;
			AssertEquals(ZString.Empty, TestComparer.ContainerFreightRatingClass);
			AssertEquals("42PS - Flat (space saver)", TestComparer.ContainerHandlingRatingClass);

			TestComparer.ContainerType = ZGuid.Empty;
			AssertEquals(ZString.Empty, TestComparer.ContainerFreightRatingClass);
			AssertEquals(ZString.Empty, TestComparer.ContainerHandlingRatingClass);
		}

		public void TestContainerClassPropertiesAreReadOnly()
		{
			AssertEquals(true, TestComparer.ContainerFreightRatingClassInfo.ReadOnly);
			AssertEquals(true, TestComparer.ContainerHandlingRatingClassInfo.ReadOnly);
		}

		#endregion

		#region VisualizerNoteSupporter
		public void TestIVisualizerNoteSupporter()
		{
			CostsComparer costsComparer = new CostsComparer();
			AssertEquals(false, ((IVisualizerNoteSupporter)costsComparer).PK.IsEmpty);
			AssertEquals(RateEntrySchema.Constants.Prefix, ((IVisualizerNoteSupporter)costsComparer).TableCode);
			AssertEquals(Guid.Empty, ((IVisualizerNoteSupporter)costsComparer).ChildBusinessObjectPK);
		}
		#endregion

		#region Implementation

		CostsComparer TestComparer
		{
			get { return fTestComparer ?? (fTestComparer = new CostsComparer()); }
		}

		CostsComparer fTestComparer;

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		class AssertionRateEntry : IEquatable<AssertionRateEntry>
		{
			public AssertionRateEntry(RateEntry entry)
			{
				rateEntry = entry;
			}

			public override string ToString()
			{
				var result = new ZStringBuilder(new[]
				{
					$"PK: {rateEntry.PK}",
					$"RateCategory: {rateEntry.TI_RateCategory}",
					$"Mode: {rateEntry.TI_Mode}",
					$"Origin: {rateEntry.TI_OriginLRC}",
					$"Destination: {rateEntry.TI_DestinationLRC}",
					$"ContractNumber: {rateEntry.TI_ContractNumber}",
					$"CommodityCode: {rateEntry.CommodityCode?.RH_Code ?? ""}",
					$"Container: {rateEntry.Container?.RC_Code ?? ""}",
					$"FreightRateClass: {rateEntry.Container?.RC_FreightRateClass ?? ""}",
					$"HandlingRateClass: {rateEntry.Container?.RC_HandlingRateClass ?? ""}",
				});

				return result.ToStringWithNewLineBetweenAppends();
			}

			public ZGuid PK => rateEntry?.PK ?? ZGuid.Invalid;

			RateEntry rateEntry { get; }

			public bool Equals(AssertionRateEntry other)
			{
				return other.PK == PK;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((AssertionRateEntry)obj);
			}

			public override int GetHashCode()
			{
				return PK.GetHashCode();
			}
		}

		class AssertionRateLine : IEquatable<AssertionRateLine>
		{
			public AssertionRateLine(RateLine line)
			{
				rateLine = line;
			}

			public override string ToString()
			{
				var result = new ZStringBuilder(new[]
				{
					$"PK: {rateLine.PK}",
					$"TL_AC: {rateLine.ChargeCode.AC_Code}",
					$"TL_RateCalculator: {rateLine.TL_RateCalculator}",
					$"TL_WeightVolume: {rateLine.TL_WeightVolume}",
				});

				return result.ToStringWithNewLineBetweenAppends();
			}

			public ZGuid PK => rateLine?.PK ?? ZGuid.Invalid;

			RateLine rateLine { get; }

			public bool Equals(AssertionRateLine other)
			{
				return other.PK == PK;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((AssertionRateLine)obj);
			}

			public override int GetHashCode()
			{
				return PK.GetHashCode();
			}
		}

		void AssertCostsComparer(IEnumerable<RateEntry> expected, string message = "")
		{
			if (expected == null)
			{
				AssertEquals(false, TestComparer.Costs.Any());
				return;
			}

			var actual = TestComparer.Costs.Cast<CostsComparerEntry>();

			var expectedEntries = expected.Select(x => new AssertionRateEntry(x));
			var actualEntries = actual.Select(x => new AssertionRateEntry(x.Entry));

			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);
		}

		void AssertCostsComparer(IEnumerable<DummyCostsComparisonEntry> expected, string message = "")
		{
			if (expected == null)
			{
				AssertEquals(false, TestComparer.Costs.Any());
				return;
			}

			var actual = TestComparer.Costs.Cast<CostsComparerEntry>().ToArray();
			var expectedEntries = expected.Select(x => new AssertionRateEntry(x.Entry)).ToArray();
			var actualEntries = actual.Select(x => new AssertionRateEntry(x.Entry)).ToArray();

			AssertContainsExactElementsInAnyOrder(message, expectedEntries, actualEntries);

			foreach (var dummyCostsComparisonEntry in expected)
			{
				var expectedLines = dummyCostsComparisonEntry.RateLines.Select(x => new AssertionRateLine(x));
				var actualEntry = actual.Single(x => x.Entry.PK == dummyCostsComparisonEntry.Entry.PK);
				var actualLines = actualEntry.RateLines.Select(x => new AssertionRateLine(x));

				AssertContainsExactElementsInAnyOrder("Compare rate lines", expectedLines, actualLines);
			}
		}

		class DummyCostsComparisonEntry
		{
			public RateEntry Entry { get; set; }
			public List<RateLine> RateLines { get; set; }
		}

		#endregion
	}
}
