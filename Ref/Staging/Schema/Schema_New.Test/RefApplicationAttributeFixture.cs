using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RefApplicationAttributeFixture
{
	[Test]
	public void Triggers()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);

		PrepareDb(connectionString);
		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var attribute = new RefApplicationAttribute
			{
				RAA_PK = Guid.NewGuid(),
				RAA_ConfigFilePath = "Test.exe.config",
				RAA_AttributeName = "OutputPath",
				RAA_Value = "UXmlFiles",
				RAA_RAT_NKType = "String",
				RAA_JobGroup = "GroupA"
			};
			entities.Add(attribute);
			entities.SaveChanges();
			var attributeSample = entities.Get<RefApplicationAttribute>().FirstOrDefault();
			Assert.AreEqual(attribute.RAA_ConfigFilePath, attributeSample.RAA_ConfigFilePath);
			Assert.AreEqual(attribute.RAA_AttributeName, attributeSample.RAA_AttributeName);
			Assert.AreEqual(attribute.RAA_Value, attributeSample.RAA_Value);

			// Update
			attribute.RAA_AttributeName = "modified";
			attribute.RAA_Value = "new value";
			entities.Update(attribute);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var attributeSample = entities.Get<RefApplicationAttribute>().FirstOrDefault();
			Assert.AreEqual("modified", attributeSample.RAA_AttributeName);
			Assert.AreEqual("new value", attributeSample.RAA_Value);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var attributeSample = entities.Get<RefApplicationAttribute>().FirstOrDefault();
			entities.Remove(attributeSample);
			entities.SaveChanges();
			Assert.AreEqual(0, entities.Get<RefApplicationAttribute>().Count());
		}
	}

	protected void PrepareDb(string connectionString)
	{
		using var entities = new StagingRepository(connectionString);
		var attributeType = new RefApplicationAttributeType
		{
			RAT_PK = Guid.NewGuid(),
			RAT_Type = "String",
			RAT_Description = "string type"
		};
		entities.Add(attributeType);
		entities.SaveChanges();
	}
}
