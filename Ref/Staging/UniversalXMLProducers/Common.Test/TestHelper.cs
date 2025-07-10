using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	internal static class TestHelper
	{
		internal static bool SimulateDownload(string fileName, string resourceDetails)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
			using (var writer = new FileStream(fileName, FileMode.Create))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);

				writer.Flush();
				return true;
			}
		}

		internal static string SimulateNavigateWebPageByXpath(IWebDriverHelper webDriverHelper, string remoteUrl, string xPath)
		{
			var result = string.Empty;
			webDriverHelper.GetWebPage(remoteUrl, 1);
			result = webDriverHelper.NavigateWebPageByXpath(xPath, 1);

			return result;
		}
	}
}
