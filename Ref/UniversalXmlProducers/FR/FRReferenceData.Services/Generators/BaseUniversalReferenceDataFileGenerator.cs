using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class BaseUniversalReferenceDataFileGenerator<T> : IUniversalReferenceDataFileGenerator where T : RefDataRepoModelEntityType
	{
		protected BaseUniversalReferenceDataFileGenerator()
		{
			xmlWriter = new XmlWriter(GetXmlWriterConfigurationMain());
		}

		public virtual bool GenerateFiles(DateTime publicationDate, ref Errors error)
		{
			error = Errors.No;
			Console.Write($"Collecting {DataSource} from FR Customs...");
			var dataCollection = GetDataCollection(publicationDate, ref error);
			if (!dataCollection.Any())
			{
				ManageExceptionIfNoResult();
			}
			var dependencies = GetDependencies(publicationDate);
			InitializeWriter(publicationDate, DataSource, dependencies);
			ExportToXml(dataCollection, Path.Combine(ApplicationConfig.Instance.OutputDirectory, OutputFile));
			Console.WriteLine($"Collecting {DataSource} Done.");
			return error == Errors.No;
		}

		public virtual void ManageExceptionIfNoResult() => UniversalDataHelper.SendEmail($"No {DataSource} information was found.", $"No information could be retrieved from {string.Join(", ", InputFiles)}. It's likely that this or one of those files is empty or its structure has changed.");

		public virtual IEnumerable<string> InputFiles => Enumerable.Empty<string>();

		public abstract string OutputFile { get; }

		protected abstract List<T> GetDataCollection(DateTime publicationDate, ref Errors error);

		protected void InitializeWriter(DateTime publishTime, string dataSource, Dependency[] dependencies)
		{
			UniversalDataHelper.InitializeWriter(xmlWriter, publishTime, dataSource, updateType, dependencies);
		}

		protected void ExportToXml(IEnumerable<T> collection, string filePath)
		{
			UniversalDataHelper.ExportToXml(xmlWriter, collection, filePath);
		}

		protected virtual RefCusCodeList[] GetExistingEUUOMs()
		{
			var refCusCodeListLoader = new RefCusCodeListLoader(new RefDataLoader());
			var existingEuCodes = refCusCodeListLoader.GetEuUomCodeList().Result.ToArray();
			return existingEuCodes;
		}
		protected abstract XmlWriterConfiguration GetXmlWriterConfiguration();
		protected XmlWriterConfiguration GetXmlWriterConfigurationMain() => GetXmlWriterConfiguration();

		public abstract string DataSource { get; }

		protected virtual Dependency[] GetDependencies(DateTime publicationTime) => Array.Empty<Dependency>();

		internal XmlWriter xmlWriter;

		protected virtual UpdateType updateType => UpdateType.Full;
	}
}
