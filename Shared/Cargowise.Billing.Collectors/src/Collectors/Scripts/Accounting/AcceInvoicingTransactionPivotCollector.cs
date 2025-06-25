using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.Billing.Collectors.Accounting
{
	public class AcceInvoicingTransactionPivotCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;

		public override string FeatureCode => "TE1";

		public override string RoleName => "Accounting";

		public override string ModuleName => "Accounting";

		public override string FeatureName => "AcceInvoicingTransactionPivot table STL Collector";

		public override string FunctionName => "AcceInvoicingTransactionPivot table STL Collector";
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string CompanyCode => "GC.GC_Code";
		public override string BranchCode => "b.GB_Code";
		public override string TransactionDateUtc => "@StartDateTimeInclusive";
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";
		public override string BillingReference1 => "AH_Ledger";
		public override string BillingReference2 => "AH_TransactionType";
		public override string BillingReference3 => "AIP_RN_NKCountryCode";
		public override string BillingReference4 => "AH_ComplianceSubType";
		public override string TransactionCount => "COUNT(ac.AIP_PK)";
		public override string WhereClause => @"ac.AIP_LastResponseReceivedUtc >= @StartDateTimeInclusive
						AND ac.AIP_LastResponseReceivedUtc < @EndDateTimeExclusive
						AND ac.AIP_ParentTableCode = 'AH'
						AND ac.AIP_ActionType = 'SUB'
				GROUP BY
					ac.AIP_Status,
					ac.AIP_RN_NKCountryCode, 
					t.AH_Ledger, 
					GC.GC_Code, 
					t.AH_TransactionType, 
					t.AH_TransactionCategory,
					t.AH_ComplianceSubType,
					t.AH_RX_NKTransactionCurrency,
					GC.GC_RX_NKLocalCurrency,
					b.GB_RL_NKHomePort,
					tb.GB_RL_NKHomePort,
					b.GB_Code,
					tb.GB_Code";
		public override string FromClause => @"dbo.AcceInvoicingTransactionPivot ac WITH (INDEX (NR_RX__AIP_LastResponseReceivedUtc))
					LEFT JOIN dbo.AccTransactionHeader t ON ac.AIP_ParentID = t.AH_PK LEFT JOIN dbo.GlbCompany GC ON GC.GC_PK = t.AH_GC
					LEFT JOIN dbo.GlbBranch b ON b.GB_PK = t.AH_GB
					LEFT JOIN dbo.GlbBranch tb ON tb.GB_PK = t.AH_GB_TaxBranch";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
					(
						SELECT
							tb.GB_Code AS TaxBranchCode,
							SUM(t.AH_LocalTotal) AS Total_LocalAmount,  
							SUM(t.AH_OSTaxAmountOtherTaxes) AS Total_OSTaxAmount,
							b.GB_RL_NKHomePort AS BranchHomePort,
							tb.GB_RL_NKHomePort AS TaxBranchHomePort,
							ac.AIP_Status,
							AH_TransactionCategory,
							GC.GC_RX_NKLocalCurrency AS LocalCurrency,
							t.AH_RX_NKTransactionCurrency AS TransactionCurrency
						FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
					)))";
		override public string ActiveOn => "ALL";

		#endregion
	}
}
