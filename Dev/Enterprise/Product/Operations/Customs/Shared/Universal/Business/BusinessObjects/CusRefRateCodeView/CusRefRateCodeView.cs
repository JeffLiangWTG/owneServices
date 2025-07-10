using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(CusRefRateCodeViewSchema.Constants.ZY1_RateCode), DescriptionProperty(CusRefRateCodeViewSchema.Constants.ZY1_Description)]
	public class CusRefRateCodeView : AutoCusRefRateCodeView, ITranslatableZZBusinessObject
	{
		public CusRefRateCodeView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public RefCusRateType RateType => RefCusRateType.Loader.Load(Factory, ZY1_ZZZ_NKDataGrouping, ZY1_RateType);

		public override ZString ZY1_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZY1_Description, RefCusRateCodeLanguageSchema.ZXC_Description);
			set => base.ZY1_Description = value;
		}

		public ZString DescriptionNotTranslated => base.ZY1_Description;

		[ReadOnly(true)]
		public override ZString ZY1_DataSet
		{
			get => base.ZY1_DataSet;
			set => base.ZY1_DataSet = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZY1_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusRateCodeLanguageSchema.Instance;
		public Type LanguageTableType => typeof(RefCusRateCodeLanguage);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<CusRefRateCodeView>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusRefRateCodeView.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusRefRateCodeView);
			}

			static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGrouping, IRateCodeLoadCriteria additionalCriteria, bool includeParentDataGrouping = true)
			{
				var filter = new ZQuery();

				if (dataGrouping.IsEmpty)
				{
					filter.IsNoResultQuery = true;
				}
				else
				{
					if (includeParentDataGrouping)
					{
						filter.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping));
					}
					else
					{
						filter.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping);
					}

					if (additionalCriteria != null)
					{
						additionalCriteria.AddToRateCodeFilter(filter);
					}
				}

				return filter;
			}

			public static CusRefRateCodeView[] Load(BusinessObjectFactory factory, ZString dataGrouping, IRateCodeLoadCriteria additionalCriteria = null, bool includeParentDataGrouping = true)
			{
				var filter = GetFilter(factory, dataGrouping, additionalCriteria, includeParentDataGrouping);
				return factory.Load<CusRefRateCodeView>(filter);
			}

			public static CusRefRateCodeView[] LoadByRateType(BusinessObjectFactory factory, ZString dataGrouping, string rateType)
			{
				var singleTypeRateCodeCriteria = new RateCodeLoadCriteria() { RateTypesToInclude = new ZString[] { rateType } };
				var filter = GetFilter(factory, dataGrouping, singleTypeRateCodeCriteria);
				return factory.Load<CusRefRateCodeView>(filter);
			}

			public static CusRefRateCodeView[] LoadByRateCode(BusinessObjectFactory factory, ZString dataGrouping, ZString rateCode, string dataSet = "")
			{
				var rateCodeQuery = new ZDBOnlyQuery(typeof(CusRefRateCodeView));
				rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateCode, rateCode);
				if (!dataGrouping.IsEmpty)
				{
					rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping);
				}
				if (!string.IsNullOrEmpty(dataSet))
				{
					rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_DataSet, dataSet);
				}

				return factory.Load<CusRefRateCodeView>(rateCodeQuery);
			}

			public static CusRefRateCodeView[] LoadByDataSet(BusinessObjectFactory factory, ZString dataGrouping, ZString dataSet, bool includeParentDataGrouping = true)
			{
				var rateCodeQuery = new ZQuery();
				rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_DataSet, dataSet);
				if (!dataGrouping.IsEmpty)
				{
					if (includeParentDataGrouping)
					{
						rateCodeQuery.AddToFilter(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping));
					}
					else
					{
						rateCodeQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping);
					}
				}

				return factory.Load<CusRefRateCodeView>(rateCodeQuery);
			}
		}
	}
}
