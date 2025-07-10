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
	public class AdditionalCodesUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.AdditionalCodesFileName;
				yield return ApplicationConfig.Instance.VATAdditionalCodesFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRAdditionalCodesOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.AdditionalCodesFileName));

			var codesToExclude = GetVatCanaList();

			XmlNodeList additionalCodeList = xmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode additionalCode in additionalCodeList)
			{
				var code = UniversalDataHelper.GetTagValue(additionalCode, "CHAMP1");

				if (codesToExclude.Contains(code) || UniversalDataHelper.GetTagValue(additionalCode, "CHAMP3") != "2")
				{
					continue;
				}

				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(additionalCode, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(additionalCode, "CHAMP4");
				var endDate = UniversalDataHelper.GetEndDateFromTag(additionalCode, "CHAMP5");
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
				});
			}

			return result.GroupBy(a => a.ZZD_Code).Select(a => a.OrderByDescending(x => x.ZZD_StartDate).First()).ToList();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ADDCD");
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			return xmlWriterConfiguration;
		}

		protected virtual string[] GetVatCanaList()
		{
			var result = new List<string>();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.VATAdditionalCodesFileName));
			XmlNodeList additionalCodeList = xmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode VATadditionalCode in additionalCodeList)
			{
				result.Add(UniversalDataHelper.GetTagValue(VATadditionalCode, "CHAMP1"));
			}

			return result.ToArray();
		}

		public override string DataSource => "FR - Additional Codes";
	}
}
