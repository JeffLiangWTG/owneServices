using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketRatingAdapter<T> : RatingAdapter<T>, IAutoRatingWarehouseInfo
		where T : WhsDocket
	{
		protected WhsDocketRatingAdapter(T docket)
			: base(docket)
		{
		}

		#region GetOutwardsStorageQuantityForProduct

		protected static ZDecimal GetOutwardsStorageQuantityForProduct(IReadOnlyDictionary<ZGuid, ZDecimal> quantitiesByProduct, OrgSupplierPart product, ZDecimal total)
		{
			ZDecimal result;
			var currentDocketTotalByProduct = total;

			quantitiesByProduct.TryGetValue(product.PK, out var runningTotal);
			if (runningTotal >= currentDocketTotalByProduct)
			{
				// don't charge
				result = 0;
			}
			else if (runningTotal <= 0)
			{
				// charge full amount
				result = currentDocketTotalByProduct;
			}
			else // 0 < runningTotal < line.SumOfUnitsMet
			{
				// partial charge
				result = (currentDocketTotalByProduct - runningTotal);
			}

			return result;
		}

		#endregion

		#region IAutoRating Members

		public override AdapterType AdapterType => AdapterType.Warehouse;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new WhsDocketJobDatesProvider(Parent); }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();

				foreach (var chargeCode in UsedChargeCodeGroups)
				{
					result.AddRange(GetServiceInfosFromJobServices(Parent.Services, chargeCode));
				}

				// we should not consider STG from registry to be a special job service as it is used to calculate storage charges in split month billing
				foreach (var serviceInfo in result.Where(c => c.ServiceCode == ChargeCodeSubGroupList.Storage).ToList())
				{
					result.Remove(serviceInfo);
				}

				return result;
			}
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				return Parent.Factory.GetCachedValue("DocketRatingAdapter|ChargeCodeGroups|" + typeof(T).Name, () =>
				{
					var chargeCodeGroups = new ChargeCodeGroupCollection();
					chargeCodeGroups.AddRange(UsedChargeCodeGroups.ToArray());
					return chargeCodeGroups;
				});
			}
		}

		protected abstract IEnumerable<string> UsedChargeCodeGroups { get; }

		public override RateType RateTypeToUse
		{
			get { return RateType.Warehouse; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		#endregion

		#region IAutoRatingOrganisations Members

		public override IDocAddress PickupAddress
		{
			get { return Parent.LoadJobDocAddressQuickly(Parent.PickUpDocAddressRequirement.DefaultDocAddressType); }
		}

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();

				var transportCoInfo = GetTransportCoOrganisationPropertyInfo();
				if (transportCoInfo != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgHeader>(transportCoInfo));
				}

				var warehouse = Parent.Warehouse;
				if (warehouse != null && warehouse.WarehouseAddress != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgAddress>(warehouse.WW_OA_WarehouseAddressInfo));
				}

				return Creditors.New(result);
			}
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();
				var localClient = InvoicingSupporter?.Job?.LocalCharges;
				if (localClient != null)
				{
					result[Registry.Business.RatingDebtorOrgTypes.LC] = localClient;
				}

				return result;
			}
		}

		protected abstract ZPropertyInfo GetTransportCoOrganisationPropertyInfo();

		#endregion

		#region IAutoRatingFreightInfo Members

		public override Directions JobDirection
		{
			get
			{
				Directions result;

				if (InvoicingSupporter.IsImport)
				{
					result = Directions.Import;
				}
				else if (InvoicingSupporter.IsExport)
				{
					result = Directions.Export;
				}
				else if (InvoicingSupporter.IsDomestic)
				{
					result = Directions.Domestic;
				}
				else
				{
					result = Directions.Unknown;
				}

				return result;
			}
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return true;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return InvoicingSupporter.ConsumerType; }
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, new Money(Parent.WD_TotalOrderValue, Parent.TotalOrderCurrency));
				return result;
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				var data = new ClosureData(Parent, AddError);
				CreateWeightVolumeUnitMeasure(result, data);

				var warehousePk = Parent.WD_WW_Whs;
				var docketRef = data.DocketReference;
				result.SetQuantityWithWarehouseDocket(MeasureType.JobWeight, JobWeight, Parent.WD_TotalWeightUnit, warehousePk, docketRef);
				result.SetQuantityWithWarehouseDocket(MeasureType.JobVolume, JobVolume, Parent.WD_TotalCubicUnit, warehousePk, docketRef);
				result.SetQuantityWithWarehouseDocket(MeasureType.JobUnit, JobLevelUnits, ZString.Empty, warehousePk, docketRef);
				result.SetQuantityWithWarehouseDocket(MeasureType.Shipment, 1m, ZString.Empty, warehousePk, docketRef);
				result.SetQuantityWithWarehouseDocket(MeasureType.ChargeablePallet, CalculateChargeablePallets(data), ZString.Empty, warehousePk, docketRef);
				SetAutoRatingContainers(result);
				CreatePackageMeasure(result, data);
				CreateLineCountMeasure(result, data);

				AddMeasuresCore(result, data);
				AddTimeToMeasures(result);

				CreateWarehousePackageLineMeasure(result, data);

				return result;

				void AddError(ZString errorMessage, MeasureType measure)
				{
					result.AddError(measure, errorMessage);
				}
			}
		}

		void CreateWarehousePackageLineMeasure(RateableMeasureSet measures, ClosureData data)
		{
			measures.CreateWarehousePackages((m) => LazyPopulateWarehousePackageLineMeasure(m, data));
		}

		protected virtual void LazyPopulateWarehousePackageLineMeasure(RateableMeasureSet measures, ClosureData data)
		{
		}

		void CreatePackageMeasure(RateableMeasureSet measures, ClosureData data)
		{
			measures.CreateWarehouseDocketPackageCountList((m) => LazyPopulatePackageMeasure(m, data));
		}

		protected virtual void LazyPopulatePackageMeasure(RateableMeasureSet measures, ClosureData data)
		{
			measures.AddWarehouseDocketPackageCount((decimal)Parent.WD_PackagesSent, Parent.WD_WW_Whs, GetDocketReference(Parent), Parent.WD_F3_NKTotalPackType, false);
		}

		protected virtual void AddMeasuresCore(RateableMeasureSet result, ClosureData data)
		{
		}

		protected static ZString GetDocketReference(WhsDocket docket)
		{
			// Note, accessing WD_ExternalReference at most once since there are tests that count property accesses
			var result = docket.WD_ExternalReference;
			if (result.IsEmpty)
			{
				result = docket.WD_DocketID;
			}
			return result;
		}

		protected void AddTimeToMeasures(RateableMeasureSet measures)
		{
			var arrivalDate = ((IAutoRating)this).JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
			var firstDayOfMonth = new ZDateTime(arrivalDate.Year, arrivalDate.Month, 1);
			measures.Time = new TimeInfo(firstDayOfMonth, arrivalDate);
		}

		protected virtual decimal JobWeight { get { return Parent.WD_TotalWeight; } }
		protected virtual decimal JobVolume { get { return Parent.WD_TotalCubic; } }
		protected virtual decimal JobLevelUnits { get { return Parent.WD_TotalUnits; } }

		#region GetMeasureFromLines

		protected class ClosureData
		{
			public ClosureData(T parent, Action<ZString, MeasureType> addError)
			{
				Parent = parent;
				AddError = addError;

				Client = new Lazy<OrgHeader>(() => Parent.Client);
				CompanyData = new Lazy<OrgCompanyData>(() => Client.Value?.CompanyData);
				LazyMiscServ = new Lazy<OrgMiscServ>(() => Client.Value?.MiscServ);
				LazyIsSplitMonthBilling = new Lazy<bool>(() => CompanyData.Value != null && CompanyData.Value.OB_ARWhsStorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
				LazyShouldChargeStorageInAdvance = new Lazy<bool>(() => CompanyData.Value != null && CompanyData.Value.OB_WhsChargeStorageInAdvance);
				LazyDocketReference = new Lazy<ZString>(() => GetDocketReference(Parent));
				LazyLines = new Lazy<DocketLine[]>(() => GetDocketLinesFromDBCombinedWithInMemoryChanges(Parent));
				LazyProducts = new Lazy<Dictionary<ZGuid, WhsProduct>>(() => GetDistinctProductsFromLines(Parent.Factory, Lines));
				LazyQuantitiesByProduct = new Lazy<Dictionary<ZGuid, ZDecimal>>(() => LoadPreviousDocketLinesForOrderSplitPeriodBilling(Parent));
				LazyOuterPackages = new Lazy<OuterPackage[]>(() => GetOuterPackages(Parent));
			}

			readonly T Parent;
			public readonly Action<ZString, MeasureType> AddError;

			Lazy<OrgHeader> Client { get; }
			Lazy<OrgCompanyData> CompanyData { get; }

			public OrgMiscServ MiscServ => LazyMiscServ.Value;
			Lazy<OrgMiscServ> LazyMiscServ { get; }

			public bool IsSplitMonthBilling => LazyIsSplitMonthBilling.Value;
			public Lazy<bool> LazyIsSplitMonthBilling { get; }

			public bool ShouldChargeStorageInAdvance => LazyShouldChargeStorageInAdvance.Value;
			Lazy<bool> LazyShouldChargeStorageInAdvance { get; }

			public ZString DocketReference => LazyDocketReference.Value;
			Lazy<ZString> LazyDocketReference { get; }

			#region Lines

			public DocketLine[] Lines => LazyLines.Value;
			Lazy<DocketLine[]> LazyLines { get; }

			static DocketLine[] GetDocketLinesFromDBCombinedWithInMemoryChanges(T parent)
			{
				var inMemoryQuery = new ZQuery();
				inMemoryQuery.AddToFilter(WhsDocketLineSchema.WE_WD, parent.PK);
				inMemoryQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				inMemoryQuery.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, null);
				inMemoryQuery.FetchOnlyFromLocalCache = true;

				var linesInMemory = parent.Factory.Load<WhsDocketLine>(inMemoryQuery)
					.Where(l => !l.IsInDatabase || l.HasChanges)
					.ToDictionary(l => l.PK, l => new DocketLine(l.PK, l.WE_OP, l.WE_PartAttrib1, l.WE_PartAttrib2, l.WE_PartAttrib3, l.WE_SerialNumber, l.WE_PalletID, l.SumOfUnitsMet, l.WE_F3_NKPackType));

				var result = new List<DocketLine>();

				foreach (var lineInDB in GetDocketLinesForJobInDB(parent))
				{
					if (linesInMemory.TryGetValue(lineInDB.PK, out var lineToUse))
					{
						result.Add(lineToUse);
					}
					else
					{
						result.Add(lineInDB);
					}

					linesInMemory.Remove(lineInDB.PK);
				}

				result.AddRange(linesInMemory.Values);

				return result.ToArray();
			}

			static IEnumerable<DocketLine> GetDocketLinesForJobInDB(T parent)
			{
				IEnumerable<DocketLine> lines;

				var cache = RatingCache.LocalSession;
				if (cache != null)
				{
					var ratingTarget = cache.Target;

					// this cache will be initialised in WhsInvoice if Autorating a Periodic Billing, otherwise initialise to null, we are just autorating this Docket.
					var docketsFromPeriodicInvoice = cache.GetCachedValue("WhsInvoice|AutoRating|Dockets|" + ratingTarget.PK, () => (WhsDocket[])null);
					if (docketsFromPeriodicInvoice != null)
					{
						var docketLineCache = cache.GetCachedValue("DocketRatingAdapter|DocketLines|" + ratingTarget.PK, () => GetDocketLines(docketsFromPeriodicInvoice));
						lines = docketLineCache.GetValueSafe(parent.PK) ?? Enumerable.Empty<DocketLine>();
					}
					else
					{
						var docketLineCache = cache.GetCachedValue("DocketRatingAdapter|DocketLines|" + parent.PK, () => GetDocketLines(new[] { parent }));
						lines = docketLineCache.GetValueSafe(parent.PK) ?? Enumerable.Empty<DocketLine>();
					}
				}
				else
				{
					lines = GetDocketLines(new[] { parent }).GetValueSafe(parent.PK) ?? Enumerable.Empty<DocketLine>();
				}

				return lines;

				Dictionary<ZGuid, List<DocketLine>> GetDocketLines(WhsDocket[] dockets)
				{
					const bool IsOrder = true;

					var collection = new DynamicBusinessObjectCollection(parent.Factory);
					var sqlParams = new ZSqlParameterCollection();

					var groupedDockets = dockets.ToLookup(d => d.WD_DocketType.EqualsIgnoringCase(DocketType.Codes.Order));
					var ordersParameter = ZSqlParameter.New("@OrderPKs", groupedDockets[IsOrder].Select(o => o.PK).ToArray(), WhsDocketLineSchema.WE_WD, isTableValued: true);
					var nonOrdersParameter = ZSqlParameter.New("@NonOrderPKs", groupedDockets[!IsOrder].Select(o => o.PK).ToArray(), WhsDocketLineSchema.WE_WD, isTableValued: true);
					sqlParams.Add(ordersParameter);
					sqlParams.Add(nonOrdersParameter);

					var sql = @"
SELECT
	WE_PK,
	WE_WD,
	WE_OP,
	CASE WHEN WE_DocketLineStatus = 'FIN' THEN WE_TransactionQuantity ELSE 0 END as Quantity,
	WE_PartAttrib1,
	WE_PartAttrib2,
	WE_PartAttrib3,
	WE_SerialNumber,
	WE_PalletID,
	WE_F3_NKPackType
FROM
	dbo.WhsDocketLine
WHERE
	WE_WD IN (SELECT Value FROM @NonOrderPKs)
	AND WE_IsOriginalInventory = 1

UNION ALL

SELECT
	WE_PK,
	WE_WD,
	WE_OP,
	CASE WHEN WD_WP IS NOT NULL THEN ISNULL(PickLineQuantity, 0) ELSE 0 END as Quantity,
	WE_PartAttrib1,
	WE_PartAttrib2,
	WE_PartAttrib3,
	WE_SerialNumber,
	'',
	WE_F3_NKPackType
FROM
	dbo.WhsDocketLine ParentLine
	JOIN dbo.WhsDocket ON WD_PK = WE_WD
	CROSS APPLY
	(
		SELECT SUM(WZ_Units) as PickLineQuantity
		FROM dbo.WhsPickLine
		WHERE WZ_WE_TransactionLine = ParentLine.WE_PK
	) as PickLineQuantity
WHERE
	WE_WD IN (SELECT Value FROM @OrderPKs)
	AND WE_IsOriginalInventory = 1
	AND WE_WE_ParentDocketLine IS NULL";

					collection.Load(sql, sqlParams);

					var docketLinesByDocketDictionary = new Dictionary<ZGuid, List<DocketLine>>();
					foreach (DynamicBusinessObject result in collection)
					{
						var docketPK = (ZGuid)result[WhsDocketLineSchema.Constants.WE_WD];
						if (!docketLinesByDocketDictionary.TryGetValue(docketPK, out var docketLines))
						{
							docketLinesByDocketDictionary[docketPK] = docketLines = new List<DocketLine>();
						}

						docketLines.Add(new DocketLine(
							(ZGuid)result[WhsDocketLineSchema.Constants.PK],
							(ZGuid)result[WhsDocketLineSchema.Constants.WE_OP],
							(ZString)result[WhsDocketLineSchema.Constants.WE_PartAttrib1],
							(ZString)result[WhsDocketLineSchema.Constants.WE_PartAttrib2],
							(ZString)result[WhsDocketLineSchema.Constants.WE_PartAttrib3],
							(ZString)result[WhsDocketLineSchema.Constants.WE_SerialNumber],
							(ZString)result[WhsDocketLineSchema.Constants.WE_PalletID],
							(ZDecimal)result[nameof(DocketLine.Quantity)],
							(ZString)result[WhsDocketLineSchema.Constants.WE_F3_NKPackType]));
					}

					return docketLinesByDocketDictionary;
				}
			}

			#endregion

			#region Products

			public IReadOnlyDictionary<ZGuid, WhsProduct> Products => LazyProducts.Value;
			Lazy<Dictionary<ZGuid, WhsProduct>> LazyProducts { get; }

			static Dictionary<ZGuid, WhsProduct> GetDistinctProductsFromLines(BusinessObjectFactory factory, IEnumerable<DocketLine> lines)
			{
				var productPKs = lines.Select(l => l.ProductPK).Distinct();
				return productPKs.ToDictionary(pk => pk, pk => WhsProduct.GetWhsProduct(factory, pk));
			}

			#endregion

			#region QuantitiesByProduct

			public IReadOnlyDictionary<ZGuid, ZDecimal> QuantitiesByProduct => LazyQuantitiesByProduct.Value;
			Lazy<Dictionary<ZGuid, ZDecimal>> LazyQuantitiesByProduct { get; }

			protected static Dictionary<ZGuid, ZDecimal> LoadPreviousDocketLinesForOrderSplitPeriodBilling(T parent)
			{
				var productQuantities = new Dictionary<ZGuid, ZDecimal>();
				var result = GetDocketLinesForPeriod(parent).GroupBy(l => (ZGuid)l[WhsDocketLineSchema.WE_OP]).ToArray();

				foreach (var group in result)
				{
					foreach (var docketLine in group.OrderBy(l => (ZDateTimeOffset)l[WhsDocketLineSchema.WE_FinalisedDate]))
					{
						UpdateRunningTotalFromBeginningOfThePeriod(group.Key, docketLine, productQuantities);
					}
				}

				return productQuantities;
			}

			#region GetDocketLinesForPeriod

			/// <summary>
			/// We use the Dynamic Bizo Collection to load the data as we could potentially have 1000s of docket lines in the month and we don't want an OutOfMemoryException
			/// </summary>
			static IEnumerable<DynamicBusinessObject> GetDocketLinesForPeriod(T parent)
			{
				var firstDayOfTheCurrentBillingPeriod = GetFirstDateOfCurrentSplitBillingPeriod(parent);

				var collection = new DynamicBusinessObjectCollection(parent.Factory);
				var sql = @"
SELECT WD_DocketType, WE_OP, WE_TransactionQuantity, WE_FinalisedDate
FROM dbo.WhsDocketLine
JOIN dbo.WhsDocket ON WE_WD = WD_PK
WHERE
	WE_WD <> @DocketPKToIgnore AND
	CAST(WE_FinalisedDate AS DATE) BETWEEN @StartDate AND @EndDate AND
	WE_DocketLineType IN (@ReceiveDocketType, @OrderDocketType, @AdjustmentDocketType) AND
	WD_DocketSubType NOT IN (@AdjustmentDocketSubTypeToExclude)
";

				var truncatedFinalisedDate = parent.WD_FinalisedDate.Date;

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@DocketPKToIgnore", parent.PK, WhsDocketLineSchema.PK);
				sqlParams.Add("@StartDate", firstDayOfTheCurrentBillingPeriod.Date, WhsDocketLineSchema.WE_ExpiryDate); // Schema column used for Date parameter
				sqlParams.Add("@EndDate", truncatedFinalisedDate.AddDays(1), WhsDocketLineSchema.WE_ExpiryDate); // Schema column used for Date parameter
				sqlParams.Add("@ReceiveDocketType", DocketType.Codes.Receive, WhsDocketSchema.WD_DocketType);
				sqlParams.Add("@OrderDocketType", DocketType.Codes.Order, WhsDocketSchema.WD_DocketType);
				sqlParams.Add("@AdjustmentDocketType", DocketType.Codes.Adjustment, WhsDocketSchema.WD_DocketType);
				sqlParams.Add("@AdjustmentDocketSubTypeToExclude", AdjustmentType.Codes.InternalWarehouseAdjustment, WhsDocketSchema.WD_DocketSubType);
				collection.Load(sql, sqlParams);

				return collection;
			}

			static ZDateTime GetFirstDateOfCurrentSplitBillingPeriod(WhsDocket docket)
			{
				ZDateTime previousPeriodicToDate;
				var storagePeriod = docket.Client.CompanyData.OB_ARWarehouseRatingPeriod;
				var currentPeriodicInvoice = GetCurrentPeriodicInvoice(docket);
				if (currentPeriodicInvoice == null
					|| docket.WD_FinalisedDate.Date >= AddStoragePeriodForDate(storagePeriod, currentPeriodicInvoice.ET_StorageFromDate.Date))
				{
					var previousPeriodicInvoice = GetPreviousPeriodicInvoice(docket);
					if (previousPeriodicInvoice != null)
					{
						previousPeriodicToDate = PredictPreviousPeriodicToDate(docket, previousPeriodicInvoice, storagePeriod);
					}
					else
					{
						previousPeriodicToDate = SubstractStoragePeriodForDate(storagePeriod, docket.WD_FinalisedDate.Date);
					}
				}
				else
				{
					previousPeriodicToDate = currentPeriodicInvoice.ET_StorageFromDate.AddDays(-1);
				}
				return previousPeriodicToDate.AddDays(1);
			}

			static IWhsInvoice GetCurrentPeriodicInvoice(WhsDocket docket)
			{
				var queryForCurrentPeriodic = new ZQuery(JobStorageSchema.ET_OH_Client, docket.WD_OH_Client);
				queryForCurrentPeriodic.AddToFilter(JobStorageSchema.ET_WW, docket.WD_WW_Whs);
				queryForCurrentPeriodic.AddToFilter(JobStorageSchema.ET_StorageFromDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, docket.WD_FinalisedDate.Date);
				queryForCurrentPeriodic.AddToFilter(JobStorageSchema.ET_StorageToDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, docket.WD_FinalisedDate.Date);

				return docket.Factory.LoadTop1(ObjectFactory.GetType<IWhsInvoice>(), queryForCurrentPeriodic) as IWhsInvoice;
			}

			static IWhsInvoice GetPreviousPeriodicInvoice(WhsDocket docket)
			{
				var queryForPreviousPeriodic = new ZQuery(JobStorageSchema.ET_OH_Client, docket.WD_OH_Client);
				queryForPreviousPeriodic.AddToFilter(JobStorageSchema.ET_WW, docket.WD_WW_Whs);
				queryForPreviousPeriodic.AddToFilter(JobStorageSchema.ET_StorageFromDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, docket.WD_FinalisedDate.Date);
				queryForPreviousPeriodic.OrderBy = JobStorageSchema.ET_StorageToDate.Name + " DESC";

				return docket.Factory.LoadTop1(ObjectFactory.GetType<IWhsInvoice>(), queryForPreviousPeriodic) as IWhsInvoice;
			}

			static ZDate PredictPreviousPeriodicToDate(WhsDocket docket, IWhsInvoice previousPeriodicInvoice, ZString storagePeriod)
			{
				ZDate currentDate;

				var nextPeriodicEnd = previousPeriodicInvoice.ET_StorageFromDate.AddDays(-1).Date;

				while ((currentDate = AddStoragePeriodForDate(storagePeriod, nextPeriodicEnd)) <= docket.WD_FinalisedDate.Date)
				{
					nextPeriodicEnd = currentDate;
				}

				return nextPeriodicEnd;
			}

			static ZDate AddStoragePeriodForDate(ZString whsStoragePeriod, ZDate date)
			{
				if (whsStoragePeriod == Constants.StorageCalculationPeriods.Monthly)
				{
					date = date.AddMonths(1);
				}
				else if (whsStoragePeriod == Constants.StorageCalculationPeriods.Weekly)
				{
					date = date.AddDays(7);
				}
				else
				{
					date = date.AddDays(1);
				}
				return date;
			}

			static ZDate SubstractStoragePeriodForDate(ZString whsStoragePeriod, ZDate date)
			{
				if (whsStoragePeriod == Constants.StorageCalculationPeriods.Monthly)
				{
					date = date.AddMonths(-1);
				}
				else if (whsStoragePeriod == Constants.StorageCalculationPeriods.Weekly)
				{
					date = date.AddDays(-7);
				}
				else
				{
					date = date.AddDays(-1);
				}
				return date;
			}

			#endregion

			#region UpdateRunningTotalFromBeginningOfThePeriod

			static void UpdateRunningTotalFromBeginningOfThePeriod(ZGuid productPK, DynamicBusinessObject docketLine, Dictionary<ZGuid, ZDecimal> productQuantities)
			{
				var result = productQuantities.GetOrAdd(productPK);

				var units = (ZDecimal)docketLine[WhsDocketLineSchema.WE_TransactionQuantity];
				var docketType = docketLine[WhsDocketSchema.Constants.WD_DocketType];
				if (docketType.Equals(DocketType.Codes.Receive)
					|| docketType.Equals(DocketType.Codes.Adjustment))
				{
					result += units;
				}
				else // this is an order
				{
					result -= units;
				}

				if (result < 0)
				{
					result = 0; // if result is negative, the order (or adjustment out) that made it negative has already had its charge applied, so we reset the result to 0.
				}

				productQuantities[productPK] = result;
			}

			#endregion

			#endregion

			#region OuterPackages

			public OuterPackage[] OuterPackages => LazyOuterPackages.Value;
			Lazy<OuterPackage[]> LazyOuterPackages { get; }

			static OuterPackage[] GetOuterPackages(T parent)
			{
				OuterPackage[] outerPackages = null;
				if (parent is WhsOrder parentOrder)
				{
					var factory = parent.Factory;
					// We do not use Parent.PackageJob.Packages here because it will register Packages as ChildEditable and cause additional DBHits
					var packageJob = factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, parent.PK));

					if (packageJob != null)
					{
						var query = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
						query.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);
						var packages = factory.Load<PkgPackage>(query);

						var packTypeIsPalletCache = new Dictionary<string, bool>();
						outerPackages = packages.Select(p => new OuterPackage(p, IsPalletPackType(packTypeIsPalletCache, parentOrder, p.KP_F3_NKPackType))).ToArray();
					}
				}

				return outerPackages;

				bool IsPalletPackType(Dictionary<string, bool> packTypeIsPalletCache, WhsOrder order, ZString packType)
				{
					if (!packTypeIsPalletCache.TryGetValue(packType, out var result))
					{
						packTypeIsPalletCache[packType] = result = order.CheckIsPalletPackType(packType);
					}

					return result;
				}
			}

			#endregion
		}

		protected struct OuterPackage
		{
			public OuterPackage(PkgPackage package, bool isPalletPackType)
			{
				Package = package;
				IsPalletPackType = isPalletPackType;
			}

			public readonly PkgPackage Package;
			public readonly bool IsPalletPackType;
		}

		void CreateLineCountMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulateLineCount = measures =>
			{
				measures.AddLineCountWithWarehouseDocket(closureData.Lines.Length, Parent.WD_WW_Whs, closureData.DocketReference);
			};

			rateableMeasures.CreateLineCountWithWarehouseDocket(lazyPopulateLineCount);
			rateableMeasures.CreateUnidentifiedCountWithWarehouseDocketBasedOnLineCount();
		}

		protected virtual void CreateWeightVolumeUnitMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			CreateDocketLineMeasure(rateableMeasures, closureData, true, true);
		}

		protected void CreateDocketLineMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData, bool useNormalMeasures, bool useStorageMeasures)
		{
			Action<RateableMeasureSet> lazyPopulateLines = measures =>
			{
				foreach (var line in closureData.Lines)
				{
					if (closureData.Products.TryGetValue(line.ProductPK, out var product) && product != null)
					{
						var quantity = line.Quantity;
						var weightInKG = GetWeightForRating(product, quantity);
						var volumeInM3 = GetVolumeForRating(product, quantity);
						var units = GetUnitsForRating(product, quantity);
						var attributes = GetProductAttributesMeasure(line, closureData.MiscServ);

						if (useNormalMeasures)
						{
							measures.AddWarehouseDocketNormalLine(weightInKG, volumeInM3, units, Parent.WD_WW_Whs, line.ProductPK, attributes, product.Parent.OP_RH_NKCommodityCode, closureData.DocketReference, line.PackType);
						}
						if (useStorageMeasures)
						{
							measures.AddWarehouseDocketStorageLine(weightInKG, volumeInM3, units, Parent.WD_WW_Whs, line.ProductPK, attributes, product.Parent.OP_RH_NKCommodityCode, closureData.DocketReference, line.PackType);
						}
					}
				}
			};

			rateableMeasures.CreateWarehouseDocketLines(lazyPopulateLines,
				useNormalMeasures: useNormalMeasures,
				useStorageMeasures: useStorageMeasures,
				RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes | RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType);
		}

		protected static (decimal Amount, string ErrorMessage) GetWeightForRating(WhsProduct product, ZDecimal quantity)
		{
			if (!product.IsWeightUnitInvalid)
			{
				return (product.GetWeightInKG(quantity), null);
			}
			else
			{
				return (0, product.GetInvalidWeightUnitErrorMessage());
			}
		}

		protected static (decimal Amount, string ErrorMessage) GetVolumeForRating(WhsProduct product, ZDecimal quantity)
		{
			if (!product.IsVolumeUnitInvalid)
			{
				return (product.GetVolumeInM3(quantity), null);
			}
			else
			{
				return (0, product.GetInvalidVolumeUnitErrorMessage());
			}
		}

		protected static decimal GetUnitsForRating(WhsProduct product, ZDecimal quantity)
			=> quantity.Round(product.Parent.OP_CountDecimalPlaces);

		#endregion

		#region CalculateChargeablePallets

		int CalculateChargeablePallets(ClosureData data) => CalculateChargeablePalletsCore(data);
		protected virtual int CalculateChargeablePalletsCore(ClosureData data) => Parent.WD_TotalPallets;

		#endregion

		#region GetProductAttributesMeasure

		static ProductAttributesMeasure GetProductAttributesMeasure(IPartAttributes attributes, OrgMiscServ miscServ)
		{
			var result = Array.Empty<string>();
			if (miscServ != null)
			{
				if (miscServ.OM_IMAttrib1IsKey)
				{
					AddToProductAttributesMeasureList(attributes.PartAttrib1, ref result, 0);
				}
				if (miscServ.OM_IMAttrib2IsKey)
				{
					AddToProductAttributesMeasureList(attributes.PartAttrib2, ref result, 1);
				}
				if (miscServ.OM_IMAttrib3IsKey)
				{
					AddToProductAttributesMeasureList(attributes.PartAttrib3, ref result, 2);
				}
				if (miscServ.OM_IMSerialNumberIsKey)
				{
					AddToProductAttributesMeasureList(attributes.SerialNumber, ref result, 3);
				}
			}

			return new ProductAttributesMeasure(result);
		}

		static void AddToProductAttributesMeasureList(ZString attribute, ref string[] result, int index)
		{
			if (result.Length < index + 1)
			{
				Array.Resize(ref result, index + 1);
			}
			result[index] = attribute;
		}

		#endregion

		#region SetAutoRatingContainers

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			measures.CreateContainerList(PopulateAutoRatingContainers, includeCommodity: false, includePalletized: true, includeDocketReference: true);
		}

		void PopulateAutoRatingContainers(RateableMeasureSet measures)
		{
			var containersByType = GetContainerInfos();
			var docketReference = GetDocketReference(Parent);
			foreach (KeyValuePair<ContainerKey, List<MeasureInfo.ContainerInfo>> kv in containersByType)
			{
				measures.AddContainerGroupWithPalletizedAndDocket(kv.Key.ContainerType, kv.Key.IsPalletized, docketReference, kv.Value);
			}
		}

		Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>> GetContainerInfos()
		{
			var containersByType = new Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>>();
			foreach (WhsDocketContainer container in Parent.Containers)
			{
				if (container.WC_IsChargeable)
				{
					var key = new ContainerKey(container);
					List<MeasureInfo.ContainerInfo> containers;
					if (!containersByType.TryGetValue(key, out containers))
					{
						containers = new List<MeasureInfo.ContainerInfo>();
						containersByType.Add(key, containers);
					}

					var teu = container.Container?.RC_TEU ?? 0m;
					containers.Add(new MeasureInfo.ContainerInfo(0m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, container.WC_ItemCount, teu, container.WC_ContainerNum));
				}
			}

			return containersByType;
		}

		struct ContainerKey : IEquatable<ContainerKey>
		{
			public ContainerKey(WhsDocketContainer container)
				: this(container.WC_RC, container.WC_IsPalletised)
			{
			}

			public ContainerKey(ZGuid containerType, ZBool isPalletized)
			{
				ContainerType = containerType;
				IsPalletized = isPalletized;
			}

			public readonly ZGuid ContainerType;
			public readonly ZBool IsPalletized;

			public bool Equals(ContainerKey other)
			{
				return ContainerType == other.ContainerType && IsPalletized == other.IsPalletized;
			}

			public override bool Equals(object obj)
			{
				return Equals((ContainerKey)obj);
			}

			public override int GetHashCode()
			{
				return ContainerType.GetHashCode() ^ IsPalletized.GetHashCode();
			}
		}

		#endregion

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>();

				var carrierServiceLevel = Parent.CarrierServiceLevel;
				if (carrierServiceLevel != null)
				{
					result.Add(new ServiceLevelInfo(carrierServiceLevel.PL_Code, ServiceLevelType.Carrier));
				}

				var serviceLevel = Parent.ServiceLevel;
				if (serviceLevel != null)
				{
					result.Add(new ServiceLevelInfo(serviceLevel.RS_Code, ServiceLevelType.Client));
				}

				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		public override OrgAddress WharfCTOAddress
		{
			get { return Parent.Warehouse?.WarehouseAddress; }
		}

		ZGuid IAutoRatingWarehouseInfo.WarehousePK
		{
			get { return Parent.WD_WW_Whs; }
		}

		public OrgHeader WarehouseFallbackConsignorForFilterOnly => WarehouseFallbackConsignorForFilterOnlyCore;
		protected virtual OrgHeader WarehouseFallbackConsignorForFilterOnlyCore => null;

		#endregion
	}
}
