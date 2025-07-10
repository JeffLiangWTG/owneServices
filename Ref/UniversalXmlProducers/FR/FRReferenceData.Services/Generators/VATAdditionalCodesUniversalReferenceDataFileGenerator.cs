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
	public class VATAdditionalCodesUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.VATAdditionalCodesFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRVATAdditionalCodesOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.VATAdditionalCodesFileName));

			XmlNodeList vatAdditionalCodeList = xmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode vatAdditionalCode in vatAdditionalCodeList)
			{
				var code = UniversalDataHelper.GetTagValue(vatAdditionalCode, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(vatAdditionalCode, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(vatAdditionalCode, "CHAMP4");
				var endDate = UniversalDataHelper.GetEndDateFromTag(vatAdditionalCode, "CHAMP5");
				var attributes = GetAttributes(description);
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
					RefCusCodeListAttributes = attributes,
				});
			}

			return result.GroupBy(a => a.ZZD_Code).Select(a => a.OrderByDescending(x => x.ZZD_StartDate).First()).ToList();
		}

		static RefCusCodeListAttribute[] GetAttributes(string description)
		{
			var result = new List<RefCusCodeListAttribute>
			{
				UniversalDataHelper.CreateRefCusCodeListAttribute("VatProcedure", "AI2")
			};

			if (description.Contains("Article 275 du CGI sans dispense de visa"))
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("SpecialMention", "61000"));
			}
			else
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("SpecialMention", "60900"));
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
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "VCANA");
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			codeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - VAT Additional Codes";
	}
}
