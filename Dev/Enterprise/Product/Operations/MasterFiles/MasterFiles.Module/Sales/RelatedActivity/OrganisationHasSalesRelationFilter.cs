using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationHasSalesRelationFilter : HasSalesRelationFilter
	{
		#region Schema

		public abstract class Schema
		{
			public const string BoolProperty = "BoolProperty";
			public const string TypeProperty = "TypeProperty";
		}

		#endregion

		#region Constructor

		public OrganisationHasSalesRelationFilter(ZString descriptionPrefix, ZString description, SchemaGuidColumn orgPKColumn)
			: base(CreateDescription(descriptionPrefix, description))
		{
			MultilingualDescription = ResString.GetMultilingualString("DA136F03-AE2C-4D52-9E50-6D893C6EDB92", "Has Sales Relation");
			this.orgPKColumn = orgPKColumn;
		}

		readonly SchemaGuidColumn orgPKColumn;

		static ZString CreateDescription(ZString descriptionPrefix, ZString description)
		{
			return (descriptionPrefix.IsEmpty ? descriptionPrefix : ZString.Format(descriptionPrefix + "_")) + (description.IsEmpty ? (ZString)SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation : description);
		}

		#endregion

		#region BoolProperty

		public override ZPropertyInfo BoolPropertyInfo
		{
			get { return GetZPropertyInfo(Schema.TypeProperty); }
		}

		#endregion

		#region TypeProperty

		public override ZPropertyInfo TypePropertyInfo
		{
			get { return GetZPropertyInfo(Schema.TypeProperty); }
		}

		public override ICodeDescriptionPairList TypeList
		{
			get { return SalesRelationActivityFilterHelper.GetSalesRelationTypeList(); }
		}

		public override ModuleIdentifier ModuleId => ModuleIDs.Organisation;

		#endregion

		#region Validation

		public new OrganisationHasSalesRelationFilterValidation Validation
		{
			get { return (OrganisationHasSalesRelationFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrganisationHasSalesRelationFilterValidation(this);
		}

		#endregion

		#region Query

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
{1}
(
	SELECT VSA_OH
	FROM dbo.ViewSalesDashboardActivity
	{2}
)
",
	orgPKColumn.Name,
	BoolProperty ? "IN" : (NoResString)"NOT IN",
	isSpecificType ? (NoResString)"WHERE VSA_ActivityType = '" + TypeProperty + (NoResString)"'" : ""
	);
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
				return query;
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("BoolProperty", BoolProperty.ToString());
			writer.WriteElementString("TypeProperty", TypeProperty);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			BoolProperty = new ZBool(reader.ReadElementString("BoolProperty"));
			TypeProperty = reader.ReadElementString("TypeProperty");
		}

		#endregion

	}
}
