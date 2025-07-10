using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using DTO = WiseRates.Api.Model;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class CargoguideRateViewModelTest : RateViewModelTest
	{
		public void TestBasicFields()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ContractNumber = "666";
			rate.Commodity = "Humans";
			rate.StartDate = new DateTime(2020, 01, 01);
			rate.ExpiryDate = new DateTime(2021, 06, 06);
			rate.PaymentTerm = "PPD";

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("Incorrect ContractNumber", "666", viewModel.ContractNumber);
			AssertEquals("Incorrect ContractOrReference", "Contract & Ref. 666", viewModel.ContractOrReference);
			AssertCollectionContains("CommodityGroups does not contain 'Humans'", "Humans", viewModel.CommodityGroups);
			AssertEquals("Incorrect StartDate", new DateTime(2020, 01, 01), viewModel.StartDate);
			AssertEquals("Incorrect ExpiryDate", new DateTime(2021, 06, 06), viewModel.ExpiryDate);
			AssertEquals("Incorrect PaymentTerms", "PPD", viewModel.PaymentTerms);
		}

		public void TestProperty_CGReference()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];

			rate.ProviderCustomFields = new[] { new CustomField { Code = Rate.CustomFields.Cargoguide.Reference, Value = "REF1", Description = "Reference" } };

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("The reference value should match", "REF1", viewModel.Reference);
			AssertEquals("The contract or reference should match", "Ref. REF1", viewModel.ContractOrReference);
			AssertNullOrEmpty("ContractNumber", viewModel.ContractNumber);
		}

		public void TestRouting()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("UAIEV > AUSYD", viewModel.Routing);

			rate.Via = "SGSIN";
			viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("UAIEV > SGSIN > AUSYD", viewModel.Routing);
		}

		#region Carrier

		public void TestCarrier_MappingDoesntExist_PopulateUniversalCode()
		{
			var response = GetValidResponse();
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[]
			{
				new RefCarrier
				{
					Code = "QANTAS",
					Name = "Qantas",
					IATACode = "QF"
				}
			};

			var viewModel = new CargoguideRateViewModel(response.Rates[0], GetContext(response));
			AssertNullOrEmpty("viewModel.CarrierCode", viewModel.CarrierCode);
			AssertEquals("Expected CarrierName to match.", "Qantas", viewModel.CarrierName);
			AssertEquals("Expected CarrierErrorLevel to be Warning.", ErrorLevel.Warning, viewModel.CarrierErrorLevel);
			AssertEquals(
				"Expected CarrierError to match the specified message.",
				@"No single Carrier is assigned with the SCAC, IATA or C1C Code of the Carrier from Rates Service:
{
  ""Code"": ""QANTAS"",
  ""Name"": ""Qantas"",
  ""IATACode"": ""QF""
}",
				viewModel.CarrierError
			);
		}

		public void TestCarrier_MappingExist_PopulateMappedCarrier()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_TwoCharacterCode = "EK";

			var carrier = TransportProvider1;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.OH_FullName = "Emirates Airlines";
			carrier.OH_IsActive = true;

			Factory.Save();

			var response = GetValidResponse();
			response.Rates[0].Carrier = "WW_EMIR";
			response.Carriers = new[]
			{
				new RefCarrier
				{
					Code = "WW_EMIR",
					Name = "Emirates",
					IATACode = "EK"
				}
			};

			var viewModel = new CargoguideRateViewModel(response.Rates[0], GetContext(response));
			AssertEquals("Carrier codes should match.", carrier.OH_Code, viewModel.CarrierCode);
			AssertEquals("Carrier names should match.", "Emirates Airlines", viewModel.CarrierName);
			AssertEquals("Error level should be None.", ErrorLevel.None, viewModel.CarrierErrorLevel);
			AssertNullOrEmpty("CarrierError should be null or empty.", viewModel.CarrierError);
		}

		#endregion

		#region Service Level

		public void TestCarrierServiceLevel_RateHasNoServiceLevel_PopulateEmptyString()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ServiceLevel = null;

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));

			AssertNullOrEmpty(nameof(viewModel.CarrierServiceLevel), viewModel.CarrierServiceLevel);
			AssertEquals("Carrier service level error level should be None", ErrorLevel.None, viewModel.CarrierServiceLevelErrorLevel);
			AssertNullOrEmpty(nameof(viewModel.CarrierServiceLevelError), viewModel.CarrierServiceLevelError);
		}

		public void TestCarrierServiceLevel_CarrierIsNotMapped_PopulateUniversalServiceLevelWithWarning()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ServiceLevel = "XXX";
			rate.Carrier = "QANTAS";

			response.Carriers = new[]
			{
				new RefCarrier
				{
					Code = "QANTAS",
					Name = "Qantas Airlines",
					IATACode = "QF"
				}
			};

			response.ServiceLevels = new[]
			{
				new DTO.RefServiceLevel()
				{
					Code = "XXX",
					Description = "Some service level"
				}
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));

			AssertEquals(
				"Expected RawCarrierServiceLevel to be equivalent to the provided response service level.",
				response.ServiceLevels[0],
				viewModel.RawCarrierServiceLevel
			);

			AssertEquals(
				"Expected CarrierServiceLevel to match the service level code 'XXX'.",
				"XXX",
				viewModel.CarrierServiceLevel
			);

			AssertEquals(
				"Expected CarrierServiceLevelErrorLevel to be set to 'Warning' when carrier mapping fails.",
				ErrorLevel.Warning,
				viewModel.CarrierServiceLevelErrorLevel
			);

			AssertEquals(
				"Expected CarrierServiceLevelError to describe mapping failure due to inability to determine the carrier.",
				"Unable to map Carrier Service Level because Carrier cannot be determined for the rate",
				viewModel.CarrierServiceLevelError
			);
		}

		public void TestCarrierServiceLevel_CarrierIsMappedButServiceLevelIsNotMapped_PopulateUniversalServiceLevelWithWarning()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline.RM_ThreeLetterCode = "QFF";
			airline.RM_TwoCharacterCode = "QF";

			var carrier = TransportProvider1;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.OH_FullName = "Qantas Airlines";
			carrier.OH_IsActive = true;

			Factory.Save();

			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ServiceLevel = "XXX";
			rate.Carrier = "QANTAS";

			response.Carriers = new[]
			{
				new RefCarrier
				{
					Code = "QANTAS",
					Name = "Qantas Airlines",
					IATACode = "QF"
				}
			};

			response.ServiceLevels = new[]
			{
				new DTO.RefServiceLevel()
				{
					Code = "XXX",
					Description = "Some service level"
				}
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));

			AssertContainsExactElementsInAnyOrder(
				"RawCarrierServiceLevel should match the first service level in the response.",
				new[] { response.ServiceLevels[0] },
				new[] { viewModel.RawCarrierServiceLevel }
			);

			AssertEquals(
				"CarrierServiceLevel should be 'XXX'.",
				"XXX",
				viewModel.CarrierServiceLevel
			);

			AssertEquals(
				"CarrierServiceLevelErrorLevel should be a warning.",
				ErrorLevel.Warning,
				viewModel.CarrierServiceLevelErrorLevel
			);

			AssertEquals(
				$"CarrierServiceLevelError should contain the proper error message.",
				$"No Carrier Service Level under Carrier '{carrier.OH_Code}' is assigned to 'XXX'",
				viewModel.CarrierServiceLevelError
			);
		}

		public void TestCarrierServiceLevel_CarrierIsMappedAndServiceLevelIsMapped_PopulateMappedServiceLevel()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline.RM_ThreeLetterCode = "QFF";
			airline.RM_TwoCharacterCode = "QF";

			var carrier = TransportProvider1;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.OH_FullName = "Qantas Airlines";
			carrier.OH_IsActive = true;

			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "EXP";
			serviceLevel.PL_CarrierServiceCode = "XXX";

			Factory.Save();

			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ServiceLevel = "XXX";
			rate.Carrier = "QANTAS";

			response.Carriers = new[]
			{
				new RefCarrier
				{
					Code = "QANTAS",
					Name = "Qantas Airlines",
					IATACode = "QF"
				}
			};

			response.ServiceLevels = new[]
			{
				new DTO.RefServiceLevel()
				{
					Code = "XXX",
					Description = "Some service level"
				}
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertContainsExactElementsInAnyOrder(
				"Ensure the raw carrier service level matches the expected service level",
				new[] { response.ServiceLevels[0] },
				new[] { viewModel.RawCarrierServiceLevel }
			);
			AssertEquals("Expected mapped carrier service level code to match", "EXP", viewModel.CarrierServiceLevel);
			AssertEquals("Expected no errors in carrier service level mapping", ErrorLevel.None, viewModel.CarrierServiceLevelErrorLevel);
			AssertNullOrEmpty("CarrierServiceLevelError should be null or empty", viewModel.CarrierServiceLevelError);
		}

		#endregion

		#region Container

		public void TestContainer()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Container = new DTO.RefContainer
			{
				Code = "RKN",
				PayloadWeight = 500m,
				PayloadVolume = 1.5m
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));

			AssertEquals("Container type should match", "RKN", viewModel.ContainerType);
			AssertEquals("Payload weight should match", "500 KG", viewModel.ContainerPayloadWeight);
			AssertEquals("Payload volume should match", "1.5 M3", viewModel.ContainerPayloadVolume);
		}

		public void TestContainerPivotWeight()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];

			rate.Container = null;
			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertNullOrEmpty("viewModel.ContainerPivotWeight", viewModel.ContainerPivotWeight);

			rate.Container = new DTO.RefContainer { Code = "20GP", ISOType = "20G0" };
			viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertNullOrEmpty("viewModel.ContainerPivotWeight", viewModel.ContainerPivotWeight);

			rate.Container.PivotWeight = 500m;
			viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("viewModel.ContainerPivotWeight should be '500 KG'", "500 KG", viewModel.ContainerPivotWeight);
		}

		#endregion

		#region Lines

		public void TestLines_PopulateLinesFromChargesOnTheRate()
		{
			InsertChargeCode(Factory, "TEST_FUL", "Test FUL", "UNT", "FRT", "U_FUL");
			InsertChargeCode(Factory, "TEST_CAF", "Test CAF", "UNT", "FRT", "U_CAF");

			Factory.Save();

			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ContainerMode = "FCL";
			rate.Charges = new List<Charge>(new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 100,
					Unit = "CN",
					Currency = "AUD"
				},
				new Charge
				{
					ChargeCode = "U_FUL",
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT"
				},
				new Charge
				{
					ChargeCode = "U_CAF",
					ChargeType = ChargeType.SubjectTo,
					PerUnitRate = 200,
					Unit = "CN",
					Currency = "UAH"
				},
				new Charge
				{
					ChargeCode = "U_WAR",
					ChargeType = ChargeType.Optional,
					FlatRate = 400,
					Currency = "USD"
				}
			});

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			var lines = viewModel.Lines;

			var frtLine = lines.Single(l => l.ChargeCode != null && l.ChargeCode.AC_Code == "FRT");
			AssertEquals("FRT line should have the correct currency", "AUD", frtLine.TL_RX_NKCurrency);
			AssertEquals("FRT line should have the correct per unit rate", 100m, frtLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals("FRT line should have the correct unit", "CN", frtLine.GetCalculator<CombinedCalculator>().Unit);

			var fulLine = lines.Single(l => l.ChargeCode != null && l.ChargeCode.AC_Code == "TEST_FUL");
			AssertType("FUL line should use the FreightInclusiveCalculator", typeof(FreightInclusiveCalculator), fulLine.Calculator);

			var cafLine = lines.Single(l => l.ChargeCode != null && l.ChargeCode.AC_Code == "TEST_CAF");
			AssertEquals("CAF line should have the correct currency", "UAH", cafLine.TL_RX_NKCurrency);
			AssertEquals("CAF line should have the correct per unit rate", 200m, cafLine.GetCalculator<CombinedCalculator>().PerUnit);
			AssertEquals("CAF line should have the correct unit", "CN", cafLine.GetCalculator<CombinedCalculator>().Unit);

			var warLine = lines.Single(l => l.ChargeCode == null);
			AssertEquals("WAR line should have the correct currency", "USD", warLine.TL_RX_NKCurrency);
			AssertEquals("WAR line should have the correct base rate", 400m, warLine.GetCalculator<CombinedCalculator>().BaseRate);
		}

		#endregion

		#region Charges

		public void TestCalculate_PopulateChargeCollections()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.ContainerMode = "FCL";
			rate.Charges = new List<Charge>(new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 100,
					Unit = "CN",
					Currency = "AUD"
				},
				new Charge
				{
					ChargeCode = "FSC",
					ChargeType = ChargeType.Included
				},
				new Charge
				{
					ChargeCode = "BAF",
					ChargeType = ChargeType.Included
				},
				new Charge
				{
					ChargeCode = "CAF",
					ChargeType = ChargeType.SubjectTo,
					PerUnitRate = 20,
					Unit = "CN",
					Currency = "UAH"
				},
				new Charge
				{
					ChargeCode = "WAR",
					ChargeType = ChargeType.Optional,
					PerUnitRate = 40,
					Unit = "CN",
					Currency = "USD"
				}
			});

			var filters = GetValidFilters();
			filters.OriginalCriteria.RateableMeasures.AddContainerGroup(Helper.Containers["20GP"].PK, new[]
			{
				new MeasureInfo.ContainerInfo(containerCount: 1),
				new MeasureInfo.ContainerInfo(containerCount: 1),
				new MeasureInfo.ContainerInfo(containerCount: 1),
			});

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			viewModel.Calculate(
				filters.CreateCriteria(),
				filters.OriginalCriteria.Creditors?.ChargeCodeGroups);

			var expectedFreightCharges = new ChargeViewModel[]
			{
				new ChargeViewModelSample { ChargeCode = "FRT", Amount = 300, Currency = "AUD" },
				new ChargeViewModelSample { ChargeCode = "FSC", IsIncluded = true, Currency = null },
				new ChargeViewModelSample { ChargeCode = "BAF", IsIncluded = true, Currency = null },
			};

			var expectedSubjectToCharges = new ChargeViewModel[]
			{
				new ChargeViewModelSample { ChargeCode = "CAF", Amount = 60, Currency = "UAH" },
			};

			var expectedOptionalCharges = new ChargeViewModel[]
			{
				new ChargeViewModelSample { ChargeCode = "WAR", Amount = 120, Currency = "USD", IsOptional = true },
			};

			AssertEquals("The total number of charge groups should be 3.", 3, viewModel.Charges.Count());

			var actualFreightCharges = viewModel.FreightCharges.Charges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			var expectedFreightChargesStrings = expectedFreightCharges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			AssertContainsExactElementsInAnyOrder("The freight charges should match the expected values.", expectedFreightChargesStrings, actualFreightCharges);

			var actualSubjectToCharges = viewModel.SubjectToCharges.Charges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			var expectedSubjectToChargesStrings = expectedSubjectToCharges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			AssertContainsExactElementsInAnyOrder("The subject-to charges should match the expected values.", expectedSubjectToChargesStrings, actualSubjectToCharges);

			var actualOptionalCharges = viewModel.OptionalCharges.Charges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			var expectedOptionalChargesStrings = expectedOptionalCharges
				.Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}|{c.IsIncluded}|{c.IsOptional}")
				.ToArray();
			AssertContainsExactElementsInAnyOrder("The optional charges should match the expected values.", expectedOptionalChargesStrings, actualOptionalCharges);
		}

		#endregion

		#region Commodity

		public void TestCommodity_RateHasNoUniversalCommodityGroup()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Commodity = null;
			rate.CarrierCommodityInfo = new CarrierSpecificCommodity
			{
				GroupName = "Humans",
				Code = "HUM",
				IncludedCommodities = new[] { "Person1", "Person2" }
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("Expected GroupName should be 'Humans'", "Humans", viewModel.Commodities);
			AssertEquals("Expected CommodityGroups to be empty", 0, viewModel.CommodityGroups.Count());
			AssertEquals("Expected CarrierCommodities to match", "Person1, Person2", viewModel.CarrierCommodities);
			AssertEquals("Expected CommodityGroupErrorLevel to be ErrorLevel.None", ErrorLevel.None, viewModel.CommodityGroupErrorLevel);
			AssertNullOrEmpty("Expected CommodityGroupError to be null or empty", viewModel.CommodityGroupError);
		}

		public void TestCommodity_RateUniversalCommodityGroupIsNotMapped()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Commodity = "XXX";
			rate.CarrierCommodityInfo = new CarrierSpecificCommodity
			{
				GroupName = "Humans",
				Code = "HUM",
				IncludedCommodities = new[] { "Person1", "Person2" }
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			AssertEquals("Expected 'Humans' as the commodity group name", "Humans", viewModel.Commodities);
			AssertEquals("Expected 'Person1, Person2' as the carrier commodities", "Person1, Person2", viewModel.CarrierCommodities);
			AssertEquals("Expected a single commodity group matching 'XXX'", "XXX", viewModel.CommodityGroups.Single());
			AssertEquals("Expected warning error level", ErrorLevel.Warning, viewModel.CommodityGroupErrorLevel);
			AssertEquals("Expected error message explaining the unassigned universal commodity group",
				"The Universal Commodity Group 'XXX' has NOT been assigned to any CW1 Commodity.",
				viewModel.CommodityGroupError);
		}

		public void TestCommodity_RateUniversalCommodityGroupIsMapped()
		{
			var query = new ZQuery(RefCommodityCodeSchema.RH_UniversalCommodityGroup, SQLComparisonOperator.IsBlank, "");
			var commodityCode = Factory.LoadTop1<RefCommodityCode>(query);
			commodityCode.RH_UniversalCommodityGroup = "XXX";
			Factory.Save();

			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Commodity = "XXX";
			rate.CarrierCommodityInfo = new CarrierSpecificCommodity
			{
				GroupName = "Humans",
				Code = "HUM",
				IncludedCommodities = new[] { "Person1", "Person2" }
			};

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));

			AssertEquals("The commodities should match the group name", "Humans", viewModel.Commodities);
			AssertEquals("The carrier commodities should match the included commodities", "Person1, Person2", viewModel.CarrierCommodities);
			AssertEquals("The single commodity group should match the rate commodity", "XXX", viewModel.CommodityGroups.Single());
			AssertEquals("The commodity group error level should be None", ErrorLevel.None, viewModel.CommodityGroupErrorLevel);
			AssertNullOrEmpty("The commodity group error should be null or empty", viewModel.CommodityGroupError);
		}

		#endregion

		#region GetAutoRateInfos

		public void TestGetAutoRateInfos_ReturnAutoRatesFromSelectedCharges()
		{
			InsertChargeCode(Factory, "TestFRT", "Freight", "FLT", "FRT", "U_FRT");
			InsertChargeCode(Factory, "TestFUL", "Freight", FreightInclusiveCalculator.Code, "FRT", "U_FUL");
			InsertChargeCode(Factory, "TestBAF", "Freight", FreightInclusiveCalculator.Code, "FRT", "U_BAF");
			InsertChargeCode(Factory, "TestCAF", "Freight", "FLT", "FRT", "U_CAF");
			InsertChargeCode(Factory, "TestWAR", "Freight", "FLT", "FRT", "U_WAR");

			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_TwoCharacterCode = "EK";

			var carrier = TransportProvider1;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = airline.PK;
			carrier.OH_FullName = "Emirates Airlines";
			carrier.OH_IsActive = true;

			Factory.Save();

			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.Charges = new List<Charge>(new[]
			{
				new Charge
				{
					ChargeCode = "U_FRT",
					PerUnitRate = 100,
					Unit = "KG",
					Currency = "AUD"
				},
				new Charge
				{
					ChargeCode = "U_FUL",
					ChargeType = ChargeType.Included,
					Currency = "AUD",
					FreightInclusiveCarriageCharge = "U_FRT"
				},
				new Charge
				{
					ChargeCode = "U_BAF",
					ChargeType = ChargeType.Included,
					Currency = "AUD",
					FreightInclusiveCarriageCharge = "U_FRT"
				},
				new Charge
				{
					ChargeCode = "U_CAF",
					ChargeType = ChargeType.SubjectTo,
					PerUnitRate = 20,
					Unit = "KG",
					Currency = "UAH"
				},
				new Charge
				{
					ChargeCode = "U_WAR",
					ChargeType = ChargeType.Optional,
					PerUnitRate = 40,
					Unit = "KG",
					Currency = "USD"
				}
			});

			var filters = GetValidFilters();
			filters.OriginalCriteria.RateableMeasures.AddWeightAndVolumeWithCommodityAndPackageType(100m, null, string.Empty, ZString.Empty);

			var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
			viewModel.Calculate(
				filters.CreateCriteria(),
				filters.OriginalCriteria.Creditors?.ChargeCodeGroups);

			viewModel.Charges.SelectMany(c => c.Charges).First(c => c.ChargeCode == "TestFUL").IsSelected = true;
			viewModel.Charges.SelectMany(c => c.Charges).First(c => c.ChargeCode == "TestBAF").IsSelected = true;
			viewModel.Charges.SelectMany(c => c.Charges).First(c => c.ChargeCode == "TestWAR").IsSelected = false;
			viewModel.Charges.SelectMany(c => c.Charges).First(c => c.ChargeCode == "TestCAF").IsSelected = true;

			var autoRates = viewModel.GetAutoRateInfos();
			var charges = autoRates.Select(a => a.ChargeCode.AC_Code.ToString()).ToArray();
			var expectedCharges = new[] { "TestFRT", "TestCAF", "TestFUL", "TestBAF" };

			AssertContainsExactElementsInAnyOrder(
				"The returned charges should match the selected charges",
				expectedCharges,
				charges
			);
		}

		#endregion

		public void TestRawRateJson_IncludeRawDataRegistrySettingIsTrue_PopulateJson()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
				AssertNullOrEmpty(viewModel.RawRateJson);
			}

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
				AssertEquals(rate.ToJsonSafe(Formatting.Indented), viewModel.RawRateJson);
			}
		}

		public void TestCargoguideRawRateJson_IncludeRawDataRegistrySettingIsTrue_PopulateCargoguideRateRateJson()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.RawRate = "Raw Rate Data Json".Compress();

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
				AssertNullOrEmpty(viewModel.CargoguideRawRateJson);
			}

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
				AssertEquals("Raw Rate Data Json", viewModel.CargoguideRawRateJson);
			}
		}

		public void TestCargoguideRawRateJson_RawRateIsInvalidGZipDataOnRate_ShouldPopulateEmptyStringAndDontCrash()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];
			rate.RawRate = "Not a base64 encoded gzip cargoguide raw rate";

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var viewModel = new CargoguideRateViewModel(rate, GetContext(response));
				AssertNullOrEmpty(
					"RawRate on the Rate is not a base64 encoded gzip data",
					viewModel.CargoguideRawRateJson
				);
			}
		}

		public void TestShowRawRateCommand_ShouldOpenRawRate()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];

			var dialogService = new Mock<IDialogService>();
			var context = GetContext(response);
			context.DialogService = dialogService.Object;

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var viewModel = new CargoguideRateViewModel(rate, context);
				viewModel.ShowCargoguideRawRateCommand();

				dialogService.Verify(s => s.ShowRawRate(It.IsAny<string>()), Times.Never);
			}

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var viewModel = new CargoguideRateViewModel(rate, context);
				viewModel.ShowRawRateCommand();

				dialogService.Verify(s => s.ShowRawRate(rate.ToJsonSafe(Formatting.Indented)));
			}

			Assert(true);
		}

		public void TestInvalidRate_ShouldReturnNullForNonSerializableRate()
		{
			var response = GetValidResponse();
			var rate = response.Rates[0];

			var cf = new CustomField();
			cf.Code = "temp";
			cf.Description = "temp desc";

			string json = @"{
  CPU: 'Intel',
  Temps: [
    '2',
    '25'
  ]
}";

			cf.Value = JObject.Parse(json);

			rate.ProviderCustomFields = new List<CustomField> { cf };
			AssertEquals("The YAML-safe conversion of a non-serializable rate should be an empty string.", string.Empty, rate.ToYAMLSafe());
		}

		public void TestRateSelectorCharges_WhenRateLineItemHasARestrictedItem_ShouldStillCompleteAutoRatingWithWarningMessage()
		{
			const string reasonForRestriction = "Some reason for restriction";

			var response = GetValidResponse();
			response.Rates = new[]
			{
				new Rate
				{
					Origin = "UAIEV",
					Destination = "AUSYD",
					Carrier = "EMIRATES",
					TransportMode = "AIR",
					ContainerMode = "LCL",
					Charges = new[]
					{
						new Charge
						{
							ChargeCode = "FRT",
							PerUnitRate = 0,
							Break = 1000,
							BreakOperator = ">=",
							Unit = "KG",
							Currency = "AUD",
							Restricted = true,
							Applicability = reasonForRestriction
						},
					}
				},
			};

			// populating charges should verify Charge.Restricted and add warning message accordingly
			var filters = GetValidFilters();
			filters.OriginalCriteria.RateableMeasures.AddContainerGroup(Helper.Containers["20GP"].PK, new[]
			{
				 new MeasureInfo.ContainerInfo(containerCount: 1),
				 new MeasureInfo.ContainerInfo(containerCount: 1),
				 new MeasureInfo.ContainerInfo(containerCount: 1),
			});

			var viewModel = new CargoguideRateViewModel(response.Rates[0], GetContext(response));
			var rateCriteria = filters.CreateCriteria();
			viewModel.Calculate(rateCriteria, filters.OriginalCriteria.Creditors?.ChargeCodeGroups);

			var charge = viewModel.FreightCharges.Charges.Single();
			charge.IsSelected = true;

			AssertEquals("Expected charge code to match.", "FRT", charge.ChargeCode);
			AssertEquals("Expected reason for restriction as charge code error.", "Some reason for restriction", charge.ChargeCodeError);
			AssertEquals("Expected error level to be warning.", ErrorLevel.Warning, charge.ChargeCodeErrorLevel);

			var autoRateInfo = viewModel.GetAutoRateInfos().Single();
			AssertEquals("Expected auto rate info to be from rates service.", true, autoRateInfo.IsFromRatesService);
			AssertEquals("Expected auto rate info to have a result.", true, autoRateInfo.HasResult);
			AssertEquals("Expected auto rate info amount to be zero.", ZDecimal.Zero, autoRateInfo.Amount);
		}

		public void TestRateSelectorCharges_WhenRateLineItemHasARestrictedItem_ShouldNotPopupWarningMessage()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			var cmbCalculator = rateLine.GetCalculator<CombinedCalculator>();
			cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 5m, 100);
			var restrictedItem = cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 5m, 0);
			restrictedItem.TM_CallForPricing = true;
			restrictedItem.TM_Text = "Call for Price";

			var criteria = new TestRatingCriteria();
			criteria.IsManualCostSelectMode = true;
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.ChargeableAmount = new Quantity(10, QuantityUnit.KG);

			Func<(IEnumerable<CalculationResult>, string)> result = () => rateLine.Calculator.Calculate(parameters);
			AssertNoExceptionThrown(() => result());
		}

		#region IsValid

		public void TestIsValid()
		{
			void AssertIsValid(ErrorLevel carrierErrorLevel, ErrorLevel serviceLevelErrorLevel, ErrorLevel chargeErrorLevel, ErrorLevel expectedErrorLevel)
			{
				var viewModel = new CargoguideRateViewModelSample();
				viewModel.CarrierErrorLevel = carrierErrorLevel;
				viewModel.CarrierServiceLevelErrorLevel = serviceLevelErrorLevel;
				viewModel.Charges.SelectMany(c => c.Charges).ForEach(c => c.ChargeCodeErrorLevel = chargeErrorLevel);

				AssertEquals("ErrorLevel should match the expected value.", expectedErrorLevel, viewModel.ErrorLevel);
				AssertEquals("IsValid should match the negation of whether ErrorLevel has the Error flag.",
					!expectedErrorLevel.HasFlag(ErrorLevel.Error),
					viewModel.IsValid);
			}

			AssertIsValid(ErrorLevel.None, ErrorLevel.None, ErrorLevel.None, expectedErrorLevel: ErrorLevel.None);
			AssertIsValid(ErrorLevel.Warning, ErrorLevel.None, ErrorLevel.None, expectedErrorLevel: ErrorLevel.Warning);
			AssertIsValid(ErrorLevel.None, ErrorLevel.Warning, ErrorLevel.None, expectedErrorLevel: ErrorLevel.Warning);
			AssertIsValid(ErrorLevel.Error, ErrorLevel.None, ErrorLevel.None, expectedErrorLevel: ErrorLevel.Error);
			AssertIsValid(ErrorLevel.None, ErrorLevel.Error, ErrorLevel.None, expectedErrorLevel: ErrorLevel.Error);
			AssertIsValid(ErrorLevel.None, ErrorLevel.None, ErrorLevel.Error, expectedErrorLevel: ErrorLevel.Error);
			AssertIsValid(ErrorLevel.Warning, ErrorLevel.Error, ErrorLevel.None, expectedErrorLevel: ErrorLevel.Warning | ErrorLevel.Error);
			AssertIsValid(ErrorLevel.None, ErrorLevel.Error, ErrorLevel.Warning, expectedErrorLevel: ErrorLevel.Warning | ErrorLevel.Error);
		}

		#endregion

		protected override RateViewModel GetInstance()
		{
			var response = GetValidResponse();
			var context = GetContext(response);

			var viewModel = new CargoguideRateViewModel(response.Rates[0], context);
			return viewModel;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "9XX";
			airline.RM_ThreeLetterCode = "EKK";
			airline.RM_TwoCharacterCode = "EK";

			TransportProvider1.OH_IsAirLine = true;
			TransportProvider1.MiscServ.OM_RM_Airline = airline.PK;
			TransportProvider1.OH_FullName = "Emirates Airlines";
			TransportProvider1.OH_IsActive = true;

			Factory.Save();
		}

		internal static RatesSearchResponse GetValidResponse(IList<Charge> charges = default, RefChargeCode[] chargeCodes = default)
		{
			return new RatesSearchResponse
			{
				Rates = new[]
				{
					new Rate
					{
						Origin = "UAIEV",
						Destination = "AUSYD",
						Carrier = "EMIRATES",
						TransportMode = "AIR",
						ContainerMode = "LCL",
						RatesServiceProvider = WRConstants.RateProviders.CargoGuide,
						Provider = WRConstants.RateProviders.CargoGuide,
						Charges = charges ?? new []
						{
							new Charge
							{
								ChargeCode = "FRT",
								Currency = "USD",
								FlatRate = 100
							}
						}
					}
				},
				Carriers = new[]
				{
					new RefCarrier
					{
						Code = "EMIRATES",
						Name = "Emirates",
						IATACode = "EK"
					}
				},
				ChargeCodes = chargeCodes ?? (new[]
					{
						new RefChargeCode
						{
							Code = "FRT",
							Group = "FRT",
							Description = "Freight"
						}
					})
			};
		}

		RateSelectorContext GetContext(RatesSearchResponse response)
		{
			return new RateSelectorContext
			{
				Factory = Factory,
				Filters = GetValidFilters(),
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory),
				RatesServiceResponse = response
			};
		}
	}
}
