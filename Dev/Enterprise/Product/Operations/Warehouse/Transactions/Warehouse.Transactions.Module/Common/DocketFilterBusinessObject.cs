using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Packing.Module;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class DocketFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddWarehouseFilter(filters);
			AddClientFilter(filters);
			AddCustomsEntryFilter(filters);
			AddPackageIdFilters(filters);
			AddHandlingUnitFilters(filters);
			AddPalletIdFilter(filters);
			AddExtraFilters(filters);
			AddProductFilter(filters);
			AddProductCategoryFilter(filters);
			AddServiceLevelFilter(filters);
			AddDocketStatusFilter(filters);
			AddPackageTypeFilters(filters);
			if (SupportsDocketPlanningStatus && WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
			{
				AddDocketTaskPlanningStatusFilter(filters);
			}
			if (SupportsDistributionCentreFilters)
			{
				AddDistributionCentreFilters(filters);
			}

			if (IncludeTransportCoFilter)
			{
				AddTransportCoFilter(filters);
			}

			filters.AddNkFilter("Commodity", GetCommodityCodeQuery, ModuleIDs.RefCommodityCode, CommodityCodes).MultilingualDescription = ResString.GetMultilingualString("01702750-e971-4d4e-ad11-e95ae0923f3e", "Commodity");
			return filters;
		}

		protected abstract bool IncludeTransportCoFilter { get; }

		protected virtual void AddTransportCoFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddGuidFilter("Transport Co", ModuleIDs.Organisation, (comparisonOperator, value) => GetDocAddressQuery(value is ZGuid ? (ZGuid)value : ZGuid.Empty, TransportCoConstants.AddressTypeCode, comparisonOperator), TransportCos);
			filter.MultilingualDescription = ResString.GetMultilingualString("010c64f5-4d1a-42d6-96bf-6ae7fff54f20", "Transport Co");
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();

			var attributeManager = new AttributeManager();
			var attributes = IncludeCustomAttribFilters
				? attributeManager.GetAllAttributes(AttributeManager.AttributeModules.Warehouse, Client, LoggedInWebUsersOrg)
				: attributeManager.GetPartAttributes(AttributeManager.AttributeModules.Warehouse, Client, LoggedInWebUsersOrg);

			ModuleFilters.AddAttributeFilters(attributes, GetAttributeFilter);
		}

		protected virtual bool IncludeCustomAttribFilters => true;

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Docket ID", WhsDocketSchema.WD_DocketID, "W") { MultilingualDescription = ResString.GetMultilingualString("3dd1c01c-8396-42aa-b021-17de67ab3d12", "Docket ID") };
		}

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, Warehouses);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("afeb2db2-ccb5-4e89-9cc5-5a287b450143", "Warehouse");

			if (!Env.Security.WhsAllowedWarehouses.IsAllowed && !Globals.IsWeb)
			{
				warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseFilter.PropertyValidation = WarehouseFilterValidation;
			}
			else if (Globals.IsWeb)
			{
				warehouseFilter.PropertyValidation = WarehouseWebFilterValidation;
			}

			AddWarehouseFilterCore(warehouseFilter);
		}

		#region AddWarehouseFilter

		protected virtual void AddWarehouseFilterCore(ModuleGuidFilter filter)
		{
			filter.PropertyInfo.ValueChanged += OnWarehouseChanged;
		}

		#endregion

		#region OnWarehouseChanged

		protected virtual void OnWarehouseChanged(object sender, EventArgs e)
		{
		}

		#endregion

		void WarehouseFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("e5b1027c-7404-4380-aa1d-d073b2bbf95f", "Please select a warehouse to filter by"));
			}
		}

		protected virtual void WarehouseWebFilterValidation(ZPropertyInfo info)
		{
		}

		#endregion

		#region Client Filter

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = new ModuleGuidFilterForOrg(ClientFilterCode, ModuleIDs.Organisation, GetClientQuery, FilterClients);
			clientFilter.SupportsBlankComparisonOperators = false;
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("da215578-cf4d-4199-b8e4-ea1f183fabcf", "Client");
			clientFilter.IsPublishedOnWeb = false;

			if (!Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
				clientFilter.PropertyValidation = ClientFilterValidation;
			}

			clientFilter.PropertyInfo.ValueChanged += new EventHandler(ClientChanged);
			clientFilter.ComparisonOperatorChanged += new EventHandler(ClientChanged);
			filters.AddFilter(clientFilter);
		}

		void ClientFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("5d6c9bf2-9c25-4e5e-a5b9-2633461dd204", "Please select a client to filter by"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant code text")]
		const string ClientFilterCode = "Client";

		#endregion

		#region CustomsEntry Filter

		void AddCustomsEntryFilter(ModuleFilterCollection filters)
		{
			var customsEntryFilter = filters.AddTextFilter("Customs Entry Key", GetCustomsEntryKeyQuery);
			customsEntryFilter.MultilingualDescription = ResString.GetMultilingualString("62c066f5-2d3f-44cb-acca-7996309d1d73", "Customs Entry Key");
			customsEntryFilter.MaxLength = WhsDocketLineSchema.WE_BondedEntryKey.MaxLength;
			customsEntryFilter.Category = FilterCategories.TextSearch;
			customsEntryFilter.IsPublishedOnWeb = false;
		}

		#endregion

		#region PackageID Filter

		protected virtual bool AddPackageIdFilter
		{
			get { return false; }
		}

		protected void AddPackageIdFilters(ModuleFilterCollection filters)
		{
			if (AddPackageIdFilter)
			{
				AddPackageIdFiltersCore(filters);
			}
		}

		void AddPackageIdFiltersCore(ModuleFilterCollection filters)
		{
			var helper = GetPackageFilterQueryHelper(PkgPackageHeaderSchema.KPH_PackageID);
			var filter = filters.AddTextFilter("Package ID", helper.QueryDelegate);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Docket|PackageID", "Package ID");
			filter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;      // Tested in OrderFilterBusinessObject.
		}

		#endregion

		#region HandlingUnit Filter

		protected virtual bool AddHandlingUnitFilter => false;

		protected void AddHandlingUnitFilters(ModuleFilterCollection filters)
		{
			if (AddHandlingUnitFilter)
			{
				AddHandlingUnitFiltersCore(filters);
			}
		}

		void AddHandlingUnitFiltersCore(ModuleFilterCollection filters)
		{
			var helper = GetPackageFilterQueryHelper(PkgPackageHeaderSchema.KPH_PackageID);
			var filter = filters.AddTextFilter("Handling Unit", helper.QueryDelegateWithHandlingUnitID);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Docket|HandlingUnit", "Handling Unit");
			filter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;
		}

		#endregion

		#region Package Type Filter

		protected void AddPackageTypeFilters(ModuleFilterCollection filters)
		{
			if (AddPackageTypeFilter)
			{
				AddPackageTypeFiltersCore(filters);
			}
		}

		void AddPackageTypeFiltersCore(ModuleFilterCollection filters)
		{
			var helper = GetPackageFilterQueryHelper(PkgPackageSchema.KP_F3_NKPackType);
			var filter = filters.AddTextFilter("Package Type", helper.QueryDelegateWithoutOperator, PackageTypes);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Docket|PackageType", "Package Type");
			filter.MaxLength = PkgPackageSchema.KP_F3_NKPackType.MaxLength;
		}

		#region PackageTypes

		public RefPackTypeCollection PackageTypes => new RefPackTypeCollection(Factory);

		#endregion

		#region AddPackageTypeFilter

		protected virtual bool AddPackageTypeFilter => false;

		#endregion

		#endregion

		#region GetPackageFilterQueryHelper

		protected virtual PackageFilterQueryHelper GetPackageFilterQueryHelper(SchemaColumn packageColumnToFilterOn) => new PackageFilterQueryHelper(TypeOfBusinessObjectToQuery, packageColumnToFilterOn);

		#endregion

		#region PalletID Filter

		protected virtual void AddPalletIdFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Pallet ID", GetPalletIDQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = WhsDocketLineSchema.WE_PalletID.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocketFilterBusinessObject|PalletID", "Pallet ID");
		}

		#endregion

		#region DocketTypeFilter

		protected virtual Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(WhsDocket); }
		}

		protected virtual SchemaColumn DocketForeignKey
		{
			get { return null; }
		}

		#endregion

		#region Attribute Filter

		public OrgHeader LoggedInWebUsersOrg
		{
			get;
			set;
		}

		ZQuery GetAttributeFilter(ZQuery filter, SchemaColumn column)
		{
			if (column.TableSchema == WhsDocketSchema.Instance)
			{
				return GetFilteredDocketQuery(filter);
			}

			if (column.TableSchema == WhsDocketLineSchema.Instance)
			{
				var result = (ZDBOnlyQuery)GetFilteredDocketQuery(GetDocketLineAttributeQuery(filter, column));
				ExtendDocketQuery(result, filter);
				return result;
			}

			throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "{0} schema is not supported", column.TableSchema));
		}

		protected virtual void ExtendDocketQuery(ZDBOnlyQuery query, ZQuery filter)
		{
		}

		#endregion

		#region WorkFlow Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			if (shouldAddCustomWorkflowFilterStripsHelper)
			{
				var helper = new WorkflowFilterStripsHelper(docketBusinessObjectType, workflowCode, Factory);
				helper.SetShouldAddWorkflowCustomFieldsFilters(true);
				helpers.Add(helper);
			}

			return helpers;
		}

		protected void AddWorkflowFilterStripsHelper(Type docketType, string workflowDescriptorCode)
		{
			shouldAddCustomWorkflowFilterStripsHelper = true;
			docketBusinessObjectType = docketType;
			workflowCode = workflowDescriptorCode;
		}

		bool shouldAddCustomWorkflowFilterStripsHelper;
		Type docketBusinessObjectType;
		string workflowCode;

		#endregion

		#region Reference Filters

		protected void AddReferenceFilters(ModuleFilterCollection filters)
		{
			var allReferenceFields = new List<FilterGroupMember>();
			allReferenceFields.AddRange(ReferenceFields);
			allReferenceFields.AddRange(AdditionalReferenceFields);
			AddFilterGroupMembers(filters, allReferenceFields, FilterCategories.NumbersAndReferences);

			var anyReferenceFields = new List<FilterGroupMember>();
			anyReferenceFields.AddRange(ReferenceFields);
			anyReferenceFields.Add(AnyAdditionalReferenceField);
			AddAnyReferenceFilter(filters, anyReferenceFields, ResString.GetMultilingualString("9581cc00-1aba-40f4-9fe8-7f745c69dbe3", "Any Reference"), FilterCategories.NumbersAndReferences);
		}

		protected virtual IList<FilterGroupMember> ReferenceFields
		{
			get
			{
				var result = new List<FilterGroupMember>();

				if (AddContainerNoFilter)
				{
					result.Add(new FilterGroupMember(ResString.GetMultilingualString("affd7d3b-525d-4c70-9df8-351671f276e7", "Container No."), WhsDocketContainerSchema.WC_ContainerNum));
				}
				if (AddCustomerReferenceFilter)
				{
					result.Add(new FilterGroupMember(ResString.GetMultilingualString("9077c6ad-49c6-462f-90b3-6ad8e5571ac8", "Customer Reference"), WhsDocketSchema.WD_CustomerReference));
				}
				if (AddTransportReferenceFilter)
				{
					result.Add(new FilterGroupMember(ResString.GetMultilingualString("6f85a038-45d1-4101-a2b9-dc14d0709e14", "Transport Reference"), WhsDocketSchema.WD_TransportReference));
				}

				return result;
			}
		}

		IList<FilterGroupMember> AdditionalReferenceFields
		{
			get
			{
				var result = new List<FilterGroupMember>();

				foreach (CodeDescriptionBool pair in WarehouseDataRegistry.Instance.AdditionalReferenceType.Value)
				{
					var description = ResString.GetMultilingualString("b7482675-2c01-4b11-a2b6-489d07914cf8", "Ref. {0} ({1})", pair.Description, (NoResString)pair.Code);
					result.Add(new FilterGroupMember(description, WhsDocketReferenceSchema.WX_Reference, new ZQuery(WhsDocketReferenceSchema.WX_RefType, pair.Code)));
				}

				return result;
			}
		}

		FilterGroupMember AnyAdditionalReferenceField
		{
			get { return new FilterGroupMember(ResString.GetMultilingualString("5eca1227-6d82-4e60-Be9b-2e9a16aa6e84", "Any Additional Reference"), WhsDocketReferenceSchema.WX_Reference); }
		}

		protected virtual bool AddContainerNoFilter { get { return true; } }
		protected virtual bool AddCustomerReferenceFilter { get { return true; } }
		protected virtual bool AddTransportReferenceFilter { get { return true; } }

		#endregion

		#region Extra Filters

		protected virtual void AddExtraFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Finalized Date", WhsDocketSchema.WD_FinalisedDate).MultilingualDescription = ResString.GetMultilingualString("799e29ee-acb4-4598-a46e-a0b872cf4d3e", "Finalized Date");
		}

		#endregion

		#region AddProductFilter

		void AddProductFilter(ModuleFilterCollection filters)
		{
			var productFilter = filters.AddGuidFilter("Product", ModuleIDs.WhsConfigProduct, GetProductQuery, GetProducts);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("00096846-e8dc-4b95-aa0b-fc4d09093e76", "Product");
			productFilter.PropertyValidation = ProductFilterValidation;
		}

		void ProductFilterValidation(ZPropertyInfo info)
		{
			if (ShouldProductFilterEnsureClientFilterIsEnteredFirst && !ProductFilter.Property.IsEmpty)
			{
				var activeClientFilters = GetActiveClientFiltersWithValidValue();
				if (activeClientFilters.Count() > 1)
				{
					info.AddError(Res.GetString("ca363f35-fd2e-4e29-82a3-59d5aad2314b", "Product Code can not be entered if there are multiple Client filters."));
				}
				else
				{
					var clientFilter = activeClientFilters.FirstOrDefault();
					if (clientFilter == null)
					{
						info.AddError(Res.GetString("df424e24-5a28-4a5b-a8c6-887911c31f75", "Product Code can not be entered without a Client Code."));
					}
					else if (clientFilter.SqlComparisonOperator == SQLComparisonOperator.NotEqual)
					{
						info.AddError(Res.GetString("2f886536-1825-4ecd-90d3-a22b6f79dc1e",
							$"Product Code can not be entered with a Client filter with a '{ModuleTextBaseFilter.ComparisonConstants.NotEqual}' comparison operator."));
					}
				}
			}
		}

		protected virtual bool ShouldProductFilterEnsureClientFilterIsEnteredFirst => true;

		IEnumerable<ModuleGuidFilter> GetActiveClientFiltersWithValidValue()
		{
			return ActiveModuleFilters
				.Where(filter => filter is ModuleGuidFilter guidFilter && IsClientFilter(guidFilter))
				.Cast<ModuleGuidFilter>()
				.Where(filter => !filter.Property.IsEmpty && filter.Property.IsValid);
		}

		bool IsClientFilter(ModuleGuidFilter filter)
		{
			return filter.ModuleId == ModuleIDs.Organisation && filter.Code.Substring(0, 6).Equals(ClientFilterCode);
		}

		#endregion

		#region AddProductCategoryFilter

		void AddProductCategoryFilter(ModuleFilterCollection filters)
		{
			var productCategoryFilter = filters.AddGuidFilter("Product Category", ModuleIDs.RefOrgPartCategory, GetProductCategoryQuery, GetProductCategories);
			productCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("E95F1F27-DBD4-4DA9-BF5E-13D419F234C0", "Product Category");
			productCategoryFilter.IsPublishedOnWeb = false;
		}

		#endregion

		#region Service Level Filter

		void AddServiceLevelFilter(ModuleFilterCollection filters)
		{
			if (IsAddServiceLevelFilter)
			{
				var serviceLevelFilter = filters.AddNkFilter("Service Level", GetServiceLevelQuery, ModuleIDs.ServiceLevel, new RefServiceLevelCollection(Factory));
				serviceLevelFilter.ErrorOnCodeNotPresent = false;
				serviceLevelFilter.Category = FilterCategories.Other;
				serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("ExtendedDocketFilterBusinessObject|ServiceLevel", "Service Level");
			}
		}

		protected virtual bool IsAddServiceLevelFilter
		{
			get { return false; }
		}

		protected virtual ZQuery GetServiceLevelQuery(ZString serviceLevel)
		{
			return new ZQuery(WhsDocketSchema.WD_RS_NKServiceLevel, serviceLevel);
		}

		#endregion

		#region Docket Status Filter

		void AddDocketStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DocketStatusFilterDescription, GetStatusQuery, DocketStatuses);
			filter.MultilingualDescription = DocketStatusFilterMultilingualDescription;
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#region DocketStatusFilterDescription

		public string DocketStatusFilterDescription
		{
			get { return DocketStatusFilterDescriptionCore(); }
		}

		protected virtual string DocketStatusFilterDescriptionCore()
		{
			return (NoResString)"Status";    // Filter Description Text
		}

		public MultilingualString DocketStatusFilterMultilingualDescription
		{
			get { return DocketStatusFilterMultilingualDescriptionCore(); }
		}

		protected virtual MultilingualString DocketStatusFilterMultilingualDescriptionCore()
		{
			return ResString.GetMultilingualString("e58558a5-c0a6-4e7b-b072-61b0c220a3aa", "Status");
		}

		#endregion

		#region GetStatusQuery

		public ZQuery GetStatusQuery(ZString value)
		{
			return GetStatusQueryCore(value);
		}

		protected virtual ZQuery GetStatusQueryCore(ZString value)
		{
			return new ZQuery(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.Equal, value);
		}

		#endregion

		#region DocketStatuses

		public CodeDescriptionPairList DocketStatuses
		{
			get
			{
				var status = GetDocketStatusCore();
				return status;
			}
		}

		protected virtual CodeDescriptionPairList GetDocketStatusCore()
		{
			return new DocketStatus();
		}

		#endregion

		#endregion

		#region DocketTaskPlanningStatusFilter

		void AddDocketTaskPlanningStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Task Planning Status", GetTaskPlanningStatusQuery, PlanningStatuses);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("f1ba9a12-a0b6-4dcd-8d32-d4aee1dbc077", "Task Planning Status");
		}

		#region GetTaskPlanningStatusQuery

		ZQuery GetTaskPlanningStatusQuery(ZString value)
		{
			return new ZQuery(WhsDocketSchema.WD_TaskPlanningStatus, SQLComparisonOperator.Equal, value);
		}

		#endregion

		#region SupportsDocketPlanningStatus

		protected virtual bool SupportsDocketPlanningStatus => false;

		#endregion

		#region DistributionCentreFilters

		void AddDistributionCentreFilters(ModuleFilterCollection filters)
		{
			var distributionCentreFilter = filters.AddGuidFilter("Distribution Center", ModuleIDs.Organisation, (comparisonOperator, value) => GetDocAddressQuery(value is ZGuid guid ? guid : ZGuid.Empty, DocAddressTypes.Codes.DistributionCentreAddress, comparisonOperator), DistributionCentres);
			distributionCentreFilter.MultilingualDescription = ResString.GetMultilingualString("55112f77-9bcc-4559-ac4c-0ffcc41751d3", "Distribution Center");

			var distributionCentreNameFilter = filters.AddTextFilter("Distribution Center Name", GetDistributionCentreNameQuery);
			distributionCentreNameFilter.MultilingualDescription = ResString.GetMultilingualString("17d29d4c-8bde-4c40-959b-5857eae834e8", "Distribution Center Name");
			distributionCentreNameFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			distributionCentreNameFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region SupportsDistributionCentreFilters

		protected virtual bool SupportsDistributionCentreFilters => false;

		#endregion

		#endregion

		#region Implementation

		protected struct FilterGroupMember
		{
			public FilterGroupMember(MultilingualString description, SchemaColumn column)
				: this(description, column, null)
			{
			}

			public FilterGroupMember(SchemaColumn column)
				: this((NoResString)ZString.Empty, column, null)
			{
			}

			public FilterGroupMember(MultilingualString description, SchemaColumn column, ZQuery additionalQuery)
			{
				Description = description;

				Column = column;
				AdditionalQuery = additionalQuery;
				Visible = !Description.IsEmpty;
			}

			public bool Visible;
			public readonly MultilingualString Description;
			public readonly SchemaColumn Column;
			public readonly ZQuery AdditionalQuery;
		}

		void AddFilterGroupMembers(ModuleFilterCollection filters, IList<FilterGroupMember> filterGroupMembers, FilterCategory category)
		{
			ModuleFilter filter;
			foreach (var filterGroupMember in filterGroupMembers)
			{
				if (filterGroupMember.Visible)
				{
					filter = filters.AddTextFilter(filterGroupMember.Description.GetUnresolvedString(), GetDelegate(filterGroupMember));
					filter.MultilingualDescription = filterGroupMember.Description;
					filter.Category = category;
					filter.MaxLength = filterGroupMember.Column.MaxLength;
				}
			}
		}

		void AddAnyReferenceFilter(ModuleFilterCollection filters, IList<FilterGroupMember> filterGroupMembers, MultilingualString theAnyDescription, FilterCategory category)
		{
			var membersArray = new FilterGroupMember[filterGroupMembers.Count];
			filterGroupMembers.CopyTo(membersArray, 0);

			var filter = filters.AddTextFilter(theAnyDescription.GetUnresolvedString(), GetDelegate(membersArray));
			filter.MultilingualDescription = theAnyDescription;
			filter.Category = category;
			filter.MaxLength = filterGroupMembers.Max(f => f.Column.MaxLength);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected virtual GetTextQueryWithOperator GetDelegate(params FilterGroupMember[] filterGroupMembers)
		{
			return delegate(SQLComparisonOperator comparisonOperator, ZString value)
				{
					ZDBOnlyQuery subQuery;
					ZDBOnlyQuery query = GetDocketQuery(out subQuery);

					foreach (FilterGroupMember filterGroupMember in filterGroupMembers)
					{
						if (filterGroupMember.Column.TableName == WhsDocketSchema.Constants.TableName && filterGroupMember.Column.MaxLength >= value.Length)
						{
							subQuery.AddToFilter(JoinCondition.Or, filterGroupMember.Column, comparisonOperator, value);
						}
					}

					AddSubQuery(subQuery, typeof(WhsDocketLine), WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.WE_WD,
						filterGroupMembers, comparisonOperator, value);
					AddSubQuery(subQuery, typeof(WhsDocketContainer), WhsDocketContainerSchema.Constants.TableName, WhsDocketContainerSchema.WC_WD,
						filterGroupMembers, comparisonOperator, value);
					AddSubQuery(subQuery, typeof(WhsDocketReference), WhsDocketReferenceSchema.Constants.TableName, WhsDocketReferenceSchema.WX_WD,
						filterGroupMembers, comparisonOperator, value);

					if (query != subQuery)
					{
						query.AddSubQuery((ZDBOnlySubQuery)subQuery, JoinCondition.And);
					}

					return query;
				};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		void AddSubQuery(ZDBOnlyQuery query, Type typeOfBusinessObjectToSubQuery, string tableName, SchemaColumn foreignKey, FilterGroupMember[] filterGroupMembers,
			SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery filter = new ZQuery();

			foreach (FilterGroupMember filterGroupMember in filterGroupMembers)
			{
				if (filterGroupMember.Column.TableName == tableName && filterGroupMember.Column.MaxLength >= value.Length)
				{
					ZQuery filter2 = new ZQuery(filterGroupMember.Column, comparisonOperator, value);
					if (filterGroupMember.AdditionalQuery != null)
					{
						filter2.AddToFilter(filterGroupMember.AdditionalQuery);
					}
					filter.AddToFilter(filter2, JoinCondition.Or);
				}
			}

			if (!filter.IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeOfBusinessObjectToSubQuery, foreignKey);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.Or);
			}
		}

		#endregion

		#endregion

		#region Properties

		#region Warehouse

		public ZGuid WD_WW_Whs
		{
			get { return WarehouseFilter != null && WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty; }
		}

		protected ModuleGuidFilter WarehouseFilter
		{
			get { return (ModuleGuidFilter)ModuleFilters["Warehouse"]; }
		}

		#endregion

		#region Client

		public ZGuid WD_OH_Client => ClientFilter.IsActive ? ClientFilter.Property : ZGuid.Empty;

		ModuleGuidFilter ClientFilter
		{
			get
			{
				var clientFilter = (ModuleGuidFilter)ModuleFilters["Client"];
				if (!clientFilter.IsActive || clientFilter.Property.IsEmpty)
				{
					var activeClientFilters = GetActiveClientFiltersWithValidValue();
					if (activeClientFilters.Any())
					{
						clientFilter = activeClientFilters.First();
					}
				}

				return clientFilter;
			}
		}

		public OrgHeader Client => Factory.Load<OrgHeader>(WD_OH_Client);

		#region ClientChanged

		void ClientChanged(object sender, EventArgs e)
		{
			RevalidateProductFilter();
		}

		void RevalidateProductFilter()
		{
			ProductFilterReValidator.RevalidateProductFilter(ClientFilter, ProductFilter, Factory);
		}

		#endregion

		ModuleGuidFilter ProductFilter => (ModuleGuidFilter)ModuleFilters["Product"];

		#endregion

		#region TransportCo

		public ZGuid TransportCoPK
		{
			get { return TransportCoFilter != null && TransportCoFilter.IsActive ? TransportCoFilter.Property : ZGuid.Empty; }
		}

		protected virtual ModuleGuidFilter TransportCoFilter
		{
			get { return null; }
		}

		#endregion

		#region Consignee

		public ZGuid ConsigneePK
		{
			get { return ConsigneeFilter != null && ConsigneeFilter.IsActive ? ConsigneeFilter.Property : ZGuid.Empty; }
		}

		protected virtual ModuleGuidFilter ConsigneeFilter
		{
			get { return null; }
		}

		#endregion

		#endregion

		#region Lookups

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region Clients

		public WarehouseClientCollection Clients
		{
			get { return new WarehouseClientCollectionWithSecurityCheck(Factory); }
		}

		public OrganisationsFindBoxCollection FilterClients
		{
			get
			{
				return new WarehouseClientCollectionWithSecurityCheck(Factory)
				{
					ShouldApplyActiveFilter = false
				};
			}
		}

		#endregion

		#region TransportCos

		public ShippingProviderCollection TransportCos
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		#endregion

		#region Consignees

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ConsigneeCollection FilterConsignees
		{
			get
			{
				return new ConsigneeCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				};
			}
		}

		#endregion

		#region DistributionCentres

		public DistributionCentreCollection DistributionCentres =>  new DistributionCentreCollection(Factory);

		#endregion

		#region GetProducts

		protected virtual WhsOrgSupplierPartCollection GetProducts()
		{
			if (products == null || productsClient != Client)
			{
				productsClient = Client;
				products = new WhsOrgSupplierPartCollection(Factory, null, productsClient, false);
			}

			return products;
		}

		WhsOrgSupplierPartCollection products;
		OrgHeader productsClient;

		#endregion

		#region GetProductCategories

		protected virtual OrgPartCategoryCollection GetProductCategories
		{
			get { return new OrgPartCategoryCollection(Factory); }
		}

		#endregion

		#region CommodityCodes

		public RefCommodityCodeCollection CommodityCodes
		{
			get { return new RefCommodityCodeCollection(Factory); }
		}

		#endregion

		#region GetCarrierServiceLevels

		public OrgCarrierServiceLevelCollection GetCarrierServiceLevels()
		{
			OrgCarrierServiceLevelCollection result;

			var transportCo = Factory.Load<OrgHeader>(TransportCoPK);
			if (transportCo == null)
			{
				result = new OrgCarrierServiceLevelCollection(Factory);
			}
			else
			{
				result = new OrgCarrierServiceLevelCollection(transportCo.MiscServ);
				result.Load();
			}

			return result;
		}

		#endregion

		#region ServiceLevels

		public RefServiceLevelCollection ServiceLevels
		{
			get { return new ActiveServiceLevelCollection(Factory); }
		}

		#endregion

		#region PickOptions

		public WhsPickOption PickOptions
		{
			get { return new WhsPickOption(); }
		}

		#endregion

		#region PlanningStatuses

		public TaskPlanningStatus PlanningStatuses => new TaskPlanningStatus();

		#endregion

		#endregion

		#region Queries

		#region GetConsigneeCompanyNameQuery

		protected virtual ZQuery GetConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetAddressQuery(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.ConsigneeAddress);
		}

		#endregion

		#region GetTransportCompanyNameQuery

		protected virtual ZQuery GetTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetAddressQuery(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, TransportCoConstants.AddressType);
		}

		#endregion

		#region GetDistributionCentreNameQuery

		protected virtual ZQuery GetDistributionCentreNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetAddressQuery(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.DistributionCentreAddress);
		}

		#endregion

		#region GetSupplierCompanyNameQuery

		protected ZQuery GetSupplierCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetAddressQuery(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.SupplierDocumentaryAddress);
		}

		#endregion

		#region GetAddressQuery

		protected ZQuery GetAddressQuery(SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue, DocAddressType addressType)
		{
			var addressTypeCode = DocAddressTypes.GetCode(Factory, addressType);
			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(addressTypeCode, comparisonOperator, orgColumn, docAddColumnm, paramValue);

			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));
			docketResult.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(docketResult);
			return result;
		}

		#endregion

		#region GetDocketQuery

		ZDBOnlyQuery GetDocketQuery(out ZDBOnlyQuery subQuery)
		{
			var result = new ZDBOnlyQuery(TypeOfBusinessObjectToQuery);

			subQuery = typeof(WhsDocket).IsAssignableFrom(TypeOfBusinessObjectToQuery)
						? result
						: new ZDBOnlySubQuery(typeof(WhsDocket), DocketForeignKey);

			return result;
		}

		#endregion

		#region GetDocketLineQuery

		ZDBOnlySubQuery GetDocketLineQuery(ZQuery filter)
		{
			var lineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			lineSubQuery.AddToFilter(filter);
			return lineSubQuery;
		}

		#endregion

		#region GetDocketLineAttributeQuery

		ZQuery GetDocketLineAttributeQuery(ZQuery filter, SchemaColumn column)
		{
			ZQuery result;

			if (filter.Params.Length > 0)
			{
				var lineSubQuery = GetDocketLineQuery(filter);
				var sqlParameter = filter.Params[0];
				AddToDocketLineAttributeQuery(lineSubQuery, column, sqlParameter.ComparisonOperator, sqlParameter.Value);
				result = lineSubQuery;
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		protected virtual void AddToDocketLineAttributeQuery(ZDBOnlySubQuery lineSubQuery, SchemaColumn column, SQLComparisonOperator comparisonOperator, object value)
		{
		}

		#endregion

		#region GetFilteredDocketQuery

		protected ZQuery GetFilteredDocketQuery(ZQuery filterOrSubQuery)
		{
			var query = GetDocketQuery(out ZDBOnlyQuery subQuery);

			if (filterOrSubQuery is ZDBOnlySubQuery filterSubQ)
			{
				subQuery.AddSubQuery(filterSubQ, JoinCondition.And);
			}
			else
			{
				subQuery.AddToFilter(filterOrSubQuery);
			}

			if (query != subQuery)
			{
				query.AddSubQuery((ZDBOnlySubQuery)subQuery, JoinCondition.And);
			}
			return query;
		}

		#endregion

		#region GetSubTypeQuery

		protected virtual ZQuery GetSubTypeQuery(ZString value)
		{
			return new ZQuery(WhsDocketSchema.WD_DocketSubType, value);
		}

		#endregion

		#region GetWarehouseQuery

		protected virtual ZQuery GetWarehouseQuery(ZGuid value)
		{
			return GetFilteredDocketQuery(new ZQuery(WhsDocketSchema.WD_WW_Whs, value));
		}

		#endregion

		#region GetClientQuery

		ZQuery GetClientQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			return GetFilteredDocketQuery(new ZQuery(WhsDocketSchema.WD_OH_Client, comparisonOperator, value));
		}

		#endregion

		#region GetProductQuery

		protected virtual ZQuery GetProductQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
			ProductQueryHelper.AddProductToQuery(Factory, query, WhsDocketLineSchema.WE_OP, value);

			return GetFilteredDocketQuery(GetDocketLineQuery(query));
		}

		#endregion

		#region GetProductCategoryQuery

		protected virtual ZQuery GetProductCategoryQuery(ZGuid value)
		{
			var relationTypeSubQuery = new ZQuery();
			relationTypeSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Owner);
			relationTypeSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(relationTypeSubQuery, JoinCondition.And);

			var categoryPKs = new OrgPartProductCategoryHelper(Factory).ProductCategoryAndSubCategories(value);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OPC_Category, categoryPKs);

			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
			productSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);

			var docketLineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
			docketLineQuery.AddSubQuery(productSubQuery, JoinCondition.And);

			return GetFilteredDocketQuery(GetDocketLineQuery(docketLineQuery));
		}

		#endregion

		#region GetPalletIDQuery

		protected virtual ZQuery GetPalletIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);

			var palletSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, notIn);
			palletSubQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, comparisonOperator, value);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(palletSubQuery, JoinCondition.And);
			return docketQuery;
		}

		#endregion

		#region GetPickLineQueryForPalletID

		// tested where used in PickingFilterBusinessObject and PickableFilterBusinessObject/OrderFilterBusinessObject
		protected ZDBOnlySubQuery GetPickLineQueryForPalletID(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var inventoryLineQuery = new ZQuery(WhsDocketLineSchema.WE_PalletID, comparisonOperator, value);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			pickLineSubQuery.AddToFilter(GetPickedFromSourceLocationQuery(inventoryLineQuery));

			return pickLineSubQuery;
		}

		protected static ZDBOnlyQuery GetPickedFromSourceLocationQuery(ZQuery inventoryLineQueryOrSubQuery)
		{
			// ZQuery does not support coalesce, have to do it this way or write SQL
			var unpickedInventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			var pickedUsingInTransitTransferInventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine);

			if (inventoryLineQueryOrSubQuery is ZDBOnlySubQuery subQuery)
			{
				unpickedInventoryLineQuery.AddSubQuery(subQuery, JoinCondition.And);
				pickedUsingInTransitTransferInventoryLineQuery.AddSubQuery(subQuery, JoinCondition.And);
			}
			else
			{
				unpickedInventoryLineQuery.AddToFilter(inventoryLineQueryOrSubQuery);
				pickedUsingInTransitTransferInventoryLineQuery.AddToFilter(inventoryLineQueryOrSubQuery);
			}

			var unpickedPickLineQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			unpickedPickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, null);
			unpickedPickLineQuery.AddSubQuery(unpickedInventoryLineQuery, JoinCondition.And);

			var pickedUsingInTransitTransferQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickedUsingInTransitTransferQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, SQLComparisonOperator.NotEqual, null);
			pickedUsingInTransitTransferQuery.AddSubQuery(pickedUsingInTransitTransferInventoryLineQuery, JoinCondition.And);

			var inventoryLineQueryFromPickLine = new ZDBOnlyQuery(typeof(WhsPickLine));
			inventoryLineQueryFromPickLine.AddToFilter(unpickedPickLineQuery);
			inventoryLineQueryFromPickLine.AddToFilter(pickedUsingInTransitTransferQuery, JoinCondition.Or);

			return inventoryLineQueryFromPickLine;
		}

		#endregion

		#region GetCustomsEntryKeyQuery

		protected ZQuery GetCustomsEntryKeyQuery(SQLComparisonOperator compOperator, ZString value)
		{
			ZQuery filter = new ZQuery(WhsDocketLineSchema.WE_BondedEntryKey, compOperator, value);
			return GetFilteredDocketQuery(GetDocketLineQuery(filter));
		}

		#endregion

		#region GetCommodityCodeQuery

		protected virtual ZQuery GetCommodityCodeQuery(ZString value)
		{
			ZQuery docketLineFilter = new ZQuery();
			docketLineFilter.FilterByForeignKey(WhsDocketLineSchema.WE_OP, Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_RH_NKCommodityCode, value)));

			return GetFilteredDocketQuery(GetDocketLineQuery(docketLineFilter));
		}

		#endregion

		#region GetNotInvoicedQuery

		protected ZQuery GetNotInvoicedQuery(ZBool value)
		{
			var rawSQL = ZString.Format("WD_PK {0} in (select WD_PK from dbo.WhsDocketWithOnlyPostedCharges)", value ? (NoResString)"not" : ""); // It is sql statement.
			var parameters = new ZSqlParameterCollection();
			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddFilterAndZSQLParameterCollection(rawSQL, parameters);

			return GetFilteredDocketQuery(docketQuery);
		}

		#endregion

		#region GetDocAddressQuery

		protected ZQuery GetDocAddressQuery(ZGuid value, ZString addressTypeCode)
			=> GetDocAddressQuery(value, addressTypeCode, SQLComparisonOperator.Equal);

		protected ZQuery GetDocAddressQuery(ZGuid value, ZString addressTypeCode, SQLComparisonOperator comparisonOperator)
		{
			if (value.IsValid
				|| comparisonOperator == SpecialComparisonOperator.IsBlank
				|| comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				return GetFilteredDocketQuery(BuildDocAddressQuery(addressTypeCode, value, comparisonOperator));
			}

			ZDBOnlyQuery subQuery;
			return GetDocketQuery(out subQuery);
		}

		#endregion

		#region BuildDocAddressQuery

		ZDBOnlySubQuery BuildDocAddressQuery(ZString docAddressTypeCode, ZGuid orgPK, SQLComparisonOperator comparisonOperator)
		{
			return GetJobDocAddressOrgHeaderParentSubQuery(docAddressTypeCode, orgPK, comparisonOperator);
		}

		protected virtual ZDBOnlySubQuery GetJobDocAddressOrgHeaderParentSubQuery(ZString docAddressTypeCode, ZGuid orgPK, SQLComparisonOperator comparisonOperator)
		{
			return JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(docAddressTypeCode, orgPK, comparisonOperator);
		}

		#endregion

		#region GetTrolleyNumberQuery

		// tested in OrderFilterBusinessObject and PickingFilterBusinessObject
		protected ZQuery GetTrolleyNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var trolleyViewSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderTrolleyView), WhsOrderTrolleyViewSchema.WTR_WD_PK, notIn: isBlank);
			if (!isBlank)
			{
				trolleyViewSubQuery.AddToFilter(WhsOrderTrolleyViewSchema.WTR_RQ_Registration, comparisonOperator, value);
			}
			var docketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			docketQuery.AddSubQuery(trolleyViewSubQuery, JoinCondition.And);

			return GetFilteredDocketQuery(docketQuery);
		}

		#endregion

		#endregion

		#region Filter Subgroups

		protected class DocketLineSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(WhsDocket));
				var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				docketLineSubQuery.AddToFilter(filter);
				result.AddSubQuery(docketLineSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion
	}
}
