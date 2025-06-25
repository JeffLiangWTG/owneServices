namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoCtoOutturn : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CTT";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo CFS/CTO Customs Functions (Outturns)";
		public override string CompanyCode => "Outturn.GC_Code";
		public override string BranchCode => "Outturn.GB_Code";
		public override string TransactionDateUtc => "Outturn.MinDate";
		public override string BillingReference1 => "Outturn.C4_SendersMessageReference";
		public override string BillingReference2 => @"CASE
								WHEN Outturn.C5_ContainerNumber <> '' THEN 'CTN: '	 + Outturn.C5_ContainerNumber
								WHEN Outturn.C5_MasterBill      <> '' THEN 'MBL: '	 + Outturn.C5_MasterBill
								WHEN Outturn.C5_HouseBill       <> '' THEN 'HBL: '	 + Outturn.C5_HouseBill
								WHEN Outturn.CS_HAWB            <> '' THEN 'HAWB: '	 + Outturn.CS_HAWB
								WHEN Outturn.C4_MAWB            <> '' THEN 'MAWB: '	 + Outturn.C4_MAWB
								WHEN Outturn.C4_FlightNo        <> '' THEN 'FLIGHT: ' + Outturn.C4_FlightNo
								ELSE ''
								END + ' ' + rtrim(convert(varchar(36), Outturn.C5_OuterPacks))
									+ ' ' + Outturn.C5_OutturnResultType";
		public override string BillingReference3 => "Outturn.CM_MasterHouseBill";
		public override string BillingReference4 => @"CASE
							WHEN Outturn.CM_MAWB  <> '' THEN 'MAWB: ' + Outturn.CM_MAWB
							WHEN Outturn.CM_MAWB2 <> '' THEN 'MAWB: ' + Outturn.CM_MAWB2
							END
";
		public override string GuidReference => "Outturn.C5_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					(
						SELECT
							GC_Code, MinDate, C4_SendersMessageReference, C5_ContainerNumber, C5_MasterBill, C5_HouseBill, CS_HAWB, 
							C4_MAWB, C4_FlightNo, C5_OuterPacks, C5_OutturnResultType, cm.CM_MAWB AS CM_MAWB,  cm2.CM_MAWB AS CM_MAWB2,
							cm.CM_MasterHouseBill AS CM_MasterHouseBill, C5_PK, GB_Code
						FROM
						(
							SELECT em2.EM_LinkUniqueID, MAX(em2.EM_GB) AS EM_GB, MIN(em2.EM_SystemCreateTimeUtc) AS MinDate
							FROM 
								dbo.EDIMessage em2
							WHERE 
								em2.EM_LinkUniqueID IN (
									SELECT em0.EM_LinkUniqueID
									FROM
										dbo.EDIMessage em0
									WHERE
										em0.EM_SystemCreateTimeUtc >= @StartDateTimeInclusive
										AND em0.EM_SystemCreateTimeUtc < @EndDateTimeExclusive
										AND em0.EM_LinkUniqueID is not null
										AND em0.EM_ReceiveTransmit = 'TRX'
										AND em0.EM_MessageType IN ('AUT', 'SUT')
										AND em0.EM_MessageSubType = 'ORG'
										AND em0.EM_Status = 'SNT'
								)
								AND em2.EM_LinkUniqueID is not null
								AND em2.EM_ReceiveTransmit = 'TRX'
								AND em2.EM_MessageType IN ('AUT', 'SUT')
								AND em2.EM_MessageSubType = 'ORG'
								AND em2.EM_Status = 'SNT'
								GROUP BY em2.EM_LinkUniqueID
						) AS em

						INNER JOIN dbo.CusUnderbond c4 ON c4.c4_PK = em.EM_LinkUniqueID
						INNER JOIN dbo.CusOutturn c5 ON c5.C5_c4_Underbond = c4.c4_PK

						LEFT JOIN (
							GlbBranch gb 
							INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
						) ON gb.GB_PK = em.EM_GB

						LEFT JOIN (
							CusHawb cs
							INNER JOIN dbo.CusMawb cm ON cm.CM_PK = cs.CS_CM
						) ON cs.CS_PK = c5.C5_ParentID AND c5.C5_ParentTableCode = 'CS'

						LEFT JOIN dbo.CusMawb cm2 ON cm2.CM_PK = c5.C5_ParentID AND c5.C5_ParentTableCode = 'CM'

						WHERE em.MinDate >= @StartDateTimeInclusive
						AND em.MinDate < @EndDateTimeExclusive
						AND isnull(gc.GC_RN_NKCountryCode, '') <> 'GB'
						AND (cs.CS_IsHVLV = 0 OR c5.C5_ParentID is NULL or cm2.CM_PK is NOT NULL)
				) Outturn";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
