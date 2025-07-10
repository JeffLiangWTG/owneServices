using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public abstract class AbstractDataPurger : IDataPurger
	{
		protected AbstractDataPurger(IStagingRepository stagingRepo)
		{
			Argument.NotNull(stagingRepo, nameof(stagingRepo));
			StagingRepo = stagingRepo;
		}

		protected IStagingRepository StagingRepo { get; }

		public void Purge()
		{
			Console.WriteLine($"{GetType().Name} Start Purging");
			foreach (var source in FetchSourceData())
			{
				if (source != null)
				{
					Console.WriteLine($"Start Purging {source.SDA_PK}");
					var purgeCount = PurgeCore(source);
					Console.WriteLine($"Finish Purging {source.SDA_PK} : {purgeCount}");
				}
			}
			Console.WriteLine($"{GetType().Name} Finish Purging");
		}

		int DeleteDPIRecordsWhenParentPKIsNull(SourceData sourceData)
		{
			var result = StagingRepo.ExecuteSqlCommand($@"DELETE d
FROM {nameof(DataProcessingInformation)} d
JOIN {nameof(SourceData)} ON {nameof(SourceData.SDA_PK)} = d.{nameof(DataProcessingInformation.DPI_SourceId)} AND d.{nameof(DataProcessingInformation.DPI_ParentPk)} IS NULL
WHERE {nameof(SourceData.SDA_PK)} = '{sourceData.SDA_PK}'");

			Console.WriteLine($"{sourceData.SDA_PK} submitted: {result}");
			return result;
		}

		int PurgeCore(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			var result = 0;
			var count = int.MaxValue;
			var dpis = GetDataProcessingInformation(sourceData);
			var info = dpis.FirstOrDefault();
			if (info != null)
			{
				result += DeleteDPIRecordsWhenParentPKIsNull(sourceData);
				if (!info.DPI_ParentPk.HasValue)
				{
					info = dpis.FirstOrDefault(x => x.DPI_ParentPk.HasValue);
				}
				if (info != null)
				{
					var pkColumn = info.DPI_ParentTableCode + "_PK";
					var entityType = typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x => x.GetProperty(pkColumn) != null);
					while (count > 0)
					{
						count = StagingRepo.ExecuteSqlCommand(CreatePurgingSql(entityType, sourceData, sourceData.SDA_ContentType == DataSourceConstants.ContentType.UniversalXML));
						result += count;
						Console.WriteLine($"{sourceData.SDA_PK} submitted: {count}");
					}
				}
			}

			SetSourceDataStatus(sourceData);
			StagingRepo.SaveChanges();
			return result;
		}

		protected virtual void SetSourceDataStatus(SourceData sourceData)
		{
		}

		protected abstract IQueryable<DataProcessingInformation> GetDataProcessingInformation(SourceData sourceData);

		protected abstract string BuildInsertBatchDataProcessingInformationSql(Guid sourceid, Type rootType);

		protected abstract IEnumerable<SourceData> FetchSourceData();

		string CreatePurgingSql(Type entityType, SourceData sourceData, bool isXml)
		{
			Argument.NotNull(entityType, nameof(entityType));
			Argument.NotNull(sourceData, nameof(sourceData));

			var result = new StringBuilder();
			var entityList = new List<Tuple<Type, Type>> { Tuple.Create((Type)null, entityType) };
			if (isXml)
			{
				var entityTypeElements = PurgeHelper.GetEntityTypeElements(sourceData.SDA_ContentText);
				entityList.AddRange(GetEntitiesForPurging(entityType, entityTypeElements).Distinct());
			}
			else
			{
				var preConfiguredEntities = GetPreConfiguredEntitiesForPurging(sourceData.SDA_SubSource);
				entityList.AddRange(preConfiguredEntities);
			}
			result.AppendLine(PurgeHelper.CreatePurgingSqlWithEntity(entityList, BuildInsertBatchDataProcessingInformationSql(sourceData.SDA_PK, entityType)));
			result.AppendLine(CultureInfo.InvariantCulture, $@"DELETE dpi
FROM {nameof(DataProcessingInformation)} dpi
JOIN #{entityType.Name}Temp ON dpi.{nameof(DataProcessingInformation.DPI_ParentPk)} = PK
");
			return result.ToString();
		}

		static IEnumerable<Tuple<Type, Type>> GetEntitiesForPurging(Type parentType, XElement[] entityTypeElements)
		{
			Argument.NotNull(parentType, nameof(parentType));
			Argument.NotNull(entityTypeElements, nameof(entityTypeElements));

			var result = new List<Tuple<Type, Type>>();
			var parentTypeElement = entityTypeElements.FirstOrDefault(x => x.Attribute("Name")?.Value == parentType.Name);
			if (parentTypeElement != null)
			{
				foreach (var property in parentType.GetProperties())
				{
					if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
					{
						var collectionInterfaceGenericType = property.PropertyType.GetGenericArguments().FirstOrDefault();
						if (collectionInterfaceGenericType != null && (parentTypeElement.Elements("Property")?.Any(x => x.Attribute("Type")?.Value == collectionInterfaceGenericType.Name) ?? false))
						{
							result.Add(Tuple.Create(parentType, collectionInterfaceGenericType));
							result.AddRange(GetEntitiesForPurging(collectionInterfaceGenericType, entityTypeElements));
						}
					}
				}
			}
			return result;
		}

		static IEnumerable<Tuple<Type, Type>> GetPreConfiguredEntitiesForPurging(string sourceDataSubSource)
		{
			var subSource = SubSourceHelper.AvailableSubSources.FirstOrDefault(x => x.SubSourceName == sourceDataSubSource);
			if (subSource == null)
			{
				throw new NotSupportedException($"SourceData with SDA_SubSource={sourceDataSubSource} not configured and cannot be purged");
			}
			return subSource.GetEntitiesForPurging();
		}

		protected const int BatchSize = 10;
	}
}
