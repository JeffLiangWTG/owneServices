using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class DataProcessingCloneFixture
{
	[Test]
	public void Triggers()
	{
		var connectionString = TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging));

		var sourceID = Guid.NewGuid();
		var newRecordPK = Guid.NewGuid();
		var expiredRecordPK = Guid.NewGuid();
		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var dataProcessingClone = new DataProcessingClone
			{
				DPC_PK = Guid.NewGuid(),
				DPC_Status = "QUE",
				DPC_TableCode = "ZZ1",
				DPC_SourceId = sourceID,
				DPC_NewRecordPK = newRecordPK,
				DPC_ExpiredRecordPK = expiredRecordPK
			};
			entities.Add(dataProcessingClone);
			entities.SaveChanges();
			var dpcRecord = entities.Get<DataProcessingClone>().FirstOrDefault();
			Assert.AreEqual("QUE", dpcRecord.DPC_Status);
			Assert.AreEqual("ZZ1", dpcRecord.DPC_TableCode);
			Assert.AreEqual(sourceID, dpcRecord.DPC_SourceId);
			Assert.AreEqual(newRecordPK, dpcRecord.DPC_NewRecordPK);
			Assert.AreEqual(expiredRecordPK, dpcRecord.DPC_ExpiredRecordPK);

			// Update
			dataProcessingClone.DPC_Status = "PRS";
			entities.Update(dataProcessingClone);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var dpcRecord = entities.Get<DataProcessingClone>().FirstOrDefault();
			Assert.AreEqual("PRS", dpcRecord.DPC_Status);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var dpcRecord = entities.Get<DataProcessingClone>().FirstOrDefault();
			entities.Remove(dpcRecord);
			entities.SaveChanges();
			Assert.AreEqual(0, entities.Get<RefApplicationAttributeType>().Count());
		}
	}
}
