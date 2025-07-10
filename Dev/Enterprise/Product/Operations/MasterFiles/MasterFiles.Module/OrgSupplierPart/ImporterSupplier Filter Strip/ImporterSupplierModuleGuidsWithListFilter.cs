using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ImporterSupplierModuleGuidsWithListFilter : ModuleGuidsFilter
	{
		public ImporterSupplierModuleGuidsWithListFilter(OrgHeaderCollection consignees, OrgHeaderCollection consignors, OrgSupplierPartFilterStripBusinessObject filter)
			: base("Importer/Supplier", ModuleIDs.Organisation, OrgPartRelationSchema.OU_OH, consignees, OrgPartRelationSchema.OU_OH, consignors)
		{
			this.MultilingualDescription = ResString.GetMultilingualString("cc8490d2-a61b-420f-a899-1a74443c8815", "Importer/Supplier");
			this.Filter = filter;
		}
		readonly OrgSupplierPartFilterStripBusinessObject Filter;

		[List("FilterConditionList")]
		[BusinessObjectTestExclude] // we store "?" when setting an invalid value
		[MaxLength(6)]
		public ZString FilterCondition
		{
			get { return filterCondition; }
			set
			{
				if (filterCondition != value)
				{
					if (!value.IsEmpty &&
						!value.EqualsIgnoringCase(ImporterSuplierFilterConditions.Codes.Loose) &&
						!value.EqualsIgnoringCase(ImporterSuplierFilterConditions.Codes.Exact) &&
						!value.EqualsIgnoringCase(ImporterSuplierFilterConditions.Codes.Exclusive) &&
						!value.EqualsIgnoringCase(ImporterSuplierFilterConditions.Codes.Both))
					{
						SetNonPersistentPropertyValue(FilterConditionInfo, ref filterCondition, "?");
					}
					else
					{
						SetNonPersistentPropertyValue(FilterConditionInfo, ref filterCondition, value);
					}

					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo FilterConditionInfo
		{
			get { return GetZPropertyInfo(nameof(FilterCondition)); }
		}

		public CodeDescriptionPairList FilterConditionList
		{
			get
			{
				if (filterConditionList == null)
				{
					filterConditionList = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair pair in new ImporterSuplierFilterConditions())
					{
						filterConditionList.Add(GetFilterConditionPair(pair.Code));
					}
				}
				return filterConditionList;
			}
		}

		ZString filterCondition;
		CodeDescriptionPairList filterConditionList;

		public CodeDescriptionPair GetFilterConditionPair(string code)
		{
			switch (code)
			{
				case ImporterSuplierFilterConditions.Codes.Both:
					return new CodeDescriptionPair(ResString.GetMultilingualString("ImporterSuplierFilterConditions.Code|Both", "both"), ImporterSuplierFilterConditions.Descriptions.Both);
				case ImporterSuplierFilterConditions.Codes.Exclusive:
					return new CodeDescriptionPair(ResString.GetMultilingualString("ImporterSuplierFilterConditions.Code|Exclusive", "excl."), ImporterSuplierFilterConditions.Descriptions.Exclusive);
				case ImporterSuplierFilterConditions.Codes.Loose:
					return new CodeDescriptionPair(ResString.GetMultilingualString("ImporterSuplierFilterConditions.Code|Loose", "either"), ImporterSuplierFilterConditions.Descriptions.Loose);
				default:
					return new CodeDescriptionPair(ResString.GetMultilingualString("ImporterSuplierFilterConditions.Code|Exact", "exact"), ImporterSuplierFilterConditions.Descriptions.Exact);
			}
		}

		protected override bool ShouldReevaluateQuery()
		{
			return true;
		}

		protected override ZQuery GetQuery()
		{
			var result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.And;

			var ownerPKs = GetOrgAndItsParents(Property1);
			var supplierPKs = GetOrgAndItsParents(Property2);

			if (FilterCondition == ImporterSuplierFilterConditions.Codes.Exact && (!Property1.IsEmpty || !Property2.IsEmpty))
			{
				result = GetMatchingQueryForOwnerAndSupplierSpecified(ownerPKs, supplierPKs, true);
			}
			else if (FilterCondition == ImporterSuplierFilterConditions.Codes.Exclusive && (!Property1.IsEmpty || !Property2.IsEmpty))
			{
				result = GetMatchingQueryForOwnerAndSupplierSpecified(ownerPKs, supplierPKs, false);
			}
			else if (FilterCondition == ImporterSuplierFilterConditions.Codes.Both && (!Property1.IsEmpty || !Property2.IsEmpty))
			{
				result = GetMatchingQueryForBothOwnerAndSupplierRequired(ownerPKs, supplierPKs);
			}
			else
			{
				if (ownerPKs.Any(x => !x.IsEmpty))
				{
					result.AddToFilter(GetProductRelationshipFilter(ownerPKs, OrgPartRelation.RelationshipTypes.Owner, FilterCondition));
				}
				if (supplierPKs.Any(x => !x.IsEmpty))
				{
					result.AddToFilter(GetProductRelationshipFilter(supplierPKs, OrgPartRelation.RelationshipTypes.Supplier, FilterCondition), JoinCondition.Or);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:Do Not Use Loop To Add Or Conditions To Filter", Justification = "Need to specify SqlComparisonOperator")]
		ZString GetProductCodeAndDescriptionPredicates()
		{
			ZString result = ZString.Empty;

			if (Filter != null)
			{
				var subQueries = new Dictionary<Enum, ZQuery>();
				var query = new ZQuery();
				foreach (var filter in Filter.ActiveModuleFilters)
				{
					if (filter is ModuleTextFilter textFilter && !textFilter.Property.IsEmpty)
					{
						SchemaStringColumn schemaColumn = null;
						if (textFilter.Description.StartsWith(OrgSupplierPartFilterStripBusinessObject.Descriptions.ProductCode))
						{
							schemaColumn = OrgSupplierPartSchema.OP_PartNum;
						}
						else if (textFilter.Description.StartsWith(OrgSupplierPartFilterStripBusinessObject.Descriptions.ProductDescription))
						{
							schemaColumn = OrgSupplierPartSchema.OP_Desc;
						}

						if (schemaColumn != null)
						{
							if (textFilter.OrCategory == FilterOrCategory.None)
							{
								query.AddToFilter(schemaColumn, textFilter.SqlComparisonOperator, textFilter.Property);
							}
							else if (subQueries.TryGetValue(textFilter.OrCategory, out var existingQuery))
							{
								existingQuery.AddToFilter(JoinCondition.Or, schemaColumn, textFilter.SqlComparisonOperator, textFilter.Property);
							}
							else
							{
								subQueries.Add(textFilter.OrCategory, new ZQuery(schemaColumn, textFilter.SqlComparisonOperator, textFilter.Property));
							}
						}
					}
				}

				foreach (var subQuery in subQueries.Values)
				{
					query.AddToFilter(subQuery);
				}

				var queryText = query.LiteralTextADO;
				if (!string.IsNullOrEmpty(queryText))
				{
					result = $" AND ({queryText})";
				}
			}

			return result;
		}

		ZQuery GetMatchingQueryForBothOwnerAndSupplierRequired(ZGuid[] ownerPKs, ZGuid[] supplierPKs)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var paramsCollection = new ZSqlParameterCollection();
			var ownerPKsParameter = ZSqlParameter.New("@AllOwnersPKTable", ownerPKs, OrgPartRelationSchema.OU_OH, isTableValued: true);
			var supplierPKsParameter = ZSqlParameter.New("@AllSuppliersPKTable", supplierPKs, OrgPartRelationSchema.OU_OH, isTableValued: true);
			paramsCollection.Add(ownerPKsParameter);
			paramsCollection.Add(supplierPKsParameter);
			paramsCollection.Add("@relationshipOWN", "OWN", OrgPartRelationSchema.OU_Relationship);
			paramsCollection.Add("@relationshipSUP", "SUP", OrgPartRelationSchema.OU_Relationship);
			paramsCollection.Add("@relationshipBTH", "BTH", OrgPartRelationSchema.OU_Relationship);

			ZString predicate = GetProductCodeAndDescriptionPredicates();

			result.AddFilterAndZSQLParameterCollection(ZString.Format(@"
OP_PK IN 
(
	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK

	JOIN 

	(
		SELECT OP_PK
		FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
		WHERE (OU_Relationship = @relationshipSUP OR OU_Relationship = @relationshipBTH) AND OU_OH IN (SELECT Value FROM @AllSuppliersPKTable){0}
	) AS relation2 ON OrgSupplierPart.OP_PK = relation2.OP_PK

	WHERE (OU_Relationship = @relationshipOWN OR OU_Relationship = @relationshipBTH) AND OU_OH IN (SELECT Value FROM @AllOwnersPKTable)

	UNION ALL

	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
	WHERE (OU_Relationship = @relationshipOWN OR OU_Relationship = @relationshipBTH) AND OU_OH IN (SELECT Value FROM @AllOwnersPKTable){0}

	AND OrgSupplierPart.OP_PK NOT IN
		(
			SELECT OU_OP 
			FROM dbo.OrgPartRelation 
			WHERE OU_Relationship = @relationshipSUP 
			OR 
			(OU_Relationship = @relationshipBTH AND OU_OH NOT IN (SELECT Value FROM @AllOwnersPKTable))) 

	UNION ALL

	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
	WHERE (OU_Relationship = @relationshipSUP OR OU_Relationship = @relationshipBTH) AND OU_OH IN (SELECT Value FROM @AllSuppliersPKTable){0}

	AND OrgSupplierPart.OP_PK NOT IN
	(
		SELECT OU_OP 
		FROM dbo.OrgPartRelation 
		WHERE OU_Relationship = @relationshipOWN 
		OR 
		(OU_Relationship = @relationshipBTH AND OU_OH NOT IN (SELECT Value FROM @AllSuppliersPKTable))) 
)", predicate), paramsCollection);

			return result;
		}

		ZQuery GetMatchingQueryForOwnerAndSupplierSpecified(ZGuid[] ownerPKs, ZGuid[] supplierPKs, bool includeBothRelationship)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var ownerSubQuery = GetRelationshipSubquery(ownerPKs, supplierPKs, includeBothRelationship, OrgPartRelation.RelationshipTypes.Owner);
			result.AddSubQuery(ownerSubQuery, JoinCondition.And);

			var supplierSubQuery = GetRelationshipSubquery(supplierPKs, ownerPKs, includeBothRelationship, OrgPartRelation.RelationshipTypes.Supplier);
			result.AddSubQuery(supplierSubQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlySubQuery GetRelationshipSubquery(ZGuid[] matchedPKs, ZGuid[] notEqualPKs, bool includeBothRelationship, ZString relationType)
		{
			var isMatchedPKsEmpty = matchedPKs.Length > 0 && matchedPKs.All(x => x.IsEmpty);
			var subQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, isMatchedPKsEmpty);

			var additionalFilter = new ZQuery(OrgPartRelationSchema.OU_Relationship, relationType);
			if (!isMatchedPKsEmpty)
			{
				additionalFilter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, matchedPKs);
			}
			if (includeBothRelationship)
			{
				var bothFilter = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
				if (!isMatchedPKsEmpty)
				{
					bothFilter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, matchedPKs);
				}
				else
				{
					bothFilter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, SQLComparisonOperator.NotEqual, notEqualPKs);
				}

				additionalFilter.AddToFilter(bothFilter, JoinCondition.Or);
			}

			subQuery.AddToFilter(additionalFilter, JoinCondition.And);
			return subQuery;
		}

		ZQuery GetProductRelationshipFilter(ZGuid[] orgPKs, ZString relationship, ZString condition)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var subQueryOrganisation = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			subQueryOrganisation.AddToFilter(OrgPartRelationSchema.OU_OH, orgPKs);
			if (relationship == OrgPartRelation.RelationshipTypes.Owner || relationship == OrgPartRelation.RelationshipTypes.Supplier)
			{
				var bothRelationship = new ZQuery(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, relationship);
				if (condition != ImporterSuplierFilterConditions.Codes.Exclusive)
				{
					bothRelationship.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);
				}
				subQueryOrganisation.AddToFilter(bothRelationship, JoinCondition.And);
			}
			else
			{
				subQueryOrganisation.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, relationship);
			}
			result.AddSubQuery(subQueryOrganisation, JoinCondition.And);
			return result;
		}

		ZGuid[] GetOrgAndItsParents(ZGuid orgPK)
		{
			var result = new List<ZGuid>();
			if (!orgPK.IsEmpty)
			{
				var factory = CreateNewFactory();
				var loader = new OrgRelatedParty.Loader(factory);
				result.AddRange(loader.LoadPartiesWithRelatedOrgAndType(orgPK, RelatedPartyTypeList.Codes.ProductRelationship)
					.Select(p => p.PR_OH_Parent)
					.Where(z => !z.IsEmpty));
			}
			return result.Prepend(orgPK).ToArray();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			FilterCondition = ImporterSuplierFilterConditions.Codes.Exact;
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("FilterCondition", FilterCondition.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			FilterCondition = reader.ReadElementString("FilterCondition");
		}
	}
}
