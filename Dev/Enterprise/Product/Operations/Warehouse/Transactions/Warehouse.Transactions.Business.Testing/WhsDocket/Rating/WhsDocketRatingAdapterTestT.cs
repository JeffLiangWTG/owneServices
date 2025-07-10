using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketRatingAdapterTest<T> : WhsTestCaseWithFactory
			where T : WhsDocket
	{
		#region IAutoRating Members

		#region TestIAutoRatingJobServices

		public void TestIAutoRatingJobServices()
		{
			var collection = (SystemDefinableCodeDescriptionBoolCollection)WarehouseDataRegistry.Instance.JobServices.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var element = collection.AddNew();
			element.Code = ChargeCodeSubGroupList.Storage;

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var docket = GetNewDocket();
				var iDocket = GetIAutoRating(docket);
				var usedChargeCodeGroups = (string[])iDocket.GetType().GetProperty(
					"UsedChargeCodeGroups", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(iDocket, null);

				var servicesWrapper = iDocket.JobServices;
				var allServiceTypes = docket.Services.AddNew().Lookups.JobServiceType_List;

				Assert("precondition", allServiceTypes.ContainsCode(ChargeCodeSubGroupList.Storage));

				foreach (CodeDescriptionPair serviceType in allServiceTypes.ToArray().Where(x => x.Code != ChargeCodeSubGroupList.Storage))
				{
					foreach (var chargeCodeGroup in usedChargeCodeGroups)
					{
						AssertEquals(true, servicesWrapper.Contains(chargeCodeGroup, serviceType.Code));
					}
				}

				AssertNotEquals("List should not be cached.", iDocket.JobServices, servicesWrapper);
				Assert("List should not contain STG.", servicesWrapper.All(x => x.ServiceCode != ChargeCodeSubGroupList.Storage));
				AssertEquals((allServiceTypes.Count - 1) * usedChargeCodeGroups.Length, servicesWrapper.Count);
			}
		}

		#endregion

		#region TestIAutoRatingChargeCodeGroups

		public void TestIAutoRatingChargeCodeGroups()
		{
			var autoRating = GetIAutoRating(GetNewDocket());

			var ccList = autoRating.ChargeCodeGroups;
			AssertNotNull(ccList);

			// assert cached
			AssertEquals(ccList, autoRating.ChargeCodeGroups);
		}

		#endregion

		#region TestIAutoRatingPopulateChargeCodeGroups

		public abstract void TestIAutoRatingPopulateChargeCodeGroups();

		#endregion

		#region TestIAutoRatingRateTypeToUse

		public void TestIAutoRatingRateTypeToUse()
		{
			var autoRating = GetIAutoRating(GetNewDocket());
			AssertEquals(RateType.Warehouse, autoRating.RateTypeToUse);
		}

		#endregion

		#region TestIAutoRatingStatusInformation

		public void TestIAutoRatingStatusInformation()
		{
			var autoRating = GetIAutoRating(GetNewDocket());
			var info = autoRating.StatusInformation;

			AssertNotNull(info);
			AssertEquals(true, info.CanExecute);
			AssertEquals(ZString.Empty, info.Message);
		}

		#endregion

		#region TestIAutoRatingConsumerType

		public void TestIAutoRatingConsumerType()
		{
			TestIAutoRatingConsumerTypeCore();
		}

		protected abstract void TestIAutoRatingConsumerTypeCore();

		#endregion

		#region TestIAutoRatingMergeCharges

		public void TestIAutoRatingMergeCharges()
		{
			var autoRating = GetIAutoRating(GetNewDocket());
			AssertEquals(MergeChargeOptions.WithinAdapter, autoRating.MergeCharges);
		}

		#endregion

		#region TestIAutoRatingAdapterTypeAndID

		public abstract void TestIAutoRatingAdapterTypeAndID();

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingFreightInfo_FreightMode

		public void TestIAutoRatingFreightInfo_FreightMode()
		{
			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(GetNewDocket());
			AssertEquals(FreightMode.UKN, autoRating.FreightMode);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_HousebillReleaseType

		public void TestIAutoRatingFreightInfo_HousebillReleaseType()
		{
			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(GetNewDocket());
			AssertEquals(ZString.Empty, autoRating.HousebillReleaseType);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Time

		public void TestIAutoRatingFreightInfo_Time()
		{
			var autoRating = GetIAutoRating(GetNewDocket());
			var timeInfo = ((RateableMeasureSet)autoRating.RateableMeasures).Time;
			AssertEquals(autoRating.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate).Day, timeInfo.Span.Days);
			AssertEquals(0, timeInfo.Span.Minutes);
			AssertEquals(0, timeInfo.Span.Hours);
		}

		public void TestIAutoRatingFreightInfo_SpecialServiceTime()
		{
			var docket = GetNewDocket();
			var autoRating = GetIAutoRating(docket);

			var service = docket.Services.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			var chargeCodeFilter = new ZQuery(
				new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
				new ZQuery(AccChargeCodeSchema.AC_Code, "DFUMI"));

			var chargeCode = Factory.Load<AccChargeCode>(chargeCodeFilter).FirstOrDefault();
			chargeCode.AC_ChargeGroup = autoRating.ChargeCodeGroups[0];

			AssertEquals(2d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_MonetaryValues

		public void TestIAutoRatingFreightInfo_MonetaryValues()
		{
			var docket = GetNewDocket();
			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(docket);
			docket.Lines.AddNew().WE_RX_NKUnitPriceCurrency = "AUD";
			docket.WD_TotalOrderValue = 10;
			AssertEquals(false, autoRating.MonetaryValues.Values.ContainsKey(MoneyType.ValueType.InsuranceValue));

			List<Money> orderValues;
			autoRating.MonetaryValues.Values.TryGetValue(MoneyType.ValueType.GoodsValue, out orderValues);
			AssertEquals(1, orderValues.Count);
			AssertEquals(10m, orderValues[0].Amount);
			AssertEquals("AUD", orderValues[0].Currency.Code);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures

		#region TestIAutoRatingFreightInfo_Measures

		public void TestIAutoRatingFreightInfo_Measures()
		{
			TestIAutoRatingFreightInfo_MeasuresCore();
		}

		public void TestIAutoRatingFreightInfo_Measures_WhenWE_OPNotSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Carton;

			Helper.SetProductWeightAndVolume(data.Part1, 1m, "LB", 1m, "L");
			Helper.SetProductWeightAndVolume(data.Part2, 10m, "KG", 0.01m, "M3");
			data.Part1.OP_RH_NKCommodityCode = "HAZ";
			data.Part2.OP_RH_NKCommodityCode = "HAZ";

			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Carton, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			var line3 = docket.Lines.AddNew();

			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;
			line3.WE_OP = data.Part1.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();
			line3.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 10m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 20m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line3, 30m);

			line2.WE_OP = ZGuid.Empty;
			line3.WE_OP = ZGuid.Empty;

			docket.WD_TotalWeightUnit = "T";
			docket.WD_TotalWeight = 0.4m;
			docket.WD_TotalCubicUnit = "L";
			docket.WD_TotalCubic = 300m;
			docket[AutoratingChargeablePalletsColumn.Name] = (ZShort)111;

			docket.WD_PackagesSent = (ZInt)3m;
			docket.WD_TotalUnits = 6m;
			docket.WD_UnitsSent = 7m;
			SetFinalisedDocketFields(docket, ZDateTime.Now);

			bool isWhsOrder = docket is WhsOrder;
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;

			foreach (MeasureType measure in Enum.GetValues(typeof(MeasureType)))
			{
				if (measure != MeasureType.ContainerCount &&
					measure != MeasureType.Unidentified &&
					measure != MeasureType.Time &&
					measure != MeasureType.WarehousePackage &&
					measure != MeasureType.WarehousePackageVolume &&
					measure != MeasureType.WarehousePackageWeight &&
					measure != MeasureType.BOMKit &&
					rateableMeasures.HasMeasureType(measure))
				{
					Assert(rateableMeasures.MeasureHasDocketReference(measure));
					Assert(rateableMeasures.MeasureHasWarehouse(measure));

					if (measure == MeasureType.Line ||
						measure == MeasureType.ChargeablePallet ||
						measure == MeasureType.PalletID ||
						measure == MeasureType.JobUnit ||
						measure == MeasureType.Package ||
						measure == MeasureType.JobWeight ||
						measure == MeasureType.JobVolume ||
						measure == MeasureType.Shipment)
					{
						Assert(!rateableMeasures.MeasureHasProduct(measure));
						Assert(!rateableMeasures.MeasureHasProductAttributes(measure));
						Assert(!rateableMeasures.MeasureHasCommodity(measure));
					}
					else
					{
						Assert(rateableMeasures.MeasureHasProduct(measure));
						Assert(rateableMeasures.MeasureHasProductAttributes(measure));
						Assert(rateableMeasures.MeasureHasCommodity(measure));
					}
				}
			}

			AssertEquals("Line1=10 rest are in error", 10m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals("10 Units", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("0 Cartons", 0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part2.PK));

			AssertEquals(0m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.Unit, "GEN"));

			AssertEquals(4.54m, Utilities.Round(rateableMeasures.GetActual(MeasureType.Weight), 2));
			AssertEquals("KG", rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(0.010m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("M3", rateableMeasures.GetUnit(MeasureType.Volume));

			if (isWhsOrder)
			{
				AssertEquals(0.00m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobWeight), 2));
				AssertEquals("T", rateableMeasures.GetUnit(MeasureType.JobWeight));
				AssertEquals(10m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobVolume), 2));
				AssertEquals("L", rateableMeasures.GetUnit(MeasureType.JobVolume));
				AssertEquals(7m, rateableMeasures.GetActual(MeasureType.JobUnit));
			}
			else
			{
				AssertEquals(0.4m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobWeight), 2));
				AssertEquals("T", rateableMeasures.GetUnit(MeasureType.JobWeight));
				AssertEquals(300m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobVolume), 2));
				AssertEquals("L", rateableMeasures.GetUnit(MeasureType.JobVolume));
				AssertEquals(6m, rateableMeasures.GetActual(MeasureType.JobUnit));
			}

			AssertEquals(111m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.ChargeablePallet));
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.Shipment));
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals(docket is WhsReceive, rateableMeasures.HasMeasureType(MeasureType.PalletID));
		}

		protected virtual void TestIAutoRatingFreightInfo_MeasuresCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Carton;

			Helper.SetProductWeightAndVolume(data.Part1, 1m, "LB", 1m, "L");
			Helper.SetProductWeightAndVolume(data.Part2, 10m, "KG", 0.01m, "M3");
			data.Part1.OP_RH_NKCommodityCode = "HAZ";
			data.Part2.OP_RH_NKCommodityCode = "HAZ";

			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Carton, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			var line3 = docket.Lines.AddNew();

			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;
			line3.WE_OP = data.Part1.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();
			line3.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 10m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 20m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line3, 30m);

			docket.WD_TotalWeightUnit = "T";
			docket.WD_TotalWeight = 0.4m;
			docket.WD_TotalCubicUnit = "L";
			docket.WD_TotalCubic = 300m;
			docket[AutoratingChargeablePalletsColumn.Name] = (ZShort)111;

			docket.WD_PackagesSent = (ZInt)3m;
			docket.WD_TotalUnits = 6m;
			docket.WD_UnitsSent = 7m;
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			var isWhsOrder = docket is WhsOrder;
			if (!isWhsOrder)
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			}

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			foreach (MeasureType measure in Enum.GetValues(typeof(MeasureType)))
			{
				if (measure != MeasureType.ContainerCount &&
					measure != MeasureType.Unidentified &&
					measure != MeasureType.Time &&
					measure != MeasureType.WarehousePackage &&
					measure != MeasureType.WarehousePackageVolume &&
					measure != MeasureType.WarehousePackageWeight &&
					measure != MeasureType.BOMKit &&
					rateableMeasures.HasMeasureType(measure))
				{
					Assert(rateableMeasures.MeasureHasDocketReference(measure));
					Assert(rateableMeasures.MeasureHasWarehouse(measure));

					if (measure == MeasureType.Line ||
						measure == MeasureType.ChargeablePallet ||
						measure == MeasureType.PalletID ||
						measure == MeasureType.JobUnit ||
						measure == MeasureType.Package ||
						measure == MeasureType.JobWeight ||
						measure == MeasureType.JobVolume ||
						measure == MeasureType.Shipment)
					{
						Assert(!rateableMeasures.MeasureHasProduct(measure));
						Assert(!rateableMeasures.MeasureHasProductAttributes(measure));
						Assert(!rateableMeasures.MeasureHasCommodity(measure));
					}
					else
					{
						Assert(rateableMeasures.MeasureHasProduct(measure));
						Assert(rateableMeasures.MeasureHasProductAttributes(measure));
						Assert(rateableMeasures.MeasureHasCommodity(measure));
					}
				}
			}

			AssertEquals("Without measure for services, the Invoice Detail report will not group the charges correctly.", "SV", rateableMeasures.GetUnit(MeasureType.Unidentified));
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.Line));
			AssertEquals("Line1=10 + Line3=30 + Line2=20", 60m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals("40 Units", 40m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("20 Cartons", 20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part2.PK));

			AssertEquals(60m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.Unit, "HAZ"));
			AssertEquals(0m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.Unit, "GEN"));

			AssertEquals(218.14m, Utilities.Round(rateableMeasures.GetActual(MeasureType.Weight), 2));
			AssertEquals("KG", rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(0.24m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("M3", rateableMeasures.GetUnit(MeasureType.Volume));

			if (isWhsOrder)
			{
				AssertEquals(0.22m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobWeight), 2));
				AssertEquals("T", rateableMeasures.GetUnit(MeasureType.JobWeight));
				AssertEquals(240m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobVolume), 2));
				AssertEquals("L", rateableMeasures.GetUnit(MeasureType.JobVolume));
				AssertEquals(7m, rateableMeasures.GetActual(MeasureType.JobUnit));
			}
			else
			{
				AssertEquals(0.4m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobWeight), 2));
				AssertEquals("T", rateableMeasures.GetUnit(MeasureType.JobWeight));
				AssertEquals(300m, Utilities.Round(rateableMeasures.GetActual(MeasureType.JobVolume), 2));
				AssertEquals("L", rateableMeasures.GetUnit(MeasureType.JobVolume));
				AssertEquals(6m, rateableMeasures.GetActual(MeasureType.JobUnit));
			}

			AssertEquals(111m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
			AssertEquals("", rateableMeasures.GetUnit(MeasureType.ChargeablePallet));
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.Shipment));
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.Package));
		}

		protected virtual SchemaColumn AutoratingChargeablePalletsColumn
		{
			get { return WhsDocketSchema.WD_TotalPallets; }
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_NoExceptions

		[ExpectNoExceptions]
		public void TestIAutoRatingFreightInfo_Measures_NoExceptions()
		{
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(GetNewDocket()).RateableMeasures;
			foreach (MeasureType measure in Enum.GetValues(typeof(MeasureType)))
			{
				if (rateableMeasures.HasMeasureType(measure))
				{
					var poke = rateableMeasures.GetActual(measure);
				}
			}
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling()
		{
			TestIAutoRatingFreightInfo_Measures_SplitPeriodBillingCore();
		}

		protected virtual void TestIAutoRatingFreightInfo_Measures_SplitPeriodBillingCore()
		{
			var year = ZDateTime.Now.Year;

			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A");
			var docket = GetNewDocket();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var prod1 = Helper.CreateProduct(org, "AAAA");
			var prod2 = Helper.CreateProduct(org, "BBBB");

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			var line3 = docket.Lines.AddNew();
			line1.WE_OP = prod1.PK;
			line2.WE_OP = prod2.PK;
			line3.WE_OP = prod1.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();
			line3.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 10m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 20m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line3, 30m);

			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);

			var autoRating = GetIAutoRating(docket);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			Assert(rateableMeasures.MeasureHasDocketReference(MeasureType.Unit));
			AssertEquals(40m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, prod1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, prod2.PK));

			org.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			docket.WD_ExternalReference = "EXTREF#1";
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("EXTREF#1", rateableMeasures.GetUniqueUnitDockets_ForTest().Single());
			AssertEquals(40m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, prod1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, prod2.PK));

			docket.WD_ExternalReference = "";
			docket.WD_DocketID = "D00001";
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("D00001", rateableMeasures.GetUniqueUnitDockets_ForTest().Single());
		}

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var helper = GetFinalisableHelper();
			var docket = helper.GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>(ExpectedDbHitsForSplitPeriodBillingMeasuresCore)
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			var newFactory = new BusinessObjectFactory();
			var docketInNewFactory = newFactory.Load<WhsDocket>(docket.PK);
			var autoRating = GetIAutoRating(docketInNewFactory);
			var measures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(10m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
			AssertDbHits(expectedDbHits, newFactory);
		}

		protected virtual Dictionary<string, int> ExpectedDbHitsForSplitPeriodBillingMeasuresCore => new Dictionary<string, int>();

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_FavoursInMemoryChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var helper = GetFinalisableHelper();
			var docket = helper.GetNewFinalisableDocketWithOneLine(data, 10m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var docketInNewFactory = newFactory.Load<WhsDocket>(docket.PK);
			docketInNewFactory.Lines[0].WE_TransactionQuantity = 9m;

			var pickLine = docketInNewFactory.Lines[0].PickLines.SingleOrDefault();
			if (pickLine != null)
			{
				pickLine.WZ_Units = 9m;
			}

			docketInNewFactory.FinaliseDocketWithoutUserConfirmation();
			docketInNewFactory.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			AssertIsFinalisedPrecondition(docketInNewFactory);

			var autoRating = GetIAutoRating(docketInNewFactory);
			var measures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(9m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
		}

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_ConsidersNewLinesNotInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var helper = GetFinalisableHelper();
			var docket = helper.GetNewFinalisableDocketWithOneLine(data, 10m, "D1");
			helper.GetNewFinalisableDocketWithOneLine(data, 5m, "D2").Lines[0].PickLines.DeleteAll();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var docketInNewFactory = newFactory.Load<WhsDocket>(docket.PK);
			var newLine = (WhsDocketLine)docketInNewFactory.Lines[0].Clone();
			newLine.WE_TransactionQuantity = 5m;
			docketInNewFactory.Lines.Add(newLine);

			var pickLine = docketInNewFactory.Lines[0].PickLines.SingleOrDefault();
			if (pickLine != null)
			{
				pickLine.Pick.AutoAllocateItemsWithMock();
			}

			docketInNewFactory.FinaliseDocketWithoutUserConfirmation();
			docketInNewFactory.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			AssertIsFinalisedPrecondition(docketInNewFactory);

			var autoRating = GetIAutoRating(docketInNewFactory);
			var measures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(15m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
		}

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_IsCachedDuringAutoRating_SingleDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var helper = GetFinalisableHelper();
			var docket = helper.GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var docketInNewFactory = newFactory.Load<WhsDocket>(docket.PK);
			var autoRating = GetIAutoRating(docketInNewFactory);

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(docketInNewFactory))
			using (TestConnection.TrackExecutedCommands())
			{
				for (int index = 0; index < 3; index++)
				{
					var measures = (RateableMeasureSet)autoRating.RateableMeasures;
					AssertEquals(10m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
				}

				AssertEquals("Should have only hit the DB once, even though we calculate multiple times.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("@OrderPKs") && c.Contains("@NonOrderPKs")));
			}
		}

		public void TestIAutoRatingFreightInfo_Measures_SplitPeriodBilling_IsCachedDuringAutoRating_MultipleDockets()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var helper = GetFinalisableHelper();
			var docket1 = helper.GetNewFinalisableDocketWithOneLine(data, 10m, "D1", finalise: true);
			docket1.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			Factory.Save();

			var docket2 = helper.GetNewFinalisableDocketWithOneLine(data, 10m, "D2", finalise: true);
			docket2.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(2);
			Factory.Save();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var docketInNewFactory1 = newFactory.Load<WhsDocket>(docket1.PK);
			var docketInNewFactory2 = newFactory.Load<WhsDocket>(docket2.PK);
			var autoRating1 = GetIAutoRating(docketInNewFactory1);
			var autoRating2 = GetIAutoRating(docketInNewFactory1);

			var invoice = (BusinessObject)newFactory.New<IWhsInvoice>();
			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(invoice))
			using (TestConnection.TrackExecutedCommands())
			{
				// pretend we are doing Periodic Invoice Autorating
				RatingCache.LocalSession.GetCachedValue("WhsInvoice|AutoRating|Dockets|" + invoice.PK, () => new WhsDocket[] { docketInNewFactory1, docketInNewFactory2 });

				for (int index = 0; index < 3; index++)
				{
					var measures1 = (RateableMeasureSet)autoRating1.RateableMeasures;
					var measures2 = (RateableMeasureSet)autoRating2.RateableMeasures;
					AssertEquals(10m, measures1.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
					AssertEquals(10m, measures2.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
				}

				AssertEquals("Should have only hit the DB once, even though we calculate multiple times and have multiple Dockets.", 1,
				TestConnection.ExecutedCommands.Count(c => c.Contains("@OrderPKs") && c.Contains("@NonOrderPKs")));
			}
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_Volume

		public void TestIAutoRatingFreightInfo_Measures_Volume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			data.Part2.OP_StockKeepingUnit = Constants.Volume.CubicCentimeters;

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 5000m);
			SetFinalisedDocketFields(docket, ZDateTime.Now);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5.005m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_Weight

		public void TestIAutoRatingFreightInfo_Measures_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			data.Part2.OP_StockKeepingUnit = Constants.Weight.Grams;

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part2.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 5000m);
			SetFinalisedDocketFields(docket, ZDateTime.Now);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(10m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_UnitInvalid

		public abstract void TestIAutoRatingFreightInfo_Measures_WhenWeightUnitIsInvalid();

		public abstract void TestIAutoRatingFreightInfo_Measures_WhenVolumeUnitIsInvalid();

		public abstract void TestIAutoRatingFreightInfo_Measures_WhenMultiFieldsAreInvalid();

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WithProductAttributes

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A");
			var docket = GetNewDocket();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(docket);
			var prod1 = Helper.CreateProduct(org, "ABCDE");
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			var line3 = docket.Lines.AddNew();
			line1.WE_OP = prod1.PK;
			line2.WE_OP = prod1.PK;
			line3.WE_OP = prod1.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();
			line3.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 10m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 20m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line3, 30m);

			line1.SetAttributes(GetTestAttributes("11", "12", "13"));
			line2.SetAttributes(GetTestAttributes("21", "22"));
			line3.SetAttributes(GetTestAttributes("31"));

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			foreach (var productAttributes in rateableMeasures.GetUnitProductAttributes_ForTest())
			{
				Assert(productAttributes.IsEmpty);
			}

			org.MiscServ.OM_IMAttrib1IsKey = true;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			var i = 1;
			foreach (var productAttributes in rateableMeasures.GetUnitProductAttributes_ForTest())
			{
				var expected = new ProductAttributesMeasure((i * 10 + 1).ToString());
				Assert(!productAttributes.IsEmpty);
				Assert(productAttributes.Equals(expected));
				i++;
			}

			org.MiscServ.OM_IMAttrib2IsKey = true;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			i = 1;
			foreach (var productAttributes in rateableMeasures.GetUnitProductAttributes_ForTest())
			{
				var expected = new ProductAttributesMeasure(
					(i * 10 + 1).ToString(), i <= 2 ? (i * 10 + 2).ToString() : "");
				Assert(!productAttributes.IsEmpty);
				Assert(productAttributes.Equals(expected));
				i++;
			}

			org.MiscServ.OM_IMAttrib3IsKey = true;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			i = 1;
			foreach (var productAttributes in rateableMeasures.GetUnitProductAttributes_ForTest())
			{
				var expected = new ProductAttributesMeasure(
					(i * 10 + 1).ToString(), i <= 2 ? (i * 10 + 2).ToString() : "", i <= 1 ? (i * 10 + 3).ToString() : "");
				Assert(!productAttributes.IsEmpty);
				Assert(productAttributes.Equals(expected));
				i++;
			}
		}

		ILineAttributes GetTestAttributes(params string[] partAttributes)
		{
			TestILineAttributes result = new TestILineAttributes();
			if (partAttributes.Length > 0)
			{
				result.PartAttrib1 = partAttributes[0];
			}

			if (partAttributes.Length > 1)
			{
				result.PartAttrib2 = partAttributes[1];
			}

			if (partAttributes.Length > 2)
			{
				result.PartAttrib3 = partAttributes[2];
			}

			return result;
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKey()
		{
			TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(isFactorySaved: false);
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_FactorySaved_SerialNumberIsKey()
		{
			TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(isFactorySaved: true);
		}

		protected virtual void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(bool isFactorySaved)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A");
			var docket = GetNewDocket();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(docket);
			var prod = Helper.CreateProduct(org, "ABCDE");
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = prod.PK;
			var line2 = docket.Lines.AddNew();
			line2.WE_OP = prod.PK;
			var line3 = docket.Lines.AddNew();
			line3.WE_OP = prod.PK;

			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();
			line3.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 1m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line2, 1m);
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line3, 1m);

			line1.WE_SerialNumber = "SN1";
			line2.WE_SerialNumber = "SN2";
			line3.WE_SerialNumber = "SN3";

			if (isFactorySaved)
			{
				Factory.Save();
			}

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			var productAttributes1 = rateableMeasures.GetUnitProductAttributes_ForTest();
			Assert(productAttributes1.All(pa => pa.IsEmpty));

			org.MiscServ.OM_IMSerialNumberIsKey = true;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			var productAttributes2 = rateableMeasures.GetUnitProductAttributes_ForTest();
			Assert(productAttributes2.All(pa => !pa.IsEmpty));
			AssertContainsExactElementsInAnyOrder(
				new[] {
					new ProductAttributesMeasure(null, null, null, "SN1"),
					new ProductAttributesMeasure(null, null, null, "SN2"),
					new ProductAttributesMeasure(null, null, null, "SN3") },
				productAttributes2);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_LazyLoaded

		public void TestIAutoRatingFreightInfo_Measures_LazyLoaded()
		{
			TestIAutoRatingFreightInfo_Measures_LazyLoadedCore(docket =>
			{
				var hitCounts = GetPersistentPropertiesHitCount(() =>
				{
					_ = GetIAutoRating(docket).RateableMeasures;
				});
				AssertEquals("Poking measures should NOT trigger many property hits.", true, hitCounts < 15);
			});
		}

		public void TestIAutoRatingFreightInfo_Measures_LazyLoaded_CachingAcrossMeasures()
		{
			TestIAutoRatingFreightInfo_Measures_LazyLoadedCore(docket =>
			{
				var hitCounts = GetPersistentPropertiesHitCount(() =>
				{
					var measures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
					foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
					{
						if (measures.HasMeasureType(measureType))
						{
							_ = measures.GetActual(measureType);
							_ = measures.GetPartCount(measureType);
						}
					}
				});
				AssertEquals("Triggering load of every measure should not have many hits.", true, hitCounts < 100);
			});
		}

		void TestIAutoRatingFreightInfo_Measures_LazyLoadedCore(Action<WhsDocket> assert)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			Helper.SetProductWeightAndVolume(data.Part1, 1m, "LB", 1m, "L");
			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			var line1 = docket.Lines.AddNew();

			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();
			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 10m);

			docket.WD_TotalWeightUnit = "T";
			docket.WD_TotalWeight = 0.4m;
			docket.WD_TotalCubicUnit = "L";
			docket.WD_TotalCubic = 300m;
			docket[AutoratingChargeablePalletsColumn.Name] = (ZShort)111;

			docket.WD_PackagesSent = (ZInt)3m;
			docket.WD_TotalUnits = 6m;
			docket.WD_UnitsSent = 7m;
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;

			assert(docket);
		}

		#endregion

		protected virtual void SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(WhsDocketLine line, ZDecimal units)
		{
			line.WE_TransactionQuantity = units;
		}

		#endregion

		#region TestIAutoRatingFreightInfo_ServiceLevel

		public void TestIAutoRatingFreightInfo_ServiceLevel()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "TSL";
			serviceLevel.RS_Description = "Test Service Level";
			var docket = GetNewDocket();
			docket.WD_RS_NKServiceLevel = "TSL";
			var autoRating = (IAutoRatingFreightInfo)GetIAutoRating(docket);
			AssertEquals("STD", autoRating.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));
			AssertEquals("TSL", autoRating.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Containers

		public void TestIAutoRatingFreightInfo_Containers()
		{
			var docket1 = GetNewDocket();
			docket1.WD_DocketID = "Docket 1";

			var docket2 = GetNewDocket();
			docket2.WD_DocketID = "Docket 2";

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container1 = docket1.Containers.AddNew();
			container1.WC_RC = gp20.PK;
			container1.WC_ItemCount = 10;

			var container2 = docket1.Containers.AddNew();
			container2.WC_RC = gp20.PK;
			container2.WC_ItemCount = 11;
			container2.WC_IsPalletised = true;

			var container3 = docket2.Containers.AddNew();
			container3.WC_RC = gp40.PK;
			container3.WC_ItemCount = 12;

			var container4 = docket1.Containers.AddNew();
			container4.WC_IsChargeable = false;
			container4.WC_RC = gp40.PK;
			container4.WC_ItemCount = 15;

			var container5 = docket2.Containers.AddNew();

			var adapter1 = (IAutoRatingFreightInfo)GetIAutoRating(docket1);
			var measures1 = (RateableMeasureSet)adapter1.RateableMeasures;
			var actualContainers1 = measures1.GetPartList(MeasureType.ContainerCount).Cast<IRateableContainer>().ToArray();
			AssertEquals("Should have 2 measures.", 2, actualContainers1.Length);
			AssertRateableContainer(
				actualContainers1[0],
				gp20.PK.ToGuid(),
				"Docket 1",
				expectedContainerCount: 1,
				expectedContainerPackages: 10,
				expectedIsOnPallets: false);

			AssertRateableContainer(
				actualContainers1[1],
				gp20.PK.ToGuid(),
				"Docket 1",
				expectedContainerCount: 1,
				expectedContainerPackages: 11,
				expectedIsOnPallets: true);

			var adapter2 = (IAutoRatingFreightInfo)GetIAutoRating(docket2);
			var measures2 = (RateableMeasureSet)adapter2.RateableMeasures;
			var actualContainers2 = measures2.GetPartList(MeasureType.ContainerCount).Cast<IRateableContainer>().ToArray();
			AssertEquals("Should have 2 measures.", 2, actualContainers2.Length);
			AssertRateableContainer(
				actualContainers2[0],
				gp40.PK.ToGuid(),
				"Docket 2",
				expectedContainerCount: 1,
				expectedContainerPackages: 12,
				expectedIsOnPallets: false);

			AssertRateableContainer(
				actualContainers2[1],
				null,
				"Docket 2",
				expectedContainerCount: 1,
				expectedContainerPackages: 0,
				expectedIsOnPallets: false);

			void AssertRateableContainer(
				IRateableContainer actualContainer,
				Guid? expectedContainerTypePk,
				string expectedDocketReference,
				int expectedContainerCount,
				int expectedContainerPackages,
				bool expectedIsOnPallets)
			{
				CombineAssertions(
					"Rateable Container should have the correct values mapped.",
					() =>
					{
						AssertEquals(nameof(IRateableContainer.ContainerTypePk), expectedContainerTypePk, actualContainer.ContainerTypePk);
						AssertEquals(nameof(IRateableContainer.ContainerCount), expectedContainerCount, actualContainer.ContainerCount);
						AssertEquals(nameof(IRateableContainer.ContainerPackages), expectedContainerPackages, actualContainer.ContainerPackages);
						AssertEquals(nameof(IRateableContainer.DocketReference), expectedDocketReference, actualContainer.DocketReference);
						AssertEquals(nameof(IRateableContainer.IsOnPallets), expectedIsOnPallets, actualContainer.IsOnPallets);
					});
			}
		}

		#endregion

		#region TestIAutoRatingFreightInfo_WharfCTOAddress

		public void TestIAutoRatingFreightInfo_WharfCTOAddress()
		{
			var docket = GetNewDocket();
			var autoRating = GetIAutoRating(docket);
			AssertEquals(null, autoRating.WharfCTOAddress);

			var whs = Helper.CreateWarehouse("1");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			whs.WW_OA_WarehouseAddress = address.PK;
			docket.WD_WW_Whs = whs.PK;
			AssertEquals(address, autoRating.WharfCTOAddress);
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		public void TestIAutoRatingLocationsOrigin()
		{
			AssertNull(GetIAutoRating(GetNewDocket()).Origin);
		}

		public void TestIAutoRatingLocationsDestination()
		{
			AssertNull(GetIAutoRating(GetNewDocket()).Destination);
		}

		public void TestVia()
		{
			AssertNull(GetIAutoRating(GetNewDocket()).Destination);
		}

		#endregion

		#region IAutoRatingOrganisations Members

		#region TestIAutoRatingOrganisationsPickupCartageEquipment

		public void TestIAutoRatingOrganisationsPickupCartageEquipment()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertEquals(ZString.Empty, autoRating.PickupCartageEquipment);
		}

		#endregion

		#region TestIAutoRatingOrganisationsPickupAddress

		public void TestIAutoRatingOrganisationsPickupAddress()
		{
			var docket = GetNewDocket();
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(docket);
			var consignor = Factory.New<OrgHeader>();
			docket.PickUpAddressPK = consignor.Addresses.MainAddress.PK;
			AssertEquals(docket.PickUpDocAddress, autoRating.PickupAddress);
		}

		public void TestIAutoRatingOrganisationsPickupAddress_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetFinalisableHelper().GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			new JobHeader.Loader(docket).TryLoadOrCreateWithoutMutexForTestOnly();

			var consignor = Helper.CreateClient("CONSIGNOR");
			docket.PickUpAddressPK = consignor.Addresses.MainAddress.PK;

			AfterDocketFinalised(docket);

			docket.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsDocket>(docket.PK);
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(docketInOtherFactory);
			AssertEquals(consignor.MainAddress.PK, autoRating.PickupAddress.E2_OA_Address);

			var expectedHits = new Dictionary<string, int>(ExpectedHitsForPickupAddressCore)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHits, otherFactory);
		}

		protected abstract Dictionary<string, int> ExpectedHitsForPickupAddressCore { get; }

		#endregion

		#region TestIAutoRatingOrganisationsDeliveryCartageEquipment

		public void TestIAutoRatingOrganisationsDeliveryCartageEquipment()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertEquals(ZString.Empty, autoRating.DeliveryCartageEquipment);
		}

		#endregion

		#region TestIAutoRatingOrganisationsCarrier

		public void TestIAutoRatingOrganisationsCarrier()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertEquals(null, autoRating.Carrier);
		}

		#endregion

		#region TestIAutoRatingOrganisationsTransportProviders

		public void TestIAutoRatingOrganisationsTransportProviders()
		{
			var docket = GetNewDocket();
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(docket);
			AssertEquals(0, autoRating.Creditors.AllOrgs.Count);

			var expectedOrgs = new List<OrgHeader>();
			if (docket is IJobWithTransportCompany jobWithTransportCompany)
			{
				var transportCo = Factory.New<OrgHeader>();
				jobWithTransportCompany.TransportCoDocAddress.E2_OA_Address = transportCo.Addresses.MainAddress.PK;
				AssertEquals(1, autoRating.Creditors.AllOrgs.Count);
				AssertEquals(transportCo, autoRating.Creditors.AllOrgs[0]);
				AssertEquals(jobWithTransportCompany.TransportCoDocAddress.Organisation, autoRating.Creditors.AllOrgs[0]);

				expectedOrgs.Add(transportCo);
			}

			var testWarehouse = Factory.New<WhsWarehouse>();
			var org = Factory.New<OrgHeader>();
			testWarehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			docket.WD_WW_Whs = testWarehouse.PK;
			expectedOrgs.Add(org);
			AssertContainsExactElementsInAnyOrder(expectedOrgs, autoRating.Creditors.AllOrgs);
		}

		public void TestIAutoRatingOrganisationsTransportProviders_OverridenAddress()
		{
			var docket = GetNewDocket();
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(docket);
			AssertEquals(0, autoRating.Creditors.AllOrgs.Count);

			if (docket is IJobWithTransportCompany jobWithTransportCompany)
			{
				jobWithTransportCompany.TransportCoDocAddress.E2_AddressOverride = true;
				jobWithTransportCompany.TransportCoDocAddress.E2_Address1 = "TEST";
			}

			var testWarehouse = Factory.New<WhsWarehouse>();
			var org = Factory.New<OrgHeader>();
			testWarehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			docket.WD_WW_Whs = testWarehouse.PK;
			AssertContainsExactElementsInAnyOrder(new[] { org }, autoRating.Creditors.AllOrgs);
		}

		public void TestIAutoRatingOrganisationsTransportProviders_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetFinalisableHelper().GetNewFinalisableDocketWithOneLine(data, 10m, finalise: true);
			new JobHeader.Loader(docket).TryLoadOrCreateWithoutMutexForTestOnly();

			var expectedOrgs = new List<ZGuid>();
			if (docket is IJobWithTransportCompany jobWithTransportCompany)
			{
				var transportCo = Helper.CreateClient("TRANSPORT");
				jobWithTransportCompany.TransportCoDocAddress.E2_OA_Address = transportCo.Addresses.MainAddress.PK;
				expectedOrgs.Add(transportCo.PK);
			}

			expectedOrgs.Add(docket.Warehouse.WarehouseAddress.OA_OH);
			AfterDocketFinalised(docket);

			docket.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsDocket>(docket.PK);
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(docketInOtherFactory);
			AssertContainsExactElementsInAnyOrder(expectedOrgs, autoRating.Creditors.AllOrgs.Select(o => o.PK));

			var orgHeaderHits = otherFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName);
			var expectedHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, orgHeaderHits },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			if (docket is IJobWithTransportCompany)
			{
				expectedHits[OrgAddressSchema.Constants.TableName] += 1;
				expectedHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			}

			AssertDbHits(expectedHits, otherFactory);
			AssertEquals("OrgHeader Db Hits should either be 1 or 2.", true, orgHeaderHits == 1 || orgHeaderHits == 2);
		}

		protected abstract FinalisableDocketHelper<T> GetFinalisableHelper();

		protected virtual void AfterDocketFinalised(T docket)
		{
		}

		#endregion

		#region TestImportBroker

		public void TestImportBroker()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertNull(autoRating.ImportBroker);
		}

		#endregion

		#region TestJobDatesProvider

		public void TestJobDatesProvider()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertType<WhsDocketJobDatesProvider>(((IAutoRating)autoRating).JobDatesProvider);
		}

		#endregion

		#region TestExportBroker

		public void TestExportBroker()
		{
			var autoRating = (IAutoRatingOrganisations)GetIAutoRating(GetNewDocket());
			AssertNull(autoRating.ExportBroker);
		}

		#endregion

		#endregion

		#region IImportExport

		public void TestIImportExport()
		{
			var iDocket = GetIAutoRating(GetNewDocket());
			Assert(!iDocket.IsImport());
			Assert(!iDocket.IsExport());
			Assert(!iDocket.IsDomestic());
			Assert(!iDocket.IsCrossTrade());
			Assert(iDocket.IsUnknown());
			AssertEquals(Directions.Unknown, iDocket.JobDirection);
		}

		#endregion

		#region IAutoRatingWarehouseInfo Members

		public void TestIAutoRatingWarehouseInfo_WarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var autorating = (IAutoRatingWarehouseInfo)GetIAutoRating(docket);
			AssertEquals(data.Whs1.PK, autorating.WarehousePK);
		}

		public void TestIAutoRatingWarehouseInfo_WarehouseFallbackConsignorForFilterOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var supplier = Helper.CreateClient("Sup");
			docket.SupplierDocAddress.OrganisationPK = supplier.PK;

			var autorating = (IAutoRatingWarehouseInfo)GetIAutoRating(docket);
			AssertEquals(
				"Return supplier if supported by adapter.",
				SupportsSupplierAsFallbackConsignorFilter ? supplier : null,
				autorating.WarehouseFallbackConsignorForFilterOnly);
		}

		protected virtual bool SupportsSupplierAsFallbackConsignorFilter => false;

		#endregion

		#region Implementaion

		protected virtual void SetFinalisedDocketFields(WhsDocket docket, ZDateTime finalisedDate)
		{
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
		}

		protected abstract IAutoRating GetIAutoRating(WhsDocket docket);
		protected abstract WhsDocket GetNewDocket();

		#endregion
	}
}
