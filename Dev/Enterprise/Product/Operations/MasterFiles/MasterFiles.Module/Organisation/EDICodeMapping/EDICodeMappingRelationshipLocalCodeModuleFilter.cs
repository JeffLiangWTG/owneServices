using System.Collections.Generic;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class EDICodeMappingRelationshipLocalCodeModuleFilter : ModuleTextFilter
	{
		public EDICodeMappingRelationshipLocalCodeModuleFilter(ZString description, OrgPatternMatchOverride orgPatternMatchOverride)
			: base(description, EmptyQuery, orgPatternMatchOverride.Lookups.OO_Relationship_List)
		{
			this.orgPatternMatchOverride = orgPatternMatchOverride;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			ComparisonConstants.Exact
		};

		[BusinessObjectTestExclude]
		[List("RelationshipsList")]
		public ZString Relationship
		{
			get => fRelationship;
			set
			{
				SetNonPersistentPropertyValue(RelationshipInfo, ref fRelationship, value);

				ComparisonOperatorForOrgCo = ComparisonOperatorsForOrgCosList.Count > 0 ? ComparisonOperatorsForOrgCosList[0].Code : string.Empty;
				OrgCoName = ZString.Empty;
				OrgCoGuid = ZGuid.Empty;
			}
		}
		ZString fRelationship;

		public ZPropertyInfo RelationshipInfo => GetZPropertyInfo(Schema.Relationship);

		public ICodeDescriptionPairList RelationshipsList => orgPatternMatchOverride.Lookups.OO_Relationship_List;

		[BusinessObjectTestExclude]
		[List("ComparisonOperatorsForOrgCosList")]
		public ZString ComparisonOperatorForOrgCo
		{
			get => fComparisonOperatorForOrgCo;
			set
			{
				if (ComparisonOperatorsForOrgCosList.ContainsCode(value))
				{
					SetNonPersistentPropertyValue(ComparisonOperatorForOrgCoInfo, ref fComparisonOperatorForOrgCo, value);
				}
				else
				{
					SetNonPersistentPropertyValue(ComparisonOperatorForOrgCoInfo, ref fComparisonOperatorForOrgCo, ComparisonConstants.Exact);
				}
			}
		}
		ZString fComparisonOperatorForOrgCo;

		public ZPropertyInfo ComparisonOperatorForOrgCoInfo => GetZPropertyInfo(Schema.ComparisonOperatorForOrgCo);

		public CodeDescriptionPairList ComparisonOperatorsForOrgCosList
		{
			get
			{
				if (EDICodeMappingHelper.IsGuidFindBox(Relationship))
				{
					if (fComparisonOperatorsForOrgCoGuidList == null)
					{
						fComparisonOperatorsForOrgCoGuidList = new CodeDescriptionPairList();
						foreach (var code in AllowedComparisonOperatorsForOrgCoGuid)
						{
							fComparisonOperatorsForOrgCoGuidList.Add(ComparisonConstants.GetComparisonOperatorPair(code));
						}
					}

					return fComparisonOperatorsForOrgCoGuidList;
				}
				else
				{
					if (fComparisonOperatorsForOrgCoNameList == null)
					{
						fComparisonOperatorsForOrgCoNameList = new CodeDescriptionPairList();
						foreach (var code in AllowedComparisonOperatorsForOrgCoName)
						{
							fComparisonOperatorsForOrgCoNameList.Add(ComparisonConstants.GetComparisonOperatorPair(code));
						}
					}

					return fComparisonOperatorsForOrgCoNameList;
				}
			}
		}

		CodeDescriptionPairList fComparisonOperatorsForOrgCoNameList;
		CodeDescriptionPairList fComparisonOperatorsForOrgCoGuidList;

		static IEnumerable<string> AllowedComparisonOperatorsForOrgCoName => new[]
		{
			ComparisonConstants.Exact,
			ComparisonConstants.NotEqual,
			ComparisonConstants.StartsWith,
			ComparisonConstants.Contains,
			ComparisonConstants.IsBlank
		};

		static IEnumerable<string> AllowedComparisonOperatorsForOrgCoGuid => new[]
		{
			ComparisonConstants.Exact
		};

		protected bool ComparisonOperatorForOrgCo_ReadOnly => RelationshipInfo.HasErrors() || Relationship == ZString.Empty;

		[BusinessObjectTestExclude]
		[List("OrgCoNamesOrGuidList")]
		public ZString OrgCoName
		{
			get => OrgCoNameInfo.ReadOnly ? ZString.Empty : fOrgCoName;
			set => SetNonPersistentPropertyValue(OrgCoNameInfo, ref fOrgCoName, value);
		}
		ZString fOrgCoName;

		public ZPropertyInfo OrgCoNameInfo => GetZPropertyInfo(Schema.OrgCoName);

		protected bool OrgCoName_ReadOnly =>
			ComparisonOperatorForOrgCoInfo.ReadOnly ||
			EDICodeMappingHelper.IsGuidFindBox(Relationship) ||
			ComparisonOperatorForOrgCo == ComparisonConstants.IsBlank;

		[BusinessObjectTestExclude]
		[List("OrgCoNamesOrGuidList")]
		public ZGuid OrgCoGuid
		{
			get => OrgCoGuidInfo.ReadOnly ? ZGuid.Empty : fOrgCoGuid;
			set => SetNonPersistentPropertyValue(OrgCoGuidInfo, ref fOrgCoGuid, value);
		}
		ZGuid fOrgCoGuid;

		public ZPropertyInfo OrgCoGuidInfo => GetZPropertyInfo(Schema.OrgCoGuid);

		protected bool OrgCoGuid_ReadOnly =>
			ComparisonOperatorForOrgCoInfo.ReadOnly ||
			!EDICodeMappingHelper.IsGuidFindBox(Relationship);

		public object OrgCoNamesOrGuidList
		{
			get
			{
				orgPatternMatchOverride.OO_Relationship = Relationship.SubstringSafe(0, OrgPatternMatchOverride.Schema.OO_RelationshipMaxLength);
				return orgPatternMatchOverride.Lookups.OrgCoNames;
			}
		}

		public new EDICodeMappingRelationshipLocalCodeModuleFilterValidation Validation => (EDICodeMappingRelationshipLocalCodeModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new EDICodeMappingRelationshipLocalCodeModuleFilterValidation(this);

		readonly OrgPatternMatchOverride orgPatternMatchOverride;

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			var query = new ZDBOnlyQuery(typeof(OrgPatternMatchOverride));

			query.AddToFilter(GetRelationshipQuery());
			query.AddToFilter(GetLocalCodeQuery());

			return query;
		}

		ZQuery GetRelationshipQuery()
		{
			if (Relationship == ZString.Empty)
			{
				return new ZQuery();
			}

			return new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, GetSqlComparisonOperator(ComparisonOperator, SQLComparisonOperator.NotSpecified), Relationship);
		}

		ZQuery GetLocalCodeQuery()
		{
			var sqlComparisonOperator = GetSqlComparisonOperator(ComparisonOperatorForOrgCo, SQLComparisonOperator.NotSpecified);

			var query = new ZQuery();

			if (EDICodeMappingHelper.IsGuidFindBox(Relationship) && OrgCoGuid != ZGuid.Empty)
			{
				query = new ZQuery(OrgPatternMatchOverrideSchema.OO_LocalGuid, sqlComparisonOperator, OrgCoGuid);
			}
			else
			{
				if (sqlComparisonOperator == SQLComparisonOperator.IsBlank)
				{
					query = new ZQuery(OrgPatternMatchOverrideSchema.OO_LocalCode, sqlComparisonOperator, ZString.Empty);
				}
				else if (OrgCoName != ZString.Empty)
				{
					query = new ZQuery(OrgPatternMatchOverrideSchema.OO_LocalCode, sqlComparisonOperator, OrgCoName);
				}
			}

			return query;
		}

		public static class Schema
		{
			public const string OrgCoGuid = "OrgCoGuid";
			public const string OrgCoName = "OrgCoName";
			public const string Relationship = "Relationship";
			public const string ComparisonOperatorForOrgCo = "ComparisonOperatorForOrgCo";
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.Relationship, Relationship);
			writer.WriteElementString(Schema.ComparisonOperatorForOrgCo, ComparisonOperatorForOrgCo);
			writer.WriteElementString(Schema.OrgCoGuid, OrgCoGuid.ToString());
			writer.WriteElementString(Schema.OrgCoName, OrgCoName);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			Relationship = reader.ReadElementString(Schema.Relationship);
			ComparisonOperatorForOrgCo = reader.ReadElementString(Schema.ComparisonOperatorForOrgCo);

			if (ZGuid.TryParse(reader.ReadElementString(Schema.OrgCoGuid), out var guid))
			{
				OrgCoGuid = guid;
			}

			OrgCoName = reader.ReadElementString(Schema.OrgCoName);
		}
	}
}
