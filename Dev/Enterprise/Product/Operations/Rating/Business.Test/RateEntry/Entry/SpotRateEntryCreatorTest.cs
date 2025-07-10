using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class SpotRateEntryCreatorTest : RatingTestCase
	{
		#region CreateFreightSpotRate

		public void TestCreateFreightSpotRateEntry_Shipment_CourierMode_ChargeableUnitKG()
			=> TestCreateFreightSpotRateEntry_Shipment_CourierMode(chargeableUnit: QuantityUnit.KG, expectedRateLineUnit: QuantityUnit.KG);

		public void TestCreateFreightSpotRateEntry_Shipment_CourierMode_ChargeableUnitM3()
			=> TestCreateFreightSpotRateEntry_Shipment_CourierMode(chargeableUnit: QuantityUnit.M3, expectedRateLineUnit: QuantityUnit.M3);

		void TestCreateFreightSpotRateEntry_Shipment_CourierMode(string chargeableUnit, string expectedRateLineUnit)
		{
			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 10m, chargeableUnit);

			var creator = new SpotRateEntryCreator(criteria);
			var (spotRate, _) = creator.CreateFreightSpotRate(isCosting: false);
			AssertEquals
			(
				"SpotRate unit should match chargeable",
				expectedRateLineUnit,
				spotRate.ChildRateLines.Single().TL_WeightVolume
			);
		}

		public void TestCreateFreightSpotRateEntry_Shipment_CourierMode_NoChargeableMeasureType()
		{
			var criteria = new TestRatingCriteria();
			var creator = new SpotRateEntryCreator(criteria);
			var (spotRate, _) = creator.CreateFreightSpotRate(isCosting: false);

			AssertEquals("Precondition: no Chargeable Measure Type", false, criteria.JobMeasures.ContainsKey(MeasureType.Chargeable));

			AssertEquals
			(
				"GIVEN no MeasureType.Chargeable WHEN CreateFreightSpotRate THEN Unit should be Env.Registry.FreightVolumeUnit",
				QuantityUnit.M3,
				spotRate.ChildRateLines.Single().TL_WeightVolume
			);
		}

		public void TestCreateFreightSpotRateEntry_Shipment()
		{
			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), null, 20m, JobServiceInfo.Constants.Codes.Day);
			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(false);
			AssertEquals("Pre-condition", 1, results.Count());

			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Shipment charges should be sell rates", !entry.IsCosting());
			AssertEquals("Expected only 1 rate line", 1, entry.ChildRateLines.Count());

			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals("DSTOR", rateLine.ChargeCode.AC_Code);
			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Destination Storage Service", rateLine.SpotRateDescription);
			AssertEquals("Should have gotten cost from service unit", 20m, rateLine.GetCalculator<UnitCalculator>().PerUnit);
			AssertEquals("Should calculate unit from service unit", QuantityUnit.DY, rateLine.TL_WeightVolume);
		}

		public void TestCreateFreightSpotRateEntry_IsContractorCreditor()
		{
			var creditor = Helper.NewOrgHeader();
			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), creditor, 20m, JobServiceInfo.Constants.Codes.Day)
			{
				IsContractorCreditor = true
			};

			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);
			var results = creator.GetAllJobServiceRates(false);
			AssertEquals("Pre-condition", 1, results.Count());

			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Should be sell rate", !entry.IsCosting());
			AssertEquals(creditor.PK, entry.ParentRatingHeader.TH_OH);
		}

		public void TestCreateFreightSpotRateEntry_Containers()
		{
			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Hour);
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());

			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Consol container charges should be cost rates", entry.IsCosting());
			AssertEquals("Expected only 1 rate line", 1, entry.ChildRateLines.Count());

			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals("DSTOR", rateLine.ChargeCode.AC_Code);
			AssertEquals("Destination Storage Service", rateLine.SpotRateDescription);
			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Should have gotten cost from service unit", 20m, rateLine.GetCalculator<UnitCalculator>().PerUnit);
			AssertEquals("Should calculate unit from service unit", QuantityUnit.HR, rateLine.TL_WeightVolume);
		}

		public void TestCreateAdHocJobServiceCharge()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWDSTSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var service = Factory.New<JobService>();
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.SteamCleaning;
			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.ES_ServiceRate = 10m;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Hour;
			service.ES_References = "STEAM001101";

			var rateLine = GetSpotRateLineFromService(service);
			AssertNotNull(rateLine);
			AssertEquals("Charge code should always be the corresponding Ad-Hoc Service Charge", expectedChargeCode.PK, rateLine.TL_AC);
			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(QuantityUnit.HR, rateLine.TL_WeightVolume);
			AssertEquals("Should get rate from service", 10m, rateLine.Calculator[Calculator.Items.Operator.UNT]);

			var entry = rateLine.ParentRateEntry;
			Assert(entry.IsSpotEntry);
			Assert("Should be sell rate as there is no contractor", !entry.IsCosting());
			AssertEquals("STEAM001101", entry.TI_ContractNumber);
		}

		public void TestCreateAdHocJobServiceCost()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWDSTSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var service = Factory.New<JobService>();
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.SteamCleaning;
			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.ES_ServiceRate = 10m;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Hour;
			service.ES_References = "SPE209735";
			service.ES_OH_Contractor = Helper.NewOrgHeader().PK;

			var jobServiceInfo = new JobServiceInfo(service);
			jobServiceInfo.ChargeCodeGroup = ChargeCodeGroupList.Codes.Destination;

			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var results = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());

			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Should be cost as there is a contractor", entry.IsCosting());
			AssertEquals("SPE209735", entry.TI_ContractNumber);

			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals("Charge code should always be the corresponding Ad-Hoc Service Charge", expectedChargeCode.PK, rateLine.TL_AC);
			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(QuantityUnit.HR, rateLine.TL_WeightVolume);
			AssertEquals("Steam Cleaning Service", rateLine.SpotRateDescription);
			AssertEquals("Should get rate from service", 10m, rateLine.Calculator[Calculator.Items.Operator.UNT]);
		}

		public void TestCreateAdHocJobServiceChargeFromFlat()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWORGSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var hasJobServicesParent = Factory.NewWithValidTestData<DummyWithServices>();
			var service = Helper.CreateAdHocJobService(hasJobServicesParent, Constants.FreightServiceType.Codes.SteamCleaning, 100m, JobServiceInfo.Constants.Codes.FlatRate);

			var jobServiceInfo = new JobServiceInfo(service);
			jobServiceInfo.ChargeCodeGroup = ChargeCodeGroupList.Codes.Origin;

			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var entry = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(false).FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Should be sell rate as there is no contractor", !entry.IsCosting());

			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals("Charge code should always be the corresponding Ad-Hoc Service Charge", expectedChargeCode.PK, rateLine.TL_AC);
			AssertEquals(FlatCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(100m, rateLine.Calculator[Calculator.Items.Operator.BAS]);
			AssertEquals("Steam Cleaning Service", rateLine.SpotRateDescription);
		}

		public void TestCreateFreightSpotRateEntry_PrefersChargeCodeWithAdHocServiceChargeToRegistry()
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_IsAdhocServiceCharge, true);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Destination);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeSubGroup, ChargeCodeSubGroupList.Storage);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);

			var result = Factory.LoadTop1<AccChargeCode>(query);
			AssertNull("Pre-condition", result);

			var expectedChargeCode = Helper.ChargeCodes["NEWSTOR"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Destination Storage", 1m, TimeSpan.FromDays(5), null, 20m, JobServiceInfo.Constants.Codes.Day);
			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(false);
			AssertEquals("Pre-condition", 1, results.Count());

			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			Assert("Shipment charges should be sell rates", !entry.IsCosting());
			AssertEquals("Expected only 1 rate line", 1, entry.ChildRateLines.Count());

			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals(expectedChargeCode.AC_Code, rateLine.ChargeCode.AC_Code);
			AssertEquals(expectedChargeCode.PK, rateLine.TL_AC);
			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Should calculate unit from service unit", QuantityUnit.DY, rateLine.TL_WeightVolume);
			AssertEquals("Should have gotten cost from service unit", 20m, rateLine.Calculator[Calculator.Items.Operator.UNT]);
		}

		public void TestCreateAdHocJobServiceCharge_UnitsMatchRatingUnits()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWDSTSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var parent = Factory.NewWithValidTestData<DummyWithServices>();
			var service = Helper.CreateAdHocJobService(parent, Constants.FreightServiceType.Codes.SteamCleaning, 10m, JobServiceInfo.Constants.Codes.Hour);

			var rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.HR, rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);

			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.ServiceOccurrence;
			rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.SV, rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);

			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.PickUpDistance;
			rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.KM, rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);

			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.DeliveryDistance;
			rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.MI, rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);

			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Chargeable;
			rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.KG, rateLine.TL_WeightVolume);
			AssertEquals("Only chargeable should use the chargeable rounding type", RatingRoundingTypes.Chargeable, rateLine.TL_Rounding);

			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Container;
			rateLine = GetSpotRateLineFromService(service);

			AssertEquals(QuantityUnit.CN, rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);
		}

		#endregion

		public void TestAdHocJobServiceCharge_FlatRateCreatesFlatCalculator()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWDSTSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var parent = Factory.NewWithValidTestData<DummyWithServices>();
			var service = Helper.CreateAdHocJobService(parent, Constants.FreightServiceType.Codes.SteamCleaning, 101m, JobServiceInfo.Constants.Codes.FlatRate);

			var rateLine = GetSpotRateLineFromService(service);

			AssertNotNull(rateLine);
			AssertEquals(FlatCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Flat calculator sholdn't have a unit", "", rateLine.TL_WeightVolume);
			AssertEquals(RatingRoundingTypes.DefaultFromRegistry, rateLine.TL_Rounding);
			AssertEquals("Base Rate should match service rate", 101m, rateLine.Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestGetAllJobServiceRates_ReturnRateEntryWithTruncatedServiceReferenceInContractNumber_WhenExceedsContractNumberMaxLength()
		{
			var expectedChargeCode = Helper.ChargeCodes["NEWDSTSTE"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.SteamCleaning;
			expectedChargeCode.AC_IsAdhocServiceCharge = true;
			expectedChargeCode.Factory.Save();

			var service = Factory.New<JobService>();
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.SteamCleaning;
			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.ES_ServiceRate = 10m;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Hour;
			service.ES_OH_Contractor = Helper.NewOrgHeader().PK;

			var jobServiceInfo = new JobServiceInfo(service)
			{
				ChargeCodeGroup = ChargeCodeGroupList.Codes.Destination,
				ServiceReference = "REF12345678901234567890123456789012345678901234567890"
			};

			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);

			var rateEntries = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(true);
			AssertEquals(1, rateEntries.Count());

			var rateEntry = rateEntries.ToArray()[0];
			AssertEquals("REF12345678901234567890123456789012345678901234567", rateEntry.TI_ContractNumber);
		}

		public void TestGetAllJobServiceRates_WhenTotalCostNotZeroAndRateIsZero()
		{
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.ContainerDetention;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.OriginDetentionChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention, "Xyzzy", 3m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 0m, JobServiceInfo.Constants.Codes.Day, true, 95m, "NZD");
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("rate should be created", 1, results.Count());
			var entry = results.Single();
			var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;
			AssertNotNull(rateLine);
			AssertEquals("Charge code", expectedChargeCode.PK, rateLine.TL_AC);
			AssertEquals(FlatCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(95m, rateLine.Calculator[Calculator.Items.Operator.BAS]);
			AssertEquals("Xyzzy Service", rateLine.SpotRateDescription);
		}

		public void TestGetAllJobServiceRates_WhenTotalCostIsZeroAndRateIsNotZero_ShouldWaiveOffCharge_UseTotalCostAndIgnoreRate()
		{
			var chargeCodeGroup = ChargeCodeGroupList.Codes.Origin;
			var chargeSubGroup = ChargeCodeSubGroupList.ContainerDetention;
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = chargeCodeGroup;
			expectedChargeCode.AC_ChargeSubGroup = chargeSubGroup;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.OriginDetentionChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			AssertGetAllJobServiceRates(
				perUnitRate: 3m,
				totalOverrideRate: 0m, //To fully waive off
				useTotalCostAndIgnoreRate: true,
				isChargeExpected: true,
				expectedCalculator: FlatCalculator.Code,
				expectedRate: 0m);

			AssertGetAllJobServiceRates(
				perUnitRate: 3m,
				totalOverrideRate: 0m,
				useTotalCostAndIgnoreRate: false,
				isChargeExpected: true,
				expectedCalculator: UnitCalculator.Code,
				expectedRate: 3m);

			void AssertGetAllJobServiceRates(
				decimal perUnitRate,
				decimal totalOverrideRate,
				bool useTotalCostAndIgnoreRate,
				bool isChargeExpected,
				string expectedCalculator = default,
				decimal expectedRate = default)
			{
				var jobServiceInfo = new JobServiceInfo(true, chargeCodeGroup, chargeSubGroup, "Xyzzy",
					rate: perUnitRate,
					unit: JobServiceInfo.Constants.Codes.Day,
					totalCost: totalOverrideRate,
					useTotalCostAndIgnoreRate: useTotalCostAndIgnoreRate);
				var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
				criteria.ConsumerType = JobInvoicingConsumerTypes.Shipment;
				criteria.JobServices.Add(jobServiceInfo);

				var creator = new SpotRateEntryCreator(criteria);
				var results = creator.GetAllJobServiceRates(false);

				if (isChargeExpected)
				{
					AssertEquals("rate should be created", 1, results.Count());

					var entry = results.Single();
					var rateLine = entry.ChildRateLines.FirstOrDefault() as RateLine;

					AssertNotNull(rateLine);
					AssertEquals("Charge code", expectedChargeCode.PK, rateLine.TL_AC);
					AssertEquals("Xyzzy Service", rateLine.SpotRateDescription);
					AssertEquals(expectedCalculator, rateLine.TL_RateCalculator);

					if (expectedCalculator == FlatCalculator.Code)
					{
						AssertEquals(expectedRate, rateLine.Calculator[Calculator.Items.Operator.BAS]);
					}
					else if (expectedCalculator == UnitCalculator.Code)
					{
						AssertEquals(expectedRate, rateLine.Calculator[Calculator.Items.Operator.UNT]);
					}
				}
			}
		}

		public void TestGetAllJobServiceRates_ChargeCodeFromRegistryForOriginDetention()
		{
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.ContainerDetention;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.OriginDetentionChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention, "The Service", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Hour);
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());
			AssertEquals(expectedChargeCode.PK, results.First().ChildRateLines.Single().TL_AC);
		}

		public void TestGetAllJobServiceRates_ChargeCodeFromRegistryForOriginStorage()
		{
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.OriginStorageChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage, "The Service", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Hour);
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());
			AssertEquals(expectedChargeCode.PK, results.First().ChildRateLines.Single().TL_AC);
		}

		public void TestGetAllJobServiceRates_ChargeCodeFromRegistryForOriginCarrierStorage()
		{
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CarrierStorage;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.OriginCarrierStorageChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CarrierStorage, "The Service", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Hour);
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());
			AssertEquals(expectedChargeCode.PK, results.First().ChildRateLines.Single().TL_AC);
		}

		public void TestGetAllJobServiceRates_ChargeCodeFromRegistryForDestinationCarrierStorage()
		{
			var expectedChargeCode = Helper.ChargeCodes["CHARGE1"];
			expectedChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			expectedChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CarrierStorage;
			expectedChargeCode.AC_IsAdhocServiceCharge = false;
			expectedChargeCode.Factory.Save();

			RatingDataRegistry.Instance.DestinationCarrierStorageChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, expectedChargeCode.PK.ToGuid());

			var jobServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage, "The Service", 1m, TimeSpan.FromDays(5), Helper.NewOrgHeader(), 20m, JobServiceInfo.Constants.Codes.Hour);
			jobServiceInfo.IsCostForSpotRate = true;
			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 5, GP20, NewClient2);
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.JobServices.Add(jobServiceInfo);

			var creator = new SpotRateEntryCreator(criteria);

			var results = creator.GetAllJobServiceRates(true);
			AssertEquals("Pre-condition", 1, results.Count());
			AssertEquals(expectedChargeCode.PK, results.First().ChildRateLines.Single().TL_AC);
		}

		static IRateLine GetSpotRateLineFromService(JobService service, bool isCosting = false)
		{
			var jobServiceInfo = new JobServiceInfo(service);
			if (jobServiceInfo.ChargeCodeGroup == "")
			{
				jobServiceInfo.ChargeCodeGroup = ChargeCodeGroupList.Codes.Destination;
			}

			var criteria = new TestRatingCriteria();
			criteria.JobServices.Add(jobServiceInfo);
			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 88.88m, QuantityUnit.KG);
			criteria.RateableMeasures.SetQuantity(MeasureType.PickupDistance, 7.5m, QuantityUnit.KM);
			criteria.RateableMeasures.SetQuantity(MeasureType.DeliveryDistance, 1m, QuantityUnit.MI);
			criteria.RateableMeasures.SetQuantity(MeasureType.ContainerCount, 2m, QuantityUnit.CN);

			var creator = new SpotRateEntryCreator(criteria);
			var results = creator.GetAllJobServiceRates(isCosting);
			var entry = results.FirstOrDefault();
			Assert(entry.IsSpotEntry);
			AssertEquals(1, entry.ChildRateLines.Count());

			return entry.ChildRateLines.FirstOrDefault();
		}
	}
}
