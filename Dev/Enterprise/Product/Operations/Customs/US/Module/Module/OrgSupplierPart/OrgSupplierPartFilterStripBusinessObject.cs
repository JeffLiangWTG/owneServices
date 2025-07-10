using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		protected override void ClassificationFilter(ModuleFilterCollection result)
		{
			result.AddGuidFilter(OrgSupplierPartFilterConstants.Classificaton.HTSClassification, ModuleIDs.ImportClassification, GetClassificationQuery, ImportClassifications);
			result.AddGuidFilter(OrgSupplierPartFilterConstants.Classificaton.ScheduleBClassification, ModuleIDs.ExportClassification, GetClassificationQuery, ExportClassifications);
		}

		protected override bool NeedCustomsTypeFilter => false;
		protected override bool NeedTariffCodeFilter => false;

		protected override ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			var result = base.GetOrgSupplierPartModuleFiltersCore();

			AddTariffFilters(result);
			AddCountryFilters(result);
			AddSPIFilters(result);
			AddPermitFilters(result);
			AddExportFilters(result);
			AddCWOFilters(result);
			AddAttributeFilters(result);
			AddIndicatorFilters(result);

			return result;
		}

		#region Classification Filters

		public ImportClassificationCollection ImportClassifications => new ImportClassificationCollection(Factory);

		public ExportClassificationCollection ExportClassifications => new ExportClassificationCollection(Factory);

		#endregion

		#region Tariff Filters

		protected void AddTariffFilters(ModuleFilterCollection filters)
		{
			var tariffProvTariffFilter = new TariffProvTariffModuleFilter(OrgSupplierPartFilterConstants.Tariff.TariffProvTariff, GetTariffProvTariffFilter);
			tariffProvTariffFilter.SetItemDescriptions(new ResourceStringData("", OrgSupplierPartFilterConstants.Tariff.TariffCaption), new ResourceStringData("", OrgSupplierPartFilterConstants.Tariff.ProvTariff));
			tariffProvTariffFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(tariffProvTariffFilter);

			var multiTariffIndicatorFilter = filters.AddFlagsFilter(OrgSupplierPartFilterConstants.Tariff.MultiTariffIndicator, new string[] { OrgSupplierPartFilterConstants.Show }, new GetFlagsQuery[] { GetMultiTariffIndicatorQuery });

			var tariffTypeFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Tariff.TariffType, GetTariffTypeQuery, Lookups.TariffTypeList);

			var tariffInvalidFilter = new TariffInvalidModuleFilter(OrgSupplierPartFilterConstants.Tariff.TariffInvalid, GetTariffInvalidFilter);
			tariffInvalidFilter.Category = FilterCategories.StatusAndFlags;
			filters.AddCustomFilter(tariffInvalidFilter);
		}

		#region Tariff/Provisional Tariff Queries

		ZQuery GetTariffProvTariffFilter(ZString tariff, ZString provTariff)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			if (provTariff.IsEmpty)
			{
				return TariffOnlyQuery(result, tariff);
			}
			else if (tariff.IsEmpty)
			{
				return ProvTariffOnlyQuery(result, provTariff);
			}
			else
			{
				return TariffProvTariffQuery(result, tariff, provTariff);
			}
		}

		ZQuery TariffOnlyQuery(ZDBOnlyQuery result, ZString tariff)
		{
			var formattedTariff = new TariffFormatter().Format(tariff);
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			var classificationQuery = new ZDBOnlySubQuery(typeof(CusClassification), CusClassificationSchema.PK);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, formattedTariff);

			var pivotTariffQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.PK);
			pivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, formattedTariff);
			pivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);

			var childPivotTariffQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_CI_Parent);
			childPivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, formattedTariff);
			childPivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
			pivotTariffQuery.AddSubQuery(childPivotTariffQuery, JoinCondition.Or);

			pivotSubQuery.AddSubQuery(CusClassPartPivotSchema.CI_CC, classificationQuery, JoinCondition.Or);
			pivotSubQuery.AddSubQuery(pivotTariffQuery, JoinCondition.Or);

			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery ProvTariffOnlyQuery(ZDBOnlyQuery result, ZString provTariff)
		{
			var provisionalTariff = new TariffFormatter().Format(provTariff);
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			var pivotTariffQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.PK);
			pivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_SupplementalTariff, SQLComparisonOperator.StartsWith, provisionalTariff);
			pivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);

			var childPivotTariffQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_CI_Parent);
			childPivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_SupplementalTariff, SQLComparisonOperator.StartsWith, provisionalTariff);
			childPivotTariffQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
			pivotTariffQuery.AddSubQuery(childPivotTariffQuery, JoinCondition.Or);

			pivotSubQuery.AddSubQuery(pivotTariffQuery, JoinCondition.Or);
			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery TariffProvTariffQuery(ZDBOnlyQuery result, ZString tariff, ZString provTariff)
		{
			var formattedTariff = new TariffFormatter().Format(tariff);
			var cusClassPartPivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			cusClassPartPivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_ChildType, ClassificationTypeList.Codes.HTI);

			var classificationSubQuery = new ZDBOnlySubQuery(typeof(CusClassification), CusClassificationSchema.PK);
			classificationSubQuery.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, formattedTariff);
			classificationSubQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

			var partPivotTariffSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.PK);
			partPivotTariffSubQuery.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, formattedTariff);
			partPivotTariffSubQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);

			var tariffNumberQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.PK);
			tariffNumberQuery.AddSubQuery(CusClassPartPivotSchema.CI_CC, classificationSubQuery, JoinCondition.Or);
			tariffNumberQuery.AddSubQuery(partPivotTariffSubQuery, JoinCondition.Or);

			cusClassPartPivotSubQuery.AddSubQuery(tariffNumberQuery, JoinCondition.And);
			result.AddSubQuery(cusClassPartPivotSubQuery, JoinCondition.And);

			var provisionalTariff = new TariffFormatter().Format(provTariff);
			var provTariffPivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			provTariffPivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_SupplementalTariff, SQLComparisonOperator.StartsWith, provisionalTariff);
			provTariffPivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);

			result.AddSubQuery(provTariffPivotSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		public ZQuery GetTariffTypeQuery(ZString value)
		{
			if ((value == ClassificationTypeList.Codes.HTI) || (value == ClassificationTypeList.Codes.HTE) || (value == ClassificationTypeList.Codes.SHB))
			{
				var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				var subQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
				subQuery.AddToFilter(CusClassPartPivotSchema.CI_ChildType, value);
				subQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.UnitedStates);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		public ZQuery GetMultiTariffIndicatorQuery(ZBool value)
		{
			if (value)
			{
				var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				var subCusClassPartPivot = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
				subCusClassPartPivot.AddFilterAndZSQLParameterCollection(string.Format("{0} IS NOT NULL", CusClassPartPivotSchema.CI_CI_Parent.Name), new ZSqlParameterCollection());
				result.AddSubQuery(subCusClassPartPivot, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		ZQuery GetCusClassPartPivotTable(SchemaColumn column, SQLComparisonOperator filterOperator, ZString value, Type typeOfBusinessObjectToQuery, SchemaGuidColumn fK, bool importTariffType = true, bool isCensusWarningOverride = false)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var subCusClassPartPivot = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			var isCensusAndBlankOrNotEqual = (isCensusWarningOverride && (filterOperator == SpecialComparisonOperator.IsBlank || filterOperator == SQLComparisonOperator.NotEqual));

			var subQuery = new ZDBOnlySubQuery(typeOfBusinessObjectToQuery, fK, isCensusAndBlankOrNotEqual);

			if (!(isCensusWarningOverride && filterOperator == SpecialComparisonOperator.IsBlank))
			{
				subQuery.AddToFilter(column, filterOperator, value);
			}

			if (isCensusWarningOverride)
			{
				subQuery.AddToFilter(CusCodeDataSchema.CY_Type, SQLComparisonOperator.Equal, CusCodeDataTypeList.Codes.CensusWarningOverride);
			}

			subCusClassPartPivot.AddSubQuery(subQuery, JoinCondition.And);
			if (importTariffType)
			{
				subCusClassPartPivot.AddToFilter(CusClassPartPivotSchema.CI_ChildType, SQLComparisonOperator.Equal, ClassificationTypeList.Codes.HTI);
			}
			else
			{
				var childType = new ZDBOnlyQuery(typeof(CusClassPartPivot));
				childType.AddToFilter(CusClassPartPivotSchema.CI_ChildType, SQLComparisonOperator.Equal, ClassificationTypeList.Codes.HTE);
				childType.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_ChildType, SQLComparisonOperator.Equal, ClassificationTypeList.Codes.SHB);
				subCusClassPartPivot.AddToFilter(childType);
			}
			result.AddSubQuery(subCusClassPartPivot, JoinCondition.And);

			return result;
		}

		ZQuery GetCusUSClassification(SchemaColumn column, SQLComparisonOperator filterOperator, ZString value, bool importTariffType = true)
		{
			return GetCusClassPartPivotTable(column, filterOperator, value, typeof(CusUSClassification), CusUSClassificationSchema.CD_ParentID, importTariffType);
		}

		ZQuery GetCusCodeData(SchemaColumn column, SQLComparisonOperator filterOperator, ZString value, bool isCensusWarningOverride = false)
		{
			return GetCusClassPartPivotTable(column, filterOperator, value, typeof(CensusWarningOverride), CusCodeDataSchema.CY_ParentID, true, isCensusWarningOverride);
		}

		ZQuery GetCusAttributeFilter(SchemaColumn column, SQLComparisonOperator filterOperator, ZString value, ZString attributeName)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var subCusClassPartPivot = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);

			var subQuery = new ZDBOnlySubQuery(typeof(CusAttributeFilter), CusAttributeFilterSchema.BG_CI);
			subQuery.AddToFilter(column, filterOperator, value);
			subQuery.AddToFilter(CusAttributeFilterSchema.BG_AttributeName, SQLComparisonOperator.Equal, attributeName);

			subCusClassPartPivot.AddSubQuery(subQuery, JoinCondition.And);
			result.AddSubQuery(subCusClassPartPivot, JoinCondition.And);

			return result;
		}

		#region Country Filters
		protected void AddCountryFilters(ModuleFilterCollection filters)
		{
			var countryOfOriginFilter = filters.AddNkFilter(OrgSupplierPartFilterConstants.Country.CountryOfOrigin, GetCountryOfOriginQuery, ModuleIDs.Customs.US.Country, new USCCountryCollection(Factory));
			countryOfOriginFilter.Category = FilterCategories.Locations;
			countryOfOriginFilter.MaxLength = CusUSClassificationSchema.CD_UC_NKCountryOfOrigin.MaxLength;

			var countryOfExportFilter = filters.AddNkFilter(OrgSupplierPartFilterConstants.Country.CountryOfExport, GetCountryOfExportQuery, ModuleIDs.Customs.US.Country, new USCCountryCollection(Factory));
			countryOfExportFilter.Category = FilterCategories.Locations;
			countryOfExportFilter.MaxLength = CusUSClassificationSchema.CD_UC_NKCountryOfExport.MaxLength;

			var manufacturerFilter = filters.AddNkFilter(OrgSupplierPartFilterConstants.Manufacturer.Code, GetManufacturerQuery, ModuleIDs.Organisation, new MasterFiles.Business.ConsignorCollection(Factory));
			manufacturerFilter.Category = FilterCategories.Organisations;
			manufacturerFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
		}

		public ZQuery GetCountryOfOriginQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_UC_NKCountryOfOrigin, filterOperator, value);
		}

		public ZQuery GetCountryOfExportQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_UC_NKCountryOfExport, filterOperator, value);
		}

		public ZQuery GetManufacturerQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var subCusClassPartPivot = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			var subCusUSClassification = new ZDBOnlySubQuery(typeof(CusUSClassification), CusUSClassificationSchema.CD_ParentID);

			if (filterOperator != SpecialComparisonOperator.IsBlank)
			{
				var subOrgAddress = new ZDBOnlySubQuery(typeof(MasterFiles.Business.OrgAddress), OrgAddressSchema.PK);
				var subOrgHeader = new ZDBOnlySubQuery(typeof(MasterFiles.Business.OrgHeader), OrgHeaderSchema.PK);

				subOrgHeader.AddToFilter(OrgHeaderSchema.OH_Code, filterOperator, value);
				subOrgAddress.AddSubQuery(OrgAddressSchema.OA_OH, subOrgHeader, JoinCondition.And);
				subCusUSClassification.AddSubQuery(CusUSClassificationSchema.CD_OA_Manufacturer, subOrgAddress, JoinCondition.And);
			}
			else
			{
				subCusUSClassification.AddToFilter(CusUSClassificationSchema.CD_OA_Manufacturer, SQLComparisonOperator.Equal, DBNull.Value);
			}

			subCusClassPartPivot.AddSubQuery(subCusUSClassification, JoinCondition.And);
			result.AddSubQuery(subCusClassPartPivot, JoinCondition.And);
			return result;
		}
		#endregion

		#region SPI Filters
		protected void AddSPIFilters(ModuleFilterCollection filters)
		{
			var indicatorFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.SPI.SPIIndicator, GetSPIIndicatorQuery, new SPICompleteList());
			indicatorFilter.MaxLength = CusUSClassificationSchema.CD_SPI.MaxLength;

			var productClaimFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.SPI.ProductClaim, GetProductClaimQuery, new SecondarySpecProgIndicatorList());
			productClaimFilter.MaxLength = CusUSClassificationSchema.CD_ProductClaim.MaxLength;
		}

		public ZQuery GetSPIIndicatorQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_SPI, filterOperator, value);
		}

		public ZQuery GetProductClaimQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ProductClaim, filterOperator, value);
		}

		#endregion

		#region PGA Indicator Filter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void AddIndicatorFilters(ModuleFilterCollection filters)
		{
			var lacey = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.LaceyActIndicator, (filterOperator, value) => GetLaceyActIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			lacey.MaxLength = CusUSClassificationSchema.CD_LaceyActIndicator.MaxLength;
			var fda = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.FDAIndicator, (filterOperator, value) => GetPGAFDAIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			fda.MaxLength = CusUSClassificationSchema.CD_ACEFDAIndicator.MaxLength;
			var nhtsa = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NHTSAIndicator, (filterOperator, value) => GetNHTSAIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			nhtsa.MaxLength = CusUSClassificationSchema.CD_NHTSAIndicator.MaxLength;
			var ods = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.ODSIndicator, (filterOperator, value) => GetODSIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			ods.MaxLength = CusUSClassificationSchema.CD_ODSIndicator.MaxLength;
			var pst = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.PSTIndicator, (filterOperator, value) => GetPSTIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			pst.MaxLength = CusUSClassificationSchema.CD_PSTIndicator.MaxLength;
			var tsa = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.TSCAIndicator, (filterOperator, value) => GetTSCAIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			tsa.MaxLength = CusUSClassificationSchema.CD_TSCAClaimIndicator.MaxLength;
			var vne = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.VNEIndicator, (filterOperator, value) => GetVNEIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			vne.MaxLength = CusUSClassificationSchema.CD_VNEIndicator.MaxLength;
			var omc = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.OMCIndicator, (filterOperator, value) => GetOMCIndicatorQuery(filterOperator, value, true), new OGAIndicatorList());
			omc.MaxLength = CusUSClassificationSchema.CD_OMCIndicator.MaxLength;
			var atf = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.ATFIndicator, (filterOperator, value) => GetATFIndicatorQuery(filterOperator, value, true), OGAIndicatorWithoutDisclaimerList);
			atf.MaxLength = CusUSClassificationSchema.CD_ATFIndicator.MaxLength;
			var ams = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.AMSIndicator, (filterOperator, value) => GetAMSIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			ams.MaxLength = CusUSClassificationSchema.CD_AMSIndicator.MaxLength;
			var nop = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NOPIndicator, (filterOperator, value) => GetNOPIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			nop.MaxLength = CusUSClassificationSchema.CD_NOPIndicator.MaxLength;
			var ttb = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.TTBIndicator, (filterOperator, value) => GetTTBIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			ttb.MaxLength = CusUSClassificationSchema.CD_TTBIndicator.MaxLength;
			var cpsc = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.CPSCIndicator, (filterOperator, value) => GetCPSCIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			cpsc.MaxLength = CusUSClassificationSchema.CD_CPSCIndicator.MaxLength;
			var dea = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.DEAIndicator, (filterOperator, value) => GetDEAIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			dea.MaxLength = CusUSClassificationSchema.CD_DEAIndicator.MaxLength;
			var aphis = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.APHISIndicator, (filterOperator, value) => GetAPHISIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			aphis.MaxLength = CusUSClassificationSchema.CD_APHISIndicator.MaxLength;
			var ddtc = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.DDTCIndicator, (filterOperator, value) => GetDDTCIndicatorQuery(filterOperator, value, true), OGAIndicatorWithoutDisclaimerList);
			ddtc.MaxLength = CusUSClassificationSchema.CD_DDTCIndicator.MaxLength;
			var fws = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.FWSIndicator, (filterOperator, value) => GetFWSIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			fws.MaxLength = CusUSClassificationSchema.CD_FWSIndicator.MaxLength;
			var nmfs370 = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NMFS370Indicator, (filterOperator, value) => GetNMFS370IndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			nmfs370.MaxLength = CusUSClassificationSchema.CD_NMFS370Indicator.MaxLength;
			var coa = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NMFSCOAIndicator, (filterOperator, value) => GetNMFSCOAIndicatorQuery(filterOperator, value, true), OGAIndicatorWithoutDisclaimerList);
			coa.MaxLength = CusUSClassificationSchema.CD_NMFSCOAIndicator.MaxLength;
			var amr = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NMFSAMRIndicator, (filterOperator, value) => GetNMFSAMRIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			amr.MaxLength = CusUSClassificationSchema.CD_NMFSAMRIndicator.MaxLength;
			var hms = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NMFSHMSIndicator, (filterOperator, value) => GetNMFSHMSIndicatorQuery(filterOperator, value, true), Factory.GetCachedValue<OGAIndicatorList>());
			hms.MaxLength = CusUSClassificationSchema.CD_NMFSHMSIndicator.MaxLength;
			var simp = filters.AddTextFilter(OrgSupplierPartFilterConstants.PGA.NMFSSIMPIndicator, (filterOperator, value) => GetNMFSSIMPIndicatorQuery(filterOperator, value, true), OGAIndicatorWithoutDisclaimerList);
			simp.MaxLength = CusUSClassificationSchema.CD_NMFSSIMPIndicator.MaxLength;

			var amsExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.AMSIndicator, (filterOperator, value) => GetAMSIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			amsExport.MaxLength = CusUSClassificationSchema.CD_AMSIndicator.MaxLength;
			var atfExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.ATFIndicator, (filterOperator, value) => GetATFIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			atfExport.MaxLength = CusUSClassificationSchema.CD_ATFIndicator.MaxLength;
			var deaExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.DEAIndicator, (filterOperator, value) => GetDEAIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			deaExport.MaxLength = CusUSClassificationSchema.CD_DEAIndicator.MaxLength;
			var epaExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.EPAIndicator, (filterOperator, value) => GetPSTIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			epaExport.MaxLength = CusUSClassificationSchema.CD_PSTIndicator.MaxLength;
			var fwsExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.FWSIndicator, (filterOperator, value) => GetFWSIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			fwsExport.MaxLength = CusUSClassificationSchema.CD_FWSIndicator.MaxLength;
			var nmfsExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.NMFSIndicator, (filterOperator, value) => GetNMFSHMSIndicatorQuery(filterOperator, value, false), OGAIndicatorWithoutDisclaimerList);
			nmfsExport.MaxLength = CusUSClassificationSchema.CD_NMFSHMSIndicator.MaxLength;
			var ttbExport = filters.AddTextFilter(OrgSupplierPartFilterConstants.ExportPGA.TTBIndicator, (filterOperator, value) => GetTTBIndicatorQuery(filterOperator, value, false), Factory.GetCachedValue<OGAIndicatorList>());
			ttbExport.MaxLength = CusUSClassificationSchema.CD_TTBIndicator.MaxLength;
		}

		public OGAIndicatorList OGAIndicatorWithoutDisclaimerList
		{
			get
			{
				var list = new OGAIndicatorList();
				list.RemoveCode("C");
				return list;
			}
		}

		public ZQuery GetLaceyActIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_LaceyActIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetOMCIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_OMCIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNHTSAIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NHTSAIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetPGAFDAIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ACEFDAIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetODSIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ODSIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetPSTIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_PSTIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetTSCAIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_TSCAClaimIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetVNEIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_VNEIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetATFIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ATFIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetAMSIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_AMSIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNOPIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NOPIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetDDTCIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_DDTCIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetTTBIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_TTBIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetCPSCIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_CPSCIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetDEAIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_DEAIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetFWSIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_FWSIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNMFSHMSIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NMFSHMSIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNMFSAMRIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NMFSAMRIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNMFS370IndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NMFS370Indicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNMFSCOAIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NMFSCOAIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetNMFSSIMPIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_NMFSSIMPIndicator, filterOperator, value, importTariffType);
		}

		public ZQuery GetAPHISIndicatorQuery(SQLComparisonOperator filterOperator, ZString value, ZBool importTariffType)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_APHISIndicator, filterOperator, value, importTariffType);
		}

		#endregion

		#region Permit Filters
		protected void AddPermitFilters(ModuleFilterCollection filters)
		{
			var aDDCaseNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.ADDCaseNum, GetADDCaseNumQuery);
			aDDCaseNumFilter.Category = FilterCategories.NumbersAndReferences;
			aDDCaseNumFilter.MaxLength = CusUSClassificationSchema.CD_ADDCaseNo.MaxLength;
			var cVDNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.CVDCaseNum, GetCVDCaseNumQuery);
			cVDNumFilter.Category = FilterCategories.NumbersAndReferences;
			cVDNumFilter.MaxLength = CusUSClassificationSchema.CD_CVDCaseNo.MaxLength;
			var cBTPACertificateFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.CBTPACertificate, GetCBTPACertificateQuery);
			cBTPACertificateFilter.Category = FilterCategories.NumbersAndReferences;
			cBTPACertificateFilter.MaxLength = CusUSClassificationSchema.CD_CBTPACertificate.MaxLength;
			var woolLicenseNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.WoolLicenseNum, GetWoolLicenseNumQuery);
			woolLicenseNumFilter.Category = FilterCategories.NumbersAndReferences;
			woolLicenseNumFilter.MaxLength = CusUSClassificationSchema.CD_WoolLicenceNo.MaxLength;
			var miscLicenseNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.MiscLicenseNum, GetMiscLicenseNumQuery);
			miscLicenseNumFilter.Category = FilterCategories.NumbersAndReferences;
			miscLicenseNumFilter.MaxLength = CusUSClassificationSchema.CD_MiscLicenceNo.MaxLength;
			var cASugarCertificateFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.CASugarCertificate, GetCASugarCertificateQuery);
			cASugarCertificateFilter.Category = FilterCategories.NumbersAndReferences;
			cASugarCertificateFilter.MaxLength = CusUSClassificationSchema.CD_SugarCertificate.MaxLength;
			var agricultureLicenseNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.AgricultureLicenseNum, GetAgricultureLicenseNumQuery);
			agricultureLicenseNumFilter.Category = FilterCategories.NumbersAndReferences;
			agricultureLicenseNumFilter.MaxLength = CusUSClassificationSchema.CD_AgricultureLicenceNo.MaxLength;
			var cottonFeeExemptFilter = filters.AddFlagsFilter(OrgSupplierPartFilterConstants.Permits.CottonFeeExempt, new string[] { OrgSupplierPartFilterConstants.Show }, new GetFlagsQuery[] { GetCottonFeeExemptQuery });
			var rulingTypeFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.RulingType, GetRulingTypeQuery, new PIRPRulingTypeList());
			rulingTypeFilter.MaxLength = CusUSClassificationSchema.CD_RulingType.MaxLength;
			var rulingNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Permits.RulingNum, GetRulingNumQuery);
			rulingNumFilter.Category = FilterCategories.NumbersAndReferences;
			rulingNumFilter.MaxLength = CusUSClassificationSchema.CD_RulingNumber.MaxLength;
			filters.AddFlagsFilter(OrgSupplierPartFilterConstants.Permits.MissingADCVInfo, new string[] { OrgSupplierPartFilterConstants.Show }, new GetFlagsQuery[] { GetMissingADCVInfoQuery });
		}

		public ZQuery GetADDCaseNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ADDCaseNo, filterOperator, value);
		}

		public ZQuery GetCVDCaseNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_CVDCaseNo, filterOperator, value);
		}

		public ZQuery GetCBTPACertificateQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_CBTPACertificate, filterOperator, value);
		}

		public ZQuery GetWoolLicenseNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_WoolLicenceNo, filterOperator, value);
		}

		public ZQuery GetMiscLicenseNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_MiscLicenceNo, filterOperator, value);
		}

		public ZQuery GetCASugarCertificateQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_SugarCertificate, filterOperator, value);
		}

		public ZQuery GetAgricultureLicenseNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_AgricultureLicenceNo, filterOperator, value);
		}

		public ZQuery GetCottonFeeExemptQuery(ZBool value)
		{
			if (value)
			{
				return GetCusUSClassification(CusUSClassificationSchema.CD_CottonFeeExempt, SQLComparisonOperator.Equal, "Y");
			}
			else
			{
				return GetCusUSClassification(CusUSClassificationSchema.CD_CottonFeeExempt, SQLComparisonOperator.Equal, "N");
			}
		}

		public ZQuery GetRulingTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_RulingType, filterOperator, value);
		}

		public ZQuery GetRulingNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_RulingNumber, filterOperator, value);
		}

		public ZQuery GetMissingADCVInfoQuery(ZBool showMissing)
		{
			var result = new ZQuery();
			if (showMissing)
			{
				var queryText = string.Format(CultureInfo.InvariantCulture, @"
				OP_PK IN
				(
					{0}
				)", GetADCVInformationFromTariff());
				var missingADCVQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				missingADCVQuery.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
				result.AddToFilter(missingADCVQuery);
			}
			return result;
		}

		ZString GetADCVInformationFromTariff()
		{
			return string.Format(CultureInfo.InvariantCulture,
@"SELECT {0} FROM {1}
JOIN {2} ON {3} = {4}
JOIN (SELECT {5}, {6}, {7}, {8}, {9}, {10}, {11} FROM {12} WHERE {13} = 'CI') AS AA ON {14} = {9}
JOIN (SELECT {15}, {16}, {17} FROM {18} WHERE {19} < '{21}' AND {20} >= '{21}') AS BB ON {22} = {15}
JOIN {23} ON CHARINDEX({24}, {15}) > 0
JOIN (SELECT * FROM {28} WHERE {29} = '{30}') AS CC ON {25} = {26} AND {27} = {10}
WHERE {31} in ('US', 'PR')
AND {32} IN ('{33}')
AND (({5} = '0' AND {7} = '' AND {16} = 'Y' AND {35} like 'A%') OR ({6} = '0' AND {8} = '' AND {17} = 'Y' AND {35} like 'C%'))
AND {11} <> 'X'

UNION ALL

SELECT {0} FROM {1}
JOIN (SELECT {5}, {6}, {7}, {8}, {9}, {10}, {11} FROM {12} WHERE {13} = 'CI') AS AA ON {14} = {9}
JOIN (SELECT {15}, {16}, {17} FROM {18} WHERE {19} < '{21}' AND {20} >= '{21}') AS BB ON {34} = {15}
JOIN {23} ON CHARINDEX({24}, {15}) > 0
JOIN (SELECT * FROM {28} WHERE {29} = '{30}') AS CC ON {25} = {26} AND {27} = {10}
WHERE {31} in ('US', 'PR')
AND {32} IN ('{33}')
AND (({5} = '0' AND {7} = '' AND {16} = 'Y' AND {35} like 'A%') OR ({6} = '0' AND {8} = '' AND {17} = 'Y' AND {35} like 'C%'))
AND {11} <> 'X'

UNION ALL

SELECT {0} FROM {1}
JOIN {2} ON {3} = {4}
JOIN (SELECT {5}, {6}, {7}, {8}, {9}, {10}, {11} FROM {12} WHERE {13} = 'CI') AS AA ON {14} = {9}
JOIN (SELECT {15}, {16}, {17} FROM {18} WHERE {19} < '{21}' AND {20} >= '{21}') AS BB ON {36} = {15}
JOIN {23} ON CHARINDEX({24}, {15}) > 0
JOIN (SELECT * FROM {28} WHERE {29} = '{30}') AS CC ON {25} = {26} AND {27} = {10}
WHERE {31} in ('US', 'PR')
AND {32} = '{37}'
AND (({5} = '0' AND {7} = '' AND {16} = 'Y' AND {35} like 'A%') OR ({6} = '0' AND {8} = '' AND {17} = 'Y' AND {35} like 'C%'))
AND {11} <> 'X'
"
, CusClassPartPivotSchema.Constants.CI_OP
, CusClassPartPivotSchema.Constants.TableName
, CusClassificationSchema.Constants.TableName
, CusClassPartPivotSchema.Constants.CI_CC
, CusClassificationSchema.Constants.PK
, CusUSClassificationSchema.Constants.CD_ADDApplicable
, CusUSClassificationSchema.Constants.CD_CVDApplicable
, CusUSClassificationSchema.Constants.CD_ADDCaseNo
, CusUSClassificationSchema.Constants.CD_CVDCaseNo
, CusUSClassificationSchema.Constants.CD_ParentID
, CusUSClassificationSchema.Constants.CD_UC_NKCountryOfOrigin
, CusUSClassificationSchema.Constants.CD_ProductClaim
, CusUSClassificationSchema.Constants.TableName
, CusUSClassificationSchema.Constants.CD_ParentTableCode
, CusClassPartPivotSchema.Constants.PK
, USCTariffSchema.Constants.UE_Tariff
, USCTariffSchema.Constants.UE_AntiDumping
, USCTariffSchema.Constants.UE_CountervailingDutyFlag
, USCTariffSchema.Constants.TableName
, USCTariffSchema.Constants.UE_DateFrom
, USCTariffSchema.Constants.UE_DateTo
, ZDateTime.Today.ToShortDateString()
, CusClassificationSchema.Constants.CC_TariffNum
, USCACCaseTariffSchema.Constants.TableName
, USCACCaseTariffSchema.Constants.U9_TariffNumber
, USCACCaseSchema.Constants.U5_CaseNumber
, USCACCaseTariffSchema.Constants.U9_CaseNumber
, USCACCaseSchema.Constants.U5_ISOCountryCode
, USCACCaseSchema.Constants.TableName
, USCACCaseSchema.Constants.U5_CaseStatus
, ACCaseStatusList.Codes.AC
, CusClassPartPivotSchema.Constants.CI_RN_NKCountry
, CusClassPartPivotSchema.Constants.CI_ChildType
, string.Join("','", new[] { ClassificationTypeList.Codes.HTI, ClassificationChildTypeList.Codes.COMPONENT, ClassificationChildTypeList.Codes.Related })
, CusClassPartPivotSchema.Constants.CI_TariffNum
, USCACCaseSchema.Constants.U5_CaseNumber
, CusClassPartPivotSchema.Constants.CI_SupplementalTariff
, ClassificationTypeList.Codes.HTI);
		}

		#endregion

		#region Export Filters
		protected void AddExportFilters(ModuleFilterCollection filters)
		{
			var exportCodeFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Export.Code, GetExportCodeQuery);
			exportCodeFilter.MaxLength = CusUSClassificationSchema.CD_ExportCode.MaxLength;
			var originIndicatorFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Export.OriginIndicator, GetOriginIndicatorQuery, new AESOriginIndicatorList());
			originIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			originIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			originIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			originIndicatorFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			originIndicatorFilter.MaxLength = CusUSClassificationSchema.CD_OriginIndicator.MaxLength;
			var eCCNFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Export.ECCN, GetECCNQuery);
			eCCNFilter.Category = FilterCategories.NumbersAndReferences;
			eCCNFilter.MaxLength = CusUSClassificationSchema.CD_ECCN.MaxLength;
			var iTARExemptionNumFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.Export.ITARExemptionNum, GetITARExemptionNumQuery);
			iTARExemptionNumFilter.Category = FilterCategories.NumbersAndReferences;
			iTARExemptionNumFilter.MaxLength = CusUSClassificationSchema.CD_ITARExemptionNo.MaxLength;
		}

		public ZQuery GetExportCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ExportCode, filterOperator, value, false);
		}

		public ZQuery GetOriginIndicatorQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_OriginIndicator, filterOperator, value, false);
		}

		public ZQuery GetECCNQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ECCN, filterOperator, value, false);
		}

		public ZQuery GetITARExemptionNumQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusUSClassification(CusUSClassificationSchema.CD_ITARExemptionNo, filterOperator, value, false);
		}

		#endregion

		#region CWO Filters
		protected void AddCWOFilters(ModuleFilterCollection filters)
		{
			var cWOCondtionFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.CWO.ConditionCode, GetCWOConditionCodeQuery, new CensusWarningCodeList());
			cWOCondtionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			cWOCondtionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			cWOCondtionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			cWOCondtionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			cWOCondtionFilter.MaxLength = CusCodeDataSchema.CY_Code.MaxLength;

			var cWOOverrideFilter = filters.AddTextFilter(OrgSupplierPartFilterConstants.CWO.OverrideCode, GetCWOOverrideQuery, new CensusOverrideCodeList());
			cWOOverrideFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			cWOOverrideFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			cWOOverrideFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			cWOOverrideFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			cWOOverrideFilter.MaxLength = CusCodeDataSchema.CY_Data.MaxLength;
		}

		public ZQuery GetCWOConditionCodeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusCodeData(CusCodeDataSchema.CY_Code, filterOperator, value, true);
		}

		public ZQuery GetCWOOverrideQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusCodeData(CusCodeDataSchema.CY_Data, filterOperator, value, true);
		}

		#endregion

		#region Attribute Filters
		protected void AddAttributeFilters(ModuleFilterCollection filters)
		{
			var attribute1Filter = filters.AddTextFilter(OrgSupplierPartFilterConstants.AppliesTo.Attribute1, GetAttribute1Query);
			attribute1Filter.MaxLength = CusAttributeFilterSchema.BG_AttributeValue1.MaxLength;
			var attribute2Filter = filters.AddTextFilter(OrgSupplierPartFilterConstants.AppliesTo.Attribute2, GetAttribute2Query);
			attribute2Filter.MaxLength = CusAttributeFilterSchema.BG_AttributeValue1.MaxLength;
			var attribute3Filter = filters.AddTextFilter(OrgSupplierPartFilterConstants.AppliesTo.Attribute3, GetAttribute3Query);
			attribute3Filter.MaxLength = CusAttributeFilterSchema.BG_AttributeValue1.MaxLength;
		}

		public ZQuery GetAttribute1Query(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusAttributeFilter(CusAttributeFilterSchema.BG_AttributeValue1, filterOperator, value, "AT1");
		}

		public ZQuery GetAttribute2Query(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusAttributeFilter(CusAttributeFilterSchema.BG_AttributeValue1, filterOperator, value, "AT2");
		}

		public ZQuery GetAttribute3Query(SQLComparisonOperator filterOperator, ZString value)
		{
			return GetCusAttributeFilter(CusAttributeFilterSchema.BG_AttributeValue1, filterOperator, value, "AT3");
		}

		#endregion
		#region Date Filters

		public ZQuery GetTariffInvalidFilter(ZDate expiredDate)
		{
			var result = new ZQuery();
			var queryText = string.Format(CultureInfo.InvariantCulture, @"
				OP_PK IN
				(
					{0}
				)", GetInvalidTariffNumbersSQL(expiredDate));
			var tariffInvalidQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			tariffInvalidQuery.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
			result.AddToFilter(tariffInvalidQuery);
			return result;
		}

		ZString GetInvalidTariffNumbersSQL(ZDateTime expiredDate)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
SELECT {0} FROM {1}
WHERE {2} IN ('US', 'PR')
AND 
(
	({3} <> '' AND {4} IN ('{5}') AND {3} NOT IN (SELECT {6} FROM {7} WHERE {8} <= '{27}' AND {9} >= '{27}'))
	OR
	({3} <> '' AND {4} = '{10}' AND {3} NOT IN (SELECT {11} FROM {12} WHERE {19} <= '{27}' AND {20} >= '{27}' AND {21} = 'US' AND {22} IN (SELECT {23} FROM {24} WHERE {25} = 'US' AND {26} = '{10}')))
	OR
	({13} <> '' AND {4} = '{14}' AND {13} NOT IN (SELECT {6} FROM {7} WHERE {8} <= '{27}' AND {9} >= '{27}'))
)

UNION ALL

SELECT {0} FROM {1}
JOIN (SELECT {15}, {16} FROM {17} WHERE {15} NOT IN (SELECT {6} FROM {7} WHERE {8} <= '{27}' AND {9} >= '{27}') AND {15} NOT IN (SELECT {11} FROM {12})) AS AA ON {16} = {18}
WHERE {2} IN ('US', 'PR')
AND
(
	({15} <> '' AND {4} IN ('{5}') AND {15} NOT IN (SELECT {6} FROM {7} WHERE {8} <= '{27}' AND {9} >= '{27}'))
	OR
	({15} <> '' AND {4} = '{10}' AND {15} NOT IN (SELECT {11} FROM {12}))
)
"
, CusClassPartPivotSchema.Constants.CI_OP
, CusClassPartPivotSchema.Constants.TableName
, CusClassPartPivotSchema.Constants.CI_RN_NKCountry
, CusClassPartPivotSchema.Constants.CI_TariffNum
, CusClassPartPivotSchema.Constants.CI_ChildType
, string.Join("','", new[] { ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationChildTypeList.Codes.COMPONENT, ClassificationChildTypeList.Codes.Related })
, USCTariffSchema.Constants.UE_Tariff
, USCTariffSchema.Constants.TableName
, USCTariffSchema.Constants.UE_DateFrom
, USCTariffSchema.Constants.UE_DateTo
, Universal.Constants.TariffTypes.ScheduleB
, TariffViewSchema.Constants.ZZ1_TariffCode
, TariffViewSchema.Constants.TableName
, CusClassPartPivotSchema.Constants.CI_SupplementalTariff
, ClassificationTypeList.Codes.HTI
, CusClassificationSchema.Constants.CC_TariffNum
, CusClassificationSchema.Constants.PK
, CusClassificationSchema.Constants.TableName
, CusClassPartPivotSchema.Constants.CI_CC
, TariffViewSchema.Constants.ZZ1_StartDate
, TariffViewSchema.Constants.ZZ1_EndDate
, TariffViewSchema.Constants.ZZ1_ZZZ_NKDataGrouping
, TariffViewSchema.Constants.ZZ1_ZZI_TariffType
, RefCusTariffTypeSchema.Constants.PK
, RefCusTariffTypeSchema.Constants.TableName
, RefCusTariffTypeSchema.Constants.ZZI_ZZZ_NKDataGrouping
, RefCusTariffTypeSchema.Constants.ZZI_TariffType
, expiredDate.ToShortDateString()
);
		}

		#endregion

		public OrgSupplierPartFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new OrgSupplierPartFilterLookups(this);
				}
				return lookups;
			}
		}
		OrgSupplierPartFilterLookups lookups;
	}
}
