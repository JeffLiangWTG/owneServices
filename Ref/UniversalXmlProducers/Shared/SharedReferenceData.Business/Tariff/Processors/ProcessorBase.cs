using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public abstract class ProcessorBase<T> : IProcessor
		where T : ITariffModel
	{
		protected ProcessorBase(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders, IProcessorLoader loader)
		{
			if (builders == null)
			{
				throw new ArgumentException("builders cannot be null");
			}
			this.builders = builders;

			if (dateTimeProvider == null)
			{
				throw new ArgumentException("dateTimeProvider cannot be null");
			}
			this.DateTimeProvider = dateTimeProvider;

			if (loader == null)
			{
				throw new ArgumentException("loader cannot be null");
			}
			this.loader = loader;
		}
		IProcessorLoader loader;

		public List<ITariffModel> Models { get; protected set; }

		public void LoadData(string chapterFilter, IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector)
		{
			Models = loader.ProcessXml(chapterFilter, files, errorCollector, (file) =>
			{
				ExecutionDate = file.ExecutionDate;
			});
		}

		public void ProcessChapter(string chapterFilter, string outputPath)
		{
			if (Models?.Any() ?? false)
			{
				foreach (var builder in builders)
				{
					builder.BuildXml(ExecutionDate, Models, outputPath, chapterFilter);
				}
			}
		}

		public void UpdateModels(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
		{
			if (Models?.Any() ?? false)
			{
				UpdateModelsCore(chapterFilter, referenceData, errorCollector);
			}
		}

		public virtual bool IsChapterSpecific => true;

		protected virtual void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector) { }

		protected IDateTimeProvider DateTimeProvider { get; set; }
		protected IRefXmlBuilder[] builders { get; set; }
		protected DateTime ExecutionDate { get; set; }
	}
}
