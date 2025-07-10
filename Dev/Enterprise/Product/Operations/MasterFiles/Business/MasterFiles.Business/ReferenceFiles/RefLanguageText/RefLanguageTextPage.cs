using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageTextPage : NonPersistentBusinessObject
	{
		readonly string tableCode;
		readonly string columnName;
		readonly BusinessObjectFactory localFactory;

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public RefLanguageTextPage(string tableCode, string columnName, string initialValue, BusinessObject businessObj, string pageDescription, string[] filterColumns)
		{
			this.columnName = columnName;
			this.tableCode = tableCode;

			localFactory = businessObj.Factory;
			Description = pageDescription;

			var allEntries = new RefLanguageTextPageEntryCollection();
			var resourceKey = businessObj[businessObj.PKSchemaColumn];

			var runtimeCaptions = GetRuntimeCaptions(businessObj, filterColumns);
			var entries = CreateEntries(AllLanguages, runtimeCaptions);

			allEntries.AddRange(entries);

			RegisterEditableChildObject(allEntries);
			all = allEntries;
			allTranslationsOfCurrentValue = new RefLanguageTextPageEntryCollection();
			allValuesInCurrentLanguage = new RefLanguageTextPageEntryCollection();

			CurrentLanguage = Res.IsSystemDefinedEnglish(Res.CurrentLanguage) ? AllLanguages.First() : Res.CurrentLanguage;
			if (!string.IsNullOrEmpty(initialValue))
			{
				CurrentCaption = new MultilingualLanguageText(resourceKey.ToString(), tableCode, columnName, initialValue, localFactory);
			}
			else
			{
				CurrentCaption = All[0].Caption;
			}
		}
		IEnumerable<RefLanguageTextPageEntry> CreateEntries(IEnumerable<string> languages, IEnumerable<MultilingualLanguageText> captions)
		{
			foreach (var groupCaptions in captions.OrderBy(x => x.EnglishText).GroupBy(x => x.ResourceKey))
			{
				foreach (var lang in languages.Where(l => ResourceStrings.Normalize(l) != Res.DefaultLanguage))
				{
					var captionInTheLanguage = captions.FirstOrDefault(x => x.Language == lang && x.ResourceKey == groupCaptions.Key);
					if (captionInTheLanguage != null)
					{
						yield return new RefLanguageTextPageEntry(lang, captionInTheLanguage);
					}
					else
					{
						var captionInEnglish = captions.FirstOrDefault(x => x.Language == SharedConstants.Languages.English && x.ResourceKey == groupCaptions.Key);
						yield return new RefLanguageTextPageEntry(lang, captionInEnglish);
					}
				}
			}
		}

		IEnumerable<MultilingualLanguageText> GetRuntimeCaptions(BusinessObject businessObj, string[] filterColumns)
		{
			return RefLanguageText.GetRuntimeLanguageCaptions(localFactory, columnName, tableCode, businessObj, filterColumns);
		}

		public RefLanguageTextPageEntryCollection AllTranslationsOfCurrentValue
		{
			get { return allTranslationsOfCurrentValue; }
		}
		readonly RefLanguageTextPageEntryCollection allTranslationsOfCurrentValue;

		public RefLanguageTextPageEntryCollection AllValuesInCurrentLanguage
		{
			get { return allValuesInCurrentLanguage; }
		}
		readonly RefLanguageTextPageEntryCollection allValuesInCurrentLanguage;
		public RefLanguageTextPageEntryCollection All
		{
			get { return all; }
		}
		readonly RefLanguageTextPageEntryCollection all;

		public ZString Description { get; private set; }

		public HashSet<string> AllLanguages
		{
			get
			{
				if (allLanguages == null)
				{
					allLanguages = new HashSet<string>();
					var languages = LanguageHelper.GetAllActiveLanguages();
					foreach (var lang in languages)
					{
						allLanguages.Add(lang.FullLanguageCode);
					}
				}
				return allLanguages;
			}
		}
		HashSet<string> allLanguages;

		public ZString CurrentLanguage
		{
			get { return currentLanguage; }
			set
			{
				if (value != currentLanguage)
				{
					currentLanguage = value;
					AllValuesInCurrentLanguage.RemoveAll();
					AllValuesInCurrentLanguage.AddRange(All.Where(entry => entry.Language == currentLanguage));
				}
			}
		}
		ZString currentLanguage;

		public MultilingualLanguageText CurrentCaption
		{
			get { return currentCaption; }
			set
			{
				if (value != currentCaption)
				{
					currentCaption = value;
					AllTranslationsOfCurrentValue.RemoveAll();
					AllTranslationsOfCurrentValue.AddRange(All.Where(entry => entry.Caption.ResourceKey == currentCaption.ResourceKey));
				}
			}
		}
		MultilingualLanguageText currentCaption;

		#region Import & Export

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant entries")]
		public Dictionary<string, ZStringBuilder> ExportEntries()
		{
			var dictionary = new Dictionary<string, ZStringBuilder>();

			foreach (RefLanguageTextPageEntry entry in All)
			{
				ZStringBuilder sb;
				if (!dictionary.TryGetValue(entry.Language, out sb))
				{
					var header = new OCsvLine(new[] { "Language", "Original", "Language", "Translation" });

					sb = new ZStringBuilder();
					sb.Append(header.ToStringWithNewLine());
					dictionary.Add(entry.Language, sb);
				}

				var line = new OCsvLine(new string[] { Res.DefaultLanguage, entry.English.Replace("\t", ""), entry.Language, entry.Translation.Replace("\t", "") });

				sb.Append(line.ToStringWithNewLine());
			}

			return dictionary;
		}

		public virtual string ImportEntries(string filename)
		{
			if (ReadOnly)
			{
				return Res.GetString("FD5064B8-1209-4D41-B95F-D7B430143453", "Editing is not allowed");
			}

			try
			{
				using (StreamReader reader = new StreamReader(File.OpenRead(filename)))
				{
					string csvLineRaw;
					int index = 0;
					var entriesPerLang = new Dictionary<string, IEnumerable<RefLanguageTextPageEntry>>();

					while ((csvLineRaw = ImportWizard.GetNextLine(reader)) != null)
					{
						if (index++ == 0)
						{
							continue;
						}

						var csvEntry = ParseLine(csvLineRaw);

						if (csvEntry == null)
						{
							return Res.GetString("CE8D4C1B-D3CD-43F1-9481-70C91F2463BB", "Unable to import {0}", filename);
						}

						IEnumerable<RefLanguageTextPageEntry> entries;
						var language = csvEntry.Language;

						if (!entriesPerLang.TryGetValue(language, out entries))
						{
							entries = All.Cast<RefLanguageTextPageEntry>().Where(e => e.Language.EqualsIgnoringCase(language));
							entriesPerLang.Add(language, entries);
						}

						var entry = entries.FirstOrDefault(e => e.English.EqualsIgnoringCase(csvEntry.Original));
						if (entry != null && !entry.Translation.Equals(csvEntry.Translation))
						{
							entry.Translation = csvEntry.Translation;
						}
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return Res.GetString("F6E16714-0FAF-4EBB-AA27-815E8BF76758", "Unable to import {0}. Error: {1}",
					filename, e.Message);
			}

			return string.Empty;
		}

		CsvEntry ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return null;
			}

			var csvLine = new OCsvLine(line);

			if (csvLine.FieldValues.Length != 4)
			{
				return null;
			}

			return new CsvEntry(csvLine.FieldValues[1], csvLine.FieldValues[2], csvLine.FieldValues[3]);
		}

		class CsvEntry
		{
			public CsvEntry(ZString original, ZString language, ZString translation)
			{
				Original = original;
				Language = language;
				Translation = translation;
			}

			public ZString Original { get; }
			public ZString Language { get; }
			public ZString Translation { get; }
		}

		#endregion

		public void Save()
		{
			var saveFactory = new BusinessObjectFactory();
			foreach (RefLanguageTextPageEntry entry in All)
			{
				if (entry.HasChanges)
				{
					var key = entry.Caption.ResourceKey;
					var match = RefLanguageText.GetByParentPKAndLanguage(saveFactory, key, entry.Language, tableCode, columnName);
					if (match == null)
					{
						match = saveFactory.New<RefLanguageText>();
						match.RLT_ColumnName = columnName;
						match.RLT_Language = entry.Language;
						match.RLT_ParentId = new Guid(entry.Caption.ResourceKey);
						match.RLT_ParentTableCode = tableCode;
					}

					match.RLT_IsSystem = false;
					match.RLT_IsClientOverridden = false;

					match.RLT_Text = entry.Translation;
					match.Factory.Save();
				}
			}
		}
	}
}
