using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI.Test
{
	public class RateChooserFilterStripControlTest : RatingTestCase
	{
		public void TestFilterControlDefaulting_JobIsOneOffQuote()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.StartDate = DateTime.Now.Date;
			quotedBooking.EndDate = ZDateTime.Now.Date;
			quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quotedBooking.Quote.CurrentOneOffQuote.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quotedBooking.LoadPort = "AUMEL";
			quotedBooking.DischargePort = "SEGOT";

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(quotedBooking.GetFirstAdapter());
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<ModuleSingleDateFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn));

				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUSYD" && x.Property2 == "USLAX"));

				AssertNotNull(activeModuleFilters.OfType<ModuleGuidFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider && x.Property == TransportProvider1.PK));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber && x.Property == "CN123"));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.NamedAccount && x.Property == "NAC"));
			}
		}

		public void TestFilterControlDefaulting_JobIsQuickBooking()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.BookingOnly);
			quickBooking.StartDate = DateTime.Now.Date;
			quickBooking.EndDate = ZDateTime.Now.Date;
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quickBooking.LoadPort = "AUMEL";
			quickBooking.DischargePort = "SEGOT";

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(quickBooking.GetFirstAdapter());
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<ModuleSingleDateFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn));

				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUMEL" && x.Property2 == "SEGOT"));

				AssertNotNull(activeModuleFilters.OfType<ModuleGuidFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider && x.Property == TransportProvider1.PK));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber && x.Property == "CN123"));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.NamedAccount && x.Property == "NAC"));
			}
		}

		public void TestFilterControlDefaulting_JobIsQuickBookingWithCode()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var quickBooking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "AIR", "LSE", ZString.Empty, consignor, consignor, consignee, TransportProvider1, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.AcceptedBookingWithQuote);
			quickBooking.StartDate = DateTime.Now.Date;
			quickBooking.EndDate = ZDateTime.Now.Date;
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "CN123");
			quickBooking.Booking.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");
			quickBooking.LoadPort = "AUMEL";
			quickBooking.DischargePort = "SEGOT";

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(quickBooking.GetFirstAdapter());
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<ModuleSingleDateFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn));

				AssertNotNull(activeModuleFilters.OfType<ModuleLocationFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.OriginDestination && x.Property1 == "AUMEL" && x.Property2 == "SEGOT"));

				AssertNotNull(activeModuleFilters.OfType<ModuleGuidFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider && x.Property == TransportProvider1.PK));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber && x.Property == "CN123"));

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.NamedAccount && x.Property == "NAC"));
			}
		}

		public void TestUniversalCarrierServiceFilter_WhenCarrierServiceCodeIsEmpty_DefaultToCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consol = ChooserHelper.CreateConsol();
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var logger = new ElementaryLogger();
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, null, null);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			var serviceLevel = carrier.MiscServ.CarrierServiceLevels[0];
			serviceLevel.PL_Code = "STD";
			serviceLevel.PL_CarrierServiceCode = "ABC";
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel && x.Property == serviceLevel.PL_CarrierServiceCode));
			}

			serviceLevel.PL_CarrierServiceCode = string.Empty;
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel && x.Property == serviceLevel.PL_Code));
			}

			serviceLevel.PL_Code = string.Empty;
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>().SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel));
			}
		}

		public void TestUniversalCarrierServiceFilter_WhenCarrierServiceCodeIsAdded_UseNewCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var logger = new ElementaryLogger();
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, null, null);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			var serviceLevel = Factory.NewWithValidTestData<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "NEW";
			carrier.MiscServ.CarrierServiceLevels.Add(serviceLevel);
			consol.JK_AWBServiceLevel = "NEW";
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNotNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>()
					.SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel && x.Property == serviceLevel.PL_Code));
			}

			serviceLevel.PL_Code = "ABC";
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				AssertNull(activeModuleFilters.OfType<WiseRatesModuleTextFilter>().SingleOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel));
			}
		}

		public void TestEffectiveDateFilterDefaulting_SingleRoute()
		{
			var consol = ChooserHelper.CreateConsol();
			var container = ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			container.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(10);

			AssertDefaultEffectiveDateFilter(consol.RatingAdapter, container.JC_FCLWharfGateIn, false);

			Assert(true);
		}

		public void TestEffectiveDateFilterDefaulting_MultiRoute()
		{
			var consol = ChooserHelper.CreateConsol();
			consol.Transports[0].JW_CarrierBookingReference = "123";
			var route2 = consol.Transports.AddNew();
			route2.JW_RL_NKLoadPort = "HKHKG";
			route2.JW_RL_NKDiscPort = "SGSIN";
			route2.JW_CarrierBookingReference = "456";

			var container = ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			container.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(10);

			var routingSupport = (IRoutingSupport)consol;
			var routeSetRatingRoute = routingSupport.TransportsIncludingRelated.RouteSets
				.Select(x => new RouteSetRatingRoute(x, routingSupport))
				.ToList();

			AssertEquals("The route set rating route count should match the expected value.", 2, routeSetRatingRoute.Count);

			var multiRouteCriteriaList = routeSetRatingRoute.Select(x => new ForwardingConsolRatingAdapter(x, true)).Cast<IAutoRating>().ToList();
			multiRouteCriteriaList.Add(new ForwardingConsolJobServicesAdapter(new ConsolRatingRoute(consol)));

			var singleRouteCriteria = consol.RatingAdapter;

			AssertDefaultEffectiveDateFilter(singleRouteCriteria, container.JC_FCLWharfGateIn, false);

			AssertDefaultEffectiveDateFilter(multiRouteCriteriaList[0], container.JC_FCLWharfGateIn, true);
			AssertDefaultEffectiveDateFilter(multiRouteCriteriaList[1], container.JC_FCLWharfGateIn, true);
			AssertDefaultEffectiveDateFilter(multiRouteCriteriaList[2], container.JC_FCLWharfGateIn, true);
		}

		void AssertDefaultEffectiveDateFilter(IAutoRating autoRating, ZDateTime expectedDate, bool isMultiRouteEnabled)
		{
			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Constants.RatingDateFilterTypes.Codes.Custom,
			};
			var frtChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			frtChargeGroup.ChargeGroupSettings.Add(new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ForwardingConsolCode,
				Mode = "SEA",
				DirectionCode = Constants.FreightShipmentDirection.Code.All,
				DateType = JobDateTypes.Codes.FirstContainerGateInDate
			});

			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteEnabled))
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var filterBizObject = (RateChooserFilterStripBusinessObject)filterControl.FilterBusinessObject;

				var effectiveDateFilter = filterBizObject
					.ActiveModuleFilters
					.OfType<ModuleSingleDateFilter>()
					.FirstOrDefault(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.EffectiveOn);

				AssertNotNull("effectiveDateFilter", effectiveDateFilter);
				AssertEquals("The effective date filter property should match the expected date.", expectedDate, effectiveDateFilter.Property1);
			}
		}

		public void TestServiceProviderFilterDefaulting_SingleRoute()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var creditor = Helper.NewOrgHeader();
			var creditorOnRoute = Helper.NewOrgHeader();
			var carrierOnRoute = Helper.NewOrgHeader();

			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.SetDefaultSendingForwarderAddress(Helper.NewOrgHeader());
			consol.SetDefaultReceivingForwarderAddress(Helper.NewOrgHeader());
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_PackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.Transports[0].CarrierPK = carrierOnRoute.PK;
			consol.Transports[0].CreditorPK = creditorOnRoute.PK;

			ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);

			var expectedOrgPKs = new[]
			{
				carrier.PK,
				creditor.PK,
				carrierOnRoute.PK,
				creditorOnRoute.PK,
			};

			AssertDefaultServiceProviderFilters(consol.RatingAdapter, expectedOrgPKs, false);

			Assert(true);
		}

		public void TestServiceProviderFilterDefaulting_MultiRoute()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var creditor = Helper.NewOrgHeader();
			var creditorOnRoute1 = Helper.NewOrgHeader();
			var carrierOnRoute1 = Helper.NewOrgHeader();
			var creditorOnRoute2 = Helper.NewOrgHeader();
			var carrierOnRoute2 = Helper.NewOrgHeader();

			Factory.Save();

			var consol = ChooserHelper.CreateConsol();
			consol.SetDefaultShippingLineAddress(carrier);
			consol.SetDefaultSendingForwarderAddress(Helper.NewOrgHeader());
			consol.SetDefaultReceivingForwarderAddress(Helper.NewOrgHeader());
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_PackDepotAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = Helper.NewOrgHeader().MainAddress.PK;
			consol.Transports[0].JW_CarrierBookingReference = "123";
			consol.Transports[0].CarrierPK = carrierOnRoute1.PK;
			consol.Transports[0].CreditorPK = creditorOnRoute1.PK;

			var route2 = consol.Transports.AddNew();
			route2.JW_RL_NKLoadPort = "HKHKG";
			route2.JW_RL_NKDiscPort = "SGSIN";
			route2.JW_CarrierBookingReference = "456";
			route2.CarrierPK = carrierOnRoute2.PK;
			route2.CreditorPK = creditorOnRoute2.PK;

			ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);

			var routingSupport = (IRoutingSupport)consol;
			var routeSetRatingRoute = routingSupport.TransportsIncludingRelated.RouteSets
				.Select(x => new RouteSetRatingRoute(x, routingSupport))
				.ToList();

			AssertEquals(
				"Expected 2 route sets for the given routing support.",
				2,
				routeSetRatingRoute.Count
			);

			var multiRouteCriteriaList = routeSetRatingRoute.Select(x =>
			{
				var result = new ForwardingConsolRatingAdapter(x, true);
				Application.DoEvents();
				return result;
			}).Cast<IAutoRating>().ToList();
			multiRouteCriteriaList.Add(new ForwardingConsolJobServicesAdapter(new ConsolRatingRoute(consol)));

			var singleRouteCriteria = consol.RatingAdapter;

			var expectedOrgPKsOnRoute1 = new[]
			{
				carrier.PK,
				creditor.PK,
				carrierOnRoute1.PK,
				creditorOnRoute1.PK,
			};
			var expectedOrgPKsOnRoute2 = new[]
			{
				carrier.PK,
				creditor.PK,
				carrierOnRoute2.PK,
				creditorOnRoute2.PK,
			};
			var expectedOrgPKsOnConsol = new[]
			{
				carrier.PK,
				creditor.PK,
				carrierOnRoute1.PK,
				creditorOnRoute1.PK,
			};

			AssertDefaultServiceProviderFilters(singleRouteCriteria, expectedOrgPKsOnConsol, false);

			AssertDefaultServiceProviderFilters(multiRouteCriteriaList[0], expectedOrgPKsOnRoute1, true);
			AssertDefaultServiceProviderFilters(multiRouteCriteriaList[1], expectedOrgPKsOnRoute2, true);
			AssertDefaultServiceProviderFilters(multiRouteCriteriaList[2], expectedOrgPKsOnConsol, true);
		}

		void AssertDefaultServiceProviderFilters(IAutoRating autoRating, ZGuid[] expectedOrgPKs, bool isMultiRouteEnabled)
		{
			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteEnabled))
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var filterBizObject = (RateChooserFilterStripBusinessObject)filterControl.FilterBusinessObject;

				var serviceProviderFilters = filterBizObject
					.ActiveModuleFilters
					.OfType<ModuleGuidFilter>()
					.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider);

				AssertEquals("ServiceProviderFilters count mismatch", 4, serviceProviderFilters.Count());
				AssertContainsExactElementsInAnyOrder(
					"Unexpected ServiceProviderFilters properties",
					expectedOrgPKs,
					serviceProviderFilters.Select(x => x.Property)
				);
			}
		}

		public void TestCarrierContractNumberTextStrip()
		{
			var consol = ChooserHelper.CreateConsol();
			consol.JK_CarrierContractNumber = ZString.Empty;

			var logger = new ElementaryLogger();
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, null, null);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				var carrierContractNumberFilters = activeModuleFilters
					.OfType<ModuleTextFilter>()
					.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);

				AssertEquals("Carrier contract number filters count should be 0.", 0, carrierContractNumberFilters.Count());
			}

			var newBlankCON = consol.Numbers.AddNew();
			newBlankCON.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			newBlankCON.CE_EntryNum = ZString.Empty;
			autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			criteria = new RatingCriteria(autoRatingInfo, Factory);
			model = new RateChooserModel(criteria, context);
			viewModel = new RateChooserViewModel(model);
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				var carrierContractNumberFilters = activeModuleFilters
					.OfType<ModuleTextFilter>()
					.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);

				AssertEquals("Carrier contract number filters count should be 0.", 0, carrierContractNumberFilters.Count());
			}

			consol.JK_CarrierContractNumber = "AAA";
			autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			criteria = new RatingCriteria(autoRatingInfo, Factory);
			model = new RateChooserModel(criteria, context);
			viewModel = new RateChooserViewModel(model);
			using (var form = new RateChooserForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = (RateChooserFilterStripControl)form.Controls.Find("stripControl", true)[0];
				var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

				var carrierContractNumberFilters = activeModuleFilters
					.OfType<ModuleTextFilter>()
					.Where(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);

				AssertEquals("Carrier contract number filter property should be 'AAA'.", (ZString)"AAA", carrierContractNumberFilters.Single().Property);
			}
		}

		RateChooserTestHelper ChooserHelper =>
			rateChooserTestHelper ?? (rateChooserTestHelper = new RateChooserTestHelper(Factory));
		RateChooserTestHelper rateChooserTestHelper;
	}
}
