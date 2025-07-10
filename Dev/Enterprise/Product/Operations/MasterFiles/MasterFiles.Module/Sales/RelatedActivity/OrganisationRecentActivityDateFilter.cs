using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationRecentActivityDateFilter : RecentActivityDateFilter
	{
		#region Constructor

		public OrganisationRecentActivityDateFilter(ZString descriptionPrefix, ZString description, SchemaGuidColumn orgPKColumn)
			: base(CreateDescription(descriptionPrefix, description), OrgHeaderSchema.Constants.Prefix, ViewSalesDashboardActivitySchema.VSA_SystemLastEditTimeUtc, typeof(OrgHeader))
		{
			this.orgPKColumn = orgPKColumn;
			MultilingualDescription = ResString.GetMultilingualString("23AB079B-4BE0-42B6-ABEE-7D7FDF5382B3", "Sales Relations Last Edit Time");
		}

		readonly SchemaGuidColumn orgPKColumn;

		static ZString CreateDescription(ZString descriptionPrefix, ZString description)
		{
			return (descriptionPrefix.IsEmpty ? descriptionPrefix : ZString.Format(descriptionPrefix + "_")) + (description.IsEmpty ? (ZString)SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate : description);
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var fromDate = FromDate;
			var toDate = ToDate;

			var isSpecificType = !TypeProperty.IsEmpty && TypeProperty != SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			var lastEditColumnName = ViewSalesDashboardActivitySchema.Constants.VSA_SystemLastEditTimeUtc;
			var lastEditColumn = isSpecificType ? "MAX(Tree.LastEdit)" : (NoResString)"COALESCE(MAX(Tree.LastEdit), Master." + lastEditColumnName + (NoResString)")";
			var relatedActivityTypeCondition = isSpecificType ? (NoResString)"AND ActivityType = @RecentActivityDateFilter_ActivityType" : "";
			var dateComparisonCondition =
				!fromDate.IsEmpty && !toDate.IsEmpty ? (NoResString)"BETWEEN @RecentActivityDateFilter_FromDate AND @RecentActivityDateFilter_ToDate" :
				!fromDate.IsEmpty ? (NoResString)" >= @RecentActivityDateFilter_FromDate" :
				!toDate.IsEmpty ? (NoResString)" <= @RecentActivityDateFilter_ToDate" :
				(NoResString)"IS NOT NULL";

			var sql = string.Format(CultureInfo.InvariantCulture, @"
{0}
IN
(
	SELECT
		Master.{1}
	FROM
		{2} Master
		LEFT JOIN dbo.vw_SalesRelationNode MasterNode ON Master.{3} = ActivityID
	WHERE

	(
		SELECT
			{4}
		FROM
		(
			SELECT
				MAX(TreeNode.ActivitySystemLastEditTime) AS LastEdit
			FROM
				dbo.vw_SalesRelationNodeWithLastEdit TreeNode
			WHERE
				MasterNode.SalesRelationTreeID = TreeNode.SalesRelationTreeID
				{5}
		) Tree
	) {6} 
)",
				orgPKColumn.Name,
				ViewSalesDashboardActivitySchema.Constants.VSA_OH,
				ViewSalesDashboardActivitySchema.Constants.TableName,
				ViewSalesDashboardActivitySchema.Constants.PK,
				lastEditColumn,
				relatedActivityTypeCondition,
				dateComparisonCondition);

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var parameters = new ZSqlParameterCollection();
			if (isSpecificType)
			{
				parameters.Add("@RecentActivityDateFilter_ActivityType", TypeProperty, ViewSalesDashboardActivitySchema.VSA_ActivityType);
			}
			if (!fromDate.IsEmpty)
			{
				parameters.Add("@RecentActivityDateFilter_FromDate", fromDate, ViewSalesDashboardActivitySchema.VSA_SystemLastEditTimeUtc);
			}
			if (!toDate.IsEmpty)
			{
				parameters.Add("@RecentActivityDateFilter_ToDate", toDate, ViewSalesDashboardActivitySchema.VSA_SystemLastEditTimeUtc);
			}
			query.AddFilterAndZSQLParameterCollection(sql, parameters);
			return query;
		}

		#endregion

	}
}
