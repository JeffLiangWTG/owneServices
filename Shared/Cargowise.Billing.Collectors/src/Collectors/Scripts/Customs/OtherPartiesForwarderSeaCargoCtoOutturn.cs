namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderSeaCargoCtoOutturn : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "STT";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Sea Cargo CFS/CTO Customs Functions (Outturns)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.MinDate";
		public override string BillingReference1 => "c6.C6_SendersMessageReference";
		public override string BillingReference2 => @"CASE
								WHEN c5.C5_ContainerNumber <> '' THEN 'CTN: ' + c5.C5_CargoType + ' ' + c5.C5_ContainerNumber + ' ' + c5.C5_MasterBill + ' ' + c5.C5_HouseBill 
								WHEN c5.C5_MasterBill <> '' THEN 'MBL: ' + c5.C5_MasterBill
								WHEN c5.C5_HouseBill <> '' THEN 'HBL: ' + c5.C5_HouseBill
								WHEN cs.CS_HAWB <> '' THEN 'HAWB: ' + cs.CS_HAWB
								WHEN c4.C4_MAWB <> '' THEN 'MAWB: ' + c4.C4_MAWB
								ELSE 'REF: ' + c4.c4_SendersMessageReference
								END + ' ' + rtrim(convert(varchar(36), c5.C5_PackagesOutturned, 15)) + ' ' + c5.C5_OutturnResultType";
		public override string GuidReference => "c5.C5_PK";
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
										AND em0.EM_MessageType IN ('AUT', 'SUT')
										AND em0.EM_MessageSubType = 'ORG'
										AND em0.EM_Status = 'SNT'
									) emCurrentPeriod ON emCurrentPeriod.EM_LinkUniqueID = em2.EM_LinkUniqueID
							WHERE 
								em2.EM_LinkUniqueID is not null
								AND em2.EM_ReceiveTransmit = 'TRX'
								AND em2.EM_MessageType IN ('AUT', 'SUT')
								AND em2.EM_MessageSubType = 'ORG'
								AND em2.EM_Status = 'SNT'
								GROUP BY em2.EM_LinkUniqueID
							) AS em

							INNER JOIN dbo.CusOutturnHeader c6 ON c6.c6_PK = em.EM_LinkUniqueID
							INNER JOIN dbo.CusUnderbond c4 ON c4.c4_c6 = c6.c6_PK
							INNER JOIN dbo.CusOutturn c5 ON c5.C5_c4_Underbond = c4.c4_PK

							LEFT JOIN (
								GlbBranch gb 
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
							) ON gb.GB_PK = em.EM_GB

							LEFT JOIN (
								CusHawb cs
								INNER JOIN dbo.CusMawb cm ON cm.CM_PK = cs.CS_CM
							) ON cs.CS_PK = c5.C5_ParentID AND c5.C5_ParentTableCode = 'CS'
						";
		public override string WhereClause => "isnull(gc.GC_RN_NKCountryCode, '') <> 'GB'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
