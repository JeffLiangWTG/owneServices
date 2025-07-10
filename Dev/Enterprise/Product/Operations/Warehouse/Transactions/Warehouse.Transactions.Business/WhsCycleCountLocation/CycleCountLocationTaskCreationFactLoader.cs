using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CycleCountLocationTaskCreationFactLoader : CycleCountLocationFactLoader<CycleCountLocationFact>, ICycleCountLocationTaskCreationFactLoader
	{
#pragma warning disable CW1161 // Res.GetString Analyzer
		protected override string AdditionalWhereClause => @"
	AND WLV_IsValidLocationForProductWarehousePutaway = 1
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsCycleCountLocation
		WHERE
			WCL_WL_Location = WLV_PK
			AND WCL_EndTime IS NULL
	)
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsCycleCountLocation
			JOIN dbo.WhsCycleCountLocationVariance ON WCC_WCL_CycleCountLocation = WCL_PK
		WHERE
			WCL_WL_Location = WLV_PK
			AND WCC_Status = 'OPN'
	)";

		protected override string PriorityAndGranularityFields => @"
	CAST(0 AS TINYINT) AS WCL_Priority,
	'' AS WCL_Granularity";
#pragma warning restore CW1161 // Res.GetString Analyzer

		protected override CycleCountLocationFact CreateLocationFact(
			CycleCountLocationCoreFact location,
			IOrganisationFact client,
			ICycleCountProductFact product,
			decimal stockOnHandForThisProduct)
			=> new CycleCountLocationFact(location, client, product, stockOnHandForThisProduct);
	}
}
