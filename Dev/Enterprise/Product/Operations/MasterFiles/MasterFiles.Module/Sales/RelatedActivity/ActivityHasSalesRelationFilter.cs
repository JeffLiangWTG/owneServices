using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ActivityHasSalesRelationFilter : HasSalesRelationFilter
	{
		#region Schema

		public abstract class Schema
		{
			public const string BoolProperty = "BoolProperty";
			public const string TypeProperty = "TypeProperty";
			public const string BizObjPK = "BizObjPK";
		}

		#endregion

		#region Constructor

		public ActivityHasSalesRelationFilter(BusinessObjectFactory factory, ZString description, Type elementType)
			: base(description)
		{
			this.elementType = elementType;
			HasSalesRelationFactory = factory;
		}

		readonly Type elementType;
		readonly BusinessObjectFactory HasSalesRelationFactory;

		#endregion

		#region IsActive

		protected override void OnIsActiveChanged()
		{
			base.OnIsActiveChanged();
			if (IsActive)
			{
				TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			}
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			BoolProperty = ZBool.True;
			BizObjPK = ZGuid.Empty;
			TypeProperty = ZString.Empty;
		}

		protected override bool IsEmptyCore => TypeProperty.IsEmpty || TypePropertyInfo.HasErrors();

		#endregion

		#region BoolProperty

		public override ZPropertyInfo BoolPropertyInfo
		{
			get { return GetZPropertyInfo(Schema.TypeProperty); }
		}

		#endregion

		#region TypeProperty

		[List(nameof(TypeList))]
		public override ZString TypeProperty
		{
			get { return typeProperty; }
			set
			{
				if (typeProperty != value)
				{
					typeProperty = value;

					if (typeProperty.IsEmpty || typeProperty == SalesRelationActivityFilterHelper.AnySalesRelationTypeCode)
					{
						BizObjPK = ZGuid.Empty;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateTypeProperty();
					}
					TypePropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public override ZPropertyInfo TypePropertyInfo
		{
			get { return GetZPropertyInfo(Schema.TypeProperty); }
		}

		public override ICodeDescriptionPairList TypeList
		{
			get { return SalesRelationActivityFilterHelper.GetSalesRelationTypeList(); }
		}

		public override ModuleIdentifier ModuleId
		{
			get
			{
				RelatableActivityTypeDefinition relatedActivityTypeInformation;
				if (TypeDefinitions.TryGetValue(TypeProperty, out relatedActivityTypeInformation))
				{
					return relatedActivityTypeInformation.ModuleId;
				}

				return null;
			}
		}

		#endregion

		#region BizObjPK

		[List(nameof(BizObjCollection))]
		public ZGuid BizObjPK
		{
			get { return bizObjPK; }
			set
			{
				if (bizObjPK != value)
				{
					bizObjPK = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateBizObjPK();
					}

					BizObjPKInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZGuid bizObjPK;

		public ZPropertyInfo BizObjPKInfo
		{
			get { return GetZPropertyInfo(Schema.BizObjPK); }
		}

		protected bool BizObjPK_ReadOnly
		{
			get { return TypeProperty.IsEmpty || TypeProperty == SalesRelationActivityFilterHelper.AnySalesRelationTypeCode; }
		}

		public IBusinessObjectCollection BizObjCollection
		{
			get
			{
				IBusinessObjectCollection collection = null;
				if (TypeDefinitions.TryGetValue(TypeProperty, out var relatedActivityTypeInformation))
				{
					collection = relatedActivityTypeInformation.GetNewCollection();
				}
				return collection;
			}
		}

		public IDictionary<string, RelatableActivityTypeDefinition> TypeDefinitions
		{
			get { return RelatedActivityLinkLookups.GetRelatableActivityTypeDefinitions(HasSalesRelationFactory); }
		}

		#endregion

		#region Validation

		public new ActivityHasSalesRelationFilterValidation Validation
		{
			get { return (ActivityHasSalesRelationFilterValidation)GetNewValidation(); }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ActivityHasSalesRelationFilterValidation(this);
		}

		#endregion

		#region Query

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL expression.")]
		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var isSpecificType = !TypeProperty.IsEmpty && TypeProperty != SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
				var sql = string.Format(System.Globalization.CultureInfo.InvariantCulture, @"
{0}
(
	SELECT 1
	FROM dbo.vw_SalesRelationNode TreeNode
	WHERE
		MasterNode.ActivityID <> TreeNode.ActivityID
		AND
		{1}
		{2}
		{3}
)",
	BoolProperty ? "EXISTS" : (NoResString)"ActivityID IS NULL OR NOT EXISTS",
	typeof(IGlbCompanyCampaign).IsAssignableFrom(elementType) ? GetCampaignElementTypeQuerySql : (NoResString)"MasterNode.SalesRelationTreeID = TreeNode.SalesRelationTreeID",
	isSpecificType && BizObjPK.IsEmpty ? "AND TreeNode.ActivityType = '" + TypeProperty + "'" : "",
	!BizObjPK.IsEmpty ? @"
AND
(
	TreeNode.ActivityID = '" + BizObjPK + "'" +
	GetCascadingCampaignSQL(TypeProperty, BizObjPK) + @"
)" : "");

				var query = new ZDBOnlyQuery(elementType);
				query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

				return query;
			}
		}

		string GetCascadingCampaignSQL(ZString typeProperty, ZGuid bizOPK)
		{
			string sql = string.Empty;

			if (!bizOPK.IsEmpty && typeProperty == RelatableActivityTypeList.Codes.CampaignManagement)
			{
				sql = @"
 OR
 (
	 TreeNode.ActivityTableCode = 'G8'
	 AND
	 TreeNode.ActivityID IN 
	 (
		 SELECT G8_PK FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = '" + bizOPK + @"'
	 )
 )";
			}

			return sql;
		}

		string GetCampaignElementTypeQuerySql
		{
			get
			{
				return @"
		(
			MasterNode.SalesRelationTreeID = TreeNode.SalesRelationTreeID
			OR
			MasterNode.SalesRelationTreeID IN
			(
				SELECT SalesRelationTreeID FROM dbo.vw_SalesRelationNode SubMasterNode 
				WHERE 
				SubMasterNode.ActivityID IN
				(
					SELECT G8_G0 FROM dbo.GlbCompanyCampaignItem WHERE G8_PK = TreeNode.SalesRelationTreeID
				)
			)
		)";
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("BoolProperty", BoolProperty.ToString());
			writer.WriteElementString("TypeProperty", TypeProperty);
			writer.WriteElementString("BizObjPK", BizObjPK.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			BoolProperty = new ZBool(reader.ReadElementString("BoolProperty"));
			TypeProperty = reader.ReadElementString("TypeProperty");
			BizObjPK = new ZGuid(reader.ReadElementString("BizObjPK"));
		}

		#endregion
	}
}
