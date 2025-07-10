using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusTaxOrFeeSchema.Constants.ZZF_Code), DescriptionProperty(RefCusTaxOrFeeSchema.Constants.ZZF_Description)]
	public sealed class RefCusTaxOrFee : AutoRefCusTaxOrFee, ITranslatableZZBusinessObject
	{
		public RefCusTaxOrFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusTaxOrFeeLanguageSchema.Instance;
		public Type LanguageTableType => typeof(RefCusTaxOrFeeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusTaxOrFee>(this);
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZF_ZZZ_NKDataGrouping
		{
			get { return base.ZZF_ZZZ_NKDataGrouping; }
			set { base.ZZF_ZZZ_NKDataGrouping = value; }
		}

		public override ZString ZZF_Description
		{
			get { return TranslationHelper.GetTranslatedValue(this, base.ZZF_Description, RefCusTaxOrFeeLanguageSchema.ZXU_Description); }
			set { base.ZZF_Description = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZF_ZZZ_NKDataGrouping); }
		}

		[RelatedBusinessObject("TaxOrFeeType")]
		public override ZString ZZF_ZX0_NKTaxOrFeeType { get => base.ZZF_ZX0_NKTaxOrFeeType; set => base.ZZF_ZX0_NKTaxOrFeeType = value; }

		public RefCusTaxOrFeeType TaxOrFeeType
		{
			get { return Factory.LoadFromNaturalKey<RefCusTaxOrFeeType>(RefCusTaxOrFeeTypeSchema.ZX0_TaxOrFeeType, ZZF_ZX0_NKTaxOrFeeType); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusTaxOrFee.Loader, Integration.Customs.Shared.IRefCusTaxOrFeeProvider
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public ZDecimal LoadMostRecentEffectiveDeminimusOfCountry(string countryCode) => LoadMostRecentEffectiveTaxOrFeeFromCodeDate(countryCode, Constants.RefCusTaxOrFeeTypes.Deminimus, ZDateTime.Now)?.ZZF_Value ?? ZDecimal.Zero;

			public ZDecimal LoadExportDeminimusOfCountry(string countryCode) => LoadMostRecentEffectiveTaxOrFeeFromCodeDate(countryCode, Constants.RefCusTaxOrFeeTypes.ExportDeminimus, ZDateTime.Now)?.ZZF_Value ?? ZDecimal.Zero;

			static ZQuery GetRefCusTaxOrFeeQuery(IEnumerable<ZString> dataGroupings, ZDateTime startDate, ZDateTime endDate, string type = "")
			{
				ZQuery result;
				var validDataGroupings = dataGroupings.Where(x => !x.IsEmpty).ToArray();
				if (!validDataGroupings.Any() || !startDate.IsValid || !endDate.IsValid)
				{
					result = ZQuery.NoResultQuery;
				}
				else
				{
					result = new ZQuery(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, validDataGroupings);
					result.AddToFilter(RefCusTaxOrFeeSchema.ZZF_StartDate, SQLComparisonOperator.LessThanOrEqualTo, startDate);
					result.AddToFilter(RefCusTaxOrFeeSchema.ZZF_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);
					if (!string.IsNullOrEmpty(type))
					{
						result.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZX0_NKTaxOrFeeType, type);
					}
				}
				return result;
			}

			public RefCusTaxOrFee LoadMostRecentEffectiveTaxOrFeeFromCodeDate(ZString dataGrouping, ZString taxOrFeeCode, ZDateTime startDate, ZDateTime endDate)
			{
				var dataGroupings = GetAllDataGroups(dataGrouping);
				var result = LoadTaxOrFeeFromCodeDate(dataGrouping, taxOrFeeCode, startDate, endDate);
				return result.OrderByDescending(x => string.Join("_", dataGroupings.ToList().IndexOf(x.ZZF_ZZZ_NKDataGrouping), x.ZZF_StartDate.ToISO8601String())).FirstOrDefault();
			}

			public RefCusTaxOrFee LoadMostRecentEffectiveTaxOrFeeFromCodeDate(string dataGrouping, string taxOrFeeCode, ZDateTime valuationDate)
				=> LoadMostRecentEffectiveTaxOrFeeFromCodeDate(dataGrouping, taxOrFeeCode, valuationDate, valuationDate);

			public RefCusTaxOrFee[] LoadTaxOrFeeFromCodeDate(ZString dataGrouping, ZString taxOrFeeCode, ZDateTime startDate, ZDateTime endDate)
			{
#if DEBUG
#else
				return Factory.GetCachedValue(string.Join("_", "RefCusTaxOrFee.Loader.LoadTaxOrFeeFromCodeDate", dataGrouping, taxOrFeeCode, startDate, endDate), () =>
				{
#endif
				var dataGroupings = GetAllDataGroups(dataGrouping);
				var query = GetRefCusTaxOrFeeQuery(dataGroupings, startDate, endDate);
				if (!query.IsNoResultQuery && !taxOrFeeCode.IsEmpty)
				{
					query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_Code, taxOrFeeCode);
				}
				return Factory.Load<RefCusTaxOrFee>(query);
#if DEBUG
#else
				});
#endif
			}

			public RefCusTaxOrFee[] LoadTaxOrFeeFromCodeDate(ZString dataGrouping, string taxOrFeeCode, ZDateTime valuationDate)
				=> LoadTaxOrFeeFromCodeDate(dataGrouping, taxOrFeeCode, valuationDate, valuationDate);

			public static CodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString dataGrouping, ZDateTime valuationDate, string type = "")
			{
				return factory.GetCachedValue(string.Join("_", "RefCusTaxOrFee.GetList", dataGrouping, valuationDate, type), () =>
				{
					var result = new CodeDescriptionPairList();
					var query = GetRefCusTaxOrFeeQuery(new[] { dataGrouping }, valuationDate, valuationDate, type);
					result.AddRange(factory.Load<RefCusTaxOrFee>(query).OrderBy(x => x.ZZF_Code).ToArray());
					return result;
				});
			}

			public static RefCusTaxOrFee[] LoadTaxOrFeeFromTypeDate(BusinessObjectFactory factory, ZString dataGrouping, ZString type, ZDateTime valuationDate)
			{
				return factory.GetCachedValue(string.Join("_", "LoadTaxOrFeeFromTypeDate", dataGrouping, type, valuationDate), () =>
				{
					var query = GetRefCusTaxOrFeeQuery(new[] { dataGrouping }, valuationDate, valuationDate, type);
					return factory.Load<RefCusTaxOrFee>(query).OrderBy(x => x.ZZF_Code).ToArray();
				});
			}

			public bool IsExistedTaxOrFee(ZString dataGrouping, ZString code, ZString type)
			{
				return Factory.GetCachedValue(string.Join("_", "IsExistedTaxOrFee", dataGrouping, code, type), () =>
				{
					var result = false;
					if (!dataGrouping.IsEmpty && !code.IsEmpty && !type.IsEmpty)
					{
						var dataGroupings = GetAllDataGroups(dataGrouping);

						var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, dataGroupings);
						query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZX0_NKTaxOrFeeType, type);
						query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_Code, code);

						result = Factory.LoadTop1<RefCusTaxOrFee>(query) != null;
					}
					return result;
				});
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusTaxOrFee);

			IEnumerable<ZString> GetAllDataGroups(ZString dataGrouping)
			{
				var parentDataGrouping = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGrouping);
				if (!parentDataGrouping.IsEmpty)
				{
					yield return parentDataGrouping;
				}
				yield return dataGrouping;
			}
		}
	}
}
