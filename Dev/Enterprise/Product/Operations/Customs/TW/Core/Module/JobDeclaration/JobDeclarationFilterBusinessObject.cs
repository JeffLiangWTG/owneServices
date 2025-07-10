using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public static class FilterTypes
		{
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
			public const string MailBox = "Mail Box";
			public const string DeclarationType = "Declaration Type";
			public const string DeclarationDate = "Declaration Date";
			public const string ClearanceStatusText = "Clearance Status";
			public const string ImporterName = "Importer Name";
			public const string SupplierName = "Supplier Name";
			public const string PackingListNumber = "Packing List Number";
			public const string ImporterChineseName = "Importer Chinese Name";
			public const string SupplierChineseName = "Supplier Chinese Name";
			public const string ImporterVATNumber = "Importer VAT Number";
			public const string SupplierVATNumber = "Supplier VAT Number";
			public const string AgencyResponseCode = "Agency Response Code";
			public const string RequiredFormalitiesCode = "Required Formalities Code";
			public const string ClearanceCode = "Clearance Code";
		}

		#endregion

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);

			ModuleFilter consigneeFilter = filters.AddGuidFilter(FilterTypes.Consignee, ModuleIDs.Organisation, (value) => new ZQuery(JobDeclarationSchema.JE_OH_Consignee, value), Lookups.Consignees);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.IsPublishedOnWeb = false;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|Consignee", FilterTypes.Consignee);

			ModuleFilter consignorFilter = filters.AddGuidFilter(FilterTypes.Consignor, ModuleIDs.Organisation, (value) => new ZQuery(JobDeclarationSchema.JE_OH_Exporter, value), Lookups.Consignors);
			consignorFilter.Category = FilterCategories.Organisations;
			consignorFilter.IsPublishedOnWeb = false;
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|Consignor", FilterTypes.Consignor);

			var importerChineseNameFilter = filters.AddTextFilter(FilterTypes.ImporterChineseName, GetImporterChineseNameQuery);
			importerChineseNameFilter.Category = FilterCategories.Organisations;
			importerChineseNameFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|ImporterChineseName", FilterTypes.ImporterChineseName);
			importerChineseNameFilter.MaxLength = JobDocAddressSchema.E2_CompanyName.MaxLength;
			SetNameFilterComparisonOperatorList(importerChineseNameFilter);

			var supplierChineseNameFilter = filters.AddTextFilter(FilterTypes.SupplierChineseName, GetSupplierChineseNameQuery);
			supplierChineseNameFilter.Category = FilterCategories.Organisations;
			supplierChineseNameFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|SupplierChineseName", FilterTypes.SupplierChineseName);
			supplierChineseNameFilter.MaxLength = JobDocAddressSchema.E2_CompanyName.MaxLength;
			SetNameFilterComparisonOperatorList(supplierChineseNameFilter);

			var importerVATNumberFilter = filters.AddTextFilter(FilterTypes.ImporterVATNumber, GetImporterVATNumberQuery);
			importerVATNumberFilter.Category = FilterCategories.Organisations;
			importerVATNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|ImporterVATNumber", FilterTypes.ImporterVATNumber);
			importerVATNumberFilter.MaxLength = JobDocAddressNumberSchema.E2N_Number.MaxLength;

			var supplierVATNumberFilter = filters.AddTextFilter(FilterTypes.SupplierVATNumber, GetSupplierVATNumberQuery);
			supplierVATNumberFilter.Category = FilterCategories.Organisations;
			supplierVATNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|SupplierVATNumber", FilterTypes.SupplierVATNumber);
			supplierVATNumberFilter.MaxLength = JobDocAddressNumberSchema.E2N_Number.MaxLength;
		}

		protected override void AddImporterNameFilter(ModuleFilterCollection filters)
		{
			var importerNameFilter = filters.AddTextFilter(FilterTypes.ImporterName, GetImporterNameQuery);
			importerNameFilter.Category = FilterCategories.Organisations;
			importerNameFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|ImporterName", FilterTypes.ImporterName);
			importerNameFilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
			SetNameFilterComparisonOperatorList(importerNameFilter);
		}

		protected override void AddSupplierNameFilter(ModuleFilterCollection filters)
		{
			var supplierNameFilter = filters.AddTextFilter(FilterTypes.SupplierName, GetSupplierNameQuery);
			supplierNameFilter.Category = FilterCategories.Organisations;
			supplierNameFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|SupplierName", FilterTypes.SupplierName);
			supplierNameFilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
			SetNameFilterComparisonOperatorList(supplierNameFilter);
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			var filter = filters.AddNumberFilter(FilterTypes.MailBox, JobDeclarationSchema.JE_CustomsProfile);
			filter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|MailBox", FilterTypes.MailBox);

			var packingListNumberFilter = filters.AddNumberFilter(FilterTypes.PackingListNumber, CusPackingListSchema.CUL_PackingListNumber);
			packingListNumberFilter.SubGroup = new CustomsPackingListSubGroup();
			packingListNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|PackingListNumber", FilterTypes.PackingListNumber);
		}

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			base.AddModeFilters(filters);

			var declarationTypeFilter = filters.AddTextFilter(FilterTypes.DeclarationType, CusEntryInstructionSchema.CEI_Style, Lookups.DeclarationTypeList);
			declarationTypeFilter.SubGroup = new EntryInstructionSubGroup();
			declarationTypeFilter.Category = FilterCategories.ModesAndTypes;
			declarationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|DeclarationType", FilterTypes.DeclarationType);
		}

		protected override void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
			base.AddAdditionalDateFilters(filters);

			var declarationDateFilter = filters.AddDateFilter(FilterTypes.DeclarationDate, CusEntryInstructionSchema.CEI_DateForDuty);
			declarationDateFilter.SubGroup = new EntryInstructionSubGroup();
			declarationDateFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|DeclarationDate", FilterTypes.DeclarationDate);
		}

		class CustomsPackingListSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusPackingList), CusPackingListSchema.CUL_ClusterKey);
				subQuery.AddToFilter(filter);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
				dbOnlyResult.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, subQuery, JoinCondition.And);

				return dbOnlyResult;
			}
		}

		protected override ZQuery GetMessageStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNotSentForFilter = value == DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			value = isNotSentForFilter ? ZString.Empty : value;

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			cusEntryFilter.AddToFilter(CusEntryHeaderSchema.CH_Status, comparisonOperator, value);
			result.AddSubQuery(cusEntryFilter, JoinCondition.And);

			if (isNotSentForFilter)
			{
				var notInCusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
				result.AddSubQuery(notInCusEntryFilter, JoinCondition.Or);
			}
			return result;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddStatusAndFlagsFilters(result);
			return result;
		}

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			AddClearanceStatusFilter(filters);
			AddDispositionStatusFilter(filters);
		}

		void AddClearanceStatusFilter(ModuleFilterCollection filters)
		{
			var clearanceStatusFilter = filters.AddTextFilter(FilterTypes.ClearanceStatusText, GetClearanceStatusQuery, Lookups.ClearanceStatusList);
			clearanceStatusFilter.Category = FilterCategories.StatusAndFlags;
			clearanceStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|TW|DeclarationFilter|ClearanceStatusText", "Clearance Status");
			SetClearanceStatusFilterComparisonOperatorList(clearanceStatusFilter);
		}

		void AddDispositionStatusFilter(ModuleFilterCollection filters)
		{
			var agencyResponseCodeFilter = filters.AddTextFilter(FilterTypes.AgencyResponseCode, GetAgencyResponseCodeQuery, Lookups.AgencyResponseCodeList);
			agencyResponseCodeFilter.Category = FilterCategories.StatusAndFlags;
			agencyResponseCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|AgencyResponseCode", FilterTypes.AgencyResponseCode);
			agencyResponseCodeFilter.MaxLength = CusDispositionSchema.CDI_Status.MaxLength;
			SetDispositionStatusFilterComparisonOperatorList(agencyResponseCodeFilter);

			var requiredFormalitiesCodeFilter = filters.AddTextFilter(FilterTypes.RequiredFormalitiesCode, GetRequiredFormalitiesCodeQuery, Lookups.RequiredFormalitiesCodeList);
			requiredFormalitiesCodeFilter.Category = FilterCategories.StatusAndFlags;
			requiredFormalitiesCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|RequiredFormalitiesCode", FilterTypes.RequiredFormalitiesCode);
			requiredFormalitiesCodeFilter.MaxLength = CusDispositionSchema.CDI_Status.MaxLength;
			SetDispositionStatusFilterComparisonOperatorList(requiredFormalitiesCodeFilter);

			var clearanceCodeFilter = filters.AddTextFilter(FilterTypes.ClearanceCode, GetClearanceCodeQuery, Lookups.ClearanceCodeList);
			clearanceCodeFilter.Category = FilterCategories.StatusAndFlags;
			clearanceCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TW|Customs|DeclarationFilter|ClearanceCode", FilterTypes.ClearanceCode);
			clearanceCodeFilter.MaxLength = CusDispositionSchema.CDI_Status.MaxLength;
			SetDispositionStatusFilterComparisonOperatorList(clearanceCodeFilter);
		}

		ZQuery GetClearanceStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == ClearanceStatusCodeList.Codes.NOT)
			{
				if (comparisonOperator == SQLComparisonOperator.Equal)
				{
					comparisonOperator = SQLComparisonOperator.IsBlank;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					comparisonOperator = SQLComparisonOperator.IsNotBlank;
				}
				value = ZString.Empty;
			}
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.NotEqual;
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryFilter = GetEntryHeaderSubQuery(comparisonOperator, value, false);
			declarationQuery.AddSubQuery(cusEntryFilter, JoinCondition.Or);
			if (notIn)
			{
				var notInEntryHeaderQuery = GetEntryHeaderSubQuery(comparisonOperator, value, true);
				declarationQuery.AddSubQuery(notInEntryHeaderQuery, JoinCondition.Or);
			}
			return declarationQuery;

			ZDBOnlySubQuery GetEntryHeaderSubQuery(SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
			{
				var cusEntryFilter = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
				var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				if (notIn && comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
				}
				else if (notIn && comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, SQLComparisonOperator.NotEqual, value);
				}
				else
				{
					entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
				}
				cusEntryFilter.AddSubQuery(entryNumberQuery, JoinCondition.And);
				return cusEntryFilter;
			}
		}

		ZQuery GetImporterNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressCompanyNameSubQuery(comparisonOperator, value, DocAddressTypes.Codes.ImporterDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetSupplierNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressCompanyNameSubQuery(comparisonOperator, value, DocAddressTypes.Codes.SupplierDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlySubQuery GetJobDocAddressCompanyNameSubQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString docAddressType)
		{
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressType);

			var overrideQuery = new ZQuery(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, true);
			overrideQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, value);

			var nonOverrideQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			nonOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, false);
			var orgAddressSubQuery = GetOrgAddressCompanyNameSubQuery(comparisonOperator, value);
			nonOverrideQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);

			var jobDocAddressOrQuery = new ZQuery(overrideQuery, JoinCondition.Or, nonOverrideQuery);
			jobDocAddressQuery.AddToFilter(jobDocAddressOrQuery);

			return jobDocAddressQuery;
		}

		ZDBOnlySubQuery GetOrgAddressCompanyNameSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
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

		ZQuery GetImporterChineseNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressChineseCompanyNameSubQuery(comparisonOperator, value, DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressTypes.Codes.ImporterTranslatedDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetSupplierChineseNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressChineseCompanyNameSubQuery(comparisonOperator, value, DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlySubQuery GetJobDocAddressChineseCompanyNameSubQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString docAddressType, ZString translatedAddressType)
		{
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

			var overrideQuery = new ZQuery(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, true);
			overrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, translatedAddressType);
			overrideQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, value);

			var nonOverrideQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			nonOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, false);
			nonOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressType);

			var orgAddressSubQuery = GetOrgAddressChineseCompanyNameSubQuery(comparisonOperator, value);
			nonOverrideQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);

			var jobDocAddressOrQuery = new ZQuery(overrideQuery, JoinCondition.Or, nonOverrideQuery);
			jobDocAddressQuery.AddToFilter(jobDocAddressOrQuery);

			return jobDocAddressQuery;
		}

		ZDBOnlySubQuery GetOrgAddressChineseCompanyNameSubQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgTranslatedAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgTranslatedAddress), OrgTranslatedAddressSchema.OTA_OA);
			orgTranslatedAddressSubQuery.AddToFilter(OrgTranslatedAddressSchema.OTA_Language, Core.SharedConstants.Languages.ChineseTraditional);
			orgTranslatedAddressSubQuery.AddToFilter(OrgTranslatedAddressSchema.OTA_CompanyName, comparisonOperator, value);

			var orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddress.AddSubQuery(orgTranslatedAddressSubQuery, JoinCondition.And);
			return orgAddress;
		}

		ZDBOnlySubQuery GetJobDocAddressVATNumberSubQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString docAddressType)
		{
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressType);

			var jobDocAddressNumberSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddressNumber), JobDocAddressNumberSchema.E2N_E2);
			jobDocAddressNumberSubQuery.AddToFilter(JobDocAddressNumberSchema.E2N_NumberType, OrgCusCode.CodeTypes.VATCode);
			jobDocAddressNumberSubQuery.AddToFilter(JobDocAddressNumberSchema.E2N_Number, comparisonOperator, value);

			var overrideQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			overrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, true);
			overrideQuery.AddSubQuery(jobDocAddressNumberSubQuery, JoinCondition.And);

			var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Taiwan);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.VATCode);
			orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, comparisonOperator, value);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddSubQuery(orgCusCodeSubQuery, JoinCondition.And);

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var nonOverrideQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			nonOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, false);
			nonOverrideQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);

			var jobDocAddressOrQuery = new ZQuery(overrideQuery, JoinCondition.Or, nonOverrideQuery);
			jobDocAddressQuery.AddToFilter(jobDocAddressOrQuery);
			return jobDocAddressQuery;
		}

		ZQuery GetImporterVATNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressVATNumberSubQuery(comparisonOperator, value, DocAddressTypes.Codes.ImporterDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetSupplierVATNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobDocAddressQuery = GetJobDocAddressVATNumberSubQuery(comparisonOperator, value, DocAddressTypes.Codes.SupplierDocumentaryAddress);
			query.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetAgencyResponseCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetCusDispositionQuery(comparisonOperator, CusDispositionStatusKeyList.Codes.ARM, value);
		}

		ZQuery GetRequiredFormalitiesCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetCusDispositionQuery(comparisonOperator, CusDispositionStatusKeyList.Codes.RFM, value);
		}

		ZQuery GetClearanceCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetCusDispositionQuery(comparisonOperator, CusDispositionStatusKeyList.Codes.CLR, value);
		}

		ZDBOnlyQuery GetCusDispositionQuery(SQLComparisonOperator comparisonOperator, ZString statusKey, ZString value)
		{
			var notIn = comparisonOperator == SQLComparisonOperator.NotContains || comparisonOperator == SQLComparisonOperator.IsBlank;
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var inEntryHeaderQuery = GetEntryHeaderSubQuery(comparisonOperator, statusKey, value, notIn);
			declarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, CusEntryHeaderSchema.CH_ClusterKey, inEntryHeaderQuery, JoinCondition.Or);
			if (notIn)
			{
				var notInEntryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK, true);
				declarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, CusEntryHeaderSchema.CH_ClusterKey, notInEntryHeaderQuery, JoinCondition.Or);
			}
			return declarationQuery;

			ZDBOnlySubQuery GetEntryHeaderSubQuery(SQLComparisonOperator comparisonOperator, ZString statusKey, ZString value, bool notIn)
			{
				var inEntryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
				var dispositionSubQuery = GetCusDispositionSubQuery(comparisonOperator, statusKey, value, notIn);
				inEntryHeaderQuery.AddSubQuery(dispositionSubQuery, JoinCondition.And);
				return inEntryHeaderQuery;
			}
		}

		ZDBOnlySubQuery GetCusDispositionSubQuery(SQLComparisonOperator comparisonOperator, ZString statusKey, ZString value, bool notIn)
		{
			var cusDispositionSubQuery = new ZDBOnlySubQuery(typeof(CusDisposition), CusDispositionSchema.CDI_ParentID, notIn);
			cusDispositionSubQuery.AddToFilter(CusDispositionSchema.CDI_ParentTableCode, SQLComparisonOperator.Equal, CusEntryHeaderSchema.Constants.Prefix);
			cusDispositionSubQuery.AddToFilter(CusDispositionSchema.CDI_StatusKey, SQLComparisonOperator.Equal, statusKey);
			cusDispositionSubQuery.AddToFilter(CusDispositionSchema.CDI_Type, SQLComparisonOperator.Equal, Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			if (comparisonOperator == SQLComparisonOperator.Contains || comparisonOperator == SQLComparisonOperator.NotContains)
			{
				cusDispositionSubQuery.AddToFilter(CusDispositionSchema.CDI_Status, SQLComparisonOperator.Equal, value);
			}
			return cusDispositionSubQuery;
		}

		void SetNameFilterComparisonOperatorList(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
		}

		void SetDispositionStatusFilterComparisonOperatorList(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
		}

		void SetClearanceStatusFilterComparisonOperatorList(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
		}
	}
}
