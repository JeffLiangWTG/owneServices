using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models.CW1RateViewModelsProviderTest
{
	public class ContainerisedTest : BaseTest
	{
		public override bool IsContainerised => true;

		#region By Contract Number

		public void TestGetRates_ByContractNumber_CaseInsensitive()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "cN1", BaseRate = 22m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "Cn1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "cN1", ChargeAmounts = new[] { 22m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "CN1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m } },
					new { ContractNumber = "",    ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_SameChargeCode_IgnoreAndReplaceCarrierContractRegistryEnabled()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "CN1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m } },
					new { ContractNumber = "",    ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "WAR", BaseRate = 66m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "DOF", BaseRate = 88m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "CN1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 77m, 88m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_DifferentChargeCodes_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "WAR", BaseRate = 66m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "DOF", BaseRate = 88m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "CN1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 77m, 88m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "CN1", BaseRate = 55m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "",    ChargeAmounts = new[] { 11m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m } },
					new { ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ContractNumber = "",    ChargeAmounts = new[] { 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 55m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_SameChargeCode_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", ContractNumber = "CN1", BaseRate = 55m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "",    ChargeAmounts = new[] { 11m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m } },
					new { ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ContractNumber = "",    ChargeAmounts = new[] { 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 55m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 11m, 22m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_DifferentChargeCodes_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 11m, 22m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				ContractNumberSelector);
		}

		#endregion

		#region By Commodity Code

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[] { new TestCommodity { Code = "COM", Group = "UCG" } }
				},
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11m, 33m } },

					new { CommodityCode = "",    ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[] { new TestCommodity { Code = "COM", Group = "UCG" } }
				},
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11m, 22m, 33m, 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_IncludingGEN_EmptyCommodityContainer()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "WAR", BaseRate = 66m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[]
					{
						new TestCommodity { Code = "GEN", Group = "GENL" },
						new TestCommodity { Code = "COM", Group = "UCG" },
					}
				},
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "GEN", ChargeAmounts = new[] { 11m, 22m, 33m, 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: ""),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_IncludingGEN_GeneralCommodityContainer()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "WAR", BaseRate = 66m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[]
					{
						new TestCommodity { Code = "GEN", Group = "GENL" },
						new TestCommodity { Code = "COM", Group = "UCG" },
					}
				},
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "GEN", ChargeAmounts = new[] { 11m, 22m, 33m, 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "GEN"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_IncludingGEN_SpecificCommodityContainer()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "WAR", BaseRate = 66m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[]
					{
						new TestCommodity { Code = "GEN", Group = "GENL" },
						new TestCommodity { Code = "COM", Group = "UCG" },
					}
				},
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11m, 22m, 55m, 66m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersHaveSingleValue_IncludingGEN_2SpecificCommodityContainer()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "GEN", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "WAR", BaseRate = 66m },
				},
				filterValues: new TestFilters
				{
					Commodities = new[]
					{
						new TestCommodity { Code = "GEN", Group = "GENL" },
						new TestCommodity { Code = "COM", Group = "UCG" },
					}
				},
				expectedRates: new[]
				{
					// To be shown on LD-7() tab page
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "GEN", ChargeAmounts = new[] { 11m, 22m, 33m, 44m } },

					// To be shown on LD-7(GEN) tab page
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "GEN", ChargeAmounts = new[] { 11m, 22m, 33m, 44m } },

					// To be shown on LD-7(COM) tab page
					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11m, 22m, 55m, 66m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: ""),
					(containerType: "LD-7", commodityCode: "GEN"),
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersAreEmpty_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "XXX", BaseRate = 55m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					// To be shown on LD-7(COM) tab page
					new { CommodityCode = "",    ChargeAmounts = new[] { 11m } },

					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11m, 22m } },

					new { CommodityCode = "",    ChargeAmounts = new[] { 44m } },

					// To be shown on LD-7(COM) tab page

					new { CommodityCode = "",    ChargeAmounts = new[] { 11m } },

					// we expected 11 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "XXX", ChargeAmounts = new[] { 11m, 33m } },

					new { CommodityCode = "",    ChargeAmounts = new[] { 44m } },

					// we expected 44 to be combined with other rates here, but will be removed in calculation
					new { CommodityCode = "XXX", ChargeAmounts = new[] { 44m, 55m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				CommodityCodeSelector);
		}

		public void TestGetRates_ByCommodityCode_FiltersAreEmpty_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "COM", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					// we expected 11 to be combined with other rates here, but would be removed in calculation
					new { CommodityCode = "COM", ChargeAmounts = new[] { 11, 22m, 33m, 44m } },

					new { CommodityCode = "",    ChargeAmounts = new[] { 11m, 22m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CommodityCodeSelector);
		}

		#endregion

		#region By Carrier Service Level

		public void TestGetRates_ByCarrierServiceLevel_FiltersHaveSingleValue_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "XXX", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "",    BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					CarrierServiceLevels = new[] { "XXX" },
				},
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "XXX", ChargeAmounts = new[] { 33m } },
					new { CarrierServiceLevel = "",    ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersHaveSingleValue_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					CarrierServiceLevels = new[] { "STD" },
				},
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m, 33m, 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersAreEmpty_SameChargeCode()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "XXX", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "",    BaseRate = 44m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "XXX", BaseRate = 55m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "",    ChargeAmounts = new[] { 11m } },
					new { CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m } },
					new { CarrierServiceLevel = "XXX", ChargeAmounts = new[] { 33m } },
					new { CarrierServiceLevel = "",    ChargeAmounts = new[] { 44m } },
					new { CarrierServiceLevel = "XXX", ChargeAmounts = new[] { 55m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersAreEmpty_DifferentChargeCodes()
		{
			AssertContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { CarrierServiceLevel = "",    ChargeAmounts = new[] { 11m, 22m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierServiceLevelSelector);
		}

		#endregion

		#region By Service Provider

		public void TestGetRates_ByServiceProvider_FiltersHaveSingleValue()
		{
			AssertContainerisedGetRates(
				message: "Rates should be matched based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", BaseRate = 41m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FiltersHaveMultipleValues()
		{
			AssertContainerisedGetRates(
				message: "Rates should be matched based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", BaseRate = 41m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
					new { ServiceProvider = "BBB", Carrier = "", ChargeAmounts = new[] { 31m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FiltersAreEmpty()
		{
			AssertContainerisedGetRates(
				message: "All rates should be loaded regardless of Service Providers",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", BaseRate = 41m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
					new { ServiceProvider = "BBB", Carrier = "", ChargeAmounts = new[] { 31m } },
					new { ServiceProvider = "CCC", Carrier = "", ChargeAmounts = new[] { 41m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_AllCombinationOfServiceProviderAndCarrier_FiltersHaveSingleValue()
		{
			AssertContainerisedGetRates(
				message: "Rates should be matched by Service Provider and/or Carrier values based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = null,  BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "CCC", BaseRate = 14m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "AAA", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "CCC", BaseRate = 24m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = null,  BaseRate = 31m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "AAA", BaseRate = 32m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "BBB", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "CCC", BaseRate = 34m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  BaseRate = 41m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "AAA", BaseRate = 42m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "BBB", BaseRate = 43m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "CCC", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "AAA", ChargeAmounts = new[] { 22m } },
					new { ServiceProvider = "AAA", Carrier = "BBB", ChargeAmounts = new[] { 23m } },
					new { ServiceProvider = "AAA", Carrier = "CCC", ChargeAmounts = new[] { 24m } },
					new { ServiceProvider = "BBB", Carrier = "AAA", ChargeAmounts = new[] { 32m } },
					new { ServiceProvider = "CCC", Carrier = "AAA", ChargeAmounts = new[] { 42m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_AllCombinationOfServiceProviderAndCarrier_FiltersHaveMultipleValues()
		{
			AssertContainerisedGetRates(
				message: "Rates should be matched by Service Provider and/or Carrier values based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = null,  BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "CCC", BaseRate = 14m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "AAA", BaseRate = 22m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "CCC", BaseRate = 24m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = null,  BaseRate = 31m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "AAA", BaseRate = 32m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "BBB", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", Carrier = "CCC", BaseRate = 34m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  BaseRate = 41m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "AAA", BaseRate = 42m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "BBB", BaseRate = 43m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "CCC", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "AAA", ChargeAmounts = new[] { 22m } },
					new { ServiceProvider = "AAA", Carrier = "BBB", ChargeAmounts = new[] { 23m } },
					new { ServiceProvider = "AAA", Carrier = "CCC", ChargeAmounts = new[] { 24m } },
					new { ServiceProvider = "BBB", Carrier = "AAA", ChargeAmounts = new[] { 32m } },
					new { ServiceProvider = "BBB", Carrier = "BBB", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "BBB", Carrier = "CCC", ChargeAmounts = new[] { 34m } },
					new { ServiceProvider = "CCC", Carrier = "AAA", ChargeAmounts = new[] { 42m } },
					new { ServiceProvider = "CCC", Carrier = "BBB", ChargeAmounts = new[] { 43m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGroupingRates_ByServiceProvider_FallbackToCarrierCostWithoutCarrier_ThereIsMoreSpecificCarrierCost()
		{
			AssertContainerisedGetRates(
				message: "A Carrier Cost with empty Carrier can NOT be loaded when there is a more specific Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "BBB", ChargeAmounts = new[] { 23m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGroupingRates_ByServiceProvider_FallbackToCarrierCostWithoutCarrier_ThereIsNotMoreSpecificCarrierCost()
		{
			AssertContainerisedGetRates(
				message: "A Carrier Cost with empty Carrier can be loaded when there is NOT any more specific Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null, BaseRate = 21m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FallbackToStandardCostWithoutCarrier_FiltersHaveSingleValue()
		{
			AssertContainerisedGetRates(
				message: "A Standard Cost without Carrier cannot be loaded even when there is NOT any other Standard/Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null, Carrier = null, BaseRate = 11m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: Array.Empty<object>(),
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FallbackToStandardCostWithoutCarrier_FiltersAreEmpty()
		{
			AssertContainerisedGetRates(
				message: "A Standard Cost without Carrier cannot be loaded even when there is NOT any other Standard/Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null, Carrier = null, BaseRate = 11m },
				},
				filterValues: new TestFilters(),
				expectedRates: Array.Empty<object>(),
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		#endregion

		#region By Carrier

		public void TestGroupingRates_ByCarrier_FallbackToStandardCostWithCarrier_ThereIsCarrierCost()
		{
			AssertContainerisedGetRates(
				message: "A Standard Cost can NOT be loaded when there is a Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		public void TestGroupingRates_ByCarrier_FallbackToStandardCostWithCarrier_ThereIsNotCarrierCost()
		{
			AssertContainerisedGetRates(
				message: "A Standard Cost can be loaded when there is NOT any Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ContainerType = "LD-7", ServiceProvider = null, Carrier = "AAA", BaseRate = 12m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = null, Carrier = "BBB", BaseRate = 13m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "", Carrier = "AAA", ChargeAmounts = new[] { 12m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				CarrierSelector);
		}

		#endregion

		#region FCL By Container

		public void TestGetRatesByFCLWithEmptyContainer_CommodityLD7()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs without container type and Container LD-7 should still show",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCLWithEmptyContainer_CommodityLD7LD8()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs without container type and Container LD-7 and LD-8 should still show",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 21m } },
					new { ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
					(containerType: "LD-8", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCLWithEmptyContainer_LD7Rate_CommodityLD7()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs without container and with container should show",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, Category = "FCL", Mode = "SEA" },
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m, ContainerType = "LD-7", Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 21m, 15m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCLWithEmptyContainer_LD8Rate_CommodityLD7()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs without container type should still show and hide not matching container",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, Category = "FCL", Mode = "SEA" },
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m, ContainerType = "LD-8", Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 21m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCLWithLD7Rate_CommodityLD7()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs matching container type",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m, ContainerType = "LD-7", Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 15m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCLWithLD8Rate_CommodityLD7()
		{
			AssertContainerisedGetRates(
				message: "FCL Costs not matching container type",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m, ContainerType = "LD-8", Category = "FCL", Mode = "SEA" },
				},
				filterValues: new TestFilters
				{
					Mode = FreightMode.FCL,
				},
				expectedRates: Array.Empty<object>(),
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				EmptyContainerCodeSelector);
		}

		#endregion

		#region Based on Priorities

		#region ContractNumber over Carrier

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_WithSpecificFilterValue()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN2", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
					ContractNumbers = new[] { "CN1", "CN2" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ContractNumber = "CN1", ChargeAmounts = new[] { 22m } },

					new { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "CCC", Carrier = "", ContractNumber = "CN2", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_WithEmptyFilter()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN2", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "", ChargeAmounts = new[] { 11m } },

					new { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "CCC", Carrier = "", ContractNumber = "CN2", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_FallbackToCarrierAtTheSameContractNumberPriorityLevel()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code & same Contract Number
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With the same Charge Code & different Contract Numbers
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN3", ChargeCode = "FRT", BaseRate = 44m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "EEE", Carrier = "FFF", ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "EEE", Carrier = "FFF", ContractNumber = "CN4", ChargeCode = "BAF", BaseRate = 66m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "EEE", Carrier = null,  ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "EEE", Carrier = null,  ContractNumber = "CN4", ChargeCode = "CAF", BaseRate = 88m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD", "EEE", "FFF" },
					ContractNumbers = new[] { "CN1", "CN2", "CN3", "CN4" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "CN1", ChargeAmounts = new[] { 11m } },

					new { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },

					new { ServiceProvider = "EEE", Carrier = "FFF", ContractNumber = "CN4", ChargeAmounts = new[] { 55m, 66m, 88m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		#endregion

		#region ContractNumber over CarrierServiceLevel

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_WithSpecificFilterValue()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
					ContractNumbers = new[] { "CN1", "CN2" },
					CarrierServiceLevels = new[] { "STD", "EXP" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", CarrierServiceLevel = "", ContractNumber = "CN1", ChargeAmounts = new[] { 11m } },

					new { ServiceProvider = "BBB", CarrierServiceLevel = "", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_WithEmptyFilter()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
					CarrierServiceLevels = new[] { "STD", "EXP" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "", ChargeAmounts = new[] { 22m } },

					new { ServiceProvider = "BBB", CarrierServiceLevel = "", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_FallbackToCarrierServiceLevelAtTheSameContractNumberPriorityLevel()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code & same Contract Number
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With the same Charge Code & different Contract Numbers
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "STD", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN3", ChargeCode = "FRT", BaseRate = 44m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CarrierServiceLevel = "STD", ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CarrierServiceLevel = "STD", ContractNumber = "CN4", ChargeCode = "BAF", BaseRate = 66m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CarrierServiceLevel = "",    ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CarrierServiceLevel = "",    ContractNumber = "CN4", ChargeCode = "CAF", BaseRate = 88m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB", "CCC" },
					CarrierServiceLevels = new[] { "STD" },
					ContractNumbers = new[] { "CN1", "CN2", "CN3", "CN4" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "CN1", ChargeAmounts = new[] { 11m } },

					new { ServiceProvider = "BBB", CarrierServiceLevel = "STD", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },

					new { ServiceProvider = "CCC", CarrierServiceLevel = "STD", ContractNumber = "CN4", ChargeAmounts = new[] { 55m, 66m, 88m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "COM"),
				},
				AllFieldsSelector);
		}

		#endregion

		#region ContractNumber over CommodityCode

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCommodityCode_WithSpecificFilterValue()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "YYY", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
					ContractNumbers = new[] { "CN1", "CN2" },
					Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" }, new TestCommodity { Code = "YYY", Group = "UCG" } },
				},
				expectedRates: new[]
				{
					// To be shown on LD-7(XXX) tab page
					new { ServiceProvider = "AAA", CommodityCode = "", ContractNumber = "CN1", ChargeAmounts = new[] { 11m } },
					new { ServiceProvider = "BBB", CommodityCode = "", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },

					// To be shown on LD-7(YYY) tab page
					new { ServiceProvider = "AAA", CommodityCode = "", ContractNumber = "CN1", ChargeAmounts = new[] { 11m } },
					new { ServiceProvider = "BBB", CommodityCode = "", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "BBB", CommodityCode = "YYY", ContractNumber = "", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
					(containerType: "LD-7", commodityCode: "YYY"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCommodityCode_WithEmptyFilter()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "YYY", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB" },
					Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" }, new TestCommodity { Code = "YYY", Group = "UCG" } },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", CommodityCode = "XXX", ContractNumber = "", ChargeAmounts = new[] { 22m } },

					new { ServiceProvider = "BBB", CommodityCode = "", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },
					new { ServiceProvider = "BBB", CommodityCode = "YYY", ContractNumber = "", ChargeAmounts = new[] { 44m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
					(containerType: "LD-8", commodityCode: "YYY"),
				},
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCommodityCode_FallbackToCommodityCodeAtTheSameContractNumberPriorityLevel()
		{
			AssertContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code & same Contract Number
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With the same Charge Code & different Contract Numbers
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "XXX", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    ContractNumber = "CN3", ChargeCode = "FRT", BaseRate = 44m },
					// With different Charge Codes
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CommodityCode = "XXX", ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CommodityCode = "XXX", ContractNumber = "CN4", ChargeCode = "BAF", BaseRate = 66m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CommodityCode = "",    ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", CommodityCode = "",    ContractNumber = "CN4", ChargeCode = "CAF", BaseRate = 88m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA", "BBB", "CCC" },
					Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" } },
					ContractNumbers = new[] { "CN1", "CN2", "CN3", "CN4" },
				},
				expectedRates: new[]
				{
					// we expected 22 to be combined with other rates here, but would be removed in calculation
					new { ServiceProvider = "AAA", CommodityCode = "XXX", ContractNumber = "CN1", ChargeAmounts = new[] { 11m, 22m } },

					new { ServiceProvider = "BBB", CommodityCode = "XXX", ContractNumber = "CN2", ChargeAmounts = new[] { 33m } },

					// we expected 77 to be combined with other rates here, but would be removed in calculation
					new { ServiceProvider = "CCC", CommodityCode = "XXX", ContractNumber = "CN4", ChargeAmounts = new[] { 55m, 66m, 77m, 88m } },
				},
				containerCommodityTabs: new[]
				{
					(containerType: "LD-7", commodityCode: "XXX"),
				},
				AllFieldsSelector);
		}

		#endregion

		#region CommodityCode over Carrier

		public void TestGetRates_BasedOnPriorities_WhenCommodityCodeTakesPriorityOverCarrier()
		{
			using (TemporaryPrioritizeCommodityCode())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CommodityCode = "XXX", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "",    ChargeCode = "BAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  CommodityCode = "XXX", ChargeCode = "CAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "", CommodityCode = "XXX", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "", CommodityCode = "XXX", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCommodityCodeTakesPriority_FallbackToCarrier()
		{
			using (TemporaryPrioritizeCommodityCode())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CommodityCode = "XXX", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-8", ServiceProvider = "CCC", Carrier = null,  CommodityCode = "XXX", ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-8", ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "CCC", Carrier = "", CommodityCode = "XXX", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
						(containerType: "LD-8", commodityCode: "XXX"),
					},
					AllFieldsSelector);
			}
		}

		IDisposable TemporaryPrioritizeCommodityCode()
		{
			var originalValue = Env.Registry.Rating.FreightSearchPriorities;

			return new DisposableAction(
				() => Env.Registry.Rating.FreightSearchPriorities = "TI_RH_NKCommodityCode,TI_RS_NKServiceLevel_NI,TI_OH_TransportProvider,TI_ViaLRC,TI_HBLDeliveryMode",
				() => Env.Registry.Rating.FreightSearchPriorities = originalValue);
		}

		#endregion

		#region CommodityCode over CarrierServiceLevel

		public void TestGetRates_BasedOnPriorities_WhenCommodityCodeTakesPriorityOverCarrierServiceLevel()
		{
			using (TemporaryPrioritizeCommodityCode())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB" },
						CarrierServiceLevels = new[] { "STD" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" }, new TestCommodity { Code = "YYY", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "", ChargeAmounts = new[] { 11m } },

						new { ServiceProvider = "BBB", CommodityCode = "XXX", CarrierServiceLevel = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "BBB", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCommodityCodeTakesPriority_FallbackToCarrierServiceLevel()
		{
			using (TemporaryPrioritizeCommodityCode())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB" },
						CarrierServiceLevels = new[] { "STD", "EXP" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 11m } },

						new { ServiceProvider = "BBB", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
						(containerType: "LD-8", commodityCode: "YYY"),
					},
					AllFieldsSelector);
			}
		}

		#endregion

		#region CarrierServiceLevel over Carrier

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriorityOverCarrier()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
						CarrierServiceLevels = new[] { "STD" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "COM"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriority_FallbackToCarrier()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "CCC", Carrier = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "COM"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriorityOverCarrier_ShouldNotLoadLessSpecificRateSeparately()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 22m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 55m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "WAR", BaseRate = 66m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB" },
						CarrierServiceLevels = new[] { "STD", "EXP" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 11m, 22m, 66m } },
						new { ServiceProvider = "AAA", Carrier = "",    CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m, 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "COM"),
					},
					AllFieldsSelector);
			}
		}

		IDisposable TemporaryPrioritizeServiceLevel()
		{
			var originalValue = Env.Registry.Rating.FreightSearchPriorities;

			return new DisposableAction(
				() => Env.Registry.Rating.FreightSearchPriorities = "TI_RS_NKServiceLevel_NI,TI_OH_TransportProvider,TI_RH_NKCommodityCode,TI_ViaLRC,TI_HBLDeliveryMode",
				() => Env.Registry.Rating.FreightSearchPriorities = originalValue);
		}

		#endregion

		#region CarrierServiceLevel over CommodityCode

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriorityOverCommodityCode()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "BBB", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB" },
						CarrierServiceLevels = new[] { "STD" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" }, new TestCommodity { Code = "YYY", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						// To be shown on LD-7(XXX) tab page
						new { ServiceProvider = "AAA", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m } },
						new { ServiceProvider = "BBB", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },

						// To be shown on LD-7(YYY) tab page
						new { ServiceProvider = "AAA", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m } },
						new { ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "BBB", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
						(containerType: "LD-7", commodityCode: "YYY"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriority_FallbackToCommodityCode()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-8", ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" }, new TestCommodity { Code = "YYY", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "BBB", CommodityCode = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "BBB", CommodityCode = "YYY", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
						(containerType: "LD-8", commodityCode: "YYY"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriorityOverCommodityCode_ShouldNotLoadLessSpecificRateSeparately()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 22m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 55m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "",    ChargeCode = "WAR", BaseRate = 66m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA" },
						CarrierServiceLevels = new[] { "STD", "EXP" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						// To be shown on LD-7(XXX) tab page
						new { ServiceProvider = "AAA", CommodityCode = "XXX", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 11m, 22m, 66m } },
						new { ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m, 44m } },

						// To be shown on LD-7(YYY) tab page
						new { ServiceProvider = "AAA", CommodityCode = "",    CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m, 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
						(containerType: "LD-7", commodityCode: "YYY"),
					},
					AllFieldsSelector);
			}
		}

		#endregion

		#region Carrier over CarrierServiceLevel

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriorityOverCarrierServiceLevel()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
						CarrierServiceLevels = new[] { "STD" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "", ChargeAmounts = new[] { 11m } },

						new { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "COM"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriority_FallbackToCarrierServiceLevel()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "CCC", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "DDD", Carrier = "EEE", CarrierServiceLevel = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "DDD", Carrier = "FFF", CarrierServiceLevel = "STD", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD", "EEE", "FFF" },
						CarrierServiceLevels = new[] { "STD" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "CCC", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "DDD", Carrier = "EEE", CarrierServiceLevel = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "DDD", Carrier = "FFF", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "COM"),
					},
					AllFieldsSelector);
			}
		}

		IDisposable TemporaryPrioritizeTransportProvider()
		{
			var originalValue = Env.Registry.Rating.FreightSearchPriorities;

			return new DisposableAction(
				() => Env.Registry.Rating.FreightSearchPriorities = "TI_OH_TransportProvider,TI_RS_NKServiceLevel_NI,TI_RH_NKCommodityCode,TI_ViaLRC,TI_HBLDeliveryMode",
				() => Env.Registry.Rating.FreightSearchPriorities = originalValue);
		}

		#endregion

		#region Carrier over CommodityCode

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriorityOverCommodityCode()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = null,  CommodityCode = "XXX", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "CCC", Carrier = null,  CommodityCode = "XXX", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "", ChargeAmounts = new[] { 11m } },

						new { ServiceProvider = "CCC", Carrier = "DDD", CommodityCode = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "", CommodityCode = "XXX", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriority_FallbackToCommodityCode()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "BBB", CommodityCode = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "AAA", Carrier = "CCC", CommodityCode = "XXX", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ContainerType = "LD-7", ServiceProvider = "DDD", Carrier = "EEE", CommodityCode = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ContainerType = "LD-7", ServiceProvider = "DDD", Carrier = "FFF", CommodityCode = "XXX", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD", "EEE", "FFF" },
						Commodities = new[] { new TestCommodity { Code = "XXX", Group = "UCG" } },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "CCC", CommodityCode = "XXX", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "DDD", Carrier = "EEE", CommodityCode = "", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "DDD", Carrier = "FFF", CommodityCode = "XXX", ChargeAmounts = new[] { 44m } },
					},
					containerCommodityTabs: new[]
					{
						(containerType: "LD-7", commodityCode: "XXX"),
					},
					AllFieldsSelector);
			}
		}

		#endregion

		#endregion

		#region GetRates Assertion

		MemoryLogger AssertContainerisedGetRates(
			IEnumerable<TestRate> existingRates,
			TestFilters filterValues,
			IEnumerable<object> expectedRates,
			IEnumerable<(string containerType, string commodityCode)> containerCommodityTabs,
			Func<CW1RateViewModel, object> propertiesToCompare = null)
		{
			return AssertContainerisedGetRates(
				message: "",
				existingRates: existingRates,
				filterValues: filterValues,
				expectedRates: expectedRates,
				containerCommodityTabs: containerCommodityTabs,
				propertiesToCompare: propertiesToCompare);
		}

		MemoryLogger AssertContainerisedGetRates(
			string message,
			IEnumerable<TestRate> existingRates,
			TestFilters filterValues,
			IEnumerable<object> expectedRates,
			IEnumerable<(string containerType, string commodityCode)> containerCommodityTabs,
			Func<CW1RateViewModel, object> propertiesToCompare = null)
		{
			return AssertGetRatesCore(
				message: message,
				existingRates: existingRates,
				filterValues: filterValues,
				expectedRates: expectedRates,
				containerCommodityTabs: containerCommodityTabs,
				propertiesToCompare: propertiesToCompare);
		}

		protected override Func<CW1RateViewModel, object> AllFieldsSelector => rate => new
		{
			ServiceProvider = rate.ServiceProviderCode,
			Carrier = rate.CarrierCode,
			ContractNumber = rate.ContractNumber,
			CarrierServiceLevel = rate.CarrierServiceLevel,
			ContainerType = rate.ContainerType,
			CommodityCode = rate.Commodities,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate)
		};

		#endregion
	}
}
