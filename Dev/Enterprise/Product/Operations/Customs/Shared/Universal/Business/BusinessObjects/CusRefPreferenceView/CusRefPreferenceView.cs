using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(CusRefPreferenceViewSchema.Constants.ZZS_Preference), DescriptionProperty(CusRefPreferenceViewSchema.Constants.ZZS_Description)]
	public sealed class CusRefPreferenceView : AutoCusRefPreferenceView, ITranslatableZZBusinessObject
	{
		public CusRefPreferenceView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZS_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}

		[ReadOnly(true)]
		public override ZString ZZS_DataSet { get => base.ZZS_DataSet; set => base.ZZS_DataSet = value; }

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZS_ZZZ_NKDataGrouping
		{
			get { return base.ZZS_ZZZ_NKDataGrouping; }
			set { base.ZZS_ZZZ_NKDataGrouping = value; }
		}

		public override ZString ZZS_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZS_Description, RefCusPreferenceLanguageSchema.ZX9_Description);
			set => base.ZZS_Description = value;
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZS_ZZZ_NKDataGrouping); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<CusRefPreferenceView>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusRefPreferenceView.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusRefPreferenceView);
			}

			public static CodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString country)
			{
				var cachedKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_ZZRefCusPreference", country, TranslationHelper.GetCurrentLanguageCode());
				return factory.GetCachedValue(cachedKey, () =>
				{
					var codes = new CodeDescriptionPairList();

					var codesQuery = new ZQuery(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, country));
					codesQuery.OrderBy = CusRefPreferenceViewSchema.ZZS_Preference.Name + OrderByClause.Ascending;

					codes.AddRange(factory.Load<CusRefPreferenceView>(codesQuery));
					return codes;
				});
			}

			public static CusRefPreferenceView LoadByPreference(BusinessObjectFactory factory, ZString dataGrouping, ZString preference, ZString dataSet)
			{
				var codesQuery = new ZQuery(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, dataGrouping));
				codesQuery.AddToFilter(CusRefPreferenceViewSchema.ZZS_Preference, preference);
				codesQuery.AddToFilter(CusRefPreferenceViewSchema.ZZS_DataSet, dataSet);
				return factory.LoadTop1<CusRefPreferenceView>(codesQuery);
			}
		}

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusPreferenceLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusPreferenceLanguage);

		#endregion
	}
}
