using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI
{
	public class JobVoyageLogFilterBusinessObject : ZStmALogFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "only used in code")]
		const string RelatedJobsShowForCode = "Related jobs";

		public JobVoyageLogFilterBusinessObject()
		{
		}

		public JobVoyageLogFilterBusinessObject(JobVoyage jobVoyage) : base(jobVoyage)
		{
		}

		protected override CodeDescriptionPairList AdditionalEventsOrigins
		{
			get
			{
				return new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(
						ResString.GetMultilingualString("70954E21-055D-4324-A5D5-090432C36C04", RelatedJobsShowForCode),
						ResString.GetMultilingualString("BA4FC089-D41C-47E9-A1ED-F8F671EAE0F8", "Jobs related to this {0}", master.HumanReadableName)
					)
				};
			}
		}

		protected override ZQuery GetBusinessObjectsWithRelatedEventsQuery(ZString showForCode)
		{
			if (showForCode.Equals(RelatedJobsShowForCode) && master is JobVoyage jobVoyage)
			{
				var query = new ZDBOnlyQuery(typeof(StmALog));

				var sqlText =
					$@"
						{StmALog.Schema.SL_Parent} IN
						(
							(
								SELECT DISTINCT {ViewSailingRelatedJobSchema.Constants.PK}
								FROM {ViewSailingRelatedJobSchema.Constants.SqlSchemaName}.{ViewSailingRelatedJobSchema.Constants.TableName}
								WHERE {ViewSailingRelatedJobSchema.Constants.VJX_JV} = @VJV_JV
							)
							UNION
							(
								SELECT DISTINCT {ViewSailingRelatedJobSchema.Constants.VJX_RelatedJobID}
								FROM {ViewSailingRelatedJobSchema.Constants.SqlSchemaName}.{ViewSailingRelatedJobSchema.Constants.TableName}
								WHERE {ViewSailingRelatedJobSchema.Constants.VJX_JV} = @VJV_JV
							)
						)
					";

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@VJV_JV", jobVoyage.PK, JobVoyageSchema.PK);

				query.AddFilterAndZSQLParameterCollection(sqlText, parameters, JoinCondition.Union);

				return query;
			}

			return base.GetBusinessObjectsWithRelatedEventsQuery(showForCode);
		}
	}
}
