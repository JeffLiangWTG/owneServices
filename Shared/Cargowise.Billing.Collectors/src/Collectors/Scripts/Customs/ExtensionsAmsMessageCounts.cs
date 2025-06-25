namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsAmsMessageCounts : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "AM2";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "US Customs";
		public override string FeatureName => "AMS Message Counts";
		public override string CompanyCode => "AMSFirstReported.GC_Code";
		public override string BranchCode => "AMSFirstReported.GB_Code";
		public override string TransactionDateUtc => "AMSFirstReported.ReportedDate";
		public override string BillingReference1 => "AMSFirstReported.BH_JobReference";
		public override string BillingReference2 => "AMSFirstReported.OceanMasterBill";
		public override string BillingReference3 => "AMSFirstReported.BillOfLading";
		public override string BillingReference4 => "AMSFirstReported.IssuerCode";
		public override string GuidReference => "AMSFirstReported.B0_PK";
		public override string CreatingUserCode => "AMSFirstReported.SubmittedUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				(
					SELECT
					B0.B0_PK,GB.GB_Code,
					GC.GC_Code,BH.BH_JobReference,
					CASE 
					WHEN sl.SL_PK is not null then 'HVL'
					ELSE B0.B0_IssuerCode 
					END
					AS IssuerCode,
					B0M.B0_MasterbillNumber AS OceanMasterBill,
					B0.B0_MasterbillNumber AS BillOfLading,
					B9_FirstAcceptedTime AS ReportedDate,
					'~BP' AS SubmittedUser
				FROM dbo.CusInBondMoveDetail AS B9
				INNER JOIN dbo.CusInbondBill AS B0 ON B0.B0_PK = B9.B9_B0
				INNER JOIN dbo.CusInBondBill AS B0M ON B0M.B0_BH = B0.B0_BH AND B0M.B0_ShipmentType = 'OBT'
				INNER JOIN dbo.CusInbondHeader AS BH ON BH.BH_PK = B0M.B0_BH
				INNER JOIN dbo.CusInBondMoveHeader AS BM ON B9.B9_BM = BM.BM_PK AND BM.BM_SubApplicationCode = 'AMS' AND BM.BM_BH = BH.BH_PK
				INNER JOIN dbo.GlbBranch GB ON GB.GB_PK = BH.BH_GB
				INNER JOIN dbo.GlbCompany GC ON GC.GC_PK = GB.GB_GC
				LEFT JOIN dbo.StmALog sl on BH.BH_PK = sl.SL_Parent
					AND CHARINDEX('|TYP=HVL', SL_Reference) > 0
					AND SL_IsCancelled = 'N'
					AND SL_SE_NKEvent = 'TRF'
				WHERE
					BH.BH_ApplicationCode = 'AMS'
					AND B0.B0_ShipmentType != 'OBT'
				) AS AMSFirstReported";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
