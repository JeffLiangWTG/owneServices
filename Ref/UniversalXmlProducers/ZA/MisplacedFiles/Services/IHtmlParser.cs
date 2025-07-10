using System;
using HtmlAgilityPack;

namespace ZAReferenceData.Services
{
	public interface IHtmlParser
	{
		string GetUpdateDate(string url, string nodeName);
	}
}
