using System.Globalization;
using System.IO;
using System.Text;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public abstract class CodebookProcessManager
	{
		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				var downloadLink = ApplicationConfig.DownloadUrlCodeBookDwuAangifteBehandeling;

				try
				{
					var contentFiles = DownloadManager.DownloadCodeBook(downloadLink, Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

					var codeBook = CodeBookHelper.ReadCodeBook(contentFiles);
					if (codeBook?.codeBookTables != null)
					{
						var dataTable = codeBook.codeBookTables.Find(x => x.tableNumber == TableNumber);
						if (dataTable == null || dataTable.elements.Count == 0)
						{
							errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Downloaded file does not contain a code list for {Description} ({TableNumber})");
						}
						else
						{
							Builder.BuildXml(Builder.DateTimeProvider.UTCDateTime, dataTable.elements, outputPath);
						}
					}
					else
					{
						errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Downloaded file is not a valid CodeBook for {Description}");
					}
				}
				catch (ProcessingException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{downloadLink}' Exception: {ex.GetBaseException().Message}");
				}
			}
			catch (IOException ex)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failed. Exception: {ex.GetBaseException().Message}");
			}
			catch (ProcessingException ex)
			{
				throw new ProcessingException("Processing failed", ex);
			}
		}

		public abstract string TableNumber { get; }
		public abstract string Description { get; }

		public abstract ICodebookBuilder Builder { get; }
		public abstract DownloadManager DownloadManager { get; }
	}
}
