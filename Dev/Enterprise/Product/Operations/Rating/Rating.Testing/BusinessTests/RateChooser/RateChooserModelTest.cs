using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Testing;
using Enterprise.Rating.Testing.GUI;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using Api = WiseRates.Api;
using Charge = WiseRates.Api.Model.Charge;
using ChargeType = WiseRates.Api.Model.ChargeType;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;
using Schedule = WiseRates.Api.Model.Schedule;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserModelTest : BaseRatingIntegrationTest
	{
		#region Show More Rates

		public void TestShowMoreRates()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, (r) => new[] { 0, 1 }.Contains(r.PageID));
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(1, model.ContainerGroups.Count());

			model.ShowMoreRates();
			AssertEquals(1, model.ContainerGroups.Count());
		}

		public void TestShowMoreRates_Paging()
		{
			int totalMockCalled = 0;

			var (filter, model) = SetupShowMoreRatesTest(() => { totalMockCalled++; });

			model.SendRatesRequest(filter, BuildRatesQuery(model.Criteria));
			AssertEquals(1, model.ContainerGroups.Count());

			model.ShowMoreRates();
			AssertEquals("WHEN ShowMoreRates, THEN should call", 2, totalMockCalled);
		}

		public void TestShowMoreRates_CurrentPageID()
		{
			int totalMockCalled = 0;

			var (filter, model) = SetupShowMoreRatesTest(() => { totalMockCalled++; });
			var ratesQuery1 = BuildRatesQuery(model.Criteria);
			var ratesQuery2 = BuildRatesQuery(model.Criteria);
			ratesQuery2.Destination = new[] { "AUSYD" };

			AssertEquals("Precondition", 0, model.CurrentPageID);
			model.SendRatesRequest(filter, ratesQuery1);
			AssertEquals(1, model.ContainerGroups.Count());

			model.ShowMoreRates();
			AssertEquals("CurrentPageID should be +1", 1, model.CurrentPageID);

			model.SendRatesRequest(filter, ratesQuery2);
			AssertEquals("CurrentPageID should be 0", 0, model.CurrentPageID);

			model.SendRatesRequest(filter, ratesQuery1);
			AssertEquals("CurrentPageID should be 0", 0, model.CurrentPageID);
		}

		(RateChooserFilterStripBusinessObject filter, RateChooserModel model) SetupShowMoreRatesTest(Action mockAction)
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var logger = new TestLogger();

			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, mockAction: mockAction);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			return (filter, model);
		}

		#endregion

		#region SendRatesRequest

		public void TestSendRatesRequest_OnlyRateService()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, (r) => new[] { 0, 1 }.Contains(r.PageID));

			var logger = new TestLogger();
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertNotNullOrEmpty(model.ConversionContext.SearchTraceID);
			AssertEquals(1, model.ContainerGroups.Count());
		}

		public void TestSendRatesRequest_OnlyCW1()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var chooserHelper = new RateChooserTestHelper(Factory);
			var carrier = chooserHelper.CreateCarrierOrg();
			var consol = chooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = "GEN";
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var logger = new ElementaryLogger();
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, null, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));

			var containerCommodity = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			AssertEquals(2, containerCommodity.RateCollection.Count);
			AssertEquals(TransportModes.Sea, model.TransportMode);
		}

		public void TestSendRatesRequest_CW1Rates_CarrierServiceLevel()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg();
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "AAA", "AWS");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "BBB");

			var carrier1 = helper.CreateCarrierOrg("SC11");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier1, "CCC", "AWS");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier1, "DDD");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier1, "AAA");

			Factory.Save();

			var consol = helper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_AWBServiceLevel = "BBB";
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "AAA", container: "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "BBB", container: "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "STD", container: "20GP");
			rateEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry3.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var costing1 = Helper.NewCosting(carrier1);
			costing1.TH_GC = Env.CurrentCompanyPK;

			var rateEntry4 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "CCC", container: "20GP");
			rateEntry4.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry4.AddRateLine(Helper.ChargeCodes["BAF"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry5 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "DDD", container: "20GP");
			rateEntry5.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry5.AddRateLine(Helper.ChargeCodes["BAF"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry6 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "US", "HKHKG", "AAA", container: "20GP");
			rateEntry6.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry6.AddRateLine(Helper.ChargeCodes["BAF"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var logger = new ElementaryLogger();
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, null, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			var ratesQuery = BuildRatesQuery(criteria);
			ChooserHelper.SetFilter_ServiceProvider(filter, carrier.PK);
			model.SendRatesRequest(filter, ratesQuery);
			var containerCommodity = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			var actual = string.Join(", ", containerCommodity.Rates.Select(x => (string)x.RateEntry.TI_PL_NKCarrierServiceLevel).Distinct().OrderBy(x => x));
			AssertEquals("No override filter means we bring all and do not use job service level as filter", "AAA, BBB, STD", actual);

			ChooserHelper.SetFilter_ServiceProvider(filter, carrier.PK);
			ChooserHelper.SetFilter_CarrierServiceLevel(filter, "AAA");
			ClearSelection(model);
			model.SendRatesRequest(filter, ratesQuery);
			actual = string.Join(", ", containerCommodity.Rates.Select(x => (string)x.RateEntry.TI_PL_NKCarrierServiceLevel).Distinct().OrderBy(x => x));
			AssertEquals("Override filter means we only bring rates with carrier service levels in filters", "AAA", actual);

			ChooserHelper.SetFilter_CarrierServiceLevel(filter, "AWS");
			ClearSelection(model);
			model.SendRatesRequest(filter, ratesQuery);
			AssertEquals("Even AAA mapped to AWS, We bring CW1 rates with Carrier Service Level in filters and ignore mapping", containerCommodity.Rates.Count(), 0);

			ChooserHelper.SetFilter_CarrierServiceLevel(filter, "AAA", "STD");
			ClearSelection(model);
			model.SendRatesRequest(filter, ratesQuery);
			actual = string.Join(", ", containerCommodity.Rates.Select(x => (string)x.RateEntry.TI_PL_NKCarrierServiceLevel).Distinct().OrderBy(x => x));
			AssertEquals("We should add STD to filter to bring rates with STD Carrier Service Level", "AAA, STD", actual);

			ChooserHelper.SetFilter_ServiceProvider(filter, carrier.PK, carrier1.PK);
			ChooserHelper.SetFilter_CarrierServiceLevel(filter, "AAA");
			ClearSelection(model);
			model.SendRatesRequest(filter, ratesQuery);
			actual = string.Join(", ", containerCommodity.Rates.Select(x => (string)x.RateEntry.TI_PL_NKCarrierServiceLevel).Distinct().OrderBy(x => x));
			AssertEquals("Override filter means we only bring rates with carrier service levels in filters", "AAA", actual);
			AssertEquals("We should bring rates from all carriers set in filter", 2, containerCommodity.Rates.Count());

			ChooserHelper.SetFilter_CarrierServiceLevel(filter, "AAA", "BBB", "CCC", "DDD");
			ClearSelection(model);
			model.SendRatesRequest(filter, ratesQuery);
			actual = string.Join(", ", containerCommodity.Rates.Select(x => (string)x.RateEntry.TI_PL_NKCarrierServiceLevel).Distinct().OrderBy(x => x));
			AssertEquals("multiple override filters", "AAA, BBB, CCC, DDD", actual);
			AssertEquals("We should bring rates from all carriers set in filter", 5, containerCommodity.Rates.Count());
		}

		public void TestSendRatesRequest_WiseRates_CarrierServiceLevel()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "").WithCarrierServiceLevel("AAA");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			var ratesQuery = BuildRatesQuery(criteria);
			model.SendRatesRequest(filter, ratesQuery);
			AssertEquals(1, model.ContainerGroups.Count());
			var rates = model.ContainerGroups.First().Rates;

			CombineAssertions("Should bring rate from Rate Service when we don't set filter", () =>
			{
				AssertEquals("rate count", 1, rates.Count());
				AssertEquals("Wise Rate found", true, rates.Any(x => x.WiseRateEntry != null));
			});

			ClearSelection(model);
			ratesQuery.ServiceLevel = new[] { "AAA" };
			model.SendRatesRequest(filter, ratesQuery, universalCarrierLevels: null);

			rates = model.ContainerGroups.First().Rates;

			CombineAssertions("Should bring rate from Rate Service when we set filter and list of universal carrier is null", () =>
			{
				AssertEquals("rate count", 1, rates.Count());
				AssertEquals("Wise Rate found", true, rates.Any(x => x.WiseRateEntry != null));
			});

			ClearSelection(model);
			ratesQuery.ServiceLevel = new[] { "AAA" };
			model.SendRatesRequest(filter, ratesQuery, universalCarrierLevels: new List<string> { "AAA" });

			rates = model.ContainerGroups.First().Rates;

			CombineAssertions("Should bring rate from Rate Service when we set filter and list of universal carrier contains filter", () =>
			{
				AssertEquals("rate count", 1, rates.Count());
				AssertEquals("Wise Rate found", true, rates.Any(x => x.WiseRateEntry != null));
			});

			ClearSelection(model);
			ratesQuery.ServiceLevel = new[] { "AAA" };
			model.SendRatesRequest(filter, ratesQuery, universalCarrierLevels: new List<string> { "BBB" });

			rates = model.ContainerGroups.First().Rates;

			CombineAssertions("Should not call Rate Service when we set filter and list of universal carrier does not contain filter", () =>
			{
				AssertEquals("rate count", 0, rates.Count());
			});
		}

		public void TestSendRatesRequest_CW1Rates_ContainerQuality()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg();

			var consol = helper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_AWBServiceLevel = "BBB";
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL).JC_ContainerQuality = ZString.Empty;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL).JC_ContainerQuality = "GOH";
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL).JC_ContainerQuality = "XYZ";

			var cw1Costing = Helper.NewCosting(carrier);
			cw1Costing.TH_GC = Env.CurrentCompanyPK;

			var cw1RateEntry = cw1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", container: "20GP", removeLines: true);
			cw1RateEntry.AddUnitRateLine("FRT", 100, QuantityUnit.CN); // Should appear on all tabs
			Factory.Save();

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ServiceGroupId = "1";
			ChooserHelper.SetContainerQuality(apiCosting1, null); // Should appear only on the tab with blank Quality
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting2.ServiceGroupId = "2";
			ChooserHelper.SetContainerQuality(apiCosting2, "GOH"); // Should appear only on GOH tab
			ChooserHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);

			var apiCosting3 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting3.ServiceGroupId = "3";
			ChooserHelper.SetContainerQuality(apiCosting3, "ABC"); // Should NOT appear on any tab
			ChooserHelper.AddPerContainerCharge(apiCosting3, "FRT", 3000);

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1, apiCosting2, apiCosting3);

			var logger = new TestLogger();
			var wiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, wiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			var ratesQuery = BuildRatesQuery(criteria);
			ChooserHelper.SetFilter_ServiceProvider(filter, carrier.PK);
			model.SendRatesRequest(filter, ratesQuery);

			foreach (ForwardingContainer container in consol.Containers)
			{
				var containerCommodity = model.ContainerGroups.Single(x => x.ContainerQuality == container.JC_ContainerQuality);

				var actualCw1Rates = containerCommodity.Rates.Where(x => x.WiseRateEntry == null);
				var actualWiseRates = containerCommodity.Rates.Where(x => x.WiseRateEntry != null);

				AssertEquals("CW1 Rates should not be filtered out", 1, actualCw1Rates.Count());
				Assert("There should be a calculated result for FRT", actualCw1Rates.Single().CalculatedResult.Count == 1);

				if (container.JC_ContainerQuality == "XYZ")
				{
					Assert("There should be no WiseRate", !actualWiseRates.Any());
				}
				else
				{
					AssertEquals("There should be only one WiseRate with the same container quality", 1, actualWiseRates.Count());
					AssertEquals(actualWiseRates.Single().WiseRateEntry.ContainerQuality(), container.JC_ContainerQuality);
				}
			}
		}

		public void TestSendRatesRequest_Logs_OriginDestination_DifferentFromCriteria()
		{
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg();
			var refContainer40 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40HC"));
			var consol = helper.CreateConsol("HKHKG", "USLAX");
			AddContainer(consol, "40HC", "GEN", ContainerModes.FCL);

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer40, carrier, "");
			apiCosting.Origin = "CNSGH";
			apiCosting.Destination = "USLAX";
			apiCosting.Charges.Add(new Charge { ChargeCode = "ODOC", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);

			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p =>
						p.RatesQuery.Contract.Any(x => x.ContractNumber == "CNT1")),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new ElementaryLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			Factory.Save();

			var ratesQuery = BuildRatesQuery(criteria);
			ratesQuery.Origin = new[] { "CNSHA" };
			ratesQuery.Destination = new[] { "USLAX" };
			ratesQuery.Contract = new[]
			{
				new RatesQueryContract { ContractNumber = "CNT1", CargoSphereID = "222" },
			};

			var viewModel = new RateChooserViewModel(model);
			viewModel.SendRatesRequest(filter, ratesQuery);
			viewModel.RefreshRates();
			List<ChooserContainerCommodityViewModel> list = viewModel.GetContainerCommodityList();
			var logs = list[0].SelectedRow.PlainLogs;

			AssertNotContains("Information: RateLine Filtered ODOC-UNT-CN-40HC-Wise Costing SCACCARRIER	reason:	Origin didn't match job HKHKG", logs);
		}

		public void TestSendRatesRequest_Carriers_DifferentFromCriteria()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier1 = helper.CreateCarrierOrg();
			var carrier2 = helper.CreateCarrierOrg("COD2");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier1, "HI");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier1, "LO");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier2, "HI");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier2, "LO");
			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = helper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			consol.JK_AWBServiceLevel = "LO";
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);

			var costing1 = Helper.NewCosting(carrier1);
			costing1.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "HI", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry2 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "LO", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var costing2 = Helper.NewCosting(carrier2);
			costing2.TH_GC = Env.CurrentCompanyPK;
			var rateEntry3 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "HI", "40GP");
			rateEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine3 = rateEntry3.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry4 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "LO", "40GP");
			rateEntry4.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine4 = rateEntry4.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var logger = new ElementaryLogger();
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, null, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory); // Origin: USLAX / Destination: HKHKG
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			ChooserHelper.SetFilter_ServiceProvider(filter, carrier2.PK);
			var model = new RateChooserModel(criteria, context);
			var ratesQuery = BuildRatesQuery(criteria);
			var serviceProviders = new[] { OrgWithSource.New(carrier2, new List<string> { "For Test" }) };
			ratesQuery.Carrier = wiseRatesQueryBuilder.GetCarriers(serviceProviders).ratesQueryCarriers;

			model.SendRatesRequest(filter, ratesQuery);

			var actualRates = model.ContainerGroups.SelectMany(x => x.Rates.Select(r => r.RateEntry));

			AssertGreaterThan(actualRates.Count(), 0);
			Assert(actualRates.All(x => x.ParentRatingHeader.Header == carrier2));
		}

		public void TestSendRatesRequest_CarrierContracts_DifferentFromCriteria()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CSL");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_AWBServiceLevel = "CSL";
			consol.JK_CarrierContractNumber = "CNT1";
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CSL", "20GP");
			rateEntry1.TI_ContractNumber = "CNT1";
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 21m;
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CSL", "20GP");
			rateEntry2.TI_ContractNumber = "CNT2";
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 22m;
			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CSL", "20GP");
			var rateLine3 = rateEntry3.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 23m;

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });
			apiCosting.WithContractNumber("CNT2");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);

			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.RatesQuery.Contract.Any(x => x.ContractNumber == "CNT2")),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			Factory.Save();

			var ratesQuery = BuildRatesQuery(criteria);
			ratesQuery.Contract = new[]
			{
				new RatesQueryContract { ContractNumber = "CNT1", CargoSphereID = null },
				new RatesQueryContract { ContractNumber = "CNT2", CargoSphereID = "222" },
			};
			ChooserHelper.SetFilter_ContractNumber(filter, "CNT1", "CNT2");
			model.SendRatesRequest(filter, ratesQuery);

			AssertNoExceptionThrown(wiseRatesClientMock.VerifyAll);

			var actualRates = model.ContainerGroups.SelectMany(x => x.Rates.Select(r => r.RateEntry));

			AssertEquals("Expected exact number of rates", 3, actualRates.Count());
			AssertEquals("Expected a single WiseEntry type rate", 1, actualRates.OfType<WiseEntry>().Count());
			Assert("All WiseEntry rates should have contract number CNT2", actualRates.OfType<WiseEntry>().All(x => x.TI_ContractNumber == "CNT2"));
			AssertEquals("Expected two RateEntry type rates", 2, actualRates.OfType<RateEntry>().Count());
			AssertEquals("Expected one RateEntry with contract number CNT1", 1, actualRates.OfType<RateEntry>().Count(x => x.TI_ContractNumber == "CNT1"));
			AssertEquals("Expected one RateEntry with contract number CNT2", 1, actualRates.OfType<RateEntry>().Count(x => x.TI_ContractNumber == "CNT2"));
		}

		public void TestSendRatesRequest_NamedAccounts_DifferentFromCriteria()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CSL");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_AWBServiceLevel = "CSL";
			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NA2");
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CSL", "20GP");
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 21m;

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });
			apiCosting.WithNamedAccounts("NA2");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);

			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.RatesQuery.NamedAccount.Any(x => x.Name == "NA2")),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			Factory.Save();

			var ratesQuery = BuildRatesQuery(criteria);
			ratesQuery.NamedAccount = new[] { new RatesQueryNamedAccount { Name = "NA2", CargoSphereID = "222" } };
			model.SendRatesRequest(filter, ratesQuery);

			AssertNoExceptionThrown(wiseRatesClientMock.VerifyAll);

			var actualRates = model.ContainerGroups.SelectMany(x => x.Rates.Select(r => r.RateEntry));

			AssertEquals("Expected 2 rates to be returned", 2, actualRates.Count());
			AssertEquals("Expected 1 rate of type WiseEntry", 1, actualRates.OfType<WiseEntry>().Count());
			AssertEquals("All named accounts should match 'NA2'", true, actualRates.OfType<WiseEntry>().SelectMany(x => x.NamedAccounts).All(y => y == "NA2"));
		}

		public void TestErrorsAndWarningsFromSearch_WhenDiagnosticSettingsIncludeRawDataIsTrue_ShouldReturnAllLogs()
		{
			var logger = new ElementaryLogger();
			logger.Log(LogType.Error, "Error message");
			logger.Log(LogType.Warning, "Warning message");
			logger.Log(LogType.Information, "Info message");
			logger.Log(LogType.Debug, "Debug message");

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();

			var consol = CreateConsol();

			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = model.ErrorsAndWarningsFromSearch.ToList();

				AssertContainsExactElementsInAnyOrder
				(
					new[]
					{
						"Error message",
						"Warning message",
						"Info message",
						"Debug message"
					},
					model.ErrorsAndWarningsFromSearch
				);
			}
			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = model.ErrorsAndWarningsFromSearch.ToList();

				AssertContainsExactElementsInAnyOrder
				(
					new[]
					{
						"Error message",
						"Warning message"
					},
					model.ErrorsAndWarningsFromSearch
				);
			}
		}

		public void TestSendRatesRequest_ContainersHavingSameISOType()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var refContainer20GM = Factory.New<RefContainer>();
			refContainer20GM.RC_Code = "20GM";

			refContainer20GM.RC_ISOType = refContainer20GP.RC_ISOType = "22G0";

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_AWBServiceLevel = "CSL";

			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GM", "GEN", "FCL", 2);

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20GP, carrier, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var model = GetResponseModel(consol, response);

			var expectedContainerCodes = new[] { "20GP", "20GM" };
			var actualContainerCodes = model.ContainerGroups.Select(x => x.ContainerRef.RC_Code.ToString()).ToArray();

			AssertContainsExactElementsInAnyOrder("The container groups should match the expected codes", expectedContainerCodes, actualContainerCodes);

			var containerGroup1 = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			AssertEquals("The container 20GP should have ISO Type 22G0", "22G0", containerGroup1.Rates.Single().WiseRateEntry.Container.Code);

			var containerGroup2 = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GM");
			AssertEquals("It should still be the same rate, just appears in 2 groups of the same ISO Type", "22G0", containerGroup2.Rates.Single().WiseRateEntry.Container.Code);
		}

		#endregion

		#region Displaying rates with different commodities

		public void TestDisplayingRates_ShowRatesWithSubstitutedCommodity_FCL()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");
			var refContainer20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_AWBServiceLevel = "CSL";
			consol.JK_RH_NKConsolCommodity = "CHEM";

			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "HAZ", "FCL", 2);

			var rate20GPGen = RateChooserTestHelper.CreateApiRate(refContainer20GP, carrier, "GEN");
			rate20GPGen.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 3m });

			var rate20GPHaz = RateChooserTestHelper.CreateApiRate(refContainer20GP, carrier, "HAZ");
			rate20GPHaz.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 5m });

			var rate20GPChem = RateChooserTestHelper.CreateApiRate(refContainer20GP, carrier, "CHEM");
			rate20GPChem.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 7m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(rate20GPGen, rate20GPHaz, rate20GPChem);

			var model = GetResponseModel(consol, response);

			var actualContainerGroups = model.ContainerGroups
				.Select(x => $"{x.ContainerRef.RC_Code}|{x.CommodityCode}")
				.ToArray();

			var expectedContainerGroups = new[]
			{
				"20GP|CHEM"
			};

			AssertContainsExactElementsInAnyOrder(
				expectedContainerGroups,
				actualContainerGroups
			);
		}

		RateChooserModel GetResponseModel(ForwardingConsol consol, RatesSearchResponse response)
		{
			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(
					It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			Factory.Save();

			var ratesQuery = BuildRatesQuery(criteria);
			model.SendRatesRequest(filter, ratesQuery);
			return model;
		}

		public void TestDisplayingRates_ShowRatesWithSubstitutedCommodity_LCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_RH_NKConsolCommodity = "CHEM";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";
			shipment1.AddPackLine(commodity: "HAZ");
			shipment1.AddPackLine(commodity: "GEN");

			var rateGen = ChooserHelper.CreateApiRate("", carrier, "GEN", ContainerModes.LCL);
			rateGen.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 3m });
			var rateHaz = ChooserHelper.CreateApiRate("", carrier, "HAZ", ContainerModes.LCL);
			rateHaz.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 5m });
			var rateChem = ChooserHelper.CreateApiRate("", carrier, "CHEM", ContainerModes.LCL);
			rateChem.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 7m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(rateGen, rateHaz, rateChem);
			model.AddWiseRatesForTest(response);

			model.ContainerGroups
				.Single()
				.Rates.All(x => x.WiseRateEntry == rateChem);

			Assert(true);
		}

		#endregion

		#region Default Currency

		public void TestDefaultCurrencyWhenCurrentStaffBranchDepartmentAreNull_ShouldNotThrowError()
		{
			using (Env.Instance.SetTemporaryUserContext(new UserContext(Guid.Empty, Guid.Empty, Guid.Empty)))
			{
				AssertEquals(string.Empty, RateChooserModel.DefaultCurrency);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, Guid.Empty))
			{
				AssertEquals("AUD", RateChooserModel.DefaultCurrency);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Guid.Empty, Env.CurrentDepartment.PK))
			{
				AssertEquals(string.Empty, RateChooserModel.DefaultCurrency);
			}

			using (Env.SetTemporaryUserContext(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("AUD", RateChooserModel.DefaultCurrency);
			}
		}

		#endregion

		public void TestHandlingOffice_AppearsInChargeViewModel()
		{
			var testHelper = new TestHelper(Factory);
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "GEN");

			apiCosting.Charges = new[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					Currency = "AUD",
					FlatRate = 100,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode() { Code = "FRT", Description = "FRT", Group = "FRT" },
					CustomCategory = WRConstants.ChargeCustomCategory.BOL,
					ProviderCustomFields = new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.HandlingOffice,
							Value = "Here is the handling office location",
							Description = "something"
						}
					}
				},
				new Charge()
				{
					ChargeCode = "CAF",
					Currency = "AUD",
					FlatRate = 300,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode() { Code = "CAF", Description = "CAF", Group = "FRT" },
					CustomCategory = WRConstants.ChargeCustomCategory.BOL,
					ProviderCustomFields = Array.Empty<CustomField>() /* no handling office */
				}
			};

			var consol = ChooserHelper.CreateConsol();
			ChooserHelper.AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 1);

			var logger = new ElementaryLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);
			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates(); // reload from Model
			var rateModels = viewModel.ContainerTabs.SelectMany(r => r.Rates);

			var actualHandlingOffices = rateModels
				.Single()
				.BOLCharges.Charges
				.Select(c => c.HandlingOffice)
				.Where(o => !string.IsNullOrWhiteSpace(o))
				.ToArray();

			var expectedHandlingOffices = new[] { "Here is the handling office location" };

			AssertContainsExactElementsInAnyOrder("Handling office location should match the expected collection", expectedHandlingOffices, actualHandlingOffices);
		}

		public void TestMultipleSameChargeCodeInRate_ShownSeparatelyModel_CargoSphereRate()
		{
			var testHelper = new TestHelper(Factory);

			var cw1Charge = testHelper.ChargeCodes.NewConsolChargeCode("CW1", "Normal charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			cw1Charge.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var cw1UniverasalChargeCodes = new AccChargeCodeUniversalCodeMappingCollection(cw1Charge);
			var uucc1 = cw1UniverasalChargeCodes.AddNew();
			var uucc2 = cw1UniverasalChargeCodes.AddNew();
			uucc1.AUP_Code = "UC1";
			uucc2.AUP_Code = "UC2";
			Factory.Save();

			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			ChooserHelper.AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 1);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "GEN");

			var uucc1Charge = RateChooserTestHelper.AddFlatCharge(apiCosting, "UC1", 1m);
			var uucc2Charge = RateChooserTestHelper.AddFlatCharge(apiCosting, "UC2", 10m);

			var logger = new ElementaryLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });

			model.AddWiseRatesForTest(response);

			var calculationResults = model.ContainerGroups
				.SelectMany(cg => cg.Rates)
				.SelectMany(r => r.CalculatedResult)
				.OfType<AutoRateInfo>()
				.Select(a => (string)a.ChargeCode.AC_Code)
				.ToArray();
			AssertContainsExactElementsInAnyOrder(
				"There should be a calculation result for each charge in the rate",
				new[] { "CW1", "CW1" },
				calculationResults
			);

			var chooseContainerCommodity = model.ContainerGroups.Single();
			chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			var rateModels = viewModel.ContainerTabs.SelectMany(r => r.Rates);

			var allCharges = new List<ChargeViewModel>();
			allCharges.AddRange(rateModels.SelectMany(rm => rm.BOLCharges?.Charges ?? Enumerable.Empty<ChargeViewModel>()));
			allCharges.AddRange(rateModels.SelectMany(rm => rm.CW1BillOfLadingCharges?.Charges ?? Enumerable.Empty<ChargeViewModel>()));
			allCharges.AddRange(rateModels.SelectMany(rm => rm.CW1FreightCharges?.Charges ?? Enumerable.Empty<ChargeViewModel>()));
			allCharges.AddRange(rateModels.SelectMany(rm => rm.OceanCharges?.Charges?.SelectMany(c => c.Charges) ?? Enumerable.Empty<ChargeViewModel>()));
			allCharges.AddRange(rateModels.SelectMany(rm => rm.InlandCharges?.Charges?.SelectMany(c => c.Charges) ?? Enumerable.Empty<ChargeViewModel>()));
			allCharges.AddRange(rateModels.SelectMany(rm => rm.OutlandCharges?.Charges?.SelectMany(c => c.Charges) ?? Enumerable.Empty<ChargeViewModel>()));

			var displayedCharges = allCharges.WhereNotNull().Select(c => new
			{
				c.CalculatedAmount,
				c.CalculatedAmountString,
				c.CalculatedFormula,
				c.Code
			}).Select(c => $"{c.CalculatedAmount}|{c.CalculatedAmountString}|{c.CalculatedFormula}|{c.Code}").ToArray();

			var expectedDisplayedCharges = new[]
			{
				"1|$1.00|Base Rate AUD 1.00|CW1",
				"10|$10.00|Base Rate AUD 10.00|CW1"
			};

			AssertContainsExactElementsInAnyOrder(
				"The displayed charges should match the expected charges",
				expectedDisplayedCharges,
				displayedCharges
			);
		}

		public void TestSearchRate_WithRateChooser_Finds_CW1RateWithBogus_TM_Text_WarningShown()
		{
			var org1 = Helper.NewOrgHeader("ORG1");

			var costing = Helper.NewCosting(org1);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "AU", container: "20GP", commodity: "GEN");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "AUSYD", container: "20GP", commodity: "GEN");
			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "AUEC", container: "20GP", commodity: "GEN");

			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry3.RateLines.RemoveAndDeleteAll();

			rateEntry1.TI_OH_TransportProvider = org1.PK;

			var rateLine1 = rateEntry1.AddRateLine("BAF", MinimumCalculator.Code);
			var rateLine2 = rateEntry2.AddRateLine("CAF", MinimumCalculator.Code);
			var rateLine3 = rateEntry3.AddRateLine("WAR", MinimumCalculator.Code);

			// The Minimum calculator has String1 bound to TM_Text. It also controls the
			// minimum type. Whether Minimum-for-job or minimum-per-chargecode
			var rateLine1Calculator = rateLine1.GetCalculator<MinimumCalculator>();
			rateLine1Calculator.String1 = "???";
			rateLine1Calculator.MinimumValue = 100;

			var rateLine2Calculator = rateLine2.GetCalculator<MinimumCalculator>();
			rateLine2Calculator.MinimumValue = 200;
			rateLine2Calculator.IsChargeCodeMinimum = true;

			var rateLine3Calculator = rateLine3.GetCalculator<MinimumCalculator>();
			rateLine3Calculator.MinimumValue = 300;
			rateLine3Calculator.IsChargeCodeMinimum = true;

			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 1);

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1, rateEntry2, rateEntry3));

			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			var rates = genContainers.Rates.Where(x => x.WiseRateEntry == null);

			var result = rates
				.SelectMany(r => r.CalculatedResult)
				.Select(c => $"{c.ChargeCode.AC_Code}|{c.CalculationDescription}|{c.HasResult}")
				.ToList();

			var expected = new[]
			{
				"BAF|Calculation failed due to the charge 'BAF' with a calculator 'MIN' having an unexpected Apply To of '???' in Costing ORG1|False",
				"CAF|MIN USD 200.00 (Charge Code Minimum)|True",
				"WAR|MIN USD 300.00 (Charge Code Minimum)|True"
			};

			AssertContainsExactElementsInAnyOrder("The calculated results should match the expected values", expected, result);

			AssertEquals("No issues reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestSearchAsync_ShouldReportUsage_WithRatesFoundDetails()
		{
			var carrier1 = CreateCarrierOrg("CAR1");
			var carrier2 = CreateCarrierOrg("CAR2");
			var carrier3 = CreateCarrierOrg("CAR3");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "20GP").AddPerUnitCharge("FRT", 201m, "CN");                                // Valid rate
			CreateRate(costing1, container: "40GP").AddPerUnitCharge("FRT", 401m, "CN");                                // Valid rate

			Factory.Save();

			// Rates Service Rates
			var rate1 = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN", serviceLevel: "STD");      // Valid rate
			RateChooserTestHelper.AddFlatCharge(rate1, "FRT", 302m);

			var rate2 = ChooserHelper.CreateApiRate("40GP", carrier2, "GEN", serviceLevel: "STD");      // Rate with error
			var charge2 = RateChooserTestHelper.AddFlatCharge(rate2, string.Empty, 602m);
			charge2.CarrierChargeCodeInfo = new CarrierSpecificChargeCode { Code = "XXX", Description = "Charge code with no mapping to universal code" };

			var rate3 = ChooserHelper.CreateApiRate("20GP", carrier3, "GEN", serviceLevel: "STD");      // Rate with warning
			RateChooserTestHelper.AddFlatCharge(rate3, "FRT", 302m);

			var rate4 = ChooserHelper.CreateApiRate("40GP", carrier3, "GEN", serviceLevel: "EXP");      // Rate with Warning and Error
			var charge4 = RateChooserTestHelper.AddFlatCharge(rate4, string.Empty, 602m);
			charge4.CarrierChargeCodeInfo = new CarrierSpecificChargeCode { Code = "XXX", Description = "Charge code with no mapping to universal code" };

			var response = new RatesSearchResponse
			{
				Rates = new[] { rate1, rate2, rate3, rate4 },
				ChargeCodes = new[]
				{
					new RefChargeCode { Code = "FRT", Group = "FRT" },
				},
				Carriers = new[]
				{
					new RefCarrier { Code = "CAR1", SCACCode = "CAR1" },
					new RefCarrier { Code = "CAR2", SCACCode = "CAR2" },
					new RefCarrier { Code = "CAR3", SCACCode = "XXXX" },
				},
			};

			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 1);
			AddContainer(consol, "40GP", "GEN", "FCL", 1);

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var cw1RatesProvider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1RatesProvider, testWiseRatesProvider, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));

			var expected = new UsageRatesSearchResult();
			expected.CargoSphere.TotalRates = 4;
			expected.CargoSphere.ErrorRates = 2;
			expected.CargoSphere.WarningRates = 2;
			expected.CargoSphere.ValidRates = 1;
			expected.CW1.TotalRates = 2;
			expected.CW1.ErrorRates = 0;
			expected.CW1.WarningRates = 0;
			expected.CW1.ValidRates = 2;

			var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch);
			var message = messages[0];

			AssertRatesSearchUsageProperties(expected);
		}

		void AssertRatesSearchUsageProperties(UsageRatesSearchResult expected)
		{
			var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch);
			var properties = messages[0].UsageProperties;
			var ratesResult = properties.Value<JObject>(UsageProperties.RatesSearchResult);

			// Verify CargoSphere results
			var cargoSphereResults = ratesResult.Value<JObject>("CargoSphere");
			AssertEquals(expected.CargoSphere.TotalRates, cargoSphereResults.Value<int>("TotalRates"));
			AssertEquals(expected.CargoSphere.TotalCharges, cargoSphereResults.Value<int>("TotalCharges"));
			AssertEquals(expected.CargoSphere.ValidRates, cargoSphereResults.Value<int>("ValidRates"));
			AssertEquals(expected.CargoSphere.ErrorRates, cargoSphereResults.Value<int>("ErrorRates"));
			AssertEquals(expected.CargoSphere.WarningRates, cargoSphereResults.Value<int>("WarningRates"));
			Assert(cargoSphereResults.Value<int>("ElapsedTime") > 0);

			// Verify Cargoguide results
			var cargoguideResults = ratesResult.Value<JObject>("Cargoguide");
			AssertEquals(expected.Cargoguide.TotalRates, cargoguideResults.Value<int>("TotalRates"));
			AssertEquals(expected.Cargoguide.TotalCharges, cargoguideResults.Value<int>("TotalCharges"));
			AssertEquals(expected.Cargoguide.ValidRates, cargoguideResults.Value<int>("ValidRates"));
			AssertEquals(expected.Cargoguide.ErrorRates, cargoguideResults.Value<int>("ErrorRates"));
			AssertEquals(expected.Cargoguide.WarningRates, cargoguideResults.Value<int>("WarningRates"));

			// Verify CW1 results
			var cw1Results = ratesResult.Value<JObject>("CW1");
			AssertEquals(expected.CW1.TotalRates, cw1Results.Value<int>("TotalRates"));
			AssertEquals(expected.CW1.TotalCharges, cw1Results.Value<int>("TotalCharges"));
			AssertEquals(expected.CW1.ValidRates, cw1Results.Value<int>("ValidRates"));
			AssertEquals(expected.CW1.ErrorRates, cw1Results.Value<int>("ErrorRates"));
			AssertEquals(expected.CW1.WarningRates, cw1Results.Value<int>("WarningRates"));
			Assert(cw1Results.Value<int>("ElapsedTime") > 0);
		}

		public void TestLegacyRateSelectorFormSeaRates_ShouldNotUseUrsRatesProvider()
		{
			var carrier1 = CreateCarrierOrg("CAR1");
			var rate1 = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN", serviceLevel: "STD");      // Valid rate
			RateChooserTestHelper.AddFlatCharge(rate1, "FRT", 302m);
			var response = new RatesSearchResponse
			{
				Rates = new[] { rate1 },
				ChargeCodes = new[]
				{
					new RefChargeCode { Code = "FRT", Group = "FRT" },
				},
				Carriers = new[]
				{
					new RefCarrier { Code = "CAR1", SCACCode = "CAR1" },
				},
			};

			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var mockWiseRatesFactory = new Mock<IWiseRatesClientFactory>();
			mockWiseRatesFactory
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			// Mock URS factory and client
			var mockUrsFactory = new Mock<IUrsRatesClientFactory>();
			var mockUrsClient = new Mock<IUrsClient>();
			mockUrsFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Returns(mockUrsClient.Object);
			mockUrsClient
				.Setup(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult<IEnumerable<TradeServiceDto>>([])); // Return empty array

			ObjectFactory.Substitute(mockWiseRatesFactory.Object);
			ObjectFactory.Substitute(mockUrsFactory.Object);

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 1);
			AddContainer(consol, "40GP", "GEN", "FCL", 1);

			var logger = new TestLogger();

			// Given
			// URS feature is ENABLED
			// legacy Rate Selector is ENABLED
			using (ObjectFactory.Substitute(MockURSFeatureHelper(true)))
			{
				var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);

				var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);
				var filter = new RateChooserFilterStripBusinessObject(criteria);
				var model = new RateChooserModel(criteria, context);
				// When Rate Selector send Rates Request
				model.SendRatesRequest(filter, BuildRatesQuery(criteria));

				// Then Urs provider should NOT be called to get rates
				mockUrsFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
				mockUrsClient.Verify(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
				Assert(true);
			}
		}

		public void TestApplyRatesToJob()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "ATPT", "FCL", 5);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var cyrcCharge = Helper.ChargeCodes.NewConsolChargeCode("CYRC", "Cont Yard Rec Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			cyrcCharge.AC_GC = Env.CurrentCompanyPK;

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			apiCosting1.Charges.Add(new Charge { ChargeCode = "CYRC", Currency = "AUD", ChargeType = Api.Model.ChargeType.Included, FreightInclusiveCarriageCharge = "FRT" });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			model.AddWiseRatesForTest(response);
			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);
			// Select CW1 Costing for "ATPT"
			var atptContainers = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");
			atptContainers.SelectedRate = atptContainers.Rates.Single(x => x.WiseRateEntry == null);

			var expected = new[]
			{
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 30m,
					CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ AUD 10.00/Container",
					CalculationDescription = "CYRC - Cont Yard Rec Charge (Included)"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 500m,
					CalculationSingleLineDescription = "FRT: 5 20GP Container(s) @ AUD 100.00/Container"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "Cont Yard Rec Charge",
					Amount = 0m,
					CalculationSingleLineDescription = "CYRC: Freight Inclusive Calculator",
				}
			};

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());
			using (_Rating.Start(mockGuiInteractor.Object))
			{
				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;
				AssertEquals(3 * 10m + 5 * 100m, charges.Sum(x => x.Amount));
				AssertEquals("Count", 3, charges.Count); //Should  populate included charge too
				AssertRatingResults(expected, result);
			}
		}

		public void TestGetSelectedRate_MultipleSelectedRatesHaveBOLCharges_SelectBOLChargesFromTheFirstOne()
		{
			var bolCharge = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BO2", "Bill of Lading Also", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var carrier1 = CreateCarrierOrg("CAR1");

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 10m);
			RateChooserTestHelper.AddFlatCharge(gp20Rate, "BOL", 30).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);

			var gp40Rate = ChooserHelper.CreateApiRate("40GP", carrier1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp40Rate, "FRT", 20m);
			RateChooserTestHelper.AddFlatCharge(gp40Rate, "BOL", 40).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			RateChooserTestHelper.AddFlatCharge(gp40Rate, "BO2", 80).WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);

			var response = new RatesSearchResponse
			{
				Rates = new[] { gp20Rate, gp40Rate },
				ChargeCodes = new[]
				{
					new RefChargeCode { Code = "FRT", Group = "FRT" },
					new RefChargeCode { Code = "FRT", Group = "FRT" },
					new RefChargeCode { Code = "BOL", Group = "DST" },
					new RefChargeCode { Code = "BO2", Group = "DST" },
				},
				Carriers = new[]
				{
					new RefCarrier { Code = "CAR1", SCACCode = "CAR1" },
				},
			};

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "40GP", "GEN", "FCL", 3);

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.ValuesCanBeSet = true;
			criteria.Creditors = new Creditors();

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var context = new RatingContext();
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			model.AddWiseRatesForTest(response);

			var tab1 = model.ContainerGroups.First();
			var tab2 = model.ContainerGroups.Last();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == gp20Rate);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == gp40Rate);

			var results = model.GetSelectedRate();

			var actual = results
				.Select(r => $"{r.ChargeCode.AC_Code}|{r.Amount}|{r.SingleLineDescription}")
				.ToArray();

			var expected = new[]
			{
				"FRT|30|FRT: 3 20GP Container(s) @ AUD 10.00/Container",
				"FRT|60|FRT: 3 40GP Container(s) @ AUD 20.00/Container",
				"BOL|30|BOL: Base Rate AUD 30.00",
				"BO2|80|BO2: Base Rate AUD 80.00"
			};

			AssertContainsExactElementsInAnyOrder("The summed up charges should match the expected collection", expected, actual);
		}

		public void TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup_SameCommodity_GetTwoResultOfOne()
		{
			var firstCommodity = "GEN";
			var secondCommidity = "GEN";
			var expectedResult = new[]
			{
				new { ChargeCode = "FRT", Amount = 11m, Desc = "FRT: 1 20GP Container(s) @ AUD 11.00/Container" },
				new { ChargeCode = "FRT", Amount = 11m, Desc = "FRT: 1 40GP Container(s) @ AUD 11.00/Container" },
			};
			var expectedResultString = expectedResult
				.Select(x => $"{x.ChargeCode}|{x.Amount}|{x.Desc}")
				.ToArray();

			TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup(firstCommodity, secondCommidity, expectedResultString);
		}

		public void TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup_DifferentCommodity_GetTwoResultOfOne()
		{
			var firstCommodity = "GEN";
			var secondCommidity = "HAZ";
			var expectedResult = new[]
			{
				new { ChargeCode = "FRT", Amount = 11m, Desc = "FRT: 1 20GP Container(s) @ AUD 11.00/Container" },
				new { ChargeCode = "FRT", Amount = 11m, Desc = "FRT: 1 40GP Container(s) @ AUD 11.00/Container" },
			};
			var expectedResultString = expectedResult
				.Select(x => $"{x.ChargeCode}|{x.Amount}|{x.Desc}")
				.ToArray();
			TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup(firstCommodity, secondCommidity, expectedResultString);
		}

		/// <summary>
		/// In CW1, 20GP and 40GP have an invalid, but non-empty iso type.
		/// In CW1, 40HC has a valid ISO type
		/// All three rates have the same RC_FreightClass.
		///
		/// A Consol exists with 1x 20GP-firstcommodity and 1x 40GP-secondcommodity
		/// A RateService rate exists with price for a 40HC rate.
		///
		/// Check that each tab contains 1x rate. (the 40HC rate), but displayed for
		///  - the corresponding container of the tab.
		/// Check that get-selected-rate returns 2x rate with 1 containers if both commodities are the same
		/// Check that get-selected-rate returns 2x rate with 1 containers if both commodities are different
		/// </summary>
		void TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup(string firstCommodity, string secondCommidity, string[] expectedResult)
		{
			var bolCharge = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge.AC_GC = GlbCompany.CurrentCompany.PK;

			// container 1 is in the job.
			var container1 = Helper.Containers["20GP"];
			container1.RC_ISOType = "XX";
			container1.RC_FreightRateClass = "45GP";

			// container 2 is in the job.
			var container2 = Helper.Containers["40GP"];
			container2.RC_ISOType = "YY";
			container2.RC_FreightRateClass = "45GP";

			// container 3 is not in the job but is in the rate service rate.
			var container3 = Helper.Containers["40HC"];
			container3.RC_ISOType = "45G0";
			container3.RC_FreightRateClass = "45GP";

			var carrier1 = CreateCarrierOrg("CAR1");

			Factory.Save();

			var hc40Rate = ChooserHelper.CreateApiRate("40HC", carrier1, "GEN");
			hc40Rate.Container.ISOTypeGroup = container3.RC_FreightRateClass;
			RateChooserTestHelper.AddPerContainerCharge(hc40Rate, "FRT", 11m);

			var response = new RatesSearchResponse
			{
				Rates = new[] { hc40Rate },
				ChargeCodes = new[]
				{
					new RefChargeCode { Code = "FRT", Group = "FRT" },
				},
				Carriers = new[]
				{
					new RefCarrier { Code = "CAR1", SCACCode = "CAR1" },
				},
			};

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", firstCommodity, "FCL", 1);
			AddContainer(consol, "40GP", secondCommidity, "FCL", 1);

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.ValuesCanBeSet = true;

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var context = new RatingContext();
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			model.AddWiseRatesForTest(response);

			AssertContainsExactElementsInAnyOrder(
				"The two job containers are still shown in their own tab",
				new[] { container1.PK, container2.PK },
				model.ContainerGroups.Select(x => x.ContainerRef.PK)
			);

			model.ContainerGroups.ForEach(tab => tab.SelectedRate = tab.Rates.Single());

			var results = model.GetSelectedRate();
			var actualResults = results
				.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				expectedResult,
				actualResults
			);
		}

		public void TestGetSelectedRate_ContainerDifferentISOTypeSameISOTypeGroup_RateServiceContainerNeitherButSameIsoTypeGgroup_GetContainersFromJob()
		{
			// Container1 and 2 are in the criteria. Container 3 is not
			// do not remove container3 as it demonstrates that it is filtered out
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.RC_ISOType = "AAAA";
			container1.RC_FreightRateClass = "44G0";
			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC");
			container2.RC_ISOType = "BBBB";
			container2.RC_FreightRateClass = "44G0";
			var container3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container3.RC_ISOType = "CXXC";
			container3.RC_FreightRateClass = "44G0";

			var carrier = CreateCarrierOrg("CAR1");

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "40GP", "GEN", "FCL", 1);
			AddContainer(consol, "40HC", "GEN", "FCL", 1);

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			criteria.ValuesCanBeSet = true;
			var testContainers = new TestContainers(Factory, container1.PK, 1, container2.PK, 1);
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var rateServiceRate = ChooserHelper.CreateApiRate(string.Empty, carrier, "GEN");
			rateServiceRate.Container = new Api.Model.RefContainer { Code = "CCCC", ISOType = "CCCC", ISOTypeGroup = "44G0" };

			RateChooserTestHelper.AddPerContainerCharge(rateServiceRate, "FRT", 11m);
			var response = new RatesSearchResponse
			{
				Rates = new[] { rateServiceRate },
				ChargeCodes = new[]
				{
					new RefChargeCode { Code = "FRT", Group = "FRT" },
				},
				Carriers = new[]
				{
					new RefCarrier { Code = "CAR1", SCACCode = "CAR1" },
				},
			};

			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var context = new RatingContext();
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			model.AddWiseRatesForTest(response);

			// Each tab should have 1 rate. Each rate should be the RateService
			// rate. However, instead of the container from the rate being shown,
			// the container of the tab wherein it is displayed is picked.
			AssertEquals("Job has 2 containers, each on their own tab", 2, model.ContainerGroups.Count());
			model.ContainerGroups.ForEach(tab =>
				AssertEquals("Each tab has 1 rate", 1, tab.Rates.Count())
			);
			var ratesInTabs = model.ContainerGroups.SelectMany(x => x.Rates).ToArray();
			AssertCollectionContains("All rates should match the service rate ID", rateServiceRate.Id, ratesInTabs.Select(x => x.RateId).Distinct());
			var expectedContainerPKs = new[] { container1.PK, container2.PK };
			AssertContainsExactElementsInAnyOrder(
				"Each received rate is matched against the container in its tab, according to the IsoTypeGroup",
				expectedContainerPKs,
				ratesInTabs.Select(x => x.RateEntry.Container.PK)
			);

			model.ContainerGroups.ForEach(tab => tab.SelectedRate = tab.Rates.Single());

			// Once a selection is made we again expect two rates that are
			// a copy of the rate service response but with the container of the
			// tab it is located in.
			var results = model.GetSelectedRate();
			var actual = results
				.Select(r => $"{r.ChargeCode.AC_Code}|{r.Amount}|{r.SingleLineDescription}")
				.ToArray();

			var expected = new[]
			{
				"FRT|11|FRT: 1 40GP Container(s) @ AUD 11.00/Container",
				"FRT|11|FRT: 1 40HC Container(s) @ AUD 11.00/Container",
			};

			AssertContainsExactElementsInAnyOrder(
				"The summarized results should match expected charges",
				expected,
				actual
			);
		}

		public void TestApplyRatesToJob_WiseRateOnly()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());
			using (_Rating.Start(mockGuiInteractor.Object))
			{
				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;
				AssertEquals(3 * 10m, charges.Sum(x => x.Amount));
				AssertEquals("charges count", 1, charges.Count);
			}
		}

		public void TestApplyRatesToJob_SelectedRateFromWiseRateAndCW1_WhenSelectedRateServiceProviderNotEqualToConsolCarrier_ShouldBringRatesAndUpdateConsolCarrier()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consolCarrier = helper.CreateCarrierOrg("COD2");

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;

			AssertEquals("Consol's carrier should equal to main address of Consol carrier", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "ATPT", "FCL", 5);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			model.AddWiseRatesForTest(response);
			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);
			// Select CW1 Costing for "ATPT"
			var atptContainers = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");
			atptContainers.SelectedRate = atptContainers.Rates.Single(x => x.WiseRateEntry == null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());

			using (_Rating.Start(mockGuiInteractor.Object))
			{
				model.ApplyCarrierBackToJob();
				AssertEquals("Consol's carrier should updated to main address of carrier", consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				CombineAssertions(() =>
				{
					AssertEquals(3 * 10m + 5 * 100m, charges.Sum(x => x.Amount));
					AssertEquals("Count", 2, charges.Count);
				});
			}
		}

		public void TestApplyRatesToJob_SelectedRateFromCW1Only_WhenSelectedRateServiceProviderNOTEqualToAnyOrganizationUsedForSearchingCW1Costing_ShouldBringRatesAndUpdateConsolCarrier()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consolCarrier = helper.CreateCarrierOrg("COD2");

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;

			AssertEquals("Consol's carrier should equal to main address of Consol carrier", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			// Select CW1 Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry == null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());

			using (_Rating.Start(mockGuiInteractor.Object))
			{
				model.ApplyCarrierBackToJob();
				AssertEquals("Consol's carrier should updated to main address of carrier", consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				CombineAssertions(() =>
				{
					AssertEquals(3 * 100m, charges.Sum(x => x.Amount));
					AssertEquals("Count", 1, charges.Count);
				});
			}
		}

		public void TestApplyRatesToJob_SelectedRateFromCW1Only_WhenSelectedRateServiceProviderIsEqualToOneOrganizationUsedForSearchingCW1Costing_ShouldBringRatesAndNotUpdateConsolCarrier()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consolCarrier = helper.CreateCarrierOrg("COD2");

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			AssertEquals("Consol's carrier should equal to main address of Consol carrier", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);
			AssertEquals("Consol's creditor should equal to main address of carrier", consol.JK_OA_CreditorAddress, carrier.MainAddress.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			// Select CW1 Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry == null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());

			using (_Rating.Start(mockGuiInteractor.Object))
			{
				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				CombineAssertions(() =>
				{
					AssertEquals("Consol's carrier should not be changed", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);
					AssertEquals(3 * 100m, charges.Sum(x => x.Amount));
					AssertEquals("Count", 1, charges.Count);
				});
			}
		}

		public void TestApplyRatesToJob_SelectedRateFromCW1Only_WhenSelectedRateIsAnStandardRate_ShouldNotReportAnyErrors()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var creditor = CreateCarrierOrg("SCAC");
			var consolCarrier = helper.CreateCarrierOrg("COD2");

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertEquals("Consol's carrier should equal to main address of Consol carrier", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);
			AssertEquals("Consol's creditor should equal to main address of creditor", consol.JK_OA_CreditorAddress, creditor.MainAddress.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var standardCosting = Helper.NewCosting(null);
			standardCosting.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = standardCosting.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));

			// Select CW1 Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry == null);

			var newCarrier = model.GetNewCarrierToApplyToJob();

			CombineAssertions(() =>
			{
				AssertNull("New Carrier is null", newCarrier);
				AssertNullOrEmpty("No Error Should Be Reported", ErrorReporter.LastMessageReported);
			});

			model.ApplyCarrierBackToJob();

			CombineAssertions(() =>
			{
				AssertEquals("Consol's carrier should not be changed", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);
				AssertNullOrEmpty("No Error Should Be Reported", ErrorReporter.LastMessageReported);
			});
		}

		public void TestApplyRatesToJob_SelectedRateFromWiseRateOnly_WhenSelectedRateServiceProviderNotEqualToConsolCarrier_ShouldBringRatesAndUpdateConsolCarrier()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consolCarrier = helper.CreateCarrierOrg("COD2");

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = consolCarrier.MainAddress.PK;

			AssertEquals("Consol's carrier should equal to main address of carrier", consol.JK_OA_ShippingLineAddress, consolCarrier.MainAddress.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());

			using (_Rating.Start(mockGuiInteractor.Object))
			{
				model.ApplyCarrierBackToJob();
				AssertEquals("Consol's carrier should updated to main address of carrier", consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				CombineAssertions(() =>
				{
					AssertEquals(3 * 10m, charges.Sum(x => x.Amount));
					AssertEquals("charges count", 1, charges.Count);
				});
			}
		}

		public void TestApplyRatesToJob_SelectedRateFromWiseRateOnly_WhenSelectedRateServiceProviderEqualToConsolCarrier_ShouldNotChangeCarrierAddress()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();

			var carrierAddr2 = carrier.Addresses.AddNew();
			carrierAddr2.OA_Address1 = "TEST CARRIER ADDRESS";
			carrierAddr2.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ShippingLineAddress = carrierAddr2.PK;
			AssertEquals("Consol's carrier should equal to new address of carrier", consol.JK_OA_ShippingLineAddress, carrierAddr2.PK);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());
			using (_Rating.Start(mockGuiInteractor.Object))
			{
				model.ApplyCarrierBackToJob();
				AssertNotEquals("Consol's carrier should not change to main address", consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);
				AssertEquals("Consol's carrier should not change to main address", consol.JK_OA_ShippingLineAddress, carrierAddr2.PK);

				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				CombineAssertions(() =>
				{
					AssertEquals(3 * 10m, charges.Sum(x => x.Amount));
					AssertEquals("charges count", 1, charges.Count);
				});
			}
		}

		public void TestApplyRatesToJob_PreferChargesFromCargoSphereOverCW1()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			var container = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var costing1 = Helper.NewCosting(org1);
			var costing2 = Helper.NewCosting(org2);

			costing1.AddRateEntryWithFlatRateLine("FCL", "SEA", "USLAX", "HKHKG", TestAWB.AC_Code, 10, "AUD", container: container.RC_Code);
			costing1.AddRateEntryWithFlatRateLine("ORG", "ALL", "USLAX", "HKHKG", TestBBK.AC_Code, 11, "AUD", container: container.RC_Code);
			costing1.AddRateEntryWithFlatRateLine("DST", "ALL", "USLAX", "HKHKG", TestADF.AC_Code, 12, "AUD", container: container.RC_Code);

			costing2.AddRateEntryWithFlatRateLine("FCL", "SEA", "USLAX", "HKHKG", TestTHC.AC_Code, 20, "AUD", container: container.RC_Code);
			costing2.AddRateEntryWithFlatRateLine("ORG", "ALL", "USLAX", "HKHKG", TestANY.AC_Code, 21, "AUD", container: container.RC_Code);
			costing2.AddRateEntryWithFlatRateLine("DST", "ALL", "USLAX", "HKHKG", TestADF.AC_Code, 22, "AUD", container: container.RC_Code);

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN");

			var csRate = RateChooserTestHelper.CreateApiRate(container, carrier, "");
			csRate.Charges.Add(new Charge { ChargeCode = TestBBK.AC_Code, Currency = "AUD", Unit = "CN", PerUnitRate = 100m });
			csRate.Charges.Add(new Charge { ChargeCode = TestADF.AC_Code, Currency = "AUD", Unit = "CN", PerUnitRate = 100m });
			csRate.Charges.Add(new Charge { ChargeCode = TestANY.AC_Code, Currency = "AUD", Unit = "CN", PerUnitRate = 100m });

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);

			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			criteria.ValuesCanBeSet = true;
			criteria.FreightMode = FreightMode.FCL;
			criteria.ChargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateAll;

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();

			using (_Rating.Start(mockGuiInteractor.Object))
			using (_Rating.StartCost())
			{
				var model = new RateChooserModel(criteria, context);
				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(csRate);
				model.AddWiseRatesForTest(response);

				var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
				genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

				mockGuiInteractor
					.Setup(x => x.SelectRate(context, criteria))
					.Returns(model.GetSelectedRate());

				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var foundRates = result.RateInfoCollection;
				var foundChargeCodes = foundRates.Select(x => $"{x.ChargeCode.AC_Code} => {x.Amount}");

				AssertContainsExactElementsInAnyOrder
				(
					new string[] {
						"TESTBBK => 100",
						"TESTADF => 100",
						"TESTANY => 100",
						"TESTAWB => 10.00",
						"TESTTHC => 20.00"
					},
					foundChargeCodes
				);
			}
		}

		public void TestRatesShouldHaveCalculatedInfo()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "ATPT", "FCL", 5);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			model.AddWiseRatesForTest(response);
			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);
			// Select CW1 Costing for "ATPT"
			var atptContainers = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");
			atptContainers.SelectedRate = atptContainers.Rates.Single(x => x.WiseRateEntry == null);

			CombineAssertions("Rate entry for both CW1 and Wise rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", 1, atptContainers.SelectedRate.SelectedCalculatedResult.Count);
				AssertEquals("Calculated amount", 500m, atptContainers.SelectedRate.SelectedCalculatedResult[0].Amount); // 5 * 100
				AssertEquals("Calculated description", "FRT: 5 20GP Container(s) @ AUD 100.00/Container", atptContainers.SelectedRate.SelectedCalculatedResult[0].SingleLineDescription);

				AssertEquals("Calculated result", 1, genContainers.SelectedRate.SelectedCalculatedResult.Count);
				AssertEquals("Calculated amount", 30m, genContainers.SelectedRate.SelectedCalculatedResult[0].Amount); // 3 * 10
				AssertEquals("Calculated description", "FRT: 3 20GP Container(s) @ AUD 10.00/Container", genContainers.SelectedRate.SelectedCalculatedResult[0].SingleLineDescription);
			});

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var tab1 = viewModel.ContainerTabs.First();

			CombineAssertions("Charge model for CW1 should contain calculated info", () =>
			{
				AssertEquals("Calculated result", "AUD $30.00", tab1.SelectedRow.TotalCalculatedCharges);
				AssertContains("Calculated description", "FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container", tab1.SelectedRow.TotalCalculatedChargesDescription);
			});

			var tab2 = viewModel.ContainerTabs.Last();

			CombineAssertions("Charge model for Wise Rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", "AUD $500.00", tab2.SelectedRow.TotalCalculatedCharges);
				AssertContains("Calculated description", "FRT 500 AUD - FRT: 5 20GP Container(s) @ AUD 100.00/Container", tab2.SelectedRow.TotalCalculatedChargesDescription);
			});
		}

		public void TestRatesRowShouldHaveTotalCalculationInfo()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "ATPT", "FCL", 5);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			model.AddWiseRatesForTest(response);

			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

			// Select CW1 Costing for "ATPT"
			var atptContainers = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");
			atptContainers.SelectedRate = atptContainers.Rates.Single(x => x.WiseRateEntry == null);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var tab1 = viewModel.ContainerTabs.First();

			CombineAssertions("Charge model for Wise Rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", "AUD $30.00", tab1.SelectedRow.TotalCalculatedCharges);
				AssertContains("Calculated description", "FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container", tab1.SelectedRow.TotalCalculatedChargesDescription);
			});

			var tab2 = viewModel.ContainerTabs.Last();

			CombineAssertions("Charge model for CW1 Rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", "AUD $500.00", tab2.SelectedRow.TotalCalculatedCharges);
				AssertContains("Calculated description", "FRT 500 AUD - FRT: 5 20GP Container(s) @ AUD 100.00/Container", tab2.SelectedRow.TotalCalculatedChargesDescription);
			});
		}

		public void TestRatesTotalCalculationInfo_ShouldUpdate_WhenOptionalChargeSelectAndDeselect()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var frtCharge2 = Helper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var frtCharge3 = Helper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Helper.NewExchangeRate(usdCurrency, ExchangeRateTypes.Code.BuyRate, 0.5);
			Helper.NewExchangeRate(usdCurrency, ExchangeRateTypes.Code.SellRate, 2);

			var hkdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "HKD");
			hkdCurrency.ExchangeRates.DeleteAll();

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var frtChargeOptional2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 200, currency: "USD")
				.OfType(Api.Model.ChargeType.Optional);
			var frtChargeOptional3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR3", 200, currency: "HKD")
				.OfType(Api.Model.ChargeType.Optional);

			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithContractNumber(string.Empty);

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2 });
			model.AddWiseRatesForTest(response);

			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.First(x => x.WiseRateEntry != null);

			var wiseRateTab = model.ContainerGroups.First();

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			var tab = viewModel.ContainerTabs.First();

			CombineAssertions("Charge model for Wise Rates should contain calculated info", () =>
			{
				AssertEquals("frtChargeOptional2", false, wiseRateTab.SelectedRate.IsActive(frtChargeOptional2));
				AssertEquals("frtChargeOptional3", false, wiseRateTab.SelectedRate.IsActive(frtChargeOptional3));
				AssertEquals("TotalIconVisibility", false, tab.SelectedRow.TotalIconVisibility);
				AssertEquals("TotalCalculatedCharges", "AUD $30.00", tab.SelectedRow.TotalCalculatedCharges);
				AssertEquals
				(
					"TotalCalculatedChargesDescription",
					@"FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container
",
					tab.SelectedRow.TotalCalculatedChargesDescription);
			});

			wiseRateTab.SelectedRate.SetActive(frtChargeOptional2, true);
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);
			viewModel.RefreshRates();
			CombineAssertions("Charge model for Wise Rates should contain calculated info for selected optional charge", () =>
			{
				AssertEquals("frtChargeOptional2", true, wiseRateTab.SelectedRate.IsActive(frtChargeOptional2));
				AssertEquals("frtChargeOptional3", false, wiseRateTab.SelectedRate.IsActive(frtChargeOptional3));
				AssertEquals("TotalIconVisibility", false, tab.SelectedRow.TotalIconVisibility);
				AssertEquals("TotalCalculatedCharges", "AUD $1,230.00", tab.SelectedRow.TotalCalculatedCharges); // $30 + 600 * 2
				AssertEquals
				(
					"TotalCalculatedChargesDescription",
					@"FR2 600 USD - FR2: 3 20GP Container(s) @ USD 200.00/Container
FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container
",
					tab.SelectedRow.TotalCalculatedChargesDescription);
			});

			wiseRateTab.SelectedRate.SetActive(frtChargeOptional3, true);
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);
			viewModel.RefreshRates();
			CombineAssertions("Select an optional charge with no possible currency conversion", () =>
			{
				AssertEquals("frtChargeOptional2", true, wiseRateTab.SelectedRate.IsActive(frtChargeOptional2));
				AssertEquals("frtChargeOptional3", true, wiseRateTab.SelectedRate.IsActive(frtChargeOptional3));
				AssertEquals("TotalIconVisibility", true, tab.SelectedRow.TotalIconVisibility);
				AssertEquals("TotalIcon", RateChooserImageRepository.ErrorIcon, tab.SelectedRow.TotalIcon);
				AssertEquals
				(
					"TotalCalculatedChargesDescription",
					@"FR2 600 USD - FR2: 3 20GP Container(s) @ USD 200.00/Container
FR3 600 HKD - FR3: 3 20GP Container(s) @ HKD 200.00/Container
FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container
",
					tab.SelectedRow.TotalCalculatedChargesDescription);
			});

			wiseRateTab.SelectedRate.SetActive(frtChargeOptional3, false);
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);
			viewModel.RefreshRates();
			CombineAssertions("de-select the charge with no currency conversion, things should return to as-before", () =>
			{
				AssertEquals("frtChargeOptional2", true, wiseRateTab.SelectedRate.IsActive(frtChargeOptional2));
				AssertEquals("frtChargeOptional3", false, wiseRateTab.SelectedRate.IsActive(frtChargeOptional3));
				AssertEquals("TotalIconVisibility", false, tab.SelectedRow.TotalIconVisibility);
				AssertEquals("TotalCalculatedCharges", "AUD $1,230.00", tab.SelectedRow.TotalCalculatedCharges); // $30 + $600 * 2
				AssertEquals
				(
					"TotalCalculatedChargesDescription",
					@"FR2 600 USD - FR2: 3 20GP Container(s) @ USD 200.00/Container
FRT 30 AUD - FRT: 3 20GP Container(s) @ AUD 10.00/Container
",
					tab.SelectedRow.TotalCalculatedChargesDescription);
			});
		}

		#region Apply new Carrier back to Consol

		public void TestApplyRatesToJob_ApplyNewCarrierBackToConsol_SingleRoute()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			var orgs = new List<OrgHeader>();
			for (var i = 0; i < 10; ++i)
			{
				orgs.Add(Helper.NewOrgHeader());
			}

			Factory.Save();

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", serviceProvider },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
				},
				true,
				true,
				-1,
				serviceProvider,
				null,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", serviceProvider },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
				},
				true,
				true,
				-1,
				orgs[1],
				null,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", serviceProvider },
					{ "CreditorOnRoute1", orgs[4] },
				},
				true,
				true,
				-1,
				orgs[1],
				null,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", serviceProvider },
				},
				true,
				true,
				-1,
				orgs[1],
				null,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
				},
				false,
				true,
				-1,
				serviceProvider,
				null,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
				},
				true,
				false,
				-1,
				serviceProvider,
				null,
				false);

			Assert(true);
		}

		public void TestApplyRatesToJob_ApplyNewCarrierBackToConsol_MultiRoute()
		{
			var serviceProvider = ChooserHelper.CreateCarrierOrg("SCAC");
			var orgs = new List<OrgHeader>();
			for (var i = 0; i < 10; ++i)
			{
				orgs.Add(Helper.NewOrgHeader());
			}

			Factory.Save();

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", serviceProvider },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				0, // 1st Route
				serviceProvider,
				orgs[3],
				true);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", serviceProvider },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				1, // 2nd Route
				serviceProvider,
				orgs[5],
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", serviceProvider },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				0, // 1st Route
				orgs[1],
				orgs[3],
				true);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", serviceProvider },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				1, // 2nd Route
				orgs[1],
				orgs[5],
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", serviceProvider },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				0, // 1st Route
				orgs[1],
				serviceProvider,
				true);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", serviceProvider },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				1, // 2nd Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", serviceProvider },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				0, // 1st Route
				orgs[1],
				orgs[3],
				true);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", serviceProvider },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				1, // 2nd Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", serviceProvider },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				0, // 1st Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", serviceProvider },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				true,
				1, // 2nd Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", serviceProvider },
				},
				true,
				true,
				0, // 1st Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", serviceProvider },
				},
				true,
				true,
				1, // 2nd Route
				orgs[1],
				orgs[5],
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				false,
				true,
				0, // 1st Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				false,
				true,
				1, // 2nd Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				false,
				0, // 1st Route
				orgs[1],
				serviceProvider,
				false);

			AssertApplyNewCarrierBackToConsol(
				new Dictionary<string, OrgHeader>
				{
					{ "ServiceProvider", serviceProvider },
					{ "Carrier", orgs[1] },
					{ "Creditor", orgs[2] },
					{ "CarrierOnRoute1", orgs[3] },
					{ "CreditorOnRoute1", orgs[4] },
					{ "CarrierOnRoute2", orgs[5] },
					{ "CreditorOnRoute2", orgs[6] },
				},
				true,
				false,
				1, // 2nd Route
				orgs[1],
				serviceProvider,
				false);

			Assert(true);
		}

		void AssertApplyNewCarrierBackToConsol(
			Dictionary<string, OrgHeader> orgs,
			bool cw1Selected,
			bool wiseRateSelected,
			int routeSetNumber,
			OrgHeader expectedConsolCarrier,
			OrgHeader expectedRouteCarrier,
			bool isLinked)
		{
			var consol = CreateConsol();
			consol.SetDefaultShippingLineAddress(GetOrg("Carrier"));
			consol.SetDefaultSendingForwarderAddress(GetOrg("SendingAgent"));
			consol.SetDefaultReceivingForwarderAddress(GetOrg("ReceivingAgent"));
			consol.JK_OA_CreditorAddress = GetOrg("Creditor").MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = GetOrg("ArrivalCTO").MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = GetOrg("UnpackDepot").MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = GetOrg("ArrivalUnpackCFSTransport").MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = GetOrg("DepartureCTO").MainAddress.PK;
			consol.JK_OA_PackDepotAddress = GetOrg("PackDepot").MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = GetOrg("DeparturePackCFSTransport").MainAddress.PK;
			consol.Transports[0].JW_CarrierBookingReference = "123";
			consol.Transports[0].CarrierPK = GetOrg("CarrierOnRoute1").PK;
			consol.Transports[0].CreditorPK = GetOrg("CreditorOnRoute1").PK;

			var route2 = consol.Transports.AddNew();
			route2.JW_RL_NKLoadPort = "USLAX";
			route2.JW_RL_NKDiscPort = "SGSIN";
			route2.JW_CarrierBookingReference = "456";
			route2.CarrierPK = GetOrg("CarrierOnRoute2").PK;
			route2.CreditorPK = GetOrg("CreditorOnRoute2").PK;

			AssertEquals("Prerequisite", true, consol.Transports.MostInterestingTransport.JW_IsLinked);

			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "20GP", "ATPT", "FCL", 5);

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var isMultiRouteEnabled = routeSetNumber >= 0;
			if (isMultiRouteEnabled)
			{
				var routingSupport = (IRoutingSupport)consol;
				var routeSetRatingRoute = routingSupport.TransportsIncludingRelated.RouteSets
					.Select(x => new RouteSetRatingRoute(x, routingSupport))
					.ToList();
				AssertEquals("Prerequisite", 2, routeSetRatingRoute.Count);
				autoRating = new ForwardingConsolRatingAdapter(routeSetRatingRoute[routeSetNumber], true);
			}
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			if (cw1Selected)
			{
				var costing = Helper.NewCosting(GetOrg("ServiceProvider"));
				costing.TH_GC = Env.CurrentCompanyPK;
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
				rateEntry1.RateLines.RemoveAndDeleteAll();
				var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;
				var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USALX", "SGSIN", "", "20GP");
				rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
				rateEntry2.RateLines.RemoveAndDeleteAll();
				var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine2.GetCalculator<UnitCalculator>().PerUnit = 200m;

				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1, rateEntry2));
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1, rateEntry2));

				// Select CW1 Costing for "ATPT"
				var atptContainers = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");
				atptContainers.SelectedRate = atptContainers.Rates.Single(x => x.WiseRateEntry == null && x.RateEntry.TI_OriginLRC == criteria.Origin.Code);
			}

			if (wiseRateSelected)
			{
				var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

				var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, GetOrg("ServiceProvider"), "");
				apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
				model.AddWiseRatesForTest(response);

				// Select Wise Rate for "GEN"
				var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
				genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteEnabled))
			{
				model.ApplyCarrierBackToJob();

				AssertEquals(expectedConsolCarrier.PK, consol.ShippingLine.PK);

				if (isMultiRouteEnabled)
				{
					var route = (consol as IRoutingSupport).TransportsIncludingRelated.RouteSets
						.FirstOrDefault(x => x.RouteSetNumber == criteria.RouteSetNumber);
					AssertEquals(expectedRouteCarrier.PK, route.ReferenceLeg.CarrierPK);
					AssertEquals(isLinked, route.ReferenceLeg.JW_IsLinked);
				}
				else
				{
					AssertEquals(true, consol.Transports.MostInterestingTransport.JW_IsLinked);
				}
			}

			OrgHeader GetOrg(string key) => orgs.TryGetValue(key, out var org) ? org : Helper.NewOrgHeader();
		}

		#endregion

		public void TestSendRatesRequest_CW1RateOnCreditor_WiseRateOnCarrier()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var costing = Helper.NewCosting(creditor);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(1, model.ContainerGroups.Count());
			var rates = model.ContainerGroups.First().Rates.Cast<ChooserRateEntry>();
			var rateCarrierScacCodes = rates.Select(x => x.WiseRateEntry != null ? x.WiseRateEntry.Carrier : x.RateEntry.ParentRatingHeader.Header.SCACCode.ToString())
				.Distinct().OrderBy(x => x).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("rate count", 2, rates.Count());
				AssertEquals("CW1 Rate on creditor is found", true, rates.Any(x => x.WiseRateEntry == null));
				AssertEquals("Wise Rate on carrier is found", true, rates.Any(x => x.WiseRateEntry != null));
				AssertEquals("Both rates from carrier and creditor shown", "ABCD, SCAC", string.Join(", ", rateCarrierScacCodes));
			});
		}

		public void TestSendRatesRequest_ShouldFindOnlyCW1Rate_When_CargoSphereIsNotActive()
		{
			AssertSendRateRequest(false, false);
		}

		public void TestSendRatesRequest_ShouldFindWiseRateAndCW1Rate_When_CargoSphereIsActive()
		{
			AssertSendRateRequest(true, true);
		}

		void AssertSendRateRequest(bool cargoSphereRegistry, bool expectWiseRateInRates)
		{
			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Sea,
					ContainerMode = ContainerModes.FCL,
					IsSubscriptionEnabled = cargoSphereRegistry
				}
			};

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var creditor = CreateCarrierOrg("ABCD");
				var carrier = CreateCarrierOrg("SCAC");

				var costing = Helper.NewCosting(creditor);
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA,
					"USLAX", "HKHKG", "", "20GP");
				rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
				rateEntry1.RateLines.RemoveAndDeleteAll();
				var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN,
					"AUD");
				rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

				var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

				var consol = CreateConsol();
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
				AddContainer(consol, "20GP", "GEN", "FCL", 3);

				Factory.Save();

				var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
				apiCosting1.Charges.Add(new Charge
				{
					ChargeCode = "FRT",
					Currency = "AUD",
					Unit = "CN",
					PerUnitRate = 10m
				});

				var logger = new TestLogger();
				WiseRatesProvider testWiseRatesProvider;
				if (expectWiseRateInRates)
				{
					var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
					testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
				}
				else
				{
					testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(new RatesSearchResponse());
				}

				var cw1Provider = new CW1RatesProvider(Factory, logger);
				var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);
				var filter = new RateChooserFilterStripBusinessObject(criteria);
				var model = new RateChooserModel(criteria, context);
				model.SendRatesRequest(filter, BuildRatesQuery(criteria));
				AssertEquals(1, model.ContainerGroups.Count());
				var rates = model.ContainerGroups.First().Rates.Cast<ChooserRateEntry>();
				var rateCarrierScacCodes = rates.Select(x =>
						x.WiseRateEntry != null
							? x.WiseRateEntry.Carrier
							: x.RateEntry.ParentRatingHeader.Header.SCACCode.ToString())
					.Distinct().OrderBy(x => x).ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("rate count", expectWiseRateInRates ? 2 : 1, rates.Count());
					AssertEquals("CW1 Rate on creditor is found", true, rates.Any(x => x.WiseRateEntry == null));
					AssertEquals("Wise Rate on carrier is found", expectWiseRateInRates, rates.Any(x => x.WiseRateEntry != null));
					AssertEquals("Rate from carrier shown", expectWiseRateInRates ? "ABCD, SCAC" : "ABCD",
						string.Join(", ", rateCarrierScacCodes));
				});
			}
		}

		public void TestSendRatesRequest_CW1RateOnCarrier_WiseRateOnCreditor()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var costing = Helper.NewCosting(carrier);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, creditor, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);

			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(1, model.ContainerGroups.Count());
			var rates = model.ContainerGroups.First().Rates.Cast<ChooserRateEntry>();
			var rateCarrierScacCodes = rates.Select(x => x.WiseRateEntry != null ? x.WiseRateEntry.Carrier : x.RateEntry.ParentRatingHeader.Header.SCACCode.ToString())
				.Distinct().OrderBy(x => x).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("rate count", 2, rates.Count());
				AssertEquals("CW1 Rate on carrier is found", true, rates.Any(x => x.WiseRateEntry == null));
				AssertEquals("Wise Rate on creditor is found", true, rates.Any(x => x.WiseRateEntry != null));
				AssertEquals("Both rates from carrier and creditor shown", "ABCD, SCAC", string.Join(", ", rateCarrierScacCodes));
			});
		}

		public void TestSendRatesRequest_CW1RateAndWiseRateOnCreditor()
		{
			var creditor = CreateCarrierOrg("ABCD");

			var costing = Helper.NewCosting(creditor);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = CreateCarrierOrg("SCAC").MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, creditor, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);

			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(1, model.ContainerGroups.Count());
			var rates = model.ContainerGroups.First().Rates.Cast<ChooserRateEntry>();
			var rateCarrierScacCodes = rates.Select(x => x.WiseRateEntry != null ? x.WiseRateEntry.Carrier : x.RateEntry.ParentRatingHeader.Header.SCACCode.ToString())
				.Distinct().OrderBy(x => x).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("rate count", 2, rates.Count());
				AssertEquals("CW1 Rate is found", true, rates.Any(x => x.WiseRateEntry == null));
				AssertEquals("Wise Rate is found", true, rates.Any(x => x.WiseRateEntry != null));
				AssertEquals("only rates from creditor shown", creditor.SCACCode, string.Join(", ", rateCarrierScacCodes));
			});
		}

		public void TestSendRatesRequest_ShouldBringAllCW1Costings_WhenThereIsNoServiceProviderFilter()
		{
			var serviceProviders = new[]
			{
				CreateCarrierOrg("AAAA"),
				CreateCarrierOrg("BBBB"),
				CreateCarrierOrg("CCCC"),
				CreateCarrierOrg("DDDD"),
				null, // Standard Costing
			};

			var fee = 0m;
			foreach (var serviceProvider in serviceProviders)
			{
				var header = Helper.NewCosting(serviceProvider);
				var entry = header.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				entry.TI_RH_NKCommodityCode = ZString.Empty;
				entry.RateLines.RemoveAndDeleteAll();
				var line = entry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				line.GetCalculator<UnitCalculator>().PerUnit = (fee += 100m);
			}

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = serviceProviders[0].MainAddress.PK;
			consol.JK_OA_CreditorAddress = serviceProviders[1].MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, serviceProviders[2], "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			var rateQuery = BuildRatesQuery(criteria);
			rateQuery.Carrier = Enumerable.Empty<RatesQueryCarrier>();
			model.SendRatesRequest(filter, rateQuery);

			AssertEquals(1, model.ContainerGroups.Count());
			var rates = model.ContainerGroups.First().Rates;
			var rateCarrierScacCodes = rates.Select(x => x.WiseRateEntry != null
					? x.WiseRateEntry.Carrier
					: x.RateEntry.ParentRatingHeader.Header?.SCACCode.ToString())
				.WhereNotNull()
				.Distinct()
				.ToArray();

			AssertEquals(5, rates.Count());
			AssertEquals(1, rates.Count(x => x.WiseRateEntry != null));
			AssertEquals(4, rates.Count(x => x.WiseRateEntry == null));
			AssertContainsExactElementsInAnyOrder(
				new[] { "AAAA", "BBBB", "CCCC", "DDDD" },
				rateCarrierScacCodes
			);
		}

		public void TestSendRatesRequest_WhenRatesQueryIsNotValidForRatesService_ShouldSearchCW1RatesFromFilterStripValues_ContractNumber()
		{
			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Sea,
					ContainerMode = ContainerModes.FCL,
					IsSubscriptionEnabled = false
				}
			};

			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				// a critical condition for making ratesQuery invalid for CS search: carrier does not have SCAC code
				var carrier = CreateCarrierOrg("");

				var costing = Helper.NewCosting(carrier);
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				rateEntry1.TI_ContractNumber = "AAA";
				rateEntry1.RateLines.RemoveAndDeleteAll();
				var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

				var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				rateEntry2.TI_ContractNumber = "BBB";
				rateEntry2.RateLines.RemoveAndDeleteAll();
				var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine2.GetCalculator<UnitCalculator>().PerUnit = 200m;

				Factory.Save();

				var consol = CreateConsol();
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
				AddContainer(consol, "20GP", "GEN", "FCL", 1);

				consol.JK_CarrierContractNumber = "AAA";

				var logger = new TestLogger();
				var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(new RatesSearchResponse());
				var cw1Provider = new CW1RatesProvider(Factory, logger);

				var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);

				var model = new RateChooserModel(criteria, context);
				var filter = new RateChooserFilterStripBusinessObject(criteria);

				var carrierFilter = (ModuleGuidFilter)filter[RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider];
				carrierFilter.IsActive = true;
				carrierFilter.Property = carrier.PK;

				var locationFilter = (ModuleLocationFilter)filter[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				locationFilter.IsActive = true;
				locationFilter.Property1 = "USLAX";
				locationFilter.Property2 = "HKHKG";

				var contractNumberFilter = (WiseRatesModuleTextFilter)filter[RateEntryFilterUtility.Constants.Codes.CarrierContractNumber];
				contractNumberFilter.IsActive = true;

				contractNumberFilter.Property = "AAA";
				var (ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(context.Logger);
				model.SendRatesRequest(filter, ratesQuery, isValidForRatesService);
				var rates = model.ContainerGroups;
				var matchedEntry = rates.Single().Rates.Single().RateEntry;
				AssertEquals("Expected the contract number to match 'AAA'.", "AAA", matchedEntry.TI_ContractNumber);

				contractNumberFilter.Property = "BBB";
				(ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(context.Logger);
				model.SendRatesRequest(filter, ratesQuery, isValidForRatesService);
				rates = model.ContainerGroups;
				matchedEntry = rates.Single().Rates.Single().RateEntry;
				AssertEquals("Expected the contract number to match 'BBB'.", "BBB", matchedEntry.TI_ContractNumber);
			}
		}

		public void TestSendRatesRequest_WhenRatesQueryIsNotValidForRatesService_ShouldSearchCW1RatesFromFilterStripValues_InvalidContainerType()
		{
			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				// a critical condition for making ratesQuery invalid for CS search: carrier does not have SCAC code
				var carrier = CreateCarrierOrg("");

				var costing = Helper.NewCosting(carrier);
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				rateEntry1.RateLines.RemoveAndDeleteAll();
				var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

				var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
				rateEntry2.RateLines.RemoveAndDeleteAll();
				var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
				rateLine2.GetCalculator<UnitCalculator>().PerUnit = 200m;

				Factory.Save();

				var consol = CreateConsol();
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
				AddContainer(consol, GP20.RC_Code, "GEN", "FCL", 1);
				AddContainer(consol, GP40.RC_Code, "GEN", "FCL", 1);

				var logger = new TestLogger();
				var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(new RatesSearchResponse());
				var cw1Provider = new CW1RatesProvider(Factory, logger);

				var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);

				var model = new RateChooserModel(criteria, context);
				var filter = new RateChooserFilterStripBusinessObject(criteria);

				var locationFilter = (ModuleLocationFilter)filter[RateEntryFilterUtility.Constants.Codes.OriginDestination];
				locationFilter.IsActive = true;
				locationFilter.Property1 = "USLAX";
				locationFilter.Property2 = "HKHKG";

				var containerTypeFilter = (ModuleTextFilter)filter[RateEntryFilterUtility.Constants.Codes.ContainerType];
				containerTypeFilter.IsActive = true;

				var hc40Container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC");
				AssertNotNull("hc40Container", hc40Container);

				containerTypeFilter.Property = "40HC";
				var (ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(context.Logger);
				AssertNotNull("ratesQuery", ratesQuery);

				model.SendRatesRequest(filter, ratesQuery, isValidForRatesService);
				var rates = model.ContainerGroups;

				AssertEquals(
					"No rate with container 40HC",
					0,
					rates.SelectMany(x => x.Rates).Where(r => r.RateEntry.TI_RC == hc40Container.PK).ToArray().Length
				);

				var nonExistentContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "???");
				AssertNull("nonExistentContainer", nonExistentContainer);

				containerTypeFilter.Property = "???";
				(ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(context.Logger);

				AssertNull("ratesQuery", ratesQuery);
				AssertEquals(false, isValidForRatesService);
				AssertCollectionContains(
					"Error: None of requested containers can be used for rates search.",
					"Error: None of requested containers can be used for rates search.",
					context.Logger.DumpLog().ToArray()
				);

				model.SendRatesRequest(filter, ratesQuery, isValidForRatesService);
				rates = model.ContainerGroups;

				AssertEquals(
					"No rate with nonexistent container code ???",
					0,
					rates.SelectMany(x => x.Rates).ToArray().Length
				);

				filter.AddTextFilterStrip(RateEntryFilterUtility.Constants.Codes.ContainerType, "20GP");
				(ratesQuery, isValidForRatesService) = filter.BuildRatesQuery(context.Logger);
				AssertNotNull("ratesQuery", ratesQuery);
			}
		}

		public void TestSendRatesRequest_WhenThereIsExistingCostWithSpotRatingBehaviour_ShouldNotAffectAnyThing()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddFlatRateLine(charge1.AC_Code, 100m);
			rateEntry.AddFlatRateLine(charge2.AC_Code, 400m);

			var consol = CreateConsol();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(GP20, TransportProvider1, "");
			apiCosting.Charges.Add(new Charge { ChargeCode = "CC3", Currency = "AUD", Unit = "CN", FlatRate = 600m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);

			var logger = new TestLogger();
			var wiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, wiseRatesProvider, null, true);

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);

			var expectedCharges = new[]
			{
				"CC1|100",
				"CC2|400",
				"CC3|600",
			};

			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));

			var rates = model.ContainerGroups.First().Rates.Cast<ChooserRateEntry>();
			var results = rates
				.SelectMany(r => r.CalculatedResult.Select(cr => $"{cr.ChargeCode.AC_Code}|{cr.Amount}"))
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Results should match the expected charges.",
				expectedCharges,
				results
			);
		}

		public void TestValidateChargeCodes()
		{
			var creditor = CreateCarrierOrg("SCAC");
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var chargeFRT2 = Helper.ChargeCodes.New("FRT2", "FRT2 Desc", UnitCalculator.Code);
			var chargeFRT3 = Helper.ChargeCodes.New("FRT3", "FRT3 Desc", UnitCalculator.Code);
			AssertEquals(Env.CurrentCompanyPK, chargeFRT2.AC_GC);
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, creditor, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "UNI1", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			apiCosting1.Charges.Add(new Charge { ChargeCode = "UNI2", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);

			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddWiseRatesForTest(response);
			var containerGroup = model.ContainerGroups.First();
			containerGroup.SelectedRate = containerGroup.RateCollection[0];
			model.ValidateAllChargeCodesAreMapped(Enumerable.Empty<ZString>());
			AssertEquals("UNI1, UNI1 Desc; UNI2, UNI2 Desc", string.Join("; ", model.UnmappedCharges.Cast<UniversalChargeCodeMapBizo>()
				.OrderBy(x => x.Code)
				.Select(x => x.Code + ", " + x.Description)));

			// Map the unmapped
			var mappingFactory = new BusinessObjectFactory();
			var unmappedCharges = model.UnmappedCharges.CopyCodeAndDescriptionToAnotherFactory(mappingFactory);
			var chargeToMap1 = unmappedCharges.Cast<UniversalChargeCodeMapBizo>().First(x => x.Code == "UNI1");
			var chargeToMap2 = unmappedCharges.Cast<UniversalChargeCodeMapBizo>().First(x => x.Code == "UNI2");
			chargeToMap1.LocalChargeCodePk = chargeFRT2.PK;
			chargeToMap2.LocalChargeCodePk = chargeFRT3.PK;
			AssertNoErrors(chargeToMap1);
			AssertNoErrors(chargeToMap2);
			unmappedCharges.ApplyUniversalCodeToChargeCodeForSaving();
			unmappedCharges.Factory.Save();
			AssertEquals("PRE", "UNI1", chargeToMap1.LocalChargeCode.UniversalChargeCodeMappings);
			AssertEquals("PRE", "UNI2", chargeToMap2.LocalChargeCode.UniversalChargeCodeMappings);

			model.UpdateMappings();
			model.ValidateAllChargeCodesAreMapped(Enumerable.Empty<ZString>());
			AssertEquals("", string.Join("; ", model.UnmappedCharges.Cast<UniversalChargeCodeMapBizo>()
				.OrderBy(x => x.Code)
				.Select(x => x.Code + ", " + x.Description)));
		}

		public void TestSelectedRatesHaveDifferentNamedAccounts()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "ATPT", ContainerModes.FCL, 5);

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "WOOLIES", "COLES" });
			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "COLES", "ALDI" });
			var apiCosting3 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "ALDI" });
			var apiCosting4 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(Array.Empty<string>());

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2, apiCosting3, apiCosting4 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			var tab2 = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);

			AssertEquals("WOOLIES/COLES vs COLES/ALDI => Coles", false, model.SelectedRatesHaveDifferentNamedAccounts);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("WOOLIES/COLES vs ALDI", true, model.SelectedRatesHaveDifferentNamedAccounts);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("WOOLIES/COLES vs blank", true, model.SelectedRatesHaveDifferentNamedAccounts);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("blank vs blank", false, model.SelectedRatesHaveDifferentNamedAccounts);
		}

		public void TestAllNamedAccounts()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "ATPT", ContainerModes.FCL, 5);

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "WOOLIES", "COLES", "IGA" });
			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "COLES", "ALDI", "IGA" });
			var apiCosting3 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(new[] { "ALDI" });
			var apiCosting4 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithNamedAccounts(Array.Empty<string>());

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2, apiCosting3, apiCosting4 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			var tab2 = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);
			AssertEquals("WOOLIES/COLES/IGA vs COLES/ALDI/IGA", "ALDI, COLES, IGA, WOOLIES", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			AssertEquals("WOOLIES/COLES/IGA vs WOOLIES/COLES/IGA", "COLES, IGA, WOOLIES", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("WOOLIES/COLES/IGA vs ALDI", "ALDI, COLES, IGA, WOOLIES", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting2);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("COLES/ALDI/IGA vs ALDI", "ALDI, COLES, IGA", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("WOOLIES/COLES/IGA vs blank", "COLES, IGA, WOOLIES", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("blank vs blank", "", string.Join(", ", model.AllNamedAccounts.OrderBy(x => x)));
		}

		public void TestApplyNamedAccountBackToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			model.ApplyNamedAccountBackToJob("COLES");
			AssertEquals("COLES", string.Join(", ", consol.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)));

			model.ApplyNamedAccountBackToJob("ALDI");
			AssertEquals("ALDI", string.Join(", ", consol.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)));

			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "WOOLIES");
			AssertEquals("PRE:", "ALDI, WOOLIES", string.Join(", ", consol.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)));

			model.ApplyNamedAccountBackToJob("IGA");
			AssertEquals("replaces first if more than one", "IGA, WOOLIES", string.Join(", ", consol.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)));
		}

		#region ApplyCarrierQuoteNumberBackToJob

		public void TestApplyCarrierQuoteNumberBackToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);

			var apiCosting20GP1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var apiCosting20GP2 = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierQuoteNumber("PI000GP20_2");
			var apiCosting40GP1 = ChooserHelper.CreateApiRate("40GP", carrier, "");
			apiCosting40GP1.ProviderRateId = "PI000GP40_1";
			var apiCosting40GP2 = ChooserHelper.CreateApiRate("40GP", carrier, "")
				.WithCarrierQuoteNumber("PI000GP40_2");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP1, apiCosting20GP2, apiCosting40GP1, apiCosting40GP2 });
			model.AddWiseRatesForTest(response);

			var container20GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			var container40GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40GP");

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry == apiCosting20GP1);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry == apiCosting40GP1);
			model.ApplyCarrierQuoteNumberBackToJob();

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals("should not have any CQN entry", 0, values.Length);

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry == apiCosting20GP2);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry == apiCosting40GP2);
			model.ApplyCarrierQuoteNumberBackToJob();

			values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertContainsExactElementsInAnyOrder("should have both CQN entries", new ZString[] { "PI000GP20_2", "PI000GP40_2" }, values);
		}

		public void TestApplyCarrierQuoteNumberBackToJob_ForSpotFormat()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40HC", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40PL", "GEN", ContainerModes.FCL);

			var apiCosting20GP = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierQuoteNumber("P_257220376_P020kpe0");
			var apiCosting40GP = ChooserHelper.CreateApiRate("40GP", carrier, "")
				.WithCarrierQuoteNumber("PI000GP20_2");
			var apiCosting40HC = ChooserHelper.CreateApiRate("40HC", carrier, "")
				.WithCarrierQuoteNumber("P_257220381_P020kpi2");
			var apiCosting40PL = ChooserHelper.CreateApiRate("40PL", carrier, "")
				.WithCarrierQuoteNumber("P_257220386_P020kpg7");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP, apiCosting40GP, apiCosting40HC, apiCosting40PL });
			model.AddWiseRatesForTest(response);

			var container20GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			var container40GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40GP");
			var container40HC = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40HC");
			var container40PL = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40PL");

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry == apiCosting20GP);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry == apiCosting40GP);
			container40HC.SelectedRate = container40HC.Rates.Single(x => x.WiseRateEntry == apiCosting40HC);
			container40PL.SelectedRate = container40PL.Rates.Single(x => x.WiseRateEntry == apiCosting40PL);
			model.ApplyCarrierQuoteNumberBackToJob();

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertContainsExactElementsInAnyOrder("should have 2 CQN entries", new ZString[] { "P_257220376/P_257220381/P_257220386", "PI000GP20_2" }, values);
			var container20GPValue = consol.Containers?.Cast<ForwardingContainer>().FirstOrDefault(x => x.JC_RC == container20GP.ContainerRef.PK);
			AssertContainsExactElementsInAnyOrder("should have CQN entries", new ZString[] { "P_257220376_P020kpe0" }, container20GPValue.AdditionalReferenceNumbers.GetAllReferenceNumbersByType("CQN"));
			var container40GPValue = consol.Containers?.Cast<ForwardingContainer>().FirstOrDefault(x => x.JC_RC == container40GP.ContainerRef.PK);
			AssertContainsExactElementsInAnyOrder("should have CQN entries", new ZString[] { "PI000GP20_2" }, container40GPValue.AdditionalReferenceNumbers.GetAllReferenceNumbersByType("CQN"));
			var container40HCValue = consol.Containers?.Cast<ForwardingContainer>().FirstOrDefault(x => x.JC_RC == container40HC.ContainerRef.PK);
			AssertContainsExactElementsInAnyOrder("should have 2 CQN entries", new ZString[] { "P_257220381_P020kpi2" }, container40HCValue.AdditionalReferenceNumbers.GetAllReferenceNumbersByType("CQN"));
			var container40PLValue = consol.Containers?.Cast<ForwardingContainer>().FirstOrDefault(x => x.JC_RC == container40PL.ContainerRef.PK);
			AssertContainsExactElementsInAnyOrder("should have 2 CQN entries", new ZString[] { "P_257220386_P020kpg7" }, container40PLValue.AdditionalReferenceNumbers.GetAllReferenceNumbersByType("CQN"));
		}

		public void TestApplyCarrierQuoteNumberBackToJob_ForSpotFormat_MaxLength()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40HC", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40PL", "GEN", ContainerModes.FCL);

			var apiCosting20GP = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierQuoteNumber("P_257220376_P020kpe0");
			var apiCosting40GP = ChooserHelper.CreateApiRate("40GP", carrier, "")
				.WithCarrierQuoteNumber("P_257220385_P020kpj8");
			var apiCosting40HC = ChooserHelper.CreateApiRate("40HC", carrier, "")
				.WithCarrierQuoteNumber("P_257220381_P020kpi2");
			var apiCosting40PL = ChooserHelper.CreateApiRate("40PL", carrier, "")
				.WithCarrierQuoteNumber("P_257220386_P020kpg7");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP, apiCosting40GP, apiCosting40HC, apiCosting40PL });
			model.AddWiseRatesForTest(response);

			var container20GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			var container40GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40GP");
			var container40HC = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40HC");
			var container40PL = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40PL");

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry == apiCosting20GP);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry == apiCosting40GP);
			container40HC.SelectedRate = container40HC.Rates.Single(x => x.WiseRateEntry == apiCosting40HC);
			container40PL.SelectedRate = container40PL.Rates.Single(x => x.WiseRateEntry == apiCosting40PL);
			model.ApplyCarrierQuoteNumberBackToJob();

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertContainsExactElementsInAnyOrder("should have 1 CQN entries", new ZString[] { "P_257220376/P_257220385/P_257220381" }, values);
		}

		public void TestApplyCarrierQuoteNumberBackToJob_ForSpotFormat_SingleCQN()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", ContainerModes.FCL);

			var apiCosting20GP = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierQuoteNumber("P_257220376_P020kpe0");
			var apiCosting40GP = ChooserHelper.CreateApiRate("40GP", carrier, "")
				.WithCarrierQuoteNumber("PI000GP20_2");

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting20GP, apiCosting40GP });
			model.AddWiseRatesForTest(response);

			var container20GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "20GP");
			var container40GP = model.ContainerGroups.Single(x => x.ContainerRef.RC_Code == "40GP");

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry == apiCosting20GP);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry == apiCosting40GP);

			model.ApplyCarrierQuoteNumberBackToJob();

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertContainsExactElementsInAnyOrder("should have 1 CQN entries and it has been correctly truncated.", new ZString[] { "P_257220376", "PI000GP20_2" }, values);
		}

		public void TestApplyCarrierQuoteNumberBackToJob_ForNonContainerised_LCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL);
			var apiCosting2 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL).WithCarrierQuoteNumber("PI000GP20_2");
			apiCosting2.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1, apiCosting2);
			model.AddWiseRatesForTest(response);

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			model.ApplyCarrierQuoteNumberBackToJob();

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals("should not have any CQN entry", 0, values.Length);

			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry == apiCosting2);
			model.ApplyCarrierQuoteNumberBackToJob();

			values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertContainsExactElementsInAnyOrder("should have CQN entries", new ZString[] { "PI000GP20_2" }, values);
		}

		#endregion

		public void TestSelectedRatesHaveDifferentCarrierContractNumbers()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "ATPT", ContainerModes.FCL, 5);

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithContractNumber("CON1");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithContractNumber("CON2");
			apiCosting2.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var apiCosting3 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithContractNumber(string.Empty);
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var apiCosting4 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithContractNumber(null);
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var apiCosting5 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "CO5")
				.WithContractNumber("con1");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2, apiCosting3, apiCosting4, apiCosting5 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			var tab2 = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);

			AssertEquals("CON1 vs CON2", true, model.SelectedRatesHaveDifferentCarrierContractNumbers);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("CON1 vs blank", true, model.SelectedRatesHaveDifferentCarrierContractNumbers);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("CON1 vs null", true, model.SelectedRatesHaveDifferentCarrierContractNumbers);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("blank vs blank", false, model.SelectedRatesHaveDifferentCarrierContractNumbers);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("null vs blank", false, model.SelectedRatesHaveDifferentCarrierContractNumbers);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting5);
			AssertEquals("Con1 vs con1 (same except for case)", false, model.SelectedRatesHaveDifferentCarrierContractNumbers);
		}

		public void TestSelectedRatesHaveDifferentCarrierServiceLevel()
		{
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL1", "UL1");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL2", "UL2");
			Factory.Save();

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "ATPT", ContainerModes.FCL, 5);

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithCarrierServiceLevel("UL1")
				.WithCharge();
			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithCarrierServiceLevel("UL2")
				.WithCharge();
			var apiCosting3 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithCarrierServiceLevel("UL1")
				.WithCharge();
			var apiCosting4 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "")
				.WithCarrierServiceLevel("")
				.WithCharge();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2, apiCosting3, apiCosting4 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			var tab2 = model.ContainerGroups.Single(x => x.CommodityCode == "ATPT");

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);
			AssertEquals("PRE:", true, tab1.SelectedRate.CarrierServiceLevelIsMapped);
			AssertEquals("PRE:", true, tab2.SelectedRate.CarrierServiceLevelIsMapped);
			AssertEquals("CL1 vs CL2", true, model.SelectedRatesHaveDifferentCarrierServiceLevel);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			AssertEquals("CL1 vs CL1", false, model.SelectedRatesHaveDifferentCarrierServiceLevel);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("CL1 vs blank", true, model.SelectedRatesHaveDifferentCarrierServiceLevel);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting4);
			AssertEquals("blank vs blank", false, model.SelectedRatesHaveDifferentCarrierServiceLevel);
		}

		public void TestApplyServiceLevelBackToJobIfNeeded()
		{
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL1", "UL1");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL2", "UL2");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierServiceLevel("UL1")
				.WithCharge();
			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierServiceLevel("UL2")
				.WithCharge();
			var apiCosting3 = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCarrierServiceLevel("")
				.WithCharge();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2, apiCosting3 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			model.ApplyServiceLevelBackToJobIfNeeded();
			AssertEquals("CL1", consol.JK_AWBServiceLevel);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting2);
			model.ApplyServiceLevelBackToJobIfNeeded();
			AssertEquals("CL2", consol.JK_AWBServiceLevel);

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting3);
			model.ApplyServiceLevelBackToJobIfNeeded();
			AssertEquals("no change if no service level selected", "CL2", consol.JK_AWBServiceLevel);
		}

		public void TestApplySpotBookingTermsBackToJobIfNeeded()
		{
			var carrier = CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "FAK", ContainerModes.FCL, 3);

			consol.Notes.AddNew(true, "Some Descriptions", "Some notes");

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "")
				.WithCharge();

			apiCosting1.BookingInfo = new BookingInfo()
			{
				BookingTerms = new BookingTerms
				{
					Items = new[]
					{
						new BookingTermItem()
						{
							Name = "Amendment Fee",
							Currency = "USD",
							Fee = 180M,
							Type = "Amendment Fee"
						}
					}
				}
			};

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			model.AddWiseRatesForTest(response);

			var tab1 = model.ContainerGroups.First();
			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);

			var tab2 = model.ContainerGroups.Skip(1).First();
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting1);

			model.ApplySpotBookingTermsBackToJobIfNeeded();

			var actualNotes = consol.Notes.GetAllNotes()
				.Select(note => $"{((StmNote)note).ST_Description}|{((StmNote)note).ST_NoteText}")
				.ToArray();

			var expectedNotes = new[]
			{
				"Some Descriptions|Some notes",
				$"{PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description}|" +
@"Spot Booking Terms for '20GP (GEN)': 
Amendment Fee                        USD 180
                                     
Spot Booking Terms for '20GP (FAK)': 
Amendment Fee                        USD 180
                                     
" };

			AssertContainsExactElementsInAnyOrder("The notes in the consol should match expected spot booking terms and fees.", expectedNotes, actualNotes);
		}

		public void TestGetSelectedRate_CarrierServiceLevelChanged()
		{
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL1");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL2");
			var bolCharge = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CL1", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], FlatCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 100;
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "CL2", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], FlatCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 200;
			var originEntryCL1 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "USLAX", "HKHKG", "CL1", "");
			originEntryCL1.TI_RH_NKCommodityCode = ZString.Empty;
			originEntryCL1.RateLines.RemoveAndDeleteAll();
			var originLineCL1 = originEntryCL1.AddRateLine(bolCharge, FlatCalculator.Code, "", "AUD");
			originLineCL1.GetCalculator<FlatCalculator>().BaseRate = 40m;
			var originEntryCL2 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "USLAX", "HKHKG", "CL2", "");
			originEntryCL2.TI_RH_NKCommodityCode = ZString.Empty;
			originEntryCL2.RateLines.RemoveAndDeleteAll();
			var originLineCL2 = originEntryCL2.AddRateLine(bolCharge, FlatCalculator.Code, "", "AUD");
			originLineCL2.GetCalculator<FlatCalculator>().BaseRate = 75m;
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_AWBServiceLevel = "CL1";
			AssertNoErrors("PRE", consol.JK_AWBServiceLevelInfo);
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			{
				var logger = new ElementaryLogger();
				var context = new RatingContext(logger);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);

				// Select rate with CL2 service level
				var model = new RateChooserModel(criteria, context);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry2));
				var tab1 = model.ContainerGroups.First();
				tab1.SelectedRate = tab1.Rates.Single(x => x.RateEntry.TI_PL_NKCarrierServiceLevel == "CL2");
				model.ApplyServiceLevelBackToJobIfNeeded();
				var selected = model.GetSelectedRate();
				CombineAssertions(() =>
				{
					AssertEquals("CL2", consol.JK_AWBServiceLevel);
					var actual = selected.Select(a => $"{a.ChargeCode.AC_Code}|{a.Amount}").ToArray();
					var expected = new[]
					{
						"FRT|200",
						"BOL|75.00"
					};
					AssertContainsExactElementsInAnyOrder(expected, actual);
				});

				// Select rate with CL1 service level
				model = new RateChooserModel(criteria, context);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry2));
				tab1 = model.ContainerGroups.First();
				tab1.SelectedRate = tab1.Rates.Single(x => x.RateEntry.TI_PL_NKCarrierServiceLevel == "CL1");
				model.ApplyServiceLevelBackToJobIfNeeded();
				selected = model.GetSelectedRate();
				CombineAssertions(() =>
				{
					AssertEquals("CL1", consol.JK_AWBServiceLevel);
					var actual = selected.Select(a => $"{a.ChargeCode.AC_Code}|{a.Amount}").ToArray();
					var expected = new[]
					{
						"FRT|100",
						"BOL|40.00"
					};
					AssertContainsExactElementsInAnyOrder(expected, actual);
				});
			}
		}

		public void TestGetSelectedRate_WiseRateSelected_OnlyIncludeCW1ChargesFromOtherOrg()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var agent = CreateCarrierOrg("OOLU");
			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BO2", "Bill of Lading Also", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var costingCarrier = Helper.NewCosting(carrier);
			costingCarrier.TH_GC = Env.CurrentCompanyPK;
			var carrierFrtEntry = AddFCLSEARateEntryWithPerContainerCharge(costingCarrier, "USLAX", "HKHKG", "", ZString.Empty, Helper.ChargeCodes["FRT"], "20GP", 100m);
			var carrierOriginEntry = AddOriginFlatCharge(costingCarrier, "USLAX", "", bolCharge1, 40m);

			var costingAgent = Helper.NewCosting(agent);
			costingAgent.TH_GC = Env.CurrentCompanyPK;
			var agentFrtEntry = AddFCLSEARateEntryWithPerContainerCharge(costingAgent, "USLAX", "HKHKG", "", ZString.Empty, Helper.ChargeCodes["BAF"], "20GP", 35);
			var agentOriginEntry = AddOriginFlatCharge(costingAgent, "USLAX", "", bolCharge2, 34m);
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "").WithCharge(perUnitRate: 10);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			{
				var logger = new ElementaryLogger();
				var context = new RatingContext(logger);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);
				var model = new RateChooserModel(criteria, context);
				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
				model.AddWiseRatesForTest(response);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", carrierFrtEntry));
				var tab1 = model.ContainerGroups.First();
				tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting);

				var selected = model.GetSelectedRate();
				var actual = selected
					.Select(a => $"{a.ChargeCode.AC_Code}|{a.Amount}")
					.ToArray();

				var expected = new[]
				{
					"FRT|30",
					"BO2|34.00"
				};

				AssertContainsExactElementsInAnyOrder(
					"Selected rates should match the expected charges",
					expected,
					actual
				);
			}
		}

		public void TestGetSelectedRate_WiseRateSelected_AllCW1ChargesFromOtherOrgShouldBeNonFCLFreightCost()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = CreateCarrierOrg("SCAC");
			var creditor = CreateCarrierOrg("ABCD");

			var consol = CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(creditor);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["ORG"], FlatCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 200m;

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var logger = new TestLogger();
				var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);
				var model = new RateChooserModel(criteria, context);
				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
				model.AddWiseRatesForTest(response);
				// Select Wise Rate for "GEN"
				var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
				genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

				var actualSelectedRates = model.GetSelectedRate();

				var actual = actualSelectedRates
					.Select(a => $"{a.ChargeCode.AC_Code}|{a.Amount}")
					.ToArray();

				var expected = new[]
				{
					"FRT|30",
					"ORG|200.00"
				};

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		[TestDate(2020, 04, 25)]
		public void TestGetSelectedRate_CSRatesWithInclusiveChargesShouldOverrideSimilarLocalCW1Charges()
		{
			var carrier = TransportProvider1;
			var creditor = TransportProvider2;

			var cyrcCharge = Helper.ChargeCodes.NewConsolChargeCode("CYRC", "Cont Yard Rec Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			cyrcCharge.AC_GC = Env.CurrentCompanyPK;

			var frtCharge = Helper.ChargeCodes["FRT"];

			var csRate = ChooserHelper.CreateApiRate("20GP", carrier, "");
			csRate.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 8m });
			csRate.Charges.Add(new Charge { ChargeCode = "CYRC", Currency = "AUD", ChargeType = Api.Model.ChargeType.Included, FreightInclusiveCarriageCharge = "FRT" });
			csRate.Provider = WRConstants.RateProviders.CargoSphere;

			var localCost = Helper.NewCosting(creditor);
			var localCYRC = localCost.AddRateEntry(RatingConstants.RateCategory.ORG, TransportModes.Sea, "USLAX", "SGSIN", "", GP20.RC_Code);
			localCYRC.RateLines.RemoveAndDeleteAll();
			localCYRC.AddRateLine(cyrcCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 10m;

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol);
			consol.Transports[0].CarrierPK = creditor.PK;

			Factory.Save();

			var expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 8m,
							CalculationSingleLineDescription = "FRT: 1 20GP Container(s) @ AUD 8.00/Container",
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Cont Yard Rec Charge",
							Amount = 0m,
							CalculationSingleLineDescription = "CYRC: Freight Inclusive Calculator",
						}
				};

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(csRate);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", localCYRC));
			model.AddWiseRatesForTest(response);
			// Select Wise Rate for "GEN"
			var genContainers = model.ContainerGroups.Single(x => x.CommodityCode == "GEN");
			genContainers.SelectedRate = genContainers.Rates.Single(x => x.WiseRateEntry != null);

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());

			using (_Rating.Start(mockGuiInteractor.Object))
			{
				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;

				AssertRatingResults(expected, result);
			}
		}

		public void TestGetSelectRate_WhereThereIsExistingCostsWithSAAorSBABehaviour_ShouldFilterChargesHavingFreightChargeGroup()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddFlatRateLine(charge1.AC_Code, 100m);
			rateEntry.AddFlatRateLine(charge2.AC_Code, 400m);
			rateEntry.AddFlatRateLine(charge3.AC_Code, 600m);

			var consol = CreateConsol();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expectedCharges = new[]
			{
				new { ChargeCode = new ZString("CC2"), Amount = new ZDecimal(400m) },
			};

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var logger = new TestLogger();
				var cw1Provider = new CW1RatesProvider(Factory, logger);
				var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, null, null, true);

				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);

				var model = new RateChooserModel(criteria, context);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry));

				var genContainers = model.ContainerGroups.Single();
				genContainers.SelectedRate = genContainers.Rates.Single();

				var selectedRates = model.GetSelectedRate()
					.Select(a => new { ChargeCode = a.ChargeCode.AC_Code, a.Amount })
					.Select(a => $"{a.ChargeCode}|{a.Amount}")
					.ToArray();

				var expectedResult = expectedCharges
					.Select(a => $"{a.ChargeCode}|{a.Amount}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Selected rates should match the expected charges.",
					expectedResult,
					selectedRates
				);
			}
		}

		public void TestGetSelectRate_WhereThereIsExistingCostsWithSAForSBFBehaviour_ShouldFilterRatesWithSameChargeCode()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddFlatRateLine(charge1.AC_Code, 100m);
			rateEntry.AddFlatRateLine(charge2.AC_Code, 400m);
			rateEntry.AddFlatRateLine(charge3.AC_Code, 600m);

			var consol = CreateConsol();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.FreightAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expectedCharges = new[]
			{
				new { ChargeCode = new ZString("CC1"), Amount = new ZDecimal(100m) },
				new { ChargeCode = new ZString("CC2"), Amount = new ZDecimal(400m) },
			};

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var logger = new TestLogger();
				var cw1Provider = new CW1RatesProvider(Factory, logger);
				var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, null, null, true);

				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);

				var model = new RateChooserModel(criteria, context);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry));

				var genContainers = model.ContainerGroups.Single();
				genContainers.SelectedRate = genContainers.Rates.Single();

				var selectedRates = model.GetSelectedRate()
					.Select(a => new { ChargeCode = a.ChargeCode.AC_Code, a.Amount })
					.Select(a => $"{a.ChargeCode}|{a.Amount}")
					.ToArray();

				var expectedResult = expectedCharges
					.Select(e => $"{e.ChargeCode}|{e.Amount}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Selected rates should match the expected charges after filtering.",
					expectedResult,
					selectedRates
				);
			}
		}

		// TODO for Saeed: Please fix the test. It is useless and doesn't test what it supposed to test. It was asserting some collection with active charges
		// which has not been using by business object and was using only by the tests. The actual collection used by business logic doesn't get updated by the
		// test as you add WiseRates charges using AddWiseRatesForTest instead of the proper way by mocking the Rates Service. It means that you bypass some GUI
		// stuff which subscribes to IsActive event change on charges and does the changes you expect. But, sycne you bypass all this, proper event handlers are not
		// called and nothing gets updated. The proper assertions added by me which test actual charges returned to business logic prooves this.

		// public void TestGetSelectedRate_OptionalCharges()
		// {
		// 	var carrier = CreateCarrierOrg("SCAC");
		// 	var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
		// 	bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
		// 	var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2 - Optional", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
		// 	bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
		// 	var bolCharge3 = Helper.ChargeCodes.NewConsolChargeCode("BL3", "Doc Fee 3 - Optional", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
		// 	bolCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
		// 	var frtCharge2 = Helper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
		// 	frtCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
		// 	var frtCharge3 = Helper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
		// 	frtCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
		// 	Factory.Save();
		//
		// 	var consol = CreateConsol();
		// 	consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
		// 	consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
		// 	AddContainer(consol, "20GP", "GEN", Enterprise.Core.Constants.ContainerModes.FCL, 3);
		//
		// 	var apiCosting1 = RateChooserTestHelper.CreateApiRate("20GP", carrier, "");
		// 	RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);
		// 	var frtChargeOptional2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 200)
		// 		.OfType(Api.Model.ChargeType.Optional);
		// 	var frtChargeOptional3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR3", 70)
		// 		.OfType(Api.Model.ChargeType.Optional);
		// 	var unmappedFrtOptional = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "UFR", 10)
		// 		.OfType(Api.Model.ChargeType.Optional);
		// 	RateChooserTestHelper.AddFlatCharge(apiCosting1, "BOL", 50, "ORG");
		// 	var docChargeOptional2 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BL2", 30, "DST")
		// 		.OfType(Api.Model.ChargeType.Optional);
		// 	var docChargeOptional3 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BL3", 30, "DST")
		// 		.OfType(Api.Model.ChargeType.Optional);
		// 	var unmappedDocOptional = RateChooserTestHelper.AddFlatCharge(apiCosting1, "UBL", 30, "DST")
		// 		.OfType(Api.Model.ChargeType.Optional);
		//
		// 	var apiCosting2 = RateChooserTestHelper.CreateApiRate("20GP", carrier, "");
		// 	RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);
		// 	var wiseRates = new[] { apiCosting1, apiCosting2 };
		//
		// 	var logger = new ElementaryLogger();
		// 	var context = new RatingContext(logger);
		// 	var autoRating = consol.RatingAdapter;
		// 	var autoRatingInfo = new AutoRatingProxy(autoRating);
		// 	var criteria = new RatingCriteria(autoRatingInfo, Factory);
		// 	var model = new RateChooserModel(criteria, context);
		// 	var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
		// 	model.AddWiseRatesForTest(response);
		// 	var tab1 = model.ContainerGroups.First();
		// 	tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
		// 	tab1.SelectedRate.SetActive(docChargeOptional2, true);
		// 	tab1.SelectedRate.SetActive(frtChargeOptional2, true);
		// 	model.Validate();
		// 	AssertEquals("PRE:", true, model.IsValid);
		//
		// 	var selected = model.GetSelectedRate();
		// 	selected
		// 		.Select(a => new { a.ChargeCode.AC_Code, a.Amount })
		// 		.Should().BeEquivalentTo(new[]
		// 		{
		// 			new { AC_Code = (ZString)"BL2", Amount = 30m },
		// 			new { AC_Code = (ZString)"BOL", Amount = 50m },
		// 			new { AC_Code = (ZString)"FR2", Amount = 600m },
		// 			new { AC_Code = (ZString)"FRT", Amount = 3000m },
		// 		});
		//
		// 	Assert(true);
		// }

		public void TestRateLocations_MultiModalRatingCostIsTrue_ShouldNotApplyToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting.Origin = "HKHKG";
			apiCosting.Destination = "USLAX";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newLocation = model.GetNewLocationsToApplyBackToJob();
				model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
				CombineAssertions("Job's locations should not change", () =>
				{
					AssertEquals("Consol Origin", "SGSIN", consol.JK_RL_NKLoadPort);
					AssertEquals("Consol Destination", "DEHAM", consol.JK_RL_NKDischargePort);
					AssertEquals("Criteria Origin", "SGSIN", criteria.OriginCode);
					AssertEquals("Criteria Destination", "DEHAM", criteria.DestinationCode);
				});
			}
		}

		public void TestRateLocations_ApplyLocationsBackToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting.Origin = "HKHKG";
			apiCosting.Destination = "USLAX";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newLocation = model.GetNewLocationsToApplyBackToJob();
				model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
				CombineAssertions("Job's locations should be updated", () =>
				{
					AssertEquals("Consol Origin", "HKHKG", consol.JK_RL_NKLoadPort);
					AssertEquals("Consol Destination", "USLAX", consol.JK_RL_NKDischargePort);
					AssertEquals("Criteria Origin", "HKHKG", criteria.OriginCode);
					AssertEquals("Criteria Destination", "USLAX", criteria.DestinationCode);
				});
			}
		}

		public void TestRateLocations_ApplyLocationsBackToJob_InvalidRateOrigin()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting.Origin = "HK???";
			apiCosting.Destination = "USLAX";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newLocation = model.GetNewLocationsToApplyBackToJob();
				model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
				CombineAssertions("locations should not be updated", () =>
				{
					AssertEquals("Consol Origin", "SGSIN", consol.JK_RL_NKLoadPort);
					AssertEquals("Consol Destination", "DEHAM", consol.JK_RL_NKDischargePort);
					AssertEquals("Criteria Origin", "SGSIN", criteria.OriginCode);
					AssertEquals("Criteria Destination", "DEHAM", criteria.DestinationCode);
				});

				AssertEquals("HK???", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestRateLocations_ApplyLocationsBackToJob_InvalidRateDestination()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting.Origin = "HKHKG";
			apiCosting.Destination = "US???";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newLocation = model.GetNewLocationsToApplyBackToJob();
				model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
				CombineAssertions("locations should not be updated", () =>
				{
					AssertEquals("Consol Origin", "SGSIN", consol.JK_RL_NKLoadPort);
					AssertEquals("Consol Destination", "DEHAM", consol.JK_RL_NKDischargePort);
					AssertEquals("Criteria Origin", "SGSIN", criteria.OriginCode);
					AssertEquals("Criteria Destination", "DEHAM", criteria.DestinationCode);
				});

				AssertEquals("US???", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestRateLocations_MultipleOriginsAndDestinations_ShouldNotApplyToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "AAA");
			apiCosting1.Origin = "HKHKG";
			apiCosting1.Destination = "USLAX";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "BBB");
			apiCosting2.Origin = "CNSHA";
			apiCosting2.Destination = "VNSGN";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();
			var tab2 = model.ContainerGroups.Last();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newLocation = model.GetNewLocationsToApplyBackToJob();
				model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
				CombineAssertions("Job's locations should not change", () =>
				{
					AssertEquals("Consol Origin", "SGSIN", consol.JK_RL_NKLoadPort);
					AssertEquals("Consol Destination", "DEHAM", consol.JK_RL_NKDischargePort);
					AssertEquals("Criteria Origin", "SGSIN", criteria.OriginCode);
					AssertEquals("Criteria Destination", "DEHAM", criteria.DestinationCode);
				});
			}
		}

		public void TestRateLocations_NonPortOriginAndDestination_ShouldNotApplyToJob()
		{
			var carrier = CreateCarrierOrg("SCAC");

			var consol = CreateConsol("SGSIN", "DEHAM");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "AAA");
			apiCosting1.Origin = "HK";
			apiCosting1.Destination = "US";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "BBB");
			apiCosting2.Origin = "HK";
			apiCosting2.Destination = "US";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 1000);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1, apiCosting2 });
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();
			var tab2 = model.ContainerGroups.Last();

			tab1.SelectedRate = tab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			tab2.SelectedRate = tab2.Rates.Single(x => x.WiseRateEntry == apiCosting2);

			var newLocation = model.GetNewLocationsToApplyBackToJob();
			model.ApplyLocationsBackToJob(newLocation.Origin, newLocation.Destination);
			CombineAssertions("Consol should not change and criteria should update", () =>
			{
				AssertEquals("Consol Origin", "SGSIN", consol.JK_RL_NKLoadPort);
				AssertEquals("Consol Destination", "DEHAM", consol.JK_RL_NKDischargePort);
				AssertEquals("Criteria Origin", "SGSIN", criteria.OriginCode);
				AssertEquals("Criteria Destination", "DEHAM", criteria.DestinationCode);
			});
		}

		public void TestMapCarrier()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var refContainer40 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			var carrier = CreateCarrierOrg();
			var consol = CreateConsol();
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);
			AddContainer(consol, "40GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Carrier = "MAEU";
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer40, carrier, "");
			apiCosting2.Carrier = "MAEU";
			apiCosting2.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 20m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var apiRates = new[] { apiCosting1, apiCosting2 };
			var response = new RatesSearchResponse
			{
				Rates = apiRates.ToArray(),
				Carriers = apiRates.Select(x => x.Carrier).Distinct()
					.Select(x => new RefCarrier { Code = x, SCACCode = x, Name = x + " Carrier" }).ToArray(),
				ChargeCodes = apiRates.SelectMany(x => x.Charges).GroupBy(x => x.ChargeCode)
					.Select(x => new RefChargeCode { Code = x.Key, Group = "FRT", Description = x.Key + " Desc" }).ToArray(),
				Providers = apiRates.Select(x => x.Provider).Distinct()
					.Select(x => new ProviderResult() { ProviderCode = x, ProviderName = x, ConnectionResult = global::WiseRates.Tools.Enums.ConnectionResult.Success }).ToArray()
			};

			model.AddWiseRatesForTest(response);

			var container20GP = model.ContainerGroups.Single(x => x.ContainerTypeWithCommodityCode == "20GP () (GEN)");
			var container40GP = model.ContainerGroups.Single(x => x.ContainerTypeWithCommodityCode == "40GP () (GEN)");

			container20GP.SelectedRate = container20GP.Rates.Single(x => x.WiseRateEntry != null);
			container40GP.SelectedRate = container40GP.Rates.Single(x => x.WiseRateEntry != null);

			AssertContainsExactElementsInAnyOrder
			(
				"Precondition: there are 2 rates with same unmapped carrier",
				new OrgHeader[2] { null, null },
				model.NotMappedCarrierRates.Select(x => x.ServiceProvider)
			);

			model.MapCarrier
			(
				promptUserToMapCarrier: (unmappedChooserRateEntry) =>
				{
					return (carrier, string.Empty);
				}
			);

			AssertContainsExactElementsInAnyOrder
			(
				"WHEN PromptUserToMapCarrier THEN both rates should have carrier mapped",
				new[] { carrier, carrier },
				model.NotMappedCarrierRates.Select(x => x.ServiceProvider)
			);
		}

		#region CalculationDescription

		public void TestCalculationDescription()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");

			var consol = ChooserHelper.CreateConsol();
			consol.Shipments.RemoveAndDeleteAll();
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			ChooserHelper.AddContainer(consol, "20GP", "GEN", ContainerModes.FCL);

			Factory.Save();

			var testInteractor = new TestInteractorEx(new[] { rateEntry });
			var cw1RateProvider = new CW1RatesProvider(Factory, testInteractor);
			var context = new RatingContext(new LoggerDecorator(testInteractor), Factory, cw1RateProvider, null, null, true);

			var autoRatingStarter = new AutoRatingStarter(new[] { consol }, context, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs);
			autoRatingStarter.ExecuteAutorating(AutoRateOptions.AutorateCosts);

			Assert(!testInteractor.AdditionResult.AutoRates.Single().Description.IsEmpty);
		}

		class TestInteractorEx : TestInteractor
		{
			public TestInteractorEx(IEnumerable<RateEntry> cw1Rates)
			{
				CW1Rates = cw1Rates;
			}
			readonly IEnumerable<RateEntry> CW1Rates;

			public override IEnumerable<AutoRateInfo> SelectRate(IRatingContext ratingContext, RatingCriteria criteria)
			{
				var model = new RateChooserModel(criteria, ratingContext);
				model.AddCalculateCW1Rates(new RateChooserTestHelper(CW1Rates.FirstOrDefault()?.Factory).NewCW1RateCombinations(criteria, "20GP", "GEN", CW1Rates.ToArray()));

				var chooseContainerCommodity = model.ContainerGroups.Single();
				chooseContainerCommodity.SelectedRate = chooseContainerCommodity.RateCollection[0];

				return model.GetSelectedRate();
			}

			public override void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
			{
				AdditionResult = additionResult;
			}
			public AutoRatesAdditionResult AdditionResult { get; private set; }
		}

		#endregion

		#region Test Subject To Fallback

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_NonFCLChargeFromCW1Found()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("CAR1");
			var carrier2 = CreateCarrierOrg("CAR2");
			var creditor = Helper.CreateCreditor("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier);
			CreateRate(costing1, container: "", commodity: "")
				.AddPerUnitCharge("FRT", 100m, "CN")
				.AddFlatCharge("BAF", 111m);
			var costing2 = Helper.NewCosting(creditor);
			CreateRate(costing2, container: "", commodity: "", category: "ORG")
				.AddFlatCharge("ODOC", 222m);
			CreateRate(costing2, container: "", commodity: "", category: "DST")
				.AddPerUnitCharge("DDOC", 333m, "CN");

			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var gp40Rate = ChooserHelper.CreateApiRate("40GP", carrier, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp40Rate, "FRT", 40m);
			RateChooserTestHelper.AddSubjectToCharge(gp40Rate, "BAF");
			RateChooserTestHelper.AddSubjectToCharge(gp40Rate, "ODOC");
			RateChooserTestHelper.AddSubjectToCharge(gp40Rate, "DDOC");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate, gp40Rate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier2.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = creditor.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 1);
			AddContainer(consol, "40GP", "GEN", "FCL", 1);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actualResults = results
					.Select(r => $"{r.ChargeCode.AC_Code}|{r.Amount}|{r.SingleLineDescription}|{string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
					.ToArray();

				var expectedResults = new[]
				{
					"FRT|20|FRT: 1 20GP Container(s) @ AUD 20.00/Container|",
					"FRT|40|FRT: 1 40GP Container(s) @ AUD 40.00/Container|",
					"BAF|111.00|BAF: Base Rate USD 111.00|", // Fallback to CW1 cost
					"ODOC|222.00|ODOC: Base Rate USD 222.00|", // Fallback to CW1 cost but also directly originating from a CW1 cost
					"DDOC|666.00|DDOC: 2 Container(s) @ HKD 333.00/Container|", // Fallback to CW1 cost but also directly originating from a CW1 cost
				};

				AssertContainsExactElementsInAnyOrder("Selected rates should match expected", expectedResults, actualResults);

				AssertCollectionContains("Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-FLT-Costing CAR1CARRIER.", logger.Infos);
				AssertCollectionNotContains("Info:RateLine Found DDOC-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine DDOC-MPU-CN-Costing CAR1.", logger.Infos);
				AssertCollectionNotContains("Info:RateLine Found ODOC-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine ODOC-FLT-Costing CAR1.", logger.Infos);

				AssertCollectionContains("Info:RateLine Found BAF-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-FLT-Costing CAR1CARRIER.", logger.Infos);
				AssertCollectionContains("Info:RateLine Found DDOC-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine DDOC-MPU-CN-Costing CAR1.", logger.Infos);
				AssertCollectionContains("Info:RateLine Found ODOC-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine ODOC-FLT-Costing CAR1.", logger.Infos);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_MatchesCorrectContainer()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "40GP", commodity: "HAZ")
				.AddPerUnitCharge("BAF", 40m, "CN", 40000m);
			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var gp40Rate = ChooserHelper.CreateApiRate("40GP", carrier1, "HAZ");
			RateChooserTestHelper.AddPerContainerCharge(gp40Rate, "FRT", 40m);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate, gp40Rate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 7);
			AddContainer(consol, "40GP", "HAZ", "FCL", 2);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actualResults = results
					.Select(r => new
					{
						ChargeCode = (string)r.ChargeCode.AC_Code,
						r.Amount,
						Desc = (string)r.SingleLineDescription,
						IncludedCharges = string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())
					})
					.Select(r => $"{r.ChargeCode}|{r.Amount}|{r.Desc}|{r.IncludedCharges}")
					.ToArray();

				var expectedResults = new[]
				{
					"FRT|140|FRT: 7 20GP Container(s) @ AUD 20.00/Container|BAF",
					"FRT|80|FRT: 2 40GP Container(s) @ AUD 40.00/Container|",
					"BAF|0|BAF: Freight Inclusive Calculator|"
				};

				AssertContainsExactElementsInAnyOrder(
					"Expected rates do not match the actual rates.",
					expectedResults,
					actualResults
				);

				AssertCollectionContains(
					"Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge could not fallback to Costing as no match was found.",
					logger.Infos
				);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_WhenContainerModeIsBCNorGRPorFCL()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "20GP", commodity: "GEN")
				.AddFlatCharge("BAF", 40000m);
			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 7);

			var expectedChargesWhenRegistryTrue = new[]
			{
				new { ChargeCode = "FRT", Amount = 20m * 7m, Desc = "FRT: 7 20GP Container(s) @ AUD 20.00/Container", IncludedCharges = "" },
				new { ChargeCode = "BAF", Amount = 40000m, Desc = "BAF: Base Rate USD 40000.00", IncludedCharges = "" },
			};
			var expectedChargesWhenRegistryFalse = new[]
			{
				new { ChargeCode = "FRT", Amount = 20m * 7m, Desc = "FRT: 7 20GP Container(s) @ AUD 20.00/Container", IncludedCharges = "BAF" },
				new { ChargeCode = "BAF", Amount = 0m, Desc = "BAF: Freight Inclusive Calculator", IncludedCharges = "" },
			};
			var expectedMessageWhenRegistryTrue = "Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-FLT-20GP-Costing CAR1CARRIER.";

			void Assert(string containerMode)
			{
				consol.JK_ConsolMode = containerMode;
				AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
				{
					var actualResults = results
						.Select(r => $"{(string)r.ChargeCode.AC_Code}|{(int)r.Amount}|{(string)r.SingleLineDescription}|{string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
						.ToArray();

					var expectedResults = expectedChargesWhenRegistryTrue
						.Select(r => $"{r.ChargeCode}|{(int)r.Amount}|{r.Desc}|{r.IncludedCharges}")
						.ToArray();

					AssertContainsExactElementsInAnyOrder(expectedResults, actualResults);

					AssertCollectionContains(
						"Expected message should be present in the logger's info",
						expectedMessageWhenRegistryTrue,
						logger.Infos
					);
				}, new[] { containerMode });

				AssertGetSelectedRateWithSubjectToFallback(false, response, consol, (logger, results) =>
				{
					var actualResults = results
						.Select(x => $"{x.ChargeCode.AC_Code}|{x.Amount}|{x.SingleLineDescription}|{string.Join(", ", x.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
						.ToArray();

					var expectedWhenRegistryFalse = expectedChargesWhenRegistryFalse
						.Select(x => $"{x.ChargeCode}|{x.Amount}|{x.Desc}|{x.IncludedCharges}")
						.ToArray();

					AssertContainsExactElementsInAnyOrder("Expected charges when registry is false do not match.", expectedWhenRegistryFalse, actualResults);
				}, new[] { containerMode });
			}

			Assert("FCL");
			Assert("GRP");
			Assert("BCN");
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_WhenConsolModeIsNonContainerizedAndLCL()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, commodity: "GEN", category: "LCL", mode: "LCL")
				.AddFlatCharge("BAF", 40000m);
			Factory.Save();

			// Rates Service Rates
			var apiCosting = ChooserHelper.CreateApiRate("", carrier1, "");
			RateChooserTestHelper.AddSubjectToCharge(apiCosting, "BAF");
			RateChooserTestHelper.AddFlatCharge(apiCosting, "FRT", 20m);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);

			// Testing
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;

			var expectedChargesWhenRegistryTrue = new[]
			{
				new { ChargeCode = "BAF", Amount = 0m, Desc = "BAF: Freight Inclusive Calculator", IncludedCharges = "" },
				new { ChargeCode = "FRT", Amount = 20m, Desc = "FRT: Base Rate AUD 20.00", IncludedCharges = "BAF" },
			};

			var expectedMessageWhenRegistryTrue = "Info:RateLine Found FRT-FLT-Wise Costing CAR1CARRIER";

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, containerModesForRegistry: [ContainerModes.LCL], checkingFunction: (logger, results) =>
			{
				var actualResults = results
					.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}|{string.Join(",", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
					.ToArray();

				var expectedResults = expectedChargesWhenRegistryTrue
					.Select(r => $"{r.ChargeCode}|{r.Amount}|{r.Desc}|{r.IncludedCharges}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder("Charges returned when registry is true should match expected", expectedResults, actualResults);

				AssertCollectionContains("Expected info log message", expectedMessageWhenRegistryTrue, logger.Infos);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_MatchesCorrectCommodity_WhenCommodityInGroupIsBlank()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "20GP", commodity: "GEN")
				.AddFlatCharge("BAF", 40000m);
			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier1, "");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "", "FCL", 7);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actual = results
					.Select(r => new
					{
						ChargeCode = (string)r.ChargeCode.AC_Code,
						r.Amount,
						Desc = (string)r.SingleLineDescription,
						IncludedCharges = string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())
					})
					.Select(r => $"{r.ChargeCode}|{r.Amount}|{r.Desc}|{r.IncludedCharges}")
					.ToArray();

				var expected = new[]
				{
					"FRT|140|FRT: 7 20GP Container(s) @ AUD 20.00/Container|",
					"BAF|40000.00|BAF: Base Rate USD 40000.00|"
				};

				AssertContainsExactElementsInAnyOrder(
					"The result should match the expected collection",
					expected,
					actual
				);

				AssertCollectionContains(
					"Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-FLT-20GP-Costing CAR1CARRIER.",
					logger.Infos
				);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_MultipleCommodityContainerGroupMatches()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["CAF"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["WAR"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["FSC"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "20GP")
				.AddPerUnitCharge("FRT", 200m, "CN")
				.AddPerUnitCharge("BAF", 20m, "CN", 20000m)
				.AddPerUnitCharge("CAF", 0m, "CN", 22000m);
			CreateRate(costing1, container: "", commodity: "HAZ")
				.AddPerUnitCharge("FSC", 0m, "CN", 88000m);
			CreateRate(costing1, container: "", commodity: "")
				.AddPerUnitCharge("FRT", 400m, "CN")
				.AddPerUnitCharge("BAF", 40m, "CN", 40000m)
				.AddPerUnitCharge("CAF", 0m, "CN", 44000m)
				.AddPerUnitCharge("FSC", 0m, "CN", 44000m)
				.AddPerUnitCharge("WAR", 0m, "CN", 44000m);
			CreateRate(costing1, container: "40GP", commodity: "HAZ")
				.AddPerUnitCharge("CAF", 0m, "CN", 55000m);
			CreateRate(costing1, container: "40GP", commodity: "PER")
				.AddPerUnitCharge("CAF", 0m, "CN", 66000m)
				.AddPerUnitCharge("WAR", 0m, "CN", 66000m);
			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier1, "GEN");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "CAF");
			var gp40HazdRate = ChooserHelper.CreateApiRate("40GP", carrier1, "HAZ");
			RateChooserTestHelper.AddPerContainerCharge(gp40HazdRate, "FRT", 40m);
			RateChooserTestHelper.AddSubjectToCharge(gp40HazdRate, "BAF");
			RateChooserTestHelper.AddSubjectToCharge(gp40HazdRate, "CAF");
			RateChooserTestHelper.AddSubjectToCharge(gp40HazdRate, "FSC");
			RateChooserTestHelper.AddSubjectToCharge(gp40HazdRate, "WAR");
			var gp40PersRate = ChooserHelper.CreateApiRate("40GP", carrier1, "PER");
			RateChooserTestHelper.AddPerContainerCharge(gp40PersRate, "FRT", 60m);
			RateChooserTestHelper.AddSubjectToCharge(gp40PersRate, "BAF");
			RateChooserTestHelper.AddSubjectToCharge(gp40PersRate, "CAF");

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate, gp40HazdRate, gp40PersRate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 7);
			AddContainer(consol, "40GP", "HAZ", "FCL", 2);
			AddContainer(consol, "40GP", "PER", "FCL", 1);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actualResults = results
					.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}|{string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
					.ToList();

				var expectedResults = new List<string>
				{
					"FRT|140|FRT: 7 20GP Container(s) @ AUD 20.00/Container|",
					"FRT|80|FRT: 2 40GP Container(s) @ AUD 40.00/Container|",
					"FRT|60|FRT: 1 40GP Container(s) @ AUD 60.00/Container|",

					"BAF|20000.00|BAF: Minimum USD 20000.00|",  // Fallback to CW1 cost
					"BAF|40000.00|BAF: Minimum USD 40000.00|",  // Fallback to CW1 cost

					"CAF|22000.00|CAF: Minimum USD 22000.00|",  // Fallback to CW1 cost
					"CAF|55000.00|CAF: Minimum USD 55000.00|",  // Fallback to CW1 cost
					"CAF|66000.00|CAF: Minimum USD 66000.00|",  // Fallback to CW1 cost

					"WAR|44000.00|WAR: Minimum USD 44000.00|",  // Fallback to CW1 cost
					"FSC|88000.00|FSC: Minimum USD 88000.00|",  // Fallback to CW1 cost
				};

				AssertContainsExactElementsInAnyOrder("Fallback true results should match", expectedResults, actualResults);

				AssertCollectionContains("Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-MPU-CN-20GP-Costing CAR1CARRIER.", logger.Infos);
				AssertCollectionContains("Info:RateLine Found CAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine CAF-MPU-CN-20GP-Costing CAR1CARRIER.", logger.Infos);
				AssertCollectionContains("Info:RateLine Found WAR-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine WAR-MPU-CN-Costing CAR1CARRIER.", logger.Infos);
				AssertCollectionContains("Info:RateLine Found FSC-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine FSC-MPU-CN-Costing CAR1CARRIER.", logger.Infos);
				AssertEquals(2, logger.Infos.Count(i => i == "Info:RateLine Found BAF-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-MPU-CN-Costing CAR1CARRIER."));
				AssertEquals(2, logger.Infos.Count(i => i == "Info:RateLine Found CAF-FRT-40GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine CAF-MPU-CN-40GP-Costing CAR1CARRIER."));
			});

			AssertGetSelectedRateWithSubjectToFallback(false, response, consol, (logger, results) =>
			{
				var actualResults = results
					.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}|{string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}")
					.ToList();

				var expectedResults = new List<string>
				{
					"FRT|140|FRT: 7 20GP Container(s) @ AUD 20.00/Container|BAF, CAF",
					"FRT|80|FRT: 2 40GP Container(s) @ AUD 40.00/Container|BAF, CAF, FSC, WAR",
					"FRT|60|FRT: 1 40GP Container(s) @ AUD 60.00/Container|BAF, CAF",

					"BAF|0|BAF: Freight Inclusive Calculator|",
					"BAF|0|BAF: Freight Inclusive Calculator|",
					"BAF|0|BAF: Freight Inclusive Calculator|",

					"CAF|0|CAF: Freight Inclusive Calculator|",
					"CAF|0|CAF: Freight Inclusive Calculator|",
					"CAF|0|CAF: Freight Inclusive Calculator|",

					"WAR|0|WAR: Freight Inclusive Calculator|",
					"FSC|0|FSC: Freight Inclusive Calculator|",
				};

				AssertContainsExactElementsInAnyOrder("Fallback false results should match", expectedResults, actualResults);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_SpecificAndGeneralRateStillFindsBAF()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("CAR1");
			var creditor = Helper.CreateCreditor("CAR1");

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier);
			CreateRate(costing1, container: "", commodity: "HAZ")
				.AddPerUnitCharge("FRT", 100m, "CN");
			CreateRate(costing1, container: "", commodity: "")
				.AddPerUnitCharge("FRT", 100m, "CN")
				.AddFlatCharge("BAF", 1000m);

			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier, "HAZ");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 20m);
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate);
			response.ChargeCodes = new[]
			{
				new RefChargeCode { Code = "FRT", Group = "FRT" },
				new RefChargeCode { Code = "BAF", Group = "ORG" },
			};

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "HAZ", "FCL", 1);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actual = results
					.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}|{string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())}|{r.Line.ChargeCode.AC_GC == ZGuid.Empty}")
					.ToArray();

				var expected = new[]
				{
					"FRT|20|FRT: 1 20GP Container(s) @ AUD 20.00/Container||False",
					"BAF|1000.00|BAF: Base Rate USD 1000.00||False",
				};

				AssertContainsExactElementsInAnyOrder(expected, actual);

				AssertCollectionContains(
					"Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR1CARRIER with Subject To charge fallback to RateLine BAF-FLT-Costing CAR1CARRIER.",
					logger.Infos
				);
			});
		}

		public void TestGetSelectedRate_SelectedCSRateWithIncludedSubjectTo_ReplacedWithMatchingCW1Ones()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["CAF"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["WAR"].AC_IsGroupageCharge = true;

			var carrier1 = CreateCarrierOrg("CAR1");
			var carrier2 = CreateCarrierOrg("CAR2");
			var exp = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			exp.PL_Code = "EXP";
			exp.PL_CarrierServiceCode = "EXP";

			// CW1 Rates
			var costing1 = Helper.NewCosting(carrier1);
			CreateRate(costing1, container: "20GP")
				.AddPerUnitCharge("FRT", 201m, "CN")
				.AddPerUnitCharge("BAF", 2001m, "CN");
			CreateRate(costing1, container: "40GP")
				.AddPerUnitCharge("FRT", 401m, "CN")
				.AddPerUnitCharge("BAF", 4001m, "CN");
			var costing2 = Helper.NewCosting(carrier2);
			CreateRate(costing2, container: "20GP", carrierServiceLevel: "EXP")
				.AddPerUnitCharge("FRT", 202m, "CN")
				.AddPerUnitCharge("BAF", 2002m, "CN");
			CreateRate(costing2, container: "40GP", carrierServiceLevel: "EXP")
				.AddPerUnitCharge("FRT", 402m, "CN")
				.AddPerUnitCharge("BAF", 4002m, "CN");
			CreateRate(costing2, container: "20GP", carrierServiceLevel: "STD")
				.AddPerUnitCharge("FRT", 111m, "CN")
				.AddPerUnitCharge("BAF", 222m, "CN");
			CreateRate(costing2, container: "40GP", carrierServiceLevel: "STD")
				.AddPerUnitCharge("FRT", 333m, "CN")
				.AddPerUnitCharge("BAF", 444m, "CN");

			Factory.Save();

			// Rates Service Rates
			var gp20Rate = ChooserHelper.CreateApiRate("20GP", carrier2, "GEN", serviceLevel: "EXP");
			RateChooserTestHelper.AddPerContainerCharge(gp20Rate, "FRT", 302m);
			RateChooserTestHelper.AddIncludedCharge(gp20Rate, "WAR");
			RateChooserTestHelper.AddSubjectToCharge(gp20Rate, "BAF");
			var gp40Rate = ChooserHelper.CreateApiRate("40GP", carrier2, "GEN", serviceLevel: "EXP");
			RateChooserTestHelper.AddPerContainerCharge(gp40Rate, "FRT", 602m);
			RateChooserTestHelper.AddSubjectToCharge(gp40Rate, "CAF");
			RateChooserTestHelper.AddSubjectToCharge(gp40Rate, "BAF");
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(gp20Rate, gp40Rate);

			// Testing
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 1);
			AddContainer(consol, "40GP", "GEN", "FCL", 1);

			AssertGetSelectedRateWithSubjectToFallback(true, response, consol, (logger, results) =>
			{
				var actual = results
					.Select(r => new
					{
						ChargeCode = (string)r.ChargeCode.AC_Code,
						r.Amount,
						Desc = (string)r.SingleLineDescription,
						IncludedCharges = string.Join(", ", r.Line.IncludedLines.Select(l => l.ChargeCode.AC_Code).Distinct())
					})
					.Select(r => $"{r.ChargeCode}|{r.Amount}|{r.Desc}|{r.IncludedCharges}")
					.ToArray();

				var expected = new[]
				{
					"FRT|302|FRT: 1 20GP Container(s) @ AUD 302.00/Container|WAR",
					"FRT|602|FRT: 1 40GP Container(s) @ AUD 602.00/Container|CAF",
					"BAF|2002.00|BAF: 1 20GP Container(s) @ USD 2002.00/Container|",
					"BAF|4002.00|BAF: 1 40GP Container(s) @ USD 4002.00/Container|",
					"WAR|0|WAR: Freight Inclusive Calculator|",
					"CAF|0|CAF: Freight Inclusive Calculator|"
				};

				AssertContainsExactElementsInAnyOrder("The response data does not match the expected output.", expected, actual);

				var expectedInfos = new[]
				{
					"Info:RateLine Found BAF-FRT-20GP-Wise Costing CAR2CARRIER with Subject To charge fallback to RateLine BAF-MPU-CN-20GP-Costing CAR2CARRIER.",
					"Info:RateLine Found CAF-FRT-40GP-Wise Costing CAR2CARRIER with Subject To charge could not fallback to Costing as no match was found."
				};

				AssertCollectionContains("Logger info messages should match the expected.", expectedInfos[0], logger.Infos);
				AssertCollectionContains("Logger info messages should match the expected.", expectedInfos[1], logger.Infos);
			});
		}

		void AssertGetSelectedRateWithSubjectToFallback(bool isFallbackEnabled, RatesSearchResponse response, ForwardingConsol consol, Action<TestLogger, AutoRateInfoCollection> checkingFunction, string[] containerModesForRegistry = null)
		{
			var fallbackSubjectToRegistry = new FallbackSubjectToChargesCollection();
			containerModesForRegistry = containerModesForRegistry ?? new[] { ContainerModes.FCL };
			foreach (var containerMode in containerModesForRegistry)
			{
				var fallbackSubjectToCharge = new FallbackSubjectToCharges();
				fallbackSubjectToRegistry.Add(fallbackSubjectToCharge);
				fallbackSubjectToCharge.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
				fallbackSubjectToCharge.TransportMode = TransportModes.Sea;
				fallbackSubjectToCharge.ContainerMode = containerMode;
				fallbackSubjectToCharge.IsFallbackEnabled = isFallbackEnabled;
			}

			var autoRatingProxy = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var logger = new TestLogger();
			var context = new RatingContext(logger);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			using (DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, fallbackSubjectToRegistry))
			{
				var ratesQuery = BuildRatesQuery(criteria);
				ratesQuery.Carrier = Array.Empty<RatesQueryCarrier>();
				var model = new RateChooserModel(criteria, context);
				model.SendRatesRequest(filter, ratesQuery);
				model.AddWiseRatesForTest(response);
				model.ContainerGroups.ForEach(t => t.SelectedRate = t.Rates.Single(x => x.WiseRateEntry != null && x.WiseRateEntry.Commodity == t.CommodityCode));

				// Imitating UI. Once the rate is selected, we call these methods to apply rate values back to the job,
				// then we call GetSelectedRate to get calculated charges.
				var oldCarrier = model.Criteria.Carrier;

				AssertCollectionContains("The old carrier should exist in the creditor organizations.", oldCarrier, model.Criteria.Creditors.AllOrgs);

				model.ApplyCarrierBackToJob();
				var newCarrier = model.Criteria.Carrier;

				Assert("Carrier should be updated in Creditors.", model.Criteria.Creditors.AllOrgs.Any(x => x.PK == newCarrier.PK));

				model.ApplyServiceLevelBackToJobIfNeeded();

				var results = model.GetSelectedRate();
				checkingFunction(logger, results);
			}
		}

		#endregion

		[TestDate(2020, 04, 25)]
		public void TestSendRatesRequest_JobHasNoNAC_ShouldNotPupolateNACOnJob()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			apiCosting1.NamedAccounts = new[] { "TEST" };

			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting2.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1, apiCosting2);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, (r) => new[] { 0, 1 }.Contains(r.PageID));

			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);

			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(0, consol.Numbers.Count);
		}

		[TestDate(2020, 04, 25)]
		public void TestSendRatesRequest_JobHasNAC_ShouldNotShowAnyPopupMessage()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			var namedAccount = consol.Numbers.AddNew();
			namedAccount.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			namedAccount.CE_EntryNum = "TEST1";

			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			apiCosting1.NamedAccounts = new[] { "TEST2" };

			var apiCosting2 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting2.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 12m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1, apiCosting2);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, (r) => new[] { 0, 1 }.Contains(r.PageID));

			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider, null, true);

			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);

			using (_Rating.Start(new TestInteractor()))
			{
				model.SendRatesRequest(filter, BuildRatesQuery(criteria));
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestCriteriaWithLCLContainer_CommodityContainerGroupFor_None()
		{
			var chooserHelper = new RateChooserTestHelper(Factory);
			var consol = chooserHelper.CreateConsol();
			var lclContainer = consol.Containers.AddNew();
			lclContainer.JC_RC = MeasureInfo.ContainerInfo.LCL;
			lclContainer.JC_ContainerMode = "LCL";
			lclContainer.JC_RH_NKContainerCommodityCode = "GEN";
			lclContainer.JC_ContainerCount = 1;

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var context = new Mock<IRatingContext>();
			context.Setup(x => x.Factory).Returns(Factory);

			var model = new RateChooserModel(criteria, context.Object);

			Assert(model.ContainerGroups.IsCountEqualTo(0));
		}

		public void TestCriteriaWithNonExitingRefContainer_CommodityContainerGroupFor_None()
		{
			var chooserHelper = new RateChooserTestHelper(Factory);
			var consol = chooserHelper.CreateConsol();
			var lclContainer = consol.Containers.AddNew();
			lclContainer.JC_RC = ZGuid.BrettsGuid;
			lclContainer.JC_ContainerMode = "FCL";
			lclContainer.JC_RH_NKContainerCommodityCode = "GEN";
			lclContainer.JC_ContainerCount = 1;

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var context = new Mock<IRatingContext>();
			context.Setup(x => x.Factory).Returns(Factory);

			var model = new RateChooserModel(criteria, context.Object);

			Assert(model.ContainerGroups.IsCountEqualTo(0));
		}

		public void TestCriteriaWithLCLandFCLContainer_CommodityContainerGroupFor_FCLOnly()
		{
			var chooserHelper = new RateChooserTestHelper(Factory);
			var consol = chooserHelper.CreateConsol();
			var lclContainer = consol.Containers.AddNew();
			lclContainer.JC_RC = MeasureInfo.ContainerInfo.LCL;
			lclContainer.JC_ContainerMode = "LCL";
			lclContainer.JC_RH_NKContainerCommodityCode = "GEN";
			lclContainer.JC_ContainerCount = 1;
			var fclContainer = chooserHelper.AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var context = new Mock<IRatingContext>();
			context.Setup(x => x.Factory).Returns(Factory);

			var model = new RateChooserModel(criteria, context.Object);

			var containerGroup = model.ContainerGroups.Single();
			AssertEquals(3, containerGroup.ContainerCount);
			AssertEquals("GEN", containerGroup.CommodityCode);
			Assert(containerGroup.ContainerRef.Is20GP);
			AssertEquals(fclContainer.JC_RC, containerGroup.ContainerRef.PK);
		}

		public void TestCalculate_CSTCalculator_ShouldNotThrowException()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			Factory.Save();

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 100m;

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			AssertNoExceptionThrown(() => model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1)));
		}

		#region LCL

		public void TestCalculate_CSTCalculator_ShouldNotThrowException_ForLCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			Factory.Save();

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "USLAX", "HKHKG");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], CompanyTariffOrCostBasedCalculator.CostBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 100m;

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			AssertNoExceptionThrown(() => model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntry1)));
		}

		public void TestRatesShouldHaveCalculatedInfo_ForCW1Rate_WhenConsolIsLCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "USLAX", "HKHKG");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, "KG", "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100;
			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry == null);

			CombineAssertions("Rate entry for both CW1 and Wise rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", 1, lclTab.SelectedRate.CalculatedResult.Count);
				AssertEquals("Calculated amount", 5000000m, lclTab.SelectedRate.CalculatedResult[0].Amount); // 50000 * 100
				AssertEquals("Calculated description", "FRT: 50000 Kilogram(s) @ AUD 100.00/KG", lclTab.SelectedRate.CalculatedResult[0].SingleLineDescription);
			});
		}

		public void TestRatesShouldHaveCalculatedInfo_ForRateServiceRate_WhenConsolIsLCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL);
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry != null);

			CombineAssertions("Rate entry for both CW1 and Wise rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", 1, lclTab.SelectedRate.CalculatedResult.Count);
				AssertEquals("Calculated amount", 500000m, lclTab.SelectedRate.CalculatedResult[0].Amount); // 50000 * 10
				AssertEquals("Calculated description", "FRT: 50000 Kilogram(s) @ AUD 10.00/KG", lclTab.SelectedRate.CalculatedResult[0].SingleLineDescription);
			});
		}

		public void TestLCLConsolHasOnlyOneTab()
		{
			var carrier = CreateCarrierOrg("SCAC");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL1", "UL1");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "CL2", "UL2");
			Factory.Save();

			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL).WithCharge();

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);
			var tab1 = model.ContainerGroups.First();

			AssertEquals("Consol should be LCL", ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals("Rate Selector should have only one tab", 1, model.ContainerGroups.Count());
			AssertEquals("LCL tab should have zero container", 0, tab1.ContainerCount);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			AssertEquals("Rate Selector should have only one tab", 1, viewModel.ContainerTabs.Count());

			AddContainer(consol, "20GP", "GEN", ContainerModes.FCL, 3);
			AddContainer(consol, "20GP", "ATPT", ContainerModes.LCL, 5);

			viewModel.RefreshRates();

			AssertEquals("Consol should be LCL", ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals("Rate Selector should have only one tab", 1, model.ContainerGroups.Count());
			AssertEquals("LCL tab should have zero container", 0, tab1.ContainerCount);
			AssertEquals("Rate Selector should have only one tab", 1, viewModel.ContainerTabs.Count());
		}

		public void TestApplyRatesWithIncludedChargesToJob_ShouldContainIncludedCharges()
		{
			var cyrcCharge = Helper.ChargeCodes.NewConsolChargeCode("CYRC", "Cont Yard Rec Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			cyrcCharge.AC_GC = Env.CurrentCompanyPK;

			Factory.Save();

			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL);
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });
			apiCosting1.Charges.Add(new Charge { ChargeCode = "CYRC", Currency = "AUD", ChargeType = Api.Model.ChargeType.Included, FreightInclusiveCarriageCharge = "FRT" });
			apiCosting1.Provider = WRConstants.RateProviders.CargoSphere;

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);
			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry != null);

			var expected = new[]
			{
				new SimpleArInfo
				{
					InvoiceLineDesc = "International Freight",
					Amount = 500000m,
					CalculationSingleLineDescription = "FRT: 50000 Kilogram(s) @ AUD 10.00/KG",
					CalculationDescription = "CYRC - Cont Yard Rec Charge (Included)"
				},
				new SimpleArInfo
				{
					InvoiceLineDesc = "Cont Yard Rec Charge",
					Amount = 0m,
					CalculationSingleLineDescription = "CYRC: Freight Inclusive Calculator",
				}
			};

			var mockGuiInteractor = new Mock<IAutoRatingGUIInteractor>();
			mockGuiInteractor
				.Setup(x => x.SelectRate(context, criteria))
				.Returns(model.GetSelectedRate());
			using (_Rating.Start(mockGuiInteractor.Object))
			{
				var freightAutoRater = new FreightAutoRater(context);
				var result = freightAutoRater.AutoRate(criteria, CostSell.Cost);
				var charges = result.RateInfoCollection;
				AssertEquals(50000 * 10m, charges.Sum(x => x.Amount));
				AssertEquals("Count", 2, charges.Count);

				AssertRatingResults(expected, result);
			}
		}

		public void TestRatesShouldHaveOriginDestination_ForCW1Rate_WhenConsolIsLCL()
		{
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "USLAX", "HKHKG");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, "KG", "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100;

			var rateEntryOrigin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "USLAX", "");
			var rateLineOrigin = rateEntryOrigin.AddRateLine(Helper.ChargeCodes["ODOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineOrigin.GetCalculator<UnitCalculator>().PerUnit = 50;

			var rateEntryDST = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "HKHKG");
			var rateLineDST = rateEntryDST.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineDST.GetCalculator<UnitCalculator>().PerUnit = 40;

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1, rateEntryOrigin, rateEntryDST));

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry == null);

			CombineAssertions("Rate entry for CW1 rates should contain calculated info", () =>
			{
				AssertEquals("Calculated result", 3, lclTab.SelectedRate.CalculatedResult.Count);
				var odocResult = lclTab.SelectedRate.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["ODOC"]);
				AssertEquals("Calculated amount ODOC", 2500000m, odocResult.Amount); // 50000 * 50
				AssertEquals("Calculated description ODOC", "ODOC: 50000 Kilogram(s) @ AUD 50.00/KG", odocResult.SingleLineDescription);

				var ddocResult = lclTab.SelectedRate.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["DDOC"]);
				AssertEquals("Calculated amount DDOC", 2000000m, ddocResult.Amount); // 40000 * 50
				AssertEquals("Calculated description DDOC", "DDOC: 50000 Kilogram(s) @ AUD 40.00/KG", ddocResult.SingleLineDescription);

				var frtResult = lclTab.SelectedRate.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["FRT"]);
				AssertEquals("Calculated amount FRT", 5000000m, frtResult.Amount); // 50000 * 100
				AssertEquals("Calculated description FRT", "FRT: 50000 Kilogram(s) @ AUD 100.00/KG", frtResult.SingleLineDescription);
			});
		}

		public void TestSelectedRateShouldBringNonFreightCW1Charges_WhenConsolIsLCLAndSelectedRateIsFromAnotherCW1Provider()
		{
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("SCAC");
			var creditor = CreateCarrierOrg("ABCD");

			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var rateEntryOrigin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "USLAX", "");
			var rateLineOrigin = rateEntryOrigin.AddRateLine(Helper.ChargeCodes["ODOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineOrigin.GetCalculator<UnitCalculator>().PerUnit = 50;

			var costingCreditor = Helper.NewCosting(creditor);
			costingCreditor.TH_GC = Env.CurrentCompanyPK;

			var rateEntryDST = costingCreditor.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "HKHKG");
			var rateLineDST = rateEntryDST.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineDST.GetCalculator<UnitCalculator>().PerUnit = 40;

			Factory.Save();

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntryOrigin, rateEntryDST));

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.RateEntry.PK == rateEntryOrigin.PK);

			var results = model.GetSelectedRate();

			var actual = results
				.Select(r => new
				{
					ChargeCode = (string)r.ChargeCode.AC_Code,
					r.Amount,
					Desc = (string)r.SingleLineDescription,
				})
				.Select(r => $"{r.ChargeCode}|{r.Amount}|{r.Desc}")
				.ToArray();

			var expected = new[]
			{
				"ODOC|2500000|ODOC: 50000 Kilogram(s) @ AUD 50.00/KG",
				"DDOC|2000000|DDOC: 50000 Kilogram(s) @ AUD 40.00/KG",
			};

			AssertContainsExactElementsInAnyOrder(
				"The summed up charges should match the expected collection",
				expected,
				actual
			);
		}

		public void TestSelectedRateShouldBringNonCW1FreightCharges_WhenConsolIsLCLAndSelectedRateIsFromWiseRates()
		{
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("SCAC");
			var creditor = CreateCarrierOrg("ABCD");

			var consol = CreateConsol();
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var costing = Helper.NewCosting(creditor);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntryDST = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "HKHKG");
			rateEntryDST.TI_OH_Supplier = creditor.PK;
			var rateLineDST = rateEntryDST.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineDST.GetCalculator<UnitCalculator>().PerUnit = 40;

			Factory.Save();

			var logger = new TestLogger();

			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntryDST));

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", ContainerModes.LCL);
			apiCosting1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			model.AddWiseRatesForTest(response);

			var lclTab = model.ContainerGroups.Single();
			lclTab.SelectedRate = lclTab.Rates.Single(x => x.WiseRateEntry != null);

			var results = model.GetSelectedRate();
			var actual = results
				.Select(r => $"{(string)r.ChargeCode.AC_Code}|{r.Amount}|{(string)r.SingleLineDescription}")
				.ToArray();

			var expectedResults = new[]
			{
				"FRT|500000|FRT: 50000 Kilogram(s) @ AUD 10.00/KG",
				"DDOC|2000000.00|DDOC: 50000 Kilogram(s) @ AUD 40.00/KG"
			};

			AssertContainsExactElementsInAnyOrder(
				"Expected selected rate results to match the calculated charges.",
				expectedResults,
				actual
			);
		}

		#endregion

		#region Helper

		public RateEntry CreateRate(
			RatingHeader header,
			string category = "FCL",
			string mode = "SEA",
			string origin = "USLAX",
			string destination = "HKHKG",
			string container = "20GP",
			string commodity = "GEN",
			string carrierServiceLevel = "STD")
		{
			var entry = header.AddRateEntry(category, mode, origin, destination, carrierServiceLevel, container, commodity);
			entry.RateLines.RemoveAndDeleteAll();

			// Force set the commodity insead it being ignored if it is null or empty by AddRateEntry
			entry.TI_RH_NKCommodityCode = commodity;

			return entry;
		}

		RateEntry AddFCLSEARateEntryWithPerContainerCharge(Costing costing, string origin, string dest, string serviceLevel, string commodityCode, AccChargeCode chargeCode, string containerCode, decimal price)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, dest, serviceLevel, containerCode);
			entry.TI_RH_NKCommodityCode = commodityCode;
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, "AUD");
			line.GetCalculator<UnitCalculator>().PerUnit = price;
			return entry;
		}

		RateEntry AddOriginFlatCharge(Costing costing, string origin, string serviceLevel, AccChargeCode chargeCode, decimal price)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, "", serviceLevel, "");
			entry.TI_RH_NKCommodityCode = ZString.Empty;
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code, "", "AUD");
			line.GetCalculator<FlatCalculator>().BaseRate = price;
			return entry;
		}

		static void ClearSelection(RateChooserModel model)
		{
			foreach (var containerCommodity in model.ContainerGroups)
			{
				containerCommodity.SelectedRate = null;
			}
		}

		OrgHeader CreateCarrierOrg(string scac = "SCAC") => ChooserHelper.CreateCarrierOrg(scac);
		public ForwardingConsol CreateConsol(string origin = "USLAX", string destination = "HKHKG") => ChooserHelper.CreateConsol(origin, destination);

		public ForwardingContainer AddContainer(
			ForwardingConsol consol,
			string code = "20GP",
			string commodityCode = "GEN",
			string containerMode = ContainerModes.FCL,
			short containerCount = 1)
			=> ChooserHelper.AddContainer(consol, code, commodityCode, containerMode, containerCount);

		protected RateChooserTestHelper ChooserHelper
		{
			get { return helper ?? (helper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper helper;

		RatesQuery BuildRatesQuery(RatingCriteria criteria)
		{
			if (wiseRatesQueryBuilder == null)
			{
				wiseRatesQueryBuilder = new WiseRatesQueryBuilder(new DummyLogger());
			}

			var (ratesQuery, _) = wiseRatesQueryBuilder.Build(criteria);
			ratesQuery.AcceptedProviders = new[] { WRConstants.RateProviders.CargoSphere };

			return ratesQuery;
		}
		WiseRatesQueryBuilder wiseRatesQueryBuilder;

		protected UsageCollectorTestHelper UsageCollectorTestHelper => usageCollectorTestHelper ?? (usageCollectorTestHelper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper usageCollectorTestHelper;

		#endregion
	}

	internal static class RateExtensions
	{
		internal static Rate WithCarrierServiceLevel(this Rate rate, string carrierServiceLevel)
		{
			rate.ServiceLevel = carrierServiceLevel;
			return rate;
		}

		internal static Rate WithNamedAccounts(this Rate rate, params string[] names)
		{
			if (rate.NamedAccounts == null || rate.NamedAccounts.Length == 0)
			{
				rate.NamedAccounts = names.ToArray();
			}
			else
			{
				rate.NamedAccounts = rate.NamedAccounts.Concat(names).ToArray();
			}
			return rate;
		}

		internal static Rate WithContractNumber(this Rate rate, string contractNumber)
		{
			rate.ContractNumber = contractNumber;
			return rate;
		}

		internal static Rate WithCarrierQuoteNumber(this Rate rate, string carrierQuoteNumber)
		{
			rate.ProviderRateId = carrierQuoteNumber;
			rate.BookingInfo = new BookingInfo
			{
				Schedule = new Schedule()
			};

			return rate;
		}

		internal static Rate WithCharge(this Rate rate, string chargeCode = "FRT", decimal perUnitRate = 10m)
		{
			rate.Charges.Add(new Charge { ChargeCode = chargeCode, Currency = "AUD", Unit = "CN", PerUnitRate = perUnitRate });
			return rate;
		}
	}

	internal static class ChargeExtensions
	{
		internal static Charge OfType(this Charge charge, ChargeType chargeType)
		{
			charge.ChargeType |= chargeType;
			if (chargeType == ChargeType.Optional || chargeType == ChargeType.Additional)
			{
				charge.IsOptional = true;
			}
			return charge;
		}

		internal static Charge IncludedIn(this Charge charge, string includedIntoChargeCode)
		{
			charge.FreightInclusiveCarriageCharge = includedIntoChargeCode;
			return charge;
		}

		internal static Charge WithCustomCategory(this Charge charge, string customCategory)
		{
			charge.CustomCategory = customCategory;
			return charge;
		}

		internal static Charge WithCallForPricing(this Charge charge, bool restricted = true)
		{
			charge.Restricted = restricted;
			return charge;
		}

		internal static Charge WithCurrency(this Charge charge, string currency)
		{
			charge.Currency = currency;
			return charge;
		}
	}
}
