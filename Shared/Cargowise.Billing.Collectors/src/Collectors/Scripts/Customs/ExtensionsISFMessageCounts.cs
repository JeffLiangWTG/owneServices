namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class ExtensionsIsfMessageCounts : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "IS2";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "US Customs";
		public override string FeatureName => "ISF Message Counts";
		public override string CompanyCode => "ISFMessageAccepted.GC_Code";
		public override string BranchCode => "ISFMessageAccepted.GB_Code";
		public override string TransactionDateUtc => "ISFMessageAccepted.EM_SystemCreateTimeUtc";
		public override string BillingReference1 => "ISFMessageAccepted.JobNumber";
		public override string BillingReference2 => "ISFMessageAccepted.BF_CustomsReference";
		public override string BillingReference3 => "ISFMessageAccepted.PortCode";
		public override string BillingReference4 => "ISFMessageAccepted.ShipmentType";
		public override string GuidReference => "ISFMessageAccepted.BF_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"(
				SELECT GB.GB_Code,
				GC.GC_Code,
				BF.BF_CustomsReference,
				BF.BF_JobReference JobNumber,
				BF.BF_PK,
				EM.EM_SystemCreateTimeUtc,
				EM.EM_ApplicationCode,
				EM.EM_MessageType,
				EM.EM_MessageText,
				EM.EM_MessageSubType,
				SUBSTRING(EM.EM_MessageText, 4, 4) AS PortCode,
				CASE 
				WHEN SL.SL_PK IS NOT NULL THEN 'HVL'
				ELSE ''
				END
				AS ShipmentType
				FROM dbo.CusISFHeader BF
				INNER JOIN dbo.EDIMessage EM ON BF.BF_PK = EM.EM_LinkUniqueID
				INNER JOIN dbo.GlbBranch GB ON GB.GB_PK = BF.BF_GB
				INNER JOIN dbo.GlbCompany GC ON GC.GC_PK = GB.GB_GC
				LEFT JOIN dbo.StmALog SL ON BF.BF_PK = SL.SL_Parent
							AND CHARINDEX('|TYP=HVL', SL_Reference) > 0
							AND SL_IsCancelled = 'N'
							AND SL_SE_NKEvent = 'TRF'
				) AS ISFMessageAccepted";
		public override string WhereClause => @" EM_ApplicationCode = 'USI'
				AND EM_MessageType = 'SN'
				AND EM_MessageText LIKE '%ISF ACCEPTED%'
				AND EM_MessageSubType = 'ADD'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}

	#endregion
}
