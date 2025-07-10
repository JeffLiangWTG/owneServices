using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class FSBXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan FSB Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_FSBCode";

		protected override string DownloadUrl => AppConfig.Customs.CodeLists.FSBTxtFileDownloadUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new FSBRefCusCodeListParser();

		protected override bool GetRecords(string filePath, out IList<string> records)
		{
			return TxtReaderHelper.TryRead(filePath, out records);
		}

		protected override bool GetPublicationDate(IHttpClientHelper httpClientHelper, out DateTime publicationDate)
		{
			return CustomsPublicationDateScraper.TryGetPublicationDate(httpClientHelper, AppConfig.Customs.CodeLists.BaseUrl, DownloadUrl, out publicationDate);
		}
	}
}
