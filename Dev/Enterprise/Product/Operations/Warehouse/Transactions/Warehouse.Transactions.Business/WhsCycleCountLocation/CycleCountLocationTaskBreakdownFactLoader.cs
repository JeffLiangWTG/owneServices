using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CycleCountLocationTaskBreakdownFactLoader : CycleCountLocationFactLoader<TaskManagementCycleCountLocationFact>, ICycleCountLocationTaskBreakdownFactLoader
	{
#pragma warning disable CW1161 // Res.GetString Analyzer
		protected override string EntityPK => "WCL_PK AS EntityPK";

		protected override string AdditionalWhereClause => @"
	AND WCL_TaskPlanningStatus = 'RFP'";

		protected override string PriorityAndGranularityFields => @"
	WCL_Priority AS WCL_Priority,
	WCL_Granularity AS WCL_Granularity";

		protected override string AdditionalJoins => @"
	JOIN dbo.WhsCycleCountLocation ON WCL_WL_Location = WLV_PK";
#pragma warning restore CW1161 // Res.GetString Analyzer

		protected override bool CycleCountTaskExists => true;

		protected override TaskManagementCycleCountLocationFact CreateLocationFact(
			CycleCountLocationCoreFact location,
			IOrganisationFact client,
			ICycleCountProductFact product,
			decimal stockOnHandForThisProduct)
			=> new TaskManagementCycleCountLocationFact(location, client, product, stockOnHandForThisProduct);
	}
}
