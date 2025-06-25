using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class AllocationManagementConsumptionTEU : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "AMC";

		public override string FeatureName => "Allocation of Jobs to Carrier Contract (TEU)";

		public override string FunctionName => "Allocation of Jobs to Carrier Contract (TEU)";

		public override string RoleName => "Consol's TEU allocated to Carrier Contract";

		public override string ModuleName => "Carrier Contract & Allocation";

		public override string TransactionDateUtc => "SL_PostedTimeUtc";

		public override string GuidReference => "JK_PK";

		public override string FromClause => @"BillableWithBranch";

		public override string CompanyCode => "";

		public override string BranchCode => "Branch";

		public override string CreatingUserCode => "JK_SystemCreateUser";

		public override string BillingReference1 => "JobType";

		public override string BillingReference2 => "JK_UniqueConsignRef";

		public override string BillingReference3 => "JK_CarrierContractNumber";

		public override string BillingReference4 => "CarrierCode";

		public override string WhereClause => "BillableTEU > 0";

		public override string TransactionCount => @"FLOOR(BillableTEU)";

		public override string PreparationScript => @"
WITH ConsolsWithContractAndMainLegDeparted AS (
	SELECT
		consol.JK_PK,
		consol.JK_UniqueConsignRef,
		consol.JK_CarrierContractNumber,
		consol.JK_RL_NKLoadPort,
		consol.JK_SystemCreateUser,
		consol.JK_ConsolMode,
		carrier.OH_Code AS CarrierCode,
		departureLog.SL_GB_NKBranch,
		departureLog.SL_PostedTimeUtc
	FROM
		dbo.StmALog departureLog
		JOIN dbo.JobConsolTransport departingLeg ON SL_Parent = JW_PK
		JOIN dbo.JobConsol consol ON JW_ParentGUID = consol.JK_PK
		JOIN dbo.OrgAddress carrierAddress ON consol.JK_OA_ShippingLineAddress = carrierAddress.OA_PK
		JOIN dbo.OrgHeader carrier ON carrierAddress.OA_OH = carrier.OH_PK
		JOIN dbo.RatingContract carrierContract ON carrierContract.RCT_ContractNumber = consol.JK_CarrierContractNumber AND carrierContract.RCT_OH = carrier.OH_PK
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'DEP'
		AND SL_IsEstimate = 'N'
		AND SL_IsCancelled = 'N'
		AND SL_Table = 'JobConsolTransport'
		AND departingLeg.JW_TransportMode = 'SEA'
		AND departingLeg.JW_TransportType = 'MAI'
		AND consol.JK_IsForwarding = 1
		AND consol.JK_IsCancelled = 0
		AND consol.JK_TransportMode = 'SEA'
		AND consol.JK_ConsolMode IN ('FCL', 'GRP', 'BCN', 'SCN', 'OTH')
		AND consol.JK_CarrierContractNumber <> ''
		AND carrierContract.RCT_ContractType = 'PRO'
		AND carrierContract.RCT_IsActive = 1
),
RefContainersWithTEU AS (
	SELECT
		RC_PK,
		CalculatedTEU = CASE
			WHEN RC_TEU IS NOT NULL
			AND RC_TEU > 0 THEN RC_TEU
			ELSE CONVERT(
				DECIMAL(5, 2),
				CASE
					LEFT(RC_ISOType, 1)
					WHEN '1' THEN 10
					WHEN '2' THEN 20
					WHEN '3' THEN 30
					WHEN '4' THEN 40
					WHEN 'A' THEN 23.5
					WHEN 'B' THEN 24
					WHEN 'C' THEN 24.5
					WHEN 'D' THEN 24.5
					WHEN 'E' THEN 25.7
					WHEN 'F' THEN 26.6
					WHEN 'G' THEN 41
					WHEN 'H' THEN 43
					WHEN 'K' THEN 44.6
					WHEN 'L' THEN 45
					WHEN 'M' THEN 48
					WHEN 'N' THEN 49
					WHEN 'P' THEN 53
					ELSE 40
				END / 20
			)
		END
	FROM
		dbo.RefContainer
),
BillableWithBranch AS (
	SELECT
		consolWithDetails.JK_PK,
		consolWithDetails.JK_UniqueConsignRef,
		consolWithDetails.JK_CarrierContractNumber,
		consolWithDetails.JK_SystemCreateUser,
		consolWithDetails.CarrierCode,
		consolWithDetails.SL_PostedTimeUtc,
		(
			SELECT SUM(container.JC_ContainerCount * refContainer.CalculatedTEU)
			FROM JobContainer container 
			JOIN RefContainersWithTEU refContainer ON container.JC_RC = refContainer.RC_PK
			WHERE container.JC_JK = consolWithDetails.JK_PK
			AND (consolWithDetails.JK_ConsolMode <> 'OTH' OR container.JC_ContainerNum <> '')
		) AS BillableTEU,
		COALESCE(
			(
				SELECT TOP 1 Branch.GB_Code
				FROM GlbBranch Branch
				LEFT JOIN GlbBranchExtraPorts ExtraPort ON ExtraPort.GY_GB = Branch.GB_PK 
				WHERE Branch.GB_GC = LogBranchCompany.GC_PK
				AND (
					Branch.GB_RL_NKHomePort = consolWithDetails.JK_RL_NKLoadPort
					OR ExtraPort.GY_RL_NKAdditionalBranchRelatedPort = consolWithDetails.JK_RL_NKLoadPort
				)
				ORDER BY Branch.GB_Code
			),
			consolWithDetails.SL_GB_NKBranch
		) AS Branch,
		'CON' AS JobType
	FROM ConsolsWithContractAndMainLegDeparted consolWithDetails
	JOIN GlbBranch LogBranch ON LogBranch.GB_Code = consolWithDetails.SL_GB_NKBranch
	JOIN GlbCompany LogBranchCompany ON LogBranchCompany.GC_PK = LogBranch.GB_GC
)";
	}

	#endregion
}
