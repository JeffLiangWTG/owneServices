namespace CargoWise.Billing.Collectors.Logistics
{
	sealed public class ForwardingPortMessagingAirPAS : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PAS";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging Air";
		public override string FeatureName => "Port Status - Exp Notification(755)";
		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "STUAndMAALogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "STUAndMAALogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "SL_UniqueConsignRef";
		public override string BillingReference2 => "CONCAT(LOC, ' - ', MST)";
		public override string BillingReference3 => "CASE WHEN TYP IS NULL OR TYP = '' THEN SL_SE_NKEvent ELSE CONCAT(SL_SE_NKEvent, ' - ', TYP) END";
		public override string BillingReference4 => @"CASE
				WHEN (SL_JK_MasterBillNum IS NULL OR SL_JK_MasterBillNum = '') AND (CRF IS NULL OR CRF = '')
					THEN NULL
				WHEN SL_JK_MasterBillNum IS NULL OR SL_JK_MasterBillNum = ''
					THEN CRF
				WHEN CRF IS NULL OR CRF = ''
					THEN SL_JK_MasterBillNum
				ELSE
					CONCAT(SL_JK_MasterBillNum, ' - ', CRF)
				END";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH STUAndMAALogs AS
(
	SELECT
		SL_PK,
		SL_SE_NKEvent,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		SL_GB_NKBranch,
		locStartPosition = CHARINDEX('|LOC=', SL_Reference),
		mstStartPosition = CHARINDEX('|MST=', SL_Reference),
		typStartPosition = CHARINDEX('|TYP=', SL_Reference),
		crfStartPosition = CHARINDEX('|CRF=', SL_Reference),
		refLen = LEN(SL_Reference)
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent IN ('STU', 'MAA')
		AND	SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%MST=Export Notification (755)%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), STUAndMAALogsWithDetails AS
(
	SELECT
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		MST = CASE mstStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, mstStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, mstStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, mstStartPosition + 1)) - mstStartPosition - 5) END,
		TYP = CASE typStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, typStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, typStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, typStartPosition + 1)) - typStartPosition - 5) END,
		CRF = CASE crfStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, crfStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, crfStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, crfStartPosition + 1)) - crfStartPosition - 5) END,
		SL_PK,
		SL_SE_NKEvent,
		SL_Reference,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_JK_MasterBillNum =
			CASE 
			WHEN JDD_ParentTableCode = 'JS'
				THEN
					(
						SELECT top 1 subConsol.JK_MasterBillNum FROM JobConShipLink 
						JOIN JobShipment AS subShipment ON JN_JS = subShipment.JS_PK 
						JOIN JobConsol AS subConsol ON subconsol.JK_PK = JN_JK
						WHERE subShipment.JS_PK = Shipment.JS_PK AND subConsol.JK_RL_NKLoadPort LIKE 'FR%' AND subConsol.JK_TransportMode = 'AIR'
					)
			WHEN JDD_ParentTableCode = 'JK' AND JK_RL_NKLoadPort LIKE 'FR%' AND JK_TransportMode = 'AIR'
				THEN JK_MasterBillNum 
			ELSE NULL 
			END,	
		SL_UniqueConsignRef =
			CASE 
			WHEN JDD_ParentTableCode = 'JS'
				THEN JS_UniqueConsignRef
			WHEN JDD_ParentTableCode = 'JK' 
				THEN JK_UniqueConsignRef 
			ELSE NULL 
			END,
		SL_GB_NKBranch =
			ISNULL(
				CASE 
				WHEN JDD_ParentTableCode = 'JS'
					THEN
						(
							SELECT TOP 1 JSBranchs.GB_Code FROM 
							(
								SELECT subBranch.* FROM GlbBranch subBranch WHERE subBranch.GB_GC = Company.GC_PK AND subBranch.GB_RL_NKHomePort = JS_RL_NKOrigin AND subBranch.GB_IsActive = 1
								UNION ALL 
								SELECT subBranch.* FROM GlbBranch subBranch 
								JOIN GlbBranchExtraPorts relatedPort on relatedPort.GY_GB = subBranch.GB_PK AND relatedPort.GY_RL_NKAdditionalBranchRelatedPort = JS_RL_NKOrigin
								WHERE subBranch.GB_GC = Company.GC_PK AND subBranch.GB_IsActive = 1
							) as JSBranchs
						)
				WHEN JDD_ParentTableCode = 'JK'
					THEN 
						(
							SELECT TOP 1 JKBranchs.GB_Code FROM 
							(
								SELECT subBranch.* FROM GlbBranch subBranch WHERE subBranch.GB_GC = Company.GC_PK AND subBranch.GB_RL_NKHomePort = JK_RL_NKLoadPort AND subBranch.GB_IsActive = 1
								UNION ALL 
								SELECT subBranch.* FROM GlbBranch subBranch 
								JOIN GlbBranchExtraPorts relatedPort on relatedPort.GY_GB = subBranch.GB_PK AND relatedPort.GY_RL_NKAdditionalBranchRelatedPort = JK_RL_NKLoadPort
								WHERE subBranch.GB_GC = Company.GC_PK AND subBranch.GB_IsActive = 1	
							) as JKBranchs
						)
				ELSE NULL END,
				(
					SELECT TOP 1
						MSNLogs.SL_GB_NKBranch
					FROM
						dbo.StmALog MSNLogs
					WHERE
						MSNLogs.SL_Parent = STUAndMAALogs.SL_Parent
						AND MSNLogs.SL_SE_NKEvent = 'MSN'
						AND MSNLogs.SL_Table = 'JobDocumentData'
						AND MSNLogs.SL_PostedTimeUtc >= DATEADD(DAY, -3, STUAndMAALogs.SL_PostedTimeUtc)
						AND MSNLogs.SL_PostedTimeUtc < STUAndMAALogs.SL_PostedTimeUtc
						AND MSNLogs.SL_Reference LIKE '%MST=Export Notification (755)%'
					ORDER BY MSNLogs.SL_PostedTimeUtc DESC
				)
			)
	FROM STUAndMAALogs
	LEFT JOIN dbo.JobDocumentData ON STUAndMAALogs.SL_Parent = JobDocumentData.JDD_PK
	LEFT JOIN dbo.JobShipment Shipment ON JobDocumentData.JDD_ParentTableCode = 'JS' AND JobDocumentData.JDD_ParentID = Shipment.JS_PK
	LEFT JOIN dbo.JobConsol Consol ON JobDocumentData.JDD_ParentTableCode = 'JK' AND JobDocumentData.JDD_ParentID = Consol.JK_PK
	LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = STUAndMAALogs.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC
)";

		public override string FromClause => @"STUAndMAALogsWithDetails
	LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = STUAndMAALogsWithDetails.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}
}
