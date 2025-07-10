using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class RefCusTariffFilterStripBusinessObject : Universal.Module.RefCusTariffFilterStripBusinessObject
	{
		public RefCusTariffFilterStripBusinessObject() : base()
		{
		}

		public RefCusTariffFilterStripBusinessObject(ChildTariffViewCollection collection) : base(collection)
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var existingFilter = result[Universal.Constants.RefCusTariffFilters.CountryOrGrouping] as ModuleNkFilter;
			if (existingFilter != null)
			{
				var existingDelegate = existingFilter.QueryDelegate as GetNkQuery;
				result.RemoveFilter(existingFilter);

				var countryOrGroupingFilter = result.AddNkFilter(Constants.RefCusTariffFilters.CountryOrGrouping, existingDelegate, Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.RefDataGrouping, new MasterFiles.Business.RefDataGroupingCollection(Factory));
				countryOrGroupingFilter.ForeignCodeColumnOverride = RefDataGroupingSchema.ZZZ_DataGrouping;
				countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("RefCusTariffFilter|Country/Region or Grouping", "Country/Region or Grouping");
				countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;
			}

			return result;
		}

		internal static ZQuery GetTariffCodePlusCheckDigitQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var tariffCodeQuery = new ZDBOnlyQuery(typeof(TariffView));
			if (!value.IsEmpty)
			{
				var tariffCode = value.SubstringSafe(0, 10).Trim();
				var checkDigit = value.SubstringSafe(10, 2).Trim();

				tariffCodeQuery.AddToFilter(TariffViewSchema.ZZ1_TariffCode, comparisonOperator, tariffCode);
				if (!checkDigit.IsEmpty)
				{
					var checkDigitSubQuery = new ZDBOnlySubQuery(typeof(TariffAttributeView), TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode);
					checkDigitSubQuery.AddToFilter(TariffAttributeViewSchema.ZZ3_Name, UniversalReferenceConstants.TariffAttributes.CheckDigit);
					checkDigitSubQuery.AddToFilter(TariffAttributeViewSchema.ZZ3_Value, checkDigit);
					tariffCodeQuery.AddSubQuery(checkDigitSubQuery, JoinCondition.And);
				}
			}

			return tariffCodeQuery;
		}

		protected override void ApplyInitialCode(ZString code, string propertyName)
		{
			if (propertyName == TariffViewSchema.Constants.ZZ1_TariffCode)
			{
				if (!code.IsEmpty)
				{
					var tariffCode = code.SubstringSafe(0, 10).Trim();
					var checkDigit = code.SubstringSafe(10, 2).Trim();

					var tariffFilter = (ModuleTextFilter)ModuleFilters[RefCusTariffFilterConstants.Tariff];
					if (tariffFilter != null)
					{
						tariffFilter.Property = tariffCode;
					}

					if (!checkDigit.IsEmpty)
					{
						var checkDigitFilter = (ModuleTextFilter)ModuleFilters[RefCusTariffFilterConstants.CheckDigit];
						if (checkDigitFilter != null)
						{
							checkDigitFilter.Visibility = FilterVisibility.AlwaysVisible;
							checkDigitFilter.Property = checkDigit;
						}
					}
				}
				else
				{
					base.ApplyInitialCode(code, propertyName);
				}
			}
		}
	}
}
