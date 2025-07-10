using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Services
{
	public interface IDownLoadService
	{
		(bool successfullyParsed, DateTime dateTime) GetDateTimeFromHtmlNode(HtmlNode htmlNode);

		bool DownloadFile(string url, string localPath);

		bool DownloadFile(string htmlString, string censusURL, string downloadFilePath);

		HtmlNode FindNode(string url, Func<HtmlNode, bool> predicate);
		IEnumerable<HtmlNode> FindNodes(string url, Func<HtmlNode, bool> predicate);

		byte[] DownloadData(string url);
	}
}

