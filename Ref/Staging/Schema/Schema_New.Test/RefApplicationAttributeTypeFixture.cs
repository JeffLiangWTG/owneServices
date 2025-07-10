using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RefApplicationAttributeTypeFixture
{
	[Test]
	public void Triggers()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);

		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var attributeType = new RefApplicationAttributeType
			{
				RAT_PK = Guid.NewGuid(),
				RAT_Type = "String",
				RAT_Description = "string type"
			};
			entities.Add(attributeType);
			entities.SaveChanges();
			var attributeTypeSample = entities.Get<RefApplicationAttributeType>().FirstOrDefault();
			Assert.AreEqual(attributeType.RAT_Type, attributeTypeSample.RAT_Type);
			Assert.AreEqual(attributeType.RAT_Description, attributeTypeSample.RAT_Description);

			// Update
			attributeType.RAT_Description = "modified type";
			entities.Update(attributeType);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var attributeTypeSample = entities.Get<RefApplicationAttributeType>().FirstOrDefault();
			Assert.AreEqual("modified type", attributeTypeSample.RAT_Description);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var attributeTypeSample = entities.Get<RefApplicationAttributeType>().FirstOrDefault();
			entities.Remove(attributeTypeSample);
			entities.SaveChanges();
			Assert.AreEqual(0, entities.Get<RefApplicationAttributeType>().Count());
		}
	}
}
