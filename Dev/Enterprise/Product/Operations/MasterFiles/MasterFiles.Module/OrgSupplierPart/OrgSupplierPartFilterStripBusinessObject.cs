using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related description, Filter name")]
		public static class Descriptions
		{
			public const string LocalPartNo = "Local Part #";
			public const string ProductCode = "Product Code";
			public const string ProductDescription = "Product Description";
			public const string DynamicPickFaceArea = "Dynamic Pick Face Area";
			internal const string ActiveStatus = "Active Status";
			public const string ClassificationOrganizations = "Classification Organizations";
		}

		#endregion

		protected sealed override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = GetOrgSupplierPartModuleFiltersCore();
			AddWorkflowCustomFieldsFilters(result);

			return result;
		}

		protected virtual void AddWorkflowCustomFieldsFilters(ModuleFilterCollection collection)
		{
			collection.AddWorkflowCustomFieldsFilters(Factory, WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode, typeof(OrgSupplierPart));
		}

		protected virtual ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var localPartSubGroup = new LocalPartSubGroup();

			result.AddTextFilter(Descriptions.ProductCode, OrgSupplierPartSchema.OP_PartNum).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ProductCode", "Product Code");
			result.AddTextFilter(Descriptions.ProductDescription, OrgSupplierPartSchema.OP_Desc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ProductDescription", "Product Description");
			result.AddTextFilter("Model", OrgSupplierPartSchema.OP_Model).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|Model", "Model");
			result.AddTextFilter("Brand", OrgSupplierPartSchema.OP_Brand).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|Brand", "Brand");
			var localPart = result.AddTextFilter(Descriptions.LocalPartNo, OrgPartRelationSchema.OU_LocalPartNumber);
			localPart.MultilingualDescription = ResString.GetMultilingualString("5CA247CC-EDB9-4152-A16D-A20B384C85FF", "Local Part #");
			localPart.Category = FilterCategories.NumbersAndReferences;
			localPart.SubGroup = localPartSubGroup;
			var localPartDescription = result.AddTextFilter("Local Part Description", OrgPartRelationSchema.OU_LocalPartDescription);
			localPartDescription.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|LocalPartDescription", "Local Part Description");
			localPartDescription.SubGroup = localPartSubGroup;
			result.AddFlagsFilter("For Resale", new string[] { Res.GetString("MasterFiles|OrgSupplierPartFilter|ForResale", "For Resale") }, new SchemaBoolColumn[] { OrgSupplierPartSchema.OP_CanResell }).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ForResale", "For Resale");
			result.AddDateFilter("Customs Job Created", GetCustomsJobCreatedQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|CustomsJobCreated", "Customs Job Created");
			result.AddDateFilter("Warehouse Job Created", GetWarehouseJobCreatedQuery, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|WarehouseJobCreated", "Warehouse Job Created");

			var filter = result.AddNkFilter("Commodity Code", GetOrdersByCommodityCodeQuery, ModuleIDs.RefCommodityCode, RefCommodity_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|CommodityCode", "Commodity Code");
			filter.Category = FilterCategories.NumbersAndReferences;

			var productCategoryFilter = result.AddGuidFilter("Product Category", ModuleIDs.RefOrgPartCategory, GetProductCategoryQuery, GetProductCategories);
			productCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("CDBD1E29-8004-4038-99B7-BCE963B6714C", "Product Category");
			productCategoryFilter.IsPublishedOnWeb = false;

			var dynamicPickFaceAreaFilter = result.AddGuidFilter(Descriptions.DynamicPickFaceArea, ModuleIDs.WhsConfigArea, GetDynamicProductAreaQuery, GetAreas);
			dynamicPickFaceAreaFilter.MultilingualDescription = ResString.GetMultilingualString("CFAAE10C-4EAF-40B4-A8B5-C419AB29EB4B", Descriptions.DynamicPickFaceArea);
			dynamicPickFaceAreaFilter.IsPublishedOnWeb = false;
			dynamicPickFaceAreaFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|DynamicPickFaceArea", "Dynamic Pick Face Area");

			var importerSupplierFilter = new ImporterSupplierModuleGuidsWithListFilter(ConsigneesAndWarehouses, Consignors, this);
			importerSupplierFilter.SetItemDescriptions(Res.GetData("MasterFiles|OrgSupplierPartFilter|Owner", "Owner"), Res.GetData("MasterFiles|OrgSupplierPartFilter|Supplier", "Supplier"));
			importerSupplierFilter.IsPublishedOnWeb = false;
			importerSupplierFilter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			importerSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ImporterSupplier", "Importer/Supplier");
			result.AddCustomFilter(importerSupplierFilter);

			var classificationOrganisationFilter = result.AddGuidFilter(Descriptions.ClassificationOrganizations, ModuleIDs.Organisation, GetClassificationOrganizationsQuery, AllOrgs);
			classificationOrganisationFilter.IsPublishedOnWeb = false;
			classificationOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("97F554D5-9E63-4065-92D9-616AC2F36AF0", Descriptions.ClassificationOrganizations);

			result.AddCustomFilter(new ABCCategoryWarehouseFilter());

			if (IsWarehouseClientSecurityRequired)
			{
				importerSupplierFilter.Visibility = FilterVisibility.AlwaysVisible;
				importerSupplierFilter.Property1Validation = ImporterFilterValidation;
				importerSupplierFilter.Property2Validation = SupplierFilterValidation;
			}

			AddBOMFilters(result);

			return result;
		}

		ZQuery GetDuplicateQuery(ZBool value)
		{
			if (!value)
			{
				return new ZQuery();
			}

			const string sql = @"
				OP_PK in (select OP_PK from
				(
					select OP_PK, count(*) over(PARTITION BY OP_PartNum, OU_OH) as DUP_Count
					from dbo.OrgSupplierPart
					inner join dbo.OrgPartRelation on OU_OP = OP_PK
					where OP_IsActive = 1
					and OU_Relationship in ('OWN', 'BTH')
					AND OP_PartNum in
					(
						select OP_PartNum from dbo.OrgSupplierPart 
						group by OP_PartNum having count(*) > 1
					)
				) DUP where DUP.DUP_Count > 1)
			";

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			result.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
			return result;
		}

		bool IsWarehouseClientSecurityRequired
		{
			get { return !Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb && ((IFilterStripBusinessObjectInternals)this).LayoutContext == ModuleIDs.WhsConfigProduct.Name; }
		}

		public override bool IsExpensiveQuery
		{
			get
			{
				bool result = false;
				if (!OverrideIsIsExpensiveQuery)
				{
					result = base.IsExpensiveQuery;
				}
				return result;
			}
		}
		public bool OverrideIsIsExpensiveQuery;

		#region Lookups

		public RefCommodityCodeCollection RefCommodity_List
		{
			get
			{
				if (fRefCommodity_List == null)
				{
					fRefCommodity_List = new RefCommodityCodeCollection(Factory);
				}
				return fRefCommodity_List;
			}
		}
		RefCommodityCodeCollection fRefCommodity_List;

		OrgHeaderCollection AllOrgs
		{
			get
			{
				if (IsWarehouseClientSecurityRequired)
				{
					return new WarehouseClientCollectionWithSecurityCheck(Factory);
				}
				else
				{
					return new OrgHeaderCollection(Factory);
				}
			}
		}

		OrgHeaderCollection Consignors
		{
			get
			{
				if (IsWarehouseClientSecurityRequired)
				{
					return new WarehouseClientCollectionWithSecurityCheck(Factory);
				}
				else
				{
					return new ConsignorCollection(Factory);
				}
			}
		}

#if DEBUG
		public
#endif
 OrgHeaderCollection ConsigneesAndWarehouses
		{
			get
			{
				if (IsWarehouseClientSecurityRequired)
				{
					return new WarehouseClientCollectionWithSecurityCheck(Factory);
				}
				else
				{
					ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, true);
					query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignee, true);
					OrgHeaderCollection collection = new OrgHeaderCollection(Factory, query);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True)); //Consignees defaults as most common (warehouse can be set manually)
					return collection;
				}
			}
		}

		#endregion

		#region Customs Job Created

		ZQuery GetCustomsJobCreatedQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@DateFrom", dateFrom, JobDeclarationSchema.JE_SystemCreateTimeUtc);
			parameters.Add("@DateTo", dateTo, JobDeclarationSchema.JE_SystemCreateTimeUtc);

			string sql = OrgSupplierPartSchema.PK.Name + " in (";
			sql += "select " + JobComInvoiceLineSchema.JI_OP.Name + " from " + JobComInvoiceLineSchema.Constants.SqlSchemaName + "." + JobComInvoiceLineSchema.Constants.TableName + " ";
			sql += " join " + JobComInvoiceHeaderSchema.Constants.SqlSchemaName + "." + JobComInvoiceHeaderSchema.Constants.TableName + "  on " + JobComInvoiceLineSchema.JI_JZ.Name + " = " + JobComInvoiceHeaderSchema.PK.Name;
			sql += " join " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + "  on " + JobComInvoiceHeaderSchema.JZ_JE.Name + " = " + JobDeclarationSchema.PK.Name;
			sql += " where " + JobComInvoiceLineSchema.JI_OP.Name + " is not null ";
			sql += " and " + JobDeclarationSchema.JE_SystemCreateTimeUtc.Name;

			if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				sql += " is not null )";
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				sql += " is null )";
			}
			else
			{
				sql += " between @DateFrom and @DateTo )";
			}

			query.AddFilterAndZSQLParameterCollection(sql, parameters);
			return query;
		}

		#endregion

		#region Warehouse Job Created

		ZQuery GetWarehouseJobCreatedQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			string rawSql = string.Format(@"
{0} in
(
	select
		{0}
	from
		{1} 
		join {2}  on {3} = {0}
		join {4}  on {5} = {6}
	where
",
			OrgSupplierPartSchema.PK.Name,              // {0}
			OrgSupplierPartSchema.Constants.TableName,  // {1}
			WhsDocketLineSchema.Constants.TableName,    // {2}
			WhsDocketLineSchema.WE_OP.Name,             // {3}
			WhsDocketSchema.Constants.TableName,        // {4}
			WhsDocketSchema.PK.Name,                    // {5}
			WhsDocketLineSchema.WE_WD.Name);            // {6}

			var parameters = new ZSqlParameterCollection();

			if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				rawSql += string.Format("{0} is not null", WhsDocketSchema.WD_BookingDate.Name);
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				rawSql += string.Format("{0} is null", WhsDocketSchema.WD_BookingDate.Name);
			}
			else
			{
				if (dateFrom.IsValidSqlDateTime)
				{
					parameters.Add("@DateFrom", dateFrom, WhsDocketSchema.WD_BookingDate);
					rawSql += WhsDocketSchema.WD_BookingDate.Name + " >= @DateFrom";
					if (dateTo.IsValidSqlDateTime)
					{
						rawSql += " and ";
					}
				}
				if (dateTo.IsValidSqlDateTime)
				{
					parameters.Add("@DateTo", dateTo, WhsDocketSchema.WD_BookingDate);
					rawSql += WhsDocketSchema.WD_BookingDate.Name + " <= @DateTo";
				}
			}
			rawSql += ")";

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddFilterAndZSQLParameterCollection(rawSql, parameters);

			return query;
		}

		#endregion

		#region Local Part Numbers

		class LocalPartSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				ZDBOnlySubQuery subQueryOrganisation = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQueryOrganisation.AddToFilter(filter);
				result.AddSubQuery(subQueryOrganisation, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Commodity Filter

		protected virtual ZQuery GetOrdersByCommodityCodeQuery(ZString value)
		{
			ZDBOnlyQuery supplierPartQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			supplierPartQuery.AddToFilter(OrgSupplierPartSchema.OP_RH_NKCommodityCode, SQLComparisonOperator.Equal, value);
			return supplierPartQuery;
		}

		#endregion

		#region Product Category Filter

		#region GetProductCategoryQuery

		protected ZQuery GetProductCategoryQuery(ZGuid value)
		{
			var relationTypeSubQuery = new ZQuery();
			relationTypeSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Owner);
			relationTypeSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(relationTypeSubQuery, JoinCondition.And);

			var categoryPKs = new OrgPartProductCategoryHelper(Factory).ProductCategoryAndSubCategories(value);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OPC_Category, categoryPKs);

			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddSubQuery(relationSubQuery, JoinCondition.And);

			return productQuery;
		}

		protected OrgPartCategoryCollection GetProductCategories
		{
			get { return new OrgPartCategoryCollection(Factory); }
		}
		#endregion

		#endregion

		#region Bill of Materials Filters

		void AddBOMFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter parentPartFilter = filters.AddTextFilter("Parent Part", GetParentPart);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			parentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			parentPartFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			parentPartFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ParentPart", "Parent Part");
			parentPartFilter.MaxLength = OrgSupplierPartSchema.OP_PartNum.MaxLength;

			ModuleTextFilter componentPartFilter = filters.AddTextFilter("Component Part", GetComponentPart);
			componentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			componentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			componentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			componentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			componentPartFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			componentPartFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgSupplierPartFilter|ComponentPart", "Component Part");
			componentPartFilter.MaxLength = OrgSupplierPartSchema.OP_PartNum.MaxLength;
		}

		protected ZQuery GetParentPart(SQLComparisonOperator sQLOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			ZDBOnlyQuery parentPartsQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			parentPartsQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, sQLOperator, value.SubstringSafe(0, OrgSupplierPartSchema.OP_PartNum.MaxLength));

			OrgSupplierPartCollection parentParts = new OrgSupplierPartCollection(Factory, parentPartsQuery);
			parentParts.Load(parentPartsQuery);

			List<ZGuid> componentsToFilter = new List<ZGuid>();
			foreach (OrgSupplierPart part in parentParts)
			{
				componentsToFilter.Add(part.PK);
				AddComponentsToFilter(componentsToFilter, part);
			}
			result.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.PK, componentsToFilter);

			return parentParts.Count == 0 && !value.IsEmpty ? ZQuery.NoResultQuery : result;
		}

		void AddComponentsToFilter(List<ZGuid> componentsToFilter, OrgSupplierPart part)
		{
			foreach (OrgPartBOM bom in part.BillOfMaterials)
			{
				componentsToFilter.Add(bom.OE_OP_Component);
				if (bom.Component != null)
				{
					AddComponentsToFilter(componentsToFilter, bom.Component);
				}
			}
		}

		protected ZQuery GetComponentPart(SQLComparisonOperator sQLOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			if (sQLOperator == SpecialComparisonOperator.IsBlank)
			{
				var isBlankQuery = new ZDBOnlySubQuery(typeof(OrgPartBOM), OrgPartBOMSchema.OE_OP_MainProduct, true);
				result.AddSubQuery(isBlankQuery, JoinCondition.And);
			}
			else if (sQLOperator == SpecialComparisonOperator.IsNotBlank)
			{
				var isNotBlankQuery = new ZDBOnlySubQuery(typeof(OrgPartBOM), OrgPartBOMSchema.OE_OP_MainProduct);
				result.AddSubQuery(isNotBlankQuery, JoinCondition.And);
			}
			else
			{
				var componentPartQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				componentPartQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, sQLOperator, value.SubstringSafe(0, OrgSupplierPartSchema.OP_PartNum.MaxLength));

				var componentParts = new OrgSupplierPartCollection(Factory, componentPartQuery);
				componentParts.Load(componentPartQuery);

				if (componentParts.Count == 0 && !value.IsEmpty)
				{
					return ZQuery.NoResultQuery;
				}
				else
				{
					var componentsToFilter = new List<ZGuid>();
					foreach (OrgSupplierPart part in componentParts)
					{
						componentsToFilter.Add(part.PK);
						AddParentsToFilter(componentsToFilter, part);
					}
					result.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.PK, componentsToFilter);
				}
			}

			return result;
		}

		void AddParentsToFilter(List<ZGuid> componentsToFilter, OrgSupplierPart part)
		{
			OrgPartBOMCollection boms = new OrgPartBOMCollection(Factory);
			boms.AdditionalFilter = new ZQuery(OrgPartBOMSchema.OE_OP_Component, part.PK);

			foreach (OrgPartBOM bom in boms)
			{
				componentsToFilter.Add(bom.OE_OP_MainProduct);
				if (bom.MainProduct != null)
				{
					AddParentsToFilter(componentsToFilter, bom.MainProduct);
				}
			}
		}

		#endregion

		#region Dynamic Pick Face Area Filter

		protected ZQuery GetDynamicProductAreaQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var subQuery = new ZDBOnlySubQuery(typeof(IWhsProductParamsByWhsAndClient), WhsProductParamsByWhsAndClientSchema.W3_OP);
			subQuery.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected IWhsAreaCollection GetAreas
		{
			get { return ObjectFactory.Get<IWhsDynamicAreaCollection>(nameof(IWhsDynamicAreaCollection), Factory); }
		}

		#endregion

		#region Classification Organisation Filter

		protected ZQuery GetClassificationOrganizationsQuery(ZGuid value)
		{
			var result = new ZQuery();
			var orgPK = value;
			if (!orgPK.IsEmpty)
			{
				var factory = CreateNewFactory();
				var loader = new OrgRelatedParty.Loader(factory);
				var orgParents = loader.LoadPartiesWithRelatedOrgAndType(orgPK, RelatedPartyTypeList.Codes.ProductRelationship);
				var orgPKsToMatch = orgParents.Select(p => p.PR_OH_Parent).Prepend(orgPK).ToArray();
				result.AddToFilter(GetProductRelationshipFilter(orgPKsToMatch));
			}
			return result;
		}

		ZDBOnlyQuery GetProductRelationshipFilter(ZGuid[] orgPKs)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var subQueryOrganisation = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			subQueryOrganisation.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, orgPKs);
			subQueryOrganisation.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
			result.AddSubQuery(subQueryOrganisation, JoinCondition.And);
			return result;
		}

		#endregion

		#region Validation

		void ImporterFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("1de63825-ae30-4f78-afe4-92b75a29ed40", "Please select a Importer to filter by"));
			}
		}

		void SupplierFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("57253a1f-bbc4-42e0-a98a-06589e29f38d", "Please select a Supplier to filter by"));
			}
		}

		#endregion
	}
}
