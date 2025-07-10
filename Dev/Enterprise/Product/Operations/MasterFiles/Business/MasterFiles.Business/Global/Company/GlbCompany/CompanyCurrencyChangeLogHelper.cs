using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class CompanyCurrencyChangeLogHelper
	{
		public static string GetCurrentCompanyCurrencyChangeLog(BusinessObjectFactory factory)
		{
			var log = $"Current Company[{Env.CurrentCompany.Code}] Currency Code Change Log: {GetCompanyCurrencyChangeLog(Env.CurrentCompanyPK, factory)}.";
			return log;
		}

		static string GetCompanyCurrencyChangeLog(ZGuid companyPK, BusinessObjectFactory factory)
		{
			var noLogFoundMsg = (NoResString)"No local currency change log found";
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, companyPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Table, GlbCompanySchema.Constants.TableName);
			query.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Company Currency");
			query.OrderBy = $"{StmALogSchema.Constants.SL_EventTime} DESC";

			var latestLog = factory.LoadTop1<StmALog>(query);
			return latestLog?.SL_Reference ?? noLogFoundMsg;
		}
	}
}
