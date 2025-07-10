using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GenericSchema = CargoWise.Schema.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceRatingAdapter : RatingAdapter<WhsInvoice>, IAutoRatingWarehouseInfo
	{
		public WhsInvoiceRatingAdapter(WhsInvoice invoice)
			: base(invoice)
		{
		}

		public override AdapterType AdapterType => AdapterType.WarehouseInvoice;

		public override IJobInvoicingSupporter InvoicingSupporter => Parent.InvoicingSupporter;

		#region IAutoRating Members

		public override IJobDatesProvider JobDatesProvider => new WhsInvoiceJobDatesProvider(Parent);

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (fChargeCodeGroups == null)
				{
					fChargeCodeGroups = new ChargeCodeGroupCollection();
					fChargeCodeGroups.Add(ChargeCodeGroupList.Codes.WHSStorage);
				}
				return fChargeCodeGroups;
			}
		}

		ChargeCodeGroupCollection fChargeCodeGroups;

		public override RateType RateTypeToUse => RateType.Warehouse;

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				var hasValidStorageDates = Parent.ET_StorageFromDate.IsValid && Parent.ET_StorageToDate.IsValid;
				return !hasValidStorageDates
					? new AutoRatingStatusInfo(false, Res.GetString("WhsInvoice|IAutoRating.StatusInformation", "Valid From and To dates are required before this Warehouse Periodic Invoice can be Auto Rated."))
					: new AutoRatingStatusInfo(true, ZString.Empty);
			}
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.WarehouseStorage;

		public override MergeChargeOptions MergeCharges => MergeChargeOptions.WithinAdapter;

		#endregion

		#region IAutoRatingOrganisations Members

		public override Creditors Creditors => Creditors.New(
			Parent.Warehouse != null && Parent.Warehouse.WarehouseAddress != null ?
				OrgWithSource.NewFrom<OrgAddress>(Parent.Warehouse.WW_OA_WarehouseAddressInfo)
				: null
			);

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

				if (InvoicingSupporter?.Job != null)
				{
					result[Registry.Business.RatingDebtorOrgTypes.LC] = InvoicingSupporter.Job.LocalCharges;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRatingFreightInfo Members

		#region IAutoRatingFreightInfo_Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				return GetMeasures(((IAutoRating)this).JobDatesProvider, true);
			}
		}

		public RateableMeasureSet GetMeasures(IJobDatesProvider jobDatesProvider, bool calculateTime = false)
		{
			var result = new RateableMeasureSet(AdapterType);
			var closureData = new ClosureData(Parent, jobDatesProvider, result.AddError);

			// Hack to stop Calculators that don't use Measures from being filtered out due to Warehouse
			result.SetUnidentifiedQuantityForWarehouse(0, Parent.ET_WW);

			if (calculateTime)
			{
				result.Time = GetTimeMeasure(closureData);
			}
			CreateWeightVolumeAndUnitMeasure(result, closureData);
			CreateChargeablePalletMeasure(result, closureData);
			CreateJobUnitMeasure(result, closureData);
			CreateLocationPalletMeasure(result, closureData);
			CreatePalletIDMeasure(result, closureData);

			return result;
		}

		void CreateWeightVolumeAndUnitMeasure(RateableMeasureSet measures, ClosureData closureData)
		{
			if (closureData.StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
			{
				CreateSplitMonthBillingInventoryMeasure(measures, closureData);
			}
			else
			{
				CreateNormalInventoryWeightVolumeAndUnitMeasure(measures, closureData);
			}
		}

		#region GetTimeMeasure

		TimeInfo GetTimeMeasure(ClosureData closureData)
		{
			TimeInfo timeInfo = null;
			var firstDay = closureData.FirstDay;
			var lastDay = closureData.LastDay;
			if (firstDay.IsValid && lastDay.IsValid)
			{
				if (closureData.StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling && closureData.ShouldChargeStorageInAdvance)
				{
					switch (closureData.CompanyData.OB_ARWarehouseRatingPeriod)
					{
						case Constants.StorageCalculationPeriods.Monthly:
							{
								firstDay = firstDay.AddMonths(1);
								if (firstDay.Day == 1)
								{
									var finalDayInMonth = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
									lastDay = new ZDateTime(firstDay.Year, firstDay.Month, finalDayInMonth);
								}
								else
								{
									lastDay = lastDay.AddMonths(1);
								}
								break;
							}
						case Constants.StorageCalculationPeriods.Weekly:
							{
								firstDay = firstDay.AddDays(7);
								lastDay = lastDay.AddDays(7);
								break;
							}
					}
				}
				timeInfo = new TimeInfo(firstDay, lastDay);
			}

			return timeInfo;
		}

		#endregion

		#region CreateChargeablePalletMeasure

		void CreateChargeablePalletMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				var numberOfPallets = 0m;
				foreach (var entry in closureData.UnitsPerProduct)
				{
					var product = GetProductbyPart(entry.Key.ProductPK);
					if (product?.Parent != null)
					{
						var palletSize = product.Parent.OP_StockKeepingUnitPerPallet;
						if (palletSize > 0m)
						{
							var quantity = closureData.GetQuantity(entry.Value);
							numberOfPallets += Utilities.Round(quantity / palletSize, 10);
						}
						else
						{
							numberOfPallets = 0m;
							break;
						}
					}
				}

				measures.AddChargeablePallet(numberOfPallets);
			};

			rateableMeasures.CreateChargeablePalletList(lazyPopulate);
		}

		WhsProduct GetProductbyPart(ZGuid productPk) => WhsProduct.GetWhsProduct(Parent.Factory, productPk);

		#endregion

		#region CreateJobUnitMeasure

		void CreateJobUnitMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				// Note, a measure with no dimensions/attributes should just be a single value
				// since there is nothing distinguishing each value.
				// The rating engine will just add them all up anyway.
				// Consider adding them all up here.
				foreach (var entry in closureData.UnitsPerProduct)
				{
					var quantity = closureData.GetQuantity(entry.Value);
					measures.AddJobUnit(quantity);
				}
			};

			rateableMeasures.CreateJobUnitList(lazyPopulate);
		}

		#endregion

		#region CreateLocationPalletMeasure

		void CreateLocationPalletMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				foreach (KeyValuePair<UnitsTableKey, BalanceUnits> entry in closureData.UnitsPerProductLocation)
				{
					var locationMeasure = GetLocationMeasure(entry.Key.LocationPK);
					var product = GetProductbyPart(entry.Key.ProductPK);
					var commodity = product?.Parent?.OP_RH_NKCommodityCode ?? ZString.Empty;

					var locationPallets = 0m;
					if (product?.Parent != null)
					{
						var palletSize = product.Parent.OP_StockKeepingUnitPerPallet;
						if (palletSize > 0m)
						{
							var quantity = closureData.GetQuantity(entry.Value);
							locationPallets = Utilities.Round(quantity / palletSize, 10);
						}
					}

					measures.AddLocationPallet(locationPallets,
						entry.Key.WarehousePK,
						locationMeasure,
						entry.Key.ProductPK,
						entry.Key.Attributes,
						commodity);
				}
			};

			rateableMeasures.CreateLocationPalletList(lazyPopulate);
		}

		LocationMeasure GetLocationMeasure(ZGuid locationPK)
		{
			var location = Parent.Factory.Load<WhsLocation>(locationPK);
			return location != null
				? new LocationMeasure(location.PK, location.LocationType.WLT_Code, location.ToLocationString())
				: new LocationMeasure(ZGuid.Empty, ZString.Empty, ZString.Empty);
		}

		#endregion

		#region CreatePalletIDMeasure

		void CreatePalletIDMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				var palletIDInfos = closureData.UnitsPerPeriod.GetUnitsByPalletID()
					.Where(e => e.Value != null && !e.Key.PalletID.IsEmpty && closureData.GetQuantity(e.Value) > 0)
					.GroupBy(entry => new { entry.Key.WarehousePK, entry.Key.PalletID })
					.Select(g => new { g.Key.WarehousePK, g.Key.PalletID }).ToList();
				if (palletIDInfos.Count > 0)
				{
					foreach (var entry in palletIDInfos)
					{
						measures.AddWarehousePalletId(entry.WarehousePK, palletId: entry.PalletID);
					}
				}
			};

			rateableMeasures.CreateWarehousePalletIdList(lazyPopulate, includeDocketReference: false);
		}

		#endregion

		#region CreateSplitMonthBillingInventoryMeasure

		void CreateSplitMonthBillingInventoryMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				foreach (var entry in closureData.UnitsPerProduct)
				{
					var product = GetProductbyPart(entry.Key.ProductPK);
					var quantity = closureData.GetQuantity(entry.Value);
					var weight = GetWeightForRating(product, quantity);
					var volume = GetVolumeForRating(product, quantity);
					var commodity = product?.Parent?.OP_RH_NKCommodityCode ?? ZString.Empty;
					measures.AddWarehouseProduct(weight, volume, quantity, entry.Key.WarehousePK, entry.Key.ProductPK, commodityCode: commodity);
				}
			};

			rateableMeasures.CreateWarehouseProductList(lazyPopulate, includeProductAttributes: false);
		}

		#endregion

		#region CreateNormalInventoryWeightVolumeAndUnitMeasure

		void CreateNormalInventoryWeightVolumeAndUnitMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulate = (measures) =>
			{
				foreach (var entry in closureData.UnitsPerProduct)
				{
					var product = GetProductbyPart(entry.Key.ProductPK);
					var quantity = closureData.GetQuantity(entry.Value);
					var weight = GetWeightForRating(product, quantity);
					var volume = GetVolumeForRating(product, quantity);
					var commodity = product?.Parent?.OP_RH_NKCommodityCode ?? ZString.Empty;
					measures.AddWarehouseProduct(weight, volume, quantity,
						entry.Key.WarehousePK,
						entry.Key.ProductPK,
						entry.Key.Attributes,
						commodityCode: commodity);
				}
			};

			rateableMeasures.CreateWarehouseProductList(lazyPopulate, includeProductAttributes: true);
		}

		static (decimal Amount, string ErrorMessage) GetWeightForRating(WhsProduct product, ZDecimal quantity)
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

		static (decimal Amount, string ErrorMessage) GetVolumeForRating(WhsProduct product, ZDecimal quantity)
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

		#endregion

		#region ClosureData class

		class ClosureData
		{
			public ClosureData(WhsInvoice invoice, IJobDatesProvider jobDatesProvider, Action<MeasureType, string> addError)
			{
				Parent = invoice;
				AddError = addError;

				LazyFirstDay = new Lazy<ZDateTime>(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
				LazyLastDay = new Lazy<ZDateTime>(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

				LazyCompanyData = new Lazy<OrgCompanyData>(() => Parent.Client?.CompanyData);
				LazyShouldChargeStorageInAdvance = new Lazy<bool>(() => CompanyData?.OB_WhsChargeStorageInAdvance ?? false);

				LazyStorageCalcMethod = new Lazy<ZString>(() => CompanyData?.OB_ARWhsStorageCalcMethod ?? OrgCompanyDataLookups.WarehouseStorageMax);

				LazyUnitsPerPeriod = new Lazy<BalanceUnitsTable>(() => GetUnitsPerPeriod(FirstDay, LastDay, CompanyData?.OB_WhsClientFreeStorageDays ?? 0));
				LazyUnitsPerProduct = new Lazy<IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>>>(GetUnitsPerProduct);
				LazyUnitsPerProductLocation = new Lazy<IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>>>(GetUnitsPerProductLocation);

				if (StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
				{
					GetQuantity = (balance) => balance?.MinimumUnits ?? 0m;
				}
				else
				{
					GetQuantity = (balance) => balance?.MaximumUnits ?? 0m;
				}
			}

			readonly WhsInvoice Parent;
			public readonly Action<MeasureType, string> AddError;
			public readonly Func<BalanceUnits, decimal> GetQuantity;

			public ZDateTime FirstDay => LazyFirstDay.Value;
			Lazy<ZDateTime> LazyFirstDay { get; }

			public ZDateTime LastDay => LazyLastDay.Value;
			Lazy<ZDateTime> LazyLastDay { get; }

			public OrgCompanyData CompanyData => LazyCompanyData.Value;
			Lazy<OrgCompanyData> LazyCompanyData { get; }

			public bool ShouldChargeStorageInAdvance => LazyShouldChargeStorageInAdvance.Value;
			Lazy<bool> LazyShouldChargeStorageInAdvance { get; }

			public ZString StorageCalcMethod => LazyStorageCalcMethod.Value;
			Lazy<ZString> LazyStorageCalcMethod { get; }

			#region UnitsPerPeriod

			public BalanceUnitsTable UnitsPerPeriod => LazyUnitsPerPeriod.Value;

			Lazy<BalanceUnitsTable> LazyUnitsPerPeriod { get; }

			BalanceUnitsTable GetUnitsPerPeriod(ZDateTime fromDate, ZDateTime toDate, int freeStorageDays)
			{
				BalanceUnitsTable result;
				if (StorageCalcMethod == OrgCompanyDataLookups.WarehouseStorageClosingBalance
					|| (StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling && ShouldChargeStorageInAdvance))
				{
					result = GetBalance(toDate.AddDays(1).Date, freeStorageDays);
				}
				else
				{
					var transactions = GetTransactions(fromDate, toDate, freeStorageDays);

					result = GetBalance(fromDate.Date, freeStorageDays);
					foreach (DynamicBusinessObject line in transactions)
					{
						var units = (ZDecimal)line["Units"];
						var docketType = (ZString)line["DocketType"];
						var docketSubType = (ZString)line["DocketSubType"];
						var isInternalAdjustment = docketType == DocketType.Codes.Adjustment && docketSubType == AdjustmentType.Codes.InternalWarehouseAdjustment;
						if (isInternalAdjustment && StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
						{
							//  we need to ignore inter internal adjustments for the current period for split month billing
							continue;
						}

						var isInternalTransfer = docketType == DocketType.Codes.Transfer && docketSubType == TransferType.Codes.Internal;
						if (!isInternalTransfer && (units > 0 || isInternalAdjustment || StorageCalcMethod != OrgCompanyDataLookups.WarehouseStorageMax))
						{
							// include negative adjustments if they are internal because this is generally used to correct
							// misinformation (for example adjust out 10 RED and adjust in 10 BLUE).
							result.AddTransaction(LineToUnitsTableKey(line), units);
						}

						result.AddPalletIDTransaction(LineToUnitsTableKey(line), units);
					}
				}
				result.RemoveNegativeValues();

				return result;
			}

			#region GetTransactions

			DynamicBusinessObjectCollection GetTransactions(ZDateTime fromDate, ZDateTime toDate, int freeStorageDays)
			{
				var transactions = new DynamicBusinessObjectCollection(Parent.Factory);

				if (fromDate.IsValidSmallDateTime && toDate.IsValidSmallDateTime)
				{
					var sqlParams = new ZSqlParameterCollection();
					sqlParams.Add("@ClientPK", Parent.ET_OH_Client, JobStorageSchema.ET_OH_Client);
					sqlParams.Add("@WarehousePK", Parent.ET_WW, JobStorageSchema.ET_WW);
					sqlParams.Add("@FromDate", fromDate.Date, WhsDocketLineSchema.WE_ExpiryDate); // Schema column used for Date parameter
					sqlParams.Add("@ToDate", toDate.AddDays(1).Date, WhsDocketLineSchema.WE_ExpiryDate); // Schema column used for Date parameter
					sqlParams.Add("@FreeStorageDays", freeStorageDays, GenericSchema.GenericIntSchemaColumn);
					sqlParams.Add("@Company", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);

					transactions.Load(
@"-- Used to Rate free storage.
SELECT OriginalDocketLinePK, WarehousePK, ClientPK, LocationPK, ProductPK, PartAttrib1, PartAttrib2, PartAttrib3, SerialNumber, PalletID, Units,
CASE WHEN WD_ExternalReference = '' THEN WD_DocketID ELSE WD_ExternalReference END AS DocketReference, StorageChargePK,
		DocketType,
		DocketSubType
FROM
(
	-- receives
	SELECT
		WE_PK as OriginalDocketLinePK,
		WD_WW_Whs as WarehousePK,
		WE_WL as LocationPK,
		WE_OP as ProductPK,
		WE_PartAttrib1 as PartAttrib1,
		WE_PartAttrib2 as PartAttrib2,
		WE_PartAttrib3 as PartAttrib3,
		WE_SerialNumber as SerialNumber,
		WE_TransactionQuantity as Units,
		WD_OH_Client as ClientPK,
		DATEADD(day, @FreeStorageDays, WD_FinalisedDate) as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		WE_PalletID as PalletID
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
	WHERE
		WE_IsOriginalInventory = 1
		AND WD_DocketType = 'INW' AND WD_DocketStatus = 'FIN'

	UNION ALL

	-- adjustments
	SELECT
		WE_PK as OriginalDocketLinePK,
		WD_WW_Whs as WarehousePK,
		WE_WL as LocationPK,
		WE_OP as ProductPK,
		WE_PartAttrib1 as PartAttrib1,
		WE_PartAttrib2 as PartAttrib2,
		WE_PartAttrib3 as PartAttrib3,
		WE_SerialNumber as SerialNumber,
		WE_TransactionQuantity as Units,
		WD_OH_Client as ClientPK,
		CASE WHEN WD_DocketSubType = 'IWA' AND WE_AdjustmentArrivalDate IS NOT NULL THEN WE_AdjustmentArrivalDate ELSE WD_FinalisedDate END as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		WE_PalletID as PalletID
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
	WHERE
		WD_DocketType = 'ADJ' AND WD_DocketStatus = 'FIN'

	UNION ALL

	-- internal-source transfers
	SELECT
		WE_PK as OriginalDocketLinePK,
		WD_WW_Whs as WarehousePK,
		WE_WL as LocationPK,
		WE_OP as ProductPK,
		WE_PartAttrib1 as PartAttrib1,
		WE_PartAttrib2 as PartAttrib2,
		WE_PartAttrib3 as PartAttrib3,
		WE_SerialNumber as SerialNumber,
		-WE_TransactionQuantity as Units,
		WD_OH_Client as ClientPK,
		WE_FinalisedDate as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		WE_TransferFromPalletId as PalletID
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
	WHERE
		WD_DocketType = 'TFR'
		AND WD_DocketSubType = 'TFR'
		AND WE_DocketLineStatus = 'FIN'

	UNION ALL

	-- internal-destination transfers
	SELECT
		WE_PK as OriginalDocketLinePK,
		WD_WW_Whs as WarehousePK,
		WE_WL as LocationPK,
		WE_OP as ProductPK,
		WE_PartAttrib1 as PartAttrib1,
		WE_PartAttrib2 as PartAttrib2,
		WE_PartAttrib3 as PartAttrib3,
		WE_SerialNumber as SerialNumber,
		WE_TransactionQuantity as Units,
		WD_OH_Client as ClientPK,
		WE_FinalisedDate as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		WE_PalletId as PalletID
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
	WHERE
		WD_DocketType = 'TFR'
		AND WD_DocketSubType = 'TFR'
		AND WE_DocketLineStatus = 'FIN'

	UNION ALL

	-- inter-Whs transfers
	SELECT
		WE_PK as OriginalDocketLinePK,
		WD_WW_Whs as WarehousePK,
		WE_WL as LocationPK,
		WE_OP as ProductPK,
		WE_PartAttrib1 as PartAttrib1,
		WE_PartAttrib2 as PartAttrib2,
		WE_PartAttrib3 as PartAttrib3,
		WE_SerialNumber as SerialNumber,
		CASE WD_DocketSubType
			WHEN 'IWS' THEN -WE_TransactionQuantity
			WHEN 'IWD' THEN WE_TransactionQuantity
		END as Units,
		WD_OH_Client as ClientPK,
		WE_FinalisedDate as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		CASE WD_DocketSubType
			WHEN 'IWS' THEN WE_TransferFromPalletId 
			WHEN 'IWD' THEN WE_PalletId
		END as PalletID
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
	WHERE
		WD_DocketType = 'TFR'
		AND (WD_DocketSubType = 'IWS' OR WD_DocketSubType = 'IWD')
		AND WE_DocketLineStatus = 'FIN'

	UNION ALL
	
	-- orders and work-orders
	SELECT
		InventoryLine.WE_WE_OriginalDocketLineForRating as OriginalDocketLinePK, -- link to original receive
		WD_WW_Whs as WarehousePK,
		InventoryLine.WE_WL as LocationPK,
		TransactionLine.WE_OP as ProductPK,
		InventoryLine.WE_PartAttrib1 as PartAttrib1,
		InventoryLine.WE_PartAttrib2 as PartAttrib2,
		InventoryLine.WE_PartAttrib3 as PartAttrib3,
		InventoryLine.WE_SerialNumber as SerialNumber,
		-WZ_Units as Units,
		WD_OH_Client as ClientPK,
		WD_FinalisedDate as FinalisedDate,
		WD_DocketType as DocketType,
		WD_DocketSubType as DocketSubType,
		InventoryLine.WE_PalletID as PalletID
	FROM
		dbo.WhsDocket
		JOIN dbo.WhsDocketLine as TransactionLine ON TransactionLine.WE_WD = WD_PK
		JOIN dbo.WhsPick ON WD_WP = WP_PK
		JOIN dbo.WhsPickLine ON WZ_WE_TransactionLine = TransactionLine.WE_PK
		JOIN dbo.WhsDocketLine as InventoryLine ON InventoryLine.WE_PK = ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
	WHERE
		WP_PickStatus = 'FIN'
		AND WD_FinalisedDate IS NOT NULL
		AND WD_DocketType IN ('ORD', 'WOR', 'DWO')
) as DocketLines
JOIN dbo.WhsDocketLine ON WE_PK = OriginalDocketLinePK -- for orders/work-orders
JOIN dbo.WhsDocket ON WD_PK = WE_WD
LEFT JOIN
(
	SELECT JR_PK as StorageChargePK, JH_ParentID, EC_Value
	FROM dbo.JobHeader
	JOIN dbo.JobCharge ON JR_JH = JH_PK
	JOIN dbo.JobChargeAttrib ON EC_JR = JR_PK
	JOIN dbo.AccChargeCode ON AC_PK = JR_AC
	WHERE JH_GC = @Company
	AND EC_Name = 'DLK'
	AND AC_ChargeGroup = 'WIN'
	AND AC_ChargeSubGroup = 'STG'
) as StorageCharge ON JH_ParentID = WE_WD AND EC_Value = CONVERT(CHAR(36), WE_PK) -- abs. shite
WHERE ClientPK = @ClientPK
AND WarehousePK = @WarehousePK
AND FinalisedDate > DateAdd(day, -1, @FromDate)
AND FinalisedDate < DateAdd(day, 1, @ToDate)
AND CAST(FinalisedDate AS DATE) >= @FromDate
AND CAST(FinalisedDate AS DATE) < @ToDate

ORDER BY FinalisedDate, Units " + (StorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling ? "DESC " : "ASC "),
						sqlParams);
				}

				return transactions;
			}

			#endregion

			#region GetBalance

			BalanceUnitsTable GetBalance(ZDate date, int freeStorageDays)
			{
				var openingBalance = new DynamicBusinessObjectCollection(Parent.Factory);
				var timeNowAsDateTime = ZDateTime.Now.ToDateTime();
				var clientParameterName = "@ClientPK_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNowAsDateTime, JobStorageSchema.ET_OH_Client, Parent.ET_OH_Client.IsValid ? Parent.ET_OH_Client.ToGuid() : Guid.Empty);
				var warehouseParameterName = "@WarehousePK_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNowAsDateTime, JobStorageSchema.ET_WW, Parent.ET_WW.IsValid ? Parent.ET_WW.ToGuid() : Guid.Empty);
				var dateParameterName = "@Date_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNowAsDateTime, WhsDocketSchema.WD_FinalisedDate, date.ToDateTime());
				var freeStorageDaysParameterName = "@freeStorageDays_" + ParameterSuffixer.Instance.GetParameterSuffix(timeNowAsDateTime, GenericSchema.GenericIntSchemaColumn, freeStorageDays);

				var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
   WarehousePK,
   ClientPK,
   LocationPK,
   ProductPK,
   PartAttrib1,
   PartAttrib2,
   PartAttrib3,
   SerialNumber,
   PalletID,
   SUM(Units) AS Units 
FROM
   (
      SELECT
         ClientPK,
         WarehousePK,
         LocationPK,
         ProductPK,
         PartAttrib1,
         PartAttrib2,
         PartAttrib3,
         SerialNumber,
         PalletID,
         Quantity as Units 
      FROM
         dbo.WhsInvoiceInventory 
      WHERE
         ClientPK = {0} AND
         WarehousePK = {1}
      UNION ALL
      SELECT
         ClientPK,
         WarehousePK,
         LocationPK,
         ProductPK,
         PartAttrib1,
         PartAttrib2,
         PartAttrib3,
         SerialNumber,
         PalletID,
         - Units as Units 
      FROM
         WhsInvoiceTransactions({3}, {2}) 
      WHERE
         ClientPK = {0} AND 
         WarehousePK = {1}
   ) result
GROUP BY
   WarehousePK,
   ClientPK,
   LocationPK,
   ProductPK,
   PartAttrib1,
   PartAttrib2,
   PartAttrib3,
   SerialNumber,
   PalletID
", clientParameterName, warehouseParameterName, dateParameterName, freeStorageDaysParameterName); // SQL query

				var parameters = new ZSqlParameterCollection();
				parameters.Add(clientParameterName, Parent.ET_OH_Client, JobStorageSchema.ET_OH_Client);
				parameters.Add(warehouseParameterName, Parent.ET_WW, JobStorageSchema.ET_WW);
				parameters.Add(dateParameterName, date, WhsDocketLineSchema.WE_ExpiryDate); // Schema column used for Date parameter
				parameters.Add(freeStorageDaysParameterName, freeStorageDays, GenericSchema.GenericIntSchemaColumn);
				openingBalance.Load(sql, parameters);

				var result = new BalanceUnitsTable(StorageCalcMethod);
				foreach (DynamicBusinessObject line in openingBalance)
				{
					var units = (ZDecimal)line["Units"];
					result.AddBalance(LineToUnitsTableKey(line), units);
				}

				return result;
			}

			#endregion

			#region LineToUnitsTableKey

			UnitsTableKey LineToUnitsTableKey(DynamicBusinessObject line)
			{
				return new UnitsTableKey(
					(ZGuid)line["WarehousePK"],
					(ZGuid)line["ClientPK"],
					(ZGuid)line["LocationPK"],
					(ZGuid)line["ProductPK"],
					GetProductAttributesMeasure(line),
					(ZString)line["PalletID"]);
			}

			ProductAttributesMeasure GetProductAttributesMeasure(DynamicBusinessObject obj)
			{
				var result = Array.Empty<string>();
				var client = Parent.Client;
				if (client != null)
				{
					var miscServ = client.MiscServ;
					if (miscServ.OM_IMAttrib1IsKey)
					{
						AddToProductAttributesMeasureList((ZString)obj["PartAttrib1"], ref result, 0);
					}

					if (miscServ.OM_IMAttrib2IsKey)
					{
						AddToProductAttributesMeasureList((ZString)obj["PartAttrib2"], ref result, 1);
					}

					if (miscServ.OM_IMAttrib3IsKey)
					{
						AddToProductAttributesMeasureList((ZString)obj["PartAttrib3"], ref result, 2);
					}

					if (miscServ.OM_IMSerialNumberIsKey)
					{
						AddToProductAttributesMeasureList((ZString)obj["SerialNumber"], ref result, 3);
					}
				}

				return new ProductAttributesMeasure(result);
			}

			void AddToProductAttributesMeasureList(ZString attribute, ref string[] result, int index)
			{
				if (result.Length < index + 1)
				{
					Array.Resize(ref result, index + 1);
				}
				result[index] = attribute;
			}

			#endregion

			#endregion

			#region UnitsPerProduct

			public IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>> UnitsPerProduct => LazyUnitsPerProduct.Value;

			Lazy<IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>>> LazyUnitsPerProduct { get; }

			IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>> GetUnitsPerProduct()
			{
				var result = UnitsPerPeriod.GetUnitsByProduct().Where(e => GetQuantity(e.Value) > 0);

				if (result.Any())
				{
					var productPKs = result.Select(e => e.Key.ProductPK);
					Parent.Factory.AddFetchHint(OrgSupplierPartSchema.Instance, new ZQuery(OrgSupplierPartSchema.PK, productPKs));
					Parent.Factory.AddFetchHint(OrgPartUnitSchema.Instance, new ZQuery(OrgPartUnitSchema.OF_OP, productPKs));
				}

				return result;
			}

			#endregion

			#region UnitsPerProductLocation

			public IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>> UnitsPerProductLocation => LazyUnitsPerProductLocation.Value;

			Lazy<IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>>> LazyUnitsPerProductLocation { get; }

			IEnumerable<KeyValuePair<UnitsTableKey, BalanceUnits>> GetUnitsPerProductLocation()
			{
				var result = UnitsPerPeriod.GetUnitsByProductLocation().Where(e => GetQuantity(e.Value) > 0);

				if (result.Any())
				{
					//fetch all Locations
					var locationPks = result.Select(e => e.Key.LocationPK);
					Parent.Factory.AddFetchHint(WhsLocationViewSchema.Instance, new ZQuery(WhsLocationViewSchema.PK, locationPks));

					var productPKs = result.Select(e => e.Key.ProductPK);
					Parent.Factory.AddFetchHint(OrgSupplierPartSchema.Instance, new ZQuery(OrgSupplierPartSchema.PK, productPKs));
					Parent.Factory.AddFetchHint(OrgPartUnitSchema.Instance, new ZQuery(OrgPartUnitSchema.OF_OP, productPKs));
				}

				return result;
			}

			#endregion
		}

		#endregion

		#region UnitsTableKey class

#if DEBUG
		internal
#endif
		class UnitsTableKey
		{
			public UnitsTableKey(ZGuid warehousePK, ZGuid clientPK, ZGuid locationPK, ZGuid productPK, ProductAttributesMeasure attributes, ZString palletID)
			{
				WarehousePK = warehousePK;
				ClientPK = clientPK;
				LocationPK = locationPK;
				ProductPK = productPK;
				Attributes = attributes;
				PalletID = palletID;
			}

			public readonly ZGuid WarehousePK;
			public readonly ZGuid ClientPK;
			public readonly ZGuid LocationPK;
			public readonly ZGuid ProductPK;
			public readonly ProductAttributesMeasure Attributes;
			public readonly ZString PalletID;
		}

		#endregion

		#region UnitsTableKeyComparerOptions enum

		[Flags]
		enum UnitsTableKeyComparerOptions
		{
			UseProduct = 0,
			UseLocation = 1,
			UseAttributes = 2,
			UsePallletID = 4
		}

		#endregion

		#region UnitsTableKeyComparer class

		class UnitsTableKeyComparer : IEqualityComparer<UnitsTableKey>
		{
			public UnitsTableKeyComparer(UnitsTableKeyComparerOptions options)
			{
				this.options = options;
			}

			readonly UnitsTableKeyComparerOptions options;

			#region IEqualityComparer<UnitsTableKey> Members

			bool IEqualityComparer<UnitsTableKey>.Equals(UnitsTableKey x, UnitsTableKey y)
			{
				return x.WarehousePK == y.WarehousePK && x.ClientPK == y.ClientPK && x.ProductPK == y.ProductPK &&
					((options & UnitsTableKeyComparerOptions.UseAttributes) == 0 || x.Attributes.Equals(y.Attributes)) &&
					((options & UnitsTableKeyComparerOptions.UseLocation) == 0 || x.LocationPK == y.LocationPK) &&
					((options & UnitsTableKeyComparerOptions.UsePallletID) == 0 || x.PalletID == y.PalletID);
			}

			int IEqualityComparer<UnitsTableKey>.GetHashCode(UnitsTableKey obj)
			{
				var result = obj.WarehousePK.GetHashCode() ^ obj.ClientPK.GetHashCode() ^ obj.ProductPK.GetHashCode();

				if ((options & UnitsTableKeyComparerOptions.UseLocation) != 0)
				{
					result ^= obj.LocationPK.GetHashCode();
				}

				if ((options & UnitsTableKeyComparerOptions.UseAttributes) != 0)
				{
					result ^= obj.Attributes.GetHashCode();
				}

				if ((options & UnitsTableKeyComparerOptions.UsePallletID) != 0)
				{
					result ^= obj.PalletID.GetHashCode();
				}

				return result;
			}

			#endregion
		}

		#region BalanceUnits

#if DEBUG
		internal
#endif
		class BalanceUnits
		{
			public BalanceUnits(decimal units)
			{
				currentUnits = units;
				maximumUnits = units;
				minimumUnits = units;
			}

			public void RemoveNegativeValues()
			{
				if (currentUnits < 0m)
				{
					currentUnits = 0m;
				}

				if (maximumUnits < 0m)
				{
					maximumUnits = 0m;
				}

				if (minimumUnits < 0m)
				{
					minimumUnits = 0m;
				}
			}

			public decimal CurrentUnits
			{
				get => currentUnits;
				set
				{
					currentUnits = value;
					maximumUnits = Math.Max(maximumUnits, currentUnits);
					minimumUnits = Math.Min(minimumUnits, currentUnits);
				}
			}

			public decimal MaximumUnits => maximumUnits;

			public decimal MinimumUnits => minimumUnits;

			decimal currentUnits;
			decimal maximumUnits;
			decimal minimumUnits;
		}

		#endregion

		#endregion

		#region BalanceUnitsTable class

#if DEBUG
		internal
#endif
		class BalanceUnitsTable
		{
			public BalanceUnitsTable(string storageCalcMethod)
			{
				var optionsForUnitsByProduct = UnitsTableKeyComparerOptions.UseAttributes;
				var optionsForUnitsByLocations = UnitsTableKeyComparerOptions.UseAttributes | UnitsTableKeyComparerOptions.UseLocation;
				if (storageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
				{
					optionsForUnitsByProduct = UnitsTableKeyComparerOptions.UseProduct;
					optionsForUnitsByLocations = UnitsTableKeyComparerOptions.UseProduct;
				}
				unitsByProduct = new Dictionary<UnitsTableKey, BalanceUnits>(new UnitsTableKeyComparer(optionsForUnitsByProduct));
				unitsByProductLocation = new Dictionary<UnitsTableKey, BalanceUnits>(new UnitsTableKeyComparer(optionsForUnitsByLocations));
				unitsByPalletID = new Dictionary<UnitsTableKey, BalanceUnits>(new UnitsTableKeyComparer(UnitsTableKeyComparerOptions.UsePallletID));
			}

			public void AddBalance(UnitsTableKey key, ZDecimal units)
			{
				AddBalance(unitsByProduct, key, units);
				AddBalance(unitsByProductLocation, key, units);
				AddBalance(unitsByPalletID, key, units);
			}

			public void AddTransaction(UnitsTableKey key, ZDecimal units)
			{
				AddTransaction(unitsByProduct, key, units);
				AddTransaction(unitsByProductLocation, key, units);
			}

			public void AddPalletIDTransaction(UnitsTableKey key, ZDecimal units)
			{
				AddTransaction(unitsByPalletID, key, units);
			}

			public void RemoveNegativeValues()
			{
				RemoveNegativeValues(unitsByProduct);
				RemoveNegativeValues(unitsByProductLocation);
				RemoveNegativeValues(unitsByPalletID);
			}

			public Dictionary<UnitsTableKey, BalanceUnits> GetUnitsByProduct()
			{
				return unitsByProduct;
			}

			public Dictionary<UnitsTableKey, BalanceUnits> GetUnitsByProductLocation()
			{
				return unitsByProductLocation;
			}

			public Dictionary<UnitsTableKey, BalanceUnits> GetUnitsByPalletID()
			{
				return unitsByPalletID;
			}

			#region Implementation

			static void AddBalance(Dictionary<UnitsTableKey, BalanceUnits> dictionary, UnitsTableKey key, ZDecimal units)
			{
				BalanceUnits balanceUnits;
				if (dictionary.TryGetValue(key, out balanceUnits))
				{
					dictionary[key] = new BalanceUnits(balanceUnits.CurrentUnits + units);
				}
				else
				{
					dictionary.Add(key, new BalanceUnits(units));
				}
			}

			static void AddTransaction(Dictionary<UnitsTableKey, BalanceUnits> dictionary, UnitsTableKey key, ZDecimal units)
			{
				BalanceUnits balanceUnits;
				if (dictionary.TryGetValue(key, out balanceUnits))
				{
					balanceUnits.CurrentUnits += units;
				}
				else
				{
					dictionary.Add(key, new BalanceUnits(units));
				}
			}

			static void RemoveNegativeValues(Dictionary<UnitsTableKey, BalanceUnits> dictionary)
			{
				foreach (KeyValuePair<UnitsTableKey, BalanceUnits> kv in dictionary)
				{
					kv.Value.RemoveNegativeValues();
				}
			}

			readonly Dictionary<UnitsTableKey, BalanceUnits> unitsByProduct;
			readonly Dictionary<UnitsTableKey, BalanceUnits> unitsByProductLocation;
			readonly Dictionary<UnitsTableKey, BalanceUnits> unitsByPalletID;

			#endregion
		}

		#endregion

		#endregion

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return true;
		}

		public override Directions JobDirection
		{
			get
			{
				Directions result;

				var invoicingSupporter = Parent.InvoicingSupporter;
				if (invoicingSupporter.IsImport)
				{
					result = Directions.Import;
				}
				else if (invoicingSupporter.IsExport)
				{
					result = Directions.Export;
				}
				else if (invoicingSupporter.IsDomestic)
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

		ZGuid IAutoRatingWarehouseInfo.WarehousePK => Parent.ET_WW;

		OrgHeader IAutoRatingWarehouseInfo.WarehouseFallbackConsignorForFilterOnly => null;

		#endregion
	}
}
