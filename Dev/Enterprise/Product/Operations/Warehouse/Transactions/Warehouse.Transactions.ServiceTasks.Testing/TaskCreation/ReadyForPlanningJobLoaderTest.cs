using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[UseSnapshotProtection]
	class ReadyForPlanningJobLoaderTest : TestCase
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<ReadyForPlanningJobLoader>(ObjectFactory.Get<IReadyForPlanningJobLoader>());
		}

		public void TestGetNextJobToProcess_NullFactory()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ReadyForPlanningJobLoader().GetNextJobToProcess(null));
		}

		public void TestGetNextJobToProcess_NoJobs()
		{
			var loader = new ReadyForPlanningJobLoader();
			AssertNull(loader.GetNextJobToProcess(Factory));
		}

		public void TestGetNextJobToProcess()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
				Factory.Save();

				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Factory.Save();
				AssertEquals("Precondition.", TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);

				var loader = new ReadyForPlanningJobLoader();
				using (var result = loader.GetNextJobToProcess(Factory))
				{
					AssertNotNull("Should have returned a result.", result);
					AssertEquals("Should have returned the receive.", receive.PK, result.Item.PK);
				}
			}
		}

		public void TestGetNextJobToProcess_ReadsPast()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, finalise: false);
				Factory.Save();

				receive1.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				receive2.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				receive1.WD_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-14);
				receive2.WD_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-7);
				Factory.Save();
				AssertEquals("Precondition.", TaskPlanningStatus.Codes.Ready, receive1.WD_TaskPlanningStatus);
				AssertEquals("Precondition.", TaskPlanningStatus.Codes.Ready, receive2.WD_TaskPlanningStatus);

				var loader = new ReadyForPlanningJobLoader();
				using (var result1 = loader.GetNextJobToProcess(Factory))
				{
					AssertNotNull("Should have returned a result.", result1);
					AssertEquals("Should have returned a receive.", true, result1.Item.PK == receive1.PK || result1.Item.PK == receive2.PK);

					using (var secondConnection = Db.NewExtraConnectionToMainDb())
					using (var result2 = loader.GetNextJobToProcess(new BusinessObjectFactory(secondConnection) { RefreshEnabled = false }))
					{
						AssertNotNull("Should have returned a result.", result2);
						AssertEquals("Should have returned a receive.", true, result2.Item.PK == receive1.PK || result2.Item.PK == receive2.PK);
						AssertNotEquals("Should have returned a different receive.", result2.Item.PK, result1.Item.PK);
					}
				}
			}
		}

		#region Implementation

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
