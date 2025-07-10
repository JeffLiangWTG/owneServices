using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CusPackingListFilterBusinessObject : FilterStripBusinessObject
	{
		public static class FilterTypes
		{
			#region SuppressResourceStringsCheckRegion

			public const string JobNumber = "Packing List Job #";
			public const string PackingListNumber = "Packing List #";
			public const string PackingListDate = "Packing List Date";
			public const string ImporterSupplier = "Importer/Supplier";
			public const string ImporterName = "Importer Name";
			public const string SupplierName = "Supplier Name";
			public const string Remarks = "Remarks";
			public const string EntryNumber = "Entry #";
			public const string DeclarationJobNumber = "Declaration Job #";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddNumberFilter(result);
			AddDateFilters(result);
			AddRemarksFilters(result);
			AddImporterSupplierFilter(result);
			return result;
		}

		void AddNumberFilter(ModuleFilterCollection filters)
		{
			var jobNumberFilter = filters.AddNumberFilter(FilterTypes.JobNumber, PkgPackageJobSchema.KJ_JobID);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|JobNumber", FilterTypes.JobNumber);
			jobNumberFilter.MaxLength = PkgPackageJobSchema.KJ_JobID.MaxLength;
			jobNumberFilter.UseMultiSearch = false;
			jobNumberFilter.SubGroup = new PkgPackageJobSubGroup();
			RemoveNegativeAndBlankComparisonOperator(jobNumberFilter);

			filters.AddNumberFilter(FilterTypes.PackingListNumber, CusPackingListSchema.CUL_PackingListNumber).MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|PackingListNumber", FilterTypes.PackingListNumber);

			var declarationJobFilter = filters.AddNumberFilter(FilterTypes.DeclarationJobNumber, JobDeclarationSchema.JE_DeclarationReference);
			declarationJobFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|DeclarationJobNumber", FilterTypes.DeclarationJobNumber);
			declarationJobFilter.MaxLength = JobDeclarationSchema.JE_DeclarationReference.MaxLength;
			declarationJobFilter.SubGroup = JobDeclarationSubGroup;

			var entryNumberFilter = filters.AddNumberFilter(FilterTypes.EntryNumber, GetEntryNumberQuery);
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|EntryNumber", FilterTypes.EntryNumber);
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			entryNumberFilter.SubGroup = JobDeclarationSubGroup;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(FilterTypes.PackingListDate, CusPackingListSchema.CUL_PackingListDate).MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|PackingListDate", FilterTypes.PackingListDate);
		}

		void AddImporterSupplierFilter(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter importerSupplierFilter = filters.AddGuidFilter(FilterTypes.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Consignees, Consignors);
			importerSupplierFilter.SetItemDescriptions(Res.GetData("Customs|CusPackingListFilter|Importer", "Importer"), Res.GetData("Customs|CusPackingListFilter|Supplier", "Supplier"));
			importerSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|ImporterSupplier", FilterTypes.ImporterSupplier);
			importerSupplierFilter.SubGroup = JobDeclarationSubGroup;

			var importerNamefilter = filters.AddTextFilter("Importer Name", GetImporterNameQuery);
			importerNamefilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|ImporterName", FilterTypes.ImporterName);
			importerNamefilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
			importerNamefilter.SubGroup = JobDeclarationSubGroup;
			RemoveNegativeAndBlankComparisonOperator(importerNamefilter);

			var supplierNamefilter = filters.AddTextFilter("Supplier Name", GetSupplierNameQuery);
			supplierNamefilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|SupplierName", FilterTypes.SupplierName);
			supplierNamefilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
			supplierNamefilter.SubGroup = JobDeclarationSubGroup;
			RemoveNegativeAndBlankComparisonOperator(supplierNamefilter);
		}

		void AddRemarksFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilterForMultipleColumns(FilterTypes.Remarks, CusPackingListSchema.CUL_Remarks).MultilingualDescription = ResString.GetMultilingualString("Customs|CusPackingListFilter|Remarks", FilterTypes.Remarks);
		}

		ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			if (!importer.IsEmpty)
			{
				jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);
			}

			if (!supplier.IsEmpty)
			{
				var subQueries = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				var subSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				subSubQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_OH_Supplier, supplier));
				subQueries.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplier);
				subQueries.AddAsUnionQuery(subSubQuery, true);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				dbOnlyResult.AddSubQuery(subQueries, JoinCondition.And);
				jobDeclarationQuery.AddToFilter(dbOnlyResult);
			}
			return jobDeclarationQuery;
		}

		ZQuery GetImporterNameQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetOrgNameQuery(comparisonOperator, value, DocAddressTypes.Codes.ImporterDocumentaryAddress);

		ZQuery GetSupplierNameQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetOrgNameQuery(comparisonOperator, value, DocAddressTypes.Codes.SupplierDocumentaryAddress);

		ZQuery GetOrgNameQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString docAddressType)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressSubQuery(comparisonOperator, value, docAddressType);
			jobDeclarationQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		ZDBOnlySubQuery GetJobDocAddressSubQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString docAddressType)
		{
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressType);

			var overrideQuery = new ZQuery(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, true);
			overrideQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, value);

			var nonOverrideQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			nonOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, false);
			var orgAddressSubQuery = GetOrgAddressSubQuery(comparisonOperator, value);
			nonOverrideQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);

			var jobDocAddressOrQuery = new ZQuery(overrideQuery, JoinCondition.Or, nonOverrideQuery);
			jobDocAddressQuery.AddToFilter(jobDocAddressOrQuery);

			return jobDocAddressQuery;
		}

		ZDBOnlySubQuery GetOrgAddressSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);

			var companyNameOverrideQuery = new ZQuery(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator, value);
			var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

			var companyNameOverrideEmptyQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			companyNameOverrideEmptyQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, SQLComparisonOperator.Equal, ZString.Empty);
			companyNameOverrideEmptyQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderQuery, JoinCondition.And);

			var orQuery = new ZQuery(companyNameOverrideQuery, JoinCondition.Or, companyNameOverrideEmptyQuery);
			orgAddress.AddToFilter(orQuery);
			return orgAddress;
		}

		ZQuery GetEntryNumberQuery(SQLComparisonOperator operartor, ZString value)
		{
			return EntryNumberQueryGenerator.GetEntryNumberQuery(operartor, value, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		void RemoveNegativeAndBlankComparisonOperator(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
		}

		OrgHeaderCollection Consignees
		{
			get
			{
				if (consignees == null)
				{
					consignees = new ConsigneeCollection(Factory);
				}
				return consignees;
			}
		}
		OrgHeaderCollection consignees;

		OrgHeaderCollection Consignors
		{
			get
			{
				if (consignors == null)
				{
					consignors = new ConsignorCollection(Factory);
				}
				return consignors;
			}
		}
		OrgHeaderCollection consignors;

		JobDeclarationSubGroup JobDeclarationSubGroup
		{
			get { return jobDeclarationSubGroup ?? (jobDeclarationSubGroup = new JobDeclarationSubGroup()); }
		}
		JobDeclarationSubGroup jobDeclarationSubGroup;
	}

	class JobDeclarationSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var packingListQuery = new ZDBOnlyQuery(typeof(CusPackingList));
			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			jobDeclarationQuery.AddToFilter(filter);
			packingListQuery.AddSubQuery(CusPackingListSchema.CUL_JE, jobDeclarationQuery, JoinCondition.And);
			return packingListQuery;
		}
	}

	class PkgPackageJobSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var packingListQuery = new ZDBOnlyQuery(typeof(CusPackingList));
			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageJobSubQuery.AddToFilter(filter);
			packingListQuery.AddSubQuery(packageJobSubQuery, JoinCondition.And);
			return packingListQuery;
		}
	}
}
