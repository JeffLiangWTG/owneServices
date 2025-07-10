using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using WebModel = Enterprise.Rating.Web.Model;

namespace Enterprise.Rating.Web.Test
{
	public class RatesAPIsAutoRaterTest : RatingTestCase
	{
		public void TestAutoRate_NonContainerized_SellRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},

							new JobPackLine()
							{
								PackageType = "SHT",
								Unit = 128,
								Volume = 4.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 907m,
					Audit = "FRT: 128 Sheet(s) @ AUD 4.00/Sheet\n\tFRT: 79 Pallet(s) @ AUD 5.00/Pallet",
					Currency = "AUD",
					LocalAmount = 907m,
					LocalCurrency = "AUD",
					ExchangeRate = 1
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-PLT-Client Rate CONSIGNEE1",
				"RateLine Found FRT-UNT-SHT-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-PLT-Client Rate CONSIGNEE1
					Job's info:
					: 79 Unit
					: 128 Unit
				RateLine FRT-UNT-SHT-Client Rate CONSIGNEE1
					Job's info:
					: 79 Unit
					: 128 Unit"
			};

			SetupAndAssertRatesForNonContainerizedQuery(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedLogs);
		}

		public void TestAutoRate_NonContainerized_BuyRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},

							new JobPackLine()
							{
								PackageType = "SHT",
								Unit = 128,
								Volume = 4.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyRatesOnly;

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 493m,
					Audit = "FRT: 128 Sheet(s) @ AUD 2.00/Sheet\n\tFRT: 79 Pallet(s) @ AUD 3.00/Pallet",
					Currency = "AUD",
					LocalAmount = 493m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-PLT-Costing TRASPROV1",
				"RateLine Found FRT-UNT-SHT-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-PLT-Costing TRASPROV1
					Job's info:
					: 79 Unit
					: 128 Unit
				RateLine FRT-UNT-SHT-Costing TRASPROV1
					Job's info:
					: 79 Unit
					: 128 Unit"
			};

			SetupAndAssertRatesForNonContainerizedQuery(rateQuery, Array.Empty<CalculationItem>(), expectedCosts, expectedLogs);
		}

		public void TestAutoRate_NonContainerized_AllRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},

							new JobPackLine()
							{
								PackageType = "SHT",
								Unit = 128,
								Volume = 4.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 907m,
					Audit = "FRT: 128 Sheet(s) @ AUD 4.00/Sheet\n\tFRT: 79 Pallet(s) @ AUD 5.00/Pallet",
					Currency = "AUD",
					LocalAmount = 907m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 493m,
					Audit = "FRT: 128 Sheet(s) @ AUD 2.00/Sheet\n\tFRT: 79 Pallet(s) @ AUD 3.00/Pallet",
					Currency = "AUD",
					LocalAmount = 493m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-PLT-Costing TRASPROV1",
				"RateLine Found FRT-UNT-SHT-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-PLT-Costing TRASPROV1
					Job's info:
					: 79 Unit
					: 128 Unit
				RateLine FRT-UNT-SHT-Costing TRASPROV1
					Job's info:
					: 79 Unit
					: 128 Unit",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-PLT-Client Rate CONSIGNEE1",
				"RateLine Found FRT-UNT-SHT-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-PLT-Client Rate CONSIGNEE1
					Job's info:
					: 79 Unit
					: 128 Unit
				RateLine FRT-UNT-SHT-Client Rate CONSIGNEE1
					Job's info:
					: 79 Unit
					: 128 Unit"
			};

			SetupAndAssertRatesForNonContainerizedQuery(rateQuery, expectedRevenues, expectedCosts, expectedLogs);
		}

		public void TestAutoRate_Containerized_SellRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 880m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 220.00/Container",
					Currency = "AUD",
					LocalAmount = 880m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Client Rate CONSIGNEE1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)"
			};

			SetupAndAssertRatesForContainerizedQuery(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedLogs);
		}

		public void TestAutoRate_Containerized_BuyRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyRatesOnly;

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 1560m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 390.00/Container",
					Currency = "AUD",
					LocalAmount = 1560m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)",
			};

			SetupAndAssertRatesForContainerizedQuery(rateQuery, Array.Empty<CalculationItem>(), expectedCosts, expectedLogs);
		}

		public void TestAutoRate_Containerized_AllRates()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 880m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 220.00/Container",
					Currency = "AUD",
					LocalAmount = 880m,
					LocalCurrency = "AUD",
					ExchangeRate = 1
				}
			};

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 1560m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 390.00/Container",
					Currency = "AUD",
					LocalAmount = 1560m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Client Rate CONSIGNEE1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)"
			};

			SetupAndAssertRatesForContainerizedQuery(rateQuery, expectedRevenues, expectedCosts, expectedLogs);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRate_Containerized_AllRates_ForeignCurrency()
		{
			GlbCompany.CurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5, 0);

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 880m,
					Audit = "FRT: 4 20GP Container(s) @ USD 220.00/Container",
					Currency = "USD",
					LocalAmount = 1362.23M,
					LocalCurrency = "AUD",
					ExchangeRate = 0.646M // CFX uplift on Company/Branch level will affect Exchange Rate directly => 0.68 - 5% = 0.646
				}
			};

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 1560m,
					Audit = "FRT: 4 20GP Container(s) @ USD 390.00/Container",
					Currency = "USD",
					LocalAmount = 2294.12M,
					LocalCurrency = "AUD",
					ExchangeRate = 0.68M // CFX Uplift on Company/Branch level does not have any effect on cost exchange rate
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Client Rate CONSIGNEE1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)"
			};

			SetupAndAssertRatesForContainerizedQuery(rateQuery, expectedRevenues, expectedCosts, expectedLogs, "USD", 0.68M);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRate_Containerized_AllRates_ForeignCurrency_NoExchangeRate()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 880M,
					Audit = "FRT: 4 20GP Container(s) @ HKD 220.00/Container",
					Currency = "HKD",
					LocalAmount = 0M,
					LocalCurrency = "AUD",
					ExchangeRate = 0M
				}
			};

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 1560m,
					Audit = "FRT: 4 20GP Container(s) @ HKD 390.00/Container",
					Currency = "HKD",
					LocalAmount = 0M,
					LocalCurrency = "AUD",
					ExchangeRate = 0M,
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Client Rate CONSIGNEE1",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Client Rate CONSIGNEE1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)"
			};

			SetupAndAssertRatesForContainerizedQuery(rateQuery, expectedRevenues, expectedCosts, expectedLogs, "HKD", 0.0M);
		}

		public void TestAutoRate_JobServices()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;

			AccChargeCode fumService = Helper.ChargeCodes.New("FUSC", "Fumigation Service Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);
			fumService.AC_IsAdhocServiceCharge = true;

			AccChargeCode cleaningService = Helper.ChargeCodes.New("CLSC", "Cleaning Service Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Cleaning);
			cleaningService.AC_IsAdhocServiceCharge = true;

			AccChargeCode tailgateService = Helper.ChargeCodes.New("TGSC", "Tailgate Service Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Tailgate);
			tailgateService.AC_IsAdhocServiceCharge = true;

			var contractor = Factory.New<OrgHeader>();
			contractor.OH_Code = "CT01";

			var serviceDate = DateTime.Now.Date;
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				JobServices = new[]
				{
					new WebModel.JobService()
					{
						Contractor = new Organisation { CWCode = "CT01" },
						Location = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" },
						Type =  Core.Constants.FreightServiceType.Codes.Fumigation,
						Booked = serviceDate.AddDays(-2),
						Completed = serviceDate,
						Count = 5,
						Duration = 500,
						Rate = 150m,
						RateCurrency = "AUD",
						MeasurementBasis = WebModel.JobService.MeasurementBases.Hour
					},
					new WebModel.JobService()
					{
						Contractor = new Organisation { CWCode = "CT01" },
						Location = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" },
						Type =  Core.Constants.FreightServiceType.Codes.Cleaning,
						Booked = serviceDate.AddDays(-2),
						Completed = serviceDate,
						Count = 5,
						Duration = 500,
						Rate = 150m,
						RateCurrency = "AUD",
						MeasurementBasis = WebModel.JobService.MeasurementBases.ServiceOccurrence,
					},
					new WebModel.JobService()
					{
						Contractor = new Organisation { CWCode = "CT01" },
						Location = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" },
						Type =  Core.Constants.FreightServiceType.Codes.Tailgate,
						Booked = serviceDate.AddDays(-2),
						Completed = serviceDate,
						Count = 5,
						Duration = 500,
						Rate = 150m,
						RateCurrency = "AUD",
						MeasurementBasis = WebModel.JobService.MeasurementBases.Container,
					}
				},
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedServiceCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 750M, // Calculation is based on Service Count which is 5
					Audit = "CLSC: 5 Origin Cleaning @ AUD 150.00/Origin Cleaning",
					Currency = "AUD",
					LocalAmount = 750M,
					LocalCurrency = "AUD",
					ExchangeRate = 1
				},
				new CalculationItem()
				{
					Amount = 6250.00M, // Calculation is based on Service hours which is 500 minutes 
					Audit = "FUSC: 41.6667 Hour(s) @ AUD 150.00/Hour",
					Currency = "AUD",
					LocalAmount = 6250.00M,
					LocalCurrency = "AUD",
					ExchangeRate = 1
				},
				new CalculationItem()
				{
					Amount = 600M, // Calculation is based on number of countainers which is 4
					Audit = "TGSC: 4 Container(s) @ AUD 150.00/Container",
					Currency = "AUD",
					LocalAmount = 600M,
					LocalCurrency = "AUD",
					ExchangeRate = 1
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RateLine Found CLSC-Job Cleaning Service",
				"RateLine Found FUSC-Job Fumigation Service",
				"RateLine Found TGSC-Job Tailgate Service"
			};

			AutoRateAndAssertResults(rateQuery, null, expectedServiceCosts, expectedLogs);
		}

		public void TestAutoRate_CompanyTariffLevelOverride()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[]
			{
				new OrganisationRole() { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code }
			};
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 1
					}
				},
				CTLevelOverride = 2
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;

			var expectedCompanyTariffLevelOverrideRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 180m,
					Audit = "FRT: 1 20GP Container(s) @ AUD 180.00/Container",
					Currency = "AUD",
					LocalAmount = 180m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedBaseCompanyTariffLevelRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 200m,
					Audit = "FRT: 1 20GP Container(s) @ AUD 200.00/Container",
					Currency = "AUD",
					LocalAmount = 200m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedCosts = Array.Empty<CalculationItem>();

			var expectedCompanyTariffLevelOverrideLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"Company Tariff Level 2 Override in record",
				"RatingHeader Found Company Tariff Level 2 Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Company Tariff Level 2",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Company Tariff Level 2
					Job's info:
					Container 20GP: 1 ContainerCount"
			};

			var expectedBaseCompanyTariffLevelLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Base Company Tariff Entries: 1",
				"RateLine Found FRT-UNT-CN-20GP-Base Company Tariff",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Base Company Tariff
					Job's info:
					Container 20GP: 1 ContainerCount"
			};
			var tariff1 = Factory.New<CompanyTariff>();
			var clientEntry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "STD", "20GP", "", true);
			var clientLine1 = clientEntry1.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			clientLine1.GetCalculator<UnitCalculator>().PerUnit = 200m;

			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.FCL, 10m);

			Factory.Save();

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoRateAndAssertResults(rateQuery, expectedCompanyTariffLevelOverrideRevenues, expectedCosts, expectedCompanyTariffLevelOverrideLogs);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutoRateAndAssertResults(rateQuery, expectedBaseCompanyTariffLevelRevenues, expectedCosts, expectedBaseCompanyTariffLevelLogs);
			}
		}

		public void TestAutoRate_ServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;
			rateQuery.ServiceLevels = new string[] { "SVL", };
			rateQuery.CarrierServiceLevels = new[] { new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "CSL" } };

			var expectedRevenues = new[]
			{
				new CalculationItem()
				{
					Amount = 960m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 240.00/Container",
					Currency = "AUD",
					LocalAmount = 960m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedCosts = new[]
			{
				new CalculationItem()
				{
					Amount = 1560m,
					Audit = "FRT: 4 20GP Container(s) @ AUD 390.00/Container",
					Currency = "AUD",
					LocalAmount = 1560m,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			};

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Costing TRASPROV1 Entries: 2",
				"RateLine Found FRT-UNT-CN-20GP-Costing TRASPROV1 (x2)",
				"RateLine Filtered FRT-UNT-CN-20GP-Costing TRASPROV1	reason:	overridden by FRT-UNT-CN-20GP-Costing TRASPROV1 by TI_PL_NKCarrierServiceLevel comparer",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing TRASPROV1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"RateEntry Filtered Client Rate CONSIGNEE1 reason: Service Level didn't match job STD,SVL.",
				"RateLine Found FRT-UNT-CN-20GP-Client Rate CONSIGNEE1 (x2)",
				"RateLine Filtered FRT-UNT-CN-20GP-Client Rate CONSIGNEE1	reason:	overridden by FRT-UNT-CN-20GP-Client Rate CONSIGNEE1 by TI_RS_NKServiceLevel_NI comparer",
@"Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Client Rate CONSIGNEE1
					Job's info:
					Container 20GP: 1 ContainerCount (x4)"
			};

			var clientRate = Helper.NewClientRate(Consignee);
			var clientEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "STD", "20GP", "", true);
			var clientLine1 = clientEntry1.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			clientLine1.GetCalculator<UnitCalculator>().PerUnit = 220m;

			var clientEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "SVL", "20GP", "", true);
			var clientLine2 = clientEntry2.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			clientLine2.GetCalculator<UnitCalculator>().PerUnit = 240m;

			var clientEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP", "", true);
			var clientLine3 = clientEntry3.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			clientLine3.GetCalculator<UnitCalculator>().PerUnit = 250m;

			var costingRate = Helper.NewCosting(TransportProvider1);
			var costingEntry1 = costingRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP", "", true);
			costingEntry1.TI_PL_NKCarrierServiceLevel = "CSL";
			var costingLine1 = costingEntry1.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			costingLine1.GetCalculator<UnitCalculator>().PerUnit = 390m;

			var costingEntry2 = costingRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP", "", true);
			costingEntry2.TI_PL_NKCarrierServiceLevel = "STD";
			var costingLine2 = costingEntry2.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			costingLine2.GetCalculator<UnitCalculator>().PerUnit = 380m;

			var costingEntry3 = costingRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP", "", true);
			costingEntry3.TI_PL_NKCarrierServiceLevel = "";
			var costingLine3 = costingEntry3.AddRateLine("FRT", UnitCalculator.Code, "CN", "AUD");
			costingLine3.GetCalculator<UnitCalculator>().PerUnit = 370m;

			Factory.Save();

			AutoRateAndAssertResults(rateQuery, expectedRevenues, expectedCosts, expectedLogs);
		}

		public void TestAutoRate_PickupAddress_Match()
		{
			Consignor.Addresses[0].Postcode = "2000";

			TestAutoratePickupAddress(
				Consignor.OH_Code,
				Consignor.Addresses[0].AddressCode,
				new[]
				{
					"RateLine Found OCART-CTZ-KM-Client Rate CONSIGNOR1",
					"RateLine Found ONOTE-FLT-Client Rate CONSIGNOR1",
					"Chargeable was added for\r\n\t\t\t\tRateLine OCART-CTZ-KM-Client Rate CONSIGNOR1\r\n\t\t\t\t\tJob's info:\r\n\t\t\t\t\tContainer 20GP: 79 Unit",
					"Matched 'Standard' for RateLine OCART-CTZ-KM-Client Rate CONSIGNOR1\r\n\t- 'Standard' fall back used",
					"OCART-CTZ-KM-Client Rate CONSIGNOR1 10.751 KM\r\n\t- Pickup Distance: empty\r\n\t- Distance Calculation Service: empty (registry disabled)\r\n\t- Lat/Long Postcode Distance: 10.751 (Consignor Pickup Address to CTO/Wharf)",
					"OCART-CTZ-KM-Client Rate CONSIGNOR1 10.751 KM\r\n\t- Pickup Distance: empty\r\n\t- Distance Calculation Service: empty (registry disabled)\r\n\t- Lat/Long Postcode Distance: 10.751 (Consignor Pickup Address to CTO/Wharf)"
				},
				shouldMatch: true);
		}

		public void TestAutorate_PickupAddress_Mismatch()
		{
			Consignor.Addresses[0].Postcode = "0000";

			TestAutoratePickupAddress(
				Consignor.OH_Code,
				Consignor.Addresses[0].AddressCode,
				new[] { "RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2000,0000." },
				shouldMatch: false);
		}

		public void TestAutoRate_PickupOrg_Missing()
		{
			TestAutoratePickupAddress(
				"DOESNOTEXIST",
				"DOESNOTEXIST",
				new[] { "RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2000,." },
				shouldMatch: false);

			var message = ExtraValidationLogger.GetExtraValidationMessages();
			AssertCollectionContains("Provided RateQuery.PickupOrg (DOESNOTEXIST) is not a valid organisation in CW1.", message);
		}

		public void TestAutoRate_PickupAddrCode_Missing()
		{
			TestAutoratePickupAddress(
				Consignor.OH_Code,
				"DOESNOTEXIST",
				new[] { "RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2000,." },
				shouldMatch: false);

			var message = ExtraValidationLogger.GetExtraValidationMessages();
			AssertCollectionContains("Provided RateQuery.PickupAddrCode (DOESNOTEXIST) is not a recognised address for this organisation.", message);
		}

		void TestAutoratePickupAddress(string pickupOrg, string pickupAddrCode, IEnumerable<string> expectedRatingLogs, bool shouldMatch)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignor.OH_Code },
				new OrganisationRole { Role = OrganisationRole.Roles.DCTO, Code = WharfCTO.OH_Code }
			};
			rateQuery.Carriers = new[]
			{
				new Organisation { CWCode = TransportProvider1.OH_Code }
			};

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			rateQuery.PickupOrg = pickupOrg;
			rateQuery.PickupAddrCode = pickupAddrCode;

			var expectedRevenues = Array.Empty<CalculationItem>();
			var expectedCosts = Array.Empty<CalculationItem>();
			var expectedCommonRatingLogs = new List<string>
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNOR1 Entries: 1",
			};

			if (shouldMatch)
			{
				expectedRevenues = new[]
				{
					new CalculationItem
					{
						Amount = 1111M,
						Audit = "ONOTE: Base Rate AUD 1111.00",
						Currency = "AUD",
						LocalAmount = 1111m,
						LocalCurrency = "AUD",
						ExchangeRate = 1,
					},
					new CalculationItem
					{
						Amount = 107.51M,
						Audit = "OCART: 10.751 Kilometer(s) @ USD 10.00/Kilometer",
						Currency = "USD",
						LocalAmount = 107.51M,
						LocalCurrency = "AUD",
						ExchangeRate = 1,
					}
				};
			}

			var clientRate = Helper.NewClientRate(Consignor);

			var pickupPostcodeRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU");
			pickupPostcodeRateEntry.TI_CartagePickupAddressPostCode = "2000";

			pickupPostcodeRateEntry.RateLines.RemoveAndDeleteAll();
			var newEntryRateLine1 = pickupPostcodeRateEntry.AddRateLine("ONOTE", FlatCalculator.Code);
			newEntryRateLine1.GetCalculator<FlatCalculator>().BaseRate = 1111m;

			var ocartRateLine = pickupPostcodeRateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KM);
			var ocartCalc = ocartRateLine.GetCalculator<CartageZoneDistanceCalculator>();
			ocartCalc.EquipmentType = EquipmentNeeded.Any;
			ocartCalc.AddRateLineItem(Calculator.Items.Operator.UNT, 1, 10m, 0m);

			newEntryRateLine1.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			AutoRateAndAssertResults(rateQuery, expectedRevenues, expectedCosts, expectedCommonRatingLogs.Concat(expectedRatingLogs));
		}

		public void TestAutorate_PickupAddress_PostcodeOverrideMatchRateEntry()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 200);
			rateEntry1.TI_CartagePickupAddressPostCode = "2000";

			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 300);
			rateEntry2.TI_CartagePickupAddressPostCode = "3000";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;
			rateQuery.PickupPostcode = "3000";
			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code },
			};

			var expectedRatingLogs = new List<string>
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 2",
				"RateEntry Filtered Client Rate CONSIGNEE1 reason: From Postcode didn't match job 2000,3000.",
				"RateLine Found OCART-FLT-Client Rate CONSIGNEE1"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "OCART: Base Rate AUD 300.00",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutorate_PickupAddress_PostcodeOverride_CTZCalculator()
		{
			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var melPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "3000"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignee, "AU", cityTown: null);
			var zoneSydney = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			zoneSydney.CreateRateTransportZoneItemForTest(sydPostCode);
			var zoneMelbourne = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			zoneMelbourne.CreateRateTransportZoneItemForTest(melPostCode);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20, zoneSydney.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30, zoneMelbourne.PK);

			Factory.Save();

			var rateQuery = GetValidRateQueryForTestingWithCTZCalculator(origin: "AUSYD", destination: "USLAX", weight: 10, weightUnit: "KG");
			rateQuery.PickupPostcode = "3000";

			var expectedRatingLogs = new[]
			{
				@"Matched 'Mel Zone' for RateLine OCART-CTZ-KG-Client Rate CONSIGNEE1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignor Pickup/Delivery Address fallback"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "OCART: 10 Kilogram(s) @ AUD 30.00/KG",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutorate_PickupAddress_CityOverride_CTZCalculator()
		{
			var citySydney = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			var cityMelbourne = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Melbourne"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignee, "AU", cityTown: null);
			var zoneSydney = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			var zoneItemSydney = zoneSydney.CreateRateTransportZoneItemForTest(citySydney);
			// precondition for only city matching:
			zoneItemSydney.TQ_IsExcludingPostCode = true;
			var zoneMelbourne = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			var zoneItemMelbourne = zoneMelbourne.CreateRateTransportZoneItemForTest(cityMelbourne);
			// precondition for only city matching:
			zoneItemMelbourne.TQ_IsExcludingPostCode = true;

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20, zoneSydney.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30, zoneMelbourne.PK);

			Factory.Save();

			var rateQuery = GetValidRateQueryForTestingWithCTZCalculator(origin: "AUSYD", destination: "USLAX", weight: 10, weightUnit: "KG");
			rateQuery.PickupCity = "Melbourne";

			var expectedRatingLogs = new[]
			{
				@"Matched 'Mel Zone' for RateLine OCART-CTZ-KG-Client Rate CONSIGNEE1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignor Pickup/Delivery Address fallback"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "OCART: 10 Kilogram(s) @ AUD 30.00/KG",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutoRate_DeliveryAddress_Match()
		{
			Consignee.Addresses[0].Postcode = "1234";

			TestAutorateDeliveryAddress(
				Consignee.OH_Code,
				Consignee.Addresses[0].AddressCode,
				new[] { "RateLine Found DNOTE-FLT-Client Rate CONSIGNEE1" },
				shouldMatch: true);
		}

		public void TestAutoRate_DeliveryAddress_Mismatch()
		{
			Consignee.Addresses[0].Postcode = "0000";

			TestAutorateDeliveryAddress(
				Consignee.OH_Code,
				Consignee.Addresses[0].AddressCode,
				new[] { "RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 1234,0000." },
				shouldMatch: false);
		}

		public void TestAutoRate_DeliveryOrg_Missing()
		{
			TestAutorateDeliveryAddress(
				"DOESNOTEXIST",
				"DOESNOTEXIST",
				new[] { "RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 1234,." },
				shouldMatch: false);

			var message = ExtraValidationLogger.GetExtraValidationMessages();
			AssertCollectionContains("Provided RateQuery.DeliveryOrg (DOESNOTEXIST) is not a valid organisation in CW1.", message);
		}

		public void TestAutoRate_DeliveryAddrCode_Missing()
		{
			TestAutorateDeliveryAddress(
				Consignee.OH_Code,
				"DOESNOTEXIST",
				new[] { "RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 1234,." },
				shouldMatch: false);

			var message = ExtraValidationLogger.GetExtraValidationMessages();
			AssertCollectionContains("Provided RateQuery.DeliveryAddrCode (DOESNOTEXIST) is not a recognised address for this organisation.", message);
		}

		void TestAutorateDeliveryAddress(string deliveryOrg, string deliveryAddrCode, IEnumerable<string> expectedRatingLogs, bool shouldMatch)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.Incoterm = IncoTerms.DeliveredDutyPaid;

			rateQuery.RateParties = new[] { new OrganisationRole { Role = OrganisationRole.Roles.CNR, Code = Consignee.OH_Code } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				}
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			rateQuery.DeliveryOrg = deliveryOrg;
			rateQuery.DeliveryAddrCode = deliveryAddrCode;

			var expectedRevenues = Array.Empty<CalculationItem>();
			var expectedCosts = Array.Empty<CalculationItem>();
			var expectedRatingCommonLogs = new List<string>
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
			};

			if (shouldMatch)
			{
				expectedRevenues = new[]
				{
					new CalculationItem
					{
						Amount = 1111M,
						Audit = "DNOTE: Base Rate AUD 1111.00",
						Currency = "AUD",
						LocalAmount = 1111m,
						LocalCurrency = "AUD",
						ExchangeRate = 1,
					}
				};
			}

			var clientRate = Helper.NewClientRate(Consignee);

			var pickupPostcodeRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "US");
			pickupPostcodeRateEntry.TI_CartageDeliveryAddressPostCode = "1234";

			pickupPostcodeRateEntry.RateLines.RemoveAndDeleteAll();
			var newEntryRateLine1 = pickupPostcodeRateEntry.AddRateLine("DNOTE", FlatCalculator.Code);
			newEntryRateLine1.GetCalculator<FlatCalculator>().BaseRate = 1111m;
			newEntryRateLine1.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			AutoRateAndAssertResults(rateQuery, expectedRevenues, expectedCosts, expectedRatingCommonLogs.Concat(expectedRatingLogs));
		}

		public void TestAutoRate_GivenDomesticQuery_ShouldUseDomesticPaymentTerms()
		{
			// Registry -> AutoRating -> Charge Code Groups -> Freight Rated Charge Code Groups
			var freightRatedChargeCodeGroups = new List<string> { "ORG", "LOD", "UNL", "DST", "INS", "FRT" };

			// Domestic payment terms: PPD, CLT, C3P, FCD
			var allDomesticPaymentTermCodes = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms).GetAllCodes().Append("").Append(null);

			// The following are not applicable to the domestic payment term
			// "C3P" (Collect Third Party): ORG, LOD, DST, UNL
			var chargeCodeGroupsNotApplicableToC3P = new HashSet<string> { "ORG", "LOD", "UNL", "DST" };

			// Arrange
			////

			// For each of the freighted charge code groups, create a rate for
			// the consignor and the consignee
			var consignorRate = Helper.NewClientRate(Consignor);
			var consignorRateEntry = consignorRate.AddRateEntry("AIR", "LSE", "AU", "AU", removeLines: true);

			var consigneeRate = Helper.NewClientRate(Consignee);
			var consigneeRateEntry = consigneeRate.AddRateEntry("AIR", "LSE", "AU", "AU", removeLines: true);

			foreach (var chargeCodeGroup in freightRatedChargeCodeGroups)
			{
				var consignorCharge = Factory.NewWithValidTestData<AccChargeCode>();
				consignorCharge.AC_Code = $"{chargeCodeGroup}CNR";
				consignorCharge.AC_ChargeGroup = chargeCodeGroup;

				var consigneeCharge = Factory.NewWithValidTestData<AccChargeCode>();
				consigneeCharge.AC_Code = $"{chargeCodeGroup}CNE";
				consigneeCharge.AC_ChargeGroup = chargeCodeGroup;

				var consignorRateLine = consignorRateEntry.AddRateLine(consignorCharge, FlatCalculator.Code);
				consignorRateLine.GetCalculator<FlatCalculator>().BaseRate = 101;

				var consigneeRateLine = consigneeRateEntry.AddRateLine(consigneeCharge, FlatCalculator.Code);
				consigneeRateLine.GetCalculator<FlatCalculator>().BaseRate = 102;
			}

			// Adjust registry to ensure consignor / consignee are liable for all configured rates
			var consignorAndConsignee = new RatesPrioritiesCollection();
			var ratesPriority = consignorAndConsignee.AddNew().OrganizationType = nameof(RatingDebtorOrgTypes.CNR);
			ratesPriority = consignorAndConsignee.AddNew().OrganizationType = nameof(RatingDebtorOrgTypes.CNE);
			RatingDataRegistry.Instance.DomesticCollectPriorities.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consignorAndConsignee);
			RatingDataRegistry.Instance.DomesticPrepaidPriorities.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consignorAndConsignee);

			Factory.Save();

			foreach (var domesticPaymentTermCode in allDomesticPaymentTermCodes)
			{
				var rq = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
				rq.Origin.Value = "AUSYD";
				rq.Destination.Value = "AUMEL";
				rq.Incoterm = domesticPaymentTermCode;
				rq.RateParties = new[]
				{
					new OrganisationRole { Role = OrganisationRole.Roles.CNR, Code = Consignor.OH_Code },
					new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code },
				};

				var expectedConsignorRevenues = consignorRateEntry.ChildRateLines.Select(rl => new CalculationItem
				{
					Amount = 101,
					Audit = $"{rl.ChargeCode.AC_Code}: Base Rate AUD 101.00",
					Currency = "AUD",
					ExchangeRate = 1,
					LocalAmount = 101,
					LocalCurrency = "AUD"
				}).ToList();

				var expectedConsigneeRevenues = consigneeRateEntry.ChildRateLines.Select(rl => new CalculationItem
				{
					Amount = 102,
					Audit = $"{rl.ChargeCode.AC_Code}: Base Rate AUD 102.00",
					Currency = "AUD",
					ExchangeRate = 1,
					LocalAmount = 102,
					LocalCurrency = "AUD"
				}).ToList();

				var expectedLogs = new List<string>
				{
					"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
					"RatingHeader Found Client Rate CONSIGNEE1 Entries: 1",
					"RatingHeader Found Client Rate CONSIGNOR1 Entries: 1",
					"RateLine Found DSTCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found DSTCNR-FLT-Client Rate CONSIGNOR1",
					"RateLine Found FRTCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found FRTCNR-FLT-Client Rate CONSIGNOR1",
					"RateLine Found INSCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found INSCNR-FLT-Client Rate CONSIGNOR1",
					"RateLine Found LODCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found LODCNR-FLT-Client Rate CONSIGNOR1",
					"RateLine Found ORGCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found ORGCNR-FLT-Client Rate CONSIGNOR1",
					"RateLine Found UNLCNE-FLT-Client Rate CONSIGNEE1",
					"RateLine Found UNLCNR-FLT-Client Rate CONSIGNOR1",
				};

				if (domesticPaymentTermCode == "C3P")
				{
					// Remove the expected charges not applicable when payment
					// term is C3P
					expectedConsignorRevenues = expectedConsignorRevenues.Where(c => !chargeCodeGroupsNotApplicableToC3P.Any(na => c.Audit.Contains($"{na}CNR"))).ToList();
					expectedConsigneeRevenues = expectedConsigneeRevenues.Where(c => !chargeCodeGroupsNotApplicableToC3P.Any(na => c.Audit.Contains($"{na}CNE"))).ToList();

					// Likewise include the log messages about filtering said
					// charges
					var filterLogMessages = new List<string>
					{
						"RateLine Filtered DSTCNE-FLT-Client Rate CONSIGNEE1\treason:\tDST charge group is not applicable for AUSYD-AUMEL Domestic CCX",
						"RateLine Filtered DSTCNR-FLT-Client Rate CONSIGNOR1\treason:\tDST charge group is not applicable for AUSYD-AUMEL Domestic CCX",
						"RateLine Filtered LODCNE-FLT-Client Rate CONSIGNEE1\treason:\tLOD charge group is not applicable for AUSYD-AUMEL Domestic PPD",
						"RateLine Filtered LODCNR-FLT-Client Rate CONSIGNOR1\treason:\tLOD charge group is not applicable for AUSYD-AUMEL Domestic PPD",
						"RateLine Filtered ORGCNE-FLT-Client Rate CONSIGNEE1\treason:\tORG charge group is not applicable for AUSYD-AUMEL Domestic PPD",
						"RateLine Filtered ORGCNR-FLT-Client Rate CONSIGNOR1\treason:\tORG charge group is not applicable for AUSYD-AUMEL Domestic PPD",
						"RateLine Filtered UNLCNE-FLT-Client Rate CONSIGNEE1\treason:\tUNL charge group is not applicable for AUSYD-AUMEL Domestic CCX",
						"RateLine Filtered UNLCNR-FLT-Client Rate CONSIGNOR1\treason:\tUNL charge group is not applicable for AUSYD-AUMEL Domestic CCX"
					};
					expectedLogs.AddRange(filterLogMessages);
				}

				var expectedRevenues = expectedConsignorRevenues.Concat(expectedConsigneeRevenues).ToList();

				var expectedCosts = Array.Empty<CalculationItem>(); // No expected costs

				// Act, Assert
				/////
				AutoRateAndAssertResults(rq, expectedRevenues.ToArray(), expectedCosts, expectedLogs);
			}
		}

		public void TestAutorate_DeliveryAddress_PostcodeOverrideMatchRateEntry()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "US", "AU", "DCART", 200);
			rateEntry1.TI_CartageDeliveryAddressPostCode = "2000";

			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "US", "AU", "DCART", 300);
			rateEntry2.TI_CartageDeliveryAddressPostCode = "3000";

			Factory.Save();

			var rateQuery = GetValidRateQueryForTestingWithCTZCalculator(origin: "USLAX", destination: "AUSYD");
			rateQuery.DeliveryPostcode = "3000";

			var expectedRatingLogs = new List<string>
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 2",
				"RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 2000,3000.",
				"RateLine Found DCART-FLT-Client Rate CONSIGNEE1"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "DCART: Base Rate AUD 300.00",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutorate_DeliveryAddress_PostcodeOverride_CTZCalculator()
		{
			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var melPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "3000"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignee, "AU", cityTown: null);
			var zoneSydney = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			zoneSydney.CreateRateTransportZoneItemForTest(sydPostCode);
			var zoneMelbourne = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			zoneMelbourne.CreateRateTransportZoneItemForTest(melPostCode);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "US", "AU");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20, zoneSydney.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30, zoneMelbourne.PK);

			Factory.Save();

			var rateQuery = GetValidRateQueryForTestingWithCTZCalculator(origin: "USLAX", destination: "AUSYD", weight: 10, weightUnit: "KG");
			rateQuery.DeliveryPostcode = "3000";

			var expectedRatingLogs = new[]
			{
				@"Matched 'Mel Zone' for RateLine DCART-CTZ-KG-Client Rate CONSIGNEE1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignee Pickup/Delivery Address fallback"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "DCART: 10 Kilogram(s) @ AUD 30.00/KG",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutorate_DeliveryAddress_CityOverride_CTZCalculator()
		{
			var citySydney = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney"));
			var cityMelbourne = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Melbourne"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignee, "AU", cityTown: null);
			var zoneSydney = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			var zoneItemSydney = zoneSydney.CreateRateTransportZoneItemForTest(citySydney);
			// precondition for only city matching:
			zoneItemSydney.TQ_IsExcludingPostCode = true;
			var zoneMelbourne = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			var zoneItemMelbourne = zoneMelbourne.CreateRateTransportZoneItemForTest(cityMelbourne);
			// precondition for only city matching:
			zoneItemMelbourne.TQ_IsExcludingPostCode = true;

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, "US", "AU");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.EquipmentType = EquipmentNeeded.Any;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20, zoneSydney.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30, zoneMelbourne.PK);

			Factory.Save();

			var rateQuery = GetValidRateQueryForTestingWithCTZCalculator(origin: "USLAX", destination: "AUSYD", weight: 10, weightUnit: "KG");
			rateQuery.DeliveryCity = "Melbourne";

			var expectedRatingLogs = new[]
			{
				@"Matched 'Mel Zone' for RateLine DCART-CTZ-KG-Client Rate CONSIGNEE1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignee Pickup/Delivery Address fallback"
			};
			var expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "DCART: 10 Kilogram(s) @ AUD 30.00/KG",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};
			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedRatingLogs);
		}

		public void TestAutorate_HBLDelivery()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 200);
			rateEntry1.TI_HBLDeliveryMode = "CFS/CFS";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;
			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code },
			};

			rateQuery.HBLDeliveryMode = "DOOR/DOOR";

			var expectedRevenues = Array.Empty<CalculationItem>();

			var expectedLogs = new List<string>
			{
					"RateEntry Filtered Client Rate CONSIGNEE1 reason: HBL Delivery Mode didn't match job DOOR/DOOR.",
			};

			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedLogs);

			rateEntry1.TI_HBLDeliveryMode = string.Empty;

			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 300);
			rateEntry2.TI_HBLDeliveryMode = "DOOR/DOOR";

			expectedRevenues = new[]
			{
				new CalculationItem
				{
					Amount = 300,
					Audit = "OCART: Base Rate AUD 300.00",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				},
			};

			expectedLogs = new List<string>
			{
					"RateLine Filtered OCART-FLT-Client Rate CONSIGNEE1\treason:\toverridden by OCART-FLT-Client Rate CONSIGNEE1 by HBL Delivery Mode comparer",
			};

			AutoRateAndAssertResults(rateQuery, expectedRevenues, Array.Empty<CalculationItem>(), expectedLogs);
		}

		public void TestAutorate_FMCTariffID()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 200);
			rateEntry1.TI_FMCTariffID = "1111";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;
			rateQuery.RateParties = [
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code }
			];

			rateQuery.FMCTariffID = "1234";

			var expectedRevenues = Array.Empty<CalculationItem>();

			var expectedLogs = new List<string>
			{
					"RateEntry Filtered Client Rate CONSIGNEE1 reason: FMC Tariff ID didn't match job 1234.",
			};

			AutoRateAndAssertResults(rateQuery, expectedRevenues, [], expectedLogs);

			rateEntry1.TI_FMCTariffID = string.Empty;

			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "US", "OCART", 300);
			rateEntry2.TI_FMCTariffID = "1234";

			Factory.Save();

			expectedRevenues = [
				new CalculationItem
				{
					Amount = 300,
					Audit = "OCART: Base Rate AUD 300.00",
					Currency = "AUD",
					LocalAmount = 300,
					LocalCurrency = "AUD",
					ExchangeRate = 1,
				}
			];

			expectedLogs = [
				"RateLine Filtered OCART-FLT-Client Rate CONSIGNEE1\treason:\toverridden by OCART-FLT-Client Rate CONSIGNEE1 by TI_FMCTariffID comparer"
			];

			AutoRateAndAssertResults(rateQuery, expectedRevenues, [], expectedLogs);
		}

		public void TestAutoRate_CustomFields_CaseInsensitive()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("dgf pickup", "Y")
			};

			AutoRate_CustomFields(customFields, shouldMatch: true);
		}

		public void TestAutoRate_CustomFields_TypeConversion_Success()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			AutoRate_CustomFields(customFields, shouldMatch: true);
		}

		public void TestAutoRate_CustomFields_TypeConversion_Fail()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "NotABoolean")
			};

			AutoRate_CustomFields(customFields, shouldMatch: false);

			var expectedExtraValidation = new List<string>
			{
				"Provided RateQuery.JobInfo.CustomFields[DGF Pickup] (NotABoolean) is not assignable to this field: data type conversion failed."
			};

			var extras = ExtraValidationLogger.GetExtraValidationMessages().ToList();

			AssertContainsExactElementsInAnyOrder(expectedExtraValidation, extras);
		}

		public void TestAutoRate_CustomFields_Assign_Boolean()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Boolean, "Y");
		}

		public void TestAutoRate_CustomFields_Assign_Byte()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Byte, "4");
		}

		public void TestAutoRate_CustomFields_Assign_ComboBox()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.ComboBox, "Yo yo");
		}

		public void TestAutoRate_CustomFields_Assign_Date()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Date, "01-Jan-23");
		}

		public void TestAutoRate_CustomFields_Assign_Datetime()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Datetime, "01-Jan-23 12:34:00");
		}

		public void TestAutoRate_CustomFields_Assign_Decimal()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Decimal, "5.5");
		}

		public void TestAutoRate_CustomFields_Assign_Guid()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Guid, "df84a5de-22f9-4574-898d-58c331da6e83");
		}

		public void TestAutoRate_CustomFields_Assign_Integer()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Integer, "11");
		}

		public void TestAutoRate_CustomFields_Assign_Short()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.Short, "11");
		}

		public void TestAutoRate_CustomFields_Assign_String()
		{
			TestSuccessfulRoundTrip(AddOnColumnDataType.Codes.String, "value");
		}

		void TestSuccessfulRoundTrip(string typeCode, string pickupVal)
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", typeCode);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", pickupVal)
			};

			AutoRate_CustomFields(customFields, shouldMatch: true, pickupVal);

			var noFailedShipmentCustomFieldMappings = Array.Empty<string>();
			var extras = ExtraValidationLogger.GetExtraValidationMessages().ToList();
			AssertContainsExactElementsInAnyOrder(noFailedShipmentCustomFieldMappings, extras.ToArray());
		}

		public void TestAutoRate_CustomFields_Specified_NotMatch()
		{
			var customFields = new CustomField[]
			{
				new CustomField("SomeField", "N")
			};

			AutoRate_CustomFields(customFields, shouldMatch: false);

			var expectedExtraValidation = new List<string>
			{
				"Provided RateQuery.JobInfo.CustomFields[SomeField] (N) is not assignable to this field: custom field SomeField is not defined."
			};

			var extras = ExtraValidationLogger.GetExtraValidationMessages().ToList();

			AssertContainsExactElementsInAnyOrder(expectedExtraValidation, extras);
		}

		public void TestAutoRate_CustomFields_Empty()
		{
			var customFields = Array.Empty<CustomField>();

			AutoRate_CustomFields(customFields, shouldMatch: false);
		}

		public void TestAutoRate_CustomFields_Null()
		{
			AutoRate_CustomFields(null, shouldMatch: false);
		}

		public void TestAutoRate_CustomFields_ByConsignee()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			processTaskTemplate.P0_OH_Client = Consignee.PK;
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			using (SetTemplateMatch(ClientInTemplateSelectionOrgTypeList.Codes.ConsigneeConsignor))
			{
				AutoRate_CustomFields(customFields, shouldMatch: true);
			}
		}

		public void TestAutoRate_CustomFields_ByConsignor()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP", name: "Custom one");
			processTaskTemplate.P0_OH_Client = Consignor.PK;
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			using (SetTemplateMatch(ClientInTemplateSelectionOrgTypeList.Codes.ConsigneeConsignor))
			{
				AutoRate_CustomFields(customFields, shouldMatch: true);
			}
		}

		public void TestAutoRate_CustomFields_ByLocalClient()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			processTaskTemplate.P0_OH_Client = NewClient.PK;
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			using (SetTemplateMatch(ClientInTemplateSelectionOrgTypeList.Codes.LocalClient))
			{
				AutoRate_CustomFields(customFields, shouldMatch: true);
			}
		}

		public void TestAutoRate_CustomFields_ByControllingCustomer()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP");
			processTaskTemplate.P0_OH_Client = NewClient2.PK;
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			using (SetTemplateMatch(ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer))
			{
				AutoRate_CustomFields(customFields, shouldMatch: true);
			}
		}

		public void TestAutoRate_CustomFields_Department_Match()
		{
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP", department: Env.CurrentDepartmentPK);
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			AutoRate_CustomFields(customFields, shouldMatch: true);
		}

		public void TestAutoRate_CustomFields_Department_Mismatch()
		{
			var fakeDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SEA", "EXP", department: fakeDepartment.PK);
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "DGF Pickup", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var customFields = new CustomField[]
			{
				new CustomField("DGF Pickup", "Y")
			};

			AutoRate_CustomFields(customFields, shouldMatch: false);
		}

		void AutoRate_CustomFields(CustomField[] customFields, bool shouldMatch, string customFieldMatchVal = "Y")
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.Incoterm = IncoTerms.DeliveredDutyPaid;

			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Role = OrganisationRole.Roles.LC, Code = NewClient.OH_Code },
				new OrganisationRole { Role = OrganisationRole.Roles.CNR, Code = Consignor.OH_Code },
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code },
				new OrganisationRole { Role = OrganisationRole.Roles.CCUS, Code = NewClient2.OH_Code }
			};
			rateQuery.Carriers = new[]
			{
				new Organisation { CWCode = TransportProvider1.OH_Code }
			};

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP",
						Unit = 4,
						PackLines = new []
						{
							new JobPackLine()
							{
								PackageType = "PLT",
								Unit = 79,
								Volume = 5.5m,
								VolumeUnit = "M3"
							},
						}
					}
				},
				CustomFields = customFields
			};

			rateQuery.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;

			var expectedRevenues = Array.Empty<CalculationItem>();
			var expectedCosts = Array.Empty<CalculationItem>();
			var expectedLogs = new List<string>
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNOR1 Entries: 1",
				"RateLine Found DNOTE-FLT-Client Rate CONSIGNOR1",
				$"RateLine Filtered DNOTE-FLT-Client Rate CONSIGNOR1\treason:\tRateLine condition \"<GetCustomField(DGF Pickup)>\" == \"{customFieldMatchVal}\" not met",
			};

			if (shouldMatch)
			{
				expectedRevenues = new[]
				{
					new CalculationItem
					{
						Amount = 1111M,
						Audit = "DNOTE: Base Rate AUD 1111.00",
						Currency = "AUD",
						LocalAmount = 1111m,
						LocalCurrency = "AUD",
						ExchangeRate = 1,
					}
				};

				expectedLogs = new List<string>
				{
					"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
					"RatingHeader Found Client Rate CONSIGNOR1 Entries: 1",
					"RateLine Found DNOTE-FLT-Client Rate CONSIGNOR1",
					$"RateLine NOT Filtered DNOTE-FLT-Client Rate CONSIGNOR1\treason:\tRateLine condition \"<GetCustomField(DGF Pickup)>\" == \"{customFieldMatchVal}\" met",
				};
			}

			var clientRate = Helper.NewClientRate(Consignor);

			var pickupPostcodeRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "US");

			pickupPostcodeRateEntry.RateLines.RemoveAndDeleteAll();
			var newEntryRateLine1 = pickupPostcodeRateEntry.AddRateLine("DNOTE", FlatCalculator.Code);
			newEntryRateLine1.GetCalculator<FlatCalculator>().BaseRate = 1111m;
			newEntryRateLine1.TL_RX_NKCurrency = "AUD";
			newEntryRateLine1.TL_Condition = "USR";
			newEntryRateLine1.TL_ConditionalExpression = $"\"<GetCustomField(DGF Pickup)>\" == \"{customFieldMatchVal}\"";

			Factory.Save();

			AutoRateAndAssertResults(rateQuery, expectedRevenues, expectedCosts, expectedLogs);
		}

		public void TestAutoRate_GivenCarrierRateParty_ShouldMatchCarrierRate_SupportingOldClients()
		{
			// Arrange
			var clientRate = Helper.NewClientRate(Consignee);

			var addCarrierRateLine = (ClientRate clientRateMatching, ZGuid transportProviderPK, ZDecimal baseRate) =>
			{
				var matching = clientRateMatching.AddRateEntry("AIR", removeLines: true);
				matching.TI_OH_TransportProvider = transportProviderPK;
				var matchingRateLine = matching.AddRateLine("FRT", "FLT");
				matchingRateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;
			};

			addCarrierRateLine(clientRate, TransportProvider1.PK, 111); // Match
			addCarrierRateLine(clientRate, TransportProvider2.PK, 222); // No match
			addCarrierRateLine(clientRate, ZGuid.Empty, 333); // No match, overridden

			Factory.Save();

			var rq = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rq.RateParties = new[]
			{
				new OrganisationRole { Code = Consignee.OH_Code, Role = OrganisationRole.Roles.CNE },
				new OrganisationRole { Code = TransportProvider1.OH_Code, Role = OrganisationRole.Roles.CAR } // to support old clients
			};

			var expectedRevenues = new CalculationItem[]
			{
				new CalculationItem
				{
					Amount = 111,
					Audit = "FRT: Base Rate AUD 111.00",
					Currency = "AUD",
					ExchangeRate = 1,
					LocalAmount = 111,
					LocalCurrency = "AUD"
				}
			};

			var expectedCosts = Array.Empty<CalculationItem>();

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"RateLine Found FRT-FLT-Client Rate CONSIGNEE1 (x2)",
				"RateLine Filtered FRT-FLT-Client Rate CONSIGNEE1\treason:\toverridden by FRT-FLT-Client Rate CONSIGNEE1 by TI_OH_TransportProvider comparer",
			};

			// Act, Assert
			AutoRateAndAssertResults(rq, expectedRevenues, expectedCosts, expectedLogs);
		}

		public void TestAutoRate_GivenCarrierRateParty_ShouldMatchCarrierRate()
		{
			// Arrange
			var clientRate = Helper.NewClientRate(Consignee);

			var addCarrierRateLine = (ClientRate clientRateMatching, ZGuid transportProviderPK, ZDecimal baseRate) =>
			{
				var matching = clientRateMatching.AddRateEntry("AIR", removeLines: true);
				matching.TI_OH_TransportProvider = transportProviderPK;
				var matchingRateLine = matching.AddRateLine("FRT", "FLT");
				matchingRateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;
			};

			addCarrierRateLine(clientRate, TransportProvider1.PK, 111); // Match
			addCarrierRateLine(clientRate, TransportProvider2.PK, 222); // No match
			addCarrierRateLine(clientRate, ZGuid.Empty, 333); // No match, overridden

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateParties = new[] { new OrganisationRole { Code = Consignee.OH_Code, Role = OrganisationRole.Roles.CNE } };
			rateQuery.Carriers = new[] { new Organisation { CWCode = TransportProvider1.OH_Code } };

			var expectedRevenues = new CalculationItem[]
			{
				new CalculationItem
				{
					Amount = 111,
					Audit = "FRT: Base Rate AUD 111.00",
					Currency = "AUD",
					ExchangeRate = 1,
					LocalAmount = 111,
					LocalCurrency = "AUD"
				}
			};

			var expectedCosts = Array.Empty<CalculationItem>();

			var expectedLogs = new string[]
			{
				"Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription",
				"RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"RateLine Found FRT-FLT-Client Rate CONSIGNEE1 (x2)",
				"RateLine Filtered FRT-FLT-Client Rate CONSIGNEE1\treason:\toverridden by FRT-FLT-Client Rate CONSIGNEE1 by TI_OH_TransportProvider comparer",
			};

			// Act, Assert
			AutoRateAndAssertResults(rateQuery, expectedRevenues, expectedCosts, expectedLogs);
		}

		#region Helpers

		void SetupAndAssertRatesForNonContainerizedQuery(RateQuery query, CalculationItem[] revenues, CalculationItem[] costs, string[] logs)
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			clientEntry.RateLines.RemoveAndDeleteAll();

			var clientLine1 = clientEntry.AddRateLine("FRT", UnitCalculator.Code, "PLT");
			clientLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;
			clientLine1.TL_RX_NKCurrency = "AUD";

			var clientLine2 = clientEntry.AddRateLine("FRT", UnitCalculator.Code, "SHT");
			clientLine2.GetCalculator<UnitCalculator>().PerUnit = 4;
			clientLine2.TL_RX_NKCurrency = "AUD";

			var costingRate = Helper.NewCosting(TransportProvider1);
			var costingEntry = costingRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costingLine1 = costingEntry.AddRateLine("FRT", UnitCalculator.Code, "PLT");
			costingLine1.GetCalculator<UnitCalculator>().PerUnit = 3m;
			costingLine1.TL_RX_NKCurrency = "AUD";

			var costingLine2 = costingEntry.AddRateLine("FRT", UnitCalculator.Code, "SHT");
			costingLine2.GetCalculator<UnitCalculator>().PerUnit = 2;
			costingLine2.TL_RX_NKCurrency = "AUD";

			AutoRateAndAssertResults(query, revenues, costs, logs);
		}

		void SetupAndAssertRatesForContainerizedQuery(RateQuery query, CalculationItem[] revenues, CalculationItem[] costs, string[] logs, string foreignCurrency = "", decimal? exchangeRate = null)
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP");
			clientEntry.RateLines.RemoveAndDeleteAll();

			var clientLine1 = clientEntry.AddRateLine("FRT", UnitCalculator.Code, "CN");
			clientLine1.GetCalculator<UnitCalculator>().PerUnit = 220m;
			if (!string.IsNullOrEmpty(foreignCurrency))
			{
				clientLine1.TL_RX_NKCurrency = foreignCurrency;
			}
			else
			{
				clientLine1.TL_RX_NKCurrency = "AUD";
			}

			var costingRate = Helper.NewCosting(TransportProvider1);
			var costingEntry = costingRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "US", "", "20GP");
			costingEntry.RateLines.RemoveAndDeleteAll();

			var costingLine1 = costingEntry.AddRateLine("FRT", UnitCalculator.Code, "CN");
			costingLine1.GetCalculator<UnitCalculator>().PerUnit = 390m;
			if (!string.IsNullOrEmpty(foreignCurrency))
			{
				costingLine1.TL_RX_NKCurrency = foreignCurrency;
			}
			else
			{
				costingLine1.TL_RX_NKCurrency = "AUD";
			}

			if (!string.IsNullOrEmpty(foreignCurrency) && exchangeRate.HasValue)
			{
				Helper.NewExchangeRate(foreignCurrency, ExchangeRateTypes.Code.BuyRate, new ZDecimal(exchangeRate));
			}

			Factory.Save();

			AutoRateAndAssertResults(query, revenues, costs, logs);
		}

		void AutoRateAndAssertResults(RateQuery query, CalculationItem[] revenues, CalculationItem[] costs, IEnumerable<string> logs)
		{
			var logger = new ElementaryLogger();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ExtraValidationLogger.RemoveAllLogs();
				var result = new RatesAPIsAutoRater().AutoRate(Factory, query, logger);

				if (revenues is object)
				{
					var actualRevenues = result.SelectMany(r => r.Charges.Where(c => c.Revenue != null).Select(c => c.Revenue)).ToList();
					AssertCalculationItems(revenues, actualRevenues.ToArray());
				}

				if (costs is object)
				{
					var actualCosts = result.SelectMany(r => r.Charges.Where(c => c.Cost != null).Select(c => c.Cost)).ToList();
					AssertCalculationItems(costs, actualCosts.ToArray());
				}

				var loggerAllLogs = logger.GetAllLogs();
				AssertCollectionContains(logs, log => loggerAllLogs.Contains(log));
			}
		}

		RateQuery GetValidRateQueryForTestingWithCTZCalculator(string origin = "AUSYD", string destination = "USLAX", decimal? weight = 10, string weightUnit = "KG")
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.CalculationScope = RateQuery.CalculationScopes.SellRatesOnly;
			rateQuery.Origin = new Location { Type = Location.Types.UNLOCO, Value = origin };
			rateQuery.Destination = new Location { Type = Location.Types.UNLOCO, Value = destination };
			rateQuery.JobInfo = new JobInfo
			{
				Containers = new[]
				{
					new JobContainer
					{
						PackLines = new[]
						{
							new JobPackLine { PackageType = "PLT", Unit = 79, Weight = weight, WeightUnit = weightUnit }
						}
					}
				}
			};
			rateQuery.RateParties = new[]
			{
				new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = Consignee.OH_Code },
			};

			return rateQuery;
		}

		IDisposable SetTemplateMatch(params string[] codes)
		{
			var registryValue = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			registryValue = (ClientInTemplateSelectionCriteriaCollection)registryValue.Clone(null, null);

			var shipmentWorkflowTemplateMatchOrder = registryValue.GetValueByCode(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			foreach (var item in shipmentWorkflowTemplateMatchOrder.SelectedItems.ToList())
			{
				shipmentWorkflowTemplateMatchOrder.SelectedItems.Remove(item);
				shipmentWorkflowTemplateMatchOrder.AvailableItems.Add(item);
			}

			foreach (var code in codes)
			{
				shipmentWorkflowTemplateMatchOrder.SelectedItems.Add(shipmentWorkflowTemplateMatchOrder.AvailableItems.GetValueByCode(code));
			}

			return WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}

		void AssertCalculationItem(CalculationItem expected, CalculationItem actual)
		{
			AssertEquals(expected.Amount, actual.Amount);
			AssertEquals(expected.Currency, actual.Currency);
			AssertEquals(expected.Audit, actual.Audit);
			AssertEquals(expected.LocalAmount, actual.LocalAmount);
			AssertEquals(expected.LocalCurrency, actual.LocalCurrency);
			AssertEquals(expected.ExchangeRate, actual.ExchangeRate);
		}

		void AssertCalculationItems(CalculationItem[] expected, CalculationItem[] actual)
		{
			AssertEquals(expected.Length, actual.Length);
			for (var i = 0; i < expected.Length; i++)
			{
				AssertCalculationItem(expected[i], actual[i]);
			}
		}

		OrgHeader wharfCto;
		OrgHeader WharfCTO
		{
			get
			{
				if (wharfCto == null)
				{
					wharfCto = Factory.New<OrgHeader>();
					wharfCto.OH_Code = "WTCO1";
					wharfCto.OH_FullName = "Wharf CTO Address 1";
					wharfCto.MainAddress.OA_Address1 = "123 Fake Street";
					wharfCto.MainAddress.OA_City = "Botany";
					wharfCto.MainAddress.OA_State = "NSW";
					wharfCto.MainAddress.OA_PostCode = "2019"; // Botany
					wharfCto.OH_RL_NKClosestPort = "AUSYD";
				}
				return wharfCto;
			}
		}

		#endregion
	}
}
