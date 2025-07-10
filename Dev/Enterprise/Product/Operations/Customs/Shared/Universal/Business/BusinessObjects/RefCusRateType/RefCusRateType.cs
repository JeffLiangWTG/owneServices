using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusRateType.Schema.ZZR_RateType), DescriptionProperty(RefCusRateType.Schema.ZZR_Description)]
	public sealed class RefCusRateType : AutoRefCusRateType, ITranslatableZZBusinessObject
	{
		public RefCusRateType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZR_ZZZ_NKDataGrouping
		{
			get { return base.ZZR_ZZZ_NKDataGrouping; }
			set { base.ZZR_ZZZ_NKDataGrouping = value; }
		}

		public override ZString ZZR_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZR_Description, RefCusRateTypeLanguageSchema.ZXT_Description);
			set => base.ZZR_Description = value;
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZR_ZZZ_NKDataGrouping); }
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusRateTypeLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusRateTypeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusRateType>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusRateType.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static RefCusRateType Load(BusinessObjectFactory factory, string countryCode, string rateType)
			{
				var query = new ZQuery(RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, countryCode);
				query.AddToFilter(RefCusRateTypeSchema.ZZR_RateType, rateType);
				return factory.LoadTop1<RefCusRateType>(query);
			}

			public static RefCusRateType GetRateTypeByRateCode(BusinessObjectFactory factory, string dataGroupingCode, ZString rateCode)
			{
				RefCusRateType result = null;
				if (!rateCode.IsEmpty)
				{
					result = factory.GetCachedValue(string.Join("_", "GetRateTypeByRateCode", dataGroupingCode, rateCode), () =>
					{
						RefCusRateType rateType = null;
						var rateCodeViews = CusRefRateCodeView.Loader.LoadByRateCode(factory, dataGroupingCode, rateCode);
						if (rateCodeViews.Any())
						{
							var query = new ZDBOnlyQuery(typeof(RefCusRateType));
							query.AddToFilter(RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, dataGroupingCode);
							query.AddToFilter(RefCusRateTypeSchema.ZZR_RateType, rateCodeViews.Select(x => x.ZY1_RateType));
							return factory.LoadTop1<RefCusRateType>(query);
						}

						return rateType;
					});
				}

				return result;
			}

			public static RefCusRateType[] GetRateTypesByDataGrouping(BusinessObjectFactory factory, string dataGroupingCode)
			{
				var query = new ZQuery(RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, dataGroupingCode);
				return factory.Load<RefCusRateType>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusRateType);
		}
	}
}
