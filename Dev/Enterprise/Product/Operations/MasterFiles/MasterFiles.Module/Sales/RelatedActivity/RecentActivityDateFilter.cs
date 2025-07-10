using System;
using System.Globalization;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RecentActivityDateFilter : ModuleDateFilter
	{
		#region Schema

		public abstract class Schema
		{
			public const string TypeProperty = "TypeProperty";
		}

		#endregion

		#region Constructor

		public RecentActivityDateFilter(ZString description, ZString tableCode, SchemaDateTimeColumn activityLastEditColumn, Type elementType)
			: base(description, true, false)
		{
			Argument.NotNull(elementType, "elementType");
			Argument.NotNull(tableCode, "tableCode");

			this.activityLastEditColumn = activityLastEditColumn;
			this.elementType = elementType;

			HideFutureDates = true;
			PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		}

		readonly SchemaDateTimeColumn activityLastEditColumn;
		readonly Type elementType;

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.SalesRelationActivity; }
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			TypeProperty = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore || TypePropertyInfo.HasErrors();

		public bool IsDateEmtpy => IsEmpty;

		#endregion

		#region TypeProperty

		[List("TypeList")]
		public ZString TypeProperty
		{
			get { return typeProperty; }
			set
			{
				if (typeProperty != value)
				{
					typeProperty = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTypeProperty();
					}
					TypePropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString typeProperty;

		public ZPropertyInfo TypePropertyInfo
		{
			get { return GetZPropertyInfo(Schema.TypeProperty); }
		}

		public ICodeDescriptionPairList TypeList
		{
			get { return SalesRelationActivityFilterHelper.GetSalesRelationTypeList(); }
		}

		#endregion

		#region Validation

		public new RecentActivityDateFilterValidation Validation
		{
			get { return (RecentActivityDateFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new RecentActivityDateFilterValidation(this);
		}

		#endregion

		#region Query

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL expression")]
		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var fromDate = FromDate;
			var toDate = ToDate;

			var isSpecificType = !TypeProperty.IsEmpty && TypeProperty != SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			var lastEditColumn = isSpecificType || activityLastEditColumn == null ? "MAX(Tree.LastEdit)" : (NoResString)"COALESCE(MAX(Tree.LastEdit), Master." + activityLastEditColumn.Name + (NoResString)")";
			var relatedActivityTypeCondition = isSpecificType ? (NoResString)"AND ActivityType = @RecentActivityDateFilter_ActivityType" : "";
			var dateComparisonCondition =
				!fromDate.IsEmpty && !toDate.IsEmpty ? "BETWEEN @RecentActivityDateFilter_FromDate AND @RecentActivityDateFilter_ToDate" :
				!fromDate.IsEmpty ? " >= @RecentActivityDateFilter_FromDate" :
				!toDate.IsEmpty ? " <= @RecentActivityDateFilter_ToDate" :
				"IS NOT NULL";

			var sql = string.Format(CultureInfo.InvariantCulture, @"
(
	SELECT
		{0}
	FROM
	(
		SELECT
			MAX(TreeNode.ActivitySystemLastEditTime) AS LastEdit
		FROM
			dbo.vw_SalesRelationNodeWithLastEdit TreeNode
		WHERE
			MasterNode.SalesRelationTreeID = TreeNode.SalesRelationTreeID
			{1}
	) Tree
) {2} ",
				lastEditColumn,
				relatedActivityTypeCondition,
				dateComparisonCondition);

			var query = new ZDBOnlyQuery(elementType);
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

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("TypeProperty", TypeProperty);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			TypeProperty = reader.ReadElementString("TypeProperty");
		}

		#endregion
	}
}
