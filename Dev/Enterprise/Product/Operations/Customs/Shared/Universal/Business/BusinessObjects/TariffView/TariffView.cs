using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs;
using GetConditionApplicabilitiesByCriteriaProcedure = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.GetConditionApplicabilitiesByCriteria;
using GetRateSelectionCriteriaInfoProcedure = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.GetRateSelectionCriteriaInfo;
using RateCriteriaTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpRateSelectionCriteria_V2;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(TariffViewSchema.Constants.ZZ1_TariffCode), DescriptionProperty(TariffViewSchema.Constants.ZZ1_Description)]
	[RestrictedFilteredItem]
	public sealed class TariffView : AutoTariffView, ITariffData, ITariff, ITranslatableZZBusinessObject, IUnitListForRateFormulaEditProvider
	{
		public TariffView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTariffView.Schema
		{
			public const string ZZ1_ZZI_TariffTypeCode = "ZZ1_ZZI_TariffTypeCode";
			public const string ZZ1_EndDate_ForDisplay = "ZZ1_EndDate_ForDisplay";
			public const string ZZ1_ZZ8_UQ1 = "ZZ1_ZZ8_UQ1";
			public const string ZZ1_ZZ8_UQ2 = "ZZ1_ZZ8_UQ2";
			public const string ZZ1_ZZ8_UQ3 = "ZZ1_ZZ8_UQ3";
			public const string ZZ1_ZZ8_UQ4 = "ZZ1_ZZ8_UQ4";
			public const string ZZ1_ZZ8_UQ5 = "ZZ1_ZZ8_UQ5";
			public const string ZZ1_DataGroupingDescription = "ZZ1_DataGroupingDescription";
			public const string ZZ1_TariffTypeDescription = "ZZ1_TariffTypeDescription";
			public const string ZZ1_AlternateLanguageDescription = "ZZ1_AlternateLanguageDescription";

			public new const int ZZ1_TariffCodeMaxLength = 35;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoTariffView.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			/// <summary>
			/// The method is used to load the manual tariff from dbo.CusRefTariff and Tariff Version exists in CusRefTariffVersion only for manual Tariff.
			/// TariffView UNION tariff data from two data Sources, ZZ tariff and manual tariff.
			/// As to the field TariffViewSchema.ZZ1_ZZZ_NKDataGrouping,
			/// From ZZ.TariffView, it matchs ZZ1_ZZZ_NKDatagrouping which means Data grouping.
			/// From CW.CusRefTariff, it matchs CR1_RN_NKCountryCode which means country/Region code.
			/// So use countryCode as parameter name other than dataGrouping which is commonly used.
			/// </summary>
			/// <param name="countryCode"> </param>
			/// <param name="tariffType"></param>
			/// <param name="tariffCode"></param>
			/// <param name="version"></param>
			/// <returns>TariffView</returns>
			public TariffView LoadTariffByVersion(ZString countryCode, ZString tariffType, ZString tariffCode, ZString version, ZDateTime startDate)
			{
				TariffView result = null;
				if (!countryCode.IsEmpty && !tariffType.IsEmpty && !tariffCode.IsEmpty && startDate.IsValid)
				{
					var query = new ZQuery();
					query.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, countryCode);
					query.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, tariffType);
					query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffCode);
					query.AddToFilter(TariffViewSchema.ZZ1_CRT_NKTariffVersion, version);
					query.AddToFilter(TariffViewSchema.ZZ1_StartDate, startDate);
					query.AddToFilter(TariffViewSchema.ZZ1_IsSystem, false);
					result = Factory.LoadTop1<TariffView>(query);
				}
				return result;
			}

			public static ZQuery GetDuplicateManualTariffFilter(TariffView tariff, ZString type, ZString countryCode, ZString code, ZString version, ZDateTime startDate, ZDateTime endDate)
			{
				var uniqueFieldsFilter = GetManualTariffBaseFilter(tariff, type, countryCode, code, version);

				var dateFilter = new ZQuery();
				dateFilter.AddToFilter(JoinCondition.Or, TariffViewSchema.ZZ1_StartDate, startDate);
				dateFilter.AddToFilter(JoinCondition.Or, TariffViewSchema.ZZ1_EndDate, endDate);
				uniqueFieldsFilter.AddToFilter(dateFilter, JoinCondition.And);

				return uniqueFieldsFilter;
			}

			public static ZQuery GetDateOverlapManualTariffFilter(TariffView tariff, ZString type, ZString countryCode, ZString code, ZString version, ZDateTime startDate, ZDateTime endDate)
			{
				var dateOverlapFilter = GetManualTariffBaseFilter(tariff, type, countryCode, code, version);

				var startDateFilter = new ZQuery();
				startDateFilter.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, startDate);
				startDateFilter.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);

				var endDateFilter = new ZQuery();
				endDateFilter.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, endDate);
				endDateFilter.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

				var dateFilter = new ZQuery();
				dateFilter.AddToFilter(startDateFilter, JoinCondition.Or);
				dateFilter.AddToFilter(endDateFilter, JoinCondition.Or);

				dateOverlapFilter.AddToFilter(dateFilter, JoinCondition.And);
				return dateOverlapFilter;
			}

			static ZQuery GetManualTariffBaseFilter(TariffView tariff, ZString type, ZString countryCode, ZString code, ZString version)
			{
				var filter = new ZQuery(TariffViewSchema.ZZ1_ZZI_NKTariffType, type);
				filter.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, countryCode);
				filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, code);
				filter.AddToFilter(TariffViewSchema.ZZ1_CRT_NKTariffVersion, version);
				if (tariff != null)
				{
					filter.AddToFilter(TariffViewSchema.PK, SQLComparisonOperator.NotEqual, tariff.PK);
				}
				return filter;
			}

			public static ZQuery GetEffectiveTariffFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime valuationDate)
				=> GetEffectiveTariffFilter(factory, dataGroupingCode, tariffType, new[] { tariffCode }, valuationDate, SQLComparisonOperator.Equal);

			public static ZQuery GetEffectiveTariffFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZString[] tariffCodes, ZDateTime valuationDate, SQLComparisonOperator tariffComparisonOperator)
				=> new Loader(factory).GetEffectiveTariffFilter(dataGroupingCode, tariffType, tariffCodes, valuationDate.IsValid ? valuationDate : ZDateTime.Today, ZString.Empty, ZString.Empty, tariffComparisonOperator);

			public ZQuery GetEffectiveTariffFilter(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime valuationDate, ZString relatedTariffCode, ZString relatedTariffType)
				=> GetEffectiveTariffFilter(dataGroupingCode, tariffType, new[] { tariffCode }, valuationDate.IsValid ? valuationDate : ZDateTime.Today, relatedTariffCode, relatedTariffType, SQLComparisonOperator.Equal);

			ZQuery GetEffectiveTariffFilter(ZString dataGroupingCode, ZString tariffType, ZString[] tariffCodes, ZDateTime valuationDate, ZString relatedTariffCode, ZString relatedTariffType, SQLComparisonOperator tariffComparisonOperator)
			{
				var result = new ZQuery();

				tariffCodes = tariffCodes.Where(tariffCode => !tariffCode.IsEmpty).ToArray();
				var hasValidTariffCode = tariffCodes.Length > 0;
				var hasTariffType = !tariffType.IsEmpty;
				var tariffTypeIsHSN = tariffType == Constants.TariffTypes.HarmonizedSystem;
				var hasRelatedTariffCode = !relatedTariffCode.IsEmpty;
				var hasRelatedTariffType = !relatedTariffType.IsEmpty;
				var tariffTypePk = ZGuid.Empty;
				var relatedTariffTypePk = ZGuid.Empty;

				if (dataGroupingCode.IsEmpty
					|| !hasValidTariffCode && !hasRelatedTariffCode
					|| hasRelatedTariffType && !(relatedTariffTypePk = RefCusTariffType.Loader.Load(Factory, dataGroupingCode, relatedTariffType)?.PK ?? ZGuid.Empty).IsValid
					|| hasTariffType && !tariffTypeIsHSN && !(tariffTypePk = RefCusTariffType.Loader.Load(Factory, dataGroupingCode, tariffType)?.PK ?? ZGuid.Empty).IsValid)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					result.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(Factory, TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode));

					if (valuationDate.IsValid)
					{
						result.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
						result.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
						result.AddToFilter(GetEffectiveVersionQuery(valuationDate, dataGroupingCode));
					}

					if (hasValidTariffCode)
					{
						result.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffComparisonOperator, tariffCodes);
					}

					if (hasTariffType)
					{
						result.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, tariffType);
					}

					if (hasRelatedTariffCode || relatedTariffTypePk.IsValid)
					{
						var relatedTariffQuery = GetTariffRelationshipViewQuery(tariffTypePk, tariffCodes, relatedTariffCode, relatedTariffTypePk);
						var relatedTariffPks = Factory.Load<TariffRelationshipView>(relatedTariffQuery).Select(x => x.ZZH_ZZ1_LinkedTariffOrNationalCode);
						result.AddToFilter(TariffViewSchema.PK, relatedTariffPks);
					}
				}

				return result;
			}

			ZQuery GetEffectiveVersionQuery(ZDateTime valuationDate, ZString countryCode)
			{
				var version = CusRefTariffVersion.Loader.Load(Factory, countryCode, valuationDate)?.CRT_Version ?? ZString.Empty;
				var query = new ZQuery(TariffViewSchema.ZZ1_CRT_NKTariffVersion, version);
				return query;
			}

			public ZQuery GetTariffRelationshipViewQuery(ZString dataGroupingCode, ZString tariffType, ZString[] tariffCodes, ZString relatedTariffCode, ZString relatedTariffType)
			{
				var tariffTypePk = ZGuid.Empty;
				var relatedTariffTypePk = ZGuid.Empty;

				if (!tariffType.IsEmpty)
				{
					tariffTypePk = RefCusTariffType.Loader.Load(Factory, dataGroupingCode, tariffType)?.PK ?? ZGuid.Empty;
				}
				if (!relatedTariffType.IsEmpty)
				{
					relatedTariffTypePk = RefCusTariffType.Loader.Load(Factory, dataGroupingCode, relatedTariffType)?.PK ?? ZGuid.Empty;
				}

				return GetTariffRelationshipViewQuery(tariffTypePk, tariffCodes, relatedTariffCode, relatedTariffTypePk);
			}

			public static ZQuery GetTariffRelationshipViewQuery(ZGuid tariffType, ZString[] tariffCodes, ZString relatedTariffCode, ZGuid relatedTariffTypePk)
			{
				var relatedTariffQuery = new ZQuery();
				var hasRelatedTariffCode = !relatedTariffCode.IsEmpty;
				var hasRelatedTariffTypePk = relatedTariffTypePk.IsValid;

				if (!hasRelatedTariffTypePk && !hasRelatedTariffCode)
				{
					relatedTariffQuery.IsNoResultQuery = true;
				}
				else
				{
					if (hasRelatedTariffTypePk)
					{
						relatedTariffQuery.AddToFilter(TariffRelationshipViewSchema.ZZH_ZZI_TariffType, relatedTariffTypePk);
					}

					if (hasRelatedTariffCode)
					{
						relatedTariffQuery.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"@TariffCode LIKE {TariffRelationshipViewSchema.Constants.ZZH_TariffCode} + '%'"),
						new ZSqlParameterCollection(ZSqlParameter.New("@TariffCode", relatedTariffCode, TariffRelationshipViewSchema.ZZH_TariffCode)));
					}

					if (!tariffType.IsEmpty)
					{
						relatedTariffQuery.AddToFilter(TariffRelationshipViewSchema.ZZH_ZZI_RelatedTariffType, tariffType);
					}

					if (tariffCodes.Length > 0)
					{
						relatedTariffQuery.AddToFilter(TariffRelationshipViewSchema.ZZH_RelatedTariffCode, tariffCodes);
					}
				}
				return relatedTariffQuery;
			}

			public TariffView LoadMostRecentCachedTariffWithAttribute(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime valuationDate, ZString attributeName, ZString attributeValue, string relatedTariffCode = null)
			{
				TariffView result = null;
				if (!dataGroupingCode.IsEmpty && !tariffType.IsEmpty && !tariffCode.IsEmpty)
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					result = Factory.GetCachedValue(string.Join("_", "LoadMostRecentCachedTariff", dataGroupingCode, tariffType, tariffCode, effectiveValuationDate, relatedTariffCode, attributeName, attributeValue), () =>
					{
						var query = GetEffectiveTariffFilter(dataGroupingCode, tariffType, tariffCode, effectiveValuationDate, relatedTariffCode, ZString.Empty);
						var tariffs = Factory.Load<TariffView>(query).ToList();
						var tariffsWithAttribute = tariffs.Where(x => x.Attributes.Any(a => a.ZZ3_Name.Equals(attributeName) && a.ZZ3_Value.Equals(attributeValue)));

						return (tariffsWithAttribute.Any() ? tariffsWithAttribute : tariffs).OrderByDescending(x => x.ZZ1_StartDate).FirstOrDefault();
					});
				}

				return result;
			}

			public TariffView LoadMostRecentCachedTariff(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime valuationDate, string relatedTariffCode = null)
			{
				TariffView result = null;
				if (!dataGroupingCode.IsEmpty && !tariffType.IsEmpty && !tariffCode.IsEmpty)
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					result = Factory.GetCachedValue(string.Join("_", "LoadMostRecentCachedTariff", dataGroupingCode, tariffType, tariffCode, effectiveValuationDate, relatedTariffCode), () =>
					{
						var query = GetEffectiveTariffFilter(dataGroupingCode, tariffType, tariffCode, effectiveValuationDate, relatedTariffCode, ZString.Empty);
						return Factory.Load<TariffView>(query).OrderByDescending(x => x.ZZ1_StartDate).FirstOrDefault();
					});
				}

				return result;
			}

			public TariffView LoadMostRecentCachedTariff(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime valuationDate, SQLComparisonOperator tariffCodeComparisonOperator)
			{
				TariffView result = null;
				if (!dataGroupingCode.IsEmpty && !tariffType.IsEmpty && !tariffCode.IsEmpty)
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					result = Factory.GetCachedValue(string.Join("_", "LoadMostRecentCachedTariff", dataGroupingCode, tariffType, tariffCode, effectiveValuationDate, tariffCodeComparisonOperator), () =>
					{
						var query = GetEffectiveTariffFilter(
							dataGroupingCode: dataGroupingCode,
							tariffType: tariffType,
							tariffCodes: new[] { tariffCode },
							valuationDate: effectiveValuationDate,
							relatedTariffCode: null,
							relatedTariffType: ZString.Empty,
							tariffComparisonOperator: tariffCodeComparisonOperator
						);
						return Factory.Load<TariffView>(query).OrderByDescending(x => x.ZZ1_StartDate).FirstOrDefault();
					});
				}

				return result;
			}

			public TariffView LoadMostRecentCachedTariff(ZString dataGroupingCode, ZString tariffCode, ZDateTime valuationDate, string relatedTariffCode = null)
			{
				TariffView result = null;
				if (!dataGroupingCode.IsEmpty && !tariffCode.IsEmpty)
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					result = Factory.GetCachedValue(string.Join("_", "LoadMostRecentCachedTariff", dataGroupingCode, tariffCode, effectiveValuationDate, relatedTariffCode), () =>
					{
						var query = GetEffectiveTariffFilter(dataGroupingCode, null, tariffCode, effectiveValuationDate, relatedTariffCode, ZString.Empty);
						return Factory.Load<TariffView>(query).OrderByDescending(x => x.ZZ1_StartDate).FirstOrDefault();
					});
				}

				return result;
			}

			public TariffView LoadLatestCachedTariff(ZString dataGroupingCode, ZString tariffType, ZString tariffCode)
			{
				TariffView result = null;
				if (!dataGroupingCode.IsEmpty && !tariffType.IsEmpty && !tariffCode.IsEmpty)
				{
					result = Factory.GetCachedValue(string.Join("_", "LoadLatestCachedTariff", dataGroupingCode, tariffType, tariffCode), () =>
					{
						var query = GetEffectiveTariffFilter(dataGroupingCode, tariffType, new[] { tariffCode }, ZDateTime.Empty, ZString.Empty, ZString.Empty, SQLComparisonOperator.Equal);
						return Factory.Load<TariffView>(query).OrderByDescending(x => x.ZZ1_StartDate).FirstOrDefault();
					});
				}

				return result;
			}

			public TariffView[] GetEffectiveChildTariffs(ZString dataGroupingCode, ZString parentTariffType, ZString parentTariffCode, ZDateTime valuationDate, string childTariffType = null)
			{
				TariffView[] result = null;
				if (dataGroupingCode.IsEmpty || parentTariffType.IsEmpty || parentTariffCode.IsEmpty)
				{
					result = Factory.Load<TariffView>(ZQuery.NoResultQuery);
				}
				else
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					result = Factory.GetCachedValue(string.Join("_", "GetEffectiveChildTariffs", dataGroupingCode, parentTariffType, parentTariffCode, effectiveValuationDate, childTariffType), () =>
					{
						var query = GetEffectiveTariffFilter(dataGroupingCode, childTariffType, ZString.Empty, effectiveValuationDate, parentTariffCode, parentTariffType);
						return Factory.Load<TariffView>(query);
					});
				}

				return result;
			}

			public bool Exists(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime? valuationDate = null)
			{
				return (!valuationDate.HasValue || valuationDate.Value.IsValid) && Factory.GetCachedValue(string.Join("_", "TariffViewExists", dataGroupingCode, tariffType, tariffCode, valuationDate), () =>
				{
					var query = new ZDBOnlyQuery(typeof(TariffView));
					query.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, tariffType);
					query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffCode);
					query.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode);
					if (valuationDate.HasValue && valuationDate.Value.IsValid)
					{
						query.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
						query.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
						query.AddToFilter(GetEffectiveVersionQuery(valuationDate.Value, dataGroupingCode));
					}
					return Factory.Exists(typeof(TariffView), query);
				});
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(TariffView);
		}

		public static string GetEffectiveTableType(BusinessObjectFactory factory, ZString dataGrouping, ZString tariffType, ZDate effectiveDate)
		{
			var key = string.Join("_", "UseNationalCode", dataGrouping, tariffType, effectiveDate.ToShortDateString());
			return factory.GetCachedValue(key, () =>
			{
				var collection = new DynamicBusinessObjectCollection(factory);
				var script = $@"
SELECT CASE WHEN EXISTS(SELECT NULL
FROM {RefCusTariffNationalCodeSchema.Constants.TableName}
WHERE {RefCusTariffNationalCodeSchema.Constants.ZZW_ZZZ_NKDataGrouping} = @DataGrouping AND {RefCusTariffNationalCodeSchema.Constants.ZZW_StartDate} <= @EffectiveDate AND {RefCusTariffNationalCodeSchema.Constants.ZZW_EndDate} >= @EffectiveDate
AND EXISTS(SELECT NULL
			FROM {RefCusTariffSchema.Constants.TableName}
			WHERE {RefCusTariffSchema.Constants.PK} = {RefCusTariffNationalCodeSchema.Constants.ZZW_ZZ1_Tariff}
			AND EXISTS(SELECT NULL
						FROM {RefCusTariffTypeSchema.Constants.TableName}
						WHERE {RefCusTariffTypeSchema.Constants.ZZI_TariffType} = @TariffType AND {RefCusTariffTypeSchema.Constants.PK} = {RefCusTariffSchema.Constants.ZZ1_ZZI_TariffType})))
THEN '{RefCusTariffNationalCodeSchema.Constants.Prefix}' ELSE '{RefCusTariffSchema.Constants.Prefix}' END {TariffViewSchema.Constants.ZZ1_TableType}
";
				var parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New("@TariffType", tariffType, RefCusTariffTypeSchema.ZZI_TariffType));
				parameters.Add(ZSqlParameter.New("@DataGrouping", dataGrouping, RefCusTariffNationalCodeSchema.ZZW_ZZZ_NKDataGrouping));
				parameters.Add(ZSqlParameter.New("@EffectiveDate", effectiveDate, RefCusTariffNationalCodeSchema.ZZW_StartDate));
				collection.Load(script, parameters);
				return collection.Count == 1 ? collection[0][TariffViewSchema.Constants.ZZ1_TableType].ToString() : RefCusTariffSchema.Constants.Prefix;
			});
		}

		public TariffViewWrapper Wrapper => wrapper ?? (wrapper = new TariffViewWrapper(this));
		TariffViewWrapper wrapper;

		public void SetupDefaultFilterDataIfNeeded(ITariffViewFilterData data)
		{
			if (!hasSetupDefaultFilterData && data != null)
			{
				hasSetupDefaultFilterData = true;
				Wrapper.RatesApplyToCountry = data.RatesApplyToCountry;
				if (data.EffectiveDate is { IsValid: true } effectiveDate)
				{
					Wrapper.EffectiveDate = effectiveDate;
				}
			}
		}
		bool hasSetupDefaultFilterData;

		#region Override Properties

		protected override ZString HumanReadableNameCore => Res.GetString("2219C790-FD72-4239-8962-F1271B3DB2BE", "Tariff: {0}/{1}/{2} - Effective From: {3}", ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_NKTariffType, ZZ1_TariffCode, StartDate);

		protected override ZString HumanReadableShortcutNameCore => Res.GetString("32B8F255-5009-4BD3-9E2A-8EAEA6A74633", "{0}/{1}/{2} - From: {3}", ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_NKTariffType, ZZ1_TariffCode, StartDate);

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => !ZZ1_IsSystem;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ZZ1_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			ZZ1_TableType = CusRefTariffSchema.Constants.Prefix;
			ZZ1_ZZ1_Tariff = PK;
			ZZ1_StartDate = ZDateTime.Today;
			ZZ1_EndDate = ZDateTime.MaxSmallDateTime;
			ZZ1_ZZI_NKTariffType = Constants.TariffTypes.HarmonizedSystem;
			ZZ1_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		ZString StartDate
		{
			get
			{
				var startDate = ZZ1_StartDate.ToShortDateString();
				if (DateTimeFormat == ZDateTimePickerFormat.Long)
				{
					startDate += " " + ZZ1_StartDate.ToShortTimeString();
				}
				return startDate;
			}
		}

		public ZDateTimePickerFormat DateTimeFormat
		{
			get
			{
				if (fDateTimeFormat == null)
				{
					var provider = Enterprise.Customs.Universal.UniversalReferenceBusinessProvider.GetProvider(Factory, ZZ1_ZZZ_NKDataGrouping);
					fDateTimeFormat = provider?.TariffDateTimeFormat ?? ZDateTimePickerFormat.Short;
				}
				return fDateTimeFormat.Value;
			}
		}
		ZDateTimePickerFormat? fDateTimeFormat;

		protected override AutologState AutoLoggingState => ZZ1_IsSystem ? AutologState.NotLogged : AutologState.AutoLogged;

		[RelatedBusinessObject("CusTariffType")]
		[List(nameof(Lookups) + "." + nameof(TariffViewLookups.TariffTypeList))]
		public override ZGuid ZZ1_ZZI_TariffType
		{
			get { return base.ZZ1_ZZI_TariffType; }
			set
			{
				var oldValue = ZZ1_ZZI_TariffType;
				base.ZZ1_ZZI_TariffType = value;
				if (!IsCopying && oldValue != ZZ1_ZZI_TariffType)
				{
					cusTariffType = null;
				}
			}
		}

		[MaxLength(TariffView.Schema.ZZ1_DescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_Description", Caption = "Description (Default Language)")]
		public override ZString ZZ1_Description
		{
			get => base.ZZ1_Description;
			set => base.ZZ1_Description = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_AlternateLanguageDescription", Caption = "Description (Alternate Language)")]
		public ZString ZZ1_AlternateLanguageDescription
		{
			get => TranslationHelper.GetAlternateLanguageDescription(this, CusRefTariffLanguageViewSchema.ZX7_Description);
		}

		public ZPropertyInfo ZZ1_AlternateLanguageDescriptionInfo => GetZPropertyInfo(Schema.ZZ1_AlternateLanguageDescription);

		[MaxLength(TariffView.Schema.ZZ1_TariffCodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_TariffCode", Caption = "Tariff Code", MediumCaption = "Tariff", ShortCaption = "Code")]
		public override ZString ZZ1_TariffCode { get => base.ZZ1_TariffCode; set => base.ZZ1_TariffCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_EndDate", Caption = "Effective To")]
		public override ZDateTime ZZ1_EndDate { get => base.ZZ1_EndDate; set => base.ZZ1_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_PublishedDate", Caption = "Published Date")]
		public override ZDate ZZ1_PublishedDate { get => base.ZZ1_PublishedDate; set => base.ZZ1_PublishedDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_StartDate", Caption = "Effective From")]
		public override ZDateTime ZZ1_StartDate { get => base.ZZ1_StartDate; set => base.ZZ1_StartDate = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(TariffViewLookups.CountryCodeList))]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping", MediumCaption = "Country/Region", ShortCaption = "Ctry./Rgn.")]
		public override ZString ZZ1_ZZZ_NKDataGrouping
		{
			get => base.ZZ1_ZZZ_NKDataGrouping;
			set
			{
				var oldValue = ZZ1_ZZZ_NKDataGrouping;
				base.ZZ1_ZZZ_NKDataGrouping = value;
				if (!IsCopying && oldValue != ZZ1_ZZZ_NKDataGrouping)
				{
					isParentDataGroupingCaching = null;
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_IsSystem", Caption = "Is System Defined")]
		public override ZBool ZZ1_IsSystem { get => base.ZZ1_IsSystem; set => base.ZZ1_IsSystem = value; }

		[ReadOnly(true)]
		public override ZString ZZ1_DataSet { get => base.ZZ1_DataSet; set => base.ZZ1_DataSet = value; }

		[List(nameof(Lookups) + "." + nameof(TariffViewLookups.TariffTypeList))]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZI_NKTariffType", Caption = "Type")]
		public override ZString ZZ1_ZZI_NKTariffType
		{
			get => base.ZZ1_ZZI_NKTariffType;
			set => base.ZZ1_ZZI_NKTariffType = value;
		}

		[MaxLength(TariffView.Schema.ZZ1_CRT_NKTariffVersionMaxLength)]
		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_CRT_NKTariffVersion", Caption = "Tariff Version")]
		[List(nameof(Lookups) + "." + nameof(TariffViewLookups.TariffVersionList))]
		public override ZString ZZ1_CRT_NKTariffVersion { get => base.ZZ1_CRT_NKTariffVersion; set => base.ZZ1_CRT_NKTariffVersion = value; }

		#endregion

		#region Related BusinessObjects

		public RefCusTariffType CusTariffType => cusTariffType ?? (cusTariffType = LoadTairffType());
		RefCusTariffType cusTariffType;

		RefCusTariffType LoadTairffType()
		{
			RefCusTariffType result;
			if (ZZ1_IsSystem && ZZ1_DataSet == Core.Constants.Customs.Universal.DataSetTypes.WTGData)
			{
				result = Factory.Load<RefCusTariffType>(ZZ1_ZZI_TariffType);
			}
			else
			{
				result = RefCusTariffType.Loader.Load(Factory, ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_NKTariffType);
			}
			return result;
		}

		public RefDataGrouping DataGrouping => refDataGrouping ?? (refDataGrouping = Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZ1_ZZZ_NKDataGrouping));
		RefDataGrouping refDataGrouping;

		#endregion

		#region New Properties

		public bool IsParentDataGrouping
		{
			get
			{
				if (!isParentDataGroupingCaching.HasValue)
				{
					isParentDataGroupingCaching = DataGrouping?.DataGroupingMembers.Any() ?? false;
				}
				return isParentDataGroupingCaching.Value;
			}
		}
		bool? isParentDataGroupingCaching;

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZ8_UQ1", Caption = "Unit of Quantity 1", ShortCaption = "UQ1")]
		public ZString ZZ1_ZZ8_UQ1
		{
			get
			{
				if (zZ1_ZZ8_UQ1Cached == null)
				{
					zZ1_ZZ8_UQ1Cached = new CachedProperty<ZString>(Factory, () =>
					{
						return UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.StatisticalUOMType)?.ZZ8_UOM ?? ZString.Empty;
					});
				}
				return zZ1_ZZ8_UQ1Cached.Value;
			}
		}
		CachedProperty<ZString> zZ1_ZZ8_UQ1Cached;

		public ZPropertyInfo ZZ1_ZZ8_UQ1Info => GetZPropertyInfo(Schema.ZZ1_ZZ8_UQ1);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZ8_UQ2", Caption = "Unit of Quantity 2", ShortCaption = "UQ2")]
		public ZString ZZ1_ZZ8_UQ2
		{
			get
			{
				if (zZ1_ZZ8_UQ2Cached == null)
				{
					zZ1_ZZ8_UQ2Cached = new CachedProperty<ZString>(Factory, () =>
					{
						return UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType)?.ZZ8_UOM ?? ZString.Empty;
					});
				}
				return zZ1_ZZ8_UQ2Cached.Value;
			}
		}
		CachedProperty<ZString> zZ1_ZZ8_UQ2Cached;

		public ZPropertyInfo ZZ1_ZZ8_UQ2Info => GetZPropertyInfo(Schema.ZZ1_ZZ8_UQ2);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZ8_UQ3", Caption = "Unit of Quantity 3", ShortCaption = "UQ3")]
		public ZString ZZ1_ZZ8_UQ3
		{
			get
			{
				if (zZ1_ZZ8_UQ3Cached == null)
				{
					zZ1_ZZ8_UQ3Cached = new CachedProperty<ZString>(Factory, () =>
					{
						return UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.CustomsUOM3Type)?.ZZ8_UOM ?? ZString.Empty;
					});
				}
				return zZ1_ZZ8_UQ3Cached.Value;
			}
		}
		CachedProperty<ZString> zZ1_ZZ8_UQ3Cached;

		public ZPropertyInfo ZZ1_ZZ8_UQ3Info => GetZPropertyInfo(Schema.ZZ1_ZZ8_UQ3);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZ8_UQ4", Caption = "Unit of Quantity 4", ShortCaption = "UQ4")]
		public ZString ZZ1_ZZ8_UQ4
		{
			get
			{
				if (zZ1_ZZ8_UQ4Cached == null)
				{
					zZ1_ZZ8_UQ4Cached = new CachedProperty<ZString>(Factory, () =>
					{
						return UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.CustomsUOM4Type)?.ZZ8_UOM ?? ZString.Empty;
					});
				}
				return zZ1_ZZ8_UQ4Cached.Value;
			}
		}
		CachedProperty<ZString> zZ1_ZZ8_UQ4Cached;

		public ZPropertyInfo ZZ1_ZZ8_UQ4Info => GetZPropertyInfo(Schema.ZZ1_ZZ8_UQ4);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZ8_UQ5", Caption = "Unit of Quantity 5", ShortCaption = "UQ5")]
		public ZString ZZ1_ZZ8_UQ5
		{
			get
			{
				if (zZ1_ZZ8_UQ5Cached == null)
				{
					zZ1_ZZ8_UQ5Cached = new CachedProperty<ZString>(Factory, () =>
					{
						return UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.CustomsUOM5Type)?.ZZ8_UOM ?? ZString.Empty;
					});
				}
				return zZ1_ZZ8_UQ5Cached.Value;
			}
		}
		CachedProperty<ZString> zZ1_ZZ8_UQ5Cached;

		public ZPropertyInfo ZZ1_ZZ8_UQ5Info => GetZPropertyInfo(Schema.ZZ1_ZZ8_UQ5);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_EndDate_ForDisplay", Caption = "Effective To")]
		public ZDateTime ZZ1_EndDate_ForDisplay => ZZ1_EndDate == ZDateTime.MaxSmallDateTime ? ZDateTime.Empty : ZZ1_EndDate;

		public ZPropertyInfo ZZ1_EndDate_ForDisplayInfo => GetZPropertyInfo(Schema.ZZ1_EndDate_ForDisplay);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZI_TariffTypeCode", Caption = "Tariff Type")]
		public ZString ZZ1_ZZI_TariffTypeCode => CusTariffType?.ZZI_TariffType ?? ZString.Empty;

		public ZPropertyInfo ZZ1_ZZI_TariffTypeCodeInfo => GetZPropertyInfo(Schema.ZZ1_ZZI_TariffTypeCode);

		TariffPreferredLanguageManager TariffPreferredLanguageManager => tariffPreferredLanguageManager ?? (tariffPreferredLanguageManager = new TariffPreferredLanguageManager());
		TariffPreferredLanguageManager tariffPreferredLanguageManager;

		public ZString FullTariffDescription(ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading) => FullTariffDescription(date, includeSectionHeadings, includeChapterHeading, false);

		public ZString FullTariffDescription(ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading, bool useTariffPreferredLanguage, string countryPreferedLanguage = null)
		{
			var nomenclatureGroupType = CusTariffType?.ZZI_ZZ9_NKNomenclatureGroupType ?? ZString.Empty;
			if (!nomenclatureGroupType.IsEmpty)
			{
				var language = string.IsNullOrEmpty(countryPreferedLanguage) ? GetTariffDescriptionTargetLanguage(useTariffPreferredLanguage) : new ZString(countryPreferedLanguage);
				var cacheKey = FormattableString.Invariant($"RefCusTariff_FullTariffDescription_{PK.ToStringKey()}_{date.ToShortDateString()}_{language}_{includeSectionHeadings}_{includeChapterHeading}_{useTariffPreferredLanguage}");

				return Factory.GetCachedValue(cacheKey, () =>
				{
					ZString fullDescription = ZString.Empty;
					if (useTariffPreferredLanguage)
					{
						fullDescription = FullTariffDescriptionPreferredLanguage(nomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading, TranslationHelper.GetLanguageCode(language));
					}

					if (fullDescription.IsEmpty)
					{
						fullDescription = FullTariffDescriptionDefaultLanguage(nomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading);
					}
					return fullDescription;
				});
			}
			return ZZ1_Description;
		}

		ZString GetTariffDescriptionTargetLanguage(bool useTariffPreferredLanguage) => useTariffPreferredLanguage ? GetTariffDescriptionPreferredLanguage() : TranslationHelper.GetCurrentLanguageCode();

		ZString GetTariffDescriptionPreferredLanguage() => TariffPreferredLanguageManager.IsDefaultLanguage ? GlbStaff.CurrentUser.Language : TariffPreferredLanguageManager.Language;

		ZString FullTariffDescriptionPreferredLanguage(ZString nomenclatureGroupType, ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading, ZString preferredLanguageCode)
		{
			var description = RefCusNomenclatureLanguage.GetFullDescriptionForCompositeKey(Factory, ZZ1_CompositeKeyOnZZ5, ZZ1_ZZZ_NKDataGrouping, nomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading, preferredLanguageCode);
			if (!description.IsEmpty)
			{
				var tariffDescription = ZZ1_TableType == RefCusTariffNationalCodeSchema.Constants.Prefix
					? ZZ1_Description
					: TranslationHelper.GetAlternateLanguageDescription(this, CusRefTariffLanguageViewSchema.ZX7_Description, preferredLanguageCode);

				if (tariffDescription != Constants.RefCusTariffFilters.NotAvailable)
				{
					description = (description + " " + tariffDescription).Trim();
				}
				else
				{
					description = ZString.Empty;
				}
			}
			return description;
		}

		ZString FullTariffDescriptionDefaultLanguage(ZString nomenclatureGroupType, ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading)
		{
			var description = RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(Factory, ZZ1_CompositeKeyOnZZ5, ZZ1_ZZZ_NKDataGrouping, nomenclatureGroupType, date, includeSectionHeadings, includeChapterHeading);
			return (description + " " + ZZ1_Description).Trim();
		}

		public bool IsTariffNationalCode => ZZ1_TableType == RefCusTariffNationalCodeSchema.Constants.Prefix;

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_DataGroupingDescription", Caption = "Country/Region or Grouping Desc.", ShortCaption = "Description")]
		public ZString ZZ1_DataGroupingDescription
		{
			get
			{
				if (zZ1_DataGroupingDescriptionCached == null)
				{
					zZ1_DataGroupingDescriptionCached = new CachedProperty<ZString>(Factory, () => DataGrouping?.ZZZ_Description ?? ZString.Empty);
				}
				return zZ1_DataGroupingDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> zZ1_DataGroupingDescriptionCached;

		public ZPropertyInfo ZZ1_DataGroupingDescriptionInfo => GetZPropertyInfo(Schema.ZZ1_DataGroupingDescription);

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_TariffTypeDescription", Caption = "Tariff Type Description", ShortCaption = "Description")]
		public ZString ZZ1_TariffTypeDescription
		{
			get
			{
				if (zZ1_TariffTypeDescriptionCached == null)
				{
					zZ1_TariffTypeDescriptionCached = new CachedProperty<ZString>(Factory, () =>
						!ZZ1_IsSystem && ZZ1_ZZI_NKTariffType == Constants.TariffTypes.HarmonizedSystem
							? (ZString)Constants.TariffTypes.HarmonizedSystemDescription
							: CusTariffType?.ZZI_Description ?? ZString.Empty);
				}
				return zZ1_TariffTypeDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> zZ1_TariffTypeDescriptionCached;

		public ZPropertyInfo ZZ1_TariffTypeDescriptionInfo => GetZPropertyInfo(Schema.ZZ1_TariffTypeDescription);

		#endregion

		#region Collections

		[ChildEditable]
		public TariffAdditionalCodeViewCollection AdditionalCodes
		{
			get
			{
				if (additionalCodes == null)
				{
					additionalCodes = new TariffAdditionalCodeViewCollection(this);
					RegisterEditableChildObject(additionalCodes);
				}
				return additionalCodes;
			}
		}
		TariffAdditionalCodeViewCollection additionalCodes;

		[ChildEditable]
		public FilteredTariffAdditionalCodeViewCollection FilteredAdditionalCodes
		{
			get
			{
				if (filteredAdditionalCodes == null)
				{
					filteredAdditionalCodes = new FilteredTariffAdditionalCodeViewCollection(this, IsParentDataGrouping);
					RegisterEditableChildObject(filteredAdditionalCodes);
				}
				return filteredAdditionalCodes;
			}
		}
		FilteredTariffAdditionalCodeViewCollection filteredAdditionalCodes;

		[ChildEditable]
		public RefCusConditionCollection Conditions
		{
			get
			{
				if (conditions == null)
				{
					conditions = new RefCusConditionCollection(this);
					RegisterEditableChildObject(conditions);
				}
				return conditions;
			}
		}
		RefCusConditionCollection conditions;

		[ChildEditable]
		public FilteredRefCusConditionCollection FilteredConditions
		{
			get
			{
				if (filteredConditions == null)
				{
					filteredConditions = new FilteredRefCusConditionCollection(this, IsParentDataGrouping);
					RegisterEditableChildObject(filteredConditions);
				}
				return filteredConditions;
			}
		}
		FilteredRefCusConditionCollection filteredConditions;

		[ChildEditable]
		public VATApplicabilityViewCollection VATApplicabilities
		{
			get
			{
				if (vatApplicabilities == null)
				{
					vatApplicabilities = new VATApplicabilityViewCollection(this);
					RegisterEditableChildObject(vatApplicabilities);
				}
				return vatApplicabilities;
			}
		}
		VATApplicabilityViewCollection vatApplicabilities;

		[ChildEditable]
		public FilteredVATApplicabilityViewCollection FilteredVATApplicabilities
		{
			get
			{
				if (filteredVATApplicabilities == null)
				{
					filteredVATApplicabilities = new FilteredVATApplicabilityViewCollection(this, IsParentDataGrouping);
					RegisterEditableChildObject(filteredVATApplicabilities);
				}
				return filteredVATApplicabilities;
			}
		}
		FilteredVATApplicabilityViewCollection filteredVATApplicabilities;

		[ChildEditable]
		public TariffUOMViewCollection UnitsOfMeasure
		{
			get
			{
				if (unitsOfMeasure == null)
				{
					unitsOfMeasure = new TariffUOMViewCollection(this);
					RegisterEditableChildObject(unitsOfMeasure);
				}
				return unitsOfMeasure;
			}
		}
		TariffUOMViewCollection unitsOfMeasure;

		[ChildEditable]
		public FilteredTariffUOMViewCollection FilteredUnitsOfMeasure
		{
			get
			{
				if (filteredUnitsOfMeasure == null)
				{
					filteredUnitsOfMeasure = new FilteredTariffUOMViewCollection(this, IsParentDataGrouping);
					RegisterEditableChildObject(filteredUnitsOfMeasure);
				}
				return filteredUnitsOfMeasure;
			}
		}
		FilteredTariffUOMViewCollection filteredUnitsOfMeasure;

		[ChildEditable]
		public TariffAttributeViewCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new TariffAttributeViewCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		TariffAttributeViewCollection attributes;

		[ChildEditable]
		public RateViewCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new RateViewCollection(this);
					RegisterEditableChildObject(rates);
				}
				return rates;
			}
		}
		RateViewCollection rates;

		[ChildEditable]
		public FilteredRateViewCollection FilteredRates
		{
			get
			{
				if (filteredRates == null)
				{
					filteredRates = new FilteredRateViewCollection(this, IsParentDataGrouping);
					RegisterEditableChildObject(filteredRates);
				}
				return filteredRates;
			}
		}
		FilteredRateViewCollection filteredRates;

		[ChildEditable]
		public TariffRelationshipViewCollection RelatedTariffs
		{
			get
			{
				if (relatedTariffs == null)
				{
					relatedTariffs = new TariffRelationshipViewCollection(this);
					RegisterEditableChildObject(relatedTariffs);
				}
				return relatedTariffs;
			}
		}
		TariffRelationshipViewCollection relatedTariffs;

		public TariffRelationshipViewCollection ChildTariffs
		{
			get
			{
				if (childTariffs == null)
				{
					childTariffs = TariffRelationshipViewCollection.NewChildTariffRelationshipCollection(this);
					childTariffs.ExcludeGeneralTariffs = true;
				}
				return childTariffs;
			}
		}
		TariffRelationshipViewCollection childTariffs;

		public FilteredTariffRelationshipViewCollection FilteredChildTariffs
		{
			get
			{
				if (filteredChildTariffs == null)
				{
					filteredChildTariffs = FilteredTariffRelationshipViewCollection.NewChildTariffRelationshipCollection(this);
					filteredChildTariffs.ExcludeGeneralTariffs = true;
				}
				return filteredChildTariffs;
			}
		}
		FilteredTariffRelationshipViewCollection filteredChildTariffs;

		[ChildEditable]
		public CusRefTariffLanguageViewCollection TariffLanguages
		{
			get
			{
				if (tariffLanguages == null)
				{
					tariffLanguages = new CusRefTariffLanguageViewCollection(this);
					RegisterEditableChildObject(tariffLanguages);
				}
				return tariffLanguages;
			}
		}
		CusRefTariffLanguageViewCollection tariffLanguages;

		#endregion

		#region Override Method

		public override void Delete()
		{
			Attributes.DeleteAll();
			UnitsOfMeasure.DeleteAll();
			Rates.DeleteAll();
			RelatedTariffs.DeleteAll();
			TariffLanguages.DeleteAll();
			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : TranslatableZZBusinessObjectFetchStrategy<TariffView>
		{
			public Strategy(TariffView tariffView)
				: base(tariffView)
			{
			}

			protected new TariffView BusinessObject => (TariffView)base.BusinessObject;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				if (columns.Any(x => x.ColumnName == Schema.ZZ1_ZZ8_UQ1 || x.ColumnName == Schema.ZZ1_ZZ8_UQ2 || x.ColumnName == Schema.ZZ1_ZZ8_UQ3 || x.ColumnName == Schema.ZZ1_ZZ8_UQ4 || x.ColumnName == Schema.ZZ1_ZZ8_UQ5))
				{
					Factory.AddFetchHint(TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode, BusinessObject.PK);
				}

				if (columns.Any(x => x.ColumnName == Schema.ZZ1_AlternateLanguageDescription))
				{
					Factory.AddFetchHint(BusinessObject.LanguageTableType, TranslationHelper.GetAlternateLanguageQuery(BusinessObject, BusinessObject.LanguageTableSchema, CurrentLanguageCode, CurrentCountryLanguageCode));
				}
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(RefCusTariffTypeSchema.PK, BusinessObject.ZZ1_ZZI_TariffType); // Tariff type is accessed just after load
				Factory.AddFetchHint(BusinessObject.LanguageTableType, TranslationHelper.GetAlternateLanguageQuery(BusinessObject, BusinessObject.LanguageTableSchema, CurrentLanguageCode, CurrentCountryLanguageCode));
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, BusinessObject.PK);
				Factory.AddFetchHint(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, BusinessObject.PK);
				Factory.AddFetchHint(TariffRelationshipViewSchema.ZZH_ZZ1_LinkedTariffOrNationalCode, BusinessObject.PK);
				Factory.AddFetchHint(TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode, BusinessObject.PK);
				Factory.AddFetchHint(VATApplicabilityViewSchema.ZX5_ZZ1_ParentTariffOrNationalCode, BusinessObject.PK);
				Factory.AddFetchHint(RefCusConditionSchema.ZX1_ZZ1_Tariff, BusinessObject.PK);
			}

			ZString CurrentLanguageCode => TranslationHelper.GetCurrentLanguageCode();

			ZString CurrentCountryLanguageCode => TranslationHelper.GetCurrentCountryLanguageCode();
		}

		#endregion

		public bool MatchDataGrouping(ZString dataGrouping)
		{
			return dataGrouping.IsEmpty || dataGrouping == ZZ1_ZZZ_NKDataGrouping || (DataGrouping?.DataGroupingMembers.Any(x => x.ZZZ_DataGrouping == dataGrouping) ?? false);
		}

		#region Unit Of Measure Related

		public ZString GetSpecificUOM(ZString type) => UnitsOfMeasure?.FirstOrDefault(a => a.ZZ8_Type == type)?.ZZ8_UOM ?? ZString.Empty;

		#endregion

		#region Rates Related

		public RateView GetApplicableRate(IZZRateSelectionCriteria criteria)
		{
			return GetApplicableRates(criteria)
				.OrderByDescending(x => x.ZZ2_StartDate)
				.ThenBy(x => x.PreferenceCode)
				.FirstOrDefault();
		}

		public IEnumerable<RateView> GetApplicableRates(IZZRateSelectionCriteria criteria)
		{
			IEnumerable<RateView> result;

			if (criteria != null)
			{
				var rateLoader = new ApplicableRateLoader(Factory);
				var tariffAndCriteriaSet = new RateLoadTariffCriteriaSet(this, criteria);
				result = rateLoader.LoadRatesForSingleCriteriaSet(tariffAndCriteriaSet);
			}
			else
			{
				result = Array.Empty<RateView>();
			}

			return result;
		}
		public IEnumerable<ZString> GetAdditionalCodesForAntiDumping(string countryOfOrigin, ZDateTime effectiveDate)
		{
			return GetAdditionalCodesForRateType(Constants.RateTypes.AntiDumping, countryOfOrigin, effectiveDate);
		}

		public IEnumerable<ZString> GetAdditionalCodesForCountervailing(string countryOfOrigin, ZDateTime effectiveDate)
		{
			return GetAdditionalCodesForRateType(Constants.RateTypes.Countervailing, countryOfOrigin, effectiveDate);
		}

		public IEnumerable<ZString> GetAdditionalCodesForDuty(string countryOfOrigin, ZDateTime effectiveDate, string dataGrouping, string primaryPreference)
		{
			return GetAdditionalCodesForRateType(Constants.RateTypes.Duty, countryOfOrigin, effectiveDate, dataGrouping, primaryPreference);
		}

		IEnumerable<ZString> GetAdditionalCodesForRateType(string rateTypeCode, string countryOfOrigin, ZDateTime effectiveDate, string dataGrouping = "", string primaryPreference = "")
		{
			var dataGroupingCode = string.IsNullOrEmpty(dataGrouping) ? ZZ1_ZZZ_NKDataGrouping : (ZString)dataGrouping;
			var criteria = new SpecificRateSelectionCriteria(countryOfOrigin, dataGroupingCode, primaryPreference, "", null, effectiveDate, rateTypeCode, "");
			var result = GetRateSelectionCriteriaInfo(new IZZRateSelectionCriteria[] { criteria });
			return result.Where(r => !r.ZZT_AdditionalCode.IsEmpty && r.Match(criteria)).Select(r => r.ZZT_AdditionalCode).Distinct();
		}

		#region GetRateSelectionCriteriaInfo

		public IEnumerable<RateSelectionCriteriaInfo> GetRateSelectionCriteriaInfo(IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new List<RateSelectionCriteriaInfo>();
			var rateCriteriaDataTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();
			var secondTradeGroupsDataTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();

			foreach (var criteria in criterias)
			{
				if (!criteria.TradeGroupCountry.IsEmpty && !criteria.EffectiveDate.IsEmpty)
				{
					var criteriaId = Guid.NewGuid();

					ApplicableRateLoader.AddNewCriteriaSetRow(rateCriteriaDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk: PK, criteria);

					if (IsTariffNationalCode)
					{
						ApplicableRateLoader.AddNewCriteriaSetRow(rateCriteriaDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk: ZZ1_ZZ1_Tariff, criteria);
					}
				}
			}

			if (rateCriteriaDataTable.Rows.Count > 0)
			{
				var sql = FormattableString.Invariant($"EXEC {GetRateSelectionCriteriaInfoProcedure.QualifiedName} {GetRateSelectionCriteriaInfoProcedure.Parameters.SelectionCriteria}, {GetRateSelectionCriteriaInfoProcedure.Parameters.LanguageCode}");

				using (var command = ((IDbConnected)Factory).Connection.Command(sql))
				{
					command.AddTableValuedParameter(GetRateSelectionCriteriaInfoProcedure.Parameters.SelectionCriteria, RateCriteriaTvp.QualifiedName, rateCriteriaDataTable);
					command.AddParameter(GetRateSelectionCriteriaInfoProcedure.Parameters.LanguageCode, SqlDbType.VarChar, TranslationHelper.GetCurrentLanguageCode().ToString());

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(PopulateRateSelectionCriteriaInfo(reader));
						}
					}
				}
			}

			return result;
		}

		RateSelectionCriteriaInfo PopulateRateSelectionCriteriaInfo(IDataReader reader)
		{
			return new RateSelectionCriteriaInfo
			{
				EffectiveDate = reader.GetDateTime(reader.GetOrdinal(GetRateSelectionCriteriaInfoProcedure.Columns.EffectiveDate)),
				TradeGroupCountry = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.TradeGroupCountry),
				RateType = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZY1_RateType),
				RateCode = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZY1_RateCode),
				ZZT_OrderNumber = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZT_OrderNumber),
				ZZT_AdditionalCode = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZT_AdditionalCode),
				ZZA_TradeGroup = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZA_TradeGroup),
				ZZA_Description = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZA_Description),
				ZZS_Preference = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZS_Preference),
				ZZS_Description = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.ZZS_Description),
				SecondTradeGroup = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.SecondTradeGroup),
				TranslatedPreferenceDescription = reader.SafeGetString(GetRateSelectionCriteriaInfoProcedure.Columns.TranslatedPreferenceDescription),
				Direction = reader.GetInt32(reader.GetOrdinal(GetRateSelectionCriteriaInfoProcedure.Columns.Direction)) == 1 ? RateDirection.Import :
								reader.GetInt32(reader.GetOrdinal(GetRateSelectionCriteriaInfoProcedure.Columns.Direction)) == 2 ? RateDirection.Export : RateDirection.Both,
			};
		}

		#endregion

		#endregion

		#region GetConditionApplicabilitiesByCriteria

		public IEnumerable<ConditionApplicabilitiesByCriteria> GetConditionApplicabilitiesByCriteriaInfo(IEnumerable<IZZConditionSelectionCriteria> criterias)
		{
			var result = new List<ConditionApplicabilitiesByCriteria>();
			var conditionDataTable = ConditionApplicabilitiesLoader.GetEmptyConditionSelectionCriteriaTable();
			var secondTradeGroupsDataTable = ApplicableConditionLoader.GetEmptySecondTradeGroupsParameterTable();

			foreach (var criteria in criterias)
			{
				if (!criteria.TradeGroupCountry.IsEmpty && !criteria.EffectiveDate.IsEmpty)
				{
					var criteriaId = Guid.NewGuid();

					ConditionApplicabilitiesLoader.AddNewCriteriaSetRow(conditionDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk: PK, criteria);

					if (IsTariffNationalCode)
					{
						ConditionApplicabilitiesLoader.AddNewCriteriaSetRow(conditionDataTable, secondTradeGroupsDataTable, criteriaId, tariffPk: ZZ1_ZZ1_Tariff, criteria, false);
					}
				}
			}

			if (conditionDataTable.Rows.Count > 0)
			{
				var sql = FormattableString.Invariant($@"EXEC {GetConditionApplicabilitiesByCriteriaProcedure.QualifiedName} {GetConditionApplicabilitiesByCriteriaProcedure.Parameters.ConditionCriteriaTvp}, {GetConditionApplicabilitiesByCriteriaProcedure.Parameters.SecondTradeGroupTvp}");

				using (var command = ((IDbConnected)Factory).Connection.Command(sql))
				{
					command.AddTableValuedParameter(GetConditionApplicabilitiesByCriteriaProcedure.Parameters.ConditionCriteriaTvp, TvpConditionSelectionCriteria.QualifiedName, conditionDataTable);
					command.AddTableValuedParameter(GetConditionApplicabilitiesByCriteriaProcedure.Parameters.SecondTradeGroupTvp, TvpSecondTradeGroup.QualifiedName, secondTradeGroupsDataTable);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(PopulateConditionApplicabilitiesByCriteria(reader));
						}
					}
				}
			}

			return result;
		}

		ConditionApplicabilitiesByCriteria PopulateConditionApplicabilitiesByCriteria(IDataReader reader)
		{
			return new ConditionApplicabilitiesByCriteria
			{
				EffectiveDate = reader.GetDateTime(reader.GetOrdinal(GetConditionApplicabilitiesByCriteriaProcedure.Columns.EffectiveDate)),
				TradeGroupCountry = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.TradeGroupCountry),
				ZX2_ConditionClass = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZX2_ConditionClass),
				ZX2_ConditionType = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZX2_ConditionType),
				ZZT_OrderNumber = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZT_OrderNumber),
				ZZT_AdditionalCode = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZT_AdditionalCode),
				ZZA_TradeGroup = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZA_TradeGroup),
				ZZA_Description = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZA_Description),
				ZZS_Preference = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZS_Preference),
				ZZS_Description = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.ZZS_Description),
				SecondTradeGroup = reader.SafeGetString(GetConditionApplicabilitiesByCriteriaProcedure.Columns.SecondTradeGroup)
			};
		}

		#endregion

		#region TaxOrFeeRelated

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|GSTVATRate", Caption = "VAT Rate")]
		public ZDecimal GSTVATRate => new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_EndDate, ZZ1_StartDate)?.ZZF_Value ?? ZDecimal.Zero;

		[ResourceStringData("Enterprise.Customs.Universal.TariffView|ZZ1_ZZF_NKTaxOrFeeCode", Caption = "VAT Code")]
		[List(nameof(Lookups) + "." + nameof(TariffViewLookups.TaxOrFeeCodeList))]
		public override ZString ZZ1_ZZF_NKTaxOrFeeCode { get => base.ZZ1_ZZF_NKTaxOrFeeCode; set => base.ZZ1_ZZF_NKTaxOrFeeCode = value; }

		public ZString GetDefaultTaxOrFeeCode(ZDateTime valuationDate, ZString dataGrouping)
		{
			if (valuationDate.IsEmpty)
			{
				throw new ArgumentException("Empty valuation date not supported");
			}

			if (dataGrouping.IsEmpty)
			{
				throw new ArgumentException("Empty data grouping not supported");
			}

			var defaultTaxOrFeeCode = ZString.Empty;
			var relatedTaxOrFeeCodes = GetEffectiveVATApplicabilities(valuationDate)
				.Where(x => !x.ZX5_ZZF_NKTaxOrFeeCode.IsEmpty && x.ZX5_ZZZ_NKDataGrouping == dataGrouping)
				.Select(x => x.ZX5_ZZF_NKTaxOrFeeCode).Distinct().ToArray();
			if (!relatedTaxOrFeeCodes.Any())
			{
				defaultTaxOrFeeCode = ZZ1_ZZF_NKTaxOrFeeCode;
			}
			else if (relatedTaxOrFeeCodes.Length == 1)
			{
				defaultTaxOrFeeCode = relatedTaxOrFeeCodes.SingleOrDefault();
			}

			if (defaultTaxOrFeeCode.IsEmpty)
			{
				var query = GetTaxOrFeeQuery(valuationDate, dataGrouping);
				defaultTaxOrFeeCode = Factory.LoadTop1<RefCusTaxOrFee>(query)?.ZZF_Code ?? ZString.Empty;

				if (defaultTaxOrFeeCode.IsEmpty && ZZ1_ZZZ_NKDataGrouping != dataGrouping)
				{
					query = GetTaxOrFeeQuery(valuationDate, ZZ1_ZZZ_NKDataGrouping);
					defaultTaxOrFeeCode = Factory.LoadTop1<RefCusTaxOrFee>(query)?.ZZF_Code ?? ZString.Empty;
				}
			}
			return defaultTaxOrFeeCode;
		}

		ZQuery GetTaxOrFeeQuery(ZDateTime valuationDate, ZString dataGrouping)
		{
			var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, dataGrouping)
			{
				OrderBy = RefCusTaxOrFeeSchema.ZZF_Value.Name + OrderByClause.Descending
			};
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZX0_NKTaxOrFeeType, SQLComparisonOperator.Equal, CusEntryFeeTypes.VAT);
			return query;
		}

		public TariffView[] GetEffectiveChildTariffs(ZDateTime valuationDate, string childTariffType = null)
		{
			return new Loader(Factory).GetEffectiveChildTariffs(ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_TariffTypeCode, ZZ1_TariffCode, valuationDate, childTariffType);
		}

		public ZDecimal GetHighestVATRateByTaxOrFeeCode(ZString dataGroupingCode, ZString code, ZDateTime date)
		{
			var result = ZDecimal.Zero;
			var originalCode = code;
			var vatApplicabilitiesList = GetEffectiveVATApplicabilities(date).Select(x => x.ZX5_ZZF_NKTaxOrFeeCode).ToHashSet();
			if (!code.IsEmpty && vatApplicabilitiesList.Count > 0 && !vatApplicabilitiesList.Contains(code))
			{
				code = ZString.Empty;
			}

			var list = new RefCusTaxOrFee.Loader(Factory).LoadTaxOrFeeFromCodeDate(dataGroupingCode, code, date);
			var matched = list.Where(x => vatApplicabilitiesList.Contains(x.ZZF_Code));
			if (matched.Any())
			{
				result = matched.Select(x => x.ZZF_Value).Max();
			}
			else
			{
				result = new RefCusTaxOrFee.Loader(Factory).LoadTaxOrFeeFromCodeDate(dataGroupingCode, originalCode, date).Select(x => x.ZZF_Value).DefaultIfEmpty(ZDecimal.Zero).Max();
			}
			return result;
		}

		public IEnumerable<VATApplicabilityView> GetEffectiveVATApplicabilities(ZDateTime date)
		{
			return VATApplicabilities.Where(x => x.ZX5_StartDate <= date && x.ZX5_EndDate >= date);
		}

		#endregion

		#region ITariffData Members

		bool ITariffData.IsNomenclatureGroup => false;

		ZString ITariffData.CompositeKey => ZZ1_CompositeKeyOnZZ5;

		ZString ITariffData.TariffCode => ZZ1_TariffCode;

		ZString ITariffData.GetDescription(ZString languageCode) => TranslationHelper.GetTranslatedValue(this, base.ZZ1_Description, CusRefTariffLanguageViewSchema.ZX7_Description, languageCode);

		#endregion

		#region ITariff Members
		ZString ITariff.Code => ZZ1_TariffCode;
		ZString ITariff.Description => ZZ1_Description;
		ZString ITariff.UQ1 => ZZ1_ZZ8_UQ1;
		ZString ITariff.UQ2 => ZZ1_ZZ8_UQ2;
		ZString ITariff.UQ3 => ZZ1_ZZ8_UQ3;
		ZString ITariff.UQ4 => ZZ1_ZZ8_UQ4;
		ZString ITariff.UQ5 => ZZ1_ZZ8_UQ5;
		#endregion

		#region Attributes Related

		RefCusTariffAttribute FindAttributeByName(string attributeName)
		{
#pragma warning disable CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
			return Attributes.OfType<RefCusTariffAttribute>().FirstOrDefault(x => x.ZZ3_Name.EqualsIgnoringCase(attributeName));
#pragma warning restore CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
		}

		public bool SetAttribute(RefCusTariffAttributeName attributeName, object value)
		{
			var result = false;
			var attrToSet = FindAttributeByName(attributeName.ZY6_Name);
			if (attrToSet != null)
			{
				attrToSet.ZZ3_Value = value.ToString();
				result = true;
			}
			return result;
		}

		public TariffAttributeView GetAttribute(ZString key) => Attributes.FirstOrDefault(x => x.ZZ3_Name == key);

		public IEnumerable<TariffAttributeView> GetAttributes(ZString key) => Attributes.Where(x => x.ZZ3_Name == key);

		public bool HasAttribute(ZString key) => GetAttribute(key) != null;

		public bool HasAttribute(ZString key, ZString value)
		{
			return Attributes.Any(x => x.ZZ3_Name == key && x.ZZ3_Value == value);
		}

		public IAdditionalAttributeInformationProvider AdditionalAttributeInformationProvider => Enterprise.Customs.Universal.AdditionalAttributeInformationProvider.GetAdditionalAttributeInformationProvider(this);
		#endregion

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => CusRefTariffLanguageViewSchema.Instance;

		public Type LanguageTableType => typeof(CusRefTariffLanguageView);

		#endregion

		#region IUnitListForRateFormulaEditProvider

		public CodeDescriptionPairList UnitList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (UnitsOfMeasure.Count > 0)
				{
					var unitList = UnitsOfMeasure[0].Lookups.UOMList;
					foreach (var uom in UnitsOfMeasure)
					{
						var uomCode = uom.ZZ8_UOM;
						if (unitList.ContainsCode(uomCode))
						{
							result.AddPairIfNotExist(uomCode, unitList.GetDescriptionFromCode(uomCode));
						}
					}
				}
				return result;
			}
		}

		#endregion
	}
}
