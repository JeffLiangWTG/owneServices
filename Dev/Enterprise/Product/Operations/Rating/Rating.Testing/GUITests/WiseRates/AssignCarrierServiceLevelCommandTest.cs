using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using Constants = Enterprise.Core.Constants;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignCarrierServiceLevelCommandTest : BaseRatingIntegrationTest
	{
		public void TestIsDisabled_WhenCarrierIsUnmapped()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			Factory.Save();

			var rateSearchResponse = GetValidRatesSearchResponse(carrier);
			var rate = rateSearchResponse.Rates.Single();
			rate.Carrier = "UNMAPPED";
			rateSearchResponse.Carriers = new[]
			{
				new RefCarrier { Code = "UNMAPPED", SCACCode = "SCAC", Name = "Universal Carrier unmapped local" }
			};

			var wiseRatingHeaderView = CreateWiseRatingHeaderViewFromRatesSearchResponse(rateSearchResponse);
			var wiseRateEntryView = wiseRatingHeaderView.WiseEntryViews.Cast<WiseEntryView>().Single();

			AssertEquals("There is no local mapped carrier", true, wiseRateEntryView.CarrierCodeInfo.HasErrors());

			using (var testGrid = new ZGrid { DataSource = wiseRatingHeaderView })
			{
				var command = new AssignCarrierServiceLevelsCommand(testGrid) as IWiseRatesCommand;

				AssertEquals(
					"Carrier is unmapped, cannot map carrier service level.", 
					false, 
					command.IsEnabled(wiseRateEntryView)
				);

				AssertEquals(
					"Carrier is unmapped, cannot map carrier service level.", 
					ZString.Empty, 
					wiseRateEntryView.TI_PL_NKCarrierServiceLevel
				);
			}
		}

		public void TestIsDisabled_WhenCarrierIsMapped_ServiceLevelIsMapped()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			// map carrier scac code
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			// map carrier service level
			var mappedCarrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			mappedCarrierServiceLevel.PL_Code = "LSC";
			mappedCarrierServiceLevel.PL_CarrierServiceCode = "USC";
			mappedCarrierServiceLevel.PL_CarrierServiceLevelDescription = "USC mapped to LSC";

			Factory.Save();

			var rateSearchResponse = GetValidRatesSearchResponse(carrier);
			var wiseRatingHeaderView = CreateWiseRatingHeaderViewFromRatesSearchResponse(rateSearchResponse);
			var wiseRateEntryView = wiseRatingHeaderView.WiseEntryViews.Cast<WiseEntryView>().Single();

			using (var testGrid = new ZGrid { DataSource = wiseRatingHeaderView })
			{
				var command = new AssignCarrierServiceLevelsCommand(testGrid) as IWiseRatesCommand;
				AssertEquals("Codes are mapped, does not need new quick mapping.", false, command.IsEnabled(wiseRateEntryView));
				AssertEquals("It is local code mapped from Universal Service Level.", "LSC", wiseRateEntryView.TI_PL_NKCarrierServiceLevel);
			}
		}

		public void TestIsEnabled_WhenCarrierIsMapped_ServiceLevelIsNotMapped()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			// map carrier scac code
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			Factory.Save();

			var standardCarrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().Single();
			AssertEquals("Standard carrier service level code mismatch", "STD", standardCarrierServiceLevel.PL_Code);

			var rateSearchResponse = GetValidRatesSearchResponse(carrier);
			var rate = rateSearchResponse.Rates.Single();
			AssertEquals("Rate service level mismatch", "USC", rate.ServiceLevel);

			var wiseRatingHeaderView = CreateWiseRatingHeaderViewFromRatesSearchResponse(rateSearchResponse);
			var wiseRateEntryView = wiseRatingHeaderView.WiseEntryViews.Cast<WiseEntryView>().Single();
			using (var testGrid = new ZGrid { DataSource = wiseRatingHeaderView })
			{
				var command = new AssignCarrierServiceLevelsCommand(testGrid) as IWiseRatesCommand;
				AssertEquals(
					"Universal Carrier Service Level Code should be unmapped.",
					true,
					command.IsEnabled(wiseRateEntryView)
				);

				AssertEquals(
					"it is unmapped Universal Service Level code.",
					ZString.Empty,
					wiseRateEntryView.TI_PL_NKCarrierServiceLevel
				);

				AssertEquals(
					"There should be errors for unmapped codes",
					true,
					wiseRateEntryView.TI_PL_NKCarrierServiceLevelInfo.HasErrors()
				);
			}
		}

		public void TestGetAllUnmappedCarrierServiceLevelsFromOneCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			// map carrier scac code
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			// map carrier service level
			var mappedCarrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			mappedCarrierServiceLevel.PL_Code = "LSC";
			mappedCarrierServiceLevel.PL_CarrierServiceCode = "USC";
			mappedCarrierServiceLevel.PL_CarrierServiceLevelDescription = "USC mapped to LSC";

			Factory.Save();

			var rateSearchResponse = GetValidRatesSearchResponse(carrier);

			var rate0 = new Rate
			{
				Carrier = "WiseCarrier",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				ServiceLevel = "USC",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Provider = WRConstants.RateProviders.CargoSphere,
				Charges = new[]
				{
					new Charge { ChargeCode = "FRT", Currency = Constants.CurrencyCodes.Australia }
				},
			};

			var rate1 = new Rate
			{
				Carrier = "WiseCarrier",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				ServiceLevel = "USC1",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Provider = WRConstants.RateProviders.CargoSphere,
				Charges = new[]
				{
					new Charge { ChargeCode = "FRT", Currency = Constants.CurrencyCodes.Australia }
				},
			};

			var rate2 = new Rate
			{
				Carrier = "WiseCarrier",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				ServiceLevel = "USC2",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Provider = WRConstants.RateProviders.CargoSphere,
				Charges = new[]
				{
					new Charge { ChargeCode = "BAF", Currency = Constants.CurrencyCodes.Australia }
				},
			};

			rateSearchResponse.Rates = new[] { rate0, rate1, rate2 };
			rateSearchResponse.ServiceLevels = new[]
			{
				new RefServiceLevel { Code = "USC", Description = "Mapped Code" },
				new RefServiceLevel { Code = "USC1", Description = "Unmapped Code 1" },
				new RefServiceLevel { Code = "USC2", Description = "Unmapped Code 2" }
			};

			var expectedUnmappedCodes = new[]
			{
				new UnmappedForeignCode("USC1", Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel),
				new UnmappedForeignCode("USC2", Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel)
			};

			var wiseRatingHeaderView = CreateWiseRatingHeaderViewFromRatesSearchResponse(rateSearchResponse);
			var wiseRateEntryView = wiseRatingHeaderView.WiseEntryViews.Cast<WiseEntryView>().First();
			var unmappedCodes = AssignCarrierServiceLevelsCommand.UnmappedCarrierServiceLevels(wiseRatingHeaderView, wiseRateEntryView);

			AssertContainsExactElementsInAnyOrder(
				"Expected unmapped carrier service levels to match the expected values",
				expectedUnmappedCodes,
				unmappedCodes
			);
		}

		#region Helpers

		RatesSearchResponse GetValidRatesSearchResponse(OrgHeader carrier)
		{
			var costing = new Rate
			{
				Carrier = "WiseCarrier",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				ServiceLevel = "USC",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Provider = WRConstants.RateProviders.CargoSphere,
				Charges = new List<Charge>(),
			};
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			return new RatesSearchResponse
			{
				Rates = new[] { costing },
				Carriers = new[]
				{
					new RefCarrier { Code = "WiseCarrier", SCACCode = "SCAC", Name = carrier.OH_FullName }
				},
				ServiceLevels = new[] {
					new RefServiceLevel { Code = "USC", Description = "Universal description" }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "FRT" } },
			};
		}

		WiseRatingHeaderView CreateWiseRatingHeaderViewFromRatesSearchResponse(RatesSearchResponse ratesSearchResponse)
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<Enterprise.Integration.ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);

			var ratesQuery = new RatesQuery();
			wiseRatingHeaderView.SendRatesRequest(new List<RatesQuery> { ratesQuery });

			return wiseRatingHeaderView;
		}

		#endregion
	}
}