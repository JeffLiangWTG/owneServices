using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;


namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class ProcessManager
	{
		public void RunProcess(string outputPath, StringBuilder errorCollector)
		{
			try
			{
				var processors = GetProcessors();
				if (processors != null && processors.Any())
				{
					var fileList = GetFileManager().GetFiles();

					var referenceData = new List<ITariffModel>();

					foreach (var loader in GetLoaders())
					{
						referenceData.AddRange(loader.LoadData(fileList, errorCollector));
					}

					var chaperSpecific = processors.Where(x => x.IsChapterSpecific);
					var nonChaperSpecific = processors.Where(x => !x.IsChapterSpecific);

					DoProcessing(nonChaperSpecific, string.Empty, fileList, errorCollector, ref referenceData, outputPath);

					foreach (var chapter in Chapters)
					{
						DoProcessing(chaperSpecific, chapter, fileList, errorCollector, ref referenceData, outputPath);

						referenceData.RemoveAll(x => x.IsChapterSpecific);
					}
				}
				else
				{
					errorCollector.Append("No processors available for processing");
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Processing failed", ex);
			}

			ThrowInvalidSourceDataExceptionIfRequested(errorCollector);
		}

		static void DoProcessing(IEnumerable<IProcessor> processors, string chapter, IReadOnlyCollection<IFileDetails> fileList, StringBuilder errorCollector, ref List<ITariffModel> referenceData, string outputPath)
		{
			foreach (var loader in processors)
			{
				loader.LoadData(chapter, fileList, errorCollector);
				referenceData.AddRange(loader.Models);
			}

			foreach (var processor in processors)
			{
				processor.UpdateModels(chapter, referenceData, errorCollector);
			}

			foreach (var processor in processors)
			{
				processor.ProcessChapter(chapter, outputPath);
			}
		}

		void ThrowInvalidSourceDataExceptionIfRequested(StringBuilder errorCollector)
		{
			var errors = GetInvalidSourceDataMessages(errorCollector).ToList();
			if (errors.Count > 0)
			{
				throw new InvalidSourceDataException(SourceDataProvider, Team, errors);
			}
		}

		static List<string> GetInvalidSourceDataMessages(StringBuilder errorCollector)
		{
			var result = new List<string>();
			var messages = errorCollector.ToString();
			var span = messages.AsSpan();
			const string findPattern = "{InvalidSourceData:";
			int startIndex;
			while ((startIndex = span.IndexOf(findPattern)) >= 0)
			{
				var endIndex = span.IndexOf('}');
				result.Add(new string(span.Slice(startIndex + findPattern.Length, endIndex - startIndex - findPattern.Length)));

				if (endIndex < 0 || endIndex + 1 == span.Length)
				{
					span = ReadOnlySpan<char>.Empty;
				}
				else
				{
					span = span.Slice(endIndex + 1);
				}
			}
			return result;
		}

		public string SourceDataProvider => SourceDataProviderCore;
		public virtual string SourceDataProviderCore => "the data provider";
		public string Team => TeamCore;
		public virtual string TeamCore => "the customs team";

		protected virtual string[] Chapters => new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }; // Split into 10's instead of 100s for now until performance tests are done

		protected abstract IFileManager GetFileManager();
		protected abstract IProcessor[] GetProcessors();
		protected abstract ILoader[] GetLoaders();
	}
}
