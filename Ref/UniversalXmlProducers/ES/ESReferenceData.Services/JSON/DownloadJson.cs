using System.Globalization;
using System.Net;
using System.Text;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class DownloadJson
	{
		public static string Download(string url)
		{
			string json;
			try
			{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
				using (var webClient = new WebClient())
#pragma warning restore SYSLIB0014 // Type or member is obsolete
				{
					json = webClient.DownloadString(url);
				}
			}
			catch (WebException e)
			{
				var error = new StringBuilder();
				error.AppendLine(CultureInfo.InvariantCulture, $"Unable to download Json from the following URL: {url}");
				error.AppendLine(e.Message);
				throw new JSONException(error.ToString());
			}
			return json;
		}
	}
}
