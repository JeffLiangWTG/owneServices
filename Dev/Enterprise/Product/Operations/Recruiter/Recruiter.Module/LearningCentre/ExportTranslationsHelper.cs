using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Module
{
	public class ExportTranslationsHelper
	{
		public string Run(string directoryFolder, IEnumerable<LearningCentreCampaign> learningCentreCampaigns)
		{
			var errors = new List<string>();

			foreach (var campaign in learningCentreCampaigns)
			{
				var export = GetTranslation(campaign);

				if (export.Count > 0)
				{
					var campaignSubFolder = GetCampaignSubFolder(directoryFolder, campaign.G0_CampaignID);

					foreach (var entry in export)
					{
						var filename = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}.csv", entry.Key); // file extension
						var path = Path.Combine(campaignSubFolder, filename);

						if (!SaveFile(path, entry.Value.ToString()))
						{
							errors.Add(filename);
						}
					}
				}
			}

			var message = Res.GetString("58e3a138-de03-4e7d-b37c-a9f74bf18c2f", "Export has been completed. The files can be found at {0}.", directoryFolder);
			if (errors.Count > 0)
			{
				message += " ";
				message += Res.GetString("47e6c3d7-7f14-45c1-85db-60e39d713e59", "The files for the following languages were not saved: {0}.", string.Join(", ", errors.ToArray()));
			}

			return message;
		}

		protected virtual string GetCampaignSubFolder(string directoryFolder, string campaignID)
		{
			var campaignFolder = Path.Combine(directoryFolder, campaignID);
			if (!Directory.Exists(campaignFolder))
			{
				Directory.CreateDirectory(campaignFolder);
			}
			return campaignFolder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant entries")]
		Dictionary<string, ZStringBuilder> GetTranslation(LearningCentreCampaign businessEntity)
		{
			var dictionary = new Dictionary<string, ZStringBuilder>();

			var allLanguages = new HashSet<string>();
			var languages = LanguageHelper.GetAllActiveLanguages();
			foreach (var language in languages)
			{
				allLanguages.Add(language.FullLanguageCode);
			}

			var infos = new List<ZPropertyInfo>();
			infos.Add(businessEntity.G0_CampaignNameInfo);
			infos.Add(businessEntity.G0_CampaignCommentInfo);

			infos.AddRange(businessEntity.Questions.Select(q => q.HY_QuestionInfo));
			infos.AddRange(businessEntity.Questions.SelectMany(quest => quest.SubQuestions).Select(q => q.HY_QuestionInfo));

			var source = LoadSource(infos, businessEntity.G0_CampaignNameMultilingual);

			foreach (var entry in source.GetRuntimeCaptions().SelectMany(caption => CreateEntries((ResourceString)caption, allLanguages)))
			{
				if (!string.IsNullOrEmpty(entry.Translation))
				{
					if (!dictionary.TryGetValue(entry.Language, out var sb))
					{
						var header = new OCsvLine(new[] { "Language", "Original", "Language", "Translation" });

						sb = new ZStringBuilder();
						sb.Append(header.ToStringWithNewLine());
						dictionary.Add(entry.Language, sb);
					}

					var line = new OCsvLine(new string[] { Res.DefaultLanguage, entry.English.Replace("\t", ""), entry.Language, entry.Translation.Replace("\t", "") });
					sb.Append(line.ToStringWithNewLine());
				}
			}

			return dictionary;
		}

		protected virtual ICustomizableDataCaptionSource LoadSource(List<ZPropertyInfo> infos, MultilingualString campaignNameMultilingual)
		{
			return new MultipleDataCaptionSource(infos.ToArray(), campaignNameMultilingual);
		}

		IEnumerable<CustomizableDataTranslationEntry> CreateEntries(ResourceString caption, IEnumerable<string> languages)
		{
			return languages
				.Where(language => ResourceStrings.Normalize(language) != Res.DefaultLanguage)
				.Select(language => new CustomizableDataTranslationEntry(language, caption));
		}

		protected virtual bool SaveFile(string path, string content)
		{
			var retryCount = 0;
			var result = false;
			while (retryCount < 3)
			{
				try
				{
					File.WriteAllText(path, content, Encoding.UTF8);
					result = true;
					break;
				}
				catch (IOException)
				{
					retryCount++;
					if (retryCount == 3)
					{
						break;
					}
				}
				Thread.Sleep(100);
			}

			return result;
		}

		class CustomizableDataTranslationEntry
		{
			public CustomizableDataTranslationEntry(ZString language, ResourceString caption)
			{
				this.Language = language;
				this.caption = caption;
				this.English = caption.ToStringWithParameters(Res.DefaultLanguage);
				this.Translation = ToStringWithParameters(language);
			}

			string ToStringWithParameters(ZString language)
			{
				if (string.IsNullOrEmpty(Caption.ResourceKey))
				{
					return Caption.EnglishText;
				}

				var result = Res.GetLanguageInstance(language).GetString(Caption.Asmid, Caption.ResourceKey);
				return string.IsNullOrEmpty(result) ? string.Empty : result;
			}

			public ResourceString Caption
			{
				get { return caption; }
			}
			readonly ResourceString caption;

			public ZString Language
			{
				get; set;
			}

			public ZString English
			{
				get; set;
			}

			public ZString Translation
			{
				get; set;
			}
		}
	}
}
