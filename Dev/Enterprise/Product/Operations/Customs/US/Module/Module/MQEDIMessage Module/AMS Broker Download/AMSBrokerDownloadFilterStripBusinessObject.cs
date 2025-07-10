using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class AMSBrokerDownloadFilterStripBusinessObject : MQEDIMessageCommonFilterStripBusinessObject
	{
		protected override ZBool ShouldAddApplicationCodeFilter => false;

		protected override ZBool ShouldAddDirectionFilter => false;

		protected override ZBool ShouldAddMessageTypeSubTypeFilter => false;

		protected override ZBool ShouldAddInterchageDetailFilters => false;

		protected override ZBool ShouldAddMessageNumFilter => false;

		protected override bool ShouldAddStatusFilter => false;

		protected override bool ShouldAddApplicationReferenceFilter => false;

		protected override ZBool ShouldAddMessageTimeFilter => false;

		protected override ZBool ShouldAddEHubIDFilters => false;

		protected override ZBool ShouldAddSenderFilters => false;

		protected override ZBool ShouldAddReceiverFilters => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var filter = result.AddNumberFilter(DeclarationFilterConstants.IssuerScacBol, GetSCACBillOfLadingNumberQuery);
			filter.MaxLength = CusCodeDataSchema.CY_Data.MaxLength;

			result.AddTextFilter(DeclarationFilterConstants.ActionStatus, GeActionStatusQuery, new EM_ActionStatusList());

			return result;
		}

		ZQuery GeActionStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(MQEDIMessage));

			var subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, value == EM_ActionStatusList.Codes.Incomplete);
			subQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			subQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			if (value == EM_ActionStatusList.Codes.Incomplete)
			{
				subQuery.AddToFilter(StmALogSchema.SL_Reference, EM_ActionStatusList.Codes.Complete);
			}
			else
			{
				subQuery.AddToFilter(StmALogSchema.SL_Reference, value);
			}
			result.AddSubQuery(subQuery, JoinCondition.Or);

			return result;
		}

		ZQuery GetSCACBillOfLadingNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(MQEDIMessage));

			var subQuery = new ZDBOnlySubQuery(typeof(IssuerAndBillNumber), CusCodeDataSchema.CY_ParentID);
			subQuery.AddToFilter(JoinCondition.And, CusCodeDataSchema.CY_ParentTableCode, SQLComparisonOperator.Equal, EDIMessageSchema.Constants.Prefix);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusCodeDataSchema.CY_Data, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}
	}
}
