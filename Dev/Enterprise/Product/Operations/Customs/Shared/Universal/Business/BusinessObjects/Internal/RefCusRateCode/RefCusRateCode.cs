using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusRateCodeSchema.Constants.ZY1_RateCode), DescriptionProperty(RefCusRateCodeSchema.Constants.ZY1_Description)]
	public sealed class RefCusRateCode : AutoRefCusRateCode, ITranslatableZZBusinessObject
	{
		public RefCusRateCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public RefCusRateType RateType => Factory.Load<RefCusRateType>(ZY1_ZZR_RateType);

		[RelatedBusinessObject("RateType")]
		public override ZGuid ZY1_ZZR_RateType
		{
			get { return base.ZY1_ZZR_RateType; }
			set { base.ZY1_ZZR_RateType = value; }
		}

		public override ZString ZY1_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZY1_Description, RefCusRateCodeLanguageSchema.ZXC_Description);
			set => base.ZY1_Description = value;
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusRateCodeLanguageSchema.Instance;
		public Type LanguageTableType => typeof(RefCusRateCodeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusRateCode>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusRateCode.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusRateCode);
			}

			static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGrouping, ZString[] rateTypes, SQLComparisonOperator rateTypeComparisonOperator, ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(RefCusRateCode));

				var rateTypeQuery = new ZDBOnlySubQuery(typeof(RefCusRateType), RefCusRateCodeSchema.ZY1_ZZR_RateType);
				rateTypeQuery.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, dataGrouping));
				if (rateTypes.Length > 0 && rateTypeComparisonOperator != null)
				{
					rateTypeQuery.AddToFilter(RefCusRateTypeSchema.ZZR_RateType, rateTypeComparisonOperator, rateTypes);
				}

				result.AddSubQuery(rateTypeQuery, JoinCondition.And);

				if (dataGrouping.IsEmpty)
				{
					result.IsNoResultQuery = true;
				}
				else if (filter != null)
				{
					result.AddToFilter(filter);
				}

				return result;
			}

			public static RefCusRateCode[] Load(BusinessObjectFactory factory, ZString dataGrouping, ZQuery additionalFilter = null)
			{
				var filter = GetFilter(factory, dataGrouping, Array.Empty<ZString>(), null, additionalFilter);
				return factory.Load<RefCusRateCode>(filter);
			}

			public static RefCusRateCode[] LoadByRateType(BusinessObjectFactory factory, ZString dataGrouping, string rateType, SQLComparisonOperator rateTypeComparisonOperator, ZQuery additionalFilter = null)
			{
				return LoadByRateType(factory, dataGrouping, new ZString[] { rateType }, rateTypeComparisonOperator, additionalFilter);
			}

			public static RefCusRateCode[] LoadByRateType(BusinessObjectFactory factory, ZString dataGrouping, ZString[] rateTypes, SQLComparisonOperator rateTypeComparisonOperator, ZQuery additionalFilter = null)
			{
				var filter = GetFilter(factory, dataGrouping, rateTypes, rateTypeComparisonOperator, additionalFilter);
				return factory.Load<RefCusRateCode>(filter);
			}
		}
	}
}
