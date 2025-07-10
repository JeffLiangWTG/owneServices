using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusNomenclatureLanguage : AutoRefCusNomenclatureLanguage
	{
		public RefCusNomenclatureLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("NomenclatureGroup")]
		public override ZGuid ZX8_ZZ5_NomenclatureGroup
		{
			get => base.ZX8_ZZ5_NomenclatureGroup;
			set => base.ZX8_ZZ5_NomenclatureGroup = value;
		}

		public RefCusNomenclatureGroup NomenclatureGroup
		{
			get { return Factory.Load<RefCusNomenclatureGroup>(ZX8_ZZ5_NomenclatureGroup); }
		}

		[BusinessObjectTestExclude]
		public override ZString ZX8_ZX6_NKLanguage
		{
			get => base.ZX8_ZX6_NKLanguage;
			set => base.ZX8_ZX6_NKLanguage = value;
		}

		public static ZString GetFullDescriptionForCompositeKey(BusinessObjectFactory factory, ZString key, ZString dataGroupingCode, ZString type, ZDateTime date, bool includeSectionHeadings, bool includeChapterHeading, ZString languageCode)
		{
			if (languageCode.IsEmpty)
			{
				return ZString.Empty;
			}
			var descBuilder = new ZStringBuilder();
			ZString dot = ".";
			var keySegments = key.Split('.');
			var dataGroupings = MasterFiles.Business.RefDataGrouping.GetDataGroupingIncludingParent(factory, dataGroupingCode);
			for (var i = includeSectionHeadings ? 0 : 1; i < keySegments.Length; i++)
			{
				var currentKey = ZString.Join(dot, keySegments, 0, i + 1);
				if (!currentKey.IsEmpty && !currentKey.EndsWith(dot, StringComparison.OrdinalIgnoreCase))
				{
					AppendDescriptionForSegment(factory, currentKey, dataGroupings, type, date, includeChapterHeading, languageCode, descBuilder, out bool descForSegmentExistsOnlyInDefaultLanguage);
					if (descForSegmentExistsOnlyInDefaultLanguage)
					{
						return ZString.Empty;
					}
				}
			}
			return descBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}
		static void AppendDescriptionForSegment(BusinessObjectFactory factory, ZString key, ZString[] dataGroupingIncludeParentCodes, ZString type, ZDateTime date, bool includeChapterHeading, ZString languageCode, ZStringBuilder fullDescriptionBuilder, out bool descForSegmentExistsOnlyInDefaultLanguage)
		{
			Argument.GreaterThanZero(dataGroupingIncludeParentCodes.Length, nameof(dataGroupingIncludeParentCodes.Length));
			var preferredLanguageDesc = GetDescriptionForCompositeKey(factory, key, dataGroupingIncludeParentCodes, type, date, includeChapterHeading, languageCode);
			if (!preferredLanguageDesc.IsEmpty)
			{
				fullDescriptionBuilder.Append(preferredLanguageDesc);
				descForSegmentExistsOnlyInDefaultLanguage = false;
			}
			else
			{
				var defaultLanguageDesc = RefCusNomenclatureGroup.GetDescriptionForCompositeKey(factory, key, dataGroupingIncludeParentCodes[0], type, date, includeChapterHeading);
				descForSegmentExistsOnlyInDefaultLanguage = !defaultLanguageDesc.IsEmpty;
			}
		}

		static ZString GetDescriptionForCompositeKey(BusinessObjectFactory factory, ZString key, ZString[] dataGroupingCodes, ZString type, ZDateTime date, bool includeChapterHeading, ZString languageCode)
		{
			var query = new ZDBOnlyQuery(typeof(RefCusNomenclatureLanguage));
			query.AddToFilter(RefCusNomenclatureLanguageSchema.ZX8_ZX6_NKLanguage, languageCode);

			var subQuery = new ZDBOnlySubQuery(typeof(RefCusNomenclatureGroup), RefCusNomenclatureLanguageSchema.ZX8_ZZ5_NomenclatureGroup);
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupingCodes);
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZ9_NKNomenclatureGroupType, type);
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, key);
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			if (!includeChapterHeading)
			{
				subQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			query.AddSubQuery(RefCusNomenclatureLanguageSchema.ZX8_ZZ5_NomenclatureGroup, RefCusNomenclatureGroupSchema.PK, subQuery, JoinCondition.And);

			var nomenclatureLanguage = factory.LoadTop1<RefCusNomenclatureLanguage>(query);
			return nomenclatureLanguage?.ZX8_Description ?? ZString.Empty;
		}
	}
}
