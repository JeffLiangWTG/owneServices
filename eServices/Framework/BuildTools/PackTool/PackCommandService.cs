using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace eServices.BuildTools.PackTool;

public partial class PackCommandService(HttpClient httpClient, ILogger<PackCommandService> logger)
{
	public void Invoke(IBuildHandler buildHandler, Uri packageSource)
	{
		buildHandler.BuildPackage();

		Version version = Version.Parse(buildHandler.PackageVersion!);
		var packPath = Path.Combine(buildHandler.PackOutputPath!, $"{buildHandler.PackageId}.{buildHandler.PackageVersion}.nupkg");
		var reportPath = Path.Combine(buildHandler.PackOutputPath!, $"{buildHandler.PackageId}.pack-report.txt");
		string reportDetails;

		FormatReleaseNotesHeader(buildHandler.ProjectFile.Directory, httpClient, out var author, out var releaseNotesHeader);

		var nugetServiceRequest = new HttpRequestMessage(HttpMethod.Get, packageSource);
		var nugetServiceResponse = httpClient.Send(nugetServiceRequest, CancellationToken.None);
		nugetServiceResponse.EnsureSuccessStatusCode();
		var nugetServiceJson = JsonNode.Parse(nugetServiceResponse.Content.ReadAsStringAsync().Result);
		var nugetSearchService = nugetServiceJson!["resources"]!.AsArray().First(r => r!["@type"]?.ToString() == "SearchQueryService")!["@id"]!.ToString();
		var nugetPackageBaseAddress = nugetServiceJson!["resources"]!.AsArray().First(r => r!["@type"]?.ToString() == "PackageBaseAddress/3.0.0")!["@id"]!.ToString();

		string? publishedInfoVersion = null;
		Version? publishedVersion = null;
		var publishedInfoRequest = new HttpRequestMessage(HttpMethod.Get, $"{nugetSearchService}?q={buildHandler.PackageId}&prerelease=false");
		var publishedInfoResponse = httpClient.Send(publishedInfoRequest, CancellationToken.None);
		if (publishedInfoResponse.IsSuccessStatusCode)
		{
			var publishedInfoJson = JsonNode.Parse(publishedInfoResponse.Content.ReadAsStringAsync().Result);
			publishedInfoVersion = publishedInfoJson!["data"]?.AsArray().FirstOrDefault(p => p?["id"]?.ToString() == buildHandler.PackageId)?["version"]?.ToString();
			if (publishedInfoVersion is not null)
			{
				publishedVersion = Version.Parse(publishedInfoVersion);
				if (publishedVersion >= version)
				{
					version = new Version(publishedVersion.Major, publishedVersion.Minor, publishedVersion.Build + 1);
				}
			}
		}
		else
		{
			logger.LogWarning("Failed to retrieve published package information. Status={StatusCode}", (int)publishedInfoResponse.StatusCode);
		}

		string? publishedPrereleaseVersion = null;
		string? publishedPrereleaseReleaseNotes = null;
		var prereleaseInfoRequest = new HttpRequestMessage(HttpMethod.Get, $"{nugetSearchService}?q={buildHandler.PackageId}&prerelease=true");
		var prereleaseInfoResponse = httpClient.Send(prereleaseInfoRequest, CancellationToken.None);
		if (prereleaseInfoResponse.IsSuccessStatusCode)
		{
			var prereleaseInfoJson = JsonNode.Parse(prereleaseInfoResponse.Content.ReadAsStringAsync().Result);
			var prereleaseInfoVersion = prereleaseInfoJson!["data"]?.AsArray().FirstOrDefault(p => p?["id"]?.ToString() == buildHandler.PackageId)?["version"]?.ToString();
			if (prereleaseInfoVersion is not null && prereleaseInfoVersion != publishedInfoVersion)
			{
				publishedPrereleaseVersion = prereleaseInfoVersion;
				var prereleaseDetailsRequest = new HttpRequestMessage(HttpMethod.Get,
					$"{nugetPackageBaseAddress}/{buildHandler.PackageId!.ToLowerInvariant()}/{publishedPrereleaseVersion.ToLowerInvariant()}/{buildHandler.PackageId.ToLowerInvariant()}.nuspec");
				var prereleaseDetailsResponse = httpClient.Send(prereleaseDetailsRequest, CancellationToken.None);
				var prereleaseInfoXml = XElement.Parse(prereleaseDetailsResponse.Content.ReadAsStringAsync().Result);
				publishedPrereleaseReleaseNotes = prereleaseInfoXml
					.XPathEvaluate("string(//*[local-name()='releaseNotes'])")
					.ToString()!.ReplaceLineEndings();
			}
		}
		else
		{
			logger.LogWarning("Failed to retrieve published prerelease package information. Status={StatusCode}", (int)prereleaseInfoResponse.StatusCode);
		}

			var publishedNupkgDir = $"{buildHandler.ProjectFile.DirectoryName}\\{buildHandler.NuspecOutputPath}Published\\";
		var compareWorkDir = $"{buildHandler.ProjectFile.DirectoryName}\\{buildHandler.NuspecOutputPath}Compare\\";
		Directory.CreateDirectory(publishedNupkgDir);

		if (publishedPrereleaseVersion is not null)
		{
			var publishedPrereleasePackPath = Path.Combine(publishedNupkgDir, $"{buildHandler.PackageId}.{publishedPrereleaseVersion}.nupkg");
			DownloadNupkg($"{nugetPackageBaseAddress}/{buildHandler.PackageId!.ToLowerInvariant()}/{publishedPrereleaseVersion.ToLowerInvariant()}/{buildHandler.PackageId.ToLowerInvariant()}.{publishedPrereleaseVersion.ToLowerInvariant()}.nupkg", publishedPrereleasePackPath);

			if (PackagesAreDifferent(publishedPrereleasePackPath, packPath, compareWorkDir, out var differenceReport))
			{
				var prereleaseMatch = RegexPrereleaseVersion().Match(publishedPrereleaseVersion);
				var prereleaseVersion = prereleaseMatch.Groups[1].Value + (int.Parse(prereleaseMatch.Groups[2].Value) + 1);
				var prereleaseReleaseNotes = $"""
					{releaseNotesHeader}
					Version:{prereleaseVersion} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
					Changes [modified=(*) added=(+) deleted=(-)]:
					{differenceReport}

					{publishedPrereleaseReleaseNotes}
					""";
				buildHandler.BuildPackageVersion(prereleaseVersion, prereleaseReleaseNotes);

				var releaseNotes = $"""
					{releaseNotesHeader}
					Version:{version} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
					Changes [modified=(*) added=(+) deleted=(-)]:
					{differenceReport}
					
					{publishedPrereleaseReleaseNotes}
					""";
				buildHandler.DeletePackage(packPath);
				buildHandler.BuildPackageVersion(version.ToString(), releaseNotes);

				reportDetails = $"""
					Published version: {publishedVersion?.ToString() ?? "N/A"}
					Published pre-release: {publishedPrereleaseVersion}
					Packed version: {version}
					Packed pre-release: {prereleaseVersion}

					{releaseNotes}
					""";
			}
			else
			{
				buildHandler.BuildPackageVersion(publishedPrereleaseVersion, publishedPrereleaseReleaseNotes!);

				var releaseNotes = publishedPrereleaseReleaseNotes!.Replace(publishedPrereleaseVersion, version.ToString());
				buildHandler.DeletePackage(packPath);
				buildHandler.BuildPackageVersion(version.ToString(), releaseNotes);

				reportDetails = $"""
					Published version: {publishedVersion?.ToString() ?? "N/A"}
					Published pre-release: {publishedPrereleaseVersion}
					Packed version: {version}
					Packed pre-release: {publishedPrereleaseVersion}
					""";
			}
		}
		else if (publishedVersion is not null)
		{
			var publishedPackPath = Path.Combine(publishedNupkgDir, $"{buildHandler.PackageId}.{publishedVersion}.nupkg");
			DownloadNupkg($"{nugetPackageBaseAddress}/{buildHandler.PackageId!.ToLowerInvariant()}/{publishedVersion.ToString().ToLowerInvariant()}/{buildHandler.PackageId!.ToLowerInvariant()}.{publishedVersion.ToString().ToLowerInvariant()}.nupkg", publishedPackPath);

			if (PackagesAreDifferent(publishedPackPath, packPath, compareWorkDir, out var differenceReport))
			{
				var prereleaseVersion = $"{version}-rc.1";
				var prereleaseReleaseNotes = $"""
					{releaseNotesHeader}
					Version:{prereleaseVersion} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
					Changes [modified=(*) added=(+) deleted=(-)]:
					{differenceReport}
					""";
				buildHandler.BuildPackageVersion(prereleaseVersion, prereleaseReleaseNotes);

				var releaseNotes = $"""
					{releaseNotesHeader}
					Version:{version} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
					Changes [modified=(*) added=(+) deleted=(-)]:
					{differenceReport}
					""";
				buildHandler.DeletePackage(packPath);
				buildHandler.BuildPackageVersion(version.ToString(), releaseNotes);

				reportDetails = $"""
					Published version: {publishedVersion}
					Published pre-release: N/A
					Packed version: {version}
					Packed pre-release: {prereleaseVersion}

					{releaseNotes}
					""";
			}
			else
			{
				reportDetails = $"""
					Published version: {publishedVersion}
					Published pre-release: N/A
					Packed version: *no differences*
					Packed pre-release: *no differences*
					""";
			}
		}
		else
		{
			var prereleaseVersion = $"{version}-rc.1";
			var prereleaseReleaseNotes = $"""
				{releaseNotesHeader}
				Version:{prereleaseVersion} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
				Initial release.
				""";
			buildHandler.BuildPackageVersion(prereleaseVersion, prereleaseReleaseNotes);

			var releaseNotes = $"""
				{releaseNotesHeader}
				Version:{version} Author:{author} PackTime:{DateTimeOffset.UtcNow:s}
				Initial release.
				""";
			buildHandler.DeletePackage(packPath);
			buildHandler.BuildPackageVersion(version.ToString(), releaseNotes);

			reportDetails = $"""
				Published version: N/A
				Published pre-release: N/A
				Packed version: {version}
				Packed pre-release: {prereleaseVersion}

				{releaseNotes}
				""";
		}

		logger.LogInformation("{ReportDetails}", reportDetails.ReplaceLineEndings("\n"));
		Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
		File.WriteAllText(reportPath, reportDetails);
	}

	internal virtual void FormatReleaseNotesHeader(DirectoryInfo? branchDir, HttpClient httpClient, out string? author, out string releaseNotesHeader)
	{
		DirectoryInfo? branchInfoDir = null;
		while (branchDir is not null && (branchInfoDir = branchDir.GetDirectories(".git").FirstOrDefault()) is null)
			branchDir = branchDir.Parent;
		if (branchInfoDir is null)
			throw new InvalidOperationException($"Branch information not found.");

		var branchRef = branchInfoDir.GetFiles("HEAD").First().OpenText().ReadToEnd().Trim()[5..];
		var returnedString = new StringBuilder(1000);
		_ = GetPrivateProfileString("remote \"origin\"", "url", "", returnedString, (uint)returnedString.Capacity, Path.Combine(branchInfoDir.FullName, "config"));
		var repoUri = new Uri(returnedString.ToString() + "/");
		var devOpsUri = new Uri(repoUri, "../../_apis/git/");
		if (branchRef == "refs/heads/master" ||
			branchRef == "refs/heads/releases/staging" ||
			branchRef == "refs/heads/releases/production")
		{
			var branchLogPath = Path.Combine(branchInfoDir.FullName, "logs", branchRef.Replace('/', '\\'));
			var branchLog = File.ReadAllLines(branchLogPath).Last();
			branchRef = "refs/heads/" + RegexBranchRef().Match(branchLog).Groups[1].Value;
		}

		var devopsRequest = new HttpRequestMessage(HttpMethod.Get, $"{devOpsUri}pullrequests?searchCriteria.sourceRefName={branchRef}");
		var devopsResponse = httpClient.Send(devopsRequest);
		if (devopsResponse.IsSuccessStatusCode
			&& JsonNode.Parse(devopsResponse.Content.ReadAsStringAsync().Result) is JsonNode pullNode
			&& pullNode?["count"]?.GetValue<int>() > 0)
		{
			var pull = pullNode["value"]![0]!;
			var pullRequestTitle = pull["title"]?.ToString();
			var pullRequestId = pull["pullRequestId"]?.ToString();
			var pullRequestUri = new Uri(repoUri, $"pullrequest/{pullRequestId}").ToString();
			author = pull["createdBy"]?["uniqueName"]?.ToString();
			releaseNotesHeader = $"""
				{pullRequestTitle}
				{pullRequestUri}
				""";
		}
		else
		{
			author = Environment.UserName;
			releaseNotesHeader = $"Branch: {branchRef}";
		}
	}

	internal virtual void DownloadNupkg(string url, string filePath)
	{
		if (!File.Exists(filePath))
		{
			var prereleaseNupkgRequest = new HttpRequestMessage(HttpMethod.Get, url);
			var prereleaseNupkgResponse = httpClient.Send(prereleaseNupkgRequest);
			prereleaseNupkgResponse.EnsureSuccessStatusCode();
			using var publishedPrereleasePackFile = File.Create(filePath);
			prereleaseNupkgResponse.Content.ReadAsStream().CopyTo(publishedPrereleasePackFile);
		}
	}

	internal virtual bool PackagesAreDifferent(string refFilePath, string difFilePath, string compareWorkDir, out string differenceReport)
	{
		List<(string Path, char Type)> differences = [];

		using (var refFileStream = File.OpenRead(refFilePath))
		using (var difFileStream = File.OpenRead(difFilePath))
		using (var refArchive = new ZipArchive(refFileStream))
		using (var difArchive = new ZipArchive(difFileStream))
		{
			var refNuspec = refArchive.Entries.First(f => Path.GetExtension(f.Name) == ".nuspec");
			var difNuspec = difArchive.Entries.First(f => Path.GetExtension(f.Name) == ".nuspec");
			using (var refNuspecStream = refNuspec.Open())
			using (var difNuspecStream = difNuspec.Open())
			using (var refNuspecRdr = new StreamReader(refNuspecStream))
			using (var difNuspecRdr = new StreamReader(difNuspecStream))
			{
				var refNuspecXml = XElement.Parse(refNuspecRdr.ReadToEnd());
				var difNuspecXml = XElement.Parse(difNuspecRdr.ReadToEnd());
				static bool excludedElementsPredicate(XElement n)
					=> n.Name.LocalName is "version" or "repository" or "releaseNotes";
				refNuspecXml.Descendants().Where(excludedElementsPredicate)?.Remove();
				difNuspecXml.Descendants().Where(excludedElementsPredicate)?.Remove();
				var result = !XNode.DeepEquals(refNuspecXml, difNuspecXml);
				if (result)
				{
					differences.Add((refNuspec.FullName, '*'));
				}
			}

			var refFiles = refArchive.Entries.Select(f => f.FullName).Where(n => !RegexIgnoredContentPathsPattern().IsMatch(n)).ToList();
			var difFiles = difArchive.Entries.Select(f => f.FullName).Where(n => !RegexIgnoredContentPathsPattern().IsMatch(n)).ToList();
			var addFiles = difFiles.Except(refFiles).ToList();
			var delFiles = refFiles.Except(difFiles).ToList();
			differences.AddRange(addFiles.Select(f => (f, '+')));
			differences.AddRange(delFiles.Select(f => (f, '-')));

			const int bufferLen = 4096;
			var refBuffer = new byte[bufferLen];
			var difBuffer = new byte[bufferLen];
			foreach (var file in refFiles.Intersect(difFiles).ToList())
			{
				var binaryDifferences = false;
				using (var refStream = refArchive.GetEntry(file)!.Open())
				using (var difStream = difArchive.GetEntry(file)!.Open())
				{
					int refRead = 0;
					int difRead = 0;
					while ((refRead = refStream.Read(refBuffer, 0, bufferLen)) > 0
						& (difRead = difStream.Read(difBuffer, 0, bufferLen)) > 0)
					{
						if (refRead != difRead || !refBuffer.Take(refRead).SequenceEqual(difBuffer.Take(difRead)))
						{
							binaryDifferences = true;
							break;
						}
					}
				}
				if (binaryDifferences)
				{
					if (Path.GetExtension(file) is ".dll" or ".exe")
					{
						logger.LogDebug("Comparing decompilation of: {file}", file);

						var refBinPath = Path.Combine(compareWorkDir, "Ref", file);
						Directory.CreateDirectory(Path.GetDirectoryName(refBinPath)!);
						var difBinPath = Path.Combine(compareWorkDir, "Dif", file);
						Directory.CreateDirectory(Path.GetDirectoryName(difBinPath)!);

						refArchive.GetEntry(file)!.ExtractToFile(refBinPath, true);
						difArchive.GetEntry(file)!.ExtractToFile(difBinPath, true);

						var refDecomp = new CSharpDecompiler(refBinPath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
						var refCs = refDecomp.DecompileWholeModuleAsString().Split(Environment.NewLine)
							.Where(l => !RegexDecompilationExclusions().IsMatch(l)).ToArray();
						var difDecomp = new CSharpDecompiler(difBinPath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
						var difCs = difDecomp.DecompileWholeModuleAsString().Split(Environment.NewLine)
							.Where(l => !RegexDecompilationExclusions().IsMatch(l)).ToArray();

						var comparer = new CompareLogic();
						comparer.Config.MaxDifferences = 10;
						var result = comparer.Compare(refCs, difCs);

						if (!result.AreEqual)
						{
							logger.LogDebug("Differences found for {file}:{result}", file, result.DifferencesString);
							differences.Add((file, '*'));
						}
					}
					else
					{
						differences.Add((file, '*'));
					}
				}
			}
		}

		differences.Sort();
		differenceReport = String.Join(Environment.NewLine, differences.Select(d => $"{d.Path} ({d.Type})"));
		return differences.Count > 0;
	}

	[GeneratedRegex(@"\tmerge origin/([\S^:]+): ")]
	private static partial Regex RegexBranchRef();
	[GeneratedRegex(@"^*.nuspec$|^\[Content_Types].xml$|^_rels/|^package/|\.pdb$", RegexOptions.Compiled)]
	private static partial Regex RegexIgnoredContentPathsPattern();

	[GeneratedRegex(@"^(\d+.\d+.\d+\-rc\.?)(\d+)$")]
	private static partial Regex RegexPrereleaseVersion();

	[GeneratedRegex(@"^\[assembly: AssemblyInformationalVersion|\[GeneratedCode\(""System.Text.RegularExpressions.Generator""|<RegexGenerator_g>")]
	private static partial Regex RegexDecompilationExclusions();

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);
}
