namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationSpecialEntryUnderbond : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "UNB";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "InBond / Underbond Movement Request (From Customs Menu) (Underbond)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.MinDate";
		public override string BillingReference1 => "c4.C4_SendersMessageReference";
		public override string BillingReference2 => @"c4.C4_MovementReason + ' ' + rtrim(convert(varchar(15), C4.C4_DestinationPremiseID)) + CASE
								WHEN cm.CM_MAWB <> '' THEN ' MAWB: ' + cm.CM_MAWB 
								WHEN cn.CN_ContainerNumber <> '' THEN ' CNT: ' + cn.CN_ContainerNumber 
								WHEN cj.CJ_ContainerNumber <> '' THEN ' CNT: ' + cj.CJ_ContainerNumber 
								WHEN cx.CX_HouseBill <> '' THEN ' House: ' + cx.CX_HouseBill
								WHEN ca.CA_HouseBill <> '' THEN ' House: ' + ca.CA_HouseBill + ' ' + rtrim(convert(varchar(15), C4.C4_PiecesManifested)) + ' ' + C4.C4_PackageType
								ELSE ''
								END";
		public override string GuidReference => "c4.C4_PK";
		public override string CreatingUserCode => "em.EM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"(
							SELECT em2.EM_LinkUniqueID, 
									MAX(em2.EM_GB) AS EM_GB, 
									MAX(em2.EM_SystemCreateUser) AS EM_SystemCreateUser,
									MIN(em2.EM_SystemCreateTimeUtc) AS MinDate
							FROM 
								dbo.EDIMessage em2
								INNER JOIN (
									SELECT em0.EM_LinkUniqueID
									FROM 
										dbo.EDIMessage em0
									WHERE
										em0.EM_SystemCreateTimeUtc >= @StartDateTimeInclusive
										AND em0.EM_SystemCreateTimeUtc < @EndDateTimeExclusive
										AND em0.EM_LinkUniqueID is not null
										AND em0.EM_ReceiveTransmit = 'TRX'
										AND em0.EM_MessageType = 'UBM'
										AND em0.EM_MessageSubType = 'ORG'
										AND em0.EM_Status = 'SNT'
									) emCurrentPeriod ON emCurrentPeriod.EM_LinkUniqueID = em2.EM_LinkUniqueID
							WHERE em2.EM_LinkUniqueID is not null
								AND em2.EM_ReceiveTransmit = 'TRX'
								AND em2.EM_MessageType = 'UBM'
								AND em2.EM_MessageSubType = 'ORG'
								AND em2.EM_Status = 'SNT'
								GROUP BY em2.EM_LinkUniqueID
							) AS em

							INNER JOIN dbo.CusUnderbond c4 ON c4.c4_PK = em.EM_LinkUniqueID
							LEFT JOIN (
								GlbBranch gb 
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
							) ON gb.GB_PK = em.EM_GB

							LEFT JOIN dbo.CusMAWB cm ON cm.CM_PK = c4.C4_ParentID AND c4.C4_ParentTableCode = 'CM'
							LEFT JOIN dbo.CusSCAContainer cn ON cn.CN_PK = c4.C4_ParentID AND c4.C4_ParentTableCode = 'CN'
							LEFT JOIN dbo.CusSCADepotContainer cj ON cj.CJ_PK = c4.C4_ParentID AND c4.C4_ParentTableCode = 'CJ'
							LEFT JOIN dbo.CusSCADepotHouse cx ON cx.CX_PK = c4.C4_ParentID AND c4.C4_ParentTableCode = 'CX'

							LEFT JOIN (
								CusSCAPivot cv 
								INNER JOIN dbo.CusSCAHouse ca ON ca.CA_PK = cv.CV_CA
							) ON cv.CV_PK = c4.C4_ParentID AND c4.C4_ParentTableCode = 'CV'
						";
		public override string WhereClause => "isnull(gc.GC_RN_NKCountryCode, '') <> 'GB'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.21.50";
	}
	#endregion
}
