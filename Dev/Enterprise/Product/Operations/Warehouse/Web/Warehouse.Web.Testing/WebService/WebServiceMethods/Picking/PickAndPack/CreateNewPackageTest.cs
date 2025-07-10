using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CreateNewPackageTest : WhsSecureServiceTestCase
	{
		#region TestCreateNewPackage

		public void TestCreateNewPackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "BOX" });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertNotNull(response.NewPackage);
				AssertEquals("TEST-001", response.NewPackage.PackageID);
				AssertEquals("Should have created a new Package.", 1, order.PackageJob.Packages.Count);
			});

			var newPackage = order.PackageJob.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have created a new Package with Correct ID.", "TEST-001", newPackage.KP_PackageID);
				AssertEquals("Should have created a new Package with Correct Pack Type.", "BOX", newPackage.KP_F3_NKPackType);
				AssertEquals("Should have saved the new Package to the Database.", true, newPackage.IsInDatabase);
			});
		}

		public void TestCreateNewPackage_UpdatesWeightAndVolumeOnOrder()
		{
			var packtype = Helper.Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_Height = 2m;
			packtype.F3_Length = 2m;
			packtype.F3_Width = 2m;
			packtype.F3_UnitOfDimension = "M3"; // 8M3 Volume
			packtype.F3_UnitOfWeight = "KG";

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition", false, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", 20m, order.WD_WeightSent);
			AssertEquals("Precondition", 5m, order.WD_CubicSent);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT" });
			AssertSuccessfulResponse(response, webService);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Use Packing Weight and Volume should be defaulted to true.", true, orderInOtherFactory.UsePackingWeightAndVolume);
			AssertEquals("Pallets Sent should be updated.", (short)1, orderInOtherFactory.WD_PalletsSent);
			AssertEquals("Weight Sent should be Updated.", 35m, orderInOtherFactory.WD_WeightSent);
			AssertEquals("Volume Sent should be Updated.", 8m, orderInOtherFactory.WD_CubicSent);
		}

		public void TestCreateNewPackage_DocketIDDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(receive.WD_DocketID, new PackageInfo { PackType = "BOX" });

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"No Warehouse Order found with Docket ID: {receive.WD_DocketID}.", response.ErrorMessage);
		}

		public void TestCreateNewPackage_ZSaveException_RespondsWithError()
		{
			var webService = GetNewWebService();

			// For this test, we need the helper to use the same factory as the webservice
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			SetupSecurityHeader(webService, data.Whs1);

			helper.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = helper.CreatePickNew(order);

			BusinessObjectFactory.SavingEventHandler action = null;
			action = delegate
			{
				UnitTestUserNotification.Instance.ClearMessages();
				order.Factory.Saving -= action;
				var row = ((INeedRow)order).Row;
				throw new ZSaveException(new DummyDataException(row, TestConnection), order.Factory);
			};
			order.Factory.Saving += action;

			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "BOX" });

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Was unable to Generate ID for new Package:\r\nBlah", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		public void TestCreateNewPackage_ZCannotSaveException_RespondsWithError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			webService.Factory.Saving += action;

			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT", PackageID = "" });
			AssertEquals("Should Error as validation prevents save.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Test - Cannot Save", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		#endregion

		#region TestCreateNewPackage_WithSSCCPrefixOnClient

		public void TestCreateNewPackage_WithSSCCPrefixOnClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "BOX" });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertNotNull(response.NewPackage);
				AssertEquals("012345670000000015", response.NewPackage.PackageID);
				AssertEquals("Should have created a new Package.", 1, order.PackageJob.Packages.Count);
			});

			var newPackage = order.PackageJob.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have created a new Package with Correct ID.", "012345670000000015", newPackage.KP_PackageID);
				AssertEquals("Should have created a new Package with Correct Pack Type.", "BOX", newPackage.KP_F3_NKPackType);
				AssertEquals("Should have saved the new Package to the Database.", true, newPackage.IsInDatabase);
			});
		}

		#endregion

		#region TestCreateNewPackage_WithSSCCPrefixOnWarehouse

		public void TestCreateNewPackage_WithSSCCPrefixOnWarehouse_OptionNotTicked()
		{
			AssertCreateNewPackage_WithSSCCPrefixOnWarehouse(useWarehousePrefix: false);
		}

		public void TestCreateNewPackage_WithSSCCPrefixOnWarehouse_OptionTicked()
		{
			AssertCreateNewPackage_WithSSCCPrefixOnWarehouse(useWarehousePrefix: true);
		}

		void AssertCreateNewPackage_WithSSCCPrefixOnWarehouse(bool useWarehousePrefix)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WarehouseAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "987654321");
			data.Whs1.WW_UseGS1PrefixFallback = useWarehousePrefix;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "BOX" });

			var id = useWarehousePrefix ? "098765432100000012" : "TEST-001";
			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertNotNull(response.NewPackage);
				AssertEquals(id, response.NewPackage.PackageID);
				AssertEquals("Should have created a new Package.", 1, order.PackageJob.Packages.Count);
			});

			var newPackage = order.PackageJob.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have created a new Package with Correct ID.", id, newPackage.KP_PackageID);
				AssertEquals("Should have created a new Package with Correct Pack Type.", "BOX", newPackage.KP_F3_NKPackType);
				AssertEquals("Should have saved the new Package to the Database.", true, newPackage.IsInDatabase);
			});
		}

		#endregion

		#region TestCreateNewPackage_WithPackageId

		public void TestCreateNewPackage_WithPackageId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT", PackageID = "ABC0001" });

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertNotNull(response.NewPackage);
				AssertEquals("ABC0001", response.NewPackage.PackageID);
				AssertEquals("Should have created a new Package.", 1, order.PackageJob.Packages.Count);
			});

			var newPackage = order.PackageJob.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Should have created a new Package with Correct ID.", "ABC0001", newPackage.KP_PackageID);
				AssertEquals("Should have created a new Package with Correct Pack Type.", "PLT", newPackage.KP_F3_NKPackType);
				AssertEquals("Should have saved the new Package to the Database.", true, newPackage.IsInDatabase);
			});
		}

		public void TestCreateNewPackage_WithPackageId_WithPackageValidationError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var existingPackage = order.PackageJob.Packages.AddNew();
			existingPackage.KP_PackageID = "ABC0001";
			Helper.Factory.Save();

			AssertEquals("Precondition: Order should have 1 Package.", 1, order.PackageJob.Packages.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT", PackageID = "ABC0001" });

			AssertEquals("Response should have an error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response should have an error.", "Unable to create a package with 'ABC0001' package id.", response.ErrorMessage);
			AssertEquals("No new package is created.", 1, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestCreateNewPackage_InvalidOperationException

		public void TestCreateNewPackage_InvalidOperationException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var existingPackage = order.PackageJob.Packages.AddNew();
			existingPackage.KP_PackageID = "ABC0001";
			Helper.Factory.Save();

			AssertEquals("Precondition: Order should have 1 Package.", 1, order.PackageJob.Packages.Count);

			var webService = GetNewWebService(data.Whs1);
			Exception ex = null;
			void action(BusinessObjectFactory factory)
			{
				webService.Factory.Saving -= action;
				ex = new InvalidOperationException("Test - Collection Changed");
				throw ex;
			}
			webService.Factory.Saving += action;

			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT", PackageID = "" });
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Was unable to Generate ID for new Package due to invalid operation:\r\nTest - Collection Changed\r\n", response.ErrorMessage);

			AssertNotNull("InvalidOperationException reported.", ErrorReporter.LastExceptionReported);
			AssertEquals("InvalidOperationException message correct.", "Test - Collection Changed", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestCreateNewPackage_InvalidOperationException_WithInnerException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var existingPackage = order.PackageJob.Packages.AddNew();
			existingPackage.KP_PackageID = "ABC0001";
			Helper.Factory.Save();

			AssertEquals("Precondition: Order should have 1 Package.", 1, order.PackageJob.Packages.Count);

			var webService = GetNewWebService(data.Whs1);
			Exception ex = null;
			void action(BusinessObjectFactory factory)
			{
				webService.Factory.Saving -= action;
				ex = new InvalidOperationException("Test - Collection Changed", new Exception("Inner exception Collection"));
				throw ex;
			}
			webService.Factory.Saving += action;

			var response = webService.CreateNewPackage(order.WD_DocketID, new PackageInfo { PackType = "PLT", PackageID = "" });
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Was unable to Generate ID for new Package due to invalid operation:\r\nTest - Collection Changed\r\nInner exception Collection", response.ErrorMessage);

			AssertNotNull("InvalidOperationException reported.", ErrorReporter.LastExceptionReported);
			AssertEquals("InvalidOperationException message correct.", "Test - Collection Changed", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		#endregion
	}
}
