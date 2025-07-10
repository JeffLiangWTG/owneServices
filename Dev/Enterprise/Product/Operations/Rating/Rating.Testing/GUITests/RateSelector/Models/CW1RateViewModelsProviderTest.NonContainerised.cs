using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models.CW1RateViewModelsProviderTest
{
	public class NonContainerisedTest : BaseTest
	{
		public override bool IsContainerised => false;

		#region By Contract Number

		public void TestGetRates_ByContractNumber_CaseInsensitive()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "cN1", BaseRate = 22m },
				},
				filterValues: new TestFilters
				{
					ContractNumbers = new[] { "Cn1" },
				},
				expectedRates: new[]
				{
					new { ContractNumber = "cN1", ChargeAmounts = new[] { 22m } },
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_SameChargeCode()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_SameChargeCode_IgnoreAndReplaceCarrierContractRegistryEnabled()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_DifferentChargeCodes()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "WAR", BaseRate = 66m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "DOF", BaseRate = 88m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersHaveSingleValue_DifferentChargeCodes_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", ChargeCode = "WAR", BaseRate = 66m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    ChargeCode = "DOF", BaseRate = 88m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_SameChargeCode()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "CN1", BaseRate = 55m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_SameChargeCode_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN2", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "",    BaseRate = 44m },
					new TestRate { ServiceProvider = "BBB", ContractNumber = "CN1", BaseRate = 55m },
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
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_DifferentChargeCodes()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 11m, 22m } },
				},
				ContractNumberSelector);
		}

		public void TestGetRates_ByContractNumber_FiltersAreEmpty_DifferentChargeCodes_IgnoreAndReplaceCarrierContractNumbers()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", ContractNumber = "CN1", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { ContractNumber = "CN1", ChargeAmounts = new[] { 33m, 44m } },
					new { ContractNumber = "", ChargeAmounts = new[] { 11m, 22m } },
				},
				ContractNumberSelector);
		}

		#endregion

		#region By Carrier Service Level

		public void TestGetRates_ByCarrierServiceLevel_FiltersHaveSingleValue_SameChargeCode()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "XXX", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "",    BaseRate = 44m },
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
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersHaveSingleValue_DifferentChargeCodes()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters
				{
					CarrierServiceLevels = new[] { "STD" },
				},
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m, 33m, 44m } },
				},
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersAreEmpty_SameChargeCode()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "XXX", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "",    BaseRate = 44m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "XXX", BaseRate = 55m },
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
				CarrierServiceLevelSelector);
		}

		public void TestGetRates_ByCarrierServiceLevel_FiltersAreEmpty_DifferentChargeCodes()
		{
			AssertNonContainerisedGetRates(
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { CarrierServiceLevel = "STD", ChargeAmounts = new[] { 22m, 33m, 44m } },
					new { CarrierServiceLevel = "",    ChargeAmounts = new[] { 11m, 22m } },
				},
				CarrierServiceLevelSelector);
		}

		#endregion

		#region By Service Provider

		public void TestGetRates_ByServiceProvider_FiltersHaveSingleValue()
		{
			AssertNonContainerisedGetRates(
				message: "Rates should be matched based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ServiceProvider = "CCC", BaseRate = 41m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FiltersHaveMultipleValues()
		{
			AssertNonContainerisedGetRates(
				message: "Rates should be matched based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ServiceProvider = "CCC", BaseRate = 41m },
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
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FiltersAreEmpty()
		{
			AssertNonContainerisedGetRates(
				message: "All rates should be loaded regardless of Service Providers",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m },
					new TestRate { ServiceProvider = "BBB", BaseRate = 31m },
					new TestRate { ServiceProvider = "CCC", BaseRate = 41m },
				},
				filterValues: new TestFilters(),
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
					new { ServiceProvider = "BBB", Carrier = "", ChargeAmounts = new[] { 31m } },
					new { ServiceProvider = "CCC", Carrier = "", ChargeAmounts = new[] { 41m } },
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_AllCombinationOfServiceProviderAndCarrier_FiltersHaveSingleValue()
		{
			AssertNonContainerisedGetRates(
				message: "Rates should be matched by Service Provider and/or Carrier values based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  Carrier = null,  BaseRate = 11m },
					new TestRate { ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ServiceProvider = null,  Carrier = "CCC", BaseRate = 14m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ServiceProvider = "AAA", Carrier = "AAA", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
					new TestRate { ServiceProvider = "AAA", Carrier = "CCC", BaseRate = 24m },
					new TestRate { ServiceProvider = "BBB", Carrier = null,  BaseRate = 31m },
					new TestRate { ServiceProvider = "BBB", Carrier = "AAA", BaseRate = 32m },
					new TestRate { ServiceProvider = "BBB", Carrier = "BBB", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", Carrier = "CCC", BaseRate = 34m },
					new TestRate { ServiceProvider = "CCC", Carrier = null,  BaseRate = 41m },
					new TestRate { ServiceProvider = "CCC", Carrier = "AAA", BaseRate = 42m },
					new TestRate { ServiceProvider = "CCC", Carrier = "BBB", BaseRate = 43m },
					new TestRate { ServiceProvider = "CCC", Carrier = "CCC", BaseRate = 44m },
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
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_AllCombinationOfServiceProviderAndCarrier_FiltersHaveMultipleValues()
		{
			AssertNonContainerisedGetRates(
				message: "Rates should be matched by Service Provider and/or Carrier values based on Service Provider filter value",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  Carrier = null,  BaseRate = 11m },
					new TestRate { ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ServiceProvider = null,  Carrier = "CCC", BaseRate = 14m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ServiceProvider = "AAA", Carrier = "AAA", BaseRate = 22m },
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
					new TestRate { ServiceProvider = "AAA", Carrier = "CCC", BaseRate = 24m },
					new TestRate { ServiceProvider = "BBB", Carrier = null,  BaseRate = 31m },
					new TestRate { ServiceProvider = "BBB", Carrier = "AAA", BaseRate = 32m },
					new TestRate { ServiceProvider = "BBB", Carrier = "BBB", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", Carrier = "CCC", BaseRate = 34m },
					new TestRate { ServiceProvider = "CCC", Carrier = null,  BaseRate = 41m },
					new TestRate { ServiceProvider = "CCC", Carrier = "AAA", BaseRate = 42m },
					new TestRate { ServiceProvider = "CCC", Carrier = "BBB", BaseRate = 43m },
					new TestRate { ServiceProvider = "CCC", Carrier = "CCC", BaseRate = 44m },
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
				CarrierSelector);
		}

		public void TestGroupingRates_ByServiceProvider_FallbackToCarrierCostWithoutCarrier_ThereIsMoreSpecificCarrierCost()
		{
			AssertNonContainerisedGetRates(
				message: "A Carrier Cost with empty Carrier can NOT be loaded when there is a more specific Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", BaseRate = 23m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "BBB", ChargeAmounts = new[] { 23m } },
				},
				CarrierSelector);
		}

		public void TestGroupingRates_ByServiceProvider_FallbackToCarrierCostWithoutCarrier_ThereIsNotMoreSpecificCarrierCost()
		{
			AssertNonContainerisedGetRates(
				message: "A Carrier Cost with empty Carrier can be loaded when there is NOT any more specific Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", Carrier = null, BaseRate = 21m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FallbackToStandardCostWithoutCarrier_FiltersHaveSingleValue()
		{
			AssertNonContainerisedGetRates(
				message: "A Standard Cost without Carrier cannot be loaded even when there is NOT any other Standard/Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null, Carrier = null, BaseRate = 11m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: Array.Empty<object>(),
				CarrierSelector);
		}

		public void TestGetRates_ByServiceProvider_FallbackToStandardCostWithoutCarrier_FiltersAreEmpty()
		{
			AssertNonContainerisedGetRates(
				message: "A Standard Cost without Carrier cannot be loaded even when there is NOT any other Standard/Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null, Carrier = null, BaseRate = 11m },
				},
				filterValues: new TestFilters(),
				expectedRates: Array.Empty<object>(),
				CarrierSelector);
		}

		#endregion

		#region By Carrier

		public void TestGroupingRates_ByCarrier_FallbackToStandardCostWithCarrier_ThereIsCarrierCost()
		{
			AssertNonContainerisedGetRates(
				message: "A Standard Cost can NOT be loaded when there is a Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null,  Carrier = "AAA", BaseRate = 12m },
					new TestRate { ServiceProvider = null,  Carrier = "BBB", BaseRate = 13m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  BaseRate = 21m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
				},
				CarrierSelector);
		}

		public void TestGroupingRates_ByCarrier_FallbackToStandardCostWithCarrier_ThereIsNotCarrierCost()
		{
			AssertNonContainerisedGetRates(
				message: "A Standard Cost can be loaded when there is NOT any Carrier Cost",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = null, Carrier = "AAA", BaseRate = 12m },
					new TestRate { ServiceProvider = null, Carrier = "BBB", BaseRate = 13m },
				},
				filterValues: new TestFilters
				{
					ServiceProviders = new[] { "AAA" },
				},
				expectedRates: new[]
				{
					new { ServiceProvider = "", Carrier = "AAA", ChargeAmounts = new[] { 12m } },
				},
				CarrierSelector);
		}

		#endregion

		#region By FCL container

		public void TestGetRatesByFCLEmptyContainer_NotContainerised()
		{
			var rateMode = FreightMode.LSE;

			AssertNonContainerisedGetRates(
				message: "FCL Costs without container type not containerised",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m },
				},
				filterValues: new TestFilters
				{
					Mode = rateMode,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 15m } },
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCL_EmptyContainerAndLD7_NotContainerised()
		{
			var rateMode = FreightMode.LSE;

			AssertNonContainerisedGetRates(
				message: "FCL Costs not containerised with and without container type should still show",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m },
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, ContainerType = "LD-7" },
				},
				filterValues: new TestFilters
				{
					Mode = rateMode,
				},
				expectedRates: new[]
				{
					new { ChargeAmounts = new[] { 15m } },
				},
				EmptyContainerCodeSelector);
		}

		public void TestGetRatesByFCL_LD7LD8_NotContainerised()
		{
			var rateMode = FreightMode.LSE;

			AssertNonContainerisedGetRates(
				message: "FCL Costs not containerised with container type LD-7 and LD-8 should still show",
				existingRates: new[]
				{
					new TestRate { ServiceProvider = "AAA", BaseRate = 15m, ContainerType = "LD-8" },
					new TestRate { ServiceProvider = "AAA", BaseRate = 21m, ContainerType = "LD-7" },
				},
				filterValues: new TestFilters
				{
					Mode = rateMode,
				},
				expectedRates: Array.Empty<object>(),
				EmptyContainerCodeSelector);
		}

		#endregion

		#region Based on Priorities

		#region CarrierServiceLevel over Carrier

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriority()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertNonContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "",    ChargeCode = "BAF", BaseRate = 33m },
						new TestRate { ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
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
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriority_FallbackToCarrier()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertNonContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 44m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA", "BBB", "CCC", "DDD" },
						CarrierServiceLevels = new[] { "STD", "EXP" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 22m } },

						new { ServiceProvider = "CCC", Carrier = "", CarrierServiceLevel = "STD", ChargeAmounts = new[] { 33m } },
						new { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "EXP", ChargeAmounts = new[] { 44m } },
					},
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenCarrierServiceLevelTakesPriority_ShouldNotLoadLessSpecificRateSeparately()
		{
			using (TemporaryPrioritizeServiceLevel())
			{
				AssertNonContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "EXP", ChargeCode = "BAF", BaseRate = 22m },
						new TestRate { ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 33m },
						new TestRate { ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "CAF", BaseRate = 44m },
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 55m },
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "WAR", BaseRate = 66m },
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
					AllFieldsSelector);
			}
		}

		IDisposable TemporaryPrioritizeServiceLevel()
		{
			var originalValue = Env.Registry.Rating.FreightSearchPriorities;

			return new DisposableAction(
				() => Env.Registry.Rating.FreightSearchPriorities = "TI_RS_NKServiceLevel_NI,TI_OH_TransportProvider,TI_RH_NKCommodityCode.Name,TI_ViaLRC,TI_HBLDeliveryMode",
				() => Env.Registry.Rating.FreightSearchPriorities = originalValue);
		}

		#endregion

		#region Carrier over CarrierServiceLevel

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriority()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertNonContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ServiceProvider = "CCC", Carrier = "DDD", CarrierServiceLevel = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ServiceProvider = "CCC", Carrier = null,  CarrierServiceLevel = "STD", ChargeCode = "BAF", BaseRate = 44m },
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
					AllFieldsSelector);
			}
		}

		public void TestGetRates_BasedOnPriorities_WhenTransportProviderTakesPriority_FallbackToCarrierServiceLevel()
		{
			using (TemporaryPrioritizeTransportProvider())
			{
				AssertNonContainerisedGetRates(
					message: "",
					existingRates: new[]
					{
						// With the same Charge Code
						new TestRate { ServiceProvider = "AAA", Carrier = "BBB", CarrierServiceLevel = "",    ChargeCode = "FRT", BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", Carrier = "CCC", CarrierServiceLevel = "STD", ChargeCode = "FRT", BaseRate = 22m },
						// With different Charge Codes
						new TestRate { ServiceProvider = "DDD", Carrier = "EEE", CarrierServiceLevel = "",    ChargeCode = "CAF", BaseRate = 33m },
						new TestRate { ServiceProvider = "DDD", Carrier = "FFF", CarrierServiceLevel = "STD", ChargeCode = "BAF", BaseRate = 44m },
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
					AllFieldsSelector);
			}
		}

		IDisposable TemporaryPrioritizeTransportProvider()
		{
			var originalValue = Env.Registry.Rating.FreightSearchPriorities;

			return new DisposableAction(
				() => Env.Registry.Rating.FreightSearchPriorities = "TI_OH_TransportProvider,TI_RS_NKServiceLevel_NI,TI_RH_NKCommodityCode.Name,TI_ViaLRC,TI_HBLDeliveryMode",
				() => Env.Registry.Rating.FreightSearchPriorities = originalValue);
		}

		#endregion

		#region ContractNumber over Carrier

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_WithSpecificFilterValue()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN2", ChargeCode = "CAF", BaseRate = 44m },
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
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_WithEmptyFilter()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "",    ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN2", ChargeCode = "CAF", BaseRate = 44m },
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
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrier_FallbackToCarrierAtTheSameContractNumberPriorityLevel()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code & same Contract Number
					new TestRate { ServiceProvider = "AAA", Carrier = "BBB", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", Carrier = null,  ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With the same Charge Code & different Contract Numbers
					new TestRate { ServiceProvider = "CCC", Carrier = "DDD", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "CCC", Carrier = null,  ContractNumber = "CN3", ChargeCode = "FRT", BaseRate = 44m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "EEE", Carrier = "FFF", ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ServiceProvider = "EEE", Carrier = "FFF", ContractNumber = "CN4", ChargeCode = "BAF", BaseRate = 66m },
					new TestRate { ServiceProvider = "EEE", Carrier = null,  ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ServiceProvider = "EEE", Carrier = null,  ContractNumber = "CN4", ChargeCode = "CAF", BaseRate = 88m },
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
				AllFieldsSelector);
		}

		#endregion

		#region ContractNumber over CarrierServiceLevel

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_WithSpecificFilterValue()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
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
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_WithEmptyFilter()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "",    ChargeCode = "FRT", BaseRate = 22m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN2", ChargeCode = "BAF", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "EXP", ContractNumber = "",    ChargeCode = "CAF", BaseRate = 44m },
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
				AllFieldsSelector);
		}

		public void TestGetRates_BasedOnPriorities_ContractNumberAlwaysTakesPriorityOverCarrierServiceLevel_FallbackToCarrierServiceLevelAtTheSameContractNumberPriorityLevel()
		{
			AssertNonContainerisedGetRates(
				message: "",
				existingRates: new[]
				{
					// With the same Charge Code & same Contract Number
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "STD", ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 11m },
					new TestRate { ServiceProvider = "AAA", CarrierServiceLevel = "",    ContractNumber = "CN1", ChargeCode = "FRT", BaseRate = 22m },
					// With the same Charge Code & different Contract Numbers
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "STD", ContractNumber = "CN2", ChargeCode = "FRT", BaseRate = 33m },
					new TestRate { ServiceProvider = "BBB", CarrierServiceLevel = "",    ContractNumber = "CN3", ChargeCode = "FRT", BaseRate = 44m },
					// With different Charge Codes
					new TestRate { ServiceProvider = "CCC", CarrierServiceLevel = "STD", ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 55m },
					new TestRate { ServiceProvider = "CCC", CarrierServiceLevel = "STD", ContractNumber = "CN4", ChargeCode = "BAF", BaseRate = 66m },
					new TestRate { ServiceProvider = "CCC", CarrierServiceLevel = "",    ContractNumber = "CN4", ChargeCode = "FRT", BaseRate = 77m },
					new TestRate { ServiceProvider = "CCC", CarrierServiceLevel = "",    ContractNumber = "CN4", ChargeCode = "CAF", BaseRate = 88m },
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
				AllFieldsSelector);
		}

		#endregion

		#endregion

		#region Debug Log

		public void TestGetRates_WithDebugLog()
		{
			using (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = AssertNonContainerisedGetRates(
					existingRates: new[]
					{
						new TestRate { ServiceProvider = null,  BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", BaseRate = 21m },
						new TestRate { ServiceProvider = "BBB", BaseRate = 31m },
						new TestRate { ServiceProvider = "CCC", BaseRate = 41m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
					},
					CarrierSelector);

				Assert(logger.Logs.Any(log => log.Level == LogType.Debug));
			}
		}

		public void TestGetRates_WithNoDebugLog()
		{
			using (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = AssertNonContainerisedGetRates(
					existingRates: new[]
					{
						new TestRate { ServiceProvider = null,  BaseRate = 11m },
						new TestRate { ServiceProvider = "AAA", BaseRate = 21m },
						new TestRate { ServiceProvider = "BBB", BaseRate = 31m },
						new TestRate { ServiceProvider = "CCC", BaseRate = 41m },
					},
					filterValues: new TestFilters
					{
						ServiceProviders = new[] { "AAA" },
					},
					expectedRates: new[]
					{
						new { ServiceProvider = "AAA", Carrier = "", ChargeAmounts = new[] { 21m } },
					},
					CarrierSelector);

				Assert(logger.Logs.All(log => log.Level != LogType.Debug));
			}
		}

		#endregion

		#region GetRates Assertion

		MemoryLogger AssertNonContainerisedGetRates(
			IEnumerable<TestRate> existingRates,
			TestFilters filterValues,
			IEnumerable<object> expectedRates,
			Func<CW1RateViewModel, object> propertiesToCompare = null)
		{
			return AssertNonContainerisedGetRates(
				message: "",
				existingRates: existingRates,
				filterValues: filterValues,
				expectedRates: expectedRates,
				propertiesToCompare: propertiesToCompare);
		}

		MemoryLogger AssertNonContainerisedGetRates(
			string message,
			IEnumerable<TestRate> existingRates,
			TestFilters filterValues,
			IEnumerable<object> expectedRates,
			Func<CW1RateViewModel, object> propertiesToCompare = null)
		{
			return AssertGetRatesCore(
				message: message,
				existingRates: existingRates,
				filterValues: filterValues,
				expectedRates: expectedRates,
				propertiesToCompare: propertiesToCompare);
		}

		#endregion
	}
}
