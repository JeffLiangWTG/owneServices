using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageContainerValidation : CommonContainerValidation
	{
		public CartageContainerValidation(CommonContainer container)
			: base(container)
		{
		}

		protected override void CheckJC_ContainerNum()
		{
			base.CheckJC_ContainerNum();
			if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Parent.Factory))
			{
				ValidateContainerNumIsNotUsedForAnotherCartage();
			}
		}

		virtual protected void ValidateContainerNumIsNotUsedForAnotherCartage()
		{
			//only if the container has a cartage

			var query = new ZQuery(JobBookedCtgMoveSchema.EW_JC_Container, Parent.PK);

			if (Parent.Factory.LoadTop1<CommonBookedCtgMove>(query) != null)
			{
				var otherCartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
				otherCartageQuery.AddToFilter(JoinCondition.And, JobCartageSchema.JJ_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-6));

				var pivotSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ);
				var containerPivotSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobBookedCtgMoveSchema.EW_JC_Container);

				var containerQuery = new ZQuery(JobContainerSchema.JC_ContainerNum, Parent.JC_ContainerNum);
				containerQuery.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				containerPivotSubQuery.AddToFilter(containerQuery);
				pivotSubQuery.AddSubQuery(containerPivotSubQuery, JoinCondition.And);
				otherCartageQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

				var otherCartages = Parent.Factory.Load<CommonCartage>(otherCartageQuery);

				foreach (CommonCartage otherCartageWithSameContainerID in otherCartages)
				{
					Parent.JC_ContainerNumInfo.AddWarning(Res.GetString("124a59e7-c3c9-4b07-bd40-21f1f7ff7476", "This container number {0} has been used on another Port Transport Job {1} within the last 6 months.", Parent.JC_ContainerNum, otherCartageWithSameContainerID.JJ_ConsignmentID));
				}
			}
		}
	}
}
