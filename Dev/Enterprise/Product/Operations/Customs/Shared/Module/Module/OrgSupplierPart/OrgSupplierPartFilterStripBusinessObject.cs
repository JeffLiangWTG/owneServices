using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : MasterFiles.Module.OrgSupplierPartFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetOrgSupplierPartModuleFiltersCore();
			ClassificationFilter(result);
			if (NeedCustomsTypeFilter)
			{
				var customsTypeFilter = new ModuleTextFilter(OrgSupplierPartFilterConstants.CustomsType, GetCustomsTypesFilter, GetClassificationTypeList());
				result.AddCustomFilter(customsTypeFilter);
				customsTypeFilter.Category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|CustomsHeader", OrgSupplierPartFilterConstants.CustomsHeader));
				customsTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|CustomsType", OrgSupplierPartFilterConstants.CustomsType);
			}
			if (NeedTariffCodeFilter)
			{
				var tariffCodeFilter = result.AddTextFilter(OrgSupplierPartFilterConstants.TariffCode, GetTariffCodeFilter);
				tariffCodeFilter.MaxLength = CusClassificationSchema.CC_TariffNum.MaxLength;
				tariffCodeFilter.Category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|CustomsHeader", OrgSupplierPartFilterConstants.CustomsHeader));
				tariffCodeFilter.MaxLength = CusClassificationSchema.CC_TariffNum.MaxLength;
				tariffCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|TariffCode", OrgSupplierPartFilterConstants.TariffCode);
			}
			return result;
		}

		protected virtual CodeDescriptionPairList GetClassificationTypeList()
		{
			return Factory.GetCachedValue<ClassificationTypeList>();
		}

		ZQuery GetCustomsTypesFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var countryCode = MasterFiles.Business.GlbCompany.CurrentCompany.Country.Code;
			var subQueryFromPivot = new ZDBOnlySubQuery(typeof(BaseCusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			subQueryFromPivot.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, countryCode);
			subQueryFromPivot.AddToFilter(CusClassPartPivotSchema.CI_ChildType, value);

			query.AddSubQuery(subQueryFromPivot, JoinCondition.And);

			return query;
		}

		ZQuery GetTariffCodeFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var countryCode = MasterFiles.Business.GlbCompany.CurrentCompany.Country.Code;

			var subQueryFromPivot = new ZDBOnlySubQuery(typeof(BaseCusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			subQueryFromPivot.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, comparisonOperator, value);

			if (comparisonOperator is IsBlankComparisonOperator)
			{
				subQueryFromPivot.AddToFilter(CusClassPartPivotSchema.CI_CC, SQLComparisonOperator.Equal, null);
			}
			else
			{
				var subQueryFromClassification = new ZDBOnlySubQuery(typeof(BaseCusClassification), CusClassificationSchema.PK);
				subQueryFromClassification.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
				subQueryFromClassification.AddToFilter(CusClassificationSchema.CC_TariffNum, comparisonOperator, value);
				subQueryFromPivot.AddSubQuery(CusClassPartPivotSchema.CI_CC, subQueryFromClassification, JoinCondition.Or);
			}

			subQueryFromPivot.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, countryCode);
			query.AddSubQuery(subQueryFromPivot, JoinCondition.And);
			return query;
		}

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore
		{
			get
			{
				return new FilterCategory[]
				{
					FilterCategories.NumbersAndReferences,
					FilterCategories.StatusAndFlags,
					FilterCategories.Dates,
					FilterCategories.Locations,
					FilterCategories.Organisations,
					FilterCategories.Customs,
					FilterCategories.FinancialDetails,
					FilterCategories.ModesAndTypes,
					FilterCategories.TextSearch,
					FilterCategories.AuditInformation,
					FilterCategories.Other
				};
			}
		}

		protected virtual bool NeedCustomsTypeFilter => true;
		protected virtual bool NeedTariffCodeFilter => true;

		protected virtual void ClassificationFilter(ModuleFilterCollection result)
		{
			result.AddGuidFilter("Classification", ModuleIDs.SingleTariffClassification, GetClassificationQuery, Classifications).MultilingualDescription = ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|Classification", "Classification");
		}

		protected ZQuery GetClassificationQuery(ZGuid classificationPk)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			if (!classificationPk.IsEmpty)
			{
				ZDBOnlyQuery classificationFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(BaseCusClassPartPivot), CusClassPartPivotSchema.CI_OP);
				ZDBOnlySubQuery classificationSubQuery = new ZDBOnlySubQuery(typeof(BaseCusClassification), CusClassPartPivotSchema.CI_CC);
				classificationSubQuery.AddToFilter(CusClassificationSchema.PK, classificationPk);
				pivotSubQuery.AddSubQuery(classificationSubQuery, JoinCondition.And);
				classificationFilter.AddSubQuery(pivotSubQuery, JoinCondition.And);

				result.AddToFilter(classificationFilter);
			}
			return result;
		}

		public virtual IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<BaseCusClassification>(Factory);
	}
}
