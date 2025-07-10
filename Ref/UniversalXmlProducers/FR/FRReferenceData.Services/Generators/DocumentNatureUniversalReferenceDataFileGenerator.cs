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
	public class DocumentNatureUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.DocumentNatureFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRDocumentNatureOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			XmlDocument documentNatureXmlDocument = new XmlDocument();
			documentNatureXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.DocumentNatureFileName));

			XmlNodeList documentNatureList = documentNatureXmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode documentNature in documentNatureList)
			{
				var code = UniversalDataHelper.GetTagValue(documentNature, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(documentNature, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(documentNature, "CHAMP3");
				var endDate = UniversalDataHelper.GetEndDateFromTag(documentNature, "CHAMP4");
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
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "DC44N");
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Document Natures";
	}
}
