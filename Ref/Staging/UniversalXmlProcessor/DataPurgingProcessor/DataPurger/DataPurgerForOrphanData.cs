using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public class DataPurgerForOrphanData : IDataPurger
	{
		public DataPurgerForOrphanData(IStagingRepository stagingRepo)
		{
			Argument.NotNull(stagingRepo, nameof(stagingRepo));
			StagingRepo = stagingRepo;
		}

		IStagingRepository StagingRepo { get; }

		public void Purge()
		{
			Console.WriteLine($"{GetType().Name} Start Purging");
			var tableNames = Application.NeedPurgeTableNames.Split(',');
			foreach (var tableName in tableNames)
			{
				if (!string.IsNullOrEmpty(tableName))
				{
					Console.WriteLine($"Start Purging {tableName}");
					var purgeCount = PurgeWithTableName(tableName);
					Console.WriteLine($"Finish Purging {tableName} : {purgeCount}");
				}
			}
			Console.WriteLine($"{GetType().Name} Finish Purging");
		}

		int PurgeWithTableName(string tableName)
		{
			var result = 0;
			var count = int.MaxValue;
			var entityType = typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x => x.Name == tableName);
			while (count > 0)
			{
				count = StagingRepo.ExecuteSqlCommand(CreatePurgingSql(entityType));
				result += count;
				Console.WriteLine($"{tableName} submitted: {count}");
			}
			return result;
		}

		string CreatePurgingSql(Type entityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			var entityList = new List<Tuple<Type, Type>>
			{
				Tuple.Create((Type)null, entityType)
			};
			entityList.AddRange(GetEntitiesForPurging(entityType));
			return PurgeHelper.CreatePurgingSqlWithEntity(entityList, BuildInsertBatchSql(entityType));
		}

		static IEnumerable<Tuple<Type, Type>> GetEntitiesForPurging(Type parentType)
		{
			Argument.NotNull(parentType, nameof(parentType));

			var result = new List<Tuple<Type, Type>>();
			foreach (var property in parentType.GetProperties())
			{
				if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					var collectionInterfaceGenericType = property.PropertyType.GetGenericArguments().FirstOrDefault();
					if (collectionInterfaceGenericType != null && !collectionInterfaceGenericType.IsNonPersistent())
					{
						result.Add(Tuple.Create(parentType, collectionInterfaceGenericType));
						result.AddRange(GetEntitiesForPurging(collectionInterfaceGenericType));
					}
				}
			}
			return result;
		}

		string BuildInsertBatchSql(Type rootType)
		{
			return $@"INSERT #{rootType.Name}Temp (PK)
SELECT TOP {BatchSize} {rootType.GetTablePrefix()}_PK
FROM  {rootType.Name}
LEFT JOIN {nameof(DataProcessingInformation)} ON DPI_ParentPK = {rootType.GetTablePrefix()}_PK
WHERE {nameof(DataProcessingInformation.DPI_ParentPk)} IS NULL";
		}

		const int BatchSize = 100;
	}
}
