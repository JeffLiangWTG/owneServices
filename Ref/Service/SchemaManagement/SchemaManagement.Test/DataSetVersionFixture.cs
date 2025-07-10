using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public enum DataPreparationStep
	{
		Insert,
		Update,
		Delete
	}

	[TestFixture]
	[TransactionedTestCase]
	public abstract class DataSetVersionFixture
	{
		[Test]
		public virtual void UpdateVersionOnInsert()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var dataSet = PrepareData();
				var tblPrefix = dataSet.FirstOrDefault().GetType().GetTablePrefix();
				var conn = context.Database.GetDbConnection();
				conn.Open();
				foreach (var data in GetData(dataSet, DataPreparationStep.Insert))
				{
					context.Add(data);
					using (var trans = conn.BeginTransaction())
					{
						context.SetUserId("~SYS");
						context.SaveChanges();
						trans.Commit();
					}
					var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentCode == tblPrefix);
					context.Entry(versionControl).Reload();
					Assert.That(versionControl.RVC_IsPublished, Is.False);
					Assert.That(versionControl.RVC_LastEditedUser, Is.EqualTo("~SYS"));
					versionControl.RVC_IsPublished = true;
					versionControl.RVC_LastEditedUser = null;
					context.SaveChanges();
				}
			}
		}

		[Test]
		public void UpdateVersionOnUpdate()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			var dataSet = PrepareData();
			var tblPrefix = dataSet.FirstOrDefault().GetType().GetTablePrefix();
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				foreach (var data in dataSet)
				{
					context.Add(data);
					context.SaveChanges();
				}
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentCode == tblPrefix);
				var conn = context.Database.GetDbConnection();
				conn.Open();
				foreach (var data in GetData(dataSet, DataPreparationStep.Update))
				{
					versionControl.RVC_IsPublished = true;
					versionControl.RVC_LastEditedUser = null;
					context.SaveChanges();
					context.Attach(data);
					if (UpdateData(data))
					{
						using (var trans = conn.BeginTransaction())
						{
							context.SetUserId("~SYS");
							context.SaveChanges();
							trans.Commit();
						}
						context.Entry(versionControl).Reload();
						Assert.That(versionControl.RVC_IsPublished, Is.False);
						Assert.That(versionControl.RVC_LastEditedUser, Is.EqualTo("~SYS"));
					}
				}
			}
		}

		[Test]
		public void UpdateVersionOnUpdateFKColumn()
		{
			var rData = PrepareFKReferencedData();
			if (rData != null)
			{
				var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
				PrepareDb(dbName);
				var dataSet = PrepareData();
				dataSet = dataSet.Concat(rData).ToArray();
				var tblPrefix = dataSet.FirstOrDefault().GetType().GetTablePrefix();
				using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
				{
					foreach (var data in dataSet)
					{
						context.Add(data);
						context.SaveChanges();
					}
				}
				using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
				{
					var versionControls = context.RefDbVersionControls.Where(x => x.RVC_ParentCode == tblPrefix);
					foreach (var versionControl in versionControls)
					{
						versionControl.RVC_IsPublished = true;
						versionControl.RVC_LastEditedUser = null;
					}
					context.SaveChanges();
					var conn = context.Database.GetDbConnection();
					conn.Open();
					foreach (var data in GetData(dataSet, DataPreparationStep.Update))
					{
						context.Attach(data);
						if (UpdateFKColumnData(data))
						{
							using (var trans = conn.BeginTransaction())
							{
								context.SetUserId("~SYS");
								context.SaveChanges();
								trans.Commit();
							}
							foreach (var versionControl in versionControls)
							{
								context.Entry(versionControl).Reload();
								Assert.That(versionControl.RVC_IsPublished, Is.False);
								Assert.That(versionControl.RVC_LastEditedUser, Is.EqualTo("~SYS"));
							}
						}
					}
				}
			}
		}

		[Test]
		public virtual void UpdateVersionOnDelete()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			PrepareDb(dbName);
			var dataSet = PrepareData();
			var tblPrefix = dataSet.FirstOrDefault().GetType().GetTablePrefix();
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				foreach (var data in dataSet)
				{
					context.Add(data);
					context.SaveChanges();
				}
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentCode == tblPrefix);
				var conn = context.Database.GetDbConnection();
				conn.Open();
				foreach (var data in GetData(dataSet, DataPreparationStep.Delete))
				{
					versionControl.RVC_IsPublished = true;
					versionControl.RVC_LastEditedUser = null;
					context.SaveChanges();
					context.Attach(data);
					context.Remove(data);
					using (var trans = conn.BeginTransaction())
					{
						context.SetUserId("~SYS");
						context.SaveChanges();
						trans.Commit();
					}
					context.Entry(versionControl).Reload();
					Assert.That(versionControl.RVC_IsPublished, Is.False);
					Assert.That(versionControl.RVC_LastEditedUser, Is.EqualTo("~SYS"));
				}
			}
		}

		protected virtual IEnumerable<object> GetData(object[] dataSet, DataPreparationStep step)
		{
			return step == DataPreparationStep.Delete ? dataSet.Skip(1).Reverse() : dataSet;
		}

		protected abstract object[] PrepareData();
		protected abstract bool UpdateData(object data);

		protected virtual object[] PrepareFKReferencedData()
		{
			return null;
		}

		protected virtual bool UpdateFKColumnData(object data)
		{
			return false;
		}

		protected virtual void PrepareDb(string dbName)
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africa"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}
	}
}
