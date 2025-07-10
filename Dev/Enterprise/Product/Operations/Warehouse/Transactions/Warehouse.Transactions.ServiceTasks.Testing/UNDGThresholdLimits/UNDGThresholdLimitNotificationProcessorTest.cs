using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	public class UNDGThresholdLimitNotificationProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new UNDGThresholdLimitNotificationProcessor(null));
		}

		#endregion

		#region TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_InProductWarehouse()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.
DG '0004a' is at 101% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_InFTZWarehouse()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.FreeTradeZone);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.
DG '0004a' is at 101% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_InTransitWarehouse()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Transit);

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			Helper.CreateWhsItemPackageState("ARV", location, rtu, package);

			var dgItem = Factory.NewWithValidTestData<UNDGDataItem>();
			dgItem.DI_ParentID = package.PK;
			dgItem.DI_ParentTableCode = "KP";
			dgItem.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			dgItem.DI_DGWeight = 101m;
			dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
			dgItem.DI_DGVolume = 101m;
			dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.
DG '0004a' is at 101% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_WarningOnly()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 90m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 90% weight capacity.
DG '0004a' is at 90% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_WeightOnly()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 1000m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_VolumeOnly()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 1000m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_MultipleProducts()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product1 = CreateProductWithBasicDGItem(client, "P1", "0004");
			var product2 = CreateProductWithBasicDGItem(client, "P2", "0004");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product1, 51m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product2, 51m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 102% weight capacity.
DG '0004a' is at 102% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_MultipleDGs()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product1 = CreateProductWithBasicDGItem(client, "P1", "0004");
			var product2 = CreateProductWithBasicDGItem(client, "P2", "0012");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product1, 101m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product2, 50m);

			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);
			Helper.CreateWhsUNDGLimit(warehouse, "0012a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 101% weight capacity.
DG '0004a' is at 101% volume capacity.
DG '0012a' is at 50% weight capacity.
DG '0012a' is at 50% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DGLimitExceeded_NoDGContactExists()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 101m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("No email should be sent for warehouse.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			logger.Verify(l => l.Log(LogType.Information, $"Checking DG Limit Thresholds for Warehouse {warehouse.WW_WarehouseName}."));
			logger.Verify(l => l.Log(LogType.Information, $"DG Contact with valid email address does not exist for Warehouse {warehouse.WW_WarehouseName}. Skipping DG Limit Thresholds check."));
			logger.VerifyNoOtherCalls();
		}

		void NotifyWarehouseManagersOfExceededUNDGLimits(WhsWarehouse warehouse, ILogger logger)
		{
			var manager = new UNDGThresholdLimitNotificationProcessor(ObjectFactory.Get<IWhsUNDGLimitValidationHelperFactory>());
			manager.NotifyWarehouseManagersOfExceededUNDGLimits(new CancellationToken(), warehouse, logger);
		}

		void AssertEmailSent(string expectedRecipient, string warehouseName, string expectedEmailBody)
		{
			var errorEmail = Env.OutgoingMailManager.EmailsCreated.Single(e => e.Subject == "DG Limit Thresholds Exceeded For Warehouse " + warehouseName);
			AssertContainsExactElementsInAnyOrder(new[] { expectedRecipient }, errorEmail.Recipients.ToStringCollection());
			AssertEquals(expectedEmailBody, errorEmail.Body);
		}

		void AssertEmailSentLogs(string warehouseName, Mock<ILogger> loggerMock)
		{
			loggerMock.Verify(l => l.Log(LogType.Information, $"Checking DG Limit Thresholds for Warehouse {warehouseName}."));
			loggerMock.Verify(l => l.Log(LogType.Information, "DG Limit Thresholds were exceeded. Queueing email to DG Contact."));
			loggerMock.VerifyNoOtherCalls();
		}

		#endregion

		#region TestNotifyWarehouseManagersOfExceededUNDGLimits_UnderLimit_UnderThreshold

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_UnderLimit_UnderThreshold()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var loggerMock = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, loggerMock.Object);

			AssertEquals("No email should be sent for warehouse.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			loggerMock.Verify(l => l.Log(LogType.Information, $"Checking DG Limit Thresholds for Warehouse {warehouse.WW_WarehouseName}."));
			loggerMock.VerifyNoOtherCalls();
		}

		#endregion

		#region TestNotifyWarehouseManagersOfExceededUNDGLimits_UnderLimit_OverThreshold

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_UnderLimit_OverThreshold()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			var product = CreateProductWithBasicDGItem(client, "P1", "0004");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 30m);
			Helper.CreateWhsUNDGLimit(warehouse, "0004a", totalWeightLimit: 100m, totalVolumeLimit: 100m);

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var logger = new Mock<ILogger>();
			NotifyWarehouseManagersOfExceededUNDGLimits(warehouse, logger.Object);

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG '0004a' is at 30% weight capacity.
DG '0004a' is at 30% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);
		}

		#endregion

		#region TestNotifyWarehouseManagersOfExceededUNDGLimits_DBHits

		public void TestNotifyWarehouseManagersOfExceededUNDGLimits_DBHits()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = CreateWarehouseWithDGLimitsEnabled("WH1", WarehouseTypes.Codes.Product);
			for (var i = 1; i < 10; i++)
			{
				CreateDG(i);
			}

			var userContact = CreateContactForWarehouse("ABC", "recipient1@email.com");
			warehouse.WW_OC_DGContact = userContact.PK;
			warehouse.WW_DGContactPhoneType = "666";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ WhsUNDGLimitSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ UNDGCountryReferenceSchema.Constants.TableName, 1 },
				{ UNDGSubstanceSchema.Constants.TableName, 1 }
			};

			var logger = new Mock<ILogger>();
			using (AssertDbHitsForAllFactories(expectedDbHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var newWarehouse = newFactory.Load<WhsWarehouse>(warehouse.PK);
				NotifyWarehouseManagersOfExceededUNDGLimits(newWarehouse, logger.Object);
			}

			AssertEquals("An email should be sent for warehouse.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedEmail = @$"The following DG Limit Thresholds have been exceeded in Warehouse WH1:
DG 'S001a' is at 153% weight capacity.
DG 'S001a' is at 153% volume capacity.
DG 'S002a' is at 156% weight capacity.
DG 'S002a' is at 156% volume capacity.
DG 'S003a' is at 159% weight capacity.
DG 'S003a' is at 159% volume capacity.
DG 'S004a' is at 162% weight capacity.
DG 'S004a' is at 162% volume capacity.
DG 'S005a' is at 165% weight capacity.
DG 'S005a' is at 165% volume capacity.
DG 'S006a' is at 168% weight capacity.
DG 'S006a' is at 168% volume capacity.
DG 'S007a' is at 171% weight capacity.
DG 'S007a' is at 171% volume capacity.
DG 'S008a' is at 174% weight capacity.
DG 'S008a' is at 174% volume capacity.
DG 'S009a' is at 177% weight capacity.
DG 'S009a' is at 177% volume capacity.
Country Reference 'AU1' is at 153% weight capacity.
Country Reference 'AU1' is at 153% volume capacity.
Country Reference 'AU2' is at 156% weight capacity.
Country Reference 'AU2' is at 156% volume capacity.
Country Reference 'AU3' is at 159% weight capacity.
Country Reference 'AU3' is at 159% volume capacity.
Country Reference 'AU4' is at 162% weight capacity.
Country Reference 'AU4' is at 162% volume capacity.
Country Reference 'AU5' is at 165% weight capacity.
Country Reference 'AU5' is at 165% volume capacity.
Country Reference 'AU6' is at 168% weight capacity.
Country Reference 'AU6' is at 168% volume capacity.
Country Reference 'AU7' is at 171% weight capacity.
Country Reference 'AU7' is at 171% volume capacity.
Country Reference 'AU8' is at 174% weight capacity.
Country Reference 'AU8' is at 174% volume capacity.
Country Reference 'AU9' is at 177% weight capacity.
Country Reference 'AU9' is at 177% volume capacity.
UNDG Class '1' is at 153% weight capacity.
UNDG Class '1' is at 153% volume capacity.
UNDG Class '2' is at 156% weight capacity.
UNDG Class '2' is at 156% volume capacity.
UNDG Class '3' is at 159% weight capacity.
UNDG Class '3' is at 159% volume capacity.
UNDG Class '4' is at 162% weight capacity.
UNDG Class '4' is at 162% volume capacity.
UNDG Class '5' is at 165% weight capacity.
UNDG Class '5' is at 165% volume capacity.
UNDG Class '6' is at 168% weight capacity.
UNDG Class '6' is at 168% volume capacity.
UNDG Class '7' is at 171% weight capacity.
UNDG Class '7' is at 171% volume capacity.
UNDG Class '8' is at 174% weight capacity.
UNDG Class '8' is at 174% volume capacity.
UNDG Class '9' is at 177% weight capacity.
UNDG Class '9' is at 177% volume capacity.

No new inventory with these DGs may enter the warehouse if the capacity exceeds 100%.
";
			AssertEmailSent("recipient1@email.com", warehouse.WW_WarehouseName, expectedEmail);
			AssertEmailSentLogs(warehouse.WW_WarehouseName, logger);

			void CreateDG(int i)
			{
				var undgUNNOCode = $"S00{i}";
				var undgVariant = "a";
				var undgCode = $"{undgUNNOCode}{undgVariant}";
				var undgStandard = "IMO";
				var undgClass = $"{i}.1D";

				var substance = Helper.CreateUNDGSubstance(undgUNNOCode, undgClass, undgCode);
				var product = Helper.CreateProduct(client, $"P{i}");
				var dgItem = product.UNDGs.AddNew();
				dgItem.DI_DG = substance.PK;
				dgItem.DI_DGWeight = 1m;
				dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
				dgItem.DI_DGVolume = 1m;
				dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;

				Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 100m);

				var reference = Helper.CreateCountryReference(referenceCode: $"AU{i}");
				Helper.CreateUNDGCountryReferencePivot(reference.PK, undgUNNOCode, undgVariant, undgStandard);
				var undgLimitCountryReference = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitCountryReference);

				var undgLimitClass = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, $"{i}", null, totalWeightLimit: 100m, totalVolumeLimit: 100m);
				warehouse.UNDGLimits.Add(undgLimitClass);

				Helper.CreateWhsReceiveWithInventory(client, warehouse, $"R{i}1", product, 50m + i);
				Helper.CreateWhsReceiveWithInventory(client, warehouse, $"R{i}2", product, 50m + i);
				Helper.CreateWhsReceiveWithInventory(client, warehouse, $"R{i}3", product, 50m + i);
			}
		}

		#endregion

		#region Implementation

		WhsWarehouse CreateWarehouseWithDGLimitsEnabled(string warehouseName, string warehouseType)
		{
			var warehouse = Helper.CreateWarehouse(warehouseName, "A", 2, 1);
			warehouse.WW_WarehouseType = warehouseType;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 20;
			return warehouse;
		}

		OrgSupplierPart CreateProductWithBasicDGItem(OrgHeader owner, string productName, string substanceName)
		{
			var product = Helper.CreateProduct(owner, productName);

			var dgItem = product.UNDGs.AddNew();
			dgItem.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, substanceName, "a", "IMO").First().PK;
			dgItem.DI_DGWeight = 1m;
			dgItem.DI_UnitOfWeight = Constants.Weight.Kilograms;
			dgItem.DI_DGVolume = 1m;
			dgItem.DI_UnitOfVolume = Constants.Volume.CubicMetres;

			return product;
		}

		OrgContact CreateContactForWarehouse(string userCode, string recipientEmail)
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			contact.OC_ContactName = userCode;
			contact.OC_Email = recipientEmail;
			Factory.Save();

			return contact;
		}

		#endregion
	}
}
