using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	class RefDataSetInformationFixture
	{
		[Test]
		public void TableStructure()
		{
			var expectedStructure = new[]
			{
				"RDS_PK",
				"RDS_DataSetId",
				"RDS_TableName",
				"RDS_DataSetName",
				"RDS_DataSetTableCode",
				"RDS_PriorityLevel",
				"RDS_LastUpdatedUTC",
				"RDS_IsPush",
				"RefDataSetInformationDefinitions",
				"RefDbVersionControls"
			};
			var dataSetInformationProperties = typeof(RefDataSetInformation).GetProperties()
				.Where(x => !x.CustomAttributes.Any(att => att.AttributeType == typeof(NotMappedAttribute))).Select(x => x.Name);

			foreach (var propName in dataSetInformationProperties)
			{
				Assert.That(expectedStructure.Contains(propName), $"{propName} not found in {nameof(RefDataSetInformation)}");
			}
		}

		[Test]
		[TransactionedTestCase]
		public void DoNotAllowSpaceInDataSetName_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var dsInfo = new RefDataSetInformation()
				{
					RDS_DataSetId = 1,
					RDS_DataSetTableCode = "ANY",
					RDS_IsPush = true,
					RDS_LastUpdatedUTC = DateTime.UtcNow,
					RDS_PK = Guid.NewGuid(),
					RDS_PriorityLevel = 0,
					RDS_TableName = "ANY",
					RDS_DataSetName = "With space"
				};

				context.Set<RefDataSetInformation>().Add(dsInfo);
				Assert.Throws<DbUpdateException>(() => SaveTransaction(context));
			}
		}

		[Test]
		[TransactionedTestCase]
		public void DoNotAllowSpaceInDataSetName_DoesNot_ThrowException()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var dsInfo = new RefDataSetInformation()
				{
					RDS_DataSetId = 1,
					RDS_DataSetTableCode = "ANY",
					RDS_IsPush = true,
					RDS_LastUpdatedUTC = DateTime.UtcNow,
					RDS_PK = Guid.NewGuid(),
					RDS_PriorityLevel = 0,
					RDS_TableName = "ANY",
					RDS_DataSetName = "NoSpace"
				};

				context.Set<RefDataSetInformation>().Add(dsInfo);
				Assert.DoesNotThrow(() => SaveTransaction(context));
			}
		}

		void SaveTransaction(SafeDbContext context)
		{
			var connection = context.Database.GetDbConnection();
			connection.Open();
			using (var trans = connection.BeginTransaction())
			{
				context.SetUserId("~SYS");
				context.SaveChanges();
				trans.Commit();
			}
			connection.Close();
		}
	}
}
