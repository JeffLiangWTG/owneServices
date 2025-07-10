using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CommercialInvoiceFilterBusinessObject : FilterStripBusinessObject
	{
		public CommercialInvoiceFilterBusinessObject()
		{
		}

		public CommercialInvoiceFilterBusinessObject(bool filterForNonCurrentCompany = false)
		{
			this.filterForNonCurrentCompany = filterForNonCurrentCompany;
		}
		readonly bool filterForNonCurrentCompany;

		public CommercialInvoiceFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		#region Implementation
		CommercialInvoiceFilterLookups lookups;

		protected virtual CommercialInvoiceFilterLookups GetNewLookups()
		{
			return new CommercialInvoiceFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			var invoiceNumberFilter = result.AddNumberFilter(CommercialInvoiceFilterConstants.InvoiceNumber, GetCommercialInvoiceNumberQuery);
			invoiceNumberFilter.MaxLength = JobComInvoiceHeaderSchema.JZ_InvoiceNumber.MaxLength;
			invoiceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|InvoiceNumber", CommercialInvoiceFilterConstants.InvoiceNumber);
			result.AddDateFilter(CommercialInvoiceFilterConstants.InvoiceDate, JobComInvoiceHeaderSchema.JZ_InvoiceDate).
				MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|InvoiceDate", CommercialInvoiceFilterConstants.InvoiceDate);
			result.AddNumberRangeFilter(CommercialInvoiceFilterConstants.InvoiceTotal, JobComInvoiceHeaderSchema.JZ_InvoiceAmount).
				MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|InvoiceTotal", CommercialInvoiceFilterConstants.InvoiceTotal);
			ModuleFilter shipTypeFilter = result.AddTextFilter(CommercialInvoiceFilterConstants.ShipmentType, GetShipmentTypeFilter, Lookups.MessageTypes);
			shipTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipTypeFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|ShipmentType", CommercialInvoiceFilterConstants.ShipmentType);
			AddOrganisationFilters(result);
			AddLineFilters(result);
			AddRoutingFilters(result);
			AddReferenceFilter(result);

			var companyFilter = result.AddGuidFilter(CommercialInvoiceFilterConstants.Company, ModuleIDs.GlbCompany, GetCompanyFilter, new GlbCompanyCollection(Factory));
			companyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			AddAttachedToDeclarationFilter(result);

			result.AddAttributeFilters(new AttributeManager().GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, LoggedInWebUsersOrg), FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("7345A789-D81E-419C-9CDC-3B3C65546CCE", "Commercial Invoice Attribute Search")), new InvoiceHeaderToInvoiceLineFilterSubGroup());

			return result;
		}

		ZQuery GetCompanyFilter(ZGuid companyPK)
		{
			if (!companyPK.IsValid)
			{
				companyPK = GlbCompany.CurrentCompany.PK;
			}

			var branchQuery = new ZDBOnlyQuery(typeof(GlbBranch));
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, filterForNonCurrentCompany ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, companyPK);

			var branches = new GlbBranchCollection(Factory, branchQuery);
			branches.Load();

			var branchesPKs = branches.GetPKs();
			if (branchesPKs.Count == 0)
			{
				return ZQuery.NoResultQuery;
			}

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobComInvoiceHeaderSchema.JZ_JE);
			declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_GB, branchesPKs);
			declarationSubQuery.AddToFilter(GetAdditionalDeclarationFilter());

			var jobComInvoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.PK);
			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			if (!filterForNonCurrentCompany)
			{
				jobComInvoiceHeaderSubQuery.AddToFilter(JoinCondition.Or, JobComInvoiceHeaderSchema.JZ_GB, branchesPKs);
			}
			jobComInvoiceHeaderSubQuery.AddSubQuery(declarationSubQuery, JoinCondition.Or);
			result.AddSubQuery(jobComInvoiceHeaderSubQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZQuery GetAdditionalDeclarationFilter()
		{
			return new ZQuery(JobDeclarationSchema.JE_ApplicationCode, SQLComparisonOperator.NotEqual, BaseJobDeclarationTypeDecider.EMCSApplicationCode);
		}

		ZQuery GetShipmentTypeFilter(SQLComparisonOperator op, ZString value)
		{
			return GetInvoiceOrDeclarationQuery(JobDeclarationSchema.JE_MessageType, JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, op, value);
		}

		ZQuery GetInvoiceOrDeclarationQuery(SchemaColumn declarationColumn, SchemaColumn invoiceColumn, SQLComparisonOperator op, object value)
		{
			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(declarationColumn, op, value);

			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			result.AddToFilter(invoiceColumn, op, value);
			result.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, declarationSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Number Filters

		protected ZQuery GetCommercialInvoiceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter_PossiblyCommaSeparated(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, comparisonOperator, value);
			return result;
		}

		#endregion

		#region Organisation Filters

		protected virtual void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var importerSupplierFilter = filters.AddGuidFilter(CommercialInvoiceFilterConstants.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Lookups.Importers, Lookups.Suppliers);
			importerSupplierFilter.SetItemDescriptions(Res.GetData("49FF3AF4-8070-471E-854C-AD4676A3495B", "Importer"), Res.GetData("660EDBFC-EFC5-4741-BB2F-592EBD47D167", "Supplier"));
			importerSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|ImporterSupplier", CommercialInvoiceFilterConstants.ImporterSupplier);

			var importerNameFilter = filters.AddTextFilter(CommercialInvoiceFilterConstants.ImporterName, OrgHeaderSchema.OH_FullName);
			importerNameFilter.Category = FilterCategories.Organisations;
			importerNameFilter.SubGroup = new OrganisationNameFilterSubGroup(JobComInvoiceHeaderSchema.JZ_OH_Buyer, JobDeclarationSchema.JE_OH_Importer);
			importerNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			importerNameFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|ImporterName", CommercialInvoiceFilterConstants.ImporterName);

			var supplierNameFilter = filters.AddTextFilter(CommercialInvoiceFilterConstants.SupplierName, OrgHeaderSchema.OH_FullName);
			supplierNameFilter.Category = FilterCategories.Organisations;
			supplierNameFilter.SubGroup = new OrganisationNameFilterSubGroup(JobComInvoiceHeaderSchema.JZ_OH_Supplier, JobDeclarationSchema.JE_OH_Supplier);
			supplierNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			supplierNameFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|SupplierName", CommercialInvoiceFilterConstants.SupplierName);

			var branchFilter = filters.AddGuidFilter(CommercialInvoiceFilterConstants.Branch, ModuleIDs.GlbBranch, JobComInvoiceHeaderSchema.JZ_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|Branch", CommercialInvoiceFilterConstants.Branch);
		}

		class OrganisationNameFilterSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaGuidColumn jzColumn;
			readonly SchemaColumn jeColumn;

			public OrganisationNameFilterSubGroup(SchemaGuidColumn jzColumn, SchemaColumn jeColumn)
			{
				this.jzColumn = jzColumn;
				this.jeColumn = jeColumn;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				subQuery.AddToFilter(filter);

				var decQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				decQuery.AddSubQuery(jeColumn, subQuery, JoinCondition.And);

				var andQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
				andQuery.AddToFilter(jzColumn, null);
				andQuery.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, decQuery, JoinCondition.And);

				var dbOnlyQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
				dbOnlyQuery.AddSubQuery(jzColumn, subQuery, JoinCondition.And);
				dbOnlyQuery.AddToFilter(andQuery, JoinCondition.Or);

				return dbOnlyQuery;
			}
		}

		protected ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			var result = new ZQuery();

			if (!importer.IsEmpty)
			{
				var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobComInvoiceHeaderSchema.JZ_JE);
				declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);

				var andQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
				andQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, null);
				andQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);

				var orQuery = new ZQuery();
				orQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Buyer, importer);
				orQuery.AddToFilter(andQuery, JoinCondition.Or);

				result.AddToFilter(orQuery);
			}

			if (!supplier.IsEmpty)
			{
				var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobComInvoiceHeaderSchema.JZ_JE);
				declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, supplier);

				var andQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
				andQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, null);
				andQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);

				var orQuery = new ZQuery();
				orQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplier);
				orQuery.AddToFilter(andQuery, JoinCondition.Or);

				result.AddToFilter(orQuery);
			}

			return result;
		}

		#endregion

		public OrgHeader LoggedInWebUsersOrg
		{
			get { return fLoggedInWebUsersOrg; }
			set { fLoggedInWebUsersOrg = value; }
		}
		OrgHeader fLoggedInWebUsersOrg;

		#region Routing

		protected void AddRoutingFilters(ModuleFilterCollection filters)
		{
			ModuleFilter vesselFilter = filters.AddNkFilter(CommercialInvoiceFilterConstants.Vessel, GetTransportVessel, ModuleIDs.RefVessel, Lookups.VesselList);
			vesselFilter.Category = FilterCategories.ModesAndTypes;
			vesselFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|Vessel", CommercialInvoiceFilterConstants.Vessel);
			filters.AddDateFilter(CommercialInvoiceFilterConstants.ETA, GetETAQuery).MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|ETA", CommercialInvoiceFilterConstants.ETA);

			ModuleFilter carrierFilter = filters.AddGuidFilter(CommercialInvoiceFilterConstants.RoutingCarrier, ModuleIDs.Organisation, GetCarrierQuery, Lookups.ShippingLines);
			carrierFilter.Category = FilterCategories.Organisations;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|RoutingCarrier", CommercialInvoiceFilterConstants.RoutingCarrier);

			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(CommercialInvoiceFilterConstants.LoadDischarge, GetLoadingQuery, Lookups.LocationList, Lookups.LocationList);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("CommercialInvoiceFilter|Load", "Load"), Res.GetData("CommercialInvoiceFilter|Discharge", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|LoadDischarge", CommercialInvoiceFilterConstants.LoadDischarge);
		}

		ZQuery GetTransportVessel(ZString value)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = value;

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(builder.ToDeclarationFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules));

			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			result.AddSubQuery(builder.ToCommercialInvoiceSubQuery(), JoinCondition.Or);
			result.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, declarationSubQuery, JoinCondition.Or);
			return result;
		}

		ZQuery GetETAQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.ETAFrom = value1;
			builder.ETATo = value2;
			builder.ETAComparisonOperator = comparisonOperator;

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(builder.ToDeclarationFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules));

			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			result.AddSubQuery(builder.ToCommercialInvoiceSubQuery(), JoinCondition.Or);
			result.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, declarationSubQuery, JoinCondition.Or);
			return result;
		}

		ZDBOnlyQuery GetCarrierQuery(ZGuid carrierPK)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));

			foreach (var subQuery in CreateCarrierTransportSubQueries(carrierPK, Core.Constants.TransportParentTypes.CommercialInvoice))
			{
				result.AddSubQuery(subQuery, JoinCondition.Or);
			}

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			foreach (var subQuery in CreateCarrierTransportSubQueries(carrierPK, Core.Constants.TransportParentTypes.Declaration))
			{
				declarationSubQuery.AddSubQuery(subQuery, JoinCondition.Or);
			}

			result.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, declarationSubQuery, JoinCondition.Or);

			return result;
		}

		ZDBOnlySubQuery[] CreateCarrierTransportSubQueries(ZGuid carrierPK, string jwParentType)
		{
			var transportCarrierAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolTransportSchema.JW_OA_CarrierAddress);
			transportCarrierAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);

			var notIncludeSeaLinkedTransportsQuery = new ZQuery();
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_TransportMode, SQLComparisonOperator.NotEqual, Core.Constants.TransportModes.Sea);
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_JX, null);

			var transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, jwParentType);
			transportSubQuery.AddToFilter(notIncludeSeaLinkedTransportsQuery, JoinCondition.And);
			transportSubQuery.AddSubQuery(transportCarrierAddressSubQuery, JoinCondition.And);

			// Linked Sea Transport
			var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageSubQuery.AddToFilter(JobVoyageSchema.JV_OH_Line, carrierPK);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

			var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			var linkedSeaTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			linkedSeaTransportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, jwParentType);
			linkedSeaTransportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);

			return new ZDBOnlySubQuery[]
			{
				transportSubQuery,
				linkedSeaTransportSubQuery
			};
		}

		ZDBOnlyQuery GetLoadingQuery(ZString value1, ZString value2)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = value1;
			builder.DischargePort = value2;

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(builder.ToDeclarationFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules));

			var result = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			result.AddSubQuery(builder.ToCommercialInvoiceSubQuery(), JoinCondition.Or);
			result.AddSubQuery(JobComInvoiceHeaderSchema.JZ_JE, declarationSubQuery, JoinCondition.Or);
			return result;
		}

		#endregion

		#region Line Filters

		protected void AddLineFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(CommercialInvoiceFilterConstants.LineOrderNumber, JobComInvoiceLineSchema.JI_OrderNumber);
			filter.MaxLength = JobComInvoiceLineSchema.JI_OrderNumber.MaxLength;
			filter.SubGroup = new InvoiceHeaderToInvoiceLineFilterSubGroup();
			filter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|LineOrderNumber", CommercialInvoiceFilterConstants.LineOrderNumber);
		}

		class InvoiceHeaderToInvoiceLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
				subQuery.AddToFilter(filter);

				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);
				return dbOnlyResult;
			}
		}

		#endregion

		#region Reference Filter

		protected void AddReferenceFilter(ModuleFilterCollection filters)
		{
			// Excluded from needing a SubGbroup in the unit test
			ReferenceModuleFilter referenceFilter = new ReferenceModuleFilter(CommercialInvoiceFilterConstants.References);
			referenceFilter.Category = FilterCategories.NumbersAndReferences;
			referenceFilter.MaxLength = AutoJobComInvoiceHeaderRefs.Schema.J2_ReferenceNumberMaxLength;
			referenceFilter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|References", CommercialInvoiceFilterConstants.References);
			filters.AddCustomFilter(referenceFilter);
		}

		#endregion

		void AddAttachedToDeclarationFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(CommercialInvoiceFilterConstants.AttachedToDeclaration, GetAttachedToDeclarationFilter, new AttachedToDeclarationFilterOptions());
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilter|AttachedToDeclaration", CommercialInvoiceFilterConstants.AttachedToDeclaration);
			filter.DefaultProperty = AttachedToDeclarationFilterOptions.Codes.NotAttachedToDeclaration;
			filter.Visibility = FilterVisibility.AlwaysApplied;
		}

		ZQuery GetAttachedToDeclarationFilter(ZString value)
		{
			if (value == AttachedToDeclarationFilterOptions.Codes.AttachedToDeclaration)
			{
				return new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.NotEqual, null);
			}

			if (value == AttachedToDeclarationFilterOptions.Codes.NotAttachedToDeclaration)
			{
				return new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, null);
			}

			return new ZQuery();
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelper(typeof(BaseJobComInvoiceHeader), WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode, Factory));
			return helpers;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}
	}
}
