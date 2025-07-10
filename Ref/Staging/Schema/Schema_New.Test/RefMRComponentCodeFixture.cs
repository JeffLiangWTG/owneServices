using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RefMRComponentCodeFixture
{
	[Test]
	public void InsertUpdateAndDelete()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var refMRComponentCode = new RefMRComponentCode
			{
				RCC_PK = Guid.NewGuid(),
				RCC_Code = "Code",
				RCC_Description = "Desc",
				RCC_Group = "CEDEX",
				RCC_IsActive = true,
				RCC_Machinery = true,
				RCC_Structural = true,
				RCC_TankCleaning = true,
				RCC_TankRepair = true,
			};
			entities.Add(refMRComponentCode);
			entities.SaveChanges();
			var mrComponentCodeRecord = entities.Get<RefMRComponentCode>().FirstOrDefault();
			Assert.AreEqual("Code", mrComponentCodeRecord.RCC_Code);
			Assert.AreEqual("Desc", mrComponentCodeRecord.RCC_Description);
			Assert.AreEqual("CEDEX", mrComponentCodeRecord.RCC_Group);
			Assert.IsTrue(mrComponentCodeRecord.RCC_IsActive);
			Assert.IsTrue(mrComponentCodeRecord.RCC_Machinery);
			Assert.IsTrue(mrComponentCodeRecord.RCC_Structural);
			Assert.IsTrue(mrComponentCodeRecord.RCC_TankCleaning);
			Assert.IsTrue(mrComponentCodeRecord.RCC_TankRepair);

			// Update
			refMRComponentCode.RCC_Group = "MERC";
			entities.Update(refMRComponentCode);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var dpcRecord = entities.Get<RefMRComponentCode>().FirstOrDefault();
			Assert.AreEqual("MERC", dpcRecord.RCC_Group);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var dpcRecord = entities.Get<RefMRComponentCode>().FirstOrDefault();
			entities.Remove(dpcRecord);
			entities.SaveChanges();
			Assert.AreEqual(0, entities.Get<RefMRComponentCode>().Count());
		}
	}
}
