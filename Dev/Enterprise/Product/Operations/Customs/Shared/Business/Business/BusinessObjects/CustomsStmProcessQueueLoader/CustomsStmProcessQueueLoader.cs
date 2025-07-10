using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class CustomsStmProcessQueueLoader
	{
		#region Constant values

		public static class Constants
		{
			public const string DocumentGeneratorApplicationCode = "CDG";
			public const string AutoSendCustomsMessaging = "ASC";
			public const string JobTypeCode = "CUS";
		}

		#endregion

		#region Load/Create Methods

		public static StmProcessQueue Load(BusinessObject businessObject, ZString applicationCode, ZString actionCode)
		{
			var filter = new ZDBOnlyQuery(typeof(StmProcessQueue));
			filter.AddToFilter(StmProcessQueueSchema.SW_ReferenceID, businessObject.PK);
			filter.AddToFilter(JoinCondition.And, StmProcessQueueSchema.SW_ReferenceTableCode, businessObject.TablePrefix);
			filter.AddToFilter(JoinCondition.And, StmProcessQueueSchema.SW_ApplicationCode, applicationCode);
			filter.AddToFilter(JoinCondition.And, StmProcessQueueSchema.SW_JobTypeCode, Constants.JobTypeCode);
			filter.AddToFilter(JoinCondition.And, StmProcessQueueSchema.SW_ActionCode, actionCode);
			filter.OrderBy = StmProcessQueueSchema.Constants.SW_PostedTimeUtc;
			return businessObject.Factory.LoadTop1<StmProcessQueue>(filter);
		}

		public static StmProcessQueue New(BusinessObject businessObject, ZString applicationCode, ZString actionCode)
		{
			var result = businessObject.Factory.New<StmProcessQueue>();
			using (result.SuspendSettingHasChanges())
			{
				result.SW_ApplicationCode = applicationCode;
				result.SW_JobTypeCode = Constants.JobTypeCode;
				result.SW_ActionCode = actionCode;
				result.SW_ReferenceID = businessObject.PK;
				result.SW_ReferenceTableCode = businessObject.TablePrefix;
				result.SW_PostedTimeUtc = ZDateTime.UtcNow;
			}
			return result;
		}

		public static StmProcessQueue LoadOrCreate(BusinessObject businessObject, ZString applicationCode, ZString actionCode)
		{
			return Load(businessObject, applicationCode, actionCode) ?? New(businessObject, applicationCode, actionCode);
		}

		#endregion
	}
}
