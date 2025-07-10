using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class SpecialMentionUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.SpecialMentionFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRSpecialMentionOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			XmlDocument specialMentionXmlDocument = new XmlDocument();
			specialMentionXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.SpecialMentionFileName));

			XmlNodeList specialMentionList = specialMentionXmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode specialMention in specialMentionList)
			{
				var code = UniversalDataHelper.GetTagValue(specialMention, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(specialMention, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(specialMention, "CHAMP3");
				var endDate = UniversalDataHelper.GetEndDateFromTag(specialMention, "CHAMP4");
				if (!UniversalDataHelper.CheckDatesAreValid(startDate, endDate))
				{
					continue;
				}

				result.Add(new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = description,
					ZZD_StartDate = startDate,
					ZZD_EndDate = endDate,
					RefCusCodeListAttributes = GetSpecialMentionsAttributes(specialMention)
				});
			}

			return result.GroupBy(a => a.ZZD_Code).Select(a => a.OrderByDescending(x => x.ZZD_StartDate).First()).ToList();
		}

		static RefCusCodeListAttribute[] GetSpecialMentionsAttributes(XmlNode specialMention)
		{
			var result = new List<RefCusCodeListAttribute>();

			var impExpType = UniversalDataHelper.GetTagValue(specialMention, "CHAMP5");

			if (impExpType == "0" || impExpType == "2")
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("Direction", "IMPORT"));
			}

			if (impExpType == "1" || impExpType == "2")
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("Direction", "EXPORT"));
			}

			return result.ToArray();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ADDIN");
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			codeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Special Mentions";
	}
}
