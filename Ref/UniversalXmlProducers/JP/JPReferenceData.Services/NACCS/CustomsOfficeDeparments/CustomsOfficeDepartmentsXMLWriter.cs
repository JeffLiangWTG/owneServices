using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CustomsOfficeDepartmentsXMLWriter
	{
		public void WriteXml(IHttpClientHelper httpClientHelper)
		{
			var parentPageUrl = AppConfig.NACCS.CodeLists.CustomsOfficeDepartmentsFileDownloadParentUrl;
			var records = GetParser(GetDownloadUrls(parentPageUrl, httpClientHelper)).Parse(httpClientHelper, out var publishDateTime);

			var xmlWriter = new XmlWriterHelper(CustomsOfficeDepartmentsHelper.GetRefCusCodeListConfiguration(), DataSource, publishDateTime, Common.UniversalXmlWriter.UpdateType.Full, FileNameWithoutExtension);
			xmlWriter.PopulateAndSave(records);
		}

		static CustomsOfficeDepartmentsDataParser GetParser(string[] downloadUrls)
		{
			return new CustomsOfficeDepartmentsDataParser(downloadUrls);
		}

		protected virtual string[] GetDownloadUrls(string parentPageUrl, IHttpClientHelper httpClientHelper)
		{
			var htmlDocument = new HtmlDocument();
			var html = httpClientHelper.GetWebPageAsync(parentPageUrl).GetAwaiter().GetResult();

			var matches = Regex.Matches(html, UrlPattern);
			return matches.Cast<Match>().Select(c => string.Concat(AppConfig.NACCS.BaseUrl, c.Value)).ToArray();
		}

		const string DataSource = "JP Customs Office Departments";
		const string FileNameWithoutExtension = "RefCusCodeList_JP_Departments";
		const string UrlPattern = @"/naccs/dfw/web/data/code/hanyo/bumon[0-9].pdf";
	}
}
