using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	internal static class CodeListsTestHelper
	{
		internal static string ExportDownloadURL(string customsCodeListIdentifier, string releaseVersion) => $"https://www.ausfuhrplus.internetzollanmeldung.de/iaap/codierungen/EX/{releaseVersion}/xml/{customsCodeListIdentifier}.xml";

		internal static string ImportXmlDownloadUrl(string customsCodeListIdentifier, string releaseVersion) => $"https://www.einfuhr.internetzollanmeldung.de/iza/codierungen/EINFUHR/{releaseVersion}/xml/{customsCodeListIdentifier}.xml";

		internal static string NctsDownloadUrl(string customsCodeListIdentifier, string releaseVersion) => $"https://www.versand.internetzollanmeldung.de/iva/codierungen/VER/{releaseVersion}/xml/{customsCodeListIdentifier}.xml";

		internal static string EmcsDownloadUrl(string codeListDetails) => $"http://www.zoll.de/SharedDocs/Downloads/DE/Links-fuer-Inhaltseiten/Fachthemen/Verbrauchsteuern/EMCS/Codeliste_{codeListDetails}";

		internal const string TestFilesManifestBasePath = "CargoWise.RefDbRepo.DEReferenceData.Tests.Business.CodeLists";

		internal static string TestFilesOutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DE\CodeList\");

		internal static Dictionary<string, string> EMCSDownloadCodeList(string testDownloadPage)
		{
			var site = new HtmlDocument();
			site.LoadHtml(TestHelper.ReadManifestResourceContent(testDownloadPage));
			var dynamicCodeListDownloadDetails = site.DocumentNode.SelectNodes("//a[@class='c-link is-download-link']");
			return dynamicCodeListDownloadDetails.ToDictionary(key => key.FirstChild.InnerText.Trim(), value => value.GetAttributeValue("href", string.Empty));
		}

		internal static Dictionary<string, string> ImportDownloadCodeList(string testDownloadPage)
		{
			var downloadLinks = TestHelper.ReadManifestResourceContent(testDownloadPage).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
			return downloadLinks.ToDictionary(key => Path.GetFileNameWithoutExtension(key), value => value);
		}

		internal static string[] GetImportDownloadLinks()
		{
			IEnumerable<string> result = Array.Empty<string>();
			foreach (var version in TestConstants.ImportCodeListVersions)
			{
				result = result.Union(SplitDownloadLinksFromUriRessource("Import", version), new URIComparer());
			}
			return result.ToArray();
		}

		internal static string[] GetExportDownloadLinks()
		{
			IEnumerable<string> result = Array.Empty<string>();
			foreach (var version in TestConstants.ExportCodeListVersions)
			{
				result = result.Union(SplitDownloadLinksFromUriRessource("Export", version), new URIComparer());
			}
			return result.ToArray();
		}

		internal static string[] GetNctsDownloadLinks()
		{
			IEnumerable<string> result = Array.Empty<string>();
			foreach (var version in TestConstants.NctsCodeListVersions)
			{
				result = result.Union(SplitDownloadLinksFromUriRessource("Ncts", version), new URIComparer());
			}
			return result.ToArray();
		}

		static IEnumerable<string> SplitDownloadLinksFromUriRessource(string system, string version)
			=> TestHelper.ReadManifestResourceContent($"{CodeListsTestHelper.TestFilesManifestBasePath}.{system}.TestFiles.{system.ToUpper(CultureInfo.InvariantCulture)}_{version}_CODELISTS_DOWNLOAD_LINKS.uri").Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#"));
	}
}
