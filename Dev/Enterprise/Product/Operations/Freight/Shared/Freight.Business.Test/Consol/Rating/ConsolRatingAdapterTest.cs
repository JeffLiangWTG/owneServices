using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolRatingAdapterTest : TestCaseWithFactory
	{
		public void TestAutoRating()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];
			var adapter = consol.RatingAdapter;

			AssertEquals(AdapterType.Consolidation, adapter.AdapterType);
			AssertEquals(consol.JK_UniqueConsignRef, adapter.JobID);
			AssertEquals(consol.JK_UniqueConsignRef + " Route 1", adapter.OperationalJobCode);

			var chargeCodeGroups = adapter.ChargeCodeGroups;
			AssertEquals(Env.Registry.Rating.FreightRatedCodes.Length, chargeCodeGroups.Count);
			AssertEquals(ChargeCodeFilter.AutorateNothing, chargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateConsolLevelOnly, chargeCodeGroups.CostChargesFilter);

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("AU");

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("USLAX", adapter.Origin.Code);
			AssertEquals("AUSYD", adapter.Destination.Code);

			var arrivalCto = Factory.New<OrgAddress>();
			var departureCTO = Factory.New<OrgAddress>();
			consol.JK_OA_ArrivalCTOAddress = arrivalCto.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTO.PK;
			Assert(adapter.IsImport());
			AssertEquals(arrivalCto.PK, adapter.WharfCTOAddress.PK);

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "GBLON";
			Assert(!adapter.IsImport());
			AssertEquals(departureCTO.PK, adapter.WharfCTOAddress.PK);

			GlbCompany.CurrentCompany.SetCountry(countryCode);

			AssertEquals(ZDateTime.Empty, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(ZDateTime.Empty, adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport.JW_ETD = new ZDateTime(2005, 12, 1);
			transport.JW_ETA = new ZDateTime(2005, 12, 10);
			AssertEquals(new ZDateTime(2005, 12, 1), adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2005, 12, 10), adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport.JW_ATD = new ZDateTime(2005, 12, 2);
			transport.JW_ATA = new ZDateTime(2005, 12, 14);
			AssertEquals(new ZDateTime(2005, 12, 2), adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2005, 12, 14), adapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);

			var consignor1 = OrgHeader.New(Factory);
			var consignor2 = OrgHeader.New(Factory);
			var consignee1 = OrgHeader.New(Factory);
			var consignee2 = OrgHeader.New(Factory);
			var ccustomer1 = OrgHeader.New(Factory);
			var ccustomer2 = OrgHeader.New(Factory);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment1.ConsignorPK = consignor1.PK;
			shipment1.ConsigneePK = consignee1.PK;
			shipment1.OuterPackLines[0].JL_RH_NKCommodityCode = "GEN";

			var scp1 = shipment1.ControllingCustomerAddress;
			scp1.OrganisationPK = ccustomer1.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.OuterPackLines.AddNew();
			shipment2.ConsignorPK = consignor1.PK;
			shipment2.ConsigneePK = consignee1.PK;
			shipment2.OuterPackLines[0].JL_RH_NKCommodityCode = "GEN";
			var scp2 = shipment2.ControllingCustomerAddress;
			scp2.OrganisationPK = ccustomer1.PK;

			AssertEquals(consignor1.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(consignee1.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);
			AssertEquals(ccustomer1.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS].PK);

			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("GEN", string.Join(", ", rateableMeasures.GetCommodities().OrderBy(x => x)));

			shipment2.ConsignorPK = consignor2.PK;
			shipment2.ConsigneePK = consignee2.PK;
			scp2.OrganisationPK = ccustomer2.PK;
			shipment2.OuterPackLines[0].JL_RH_NKCommodityCode = "HAZ";

			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);

			rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("GEN, HAZ", string.Join(", ", rateableMeasures.GetCommodities().OrderBy(x => x)));

			shipment1.ConsignorPK = consignor2.PK;
			shipment1.ConsigneePK = consignee2.PK;
			scp1.OrganisationPK = ccustomer2.PK;
			shipment1.OuterPackLines[0].JL_RH_NKCommodityCode = "HAZ";
			AssertEquals(consignor2.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(consignee2.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);
			AssertEquals(ccustomer2.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS].PK);

			rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("HAZ", string.Join(", ", rateableMeasures.GetCommodities().OrderBy(x => x)));

			shipment1.ConsignorPK = ZGuid.Empty;
			shipment2.ConsigneePK = ZGuid.Empty;
			scp1.OrganisationPK = ZGuid.Empty;
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals(ccustomer2.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS].PK);

			shipment2.ConsignorPK = ZGuid.Empty;
			shipment1.ConsigneePK = ZGuid.Empty;
			scp2.OrganisationPK = ZGuid.Empty;
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);

			shipment1.JS_ActualWeight = 1m;
			shipment1.JS_DocumentedWeight = 1.1m;
			shipment1.JS_ManifestedWeight = 0.975m;
			shipment1.JS_UnitOfWeight = "T";
			shipment1.JS_ActualVolume = 0.95m;
			shipment1.JS_UnitOfVolume = "M3";
			shipment1.JS_OuterPacks = 50;
			shipment1.JS_GoodsValue = 1000m;
			shipment1.JS_RX_NKGoodsValueCurr = "USD";
			shipment1.JS_InsuranceValue = 2000m;
			shipment1.JS_RX_NKInsuranceCurrency = "USD";
			shipment1.JS_ActualChargeable = 1.0M;
			shipment1.JS_DocumentedChargeable = 2.0M;
			shipment1.JS_ManifestedChargeable = 3.0M;
			shipment1.JS_LoadingMeters = 1.1m;
			shipment1.JS_DocumentedLoadingMeters = 1.2m;
			shipment1.JS_ManifestedLoadingMeters = 1.3m;

			shipment2.JS_ActualWeight = 600m;
			shipment2.JS_UnitOfWeight = "KG";
			shipment2.JS_ActualVolume = 700m;
			shipment2.JS_DocumentedVolume = 800m;
			shipment2.JS_ManifestedVolume = 600m;
			shipment2.JS_UnitOfVolume = "L";
			shipment2.JS_OuterPacks = 30;
			shipment2.JS_GoodsValue = 800m;
			shipment2.JS_RX_NKGoodsValueCurr = "EUR";
			shipment2.JS_InsuranceValue = 400m;
			shipment2.JS_RX_NKInsuranceCurrency = "EUR";
			shipment2.JS_ActualChargeable = 4.0M;
			shipment2.JS_DocumentedChargeable = 5.0M;
			shipment2.JS_ManifestedChargeable = 6.0M;
			shipment2.JS_LoadingMeters = 2.1m;
			shipment2.JS_DocumentedLoadingMeters = 2.2m;
			shipment2.JS_ManifestedLoadingMeters = 2.3m;

			rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals(1.600m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(1.700m, rateableMeasures.GetForClient(MeasureType.Weight));
			AssertEquals(1.575m, rateableMeasures.GetForProvider(MeasureType.Weight));
			AssertEquals("T", rateableMeasures.GetUnit(MeasureType.Weight));

			AssertEquals(1.65m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(1.75m, rateableMeasures.GetForClient(MeasureType.Volume));
			AssertEquals(1.55m, rateableMeasures.GetForProvider(MeasureType.Volume));
			AssertEquals("M3", rateableMeasures.GetUnit(MeasureType.Volume));

			AssertEquals(3.2m, rateableMeasures.GetActual(MeasureType.LoadingMeters));
			AssertEquals(3.4m, rateableMeasures.GetForClient(MeasureType.LoadingMeters));
			AssertEquals(3.6m, rateableMeasures.GetForProvider(MeasureType.LoadingMeters));

			AssertEquals(80m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals(2m, rateableMeasures.Shipments);

			AssertEquals(consol.JK_ConsolChargeable, rateableMeasures.GetActual(MeasureType.Chargeable));
			AssertEquals(consol.JK_ConsolChargeable, rateableMeasures.GetForClient(MeasureType.Chargeable));
			AssertEquals(consol.JK_ConsolChargeable, rateableMeasures.GetForProvider(MeasureType.Chargeable));
			AssertEquals("M3", rateableMeasures.GetUnit(MeasureType.Chargeable));

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 102.4m;
			consol.JK_CorrectedConsolVolume = 8.192m;
			consol.JK_CorrectedConsolWeightUnit = "T";
			consol.JK_CorrectedConsolVolumeUnit = "CY";

			AssertEquals("Precondition", Constants.Weight.Kilograms, consol.JK_TotalShipmentWeightUnit);
			AssertEquals("Precondition", Constants.Volume.CubicMetres, consol.JK_TotalShipmentVolumeUnit);

			AssertEquals(Constants.Weight.Tonnes, consol.JK_CorrectedConsolWeightUnit);
			AssertEquals(Constants.Volume.CubicYards, consol.JK_CorrectedConsolVolumeUnit);

			rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals(102.4m, rateableMeasures.GetActual(MeasureType.JobWeight));
			AssertEquals(102.4m, rateableMeasures.GetForClient(MeasureType.JobWeight));
			AssertEquals(102.4m, rateableMeasures.GetForProvider(MeasureType.JobWeight));
			AssertEquals(Constants.Weight.Tonnes, rateableMeasures.GetUnit(MeasureType.JobWeight));

			AssertEquals(8.192m, rateableMeasures.GetActual(MeasureType.JobVolume));
			AssertEquals(8.192m, rateableMeasures.GetForClient(MeasureType.JobVolume));
			AssertEquals(8.192m, rateableMeasures.GetForProvider(MeasureType.JobVolume));
			AssertEquals(Constants.Volume.CubicYards, rateableMeasures.GetUnit(MeasureType.JobVolume));

			var monetaryValues = adapter.MonetaryValues;

			AssertEquals(2, monetaryValues.Values[MoneyType.ValueType.GoodsValue].Count);
			AssertEquals(2, monetaryValues.Values[MoneyType.ValueType.InsuranceValue].Count);

			AssertEquals(1000m, monetaryValues.Values[MoneyType.ValueType.GoodsValue][0].Amount);
			AssertEquals("USD", monetaryValues.Values[MoneyType.ValueType.GoodsValue][0].Currency.Code);
			AssertEquals(2000m, monetaryValues.Values[MoneyType.ValueType.InsuranceValue][0].Amount);
			AssertEquals("USD", monetaryValues.Values[MoneyType.ValueType.InsuranceValue][0].Currency.Code);

			AssertEquals(800m, monetaryValues.Values[MoneyType.ValueType.GoodsValue][1].Amount);
			AssertEquals("EUR", monetaryValues.Values[MoneyType.ValueType.GoodsValue][1].Currency.Code);
			AssertEquals(400m, monetaryValues.Values[MoneyType.ValueType.InsuranceValue][1].Amount);
			AssertEquals("EUR", monetaryValues.Values[MoneyType.ValueType.InsuranceValue][1].Currency.Code);
		}

		public void TestJobID()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUPER";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUPER";
			transport2.JW_RL_NKDiscPort = "USLAX";

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var adapter = consol.RatingAdapter;
				AssertEquals(consol.JK_UniqueConsignRef, adapter.JobID);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				transport1.JW_CarrierBookingReference = "AAA";
				transport2.JW_CarrierBookingReference = "BBB";
				var adapter = consol.RatingAdapter;

				var message = "Even if we are going to AutoRate based on routes, still the JobId refers to Consol UniqueConsignRef";
				AssertEquals(message, consol.JK_UniqueConsignRef, adapter.JobID);
			}
		}

		public void TestViaGetter_ViasAreNotSpecified_ReturnEmptyString()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "UAIEV";

			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Cost));
			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_LoadViaIsSpecified_ReturnLoadVia()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "UAIEV";

			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_DischargeViaIsSpecified_ReturnLoadDischargeVia()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USNYC";

			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ConsolIsImportAndBothViasAreSpecified_ReturnDischargeVia()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";

			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals("Via", "AUMEL", consol.RatingAdapter.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "AUMEL", consol.RatingAdapter.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ConsolIsExportAndBothViasAreSpecified_ReturnLoadVia()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", consol.RatingAdapter.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ConsolIsOffshoreAndBothViasAreSpecified_ReturnEmptyString()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "UAIEV";

			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "AUMEL";

			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Cost));
			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_ConsolIsDomesticAndBothViasAreSpecified_ReturnEmptyString()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUCNS";

			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "AUMEL";

			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Cost));
			AssertNull("Via", consol.RatingAdapter.GetVia(CostSell.Revenue));
		}

		public void TestMatchingLocations()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD"; // FirstLoad
			consol.JK_RL_NKDischargePort = "AUCNS"; // LastDischarge

			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_TransportType = "PRE";
			consol.Transports[0].JW_TransportMode = "ROA";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "AUFRE";

			var transport1 = consol.Transports.AddNew("AUFRE", "USNYC");
			transport1.JW_TransportMode = Constants.TransportModes.Air;

			var transport2 = consol.Transports.AddNew("USNYC", "AUMEL");
			transport2.JW_TransportMode = Constants.TransportModes.Sea;

			var transport3 = consol.Transports.AddNew("AUMEL", "AUBNE");
			transport3.JW_TransportMode = Constants.TransportModes.Sea;

			var transport4 = consol.Transports.AddNew("AUBNE", "AUCNS");
			transport4.JW_TransportMode = Constants.TransportModes.Air;

			AssertEquals("FirstLoad", "AUSYD", consol.RatingAdapter.GetFirstLoad(CostSell.Cost).Code);
			AssertEquals("FirstLoad", "AUSYD", consol.RatingAdapter.GetFirstLoad(CostSell.Revenue).Code);
			AssertEquals("LastDischarge", "AUCNS", consol.RatingAdapter.GetLastDischarge(CostSell.Cost).Code);
			AssertEquals("LastDischarge", "AUCNS", consol.RatingAdapter.GetLastDischarge(CostSell.Revenue).Code);
			AssertEquals("FirstRouteSetLoad", "USNYC", consol.RatingAdapter.GetFirstRouteSetLoad(CostSell.Cost).Code);
			AssertEquals("FirstRouteSetLoad", "USNYC", consol.RatingAdapter.GetFirstRouteSetLoad(CostSell.Revenue).Code);
			AssertEquals("LastRouteSetDischarge", "AUBNE", consol.RatingAdapter.GetLastRouteSetDischarge(CostSell.Cost).Code);
			AssertEquals("LastRouteSetDischarge", "AUBNE", consol.RatingAdapter.GetLastRouteSetDischarge(CostSell.Revenue).Code);

			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("FirstLoad", "AUSYD", consol.RatingAdapter.GetFirstLoad(CostSell.Cost).Code);
			AssertEquals("FirstLoad", "AUSYD", consol.RatingAdapter.GetFirstLoad(CostSell.Revenue).Code);
			AssertEquals("LastDischarge", "AUCNS", consol.RatingAdapter.GetLastDischarge(CostSell.Cost).Code);
			AssertEquals("LastDischarge", "AUCNS", consol.RatingAdapter.GetLastDischarge(CostSell.Revenue).Code);
			AssertEquals("FirstRouteSetLoad", "AUFRE", consol.RatingAdapter.GetFirstRouteSetLoad(CostSell.Cost).Code);
			AssertEquals("FirstRouteSetLoad", "AUFRE", consol.RatingAdapter.GetFirstRouteSetLoad(CostSell.Revenue).Code);
			AssertEquals("LastRouteSetDischarge", "AUCNS", consol.RatingAdapter.GetLastRouteSetDischarge(CostSell.Cost).Code);
			AssertEquals("LastRouteSetDischarge", "AUCNS", consol.RatingAdapter.GetLastRouteSetDischarge(CostSell.Revenue).Code);
		}

		public void TestWharfCTOAddress()
		{
			var consol = Factory.New<CommonConsol>();
			var adapter = consol.RatingAdapter;

			consol.JK_OA_DepartureCTOAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_PackDepotAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;

			GlbCompany.CurrentCompany.SetCountry("AU");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Assert(adapter.IsImport());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(consol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(consol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(consol.JK_OA_ArrivalCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(consol.JK_OA_UnpackDepotAddress, adapter.WharfCTOAddress.PK);

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "GBLON";
			Assert(!adapter.IsImport());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(consol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(consol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(consol.JK_OA_DepartureCTOAddress, adapter.WharfCTOAddress.PK);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(consol.JK_OA_PackDepotAddress, adapter.WharfCTOAddress.PK);
		}

		public void TestAutoRatingFreightMode()
		{
			var consol = Factory.New<CommonConsol>();
			var adapter = consol.RatingAdapter;

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			AssertEquals(FreightMode.LSE, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			AssertEquals(FreightMode.ULD, adapter.FreightMode);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(FreightMode.FCL, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(FreightMode.LCL, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			AssertEquals(FreightMode.FCL, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(FreightMode.LCL, adapter.FreightMode);

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals(FreightMode.LCL, adapter.FreightMode);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40GP").PK;
			Assert(((RateableMeasureSet)adapter.RateableMeasures).IsContainerized().Value);
			AssertEquals(FreightMode.FCL, adapter.FreightMode);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(FreightMode.FRA, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(FreightMode.LRA, adapter.FreightMode);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(FreightMode.FRO, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(FreightMode.LRO, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.FTL;
			AssertEquals(FreightMode.FTL, adapter.FreightMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LTL;
			AssertEquals(FreightMode.LRO, adapter.FreightMode);
		}

		public void TestAutoRatingContainers()
		{
			var consol = Factory.New<CommonConsol>();
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_TareWeight = 1000;

			Factory.Save();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = gp20.PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var shipment = Factory.New<CommonShipment>();

			var pack11 = shipment.OuterPackLines.AddNew();
			consol.Shipments.Add(shipment);

			pack11.SetContainer(consol, container1);
			pack11.JL_ActualWeight = 1000m;
			pack11.JL_ActualVolume = 2m;
			pack11.JL_PackageCount = 100;

			var pack12 = shipment.OuterPackLines.AddNew();
			pack12.SetContainer(consol, container1);
			pack12.JL_ActualWeight = 5m;
			pack12.JL_ActualWeightUQ = "T";
			pack12.JL_ActualVolume = 7000m;
			pack12.JL_ActualVolumeUQ = "L";
			pack12.JL_PackageCount = 120;

			Assert(gp20.RC_TareWeight > 0);
			var rateableContainers = ((RateableMeasureSet)consol.RatingAdapter.RateableMeasures).GetAllContainers().ToList();
			AssertEquals(1, rateableContainers.Count);
			AssertEquals(6000m, rateableContainers[0].ContainerWeightInKG);
			AssertEquals(9m, rateableContainers[0].ContainerVolumeInM3);
			AssertEquals(220, rateableContainers[0].ContainerPackages);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = gp20.PK;
			container2.JC_ContainerMode = Constants.ContainerModes.FCL;
			var pack21 = shipment.OuterPackLines.AddNew();
			pack21.SetContainer(consol, container2);

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = gp20.PK;
			container3.JC_ContainerMode = Constants.ContainerModes.LCL;

			var pack31 = shipment.OuterPackLines.AddNew();
			pack31.SetContainer(consol, container3);
			pack31.JL_ActualWeight = 200m;
			pack31.JL_ActualVolume = 0.5m;
			pack31.JL_PackageCount = 10;

			var measures = (RateableMeasureSet)consol.RatingAdapter.RateableMeasures;
			rateableContainers = measures.GetAllContainers().ToList();
			AssertEquals(2, measures.GetContainerTypePKs().Count());
			AssertEquals(3, rateableContainers.Count);
			foreach (var info in measures.GetContainerGroups())
			{
				if (info.ContainerTypePK == MeasureInfo.ContainerInfo.LCL)
				{
					AssertEquals(1, info.ContainerCount);
					AssertEquals(200m, info.Containers.Single().ContainerWeightInKG);
					AssertEquals(0.5m, info.Containers.Single().ContainerVolumeInM3);
				}
				else if (info.ContainerTypePK == gp20.PK)
				{
					AssertEquals(2, info.ContainerCount);
					AssertEquals(container1.PK.ToGuid(), info.Containers.ElementAt(0).ContainerPK);
					AssertEquals(6000m, info.Containers.ElementAt(0).ContainerWeightInKG);
					AssertEquals(new Quantity(7000m, "KG"), info.Containers.ElementAt(0).ContainerGrossWeight);

					AssertEquals(container2.PK.ToGuid(), info.Containers.ElementAt(1).ContainerPK);
					AssertEquals(0m, info.Containers.ElementAt(1).ContainerWeightInKG);
					AssertEquals(new Quantity(1000m, "KG"), info.Containers.ElementAt(1).ContainerGrossWeight);
				}
				else
				{
					Assert(false);
				}
			}
		}

		public void TestAutoRatingTransportProviders_GatewayConsolCosting()
		{
			var creditor = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			var cto = Factory.New<OrgHeader>();

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			Assert("Pre-condition", consol.IsGatewayBillingEnabled());

			var adapter = consol.RatingAdapter;
			AssertEquals("Creditor is not branch org proxy", 3, adapter.Creditors.AllOrgs.Count);

			consol.JK_OA_CreditorAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			AssertEquals("Creditor IS branch org proxy, and performing consol costing so ONLY creditor", 1, adapter.Creditors.AllOrgs.Count);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.PK, adapter.Creditors.AllOrgs[0].PK);

			consol.JK_AgentType = Constants.AgentType.Courier;

			AssertEquals("No longer gateway, so normal behaviour", 2, adapter.Creditors.AllOrgs.Count);
		}

		public void TestJobServices_DeduplicatesHiddenServiceCodes()
		{
			var testConsol = Factory.New<CommonConsol>();
			testConsol.JK_RL_NKLoadPort = "USLAX";
			testConsol.JK_RL_NKDischargePort = "AUSYD";

			var container1 = testConsol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			// Create 4 hidden services
			var penalties = container1.ImportPenalties;
			penalties.CreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Storage, Constants.ContainerPenaltyCreditorType.Codes.CTO, timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days).DurationAsDays = 2;
			penalties.CreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Storage, Constants.ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days).DurationAsDays = 2;
			penalties.CreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Detention, Constants.ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days).DurationAsDays = 2;
			penalties.CreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime, Constants.ContainerPenaltyCreditorType.Codes.Transport, timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days).DurationAsDays = 2;

			// Simulate the user adding all the hidden service codes to the shipment service registry.
			// The two sources of codes need to be de-duplicated by the adapter.
			var serviceRegistry = FreightDataRegistry.Instance.JobServices;
			var regValue = serviceRegistry.Value;
			var hiddenServiceCodeList = FreightRatingHelper.HiddenContainerServices.GetAllCodes();
			foreach (var hiddenServiceCode in hiddenServiceCodeList)
			{
				if (!regValue.ContainsCode(hiddenServiceCode))
				{
					regValue.Add(hiddenServiceCode);
				}
			}
			serviceRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			var adapter = testConsol.RatingAdapter;
			var actualServices = adapter.JobServices;
			var actualHiddenServices = actualServices.Where(x => hiddenServiceCodeList.Contains((string)x.ServiceCode) && x.IsEnabled).ToList();
			var expected = new string[] {
				ChargeCodeSubGroupList.CartageDemurrageTotal,
				ChargeCodeSubGroupList.ContainerDetention,
				ChargeCodeSubGroupList.Storage,
				ChargeCodeSubGroupList.CarrierStorage };
			AssertContainsExactElementsInAnyOrder(expected, actualHiddenServices.Select(x => x.ServiceCode));
		}

		public void TestAutoRatingJobServices()
		{
			var currentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var testConsol = Factory.New<CommonConsol>();
			testConsol.JK_RL_NKLoadPort = "USLAX";
			testConsol.JK_RL_NKDischargePort = "AUSYD";
			var adapter = testConsol.RatingAdapter;

			var cont1 = testConsol.Containers.AddNew();
			cont1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont1.JC_ContainerMode = Constants.ContainerModes.FCL;
			var cont2 = testConsol.Containers.AddNew();
			cont2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont2.JC_ContainerMode = Constants.ContainerModes.FCL;

			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			var fumigation1 = cont1.Services.AddNew();
			fumigation1.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation1.ES_Completed = ZDateTime.Today;
			var fumigationContractor = Factory.NewWithValidTestData<OrgHeader>();
			fumigationContractor.OH_Code = "FC";
			fumigation1.ES_OH_Contractor = fumigationContractor.PK;
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.GetContractors().Contains(fumigationContractor));

			var fumigation2 = cont2.Services.AddNew();
			fumigation2.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation2.ES_Completed = ZDateTime.Today;
			fumigation2.ES_OH_Contractor = fumigationContractor.PK;

			fumigation1.ES_OA_Location = fumigationContractor.MainAddress.PK;
			fumigation2.ES_OA_Location = fumigationContractor.MainAddress.PK;

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "";
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "ISREY";
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.GetContractors().Contains(fumigationContractor));

			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_RL_NKDischargePort = "USLAX";
			fumigation1.ES_OA_Location = ZGuid.Empty;
			fumigation2.ES_OA_Location = ZGuid.Empty;
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigation1.ES_OA_Location = fumigationContractor.MainAddress.PK;
			fumigation2.ES_OA_Location = fumigationContractor.MainAddress.PK;

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "";
			fumigationContractor.MainAddress.OA_RN_NKCountryCode = "";
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "ISREY";
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			Assert(adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));
			Assert(!adapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchHomePort;
		}

		public void TestAutoRatingJobServices_HiddenServicesAlwaysDestinationCharges()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.ArrivalCTOStorageDays = 5;

			var services = consol.RatingAdapter.JobServices;

			AssertEquals(false, services.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage));
			AssertEquals("Container storage is always dst charge", true, services.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage));

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "CNSHA";
			services = consol.RatingAdapter.JobServices;

			AssertEquals(false, services.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage));
			AssertEquals("Container storage is always dst charge", true, services.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage));
		}

		public void TestAutoRatingTime_SpecialService()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var consol = Factory.New<CommonConsol>();
			var adapter = consol.RatingAdapter;
			var container = consol.Containers.AddNew();

			var service = container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			AssertEquals(2d, adapter.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestContainerJobServices_StandardService_DurationIsCalculatedFromServiceTime()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var consol = Factory.New<CommonConsol>();
			var adapter = consol.RatingAdapter;
			var container = consol.Containers.AddNew();

			var service = container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			var services = adapter.JobServices.FindServices(chargeCode);
			AssertEquals(1, services.Count());
			AssertEquals(2d, services.FirstOrDefault().ServiceDuration.TotalHours);
		}

		public void TestAutoRatingNamedAccount()
		{
			var consol = Factory.New<CommonConsol>();

			var contractNamedAccount = consol.Numbers.AddNew();
			contractNamedAccount.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			contractNamedAccount.CE_EntryNum = "Contract Named Account";

			AssertEquals("Contract Named Account", consol.RatingAdapter.NamedAccount);
		}
	}
}
