using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class C44DocumentsProvider : RequestCodeProvider
	{
		protected override Encoding Encoding => Encoding.GetEncoding("ISO-8859-1");

		public enum NctsC44Type
		{
			CSRDT213,
			TRSUPNAC,
			TRSUPNCA,
			TRSUPNHO,
			TRSUPNPA
		}

		public IEnumerable<(string Code, string Description)> RequestCodes(string url, string date)
		{
			var htmlCusCodeList = new List<string>();
			var webRequestWrapper = GetWebRequestWrapper();

			var htmlContent = webRequestWrapper.GetContent($"{url}?VEZ=BUSCAR&FEC_CONSULTA={date}");
			htmlCusCodeList.Add(htmlContent);

			var emptyKeys = new string[] { "ESTADO_COLS", "VEZ" };
			var postArgs = GetPostArgsFromHtml(htmlContent, emptyKeys);

			if (!double.TryParse(postArgs["NUM_RESULTADOS_RS"], result: out double _))
			{
				throw new C44DocumentsException("Cannot continue due to empty or invalid 'NUM_RESULTADOS_RS'");
			}

			var pages = CalulateNumberOfPages(postArgs);

			postArgs["VEZ"] = "AVANZAR";
			var resultsPerPage = ResultsPerPage;
			for (int i = resultsPerPage; i < pages * resultsPerPage; i += resultsPerPage)
			{
				if (i > resultsPerPage)
				{
					PreparePostArgsForNextPage(postArgs, i);
				}

				htmlCusCodeList.Add(webRequestWrapper.GetContentFromPost(url, GetFormattedPostData(postArgs)));
			}

			var allDocuments = new List<(string Code, string Description)>();
			foreach (string htmlString in htmlCusCodeList)
			{
				GetTableElements("GESTOR", htmlString, allDocuments);
			}

			return allDocuments;
		}

		public string RequestCodesNCTS(string baseURL, string date, NctsC44Type type)
			=> RequestExportableDocument(baseURL, date, type);

		static void GetTableElements(string id, string html, List<(string Code, string Description)> allDocuments)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var nodes = doc.DocumentNode.SelectNodes($"//table[@id='{id}']//tr");
			if (nodes != null)
			{
				foreach (var row in nodes)
				{
					var tdNodes = row.SelectNodes("td");
					if (tdNodes != null && tdNodes.Count == 2)
					{
						var document = (tdNodes[0].InnerText.Replace(" ", ""), Regex.Replace(tdNodes[1].InnerText, @"\s+", " "));
						allDocuments.Add(document);
					}
				}
			}
		}

		protected override Exception GetNewExceptionToThrow(string exceptionMessage) => new C44DocumentsException(exceptionMessage);
	}
}
