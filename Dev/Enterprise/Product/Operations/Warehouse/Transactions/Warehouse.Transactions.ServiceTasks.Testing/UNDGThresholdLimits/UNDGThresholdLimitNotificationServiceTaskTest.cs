using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(UNDGThresholdLimitNotificationServiceTask))]
	class UNDGThresholdLimitNotificationServiceTaskTest : ServiceTaskTestCase<UNDGThresholdLimitNotificationServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestServiceTask_NoDGWarehouse()
		{
			var logger = RunServiceTask();

			AssertEquals("Since there are no warehouses, Information should be shown.",
				"Information|There are no warehouses using UNDG Limit functionality.", logger.ToString().Trim());
		}

		public void TestServiceTask_EndToEnd()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("C1");
			var warehouse = helper.CreateWarehouse("WH1", "A", 2, 1);
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 20;

			var product = helper.CreateProduct(client, "P1");

			var dgItem1 = product.UNDGs.AddNew();
			dgItem1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			dgItem1.DI_DGWeight = 1m;
			dgItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			dgItem1.DI_DGVolume = 1m;
			dgItem1.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			var dgItem2 = product.UNDGs.AddNew();
			dgItem2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;
			dgItem2.DI_DGWeight = 1m;
			dgItem2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			dgItem2.DI_DGVolume = 1m;
			dgItem2.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			Factory.Save();

			helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Factory.Save();

			helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);
			helper.CreateWhsUNDGLimit(warehouse, "0005", totalWeightLimit: 200m, totalVolumeLimit: 300m);
			Factory.Save();

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			contact.OC_ContactName = "ABC";
			contact.OC_Email = "recipient1@email.com";
			warehouse.WW_OC_DGContact = contact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new UNDGThresholdLimitNotificationServiceTask() { ServiceLogger = logger };
			InitialiseAndRunTaskSchedule(task);

			AssertEquals(2, logger.Count);
			AssertEquals($"Information|Checking DG Limit Thresholds for Warehouse {warehouse.WW_WarehouseName}.", logger[0]);
			AssertEquals("Information|DG Limit Thresholds were exceeded. Queueing email to DG Contact.", logger[1]);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.
DG '0004a' is at 101% volume capacity.
DG '0005' is at 51% weight capacity.
DG '0005' is at 34% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";

			var errorEmail = Env.OutgoingMailManager.EmailsCreated.Single(e => e.Subject == "DG Limit Thresholds Exceeded For Warehouse " + warehouse.WW_WarehouseName);
			AssertContainsExactElementsInAnyOrder(new[] { "recipient1@email.com" }, errorEmail.Recipients.ToStringCollection());
			AssertEquals(expectedEmail, errorEmail.Body);
		}

		public void TestServiceTask_ThroughObjectFactory()
		{
			var bizo = Factory.New<WhsWarehouse>();
			var mockedFinder = new Mock<IUNDGThresholdLimitWarehouseFinder>();
			mockedFinder.Setup(f => f.LoadWarehousesWithDGLimits()).Returns(new[] { bizo });

			var mockedProcessor = new Mock<IUNDGThresholdLimitNotificationProcessor>();
			mockedProcessor.Setup(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), bizo, It.IsAny<ILogger>()));

			using (ObjectFactory.Substitute(mockedFinder.Object))
			using (ObjectFactory.Substitute(mockedProcessor.Object))
			{
				var task = new UNDGThresholdLimitNotificationServiceTask();
				InitialiseAndRunTaskSchedule(task);
				mockedFinder.Verify(f => f.LoadWarehousesWithDGLimits(), Times.Once);
				mockedProcessor.Verify(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), bizo, It.IsAny<ILogger>()), Times.Once);
			}
		}

		public void TestServiceTask_CancelInMiddle()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse2 = Factory.New<WhsWarehouse>();
			var warehouse3 = Factory.New<WhsWarehouse>();
			var mockedFinder = new Mock<IUNDGThresholdLimitWarehouseFinder>();
			mockedFinder.Setup(f => f.LoadWarehousesWithDGLimits()).Returns(new[] { warehouse1, warehouse2, warehouse3 });

			var cancellationTokenSource = new CancellationTokenSource();
			var mockedProcessor = new Mock<IUNDGThresholdLimitNotificationProcessor>();
			mockedProcessor.Setup(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), It.IsAny<WhsWarehouse>(), It.IsAny<ILogger>()))
				.Callback((CancellationToken token, WhsWarehouse warehouse, ILogger logger) =>
				{
					if (warehouse == warehouse2)
					{
						cancellationTokenSource.Cancel();
					}
				});

			using (ObjectFactory.Substitute(mockedFinder.Object))
			using (ObjectFactory.Substitute(mockedProcessor.Object))
			{
				var task = new UNDGThresholdLimitNotificationServiceTask();
				try
				{
					InitialiseAndRunTaskSchedule(task, cancellationTokenSource.Token);
				}
				catch (OperationCanceledException e)
				{
					AssertEquals("The operation was canceled.", e.Message);
				}

				mockedFinder.Verify(f => f.LoadWarehousesWithDGLimits(), Times.Once);
				mockedFinder.VerifyNoOtherCalls();

				mockedProcessor.Verify(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), warehouse1, It.IsAny<ILogger>()), Times.Once);
				mockedProcessor.Verify(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), warehouse2, It.IsAny<ILogger>()), Times.Once);
				mockedProcessor.Verify(p => p.NotifyWarehouseManagersOfExceededUNDGLimits(It.IsAny<CancellationToken>(), warehouse3, It.IsAny<ILogger>()), Times.Never);
				mockedProcessor.VerifyNoOtherCalls();
			}
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(UNDGThresholdLimitNotificationServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == UNDGThresholdLimitNotificationServiceTask.Code);
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new UNDGThresholdLimitNotificationServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("DGT", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
