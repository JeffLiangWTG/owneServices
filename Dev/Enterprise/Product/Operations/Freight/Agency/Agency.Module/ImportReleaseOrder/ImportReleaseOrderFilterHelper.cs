
namespace Enterprise.Freight.Agency.Module
{
	using System.Globalization;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Module.ImportReleaseOrder;
	using Enterprise.ZArchitecture.Business;
	public static class ImportReleaseOrderFilterHelper
	{
		public static ZQuery GetFilterQuery(ZString status)
		{
			switch (status)
			{
				case ImportReleaseOrderFilterList.Codes.Sent:
					return GetFilterQueryCore(Events.MessageSentCode, false);
				case ImportReleaseOrderFilterList.Codes.InterchangeRejected:
					return GetFilterQueryCore(Events.InterchangeRejectedCode, false);
				case ImportReleaseOrderFilterList.Codes.NotSent:
					return GetFilterQueryCore(Events.MessageWithdrawCancelRequestCode, true);
				default:
					return new ZQuery();
			}
		}

		static ZQuery GetFilterQueryCore(ZString eventCode, bool includeMessageLessContainers)
		{
			var baseQuery = @"
				JC_PK in
				(
					select SL_Parent from dbo.StmALog 
					join
					(
						select SL_Parent AS ParentPK, max(SL_PostedTimeUtc) as lastLogTime from dbo.StmALog
						where SL_SE_NKEvent in ('MSN', 'MWR', 'IRJ') and SL_Reference like '%|MST=Import Release Order%'
						group by SL_Parent
					) lastMessageDate on SL_Parent = ParentPK and SL_PostedTimeUtc = lastLogTime
					where SL_SE_NKEvent = '{0}'

					{1}
				)";

			var messageLessContainersQuery = @"
				union all

				select JC_PK from dbo.JobContainer
				where not exists (
					select 1 from dbo.StmALog
					where SL_Parent = JC_PK AND SL_SE_NKEvent in ('MSN', 'MWR', 'IRJ') and SL_Reference like '%|MST=Import Release Order%'
				)";

			var sqlQuery = string.Format(CultureInfo.InvariantCulture, baseQuery, eventCode, includeMessageLessContainers ? messageLessContainersQuery : string.Empty);

			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddFilterAndZSQLParameterCollection(sqlQuery, new ZSqlParameterCollection());

			return query;
		}
	}
}



