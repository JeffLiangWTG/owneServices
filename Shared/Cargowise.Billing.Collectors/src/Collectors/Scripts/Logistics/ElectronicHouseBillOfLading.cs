using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public abstract class ElectronicHouseBillOfLading : RefStlScriptWithDefaults
	{
		public override string RoleName => "Forwarding Shipment";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Electronic House Bill Of Lading";
		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string MinCW1Version => "24.8.8.51";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "OriginalBillSentForPublicationEventBranch";

		public override string TransactionDateUtc => "FilteredBLULog.SL_PostedTimeUtc";
		public override string GuidReference => "FilteredBLULog.SL_PK";

		public override string TransactionCount => "1";

		public override string BillingReference1 => "Shipment.JS_UniqueConsignRef";
		public override string BillingReference2 => "Shipment.JS_HouseBill";
		public override string BillingReference3 => "IIF(FirstHolder.OH_FullName IS NOT NULL, CONCAT(FirstHolder.OH_Code, ' - ', FirstHolder.OH_FullName), FirstHolder.OH_Code)";
		public override string BillingReference4 => @"CASE
	WHEN Shipment.JS_ElectronicBillOfLadingTerms = 'TRA' THEN 'TRA-Transferable'
	WHEN Shipment.JS_ElectronicBillOfLadingTerms = 'NTR' THEN 'NTR-Non-Transferable'
	ELSE ''
END";

		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			ShipmentType = Shipment.JS_ShipmentType,
			ContainerMode = Shipment.JS_PackingMode,
			FirstHolder = IIF(FirstHolder.OH_FullName IS NOT NULL, CONCAT(FirstHolder.OH_Code, ' - ', FirstHolder.OH_FullName), FirstHolder.OH_Code),
			BillNumber = CONCAT(Shipment.JS_HouseBill, '/', Shipment.JS_ElectronicBillOfLadingVersion),
			BillType = Shipment.JS_ElectronicBillOfLadingType,
			BillTerms = Shipment.JS_ElectronicBillOfLadingTerms,
			eBillIdentifier = Shipment.JS_ElectronicBillOfLadingReference,
			PlaceOfReceipt = Shipment.JS_RL_NKOrigin,
			PlaceOfDelivery = Shipment.JS_RL_NKDestination
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)
))";

		public override string PreparationScript => $@"WITH BLULog AS
(
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		SL_PK,
		ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PostedTimeUtc ASC) AS RowNum
	FROM
		dbo.StmALog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'BLU'
		AND SL_Table = 'JobShipment'
		AND SL_Reference LIKE '%DEP=Title Registry%'
		AND SL_Reference LIKE '%TYP={EventReferenceType}%'
),
FilteredBLULog AS
(
	SELECT
		blulog.SL_Parent,
		blulog.SL_PostedTimeUtc,
		blulog.SL_PK,
		OriginalBillSentForPublicationEventBranch =
		(
			SELECT TOP 1
				originalBillSentForPublicationEvent.SL_GB_NKBranch
			FROM
				dbo.StmALog originalBillSentForPublicationEvent
			WHERE
				originalBillSentForPublicationEvent.SL_Parent = blulog.SL_Parent
				AND originalBillSentForPublicationEvent.SL_PostedTimeUtc >= DATEADD(MONTH, -1, blulog.SL_PostedTimeUtc)
				AND originalBillSentForPublicationEvent.SL_PostedTimeUtc < blulog.SL_PostedTimeUtc
				AND originalBillSentForPublicationEvent.SL_SE_NKEvent = 'MSN'
				AND originalBillSentForPublicationEvent.SL_Table = 'JobShipment'
				AND originalBillSentForPublicationEvent.SL_Reference LIKE '%DEP=Title Registry%'
				AND originalBillSentForPublicationEvent.SL_Reference LIKE '%TYP=Original Bill Sent for Publication%'
			ORDER BY
				originalBillSentForPublicationEvent.SL_PostedTimeUtc DESC
		),
		FirstHolderAddress = 
		(
			SELECT TOP 1
				JobDocAddress.E2_OA_Address
			FROM
				dbo.JobDocAddress
			WHERE
				JobDocAddress.E2_ParentID = blulog.SL_Parent
				AND JobDocAddress.E2_ParentTableCode = 'JS'
				AND JobDocAddress.E2_AddressType = 'HLD'
				AND JobDocAddress.E2_AddressOverride = 0
			ORDER BY
				JobDocAddress.E2_AddressSequence ASC
		) 
	FROM
		BLULog blulog
	WHERE
		RowNum = 1
		AND NOT EXISTS (
			SELECT 1
			FROM
				dbo.StmALog
			WHERE
				SL_Parent = blulog.SL_Parent
				AND SL_SE_NKEvent = 'BLU'
				AND SL_Table = 'JobShipment'
				AND SL_Reference LIKE '%DEP=Title Registry%'
				AND SL_Reference LIKE '%TYP={EventReferenceType}%'
				AND SL_PostedTimeUtc < blulog.SL_PostedTimeUtc
			)
)";
		public override string FromClause => @"FilteredBLULog
LEFT JOIN dbo.JobShipment Shipment ON Shipment.JS_PK = FilteredBLULog.SL_Parent
LEFT JOIN dbo.OrgAddress FirstHolderAddr ON FirstHolderAddr.OA_PK = FilteredBLULog.FirstHolderAddress
LEFT JOIN dbo.OrgHeader FirstHolder ON FirstHolder.OH_PK = FirstHolderAddr.OA_OH
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = FilteredBLULog.OriginalBillSentForPublicationEventBranch
LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;

		public override string ActiveOn => "ALL";

		public abstract string EventReferenceType { get; }
	}

	#endregion
}
