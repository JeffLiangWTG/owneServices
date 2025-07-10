using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public abstract class AdditionalSupplementsProcessManager
	{
		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				var builder = GetAdditionalSupplementsBuilder();
				var downloadDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				var downloadLink = ApplicationConfig.DownloadUrlAdditionalSupplements;
				downloadLink = downloadLink.Replace("&amp;", "&");
				downloadLink = downloadLink.Replace("[SearchDate]", DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("nl-NL")));

				try
				{
					DownloadManagerHelper.PrepareEnvironment(downloadDir);
					this.downloadDir = Path.GetFullPath(downloadDir);

					using (var downloadManager = GetDownloadManager())
					{
						List<string> files = downloadManager.DownloadFilesAsExcel(downloadLink);
						if (files != null && files?.Count != 0)
						{
							var parser = new AdditionalSupplementsExcelParser();
							var additionalSupplementsData = parser.ReadXlsFileIntoResults(files);
							DownloadManagerHelper.RemoveExpectedFiles(files);
							if (additionalSupplementsData != null && additionalSupplementsData?.Count != 0)
							{
								var data = additionalSupplementsData.Any(x => x.Code.StartsWith("Q", StringComparison.Ordinal)) && additionalSupplementsData.Any(x => x.Code.StartsWith("U", StringComparison.Ordinal)) && additionalSupplementsData.Any(x => x.Code.StartsWith("V", StringComparison.Ordinal));
								if (data == false)
								{
									throw new ProcessingException("Downloaded file(s) does not contain a code list for all Additional Supplements Q, U and V");
								}
								builder.BuildXml(DateTime.Now, additionalSupplementsData, outputPath);
							}
							else
							{
								throw new ProcessingException("Downloaded file(s) has no code list for Additional Supplements");
							}
						}
						else
						{
							throw new ProcessingException("No Excel file(s) downloaded for Additional Supplements");
						}
					}
				}
				catch (IOException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{downloadLink}' Exception: {ex.GetBaseException().Message}");
				}
				catch (ProcessingException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{downloadLink}' Exception: {ex.GetBaseException().Message}");
				}
			}
			catch (Exception ex)
			{
				throw new ProcessingException("Processing failed", ex);
			}
		}

		public abstract IDataBuilder<AdditionalSupplementsData> GetAdditionalSupplementsBuilder();
		public abstract DownloadManager GetDownloadManager();

		protected string downloadDir { get; set; }
	}

	public class AdditionalSupplementsWebDriverHelperProcessManager : AdditionalSupplementsProcessManager
	{
		public AdditionalSupplementsWebDriverHelperProcessManager(IDataBuilder<AdditionalSupplementsData> additionalSupplementsBuilder)
		{
			AdditionalSupplementsBuilder = additionalSupplementsBuilder;
		}
		readonly IDataBuilder<AdditionalSupplementsData> AdditionalSupplementsBuilder;

		public override IDataBuilder<AdditionalSupplementsData> GetAdditionalSupplementsBuilder() => AdditionalSupplementsBuilder;
		public override DownloadManager GetDownloadManager() => new DownloadManager(new WebDriverHelperWrapper(downloadDir));
	}
}
