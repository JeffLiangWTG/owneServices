using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[TestFixture]
	abstract class TransformationFixture
	{
		[Test]
		public void IsMapped()
		{
			var taskType = GetTask().GetType();
			var result = new PreUpgradeTransformationTasksForTest().TasksExposed.Any(x => x.GetType() == taskType);
			result = result || new DataTransformationTasksForTest().TasksExposed.Any(x => x.GetType() == taskType);
			result = result || IsMappedInTextFile(taskType, "ShelfCheckinDataTransformationMapper.txt");
			result = result || IsMappedInTextFile(taskType, "ShelfCheckinPreUpgradeTransformationMapper.txt");
			Assert.True(result);
		}

		bool IsMappedInTextFile(Type taskType, string textFileName)
		{
			var textFilePath = Path.Combine(FolderHelper.GetBinFolder(), textFileName);
			var content = File.ReadAllText(textFilePath);
			return content.Contains(taskType.FullName);
		}

		[Test]
		[TransactionedTestCase]
		public void RunAndAssertResultsTwice()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			try
			{
				Connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
				Connection.Open();
				Transaction = Connection.BeginTransaction();

				PrepareTestData();

				var task = GetTask();
				task.Run(Transaction);

				AssertTransformationResults();

				task.Run(Transaction);

				AssertTransformationResults();
			}
			finally
			{
				try
				{
					if (Transaction != null)
					{
						Transaction.Rollback();
						Transaction.Dispose();
					}
				}
				finally
				{
					Connection?.Dispose();
				}
			}
		}

		protected abstract void PrepareTestData();
		protected abstract void AssertTransformationResults();
		protected abstract IDataTransformationTask GetTask();

		protected IDbConnection Connection;
		protected IDbTransaction Transaction;
	}

	class PreUpgradeTransformationTasksForTest : PreUpgradeTransformationTasks
	{
		public IEnumerable<IDataTransformationTask> TasksExposed => Tasks;
	}

	class DataTransformationTasksForTest : DataTransformationTasks
	{
		public IEnumerable<IDataTransformationTask> TasksExposed => Tasks;
	}
}
