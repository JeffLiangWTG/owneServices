using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MultilingualLanguageText : MultilingualString
	{
		readonly ZString _column;
		readonly ZString _englishText;
		public string EnglishText => _englishText;

		public string ResourceKey { get; }
		public string TableCode { get; }
		public string Translation { get; }
		public string Language { get; }

		public MultilingualLanguageText(string resourceKey, ZString tableCode, ZString column, ZString englishText, BusinessObjectFactory boFactory)
			: this(resourceKey, tableCode, column, englishText, englishText, SharedConstants.Languages.English, boFactory)
		{
		}

		public MultilingualLanguageText(string resourceKey, string tableCode, string column, string englishText, string translation, string language, BusinessObjectFactory boFactory)
		{
			_column = column;
			_englishText = englishText;
			ResourceKey = resourceKey;
			TableCode = tableCode;
			Translation = translation;
			Language = language;
			BoFactory = boFactory;
		}

		public override string GetUnresolvedString()
		{
			return _englishText;
		}

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		BusinessObjectFactory BoFactory { get; }

		public static MultilingualLanguageText GetMultilingualText(string resourceKey, ZString tableCode, ZString column, ZString englishText, BusinessObjectFactory boFactory)
		{
			return new MultilingualLanguageText(resourceKey, tableCode, column, englishText, boFactory);
		}

		public override string ToString(string language)
		{
			if (string.IsNullOrEmpty(ResourceKey) || language == SharedConstants.Languages.English)
			{
				return _englishText;
			}
			var translation = RefLanguageText.GetTranslation(BoFactory, language, _column, TableCode, ResourceKey);
			return !string.IsNullOrEmpty(translation) ? translation : _englishText;
		}

		public override string ToString()
		{
			return ToString(Res.CurrentLanguage);
		}

		public class MultilanguageTextKeyEqualityComparer : IEqualityComparer<MultilingualLanguageText>
		{
			public bool Equals(MultilingualLanguageText x, MultilingualLanguageText y)
			{
				if (x == null)
				{
					return y == null;
				}

				if (y == null)
				{
					return false;
				}

				return x.ResourceKey == y.ResourceKey && x.Language == y.Language;
			}

			public int GetHashCode(MultilingualLanguageText obj)
			{
				if (string.IsNullOrEmpty(obj?.ResourceKey))
				{
					return 0;
				}

				return obj.ResourceKey.GetHashCode();
			}
		}
	}
}
