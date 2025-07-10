using System;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public static class Utils
	{
		public static string GetLastDayInMonthDateString(int year, int month)
		{
			int days = DateTime.DaysInMonth(year, month);
			return string.Join("", year, month.ToString("00", CultureInfo.InvariantCulture), days);
		}

		public static HttpClient GetClient()
		{
			var client = new HttpClient();
			var headers = client.DefaultRequestHeaders;
			headers.Add(HttpRequestHeader.UserAgent.ToString(), "Wisetech Tools");
			return client;
		}

		public static DateTime GetFirstDayInMonthDate(string yearMonthDay)
		{
			var year = int.Parse(yearMonthDay.Substring(0, 4), CultureInfo.InvariantCulture);
			var month = int.Parse(yearMonthDay.Substring(4, 2), CultureInfo.InvariantCulture);
			return new DateTime(year, month, 1);
		}

		public static DateTime CreateDateTime(string yearMonthDay)
		{
			var year = int.Parse(yearMonthDay.Substring(0, 4), CultureInfo.InvariantCulture);
			var month = int.Parse(yearMonthDay.Substring(4, 2), CultureInfo.InvariantCulture);
			var day = int.Parse(yearMonthDay.Substring(6, 2), CultureInfo.InvariantCulture);
			return new DateTime(year, month, day);
		}

		public static Uri BuildUri(Uri baseUri, string pathWithQueryString)
		{
			return baseUri.AbsoluteUri.EndsWith('/')
				? new Uri(baseUri, pathWithQueryString)
				: new Uri(baseUri.AbsoluteUri + "/" + pathWithQueryString);
		}
	}
}
