using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class BuilderBase<T> : IRefXmlBuilder
		where T : RefDataRepoModelEntityType
	{
		protected BuilderBase(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector)
		{
			if (dateTimeProvider == null)
			{
				throw new ArgumentNullException(nameof(dateTimeProvider));
			}

			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}

			ErrorCollector = errorCollector;
			this.dateTimeProvider = dateTimeProvider;
		}

		public void BuildXml(DateTime publicationDate, List<ITariffModel> data, string outputPath, string chapterFilter)
		{
			var refModels = ConvertToRefModels(data);
			var uniqueItems = new HashSet<string>();
			var content = new List<T>();

			foreach (var refModel in refModels)
			{
				if (IsValid(refModel, chapterFilter) && !IsExpired(refModel))
				{
					var uniqueId = UniqueId(refModel);
					if (!uniqueItems.Contains(uniqueId))
					{
						uniqueItems.Add(uniqueId);
						content.Add(refModel);
					}
					else
					{
						DuplicateError(refModel, uniqueId, chapterFilter);
					}
				}
			}

			if (content.Any())
			{
				FileNumber++;
				var filter = string.IsNullOrEmpty(chapterFilter) ? string.Empty : $"_Filter_{chapterFilter}";
				var dataSource = $"{XMLWriterDataSource}{filter}";
				Helper.ExportToXMLFile(dataSource.Substring(0, Math.Min(dataSource.Length, 75)), Path.Combine(outputPath, OutputFileName), XmlWriterConfiguration(), publicationDate, UpdateType.Full, content);
			}
		}

		protected int FileNumber { get; set; }
		protected string OutputFileName => Invariant($"{FilePrefix}_{FileNumber:00000}_{dateTimeProvider.UTCDateTime:HHmmssfff}.xml");

		protected IDateTimeProvider dateTimeProvider { get; set; }
		public StringBuilder ErrorCollector { get; private set; }

		protected abstract string FilePrefix { get; }
		protected abstract string XMLWriterDataSource { get; }

		protected abstract XmlWriterConfiguration XmlWriterConfiguration();
		protected abstract IEnumerable<T> ConvertToRefModels(List<ITariffModel> data);

		protected abstract bool IsValid(T refModel, string chapterFilter);
		protected abstract bool IsExpired(T refModel);
		protected abstract void DuplicateError(T refModel, string uniqueId, string chapterFilter);
		protected abstract string UniqueId(T refModel);
	}
}
