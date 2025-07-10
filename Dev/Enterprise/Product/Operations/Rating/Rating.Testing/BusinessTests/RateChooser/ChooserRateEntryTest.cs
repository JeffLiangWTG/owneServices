using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business.Testing;
using Moq;
using NUnit.Framework;
using Api = WiseRates.Api;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(ChooserRateEntry))]
	public class ChooserRateEntryTest : NonPersistentBusinessObjectTestCase
	{
		RateChooserTestHelper ChooserHelper;

		protected override void SetUp()
		{
			base.SetUp();
			ChooserHelper = new RateChooserTestHelper(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TestHelper(Factory);
			var rate = helper.NewCosting(helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 10m);
			return new ChooserRateEntry(Factory, rateEntry, null, null);
		}

		public void TestCargoSphereProperties()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query
			var apiCosting1 = helper.CreateApiRate("20GP", carrier, "CO1");
			var charge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100);
			apiCosting1.ProviderCustomFields = new string[]
			{
				"rateType",
				"rateType2",
				"vessel",
				"arbitraryPermission",
				"tradeLane",
				"routing",
				"serviceString"
			}
			.Select(x => new Api.Model.CustomField() { Code = x, Description = x + " Desc", Value = x + "Value" })
			.ToArray();

			var logger = new ElementaryLogger();
			var response = new Api.Model.RatesSearchResponse
			{
				Rates = new[] { apiCosting1 },
				Carriers = new[] { new Api.Model.RefCarrier { Code = carrier.SCACCode, SCACCode = carrier.SCACCode } },
				ChargeCodes = new[] { new Api.Model.RefChargeCode { Code = "UNI", Group = "FRT", Description = "Universal Freight Charge" } },
			};
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services: null, null);
			AssertEquals("rateTypeValue", chooserRateEntry1.RateType);
			AssertEquals("rateType2Value", chooserRateEntry1.RateType2);
			AssertEquals("vesselValue", chooserRateEntry1.Vessel);
			AssertEquals("arbitraryPermissionValue", chooserRateEntry1.AddOn);
			AssertEquals("tradeLaneValue", chooserRateEntry1.TradeLane);
			AssertEquals("routingValue", chooserRateEntry1.Routing);
			AssertEquals("serviceStringValue", chooserRateEntry1.ServiceString);
		}

		public void TestActiveCharges()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query
			var apiCosting1 = helper.CreateApiRate("20GP", carrier, "CO1");
			var charge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100);
			var charge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BAF", 10)
				.OfType(Api.Model.ChargeType.Optional);
			var logger = new ElementaryLogger();
			var response = new Api.Model.RatesSearchResponse
			{
				Rates = new[] { apiCosting1 },
				Carriers = new[] { new Api.Model.RefCarrier { Code = carrier.SCACCode, SCACCode = carrier.SCACCode } },
				ChargeCodes = new[] { new Api.Model.RefChargeCode { Code = "UNI", Group = "FRT", Description = "Universal Freight Charge" } },
			};
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, null, null);
			AssertEquals(true, chooserRateEntry1.IsActive(charge1));
			AssertEquals(false, chooserRateEntry1.IsActive(charge2));

			chooserRateEntry1.SetActive(charge2, true);
			AssertEquals(true, chooserRateEntry1.IsActive(charge2));

			chooserRateEntry1.SetActive(charge2, false);
			AssertEquals(false, chooserRateEntry1.IsActive(charge2));
		}

		public void TestSetActive()
		{
			var testhelper = new TestHelper(Factory);
			var chooserHelper = new RateChooserTestHelper(Factory);
			var carrier = chooserHelper.CreateCarrierOrg("SCAC");
			var frtCharge2 = testhelper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			frtCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = chooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			chooserHelper.AddContainer(consol, "20GP", "GEN", "FCL", 3);
			chooserHelper.AddContainer(consol, "20GP", "ATPT", "FCL", 5);
			var apiCosting1 = chooserHelper.CreateApiRate("20GP", carrier, "");
			var charge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100);
			var charge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10)
				.OfType(Api.Model.ChargeType.Optional);

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
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
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);

			tab1.SelectedRate.SetActive(charge2, true);
			AssertEquals("charge is active on all tabs", true, tab2.SelectedRate.IsActive(charge2));
		}

		public void TestCarrierServiceLevelIsMapped()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "HI");
			RateChooserTestHelper.AddCarrierServiceLevel(carrier, "LO", "ULO");
			var apiCosting1 = helper.CreateApiRate("20GP", carrier, "CO1")
				.WithCarrierServiceLevel("UHI");
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var apiCosting2 = helper.CreateApiRate("20GP", carrier, "CO2")
				.WithCarrierServiceLevel("ULO");
			apiCosting2.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var logger = new ElementaryLogger();
			var response = new Api.Model.RatesSearchResponse
			{
				Rates = new[] { apiCosting1 },
				Carriers = new[] { new Api.Model.RefCarrier { Code = carrier.SCACCode, SCACCode = carrier.SCACCode } },
				ChargeCodes = new[] { new Api.Model.RefChargeCode { Code = "FRT", Group = "FRT" } },
			};
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			AssertEquals("PRE:", 1, converted1.Count);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, null, null);

			response.Rates = new[] { apiCosting2 };
			var converted2 = converter.Convert(response, null);
			AssertEquals("PRE:", 1, converted2.Count);
			var chooserRateEntry2 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting2, converted2, null, null);

			AssertEquals(false, chooserRateEntry1.CarrierServiceLevelIsMapped);
			AssertEquals("No Carrier Service Level under Carrier 'SCACCARRIER' is assigned to 'UHI'", chooserRateEntry1.CarrierServiceLevelError);
			AssertEquals(true, chooserRateEntry2.CarrierServiceLevelIsMapped);
			AssertEquals(string.Empty, chooserRateEntry2.CarrierServiceLevelError);
		}
	}
}
