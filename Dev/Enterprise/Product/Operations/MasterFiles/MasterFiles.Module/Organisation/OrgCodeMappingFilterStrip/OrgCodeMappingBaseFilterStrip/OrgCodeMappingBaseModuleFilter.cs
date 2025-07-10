using System.Collections;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public abstract class OrgCodeMappingBaseModuleFilter : ModuleTextFilter
	{
		#region Construction

		protected OrgCodeMappingBaseModuleFilter(ZString description)
			: base(description, DummyQuery)
		{
		}

		protected OrgCodeMappingBaseModuleFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		#endregion

		#region Schema

		public abstract class BaseCodeMappingSchema
		{
			public const string RelationshipType = nameof(RelationshipType);
			public const string Context = nameof(Context);
		}

		#endregion

		#region Properties

		#region RelationshipType

		[List("RelationshipTypeList")]
		[MaxLength(3)]
		public ZString RelationshipType
		{
			get { return relationshipType; }
			set
			{
				InvalidateCachedQuery();
				if (SetNonPersistentPropertyValue(RelationshipTypeInfo, ref relationshipType, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateRelationshipType();
					}
					RelationshipTypeInfo.RefreshBinding();
				}
			}
		}

		ZString relationshipType;

		public ZPropertyInfo RelationshipTypeInfo => GetZPropertyInfo(BaseCodeMappingSchema.RelationshipType);

		public CodeDescriptionPairList RelationshipTypeList => relationshipTypeList ?? (relationshipTypeList = OrgPatternMatchOverrideLookups.GetRelationshipCodeDescriptionList());
		CodeDescriptionPairList relationshipTypeList;

		#endregion

		#region Context

		[List("ContextList")]
		[MaxLength(3)]
		public ZString Context
		{
			get { return context; }
			set
			{
				InvalidateCachedQuery();
				if (SetNonPersistentPropertyValue(ContextInfo, ref context, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateContext();
					}
					ContextInfo.RefreshBinding();
				}
			}
		}
		ZString context;

		public ZPropertyInfo ContextInfo => GetZPropertyInfo(BaseCodeMappingSchema.Context);

		public CodeDescriptionPairList ContextList => contextList ?? (contextList = new CodeDescriptionPairList(OrganisationsDataRegistry.Instance.UserDefinedContext.Value));
		CodeDescriptionPairList contextList;

		#endregion

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			RelationshipType = string.Empty;
			Context = string.Empty;
		}

		#endregion

		#region Dummies

		protected static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			if (!IsEmpty)
			{
				var subQuery = GetCodeMappingSubQuery();
				subQuery.AddToFilter(GetCodeMappingFilter());
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			return query;
		}

		protected abstract ZQuery GetCodeMappingFilter();

		ZDBOnlySubQuery GetCodeMappingSubQuery()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(OrgPatternMatchOverride), OrgPatternMatchOverrideSchema.OO_OH);

			if (!string.IsNullOrEmpty(RelationshipType))
			{
				subQuery.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, RelationshipType);
			}
			if (!string.IsNullOrEmpty(Context))
			{
				subQuery.AddToFilter(OrgPatternMatchOverrideSchema.OO_Context, Context);
			}

			return subQuery;
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => RelationshipType.IsEmpty && Context.IsEmpty;

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(BaseCodeMappingSchema.RelationshipType, RelationshipType);
			writer.WriteElementString(BaseCodeMappingSchema.Context, Context);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			RelationshipType = reader.ReadElementString(BaseCodeMappingSchema.RelationshipType);
			Context = reader.ReadElementString(BaseCodeMappingSchema.Context);
		}

		public new OrgCodeMappingBaseModuleFilterValidation Validation => (OrgCodeMappingBaseModuleFilterValidation)base.Validation;

		#endregion
	}
}
