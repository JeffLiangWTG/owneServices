using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateQueryToRateServiceQueryConverterTest : RatingTestCase
	{
		public void TestConvert_Origin()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.Origin = new Location() { Type = Location.Types.Country, Value = "AUSYD" };

			var expectedRatesQueryOrigins = new string[] { "AUSYD", "AU", "SYD", "OCEG" }; // OCEG is Oceania Zone (Rates Service Ocean)

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());

			var ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryOrigins, ratesQuery.Origin);

			rateQuery.Origin = new Location() { Type = Location.Types.Country, Value = "AU" };
			expectedRatesQueryOrigins = new string[] { "AU", "OCEG" };

			ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryOrigins, ratesQuery.Origin);

			rateQuery.Origin = new Location() { Type = Location.Types.Country, Value = "SYD" };
			expectedRatesQueryOrigins = new string[] { "AU", "SYD", "OCEG" };

			ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryOrigins, ratesQuery.Origin);
		}

		public void TestConvert_Destination()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.Destination = new Location() { Type = Location.Types.Country, Value = "AUSYD" };

			var expectedRatesQueryDestinations = new string[] { "AUSYD", "AU", "SYD", "OCEG" }; // OCEG is Oceania Zone (Rates Service Ocean)

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());

			var ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryDestinations, ratesQuery.Destination);

			rateQuery.Destination = new Location() { Type = Location.Types.Country, Value = "AU" };
			expectedRatesQueryDestinations = new string[] { "AU", "OCEG" };

			ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryDestinations, ratesQuery.Destination);

			rateQuery.Destination = new Location() { Type = Location.Types.Country, Value = "SYD" };
			expectedRatesQueryDestinations = new string[] { "AU", "SYD", "OCEG" };

			ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedRatesQueryDestinations, ratesQuery.Destination);
		}

		public void TestConvert_TransportMode_ContainerMode()
		{
			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);
			var rateQuery = GetAValidRateQueryToForRateService();

			void AssertTransportModeAndContainerMode(string transportMode, string containerMode)
			{
				var supportedTransportModes = new[] { "AIR", "SEA" };
				var expectedContainerModeMappings = new Dictionary<string, string>()
				{
					{ "LSE", "LCL" },
					{ "ULD", "FCL" }
				};

				rateQuery.TransportMode = transportMode;
				rateQuery.ContainerMode = containerMode;

				var ratesQuery = converter.Convert(rateQuery, out _);

				if (supportedTransportModes.Contains(transportMode))
				{
					AssertContainsExactElementsInAnyOrder(new[] { transportMode }, ratesQuery.TransportMode);

					if (expectedContainerModeMappings.ContainsKey(containerMode))
					{
						AssertContainsExactElementsInAnyOrder(new[] { expectedContainerModeMappings[containerMode] }, ratesQuery.ContainerMode);
					}
					else
					{
						AssertContainsExactElementsInAnyOrder(new[] { containerMode }, ratesQuery.ContainerMode);
					}
				}
				else
				{
					AssertNull(ratesQuery);
				}
			}

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAllRatesServiceRegistrySettingsEnabled()))
			{
				AssertTransportModeAndContainerMode("AIR", "LSE");

				AssertTransportModeAndContainerMode("AIR", "ULD");

				AssertTransportModeAndContainerMode("SEA", "LCL");

				AssertTransportModeAndContainerMode("SEA", "FCL");

				AssertTransportModeAndContainerMode("RAI", "LCL");

				AssertTransportModeAndContainerMode("RAI", "FCL");

				AssertTransportModeAndContainerMode("ROA", "LCL");

				AssertTransportModeAndContainerMode("ROA", "FCL");
			}
		}

		public void TestConvert_Commodity()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAA";
			commodity1.RH_IsHazardous = true;

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBB";
			commodity2.RH_IsPerishable = true;

			var commodity3 = Factory.New<RefCommodityCode>();
			commodity3.RH_Code = "CCC";
			commodity3.RH_IsTimber = true;

			var commodity4 = Factory.New<RefCommodityCode>();
			commodity4.RH_Code = "DDD";
			commodity4.RH_IsFlammable = true;

			var commodity5 = Factory.New<RefCommodityCode>();
			commodity5.RH_Code = "EEE";
			commodity5.RH_ContainerVentRequired = true;

			var commodity6 = Factory.New<RefCommodityCode>();
			commodity6.RH_Code = "FFF";
			commodity6.RH_UniversalCommodityGroup = "UUU";

			var commodity7 = Factory.New<RefCommodityCode>();
			commodity7.RH_Code = "YYY";

			var commodity8 = Factory.New<RefCommodityCode>();
			commodity8.RH_Code = "ZZZ";

			Factory.Save();

			var rateQuery = GetAValidRateQueryToForRateService();

			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "AAA" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "BBB" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "CCC" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "DDD" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "EEE" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "FFF" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "YYY" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "XXX" }
			};

			var expectedUniversalCommodityGroups = new[]
			{
				RefCommodityCode.HAZD,
				RefCommodityCode.PERS ,
				RefCommodityCode.TIMB ,
				RefCommodityCode.FLAM ,
				RefCommodityCode.CNVT ,
				"UUU"
			};

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);

			var ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(expectedUniversalCommodityGroups, ratesQuery.Commodities);

			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "NNN" },
			};

			logger.ClearLogs();
			ratesQuery = converter.Convert(rateQuery, out _);

			Assert(ratesQuery == null);
			AssertContainsExactElementsInAnyOrder(
				new[] { "Cannot search for rates via Rate Service. Provided CW Commodities does not exist in CW or cannot be recognized." },
				logger.Errors);
		}

		public void TestConvert_EffectiveDate()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.EffectiveDate = DateTimeOffset.UtcNow;

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertEquals(new ZDate(rateQuery.EffectiveDate.ToLocalTime().DateTime), ratesQuery.EffectiveDate);
			AssertEquals(new ZDate(rateQuery.EffectiveDate.ToLocalTime().DateTime), ratesQuery.EndDate);
		}

		public void TestConvert_CarrierContractNumber()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.CarrierContracts = new[] { "C1", "C2" };

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					new { ContractNumber = "C1" },
					new { ContractNumber = "C2" },
				},
				ratesQuery.Contract.Select(c => new { c.ContractNumber }));
		}

		public void TestConvert_CarrierServiceLevel()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "SL1" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "SL2" },
			};

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertContainsExactElementsInAnyOrder(
				new[] { "SL1", "SL2" },
				ratesQuery.ServiceLevel.ToArray());
		}

		public void TestConvert_ContainerType()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.ContainerTypes = new[]
			{
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "42G0" },
			};

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);
			var ratesQuery = converter.Convert(rateQuery, out _);
			AssertEquals(0, logger.GetAllLogs().Count());

			var expectedContainers = new[]
			{
				new { Code = "20GP", ISOType = "22G0" },
				new { Code = "40GP", ISOType = "42G0" }
			};
			AssertContainsExactElementsInAnyOrder(expectedContainers, ratesQuery.Container.Select(c => new { c.Code, c.ISOType }));

			rateQuery.ContainerTypes = new[]
			{
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "25GP" },
			};

			logger.ClearLogs();
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery);

			var expectedErrors = new[]
			{
				"Cannot search for rates via Rate Service. Provided Container Types does not exist in CW or cannot be recognized."
			};
			AssertContainsExactElementsInAnyOrder(expectedErrors, logger.Errors.ToArray());
		}

		public void TestConvert_ServiceProvider()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";
			provider1.OH_IsCreditor = true;
			provider1.OH_IsShippingProvider = true;

			var airLine = Factory.NewWithValidTestData<RefAirline>();
			airLine.RM_TwoCharacterCode = "A1";
			airLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";
			var provider2 = Helper.NewOrgHeader();
			provider2.OH_FullName = "Provider2";
			provider2.MiscServ.OM_RM_Airline = airLine.PK;
			provider2.OH_IsCreditor = true;
			provider2.OH_IsShippingProvider = true;

			var provider3 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "STDC";
			shippingLine1.RSL_CargoWiseOneCode = "C1C";
			provider3.OH_FullName = "Provider3";
			provider3.OH_RSL_ShippingLine = shippingLine1.PK;
			provider3.OH_IsCreditor = true;
			provider3.OH_IsShippingProvider = true;

			var provider4 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "SCAC";
			shippingLine2.RSL_CargoWiseOneCode = "C1C1";
			provider4.OH_FullName = "Provider4";
			provider4.OH_RSL_ShippingLine = shippingLine2.PK;
			provider4.OH_IsCreditor = true;
			provider4.OH_IsShippingProvider = true;

			var provider5 = Helper.NewOrgHeader();
			provider5.OH_Code = "Pn";
			provider5.OH_IsCreditor = true;
			provider5.OH_IsShippingProvider = true;

			var anotherAirLine = Factory.NewWithValidTestData<RefAirline>();
			anotherAirLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "KTH";
			var provider6 = Helper.NewOrgHeader();
			provider6.MiscServ.OM_RM_Airline = anotherAirLine.PK;
			provider6.OH_IsCreditor = true;
			provider6.OH_IsShippingProvider = true;

			var provider7 = Helper.NewOrgHeader();
			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine3.RSL_StandardCarrierAlphaCode = "SCAN";
			shippingLine3.RSL_CargoWiseOneCode = "C1CN";
			provider7.OH_RSL_ShippingLine = shippingLine3.PK;
			provider7.OH_IsCreditor = true;
			provider7.OH_IsShippingProvider = true;

			Factory.Save();

			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.ServiceProviders = new[]
			{
				new Organisation() { CWCode = "P1" },
				new Organisation() { IATACode = "A1" },
				new Organisation() { C1CCode = "C1C" },
				new Organisation() { SCAC = "SCAC" },
			};

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);
			var ratesQuery = converter.Convert(rateQuery, out _);

			var expectedCarriers = new[]
			{
				new { C1Code = (string)null , SCACCode = (string)null, IATACode = "A1", Name = "Provider2",  Source = "Rates API --> Rate Query" },
				new { C1Code = "C1C", SCACCode = "STDC", IATACode = (string)null, Name = "Provider3", Source = "Rates API --> Rate Query" },
				new { C1Code = "C1C1", SCACCode = "SCAC", IATACode = (string)null, Name = "Provider4", Source = "Rates API --> Rate Query" },
			};
			AssertContainsExactElementsInAnyOrder(expectedCarriers, ratesQuery.Carrier.Select(c => new { c.C1Code, c.SCACCode, c.IATACode, c.Name, c.Source }));

			var expectedWarnings = new[]
			{
				"P1 carrier (Rates API --> Rate Query) will not be included into the search request to Rates Service as it has no code mapping, SCAC, IATA or C1Code code specified."
			};
			AssertContainsExactElementsInAnyOrder(expectedWarnings, logger.Warnings);

			rateQuery.ServiceProviders = new[]
			{
				new Organisation() { CWCode = "PX" },
				new Organisation() { IATACode = "AX" },
				new Organisation() { C1CCode = "CXC" },
				new Organisation() { SCAC = "SCXC" },
			};
			logger.ClearLogs();
			ratesQuery = converter.Convert(rateQuery, out _);

			var expectedNewCarriers = new[]
			{
				new { C1Code = (string)null , SCACCode = (string)null, IATACode = "AX", Source = "Rates API --> Rate Query" },
				new { C1Code = "CXC", SCACCode = (string)null, IATACode = (string)null, Source = "Rates API --> Rate Query" },
				new { C1Code = (string)null, SCACCode = "SCXC", IATACode = (string)null, Source = "Rates API --> Rate Query" },
			};
			AssertContainsExactElementsInAnyOrder(expectedNewCarriers, ratesQuery.Carrier.Select(c => new { c.C1Code, c.SCACCode, c.IATACode, c.Source }));

			AssertEquals(0, logger.Errors.Count);
			AssertEquals(0, logger.Warnings.Count);
		}

		public void TestConvert_NamedAccount()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.NamedAccounts = new[]
			{
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC01" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "NAC02" },
			};

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);
			var ratesQuery = converter.Convert(rateQuery, out _);

			var expectedNamedAccounts = new[]
			{
				new { Name = "NAC01" }
			};
			AssertContainsExactElementsInAnyOrder(expectedNamedAccounts, ratesQuery.NamedAccount.Select(n => new { n.Name }));
		}

		public void TestConvert_PaymentTerm()
		{
			var rateQuery = GetAValidRateQueryToForRateService();

			rateQuery.CarrierPayTerm = "CCX";

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);

			var ratesQuery = converter.Convert(rateQuery, out _);
			AssertContainsExactElementsInAnyOrder(new[] { "CCX" }, ratesQuery.PaymentTerm);

			rateQuery.CarrierPayTerm = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.PaymentTerm);

			rateQuery.CarrierPayTerm = "";
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.PaymentTerm);
		}

		public void TestConvert_CGFilter()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "LSE";
			rateQuery.CGFilter = new CGFilter()
			{
				Products = new[] { "P1", "P2" },
				RateClasses = new[] { 5, 6, 7, 8 },
				References = new[] { "R1", "RN0078" },
				Vias = new[] { "AUSYD", "HKHKG" },
			};

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertContainsExactElementsInAnyOrder(new[] { "P1", "P2" }, ratesQuery.CargoGuideFilters.ProductNames);
			AssertContainsExactElementsInAnyOrder(new[] { 5, 6, 7, 8 }, ratesQuery.CargoGuideFilters.RateClasses);
			AssertContainsExactElementsInAnyOrder(new[] { "R1", "RN0078" }, ratesQuery.CargoGuideFilters.References);
			AssertContainsExactElementsInAnyOrder(new[] { "AUSYD", "HKHKG" }, ratesQuery.CargoGuideFilters.Vias);

			rateQuery.CGFilter.Products = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoGuideFilters.ProductNames);

			rateQuery.CGFilter.RateClasses = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoGuideFilters.RateClasses);

			rateQuery.CGFilter.References = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoGuideFilters.References);

			rateQuery.CGFilter.Vias = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoGuideFilters.Vias);
		}

		public void TestConvert_CSFilter()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.CSFilter = new CSFilter()
			{
				RateTypes = new[] { "R1", "R2" },
				RateTypes2 = new[] { "R21", "R22" },
				ServiceStrings = new[] { "SS1", "SS2", "SS3" },
			};

			var converter = new RateQueryToRateServiceQueryConverter(Factory, new ElementaryLogger());
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertContainsExactElementsInAnyOrder(new[] { "R1", "R2" }, ratesQuery.CargoSphereFilters.RateTypes);
			AssertContainsExactElementsInAnyOrder(new[] { "R21", "R22" }, ratesQuery.CargoSphereFilters.RateTypes2);
			AssertContainsExactElementsInAnyOrder(new[] { "SS1", "SS2", "SS3" }, ratesQuery.CargoSphereFilters.ServiceStringIDs);

			rateQuery.CSFilter.RateTypes = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoSphereFilters.RateTypes);

			rateQuery.CSFilter.RateTypes2 = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoSphereFilters.RateTypes2);

			rateQuery.CSFilter.ServiceStrings = null;
			ratesQuery = converter.Convert(rateQuery, out _);
			AssertNull(ratesQuery.CargoSphereFilters.ServiceStringIDs);
		}

		public void TestConvert_ErrorsWillBeLoggedInProvidedLogger()
		{
			var rateQuery = GetAValidRateQueryToForRateService();
			rateQuery.TransportMode = "";
			rateQuery.ContainerMode = "FCL";

			var logger = new ElementaryLogger();
			var converter = new RateQueryToRateServiceQueryConverter(Factory, logger);
			var ratesQuery = converter.Convert(rateQuery, out _);

			AssertNull(ratesQuery);
			var logs = logger.GetAllLogs();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"Job is neither Air Freight nor Sea Freight",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for -FCL: AutoRating -> Rates Service -> Rates Service Subscription"
				},
				logs
			);
		}

		RateQuery GetAValidRateQueryToForRateService()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.RateProviders = new[] { RatesAPIsConstants.RateProviders.CGCS };
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "LSE";

			return rateQuery;
		}

		RatesServiceRegistrySettingsCollection GetAllRatesServiceRegistrySettingsEnabled()
		{
			var result = RatesServiceRegistrySettingsCollection.GetEnabled();

			result.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, true));
			result.Add(new RatesServiceRegistrySettings(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.ULD, true));

			return result;
		}
	}
}
