using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageText : AutoRefLanguageText, IRefLanguageText
	{
		public RefLanguageText(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static ZString GetDatabaseValueFromUnknownObject(BusinessObjectFactory factory, ZGuid pk, ZString tableCode, ZString columnName)
		{
			var result = factory.Load(tableCode, pk);
			if (result != null)
			{
				return result[columnName].ToString();
			}

			return string.Empty;
		}

		public static ZString GetTranslation(BusinessObjectFactory factory, ZString language, ZString columnName, ZString parentTableCode, ZString parentPk)
		{
			return GetTranslation(factory, language, columnName, parentTableCode, new ZGuid(parentPk));
		}

		public static ZString GetTranslation(BusinessObjectFactory factory, ZString language, ZString columnName, ZString parentTableCode, ZGuid parentPk)
		{
			var filter = new ZQuery(RefLanguageTextSchema.RLT_Language, language);
			filter.AddToFilter(RefLanguageTextSchema.RLT_ColumnName, columnName);
			filter.AddToFilter(RefLanguageTextSchema.RLT_ParentTableCode, parentTableCode);
			filter.AddToFilter(RefLanguageTextSchema.RLT_ParentId, parentPk);

			var result = factory.LoadTop1<RefLanguageText>(filter);
			if (result != null)
			{
				return result.RLT_Text;
			}
			return GetDatabaseValueFromUnknownObject(factory, parentPk, parentTableCode, columnName);
		}

		public static RefLanguageText GetByParentPKAndLanguage(BusinessObjectFactory factory, ZString parentPk, ZString language, ZString parentTableCode, ZString columnName)
		{
			var filter = new ZQuery(new ZQuery(RefLanguageTextSchema.RLT_ParentTableCode, parentTableCode));
			filter.AddToFilter(new ZQuery(RefLanguageTextSchema.RLT_ParentId, new ZGuid(parentPk)));
			filter.AddToFilter(new ZQuery(RefLanguageTextSchema.RLT_Language, language));
			filter.AddToFilter(new ZQuery(RefLanguageTextSchema.RLT_ColumnName, columnName));
			return factory.LoadTop1<RefLanguageText>(filter);
		}

		public static IEnumerable<MultilingualLanguageText> GetRuntimeLanguageCaptions(BusinessObjectFactory factory, ZString columnName, ZString parentTableCode, BusinessObject businessObj, string[] filterContextColumns)
		{
			var result = new List<MultilingualLanguageText>();
			var contextRecordsFilter = new ZQuery();
			if (filterContextColumns != null)
			{
				foreach (var f in filterContextColumns)
				{
					var contextValue = businessObj[f];
					contextRecordsFilter.AddToFilter(
						ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(f, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(CargoWise.Schema.Schema.GetPrefixFromColumnName(f)).TableName),
						contextValue);
				}
			}
			var contextResults = factory.Load(businessObj.GetType(), contextRecordsFilter);

			var filter = new ZQuery();
			filter.AddToFilter(RefLanguageTextSchema.RLT_ColumnName, columnName);
			filter.AddToFilter(RefLanguageTextSchema.RLT_ParentTableCode, parentTableCode);
			filter.AddToFilter(RefLanguageTextSchema.RLT_ParentId, contextResults.Select(x => x.PK));

			var languageTextResults = factory.Load<RefLanguageText>(filter);
			if (filterContextColumns != null)
			{
				foreach (var contextResult in contextResults)
				{
					if (!languageTextResults.Any(x => x.RLT_ParentId == contextResult.PK && x.RLT_Language == SharedConstants.Languages.English))
					{
						result.Add(new MultilingualLanguageText(contextResult.PK.ToString(), parentTableCode, columnName, contextResult[columnName].ToString(), factory));
					}
				}
				foreach (var langText in languageTextResults)
				{
					var englishText = langText.RLT_Text;
					if (langText.RLT_Language != SharedConstants.Languages.English)
					{
						var captionOnEnglish = languageTextResults.FirstOrDefault(o => o.RLT_ParentId == langText.RLT_ParentId && o.RLT_Language == SharedConstants.Languages.English);
						if (captionOnEnglish != null)
						{
							englishText = captionOnEnglish.RLT_Text;
						}
						else
						{
							englishText = result.FirstOrDefault(x => x.ResourceKey == langText.RLT_ParentId.ToString() && x.Language == SharedConstants.Languages.English);
						}
					}
					result.Add(new MultilingualLanguageText(langText.RLT_ParentId.ToString(), langText.RLT_ParentTableCode, langText.RLT_ColumnName, englishText, langText.RLT_Text, langText.RLT_Language, factory));
				}
			}

			return new HashSet<MultilingualLanguageText>(result, new MultilingualLanguageText.MultilanguageTextKeyEqualityComparer());
		}
	}
}
