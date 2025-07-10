using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ReceiptProcessor
	{
		public ReceiptProcessor(BusinessObjectFactory factory, GlbCompany company, LoggingInformation logger)
		{
			this.factory = factory;
			this.logger = logger;
			this.company = company;
		}

		public void ProcessReceipts()
		{
			underReceipts = new List<UnderReceipt>();
			using (var transactionManager = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				ProcessQueuedReceipts();

				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}

				transactionManager.CommitTransaction();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Calling complex SQL")]
		void ProcessQueuedReceipts()
		{
			using (var dbCommand = Db.Connection.Command(ProcessReceiptsSQL))
			{
				var wotMatchingPeriod = ZACustomsRegistry.Instance.WOTReceiptsMatchingPeriod.Value;
				dbCommand.CommandTimeout = 0;
				dbCommand.AddParameter("Company", SqlDbType.UniqueIdentifier, company.PK.ToGuid());
				dbCommand.AddParameter("CreateDate", SqlDbType.DateTime, wotMatchingPeriod == 0 ? DBNull.Value : ZDateTime.UtcNow.AddDays(-wotMatchingPeriod));
				dbCommand.AddParameter("LastEditDate", SqlDbType.DateTime, ZDateTime.UtcNow);
				dbCommand.AddParameter("LastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());

				using (var reader = dbCommand.ExecuteReader())
				{
					_ = reader.Read();
					var warehouseCount = (int)reader["WarehouseMatchCount"];
					var customsCount = (int)reader["CustomsMatchCount"];
					var processedCount = (int)reader["ReceiptsProcessed"];

					_ = reader.NextResult();
					while (reader.Read())
					{
						var underReceiptPK = (Guid)reader[CusWHSOperatorTransactionSchema.Constants.PK];

						underReceipts.Add(new UnderReceipt(factory)
						{
							ReceiptPK = underReceiptPK,
							WOB_PK = (Guid)reader[CusWHSOperatorTransactionBatchSchema.Constants.PK],
							WOB_GC_Company = (Guid)reader[CusWHSOperatorTransactionBatchSchema.Constants.WOB_GC_Company],
							WOB_OA_Warehouse = (Guid)reader[CusWHSOperatorTransactionBatchSchema.Constants.WOB_OA_Warehouse],
							WOT_OP_Product = (Guid)reader[CusWHSOperatorTransactionSchema.Constants.WOT_OP_Product],
							WOT_CustomsEntryNumber = (string)reader[CusWHSOperatorTransactionSchema.Constants.WOT_CustomsEntryNumber],
							BondedQuantity = (decimal)reader["BondedQty"],
							ReceiptQuantity = (decimal)reader["TotalReceipt"]
						});
					}

					logger.Log(string.Format(CultureInfo.CurrentCulture, "{0} Receipt(s) matched to warehouse entries", warehouseCount));
					logger.Log(string.Format(CultureInfo.CurrentCulture, "{0} Receipt(s) matched to customs entries", customsCount));
					logger.Log(string.Format(CultureInfo.CurrentCulture, "{0} Receipt(s) processed", processedCount));
				}
			}
			logger.Log(string.Format(CultureInfo.CurrentCulture, "Processing Under Receipts"));
			CreateUnderReceipts();
		}

		void CreateUnderReceipts()
		{
			if (underReceipts.Any())
			{
				var groupedUnderReceipts = underReceipts.GroupBy(x => new { x.WOB_PK, x.WOB_GC_Company, x.WOB_OA_Warehouse, x.WOT_OP_Product, x.WOT_CustomsEntryNumber }, (key, group) => new
				{
					key.WOB_PK,
					Result = group.ToList()
				});

				foreach (var group in groupedUnderReceipts)
				{
					CreateAdjustmentBatch(group.WOB_PK, group.Result.FirstOrDefault());
				}
			}
		}

		void CreateAdjustmentBatch(Guid batchPK, UnderReceipt underReceipt)
		{
			var batch = factory.Load<CusWHSOperatorTransactionBatch>(batchPK);
			logger.Log(string.Format(CultureInfo.CurrentCulture, "Creating system batch for under receipts from Batch: {0}", batch?.WOB_Batch));
			var newBatch = batch?.Clone() as CusWHSOperatorTransactionBatch;
			newBatch.WOB_Batch = "Under Receipt - " + newBatch.WOB_Batch.Substring(0, 45);
			newBatch.WOB_IsSystemCreated = true;
			CreateAdjustment(newBatch, underReceipt);
		}

		void CreateAdjustment(CusWHSOperatorTransactionBatch newBatch, UnderReceipt underReceipt)
		{
			logger.Log(string.Format(CultureInfo.CurrentCulture, "Creating under receipt for Owner Ref: {0} Product: {1}", underReceipt.Receipt.WOT_OwnerReference, underReceipt.Receipt.Product.OP_PartNum));
			var adjustment = underReceipt.Receipt.Clone() as CusWHSOperatorTransaction;
			adjustment.WOT_WOB_CusWHSTransactionBatch = newBatch.PK;
			adjustment.WOT_TransactionDate = ZDateTime.UtcToday.Date;
			adjustment.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ADJ;
			adjustment.WOT_IsCustomsControlled = true;
			adjustment.WOT_Quantity = underReceipt.BondedQuantity - underReceipt.ReceiptQuantity;
			adjustment.WOT_IsFinal = false;
			adjustment.WOT_TotalValue = 0;
			adjustment.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			adjustment.WOT_RN_NKOrigin = ZString.Empty;
		}

		protected readonly LoggingInformation logger;
		readonly GlbCompany company;
		protected readonly BusinessObjectFactory factory;
		List<UnderReceipt> underReceipts;

		internal const string ProcessReceiptsSQL = @"DECLARE @ReceiptsMatchedToWarehouseEntry INT;
DECLARE @ReceiptsMatchedToCustomsEntry INT;
DECLARE @ReceiptsProcessed INT;

SELECT DISTINCT
	CusWHSOperatorTransaction.WOT_PK,
	WB_RN_NKCountryOfOrigin AS Origin,
	WB_EntryKey AS EntryNumber,
	CASE WHEN LEN(WB_EntryKey) >= 11 AND ISDATE(SUBSTRING(WB_EntryKey, 4, 8)) = 1 THEN CONVERT(DATETIME, SUBSTRING(WB_EntryKey, 4, 8), 103) ELSE NULL END AS IntoBondDate,
	WB_SystemCreateTimeUtc AS SystemDate
INTO #WarehouseMatches
FROM dbo.CusWHSOperatorTransaction
JOIN dbo.CusWHSOperatorTransactionBatch ON WOB_PK = WOT_WOB_CusWHSTransactionBatch
JOIN dbo.WhsDocket ON WD_OH_Client = WOT_OH_ProductOwner
JOIN dbo.WhsWarehouse ON WW_PK = WD_WW_Whs AND WW_OA_WarehouseAddress = WOB_OA_Warehouse
JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
JOIN dbo.OrgSupplierPart ON  WE_OP = OP_PK AND WOT_OP_Product = OP_PK
JOIN dbo.WhsBondedWarehouseAttribute ON WB_ParentID = WE_PK
JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
WHERE
	WOT_TransactionType = 'REC'
	AND WOT_Status = 'QUE'
	AND WB_MatchingKey <> ''
	AND	WB_MatchingKey >= WOT_OwnerReference
	AND WB_MatchingKey LIKE WOT_OwnerReference + '%'
	AND GB_GC = @Company
	AND (@CreateDate IS NULL OR WB_SystemCreateTimeUtc >= @CreateDate)
	AND	(dbo.WhsDocket.WD_DocketType = 'INW'
		 OR (dbo.WhsDocket.WD_DocketType = 'ADJ' AND dbo.WhsDocketLine.WE_DocketLineStatus = 'FIN' AND dbo.WhsDocketLine.WE_TransactionQuantity > 0)
		 OR (dbo.WhsDocket.WD_DocketType = 'TFR' AND dbo.WhsDocketLine.WE_DocketLineStatus = 'FIN' AND dbo.WhsDocket.WD_DocketSubType <> 'IWS'))
	AND WB_RN_NKCountryOfOrigin <> ''
	AND WB_EntryKey <> '<PendingCustomsResponse>';

SELECT DISTINCT
	CusWHSOperatorTransaction.WOT_PK,
	JI_CountryOfOrigin AS Origin,
	ISNULL(CE_EntryNum, '') AS EntryNumber,
	CH_EntryReleaseDate     AS SystemDate
INTO #CustomsMatches
FROM dbo.CusWHSOperatorTransaction
JOIN dbo.JobComInvoiceLine ON JI_OP = WOT_OP_Product AND JI_MatchingKey LIKE WOT_OwnerReference + '%'
JOIN dbo.CusEntryLine ON CL_ClusterKey = JI_ClusterKey AND CL_PK = JI_CL
JOIN dbo.CusEntryHeader ON CH_ClusterKey = JI_ClusterKey AND CH_PK = CL_CH
LEFT OUTER JOIN dbo.CusEntryNum ON CE_ParentID = CH_PK AND CE_EntryType = 'MRN' AND CE_RN_NKCountryCode = 'ZA'
JOIN dbo.JobDeclaration ON JE_ClusterKey = CH_ClusterKey
WHERE
	WOT_TransactionType = 'REC'
	AND WOT_Status = 'QUE'
	AND JE_GC = @Company
	AND JI_CountryOfOrigin <> ''
	AND (@CreateDate IS NULL OR CH_EntryReleaseDate >= @CreateDate)
	AND JI_Procedure <> '4000';

SELECT
	WOT_PK,
	IntoBondDate,
	TranDate,
	Origin,
	IsCustomsControlled,
	EntryNumber
INTO #LatestMatches
FROM
(
	SELECT
		WOT_PK,
		IntoBondDate,
		TranDate,
		Origin,
		IsCustomsControlled,
		EntryNumber,
		ROW_NUMBER() OVER (PARTITION BY WOT_PK ORDER BY TranDate DESC) RN
	FROM
	(
		SELECT
			WOT_PK,
			IntoBondDate,
			SystemDate AS TranDate,
			Origin,
			1 AS IsCustomsControlled,
			EntryNumber
		FROM #WarehouseMatches
		UNION ALL
		SELECT
			WOT_PK,
			NULL AS IntoBondDate,
			SystemDate AS TranDate,
			Origin,
			0 AS IsCustomsControlled,
			EntryNumber
		FROM #CustomsMatches
	) AllMatches
) OrderedMatches
WHERE RN = 1;

UPDATE dbo.CusWHSOperatorTransaction
SET 
	WOT_Status = 'VAL',
	WOT_IsCustomsControlled = CASE WHEN #LatestMatches.IsCustomsControlled IS NULL THEN 0 ELSE #LatestMatches.IsCustomsControlled END,
	WOT_RN_NKOrigin = CASE WHEN #LatestMatches.Origin IS NULL THEN 'ZA' ELSE #LatestMatches.Origin END,
	WOT_CustomsEntryNumber = CASE WHEN #LatestMatches.EntryNumber IS NULL THEN '' ELSE #LatestMatches.EntryNumber END,
	WOT_IntoBondDate = CASE WHEN #LatestMatches.IntoBondDate IS NULL THEN NULL ELSE #LatestMatches.IntoBondDate END, 
	WOT_SystemLastEditTimeUtc = @LastEditDate,
	WOT_SystemLastEditUser = @LastEditUser
FROM dbo.CusWHSOperatorTransaction
JOIN dbo.CusWhsOperatorTransactionBatch ON WOB_PK = WOT_WOB_CusWHSTransactionBatch
LEFT JOIN #LatestMatches ON #LatestMatches.WOT_PK = CusWHSOperatorTransaction.WOT_PK
WHERE
	WOB_GC_Company = @Company
	AND WOT_TransactionType = 'REC'
	AND WOT_Status = 'QUE';

SET @ReceiptsProcessed = @@RowCount;
SET @ReceiptsMatchedToWarehouseEntry = (SELECT COUNT(1) AS WarehouseCount
                                        FROM   #LatestMatches
                                        WHERE  IsCustomsControlled = 1);
SET @ReceiptsMatchedToCustomsEntry = (SELECT COUNT(1) AS CustomsCount
                                      FROM   #LatestMatches
                                      WHERE  IsCustomsControlled = 0);

SELECT 
	@ReceiptsMatchedToWarehouseEntry AS WarehouseMatchCount,
	@ReceiptsMatchedToCustomsEntry AS CustomsMatchCount,
	@ReceiptsProcessed AS ReceiptsProcessed;

SELECT
	Matches.WOT_PK,
	Matches.WOB_PK,
	Matches.WOB_GC_Company,
	Matches.WOB_OA_Warehouse,
	Matches.WOT_OP_Product,
	Matches.WOT_CustomsEntryNumber,
	BondedQty,
	TotalReceipt
FROM
(
	SELECT 
		CusWHSOperatorTransaction.WOT_PK,
		WOB_PK,
		WOB_GC_Company,
		WOB_OA_Warehouse,
		WOT_OP_Product,
		WOT_CustomsEntryNumber
	FROM #WarehouseMatches AS WarehouseMatch
	JOIN #LatestMatches ON #LatestMatches.WOT_PK = WarehouseMatch.WOT_PK
	JOIN dbo.CusWHSOperatorTransaction ON CusWHSOperatorTransaction.WOT_PK = WarehouseMatch.WOT_PK
	JOIN dbo.CusWHSOperatorTransactionBatch ON WOB_PK = WOT_WOB_CusWHSTransactionBatch
	GROUP BY
		CusWHSOperatorTransaction.WOT_PK,
		WOB_PK,
		WOB_GC_Company,
		WOB_OA_Warehouse,
		WOT_OP_Product,
		WOT_CustomsEntryNumber
) Matches
JOIN
(
	SELECT 
		GB_GC,
		WW.WW_OA_WarehouseAddress,
		OP.OP_PK,
		WB.WB_EntryKey,
		SUM(WE_TransactionQuantity) AS BondedQty
	FROM dbo.WhsWarehouse WW
	JOIN dbo.WhsDocket WD ON WD.WD_WW_Whs = WW.WW_PK
	JOIN dbo.WhsDocketLine WE ON WE.WE_WD = WD.WD_PK
	JOIN dbo.WhsBondedWarehouseAttribute WB ON WB.WB_ParentID = WE.WE_PK
	JOIN dbo.OrgSupplierPart OP ON OP.OP_PK = WE.WE_OP
	JOIN dbo.GlbBranch GB ON GB.GB_PK = WW.WW_GB_RelatedCompanyBranch
	WHERE 
		GB.GB_GC = @Company
		AND (WD.WD_DocketType = 'INW'
			 OR (WD.WD_DocketType = 'ADJ' AND WE.WE_DocketLineStatus = 'FIN'))
	GROUP BY
		GB_GC,
		WW.WW_OA_WarehouseAddress,
		OP.OP_PK,
		WB.WB_EntryKey
) BondstoreStock ON BondstoreStock.GB_GC = Matches.WOB_GC_Company
				AND BondstoreStock.WW_OA_WarehouseAddress = Matches.WOB_OA_Warehouse
				AND BondstoreStock.OP_PK = Matches.WOT_OP_Product
				AND BondstoreStock.WB_EntryKey = Matches.WOT_CustomsEntryNumber
JOIN
(
	SELECT
		WOB_GC_Company,
		WOB_OA_Warehouse,
		WOT.WOT_OP_Product,
		WOT.WOT_CustomsEntryNumber,
		Sum(WOT_Quantity) as TotalReceipt
	FROM dbo.CusWHSOperatorTransactionBatch WOB
	JOIN dbo.CusWHSOperatorTransaction WOT ON WOT.WOT_WOB_CusWHSTransactionBatch = WOB.WOB_PK
	WHERE
		WOB_GC_Company = @Company
		AND WOT_TransactionType IN ('REC','ADJ')
	GROUP BY
		WOB_GC_Company,
		WOB_OA_Warehouse,
		WOT.WOT_OP_Product,
		WOT.WOT_CustomsEntryNumber
	HAVING 
		MAX(CONVERT(INT, WOT.WOT_IsFinal)) = 1
) RecSummary ON RecSummary.WOB_GC_Company = Matches.WOB_GC_Company
				AND RecSummary.WOB_OA_Warehouse = Matches.WOB_OA_Warehouse
				AND RecSummary.WOT_OP_Product = Matches.WOT_OP_Product
				AND RecSummary.WOT_CustomsEntryNumber = Matches.WOT_CustomsEntryNumber
WHERE
	BondedQty > TotalReceipt;

DROP TABLE #WarehouseMatches;
DROP TABLE #CustomsMatches;
DROP TABLE #LatestMatches;";

		class UnderReceipt
		{
			public UnderReceipt(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public Guid ReceiptPK { get; set; }
			public Guid WOB_PK { get; set; }
			public Guid WOB_GC_Company { get; set; }
			public Guid WOB_OA_Warehouse { get; set; }
			public Guid WOT_OP_Product { get; set; }
			public ZString WOT_CustomsEntryNumber { get; set; }
			public ZDecimal ReceiptQuantity { get; set; }
			public ZDecimal BondedQuantity { get; set; }
			public CusWHSOperatorTransaction Receipt => receipt ?? (receipt = factory.Load<CusWHSOperatorTransaction>(ReceiptPK));
			CusWHSOperatorTransaction receipt;
			readonly BusinessObjectFactory factory;
		}
	}
}
