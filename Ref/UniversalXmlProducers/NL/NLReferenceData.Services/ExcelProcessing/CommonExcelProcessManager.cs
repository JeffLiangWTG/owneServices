using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public abstract class CommonExcelProcessManagerAbstract<T> where T : class
	{
		public IDataBuilder<T> DataBuilder { get; }
		public IExcelParser<T> Parser { get; }
		protected abstract string ResourceContent { get; }
		protected abstract string ExcelFileName { get; }
		protected abstract string EntityName { get; }
		protected virtual DateTime PublicationTime => DateTime.Now;

		protected CommonExcelProcessManagerAbstract(IDataBuilder<T> dataBuilder, IExcelParser<T> parser)
		{
			DataBuilder = dataBuilder;
			Parser = parser;
		}

		protected CommonExcelProcessManagerAbstract()
		{
		}

		protected virtual bool SaveResourceContentToFile(string fileName, string resourceDetails)
		{
			try
			{
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceDetails))
				using (var writer = new FileStream(fileName, FileMode.Create))
				{
					stream.Seek(0, SeekOrigin.Begin);
					stream.CopyTo(writer);

					writer.Flush();
				}
				return true;
			}
			catch (ApplicationException)
			{
				return false;
			}
		}

		string WorkingFolder;

		void PrepareEnvironment()
		{
			try
			{
				WorkingFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;
			}
			catch
			{
				throw new ProcessingException($"Could not create working folder.");
			}
		}

		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				PrepareEnvironment();
				var fileName = Path.Combine(Path.GetFullPath(WorkingFolder), ExcelFileName);

				try
				{
					if (SaveResourceContentToFile(fileName, ResourceContent))
					{
						var data = Parser.ReadXlsFile(new List<string> { fileName });
						if (data != null && data.Any())
						{
							DataBuilder.BuildXml(PublicationTime, data, outputPath);
						}
						else
						{
							throw new ProcessingException("Downloaded file(s) has no code list for " + EntityName);
						}
					}
					else
					{
						throw new ProcessingException("Excel file could not be found for " + EntityName);
					}
				}
				catch (ApplicationException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{fileName}' Exception: {ex.GetBaseException().Message}");
				}
			}
			catch (Exception ex)
			{
				throw new ProcessingException("Processing failed", ex);
			}
			finally
			{
				CleanEnvironment();
			}
		}

		void CleanEnvironment()
		{
			if (Directory.Exists(WorkingFolder))
			{
				Directory.Delete(WorkingFolder, true);
			}
		}
	}
}
