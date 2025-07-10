using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;
using Api = WiseRates.Api;

namespace Enterprise.Rating.Testing.GUI
{
	internal class CargoSphereIntegrationTest : BaseRatingIntegrationTest
	{
		[TestDate(2020, 04, 25)]
		[GuiTest]
		public void TestAutorate_CargosphereRate_ContainerWithISOTypeGroup()
		{
			// Container1 and 2 are in the criteria. Container 3 is not
			// do not remove container3 as it demonstrates that it is filtered out
			var refContainer1 = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "40GP");
			refContainer1.RC_ISOType = "AAAA";
			refContainer1.RC_FreightRateClass = "44G0";
			var refContainer2 = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "40HC");
			refContainer2.RC_ISOType = "BBBB";
			refContainer2.RC_FreightRateClass = "44G0";
			var refContainer3 = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			refContainer3.RC_ISOType = "CXXC";
			refContainer3.RC_FreightRateClass = "44G0";

			var carrier = NewCarrierCreditor("CARRIER");
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "HKHKG", 310, 1m));

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = refContainer1.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RH_NKContainerCommodityCode = "GEN";
			container1.JC_ContainerCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainer2.PK;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_RH_NKContainerCommodityCode = "GEN";
			container2.JC_ContainerCount = 1;

			var job = new JobHeader.Loader(consol).TryLoadOrCreate();
			var helper = new RateChooserTestHelper(Factory);
			Factory.Save();

			var rateServiceRate = helper.CreateApiRate(string.Empty, carrier, commodity: "");
			rateServiceRate.Origin = "AUSYD";
			rateServiceRate.Destination = "HKHKG";
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
					new RefCarrier { Code = carrier.SCACCode , SCACCode = carrier.SCACCode },
				},
			};

			var mockFactory = new Mock<IWiseRatesClientFactory>();
			var mockClient = new Mock<IWiseRatesClient>();
			mockFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((mockClient.Object, string.Empty));
			mockClient
				.Setup(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			ObjectFactory.Substitute(mockFactory.Object);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 11.00,
						CostCalculationDescription = $"FRT: 1 40HC Container(s) @ AUD 11.00/Container\r\n\r\nInternational Freight\r\n\r\nCharge located in CARRIER (Consol C00001000 --> Carrier) Wise Costing with the following details:\r\n\r\n{DescriptionHelpers.FormatWithTab("Mode:")}SEA\r\n{DescriptionHelpers.FormatWithTab("Charge Code Group:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Rate Provider:")}CGSP\r\n{DescriptionHelpers.FormatWithTab("Start Date:")}25 January 2020\r\n{DescriptionHelpers.FormatWithTab("Universal Charge Codes:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Carrier Charge Code:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Container:")}40HC\r\n{DescriptionHelpers.FormatWithTab("Carrier Service Level:")}STD\r\n{DescriptionHelpers.FormatWithTab("Autorated for:")}Consol C00001000\r\n{DescriptionHelpers.FormatWithTab("Leg:")}AUSYD-HKHKG\r\n\r\n\r\nUser:\t\tCargoWise Support\r\nTime:\t\t25-Apr-20 00:00:00\r\n\r\n\r\n",
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 11.00,
						CostCalculationDescription = $"FRT: 1 40GP Container(s) @ AUD 11.00/Container\r\n\r\nInternational Freight\r\n\r\nCharge located in CARRIER (Consol C00001000 --> Carrier) Wise Costing with the following details:\r\n\r\n{DescriptionHelpers.FormatWithTab("Mode:")}SEA\r\n{DescriptionHelpers.FormatWithTab("Charge Code Group:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Rate Provider:")}CGSP\r\n{DescriptionHelpers.FormatWithTab("Start Date:")}25 January 2020\r\n{DescriptionHelpers.FormatWithTab("Universal Charge Codes:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Carrier Charge Code:")}FRT\r\n{DescriptionHelpers.FormatWithTab("Container:")}40GP\r\n{DescriptionHelpers.FormatWithTab("Carrier Service Level:")}STD\r\n{DescriptionHelpers.FormatWithTab("Autorated for:")}Consol C00001000\r\n{DescriptionHelpers.FormatWithTab("Leg:")}AUSYD-HKHKG\r\n\r\n\r\nUser:\t\tCargoWise Support\r\nTime:\t\t25-Apr-20 00:00:00\r\n\r\n\r\n",
					},
				};

				AutoCostAndAssert
				(
					"",
					null,
					expectedCosts,
					consol,
					autorateRevenue: false,
					autorateCosts: true
				);
			}
		}

		[GuiTest]
		[TestDate(2020, 04, 25)]
		public void TestCargoSphereRateSelectorShouldNotBeShownForShipmentWhenAutoRatingCostsAndRevenuesOnConsol()
		{
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";

			var transport = consol.Transports[0];
			transport.JW_ETD = DateTime.Now.Date.AddDays(-30);
			transport.JW_ETA = DateTime.Now.Date;

			var c = consol.Containers.AddNew();
			c.JC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			c.JC_ContainerCount = 1;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var formIsUsedForShipment = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
						formIsUsedForShipment = rateChooser.Model.Criteria.AdapterType != AdapterType.Consolidation;
					}
				});

				AutoCostAndAssert
				(
					"",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: true,
					autorateCosts: true
				);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for shipment", false, formIsUsedForShipment);
			}
		}

		[GuiTest]
		[TestDate(2020, 04, 25)]
		public void TestCargoGuideRateSelectorShouldNotBeShownForShipmentWhenAutoRatingCostsAndRevenuesOnConsol()
		{
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = "LSE";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_AWBServiceLevel = "STD";

			var transport = consol.Transports[0];
			transport.JW_ETD = DateTime.Now.Date.AddDays(-30);
			transport.JW_ETA = DateTime.Now.Date;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;
				var formIsUsedForShipment = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm selectorForm)
					{
						timesRateSelectorFormIsShown++;
						selectorForm.BtnSkipRateSelectionClick(null, null);
						formIsUsedForShipment = selectorForm.Criteria.AdapterType != AdapterType.Consolidation;
					}
				});

				AutoCostAndAssert
				(
					"",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: true,
					autorateCosts: true
				);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
				AssertEquals("Rate Selector should not be shown for shipment", false, formIsUsedForShipment);
			}
		}

		[TestDate(2020, 04, 25)]
		[GuiTest]
		public void TestAutorate_UrsRatesProvider_IsCalledWhenUrsFeatureEnabled()
		{
			var refContainer = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			refContainer.RC_ISOType = "22G1";
			refContainer.RC_FreightRateClass = "22G1";

			var carrier = NewCarrierCreditor("CARRIER");
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "HKHKG", 100, 1m));

			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RH_NKContainerCommodityCode = "GEN";
			container.JC_ContainerCount = 1;

			var job = new JobHeader.Loader(consol).TryLoadOrCreate();
			Factory.Save();

			// Mock WiseRates factory and client (should not be called)
			var mockWiseRatesFactory = new Mock<IWiseRatesClientFactory>();
			var mockWiseRatesClient = new Mock<IWiseRatesClient>();
			mockWiseRatesFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((mockWiseRatesClient.Object, string.Empty));

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

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			// Given
			// Rate Service is ENABLED
			// URS feature is ENABLED
			// legacy Rate Selector is DISABLED
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (ObjectFactory.Substitute(MockURSFeatureHelper(true)))
			{
				// When doing Autorating
				AutoCostAndAssert
				(
					"",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: false,
					autorateCosts: true
				);

				// Then
				// URS provider (UrsRatesProvider) should be called to get rates
				// legacy Rate Service provider (WiseRatesProvider) should NOT be called to get rates
				mockUrsFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
				mockUrsClient.Verify(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
				mockWiseRatesFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()), Times.Never);
				mockWiseRatesClient.Verify(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
			}
		}

		[TestDate(2020, 04, 25)]
		[GuiTest]
		public void TestAutorate_UrsRatesProvider_IsNotCalledWhenUrsFeatureDisabled()
		{
			// Arrange
			var refContainer = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			refContainer.RC_ISOType = "22G1";
			refContainer.RC_FreightRateClass = "22G1";

			var carrier = NewCarrierCreditor("CARRIER");
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Sea, consignor.PK, ZGuid.Empty, "AUSYD", "HKHKG", 100, 1m));

			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RH_NKContainerCommodityCode = "GEN";
			container.JC_ContainerCount = 1;

			var job = new JobHeader.Loader(consol).TryLoadOrCreate();
			Factory.Save();

			// Mock WiseRates factory and client
			var helper = new RateChooserTestHelper(Factory);
			var rate1 = helper.CreateApiRate("20GP", carrier, "GEN", serviceLevel: "STD");
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
					new RefCarrier { Code = carrier.SCACCode , SCACCode = carrier.SCACCode },
				},
			};

			var mockWiseRatesClient = new Mock<IWiseRatesClient>();
			mockWiseRatesClient
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(response));

			var mockWiseRatesFactory = new Mock<IWiseRatesClientFactory>();
			mockWiseRatesFactory
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((mockWiseRatesClient.Object, string.Empty));

			// Mock URS factory and client (should not be called)
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

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			// Given
			// Rate Service is ENABLED
			// URS feature is DISABLED
			// legacy Rate Selector is DISABLED
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (ObjectFactory.Substitute(MockURSFeatureHelper(false)))
			{
				// When doing Autorating
				AutoCostAndAssert
				(
					"",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: false,
					autorateCosts: true
				);

				// Then
				// URS provider (UrsRatesProvider) should NOT be called to get rates
				// legacy Rate Service provider (WiseRatesProvider) should be called to get rates
				mockUrsFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
				mockUrsClient.Verify(f => f.GetTradeServicesAsync(It.IsAny<QueryRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
				mockWiseRatesFactory.Verify(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()), Times.Once);
				mockWiseRatesClient.Verify(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
			}
		}
	}
}
