using System;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public abstract class ElectronicBillLading : RefStlScriptWithDefaults
	{
		public override string RoleName => "Forwarding Consol";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Electronic Bill Of Lading";
		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string MinCW1Version => "23.11.9.472";

		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "ConsolCreatedBranch";

		public override string TransactionDateUtc => "FilteredBLULog.SL_PostedTimeUtc";
		public override string GuidReference => "FilteredBLULog.SL_PK";

		public override string BillingReference1 => "JK_UniqueConsignRef";
		public override string BillingReference2 => "IIF(JK_AgentType = 'CLD', JK_CoLoadMasterBill, JK_MasterBillNum)";
		public override string BillingReference3 => "IIF(JK_AgentType = 'CLD'" +
			", rslCoLoadWith.RSL_CargoWiseOneCode + ' [' + rslCoLoadWith.RSL_StandardCarrierAlphaCode + ']'" +
			", rslCarrier.RSL_CargoWiseOneCode + ' [' + rslCarrier.RSL_StandardCarrierAlphaCode + ']')";
		public override string BillingReference4 => @"CASE
			WHEN JK_ElectronicBillOfLadingTerms = 'TRA' THEN 'TRA-Transferable'
			WHEN JK_ElectronicBillOfLadingTerms = 'NTR' THEN 'NTR-Non-Transferable'
			ELSE ''
		END";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
					(SELECT
						ConsolType = JK_AgentType,
						ContainerMode = JK_ConsolMode,
						CarrierName = IIF(JK_AgentType = 'CLD', CoLoadWith.OH_FullName, Carrier.OH_FullName),
						CarrierBookingReference = IIF(JK_AgentType = 'CLD', JK_CoLoadBookingReference, JK_BookingReference),
						BillType = JK_ElectronicBillOfLadingType,
						BillTerms = JK_ElectronicBillOfLadingTerms,
						eBillIdentifier = JK_ElectronicBillOfLadingReference,
						PlaceOfReceipt = JK_RL_NKLoadPort,
						PlaceOfDelivery = JK_RL_NKDischargePort
					FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string PreparationScript => $@"WITH BLULog AS
(
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		SL_PK,
		ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PostedTimeUtc ASC) AS RowNum
	FROM StmALog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'BLU'
		AND SL_Table = 'JobConsol'
		AND SL_Reference LIKE '%DEP=Carrier%'
		AND SL_Reference LIKE '%TYP={EventReferenceType}%'
		{(string.IsNullOrEmpty(EventReferenceOld) ? string.Empty : "AND SL_Reference LIKE '%OLD=" + EventReferenceOld + "%'")}
), FilteredBLULog AS
(
	SELECT
		blulog.SL_Parent,
		blulog.SL_PostedTimeUtc,
		blulog.SL_PK,
		ConsolCreatedBranch =
		(
			SELECT
				TOP 1 addedARecordToTheSystemLog.SL_GB_NKBranch
			FROM
				StmALog addedARecordToTheSystemLog
			WHERE
				addedARecordToTheSystemLog.SL_Parent = blulog.SL_Parent
				AND addedARecordToTheSystemLog.SL_SE_NKEvent = 'ADD'
				AND addedARecordToTheSystemLog.SL_Table = 'JobConsol'
			ORDER BY
				addedARecordToTheSystemLog.SL_PostedTimeUtc
		)
	FROM
		BLULog blulog
	WHERE
		RowNum = 1
		AND NOT EXISTS (
			SELECT 1 FROM StmALog
			WHERE
				SL_Parent = blulog.SL_Parent
				AND SL_SE_NKEvent = 'BLU'
				AND SL_Table = 'JobConsol'
				AND SL_Reference LIKE '%DEP=Carrier%'
				AND SL_Reference LIKE '%TYP={EventReferenceType}%'
				{(string.IsNullOrEmpty(EventReferenceOld) ? string.Empty : "AND SL_Reference LIKE '%OLD=" + EventReferenceOld + "%'")}
				AND SL_PostedTimeUtc < blulog.SL_PostedTimeUtc
			)
)";
		public override string FromClause => @"FilteredBLULog
LEFT JOIN JobConsol ON JK_PK = FilteredBLULog.SL_Parent
LEFT JOIN dbo.OrgAddress CarrierAddr ON CarrierAddr.OA_PK = JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader Carrier ON  Carrier.OH_PK = CarrierAddr.OA_OH
LEFT JOIN dbo.OrgAddress CoLoadWithAddr ON CoLoadWithAddr.OA_PK = JK_OA_CreditorAddress
LEFT JOIN dbo.OrgHeader CoLoadWith ON  CoLoadWith.OH_PK = CoLoadWithAddr.OA_OH
LEFT JOIN dbo.RefShippingLine rslCarrier ON Carrier.OH_RSL_ShippingLine = rslCarrier.RSL_PK
LEFT JOIN dbo.RefShippingLine rslCoLoadWith ON CoLoadWith.OH_RSL_ShippingLine = rslCoLoadWith.RSL_PK
LEFT JOIN dbo.GlbBranch Branch ON GB_Code = ConsolCreatedBranch
LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";
		public override string WhereClause => string.Empty;

		public override string ActiveOn => "ALL";

		public virtual string EventReferenceType => throw new NotImplementedException();

		public virtual string EventReferenceOld => string.Empty;

	}

	#endregion
}
