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
	public sealed class RefCusNomenclatureGroup : AutoRefCusNomenclatureGroup, ITariffData, ITranslatableZZBusinessObject
	{
		public RefCusNomenclatureGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZString GetFullDescriptionForCompositeKey(BusinessObjectFactory factory, ZString key, ZString dataGroupingCode, ZString type, ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading)
		{
			var result = new ZStringBuilder();
			ZString dot = ".";
			var keySegments = key.Split('.');
			for (var i = includeSectionHeadings ? 0 : 1; i < keySegments.Length; i++)
			{
				var currentKey = ZString.Join(dot, keySegments, 0, i + 1);
				if (!currentKey.IsEmpty && !currentKey.EndsWith(dot, StringComparison.OrdinalIgnoreCase))
				{
					var desc = GetDescriptionForCompositeKey(factory, currentKey, dataGroupingCode, type, date, includeChapterHeading);
					if (!desc.IsEmpty)
					{
						result.Append(desc);
					}
				}
			}
			return result.ToStringWithDelimiterBetweenAppends(" ");
		}

		public static ZString GetDescriptionForCompositeKey(BusinessObjectFactory factory, ZString key, ZString dataGroupingCode, ZString type, ZDateTime date, bool includeChapterHeading)
		{
			var query = new ZQuery(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupingCode));
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZ9_NKNomenclatureGroupType, type);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, key);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			if (!includeChapterHeading)
			{
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			var nomenclatureGroup = factory.LoadTop1<RefCusNomenclatureGroup>(query);
			return nomenclatureGroup?.ZZ5_Description ?? ZString.Empty;
		}

		#region Properties

		public ZString ZZ5_AlternateLanguageDescription
		{
			get => TranslationHelper.GetAlternateLanguageDescription(this, RefCusNomenclatureLanguageSchema.ZX8_Description);
		}

		#endregion

		#region ITariffData Members

		bool ITariffData.IsNomenclatureGroup => true;

		ZString ITariffData.CompositeKey => ZZ5_CompositeKey;

		ZString ITariffData.TariffCode => ZZ5_Value;

		ZString ITariffData.GetDescription(ZString languageCode) => TranslationHelper.GetTranslatedValue(this, base.ZZ5_Description, RefCusNomenclatureLanguageSchema.ZX8_Description, languageCode);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusNomenclatureGroup>(this);
		}

		[ChildEditable]
		public RefCusConditionCollectionForNomenclature Conditions
		{
			get
			{
				if (conditions == null)
				{
					conditions = new RefCusConditionCollectionForNomenclature(this);
					RegisterEditableChildObject(conditions);
				}
				return conditions;
			}
		}
		RefCusConditionCollectionForNomenclature conditions;

		#region ITranslatableZZBusinessObject

		public ITableSchema LanguageTableSchema => RefCusNomenclatureLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusNomenclatureLanguage);

		#endregion
	}
}
