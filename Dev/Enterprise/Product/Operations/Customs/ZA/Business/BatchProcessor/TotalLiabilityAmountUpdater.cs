using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Calling complex SQL")]
	public class TotalLiabilityAmountUpdater
	{
		public TotalLiabilityAmountUpdater(ILogger logger)
		{
			this.logger = logger;
			try
			{
				dutyAndTaxCalculator = GetDutyAndTaxCalculator();
			}
			catch (NotImplementedException)
			{
				logger.Error($"Unable to load WhsInventoryDutyAndTaxCalculator for {CountryCodes.SouthAfrica}");
			}
		}

		readonly ILogger logger;
		readonly IWhsInventoryDutyAndTaxCalculator dutyAndTaxCalculator;

		const int BatchSize = 5000;

		public void Process(CancellationToken token)
		{
			var valuationDate = ZDateTime.Now;
			var processedRow = 0;
			var isDone = false;
			while (!isDone && !token.IsCancellationRequested)
			{
				var sourceTable = LoadSourceTable();
				ProcessBatch(sourceTable, valuationDate);
				var rowCount = sourceTable.Rows.Count;
				processedRow += rowCount;
				isDone = rowCount != BatchSize;
				logger.Information($"Updated {processedRow} rows");
			}
		}

		DataTable LoadSourceTable()
		{
			var warehouseAttributeSql = $@"
SELECT TOP {BatchSize} WB_PK                                            AS PK,
                ISNULL(WE_AdjustmentArrivalDate, WD_ArrivalDate) AS ArrivalDate,
                WB_Tariff                                        AS CustomsTariffCode,
                WB_RN_NKCountryOfOrigin                          AS CountryOfOrigin,
                WB_CustomsQty                                    AS CustomsQty,
                WB_ValueForDuty                                  AS CustomsValue,
                WB_CustomsUnitOfQty                              AS CustomsUnitOfQty,
                WB_CustomsSecondQuantity                         AS CustomsSecondQuantity,
                WB_CustomsSecondUnitQty                          AS CustomsSecondUnitQty,
                WB_CustomsThirdQuantity                          AS CustomsThirdQuantity,
                WB_CustomsThirdUnitQty                           AS CustomsThirdUnitQty
FROM   dbo.WhsDocketLine
       INNER JOIN dbo.WhsDocket
               ON WD_PK = WE_WD
       INNER JOIN dbo.WhsBondedWarehouseAttribute
               ON WB_ParentID = WE_PK
       INNER JOIN dbo.WhsWarehouse
               ON WD_WW_Whs = WW_PK
       INNER JOIN dbo.OrgAddress
               ON OA_PK = WW_OA_WarehouseAddress
WHERE  OA_RN_NKCountryCode = '{CountryCodes.SouthAfrica}'
       AND (WD_DocketType = 'INW'
             OR (WE_DocketLineStatus = 'FIN'
                 AND ((WD_DocketType = 'ADJ' AND WE_TransactionQuantity > 0)
                       OR (WD_DocketType = 'TFR' AND WD_DocketSubType <> 'IWS'))))
       AND (WB_AllDutiesAmount IS NULL OR WB_VATAmount IS NULL
             OR WB_SystemLastEditTimeUtc <= CONVERT(SMALLDATETIME, DATEADD(HOUR, -1, GETUTCDATE())));";

			var result = new DataTable();
			using (var cmd = Db.Connection.Command(warehouseAttributeSql))
			{
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(result);
				}
			}
			return result;
		}

		void ProcessBatch(DataTable sourceTable, ZDateTime valuationDate)
		{
			foreach (DataRow row in sourceTable.Rows)
			{
				var pk = (Guid)row["PK"];
				var (allDuties, vat) = CalculateTotalLiabilityAmount(dutyAndTaxCalculator, row, valuationDate);
				UpdateTotalLiabilityAmount(pk, allDuties, vat);
			}
		}

		IWhsInventoryDutyAndTaxCalculator GetDutyAndTaxCalculator()
		{
			var dutyAndTaxCalculatorProvider = ObjectFactory.Get<IWhsInventoryDutyAndTaxCalculatorProvider>();
			return dutyAndTaxCalculatorProvider.GetProviderFor(new BusinessObjectFactory(), CountryCodes.SouthAfrica);
		}

		(decimal allDuties, decimal vat) CalculateTotalLiabilityAmount(IWhsInventoryDutyAndTaxCalculator dutyAndTaxCalculator, DataRow row, ZDateTime valuationDate)
		{
			var arrivalDate = new ZDateTimeOffset(row["ArrivalDate"]).ToZDateTime();
			var customsTariffCode = Convert.ToString(row["CustomsTariffCode"]);
			var countryOfOrigin = Convert.ToString(row["CountryOfOrigin"]);
			var customsQty = Convert.ToDecimal(row["CustomsQty"]);
			var customsValue = Convert.ToDecimal(row["CustomsValue"]);
			var customsUnitOfQty = Convert.ToString(row["CustomsUnitOfQty"]);
			var customsSecondQuantity = Convert.ToDecimal(row["CustomsSecondQuantity"]);
			var customsSecondUnitQty = Convert.ToString(row["CustomsSecondUnitQty"]);
			var customsThirdQuantity = Convert.ToDecimal(row["CustomsThirdQuantity"]);
			var customsThirdUnitQty = Convert.ToString(row["CustomsThirdUnitQty"]);

			var results = new Dictionary<ZString, IZType>();
			dutyAndTaxCalculator.Calculate(results, ratio: 1, arrivalDate, valuationDate, customsTariffCode, countryOfOrigin, customsValue,
				customsQty, customsUnitOfQty, customsSecondQuantity, customsSecondUnitQty, customsThirdQuantity, customsThirdUnitQty);
			return (GetDecimalValue(results[CusEntryPayTypes.AllDuties]), GetDecimalValue(results[CusEntryPayTypes.ValueAddedTax]));
		}

		decimal GetDecimalValue(IZType value) => value is ZDecimal @decimal ? @decimal : 0m;

		void UpdateTotalLiabilityAmount(Guid pk, decimal allDuties, decimal vat)
		{
			var updateSql = @"
UPDATE dbo.WhsBondedWarehouseAttribute
SET    WB_AllDutiesAmount = @AllDutiesAmount,
       WB_VATAmount = @VATAmount,
       WB_SystemLastEditUser = '~BP',
       WB_SystemLastEditTimeUtc = GETUTCDATE()
WHERE  WB_PK = @PK;";
			using (var cmd = Db.Connection.Command(updateSql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@AllDutiesAmount", SqlDbType.Decimal, allDuties);
				cmd.AddParameter("@VATAmount", SqlDbType.Decimal, vat);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
