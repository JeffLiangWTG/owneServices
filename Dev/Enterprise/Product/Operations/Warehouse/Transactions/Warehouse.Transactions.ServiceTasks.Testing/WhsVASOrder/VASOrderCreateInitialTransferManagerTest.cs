using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class VASOrderCreateInitialTransferManagerTest : WhsTestCaseWithFactory
	{
		#region TestCheckMaxWeightVolume_NotEnoughSpace_Weight

		public void TestCreateTransferIfThereIsEnoughSpace()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupEnviromentAndCreateReceive(data,
											receivePart1: 50m,
											receivePart2: 20m);

			var vasOrder1 = Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 25m);
			var vasOrder2 = Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part2, quantity: 10m);
			Factory.Save();

			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransfersForVASOrdersWithNoInitialTransfer();

			AssertNotNull(vasOrder1.TransferIntoServiceArea);
			AssertNotNull(vasOrder2.TransferIntoServiceArea);
			var logs = logger.ToString();
			Assert(logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000001 with Client 111 Warehouse 1."));
			Assert(logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000002 with Client 111 Warehouse 1."));
		}

		#endregion

		#region TestCheckMaxWeightVolume_NotEnoughSpace_Weight

		public void TestCheckMaxWeightVolume_NotEnoughSpace_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupEnviromentAndCreateReceive(data,
											maxWeightServiceArea1: 10m,
											maxWeightServiceArea2: 10m,
											receivePart1: 50m);

			Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 25m);
			Factory.Save();

			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransfersForVASOrdersWithNoInitialTransfer();

			// Total inventory Weight (25.00 KG) exceeds the maximum allowed Weight (10.00 KG) for these locations.
			AssertEquals(@"Error|Creating a transfer for VAS Order WV00000001 with Client 111 and Warehouse 1 failed. Error below:
Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.", Logger.ToString().Trim());
		}

		#endregion

		#region TestCombineSuccessAndFail

		public void TestCombineSuccessAndFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupEnviromentAndCreateReceive(data,
											maxWeightServiceArea1: 10m,
											maxWeightServiceArea2: 10m,
											receivePart1: 50m);

			Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 25m);
			Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 1m);
			Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part2, quantity: 2m);
			Factory.Save();

			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransfersForVASOrdersWithNoInitialTransfer();

			// Total inventory Weight (25.00 KG) exceeds the maximum allowed Weight (10.00 KG) for these locations.
			var logs = Logger.ToString().Trim();
			Assert("Transfer was created for VAS Order WV00000002", logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000002 with Client 111 Warehouse 1."));
			Assert("Failed to create for VAS Order WV00000001", logs.Contains(@"Error|Creating a transfer for VAS Order WV00000001 with Client 111 and Warehouse 1 failed. Error below:
Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer."));
			Assert("Failed to create for VAS Order WV00000003", logs.Contains(@"Error|Creating a transfer for VAS Order WV00000003 with Client 111 and Warehouse 1 failed. Error below:
Cannot create Transfer because of the following Shortfalls:
2x P2"));
		}

		#endregion

		#region TestConcurrencyError

		public void TestConcurrencyError()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, quantity: 5m);
			Factory.Save();

			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransferForTest += _ =>
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var newVasOrder = newFactory.Load<WhsVASOrder>(vasOrder.PK);
				var initialTransfer = newVasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
				newFactory.Save();
			};

			AssertNoExceptionThrown("Should not throw exception", manager.CreateTransfersForVASOrdersWithNoInitialTransfer);

			AssertEquals(@"Error|Creating a transfer for VAS Order WV00000001 with Client 111 and Warehouse 1 failed. Error below:
Another user has modified the VAS Order or created the initial transfer. The transfer cannot be saved because it may conflict with the other user's changes. Please try again.", Logger.ToString().Trim());
		}

		public void TestCreateTransferWithConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			NotificationHandler.Instance = null; //issue reports when NotificationHandler.Instance is DefaultNotificationHandler

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, rowCode: "Test");
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Factory.Save();

			var serviceLocation = data.Whs1.FindLocation("Test");
			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransferForTest += t =>
			{
				var locationInManager = t.Factory.Load<WhsLocation>(serviceLocation.PK);
				ConcurrencyInfo.SetConcurrencyPolicy(locationInManager, nameof(WhsLocation.WLV_LastInventoryChangeDate), ConcurrencyPolicy.Strict);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var locationNew = newFactory.Load<WhsLocation>(serviceLocation.PK);
				locationNew.WLV_LastInventoryChangeDate = ZDateTimeOffset.Now.AddHours(-1);
				newFactory.Save();
			};

			AssertNoExceptionThrown("Should not throw exception", manager.CreateTransfersForVASOrdersWithNoInitialTransfer);

			AssertEquals(@"Error|Creating a transfer for VAS Order WV00000001 with Client 111 and Warehouse 1 failed. Error below:
Another user has modified the VAS Order or created the initial transfer. The transfer cannot be saved because it may conflict with the other user's changes. Please try again.", Logger.ToString().Trim());
		}

		public void TestCreateTransferWithTriggerException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, rowCode: "Test");
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Factory.Save();

			var serviceLocation = data.Whs1.FindLocation("Test");
			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransferForTest += (WhsTransfer t) =>
			{
				t.Lines[0].PickLines[0].Delete();
			};

			AssertNoExceptionThrown("Should not throw exception", manager.CreateTransfersForVASOrdersWithNoInitialTransfer);

			AssertEquals(@"Error|Creating a transfer for VAS Order WV00000001 with Client 111 and Warehouse 1 failed. Error below:
Another user has modified the VAS Order or created the initial transfer. The transfer cannot be saved because it may conflict with the other user's changes. Please try again.", Logger.ToString().Trim());
		}

		#endregion

		#region TestProcessOnlyActiveVASOrderDoesNotHaveTransfer

		public void TestProcessOnlyActiveVASOrderDoesNotHaveTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetupEnviromentAndCreateReceive(data,
											receivePart1: 50m);

			Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 1m);
			var vasOrderWithTransfer = Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part1, quantity: 2m);
			var cancelledVasOrder = Helper.CreateWhsVASOrderWithLine(ServiceArea, data.Org1, data.Part2, quantity: 1m);
			cancelledVasOrder.IsCancelled = true;
			Factory.Save();
			var buffer = new NotificationBuffer();
			vasOrderWithTransfer.GetOrCreateInitialTransfer(buffer);
			AssertEquals("Precondition, Should not have error to create initial transfer", false, buffer.HasErrors);

			Factory.Save();

			var manager = new VASOrderCreateInitialTransferManager(Logger);
			manager.CreateTransfersForVASOrdersWithNoInitialTransfer();

			AssertEquals("Should not process VASOrder with initial transfer.", @"Information|Initial transfer was created successfully for VAS Order WV00000001 with Client 111 Warehouse 1.", Logger.ToString().Trim());
		}

		#endregion

		#region Test

		public void TestCreateTransfer_SetsUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = Helper.CreateClient("CL1");
			var product1 = Helper.CreateProduct("PROD1", client1);
			var client2 = Helper.CreateClient("CL2");
			var product2 = Helper.CreateProduct("PROD2", client2);
			var client3 = Helper.CreateClient("CL3");
			var product3 = Helper.CreateProduct("PROD3", client3);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R2", product2, 20m);
			Helper.CreateWhsReceiveWithInventory(client3, warehouse1, "R3", product3, 20m);
			Factory.Save();

			var serviceArea1 = Helper.CreateServiceAreaForVASOrder(warehouse1, 1, 2, 1);
			var serviceArea2 = Helper.CreateServiceAreaForVASOrder(warehouse2, 1, 2, 1);

			var vasOrder1 = Helper.CreateWhsVASOrderWithLine(serviceArea1, client1, product1, quantity: 10m);
			var vasOrder2 = Helper.CreateWhsVASOrderWithLine(serviceArea2, client2, product2, quantity: 10m);
			var vasOrder3 = Helper.CreateWhsVASOrderWithLine(serviceArea1, client3, product3, quantity: 10m);
			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;
				var manager = new VASOrderCreateInitialTransferManager(Logger);
				manager.CreateTransfersForVASOrdersWithNoInitialTransfer();

				AssertNotNull(vasOrder1.TransferIntoServiceArea);
				AssertNotNull(vasOrder2.TransferIntoServiceArea);
				AssertNotNull(vasOrder3.TransferIntoServiceArea);
				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region Setup / Assert

		void SetupEnviromentAndCreateReceive(
										TestDataSimpleEnvironment data,
										decimal maxWeightServiceArea1 = 0m, string maxWeightUQServiceArea1 = "KG", decimal maxVolumeServiceArea1 = 0m, string maxVolumeUQServiceArea1 = "CC",
										decimal maxWeightServiceArea2 = 0m, string maxWeightUQServiceArea2 = "KG", decimal maxVolumeServiceArea2 = 0m, string maxVolumeUQServiceArea2 = "CC",
										decimal receivePart1 = 0m,
										decimal receivePart2 = 0m)
		{
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 10m, "CC");
			Helper.SetProductWeightAndVolume(data.Part2, 2m, "KG", 20m, "CC");

			if (receivePart1 > 0)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, receivePart1);
			}
			if (receivePart2 > 0)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, receivePart2);
			}

			ServiceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			Helper.SetLocationMaxWeightAndVolume(data.Whs1.FindLocation("SERVICEROW-1-1"), maxWeightServiceArea1, maxWeightUQServiceArea1, maxVolumeServiceArea1, maxVolumeUQServiceArea1);
			Helper.SetLocationMaxWeightAndVolume(data.Whs1.FindLocation("SERVICEROW-1-2"), maxWeightServiceArea2, maxWeightUQServiceArea2, maxVolumeServiceArea2, maxVolumeUQServiceArea2);

			Factory.Save();
		}

		#endregion

		#region Logger

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion

		#region Properties

		WhsArea ServiceArea;

		#endregion
	}
}
