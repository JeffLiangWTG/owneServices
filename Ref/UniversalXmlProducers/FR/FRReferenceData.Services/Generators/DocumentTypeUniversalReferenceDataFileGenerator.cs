using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class DocumentTypeUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.DocumentTypeFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRDocumentTypeOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();
			var permitDocumentTypes = ApplicationConfig.Instance.PermitDocumentTypes.Split(new char[] { ',' });
			var oDSDocumentTypes = ApplicationConfig.Instance.ODSDocumentTypes.Split(new char[] { ',' });


			XmlDocument documentTypeXmlDocument = new XmlDocument();
			documentTypeXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.DocumentTypeFileName));

			XmlNodeList documentTypeList = documentTypeXmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode documentType in documentTypeList)
			{
				var code = UniversalDataHelper.GetTagValue(documentType, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(documentType, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(documentType, "CHAMP5");
				var endDate = UniversalDataHelper.GetEndDateFromTag(documentType, "CHAMP6");
				if (!UniversalDataHelper.CheckDatesAreValid(startDate, endDate))
				{
					continue;
				}

				var isPermit = permitDocumentTypes.Contains(code);
				var isODS = oDSDocumentTypes.Contains(code);
				var attributes = GetDocumentTypesAttributes(UniversalDataHelper.GetTagValue(documentType, "CHAMP3"), UniversalDataHelper.GetTagValue(documentType, "CHAMP4"), isPermit, isODS);
				result.Add(new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = description,
					ZZD_StartDate = startDate,
					ZZD_EndDate = endDate,
					ZZD_ZZK_NKCodeType = "DC44I",
					RefCusCodeListAttributes = attributes,
				});

				result.Add(new RefCusCodeList()
				{
					ZZD_Code = code,
					ZZD_Description = description,
					ZZD_StartDate = startDate,
					ZZD_EndDate = endDate,
					ZZD_ZZK_NKCodeType = "DC44E",
					RefCusCodeListAttributes = attributes,
				});
			}

			return result.GroupBy(a => new { a.ZZD_Code, a.ZZD_ZZK_NKCodeType }).Select(a => a.OrderByDescending(x => x.ZZD_StartDate).FirstOrDefault(y => y.ZZD_StartDate <= publicationDate) ?? a.OrderByDescending(x => x.ZZD_StartDate).First()).ToList();
		}

		static RefCusCodeListAttribute[] GetDocumentTypesAttributes(string isDTP, string isD48, bool isPermit, bool isODS)
		{
			var result = new List<RefCusCodeListAttribute>();

			if (isD48 == "1")
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("IsD48", UniversalDataHelper.Constants.Yes));
			}

			if (isDTP == "0")
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("IsDTP", UniversalDataHelper.Constants.Yes));
			}

			if (isPermit)
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("PERMIT", UniversalDataHelper.Constants.Yes));
			}

			if (isODS)
			{
				result.Add(UniversalDataHelper.CreateRefCusCodeListAttribute("IsODS", UniversalDataHelper.Constants.Yes));
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
			codeList.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			codeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Document Types";
	}
}
