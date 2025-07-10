using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Api = WiseRates.Api;
using WRConstants = WiseRates.Constants.WRConstants;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserTestHelper : TestHelper
	{
		public RateChooserTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgHeader CreateCarrierOrg(string scac = "SCAC", string carrierFullName = null)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = scac + "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.OH_FullName = carrierFullName;
			carrier.CompanyData.SetAPTaxApplicable(false);

			if (!string.IsNullOrWhiteSpace(scac))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = scac;
				shippingLine.RSL_BookingRequestAvailable = true;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			return carrier;
		}

		public static Api.Model.Rate CreateApiRate(RefContainer refContainer, OrgHeader carrier, string commodity, string containerMode = "FCL", string transportMode = "SEA", string serviceLevel = "STD", string rateId = null)
		{
			return new Api.Model.Rate
			{
				Carrier = carrier?.SCACCode ?? string.Empty,
				Charges = new List<Api.Model.Charge>(),
				ContainerMode = containerMode,
				Destination = "HKHKG",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "USLAX",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = transportMode,
				Provider = WRConstants.RateProviders.CargoSphere,
				Container = refContainer != null
					? new Api.Model.RefContainer { Code = refContainer.RC_ISOType, ISOType = refContainer.RC_ISOType }
					: null,
				Commodity = commodity,
				ServiceLevel = serviceLevel,
				ProviderRateId = rateId
			};
		}

		public Api.Model.Rate CreateApiRate(string containerType, OrgHeader carrier, string commodity, string containerMode = "FCL", string transportMode = "SEA", string serviceLevel = "STD", string rateId = null)
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerType));
			return CreateApiRate(refContainer, carrier, commodity, containerMode, transportMode, serviceLevel, rateId);
		}

		public Api.Model.Charge AddPercentageCharge(Api.Model.Rate rate, string code, decimal percentage, string percentageAppliesTo, string group = "ORG", string carrierCode = null, string carrierDesc = null, bool mapCodeWithCW1ChargeCode = false)
		{
			if (mapCodeWithCW1ChargeCode)
			{
				code = ChargeCodes[code].AC_Code;
			}

			var charge = new Api.Model.Charge
			{
				ChargeCode = code,
				Currency = "AUD",
				Percentage = percentage,
				PercentageAppliesTo = percentageAppliesTo,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = carrierCode ?? code, Description = carrierDesc ?? code, Group = group },
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean
			};
			rate.Charges.Add(charge);
			return charge;
		}

		public Api.Model.Charge AddPerContainerCharge(Api.Model.Rate rate, string code, decimal perContainerRate, string group = "FRT", string carrierCode = null, string carrierDesc = null, bool mapCodeWithCW1ChargeCode = false, string currency = "AUD")
		{
			if (mapCodeWithCW1ChargeCode)
			{
				code = ChargeCodes[code].AC_Code;
			}
			return AddPerContainerCharge(rate, code, perContainerRate, group, carrierCode, carrierDesc, currency);
		}

		public static Api.Model.Charge AddPerContainerCharge(Api.Model.Rate rate, string code, decimal perContainerRate, string group = "FRT", string carrierCode = null, string carrierDesc = null, string currency = "AUD")
		{
			var charge = new Api.Model.Charge
			{
				ChargeCode = code,
				Currency = currency,
				Unit = "CN",
				PerUnitRate = perContainerRate,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = carrierCode ?? code, Description = carrierDesc ?? code, Group = group },
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean
			};
			rate.Charges.Add(charge);
			return charge;
		}

		public Api.Model.Charge AddFlatCharge(Api.Model.Rate rate, string code, decimal flatRate, string group = "DST", string carrierCode = null, string carrierDesc = null, bool mapCodeWithCW1ChargeCode = false)
		{
			if (mapCodeWithCW1ChargeCode)
			{
				var helper = new TestHelper(Factory);
				code = helper.ChargeCodes[code].AC_Code;
			}
			return AddFlatCharge(rate, code, flatRate, group, carrierCode, carrierDesc);
		}

		public static Api.Model.Charge AddFlatCharge(Api.Model.Rate rate, string code, decimal flatRate, string group = "DST", string carrierCode = null, string carrierDesc = null)
		{
			var charge = new Api.Model.Charge
			{
				ChargeCode = code,
				Currency = "AUD",
				FlatRate = flatRate,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = carrierCode ?? code, Description = carrierDesc ?? code, Group = group },
				CustomCategory = WRConstants.ChargeCustomCategory.BOL
			};
			rate.Charges.Add(charge);
			return charge;
		}

		public static Api.Model.Charge AddSubjectToCharge(Api.Model.Rate rate, string code, string freightChargeCode = "FRT")
		{
			var charge = new Api.Model.Charge
			{
				ChargeCode = code,
				Currency = "AUD",
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = code, Description = code, Group = "FRT" },
				ChargeType = Api.Model.ChargeType.SubjectTo,
				FreightInclusiveCarriageCharge = freightChargeCode
			};

			rate.Charges.Add(charge);
			return charge;
		}

		public static Api.Model.Charge AddIncludedCharge(Api.Model.Rate rate, string code, string freightChargeCode = "FRT")
		{
			var charge = new Api.Model.Charge
			{
				ChargeCode = code,
				Currency = "AUD",
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = code, Description = code, Group = "FRT" },
				ChargeType = Api.Model.ChargeType.Included,
				FreightInclusiveCarriageCharge = freightChargeCode
			};

			rate.Charges.Add(charge);
			return charge;
		}

		public ForwardingConsol CreateConsol(string origin = "USLAX", string destination = "HKHKG")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.Transports[0].JW_ETD = ZDate.Today.AddDays(2);
			consol.Transports[0].JW_ETA = ZDate.Today.AddDays(4);
			consol.Shipments.AddNew();

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			return consol;
		}

		public ForwardingContainer AddContainer(
			ForwardingConsol consol,
			string code = "20GP",
			string commodityCode = "GEN",
			string containerMode = Core.Constants.ContainerModes.FCL,
			short containerCount = 1)
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, code);

			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerMode = containerMode;
			container.JC_RH_NKContainerCommodityCode = commodityCode;
			container.JC_ContainerCount = containerCount;
			return container;
		}

		public IEnumerable<CW1RateCombinationResult> NewCW1RateCombinations(RatingCriteria criteria, string containerType, string commodityCode, params IRateEntry[] entries) =>
			NewCW1RateCombinations(criteria, Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType), commodityCode, entries);

		public IEnumerable<CW1RateCombinationResult> NewCW1RateCombinations(RatingCriteria criteria, RefContainer containerType, string commodityCode, params IRateEntry[] entries) =>
			NewCW1RateCombinations(criteria, containerType?.PK ?? ZGuid.Empty, commodityCode, entries);

		public IEnumerable<CW1RateCombinationResult> NewCW1RateCombinations(RatingCriteria criteria, ZGuid containerTypePk, string commodityCode, params IRateEntry[] entries)
		{
			criteria.IsManualCostSelectMode = true;

			yield return new CW1RateCombinationResult
			{
				Criteria = criteria,
				ContainerTypePk = containerTypePk,
				CommodityCode = commodityCode,
				HeadEntry = entries.FirstOrDefault(),
				Lines = entries.SelectMany(x => x.ChildRateLines)
			};
		}

		public void SetFilter_ContractNumber(RateChooserFilterStripBusinessObject filter, params string[] contractNumbers)
		{
			foreach (var contractNumberFilter in filter.GetFilters<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber))
			{
				contractNumberFilter.Property = ZString.Empty;
			}

			foreach (var contractNumber in contractNumbers)
			{
				var contractNumberFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);
				((ModuleTextFilter)contractNumberFilter.CurrentModuleFilter).Property = contractNumber;
			}
		}

		public void SetFilter_ServiceProvider(RateChooserFilterStripBusinessObject filter, params ZGuid[] serviceProviderPKs)
		{
			foreach (var serviceProviderFilter in filter.GetFilters<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider))
			{
				serviceProviderFilter.Property = ZGuid.Empty;
			}

			foreach (var serviceProviderPk in serviceProviderPKs)
			{
				var serviceProviderFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
				((ModuleGuidFilter)serviceProviderFilter.CurrentModuleFilter).Property = serviceProviderPk;
			}
		}

		public void SetFilter_CarrierServiceLevel(RateChooserFilterStripBusinessObject filter, params string[] serviceLevels)
		{
			foreach (var serviceProviderFilter in filter.GetFilters<ModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel))
			{
				serviceProviderFilter.Property = ZString.Empty;
			}

			foreach (var serviceLevel in serviceLevels)
			{
				var serviceProviderFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel);
				((ModuleTextFilter)serviceProviderFilter.CurrentModuleFilter).Property = serviceLevel;
			}
		}

		public Api.Model.RatesSearchResponse CreateWiseRateResponseForCargoSphere(params Api.Model.Rate[] apiRates)
		{
			var helper = new TestHelper(Factory);
			foreach (var rate in apiRates)
			{
				if (string.IsNullOrEmpty(rate.Provider))
				{
					rate.Provider = WRConstants.RateProviders.CargoSphere;
				}
			}

			var response = new Api.Model.RatesSearchResponse
			{
				Rates = apiRates.ToArray(),
				Carriers = apiRates.Select(x => x.Carrier).Distinct()
					.Select(x => new Api.Model.RefCarrier { Code = x, SCACCode = x, Name = x + " Carrier" }).ToArray(),
				Providers = apiRates.Select(x => x.Provider).Distinct()
					.Select(x => new Api.Model.ProviderResult() { ProviderCode = x, ProviderName = x, ConnectionResult = global::WiseRates.Tools.Enums.ConnectionResult.Success }).ToArray(),
				ServiceLevels = apiRates.Select(r => r.ServiceLevel).Distinct().Select(s => new Api.Model.RefServiceLevel() { Code = s, Description = s }).ToArray()
			};

			var responseChargeCodes = new List<Api.Model.RefChargeCode>();
			foreach (var chargeCodeGroup in apiRates.SelectMany(x => x.Charges).GroupBy(x => x.ChargeCode))
			{
				var cw1ChargeCode = helper.ChargeCodes.GetExisting(chargeCodeGroup.Key);
				responseChargeCodes.Add(new Api.Model.RefChargeCode
				{
					Code = chargeCodeGroup.Key,
					Group = cw1ChargeCode?.AC_ChargeGroup ?? "FRT",
					Description = chargeCodeGroup.Key + " Desc"
				});
			}
			response.ChargeCodes = responseChargeCodes.ToArray();

			return response;
		}

		public WiseRatesProvider CreateMockWiseRateProviderWithResponse(Api.Model.RatesSearchResponse response, Predicate<Api.Model.RatesSearchRequest> predicate = null, Action mockAction = null)
		{
			var wiseRatesClientMock = new Mock<Api.Client.IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.Is<Api.Model.RatesSearchRequest>(p => predicate == null || predicate.Invoke(p)), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Callback(() => mockAction?.Invoke())
				.Returns(Task.FromResult(response));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			return new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
		}

		public AutoRateInfoCollection GetCalculatedResultFromEntries(IEnumerable<IRateEntry> entries, OrgHeader carrier, RefContainer refContainer = default, ILoggerExtended logger = default)
		{
			if (refContainer == default)
			{
				refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			}

			if (logger == default)
			{
				logger = new ElementaryLogger();
			}

			var billTo = Factory.NewWithValidTestData<OrgHeader>();
			var testRating = new TestRatingCriteria("AUSYD", "INIXE", 1, refContainer, billTo);
			testRating.AdapterType = AdapterType.Consolidation;
			testRating.OperationalJobCode = "S000000001";

			var ratingContext = new RatingContext(logger);
			testRating.Creditors = Creditors.New(OrgWithSource.New(carrier, new List<string> { "Consol" }));

			var options = new NotApplicableRateLineRemover.FilterOptions { DisableSpotFilter = true };
			var freightAutoRater = new FreightAutoRater(ratingContext);
			return freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, testRating, entries, options);
		}

		public AutoRateInfoCollection GetCalculatedResultFromEntries(IList<WiseEntry> entries, string adapterID)
		{
			var collection = new AutoRateInfoCollection(Factory);

			foreach (var item in entries)
			{
				foreach (var line in item.ChildRateLines)
				{
					var result = new AutoRateInfo(Factory)
					{
						ChargeCode = line.ChargeCode,
						Currency = line.TL_RX_NKCurrency
					};
					result.SetLine_ForTest(line);

					if (line.Calculator is FlatCalculator flatCalc)
					{
						result.AddFlatPaymentBasis(flatCalc.BaseRate, adapterID, line.TL_RX_NKCurrency);
					}

					collection.Add(result);
				}
			}

			return collection;
		}

		public AutoRateInfo BuildAutoRateInfo(string chargeCode, List<WiseLine> lines)
		{
			var result = new AutoRateInfo(Factory)
			{
				ChargeCode = ChargeCodes[chargeCode],
				Currency = "AUD",
				ChargeUnit = "KG"
			};
			result.SetLine_ForTest(FindRateLineWithChargeCode(chargeCode, lines));

			return result;
		}

		WiseLine FindRateLineWithChargeCode(string wantedChargeCode, IList<WiseLine> rateLines)
		{
			return rateLines.Single(rl => rl.ChargeCode.AC_Code == wantedChargeCode);
		}

		public static OrgCarrierServiceLevel AddCarrierServiceLevel(OrgHeader carrier, string carrierCode, string universalCode = null)
		{
			var level = carrier.MiscServ.CarrierServiceLevels.AddNew();
			level.PL_Code = carrierCode;
			level.PL_CarrierServiceLevelDescription = carrierCode + " Desc";
			level.PL_CarrierServiceCode = universalCode;
			return level;
		}
	}

	internal class DummyRateChooserServices : IRateChooserServices
	{
		public const decimal AUDRate = 0.75m;
		public const decimal NZDRate = 0.5m;
		public const decimal HKDRate = 0.2m;
		public const decimal DefaultRate = 0.25m;

		public DummyRateChooserServices(BusinessObjectFactory factory, params string[] currenciesMissingRates)
		{
			Factory = factory;
			this.currenciesMissingRates = currenciesMissingRates;
		}
		public BusinessObjectFactory Factory { get; }
		readonly string[] currenciesMissingRates;

		public Money ConvertToDefaultCurrency(decimal amount, string currency)
		{
			if (currency == RateChooserModel.DefaultCurrency)
			{
				return new Money(amount, LoadCurrency(RateChooserModel.DefaultCurrency));
			}
			else if (currenciesMissingRates.Any(x => x == currency))
			{
				return new Money(amount, LoadCurrency(currency), false);
			}
			else
			{
				decimal rate;
				switch (currency)
				{
					case "AUD":
						rate = AUDRate;
						break;
					case "NZD":
						rate = NZDRate;
						break;
					case "HKD":
						rate = HKDRate;
						break;
					default:
						rate = DefaultRate;
						break;
				}
				return new Money(amount * rate, LoadCurrency(currency));
			}
		}

		RefCurrency LoadCurrency(string code) => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, code);

		public string ConvertToCurrentCompanyFormat(decimal amount)
			=> "$" + new ZDecimal(amount).ToString(2);
	}
}