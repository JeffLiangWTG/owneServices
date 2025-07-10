using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module FilterBusinessObject for USCTariff.
	/// Override validation and filter SQL generation here.
	/// </summary>
	public class USCTariffFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Constants
		{
			public const string QuotaIndicator = "Quota Indicator";
			public const string AntiDumpingFlag = "Anti Dumping Flag";
			public const string CountervailingDutyFlag = "Countervailing Duty Flag";
		}

		public CodeDescriptionPairList SpecProgramIndicatorList => SPICompleteList.GetCachedList(Factory);

		public CodeDescriptionPairList PermitLicenseTypeList => LicencePermitTypeList.GetLicencePermitTypeList(Factory);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddDateFilter(USCTariff.FilterSchema.Date, GetDateQuery).MultilingualDescription = ResString.GetMultilingualString("050e6056-2af4-45b3-bd3f-bbc778622e65", USCTariff.FilterSchema.Date);
			var tariff = result.AddTextFilter(USCTariff.FilterSchema.Tariff, GetTariffQuery);
			tariff.MaxLength = USCTariffSchema.UE_Tariff.MaxLength + 2;
			tariff.Visibility = FilterVisibility.AlwaysVisible;
			tariff.MultilingualDescription = ResString.GetMultilingualString("acc15fe8-5949-4eed-bddc-11ceb5b60b99", USCTariff.FilterSchema.Tariff);

			var pGACode = result.AddTextFilter(USCTariff.FilterSchema.PGACode, USCTariffSchema.UE_PGACodes);
			pGACode.MaxLength = USCTariffSchema.UE_PGACodes.MaxLength;
			pGACode.MultilingualDescription = ResString.GetMultilingualString("079ce794-be71-483c-aa84-b3efa0918b9f", USCTariff.FilterSchema.PGACode);
			pGACode.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
			pGACode.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			pGACode.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			pGACode.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);

			result.AddTextFilter(USCTariff.FilterSchema.Description, USCTariffSchema.UE_ShortDescription).MultilingualDescription = ResString.GetMultilingualString("56cd629e-7acd-4beb-88c0-888750b5adee", USCTariff.FilterSchema.Description);

			AddSPIFilter(result);
			AddPermitLicenseCodeFilter(result);
			AddFlagsFilter(result);
			return result;
		}

		protected void AddSPIFilter(ModuleFilterCollection filters)
		{
			var spiFilter = filters.AddTextFilter(USCTariff.FilterSchema.SPI, GetSPIQuery, SpecProgramIndicatorList);
			spiFilter.MultilingualDescription = ResString.GetMultilingualString("9cf34018-9841-47bd-80a3-dca24d8b67e1", USCTariff.FilterSchema.SPI);
			AddContainBlankOptions(spiFilter);
		}

		protected void AddPermitLicenseCodeFilter(ModuleFilterCollection filters)
		{
			var permitFilter = filters.AddTextFilter(USCTariff.FilterSchema.PermitLicenseCode, GetPermitLicenseCodeQuery, PermitLicenseTypeList);
			permitFilter.MultilingualDescription = ResString.GetMultilingualString("5e0e1811-c934-48aa-a09c-eaf687ed4190", USCTariff.FilterSchema.PermitLicenseCode);
			permitFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			permitFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			permitFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			permitFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);

			permitFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
		}

		protected void AddFlagsFilter(ModuleFilterCollection filters)
		{
			var antiDumpingFlagFilter = filters.AddFlagsFilter(USCTariffFilterStripBusinessObject.Constants.AntiDumpingFlag, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetAntiDumpingFlagQuery });
			antiDumpingFlagFilter.Property0 = true;
			antiDumpingFlagFilter.Category = FilterCategories.StatusAndFlags;
			antiDumpingFlagFilter.MultilingualDescription = ResString.GetMultilingualString("944433b5-d90f-4ee0-bfe5-e32677240583", USCTariffFilterStripBusinessObject.Constants.AntiDumpingFlag);

			var quotaIndicatorFlagFilter = filters.AddFlagsFilter(USCTariffFilterStripBusinessObject.Constants.QuotaIndicator, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetQuotaIndicatorQuery });
			quotaIndicatorFlagFilter.Property0 = true;
			quotaIndicatorFlagFilter.Category = FilterCategories.StatusAndFlags;
			quotaIndicatorFlagFilter.MultilingualDescription = ResString.GetMultilingualString("79486fd4-3d5f-4d8f-b786-e82b3f4ccd1f", USCTariffFilterStripBusinessObject.Constants.QuotaIndicator);

			var countervailingDutyFlagFilter = filters.AddFlagsFilter(USCTariffFilterStripBusinessObject.Constants.CountervailingDutyFlag, new string[] { DeclarationFilterConstants.Show }, new GetFlagsQuery[] { GetCountervilingDutyFlag });
			countervailingDutyFlagFilter.Property0 = true;
			countervailingDutyFlagFilter.Category = FilterCategories.StatusAndFlags;
			countervailingDutyFlagFilter.MultilingualDescription = ResString.GetMultilingualString("fb74d33e-109f-415f-8ae4-baf6e5fba454", USCTariffFilterStripBusinessObject.Constants.CountervailingDutyFlag);
		}

		protected override void ApplyInitialCode(ZString code, string propertyName)
		{
			if (propertyName == AutoUSCTariff.Schema.UE_Tariff)
			{
				((ModuleTextFilter)ModuleFilters[USCTariff.FilterSchema.Tariff]).Property = code;
			}
			else
			{
				base.ApplyInitialCode(code, propertyName);
			}
		}

		ZQuery GetTariffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(USCTariffSchema.UE_Tariff, comparisonOperator, value.ExcludeChars(".").SubstringSafe(0, USCTariffSchema.UE_Tariff.MaxLength));
		}

		ZQuery GetSPIQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery(USCTariffSchema.UE_SPICode, comparisonOperator, value.PadRight(2, ' '));
			return result;
		}

		ZQuery GetPermitLicenseCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery(USCTariffSchema.UE_PermitLicenseIndicator, comparisonOperator, value);
			return result;
		}

		ZQuery GetPermitLicenseFlags(SchemaColumn schemaColumn, ZBool value)
		{
			var result = new ZQuery();
			result.AddToFilter(schemaColumn, value);
			return result;
		}

		ZQuery GetQuotaIndicatorQuery(ZBool show)
		{
			return GetPermitLicenseFlags(USCTariffSchema.UE_QuotaIndicator, show);
		}

		ZQuery GetAntiDumpingFlagQuery(ZBool show)
		{
			return GetPermitLicenseFlags(USCTariffSchema.UE_AntiDumping, show);
		}

		ZQuery GetCountervilingDutyFlag(ZBool show)
		{
			return GetPermitLicenseFlags(USCTariffSchema.UE_CountervailingDutyFlag, show);
		}

		void AddContainBlankOptions(ModuleTextFilter filters)
		{
			filters.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filters.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filters.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filters.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);

			filters.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
		}

		ZQuery GetDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var result = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				if (dateFrom.IsValid)
				{
					result.AddToFilter(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, dateFrom);
				}
				else
				{
					if (dateTo.IsValid)
					{
						result.AddToFilter(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, dateTo);
					}
				}

				if (dateTo.IsValid)
				{
					result.AddToFilter(JoinCondition.And, USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateTo);
				}
				else
				{
					if (dateFrom.IsValid)
					{
						result.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateFrom);
					}
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}
	}
}
