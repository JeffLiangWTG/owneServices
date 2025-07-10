using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReleaseLineValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals(typeof(WhsReleaseLineValidation), releaseLine.Validation.AutoValidationType);
		}

		#endregion

		#region TestCheckExpiryDate

		public void TestCheckExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "", ZDate.Today.AddDays(1), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "", ZDate.Today.AddDays(2), ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var releaseLineInFactory2 = factory2.Load<WhsOrderLine>(orderLine.PK).ReleaseLines[0];

			releaseLineInFactory2.SetExpiryDateForTesting(ZDate.Today.AddYears(15));
			releaseLineInFactory2.Validation.ValidateAll();
			AssertNoErrors("Release Lines should not have errors for expiry date as it is not editable.", releaseLineInFactory2.ExpiryDateInfo);

			releaseLineInFactory2.SetExpiryDateForTesting(ZDate.Today.AddYears(-15));
			releaseLineInFactory2.Validation.ValidateAll();
			AssertNoErrors("Release Lines should not have errors for expiry date as it is not editable.", releaseLineInFactory2.ExpiryDateInfo);
		}

		#endregion

		#region TestCheckPackingDate

		public void TestCheckPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLineInFactory2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsOrderLine>(orderLine.PK).ReleaseLines[0];
			releaseLineInFactory2.SetPackingDateForTesting(ZDate.Today.AddYears(15));
			releaseLineInFactory2.Validation.ValidateAll();
			AssertNoErrors("Release Lines should not have errors for packing date as it is not editable.", releaseLineInFactory2.PackingDateInfo);

			releaseLineInFactory2.SetPackingDateForTesting(ZDate.Today.AddYears(-15));
			releaseLineInFactory2.Validation.ValidateAll();
			AssertNoErrors("Release Lines should not have errors for packing date as it is not editable.", releaseLineInFactory2.PackingDateInfo);
		}

		#endregion

		#region Attributes

		#region TestBOMReleaseLinePartAttributeValidation

		public void TestBOMReleaseLinePartAttributeValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 5);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Unit);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, mainProduct, AttributeNumber.One, true);

			var inventoryLocation = data.Whs1.FindLocation("A-1-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 2m, data.Whs1.FindLocation("A-1-1"), ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 3m, data.Whs1.FindLocation("A-1-2"), ZDate.Empty, ZDate.Empty, "PA2", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, data.Whs1.FindLocation("A-2-1"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "PA1");
			releaseLine1.Quantity = 2m;
			releaseLine1.Validation.ValidatePartAttribute1();
			AssertNoErrors(releaseLine1.PartAttribute1Info);

			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "");
			releaseLine2.Quantity = 2m;
			releaseLine2.Validation.ValidatePartAttribute1();
			AssertHasError(releaseLine2.PartAttribute1Info, "Please enter a Part Attrib. 1.");

			releaseLine2.Quantity = 10m;
			releaseLine2.Validation.ValidatePartAttribute1();
			AssertNoErrors(releaseLine2.PartAttribute1Info);

			releaseLine2.Quantity = 5m;
			releaseLine1.Quantity = 5m;
			releaseLine1.PartAttribute1 = "";
			AssertHasError(releaseLine1.PartAttribute1Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");
		}

		#endregion

		#region TestCheckPartAttribute1_IsReleaseCapturedAttributeUnallocated

		public void TestCheckPartAttribute1_IsReleaseCapturedAttributeUnallocated()
		{
			AssertValidateForReleaseCapturedAttributeBeingAllocated(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestCheckPartAttribute2_IsReleaseCapturedAttributeUnallocated

		public void TestCheckPartAttribute2_IsReleaseCapturedAttributeUnallocated()
		{
			AssertValidateForReleaseCapturedAttributeBeingAllocated(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestCheckPartAttribute3_IsReleaseCapturedAttributeUnallocated

		public void TestCheckPartAttribute3_IsReleaseCapturedAttributeUnallocated()
		{
			AssertValidateForReleaseCapturedAttributeBeingAllocated(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestCheckSerialNumber_IsReleaseCapturedAttributeUnallocated

		public void TestCheckSerialNumber_IsReleaseCapturedAttributeUnallocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var info = releaseLine.SerialNumberInfo;
			AssertNoErrors("Precondition:", info);

			// There is stock allocated for this OrderLine, there should be no errors.
			releaseLine.SerialNumber = "SN1";
			AssertNoErrors(info);

			var newReleaseLine = orderLine.ReleaseLines.AddNew();
			var infoForNewReleaseLine = newReleaseLine.SerialNumberInfo;
			AssertNoErrors("Precondition:", newReleaseLine);

			newReleaseLine.Quantity = 1m;
			newReleaseLine.SerialNumber = "SN2";
			AssertHasError(infoForNewReleaseLine, "This Attribute cannot be Release Captured for the Product because there is not enough Stock Allocated to this Order Line.");
		}

		#endregion

		#region TestCheckPartAttribute1_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised

		public void TestCheckPartAttribute1_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised()
		{
			AssertPartAttribDoesNotValidateReleaseCapturedAttributesUnlessFinalisingOrFinalised(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1, line => line.PartAttribute1Info);
		}

		#endregion

		#region TestCheckPartAttribute2_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised

		public void TestCheckPartAttribute2_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised()
		{
			AssertPartAttribDoesNotValidateReleaseCapturedAttributesUnlessFinalisingOrFinalised(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2, line => line.PartAttribute2Info);
		}

		#endregion

		#region TestCheckPartAttribute3_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised

		public void TestCheckPartAttribute3_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised()
		{
			AssertPartAttribDoesNotValidateReleaseCapturedAttributesUnlessFinalisingOrFinalised(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3, line => line.PartAttribute3Info);
		}

		#endregion

		#region TestCheckSerialNumber_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised

		public void TestCheckSerialNumber_DoesNotValidateForReleaseCapturedAttributesUnlessFinalisingOrFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "SN1");
			inventory.InDocketLine.WE_SerialNumber = "SN1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;

			var propertyInfo = releaseLine.SerialNumberInfo;
			releaseLine.SerialNumber = "";
			AssertHasError(propertyInfo, "Please enter a Serial Number.");

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			releaseLine.SerialNumber = "";
			AssertNoErrors(propertyInfo);

			releaseLine.SerialNumber = "SN1";
			AssertNoErrors(propertyInfo);

			using (new SemaphoreManager(order.FinaliseDocketSemaphore))
			{
				releaseLine.SerialNumber = "";
				AssertHasError(propertyInfo, "Please enter a Serial Number.");
			}

			releaseLine.SerialNumber = "SN1";
			AssertNoErrors(propertyInfo);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			releaseLine.SerialNumber = "";
			AssertHasError(propertyInfo, "Please enter a Serial Number.");

			releaseLine.SerialNumber = "SN1";
			AssertNoErrors(propertyInfo);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			releaseLine.SerialNumber = "";
			AssertNoErrors("Should not validate once the pick is finalised.", propertyInfo);
		}

		#endregion

		#region TestCheckPartAttribute1_ValidationOfReleaseCapturedAttributesWhenPacked

		public void TestCheckPartAttribute1_ValidationOfReleaseCapturedAttributesWhenPacked()
		{
			AssertValidateForReleaseCapturedAttributeAfterBeingPacked(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestCheckPartAttribute2_ValidationOfReleaseCapturedAttributesWhenPacked

		public void TestCheckPartAttribute2_ValidationOfReleaseCapturedAttributesWhenPacked()
		{
			AssertValidateForReleaseCapturedAttributeAfterBeingPacked(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestCheckPartAttribute3_ValidationOfReleaseCapturedAttributesWhenPacked

		public void TestCheckPartAttribute3_ValidationOfReleaseCapturedAttributesWhenPacked()
		{
			AssertValidateForReleaseCapturedAttributeAfterBeingPacked(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestCheckSerialNumber_ValidationOfReleaseCapturedAttributesWhenPacked

		public void TestCheckSerialNumber_ValidationOfReleaseCapturedAttributesWhenPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, releaseLine.Quantity);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine.IsPacked);

			using (new SemaphoreManager(order.FinaliseDocketSemaphore))
			{
				var info = releaseLine.SerialNumberInfo;
				AssertNoErrors("Precondition:", info);

				releaseLine.SerialNumber = "";
				AssertHasError(info, "Please enter a Serial Number. Release Captured Attributes should be entered prior to Packing when not using RF. Unpack items, enter Release Captured Attributes and then Pack.");
			}
		}

		#endregion

		#region TestCheckPartAttribute1

		public void TestCheckPartAttribute1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];

			using (new PartAttributeValidationChecker.AttributeCallChecker(releaseLine.Client, releaseLine.SupplierPart, releaseLine.PartAttribute1Info, 1))
			{
				releaseLine.PartAttribute1 = "PA1-1";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine.Quantity = 0m;
				releaseLine.Validation.ValidatePartAttribute1();
			}

			releaseLine.Quantity = 10m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			using (releaseLine.GetValidationSuspender())
			{
				releaseLine.PartAttribute1 = "PA1-2";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine.Validation.ValidatePartAttribute1();
			}
		}

		public void TestCheckPartAttribute1_DuplicatedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			AssertNoErrors(releaseLine2.PartAttribute1Info);

			releaseLine2.PartAttribute1 = "PA1";
			AssertHasError(releaseLine2.PartAttribute1Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");

			releaseLine2.PartAttribute1 = "PA1_2";
			AssertNoError(releaseLine2.PartAttribute1Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");
		}

		#endregion

		#region TestCheckPartAttribute2

		public void TestCheckPartAttribute2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];

			using (new PartAttributeValidationChecker.AttributeCallChecker(releaseLine1.Client, releaseLine1.SupplierPart, releaseLine1.PartAttribute2Info, 2))
			{
				releaseLine1.PartAttribute2 = "PA2-1";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine1.Quantity = 0m;
				releaseLine1.Validation.ValidatePartAttribute2();
			}

			releaseLine1.Quantity = 10m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			using (releaseLine1.GetValidationSuspender())
			{
				releaseLine1.PartAttribute2 = "PA2-2";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine1.Validation.ValidatePartAttribute2();
			}
		}

		public void TestCheckPartAttribute2_DuplicatedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute2 = "PA2";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			AssertNoErrors(releaseLine2.PartAttribute2Info);

			releaseLine2.PartAttribute2 = "PA2";
			AssertHasError(releaseLine2.PartAttribute2Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");

			releaseLine2.PartAttribute2 = "PA2_2";
			AssertNoError(releaseLine2.PartAttribute2Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");
		}

		#endregion

		#region TestCheckPartAttribute3

		public void TestCheckPartAttribute3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];

			using (new PartAttributeValidationChecker.AttributeCallChecker(releaseLine.Client, releaseLine.SupplierPart, releaseLine.PartAttribute3Info, 3))
			{
				releaseLine.PartAttribute3 = "PA3-1";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine.Quantity = 0m;
				releaseLine.Validation.ValidatePartAttribute3();
			}

			releaseLine.Quantity = 10m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			using (releaseLine.GetValidationSuspender())
			{
				releaseLine.PartAttribute3 = "PA3-2";
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker())
			{
				releaseLine.Validation.ValidatePartAttribute3();
			}
		}

		public void TestCheckPartAttribute3_DuplicatedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute3 = "PA3";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			AssertNoErrors(releaseLine2.PartAttribute3Info);

			releaseLine2.PartAttribute3 = "PA3";
			AssertHasError(releaseLine2.PartAttribute3Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");

			releaseLine2.PartAttribute3 = "PA3_2";
			AssertNoError(releaseLine2.PartAttribute3Info, "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line.");
		}

		#endregion

		#region TestClearingReleaseCapturedAttributeWhenPicked

		public void TestClearingReleaseCapturedAttribute1WhenPicked()
		{
			AssertClearingReleaseCapturedAttributeWhenPicked(AttributeNumber.One, PartAttributeNumber.One);
		}

		public void TestClearingReleaseCapturedAttribute2WhenPicked()
		{
			AssertClearingReleaseCapturedAttributeWhenPicked(AttributeNumber.Two, PartAttributeNumber.Two);
		}

		public void TestClearingReleaseCapturedAttribute3WhenPicked()
		{
			AssertClearingReleaseCapturedAttributeWhenPicked(AttributeNumber.Three, PartAttributeNumber.Three);
		}

		#endregion

		#region TestPartAttributeValidationValidatesReleaseSerials

		public void TestPartAttributeValidationValidatesReleaseSerials_SerialNumberEnabled()
		{
			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "SN2");
				receiveLine1.InDocketLine.WE_SerialNumber = "SN2";
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
				Factory.Save();
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				var pick = Helper.CreatePickNew(order);
				var releaseLine1 = orderLine.ReleaseLines[0];
				releaseLine1.Quantity = 1m;
				releaseLine1.SerialNumber = "SN1";

				var releaseLine2 = orderLine.ReleaseLines.AddNew();
				releaseLine2.Quantity = 1m;
				AssertNoErrors(releaseLine2.SerialNumberInfo);

				releaseLine2.PartAttribute2 = "BLUE";
				releaseLine2.SerialNumber = "SN1";
				AssertHasError(releaseLine2.SerialNumberInfo, "Serial # already used.");

				releaseLine2.SerialNumber = "SN2";
				AssertNoErrors(releaseLine2.SerialNumberInfo);

				using (new SemaphoreManager(order.FinaliseDocketSemaphore))
				{
					releaseLine2.SerialNumber = "SN2";
					AssertHasError(releaseLine2.SerialNumberInfo, "Serial # already used.");
				}
			}
		}

		#endregion

		#region TestPartAttributeValidationValidatesReleaseSerials_WhenPacked

		public void TestPartAttributeValidationValidatesReleaseSerials_WhenPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			Helper.CreatePickNew(order1);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			Helper.CreatePickNew(order2);

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.SerialNumber = "SN1";
			AssertNoErrors(releaseLine2.SerialNumberInfo);

			using (new SemaphoreManager(order2.FinaliseDocketSemaphore))
			{
				releaseLine2.Validation.ValidateSerialNumber();
				AssertHasError(releaseLine2.SerialNumberInfo, "Serial # already used.");
			}

			releaseLine2.Validation.ValidateSerialNumber();
			AssertNoErrors(releaseLine2.SerialNumberInfo);

			var package = order2.PackageJob.Packages.AddNew();
			package.Pack(releaseLine2, 1m);

			using (new SemaphoreManager(order2.FinaliseDocketSemaphore))
			{
				releaseLine2.Validation.ValidateSerialNumber();
				AssertHasError(releaseLine2.SerialNumberInfo, "Serial # already used. The item must first be Unpacked before entering a new Serial Number.");
			}
		}

		#endregion

		#region TestPartAttributeValidationChecksReleaseLinesForDuplicateSerials

		public void TestPartAttributeValidationChecksReleaseLinesForDuplicateSerials()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m);
			Factory.Save();

			var orderWithPart1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLineWithPart1 = Helper.CreateWhsOrderLine(orderWithPart1, data.Part1, 1m);
			var pickWithPart1 = Helper.CreatePickNew(orderWithPart1);
			orderLineWithPart1.ReleaseLines[0].SerialNumber = "SN1";
			Factory.Save();

			var orderWithPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineWithPart2 = Helper.CreateWhsOrderLine(orderWithPart2, data.Part2, 1m);
			var pickWithPart2 = Helper.CreatePickNew(orderWithPart2);
			var releaseLine = orderLineWithPart2.ReleaseLines[0];

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"))
			{
				releaseLine.SerialNumber = "SN1";
				AssertNoErrors(releaseLine.SerialNumberInfo);

				using (new SemaphoreManager(orderWithPart2.FinaliseDocketSemaphore))
				{
					releaseLine.SerialNumber = "SN1";
					AssertHasError(releaseLine.SerialNumberInfo, "Serial # already used.");
				}
			}

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO"))
			{
				using (new SemaphoreManager(orderWithPart2.FinaliseDocketSemaphore))
				{
					releaseLine.SerialNumber = "SN1";
					AssertNoErrors(releaseLine.SerialNumberInfo);
				}

				var orderWithDuplicateSerialForPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
				var orderLineWithDuplicateSerialForPart2 = Helper.CreateWhsOrderLine(orderWithDuplicateSerialForPart2, data.Part2, 1m);
				Helper.CreatePickNew(orderWithDuplicateSerialForPart2);

				var releaseLineWithDuplicateSerialForPart2 = orderLineWithDuplicateSerialForPart2.ReleaseLines[0];
				releaseLineWithDuplicateSerialForPart2.SerialNumber = "SN1";
				AssertNoErrors(releaseLine.SerialNumberInfo);

				using (new SemaphoreManager(orderWithDuplicateSerialForPart2.FinaliseDocketSemaphore))
				{
					releaseLineWithDuplicateSerialForPart2.SerialNumber = "SN1";
					AssertHasError(releaseLineWithDuplicateSerialForPart2.SerialNumberInfo, "Serial # already used.");
				}
			}
		}

		#region TestPartAttributeValidationValidatesReleaseSerials_Swap

		public void TestPartAttributeValidationValidatesReleaseSerials_Swap()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"))
			{
				var orderWithPart1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLineWithPart1 = Helper.CreateWhsOrderLine(orderWithPart1, data.Part1, 1m);
				var pickWithPart1 = Helper.CreatePickNew(orderWithPart1);
				var releaseLine1 = orderLineWithPart1.ReleaseLines[0];
				releaseLine1.SerialNumber = "SN1";

				var orderWithPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLineWithPart2 = Helper.CreateWhsOrderLine(orderWithPart2, data.Part2, 1m);
				var pickWithPart2 = Helper.CreatePickNew(orderWithPart2);
				var releaseLine2 = orderLineWithPart2.ReleaseLines[0];
				releaseLine2.SerialNumber = "SN2";
				AssertNoErrors(releaseLine2.SerialNumberInfo);
				AssertNoExceptionThrown(() => Factory.Save());

				releaseLine1.SerialNumber = "SN2";
				releaseLine2.SerialNumber = "SN1";
				AssertNoErrors(releaseLine2.SerialNumberInfo);

				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#endregion

		public void TestPartAttributeValidationChecksReleaseLinesForDuplicateSerials_DuplicateOnFinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			Factory.Save();

			var orderWithPart1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLineWithPart1 = Helper.CreateWhsOrderLine(orderWithPart1, data.Part1, 1m);
			var pickWithPart1 = Helper.CreatePickNew(orderWithPart1);
			orderLineWithPart1.ReleaseLines[0].SerialNumber = "SN1";
			Factory.Save();

			var orderWithPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineWithPart2 = Helper.CreateWhsOrderLine(orderWithPart2, data.Part2, 1m);
			Helper.CreatePickNew(orderWithPart2);
			var releaseLine = orderLineWithPart2.ReleaseLines[0];

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"))
			{
				releaseLine.SerialNumber = "SN1";
				AssertNoErrors(releaseLine.SerialNumberInfo);

				using (new SemaphoreManager(orderWithPart2.FinaliseDocketSemaphore))
				{
					releaseLine.SerialNumber = "SN1";
					AssertHasError(releaseLine.SerialNumberInfo, "Serial # already used.");
				}

				pickWithPart1.FinaliseAllOrders();
				pickWithPart1.FinalisePick();
				AssertIsFinalisedPrecondition(orderWithPart1);
				AssertIsFinalisedPrecondition(pickWithPart1);
				Factory.Save();

				using (new SemaphoreManager(orderWithPart2.FinaliseDocketSemaphore))
				{
					releaseLine.SerialNumber = "SN1";
					AssertNoErrors(releaseLine.SerialNumberInfo);
				}
			}
		}

		public void TestPartAttributeValidationChecksReleaseLinesForDuplicateSerials_DuplicateOnFinalisedPick_ProductLevelSerialUniqueness()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var orderWithPart1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLineWithPart1 = Helper.CreateWhsOrderLine(orderWithPart1, data.Part1, 1m);
			Helper.CreatePickNew(orderWithPart1);
			orderLineWithPart1.ReleaseLines[0].SerialNumber = "SN1";

			var orderWithPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineWithPart2 = Helper.CreateWhsOrderLine(orderWithPart2, data.Part2, 1m);
			var pick2 = Helper.CreatePickNew(orderWithPart2);
			orderLineWithPart2.ReleaseLines[0].SerialNumber = "SN1";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO"))
			{
				var orderWithDuplicateSerialForPart2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
				var orderLineWithDuplicateSerialForPart2 = Helper.CreateWhsOrderLine(orderWithDuplicateSerialForPart2, data.Part2, 1m);
				Helper.CreatePickNew(orderWithDuplicateSerialForPart2);

				var releaseLineWithDuplicateSerialForPart2 = orderLineWithDuplicateSerialForPart2.ReleaseLines[0];
				releaseLineWithDuplicateSerialForPart2.SerialNumber = "SN1";
				AssertNoErrors(releaseLineWithDuplicateSerialForPart2.SerialNumberInfo);

				using (new SemaphoreManager(orderWithDuplicateSerialForPart2.FinaliseDocketSemaphore))
				{
					releaseLineWithDuplicateSerialForPart2.SerialNumber = "SN1";
					AssertHasError(releaseLineWithDuplicateSerialForPart2.SerialNumberInfo, "Serial # already used.");
				}

				pick2.FinaliseAllOrders();
				pick2.FinalisePick();
				AssertIsFinalisedPrecondition(orderWithPart2);
				AssertIsFinalisedPrecondition(pick2);
				Factory.Save();

				using (new SemaphoreManager(orderWithDuplicateSerialForPart2.FinaliseDocketSemaphore))
				{
					releaseLineWithDuplicateSerialForPart2.SerialNumber = "SN1";
					AssertNoErrors(releaseLineWithDuplicateSerialForPart2.SerialNumberInfo);
				}
			}
		}

		public void TestPartAttributeValidationChecksReleaseLinesForDuplicateSerials_DuplicateOnOtherClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("ORG3");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(org2, AttributeNumber.Serial, false);
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.Serial, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var orderWithPart1Client1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLineWithPart1 = Helper.CreateWhsOrderLine(orderWithPart1Client1, data.Part1, 1m);
			Helper.CreatePickNew(orderWithPart1Client1);
			orderLineWithPart1.ReleaseLines[0].SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "SN1");
			Factory.Save();

			var orderWithPart1Client2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2");
			var orderLineWithClient2 = Helper.CreateWhsOrderLine(orderWithPart1Client2, data.Part1, 1m);
			Helper.CreatePickNew(orderWithPart1Client2);
			var releaseLineClient2 = orderLineWithClient2.ReleaseLines[0];

			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI"))
			{
				releaseLineClient2.SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "SN1");
				AssertNoErrors(releaseLineClient2.SerialNumberInfo);

				using (new SemaphoreManager(orderWithPart1Client2.FinaliseDocketSemaphore))
				{
					releaseLineClient2.SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "SN1");
					AssertNoErrors(releaseLineClient2.SerialNumberInfo);
				}
			}
		}

		#endregion

		#region TestPartAttributeValidationChecksSerialNumberAfterFinalizeOrder

		public void TestPartAttributeValidationChecksSerialNumberAfterFinalizeOrder()
		{
			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO"))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
				var pick1 = Helper.CreatePickNew(order1);
				var releaseLine1_1 = orderLine1.ReleaseLines[0];
				var releaseLine1_2 = orderLine1.ReleaseLines.AddNew();
				releaseLine1_1.PartAttribute2 = "RED";
				releaseLine1_2.PartAttribute2 = "BLUE";
				releaseLine1_1.SerialNumber = "SN1";
				releaseLine1_2.SerialNumber = "SN1";
				releaseLine1_1.Quantity = 1m;
				releaseLine1_2.Quantity = 1m;
				releaseLine1_2.Validation.ValidateSerialNumber();
				AssertHasError(releaseLine1_2.SerialNumberInfo, "Serial # already used.");

				pick1.FinaliseAllOrders();
				AssertEquals("Precondition", false, order1.IsFinalised);

				releaseLine1_1.SerialNumber = "SN1";
				releaseLine1_2.SerialNumber = "SN2";
				releaseLine1_2.Validation.ValidateSerialNumber();
				AssertNoErrors(releaseLine1_2.SerialNumberInfo);

				releaseLine1_1.Validation.ValidateSerialNumber();
				pick1.FinaliseAllOrders();
				AssertIsFinalisedPrecondition(order1);

				Factory.Save();
				releaseLine1_2.SerialNumber = "SN1";
				pick1.FinalisePick();
				Factory.Save();
				AssertEquals("Cannot finalize pick is serial no is changed to non unique.", false, pick1.IsFinalised);
				AssertHasError(releaseLine1_2.SerialNumberInfo, "Serial # already used.");

				releaseLine1_2.SerialNumber = "SN3";
				pick1.FinalisePick();
				AssertEquals("serial no is unique.", true, pick1.IsFinalised);
			}
		}

		#endregion

		void AssertPartAttribDoesNotValidateReleaseCapturedAttributesUnlessFinalisingOrFinalised(AttributeNumber attributeNumber, string attributeColumn, Func<WhsReleaseLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory, attributeName: "Colour");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			switch (attributeNumber)
			{
				case AttributeNumber.One:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "RED", "", "", "");
					break;
				case AttributeNumber.Two:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
					break;
				case AttributeNumber.Three:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "", "RED", "");
					break;
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;

			var propertyInfo = getPropertyInfo(releaseLine);
			releaseLine[attributeColumn] = "";
			AssertHasError(propertyInfo, "Please enter a Colour.");

			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
			releaseLine[attributeColumn] = "";
			AssertNoErrors(propertyInfo);

			releaseLine[attributeColumn] = "BLUE";
			AssertNoErrors(propertyInfo);

			using (new SemaphoreManager(order.FinaliseDocketSemaphore))
			{
				releaseLine[attributeColumn] = "";
				AssertHasError(propertyInfo, "Please enter a Colour.");
			}

			releaseLine[attributeColumn] = "BLUE";
			AssertNoErrors(propertyInfo);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			releaseLine[attributeColumn] = "";
			AssertHasError(propertyInfo, "Please enter a Colour.");
		}

		void AssertValidateForReleaseCapturedAttributeAfterBeingPacked(AttributeNumber attributeNumber, string attributeColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory, attributeName: "Colour");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, releaseLine.Quantity);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine.IsPacked);

			using (new SemaphoreManager(order.FinaliseDocketSemaphore))
			{
				var info = releaseLine.ZPropertyInfoHash[attributeColumn];
				AssertNoErrors("Precondition:", info);

				releaseLine[attributeColumn] = "";
				AssertHasError(info, "Please enter a Colour. Release Captured Attributes should be entered prior to Packing when not using RF. Unpack items, enter Release Captured Attributes and then Pack.");
			}
		}

		void AssertValidateForReleaseCapturedAttributeBeingAllocated(AttributeNumber releaseCapturedAttrib, string releaseCapturedAttribColumn)
		{
			var partAttribs = new[]
			{
				new { Attrib = AttributeNumber.One, Column = WhsReleaseLine.Schema.PartAttribute1 },
				new { Attrib = AttributeNumber.Two, Column = WhsReleaseLine.Schema.PartAttribute2 },
				new { Attrib = AttributeNumber.Three, Column = WhsReleaseLine.Schema.PartAttribute3 },
			};
			var nonReleaseCapturedAttrib = partAttribs.First(a => a.Attrib != releaseCapturedAttrib);

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, releaseCapturedAttrib, PartAttributeTypeList.Codes.Mandatory, attributeName: "Colour");
			Helper.SetClientAttributeType(data.Org1, nonReleaseCapturedAttrib.Attrib, PartAttributeTypeList.Codes.Mandatory, attributeName: "Size");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, releaseCapturedAttrib, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, nonReleaseCapturedAttrib.Attrib, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			switch (nonReleaseCapturedAttrib.Attrib)
			{
				case AttributeNumber.One:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
					break;
				case AttributeNumber.Two:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "MEDIUM", "", "");
					break;
				case AttributeNumber.Three:
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "", "MEDIUM", "");
					break;
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var info = releaseLine.ZPropertyInfoHash[releaseCapturedAttribColumn];
			AssertNoErrors("Precondition:", info);

			// There is stock allocated for this OrderLine, there should be no errors.
			releaseLine[releaseCapturedAttribColumn] = "RED";
			AssertNoErrors(info);

			var newReleaseLine = orderLine.ReleaseLines.AddNew();
			var infoForNewReleaseLine = newReleaseLine.ZPropertyInfoHash[releaseCapturedAttribColumn];
			AssertNoErrors("Precondition:", newReleaseLine);

			newReleaseLine.Quantity = 1m;
			newReleaseLine[releaseCapturedAttribColumn] = "BLUE";
			AssertHasError(infoForNewReleaseLine, "This Attribute cannot be Release Captured for the Product because there is not enough Stock Allocated to this Order Line.");

			// Non-Release Captured Attributes should not get an Error.
			var infoForNextAttrib = newReleaseLine.ZPropertyInfoHash[nonReleaseCapturedAttrib.Column];
			newReleaseLine[nonReleaseCapturedAttrib.Column] = "MEDIUM";
			AssertNoErrors(infoForNextAttrib);

			newReleaseLine[nonReleaseCapturedAttrib.Column] = "LARGE";
			AssertNoErrors(infoForNextAttrib);
		}

		void AssertClearingReleaseCapturedAttributeWhenPicked(AttributeNumber releaseCapturedAttrib, PartAttributeNumber partAttribute)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, releaseCapturedAttrib, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, releaseCapturedAttrib, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			orderLine.ClearReleaseLines();

			var releaseCapturedPickLine1 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-2");
			var releaseCapturedPickLine2 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-1");

			switch (releaseCapturedAttrib)
			{
				case AttributeNumber.One:
					releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib1 = "BATCH123";
					releaseCapturedPickLine2.WZ_ReleaseCapturedPartAttrib1 = "BATCH456";
					break;
				case AttributeNumber.Two:
					releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib2 = "BATCH123";
					releaseCapturedPickLine2.WZ_ReleaseCapturedPartAttrib2 = "BATCH456";
					break;
				case AttributeNumber.Three:
					releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib3 = "BATCH123";
					releaseCapturedPickLine2.WZ_ReleaseCapturedPartAttrib3 = "BATCH456";
					break;
				default: throw new ArgumentException("Invalid Attribute: " + releaseCapturedAttrib);
			}

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine1 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.GetPartAttribute(partAttribute) == "BATCH123");
			var releaseLine2 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.GetPartAttribute(partAttribute) == "BATCH456");
			AssertEquals("Release Line Quantity should be correct.", 5m, releaseLine1.Quantity);
			AssertEquals("Release Line Quantity should be correct.", 10m, releaseLine2.Quantity);

			releaseCapturedPickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			releaseCapturedPickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			Action<ZString> attribSetter = releaseCapturedAttrib switch
			{
				AttributeNumber.One => (ZString value) => releaseLine1.PartAttribute1 = value,
				AttributeNumber.Two => (ZString value) => releaseLine1.PartAttribute2 = value,
				AttributeNumber.Three => (ZString value) => releaseLine1.PartAttribute3 = value,
				_ => throw new ArgumentException("Invalid Attribute: " + releaseCapturedAttrib),
			};

			attribSetter("");

			var pickLineValue = releaseCapturedAttrib switch
			{
				AttributeNumber.One => releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib1,
				AttributeNumber.Two => releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib2,
				AttributeNumber.Three => releaseCapturedPickLine1.WZ_ReleaseCapturedPartAttrib3,
				_ => throw new ArgumentException("Invalid Attribute: " + releaseCapturedAttrib),
			};

			var info = releaseCapturedAttrib switch
			{
				AttributeNumber.One => releaseLine1.PartAttribute1Info,
				AttributeNumber.Two => releaseLine1.PartAttribute2Info,
				AttributeNumber.Three => releaseLine1.PartAttribute3Info,
				_ => throw new ArgumentException("Invalid Attribute: " + releaseCapturedAttrib),
			};

			AssertEquals("PickLine should be unchanged", "BATCH123", pickLineValue);
			AssertNoErrors("No errors as RunPreSaveValidation has not run.", info);
			AssertNoRowErrors("No errors as RunPreSaveValidation has not run.", orderLine);
			releaseLines.RunPreSaveValidation();

			AssertHasRowError(orderLine, $@"There are 5x [Part Attrib. {(int)releaseCapturedAttrib + 1}: BATCH123] Release Captured Attributes for Product P1 but only 0 released on this Order Line.
This may have been caused by entering Release Captured Attributes in {Core.Constants.ProductName} before Release Capturing Attributes in RF. You must update the Release Lines for this Order line to match the Release Captured Quantity.");

			attribSetter("BATCH123");
			AssertNoErrors("No errors as attribute is correct again.", info);

			releaseLines.RunPreSaveValidation();
			AssertNoRowErrors("No errors as attribute is correct again.", orderLine);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Order should successfully finalise.", true, order.IsFinalised);

			pick.FinalisePick();
			AssertEquals("Pick should successfully finalise.", true, pick.IsFinalised);
		}

		#region TestCheckSerialNumber

		public void TestCheckSerialNumber_QtyGreaterThanOne()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertNoErrors("No errors.", releaseLine.QuantityInfo);

			releaseLine.SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "Ser");
			releaseLine.Validation.ValidateQuantity();
			AssertHasError("Errors for serial number and 10 Qty", releaseLine.QuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			releaseLine.Quantity = 1m;
			AssertNoErrors("Precondition: No errors for serial number and 1 Qty", releaseLine.QuantityInfo);

			releaseLine.SetupReleaseLine("", "", "", ZDate.Empty, ZDate.Empty, "");
			releaseLine.Quantity = 10m;
			AssertNoErrors("No errors for No serial number and 10 Qty", releaseLine.QuantityInfo);
		}

		#endregion

		#endregion

		#region TestCheckQuantity

		public void TestCheckQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			var order = Helper.CreateWhsOrder(data.Org2, data.Whs1, "1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			// check all is ok before altering pick
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = FindPickInventory(orderedInventory, "PA1", 0m);
			AssertNotNull("Precondition", availableInventory);

			availableInventory.PickLineQuantity = 10m; // do this b4 AlterPick so OrderLine.ReleaseLines are set
			AssertEquals("Precondition", 10m, orderedInventory.PickLineQuantity);
			AssertEquals("Precondition", 10m, orderLine.ReleaseLines[0].Quantity);

			// pretend this is a MOP, to ensure Alter Pick does not change Units Sent...
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			pick.Orders.Add(order2);
			order.WD_FinalisedDate = ZDateTimeOffset.Now;

			// alter the pick
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 4;
			AssertEquals("Precondition", 4m, orderedInventory.PickLineQuantity);
			orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "PA1").Quantity = 10m;

			var availableInventory1 = FindPickInventory(orderedInventory, "PA12", 0m);
			AssertNotNull("Precondition", availableInventory1);

			availableInventory1.PickLineQuantity = 6;
			orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "PA12").Quantity = 0m;
			AssertEquals("Precondition", 10m, orderedInventory.PickLineQuantity);

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(order2);

			pick.FinalisePick();
			AssertEquals("Precondition: Pick is not finalised.", false, pick.IsFinalised);

			// half fix mismatch
			orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "PA1").Quantity = 4m;
			pick.FinaliseAllOrders();
			pick.FinalisePick();

			// fix mismatch
			var releaseLine = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "PA12");
			releaseLine.Quantity = 6m;
			releaseLine.Validation.ValidateQuantity();

			releaseLine.Quantity = 2m;
			AssertHasWarnings("Should have shortfall warning", orderLine.SumOfUnitsMetInfo);

			releaseLine.Quantity = 6m;
			AssertNoWarnings("Warning should be cleared", orderLine.SumOfUnitsMetInfo);

			pick.FinalisePick();
			AssertEquals("Pick should now be finalised", true, pick.IsFinalised);
		}

		WhsPickAvailableInventory FindPickInventory(WhsPickOrderedInventory orderedInventory, ZString attrib1, ZDecimal allocated)
		{
			return orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>()
				.FirstOrDefault(a => a.PartAttrib1 == attrib1 && a.PickLineQuantity == allocated);
		}

		#endregion

		#region TestCheckQuantity_AddsErrorWhenReleaseQuantityIsZero

		public void TestCheckQuantity_AddsErrorWhenReleaseQuantityIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			AssertNoWarnings("Precondition", releaseLine.QuantityInfo);

			releaseLine.Quantity = 0m;
			AssertHasError(releaseLine.QuantityInfo, "You cannot release 0 units.");
		}

		#endregion

		#region TestCheckQuantity_DoesNotAllowNegative

		public void TestCheckQuantity_DoesNotAllowNegative()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			releaseLine.Quantity = -1;
			AssertHasError(releaseLine.QuantityInfo, "Quantity Met cannot be negative.");
		}

		#endregion

		#region TestCheckQuantity_DoesNotReduceBelowPackedQty

		public void TestCheckQuantity_DoesNotReduceBelowPackedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, order.Lines[0].PickLineQuantity);
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			// pack 5 items
			var package = order.PackageJob.Packages.AddNew("BOX");
			package.Pack(order.Lines[0].ReleaseLines[0], 5);

			// lower the picked qty to 3 (even though 5 are packed)
			releaseLine.Quantity = 3;
			AssertHasError(releaseLine.QuantityInfo, "This item is packed.\r\n\r\nQuantity released (3) cannot be less than quantity packed (5). Reduce the quantity packed first.");
			AssertEquals("Validation should use the Quantity that was reverted.", 3m, releaseLine.InvalidQuantityThatWasReversed);
			AssertEquals("Quantity should have been reverted.", 10m, releaseLine.Quantity);

			releaseLine.Quantity = 5m;
			AssertNoErrors(releaseLine.QuantityInfo);

			var duplicateReleaseLine = order.Lines[0].ReleaseLines.AddNew();
			duplicateReleaseLine.Quantity = 1m;
			AssertNoErrors(duplicateReleaseLine.QuantityInfo);
		}

		#endregion

		#region TestCheckQuantity_IsSavedFromOrderForm

		public void TestCheckQuantity_IsSavedFromOrderForm()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition", 10m, order.Lines[0].PickLineQuantity);
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			releaseLine.Quantity = 12m;
			AssertHasError(releaseLine.QuantityInfo, "Quantity Met cannot be greater than Quantity Ordered");

			releaseLine.Quantity = 10m;
			AssertNoErrors(releaseLine.QuantityInfo);

			order.IsSavedFromOrderForm = true;
			releaseLine.Quantity = 12m;
			AssertNoErrors(releaseLine.QuantityInfo);
		}

		#endregion

		#region TestCheckQuantity_QuantityMetCannotBeGreaterThanOrderedQty

		public void TestCheckQuantity_QuantityMetCannotBeGreaterThanOrderedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertNoErrors(releaseLine.QuantityInfo);

			releaseLine.Quantity = 11m;
			AssertHasError(releaseLine.QuantityInfo, "Quantity Met cannot be greater than Quantity Ordered");

			releaseLine.Quantity = 10m;
			AssertNoErrors(releaseLine.QuantityInfo);
		}

		#endregion

		#region TestCheckQuantity_ReleaseCapturedAttribs_PickLinesNotOverReleased

		public void TestCheckQuantity_ReleaseCapturedAttribs_PickLinesNotOverReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			releaseLine.PartAttribute1 = "RED";
			AssertNoErrors(releaseLine.PartAttribute1Info);
			AssertNoErrors(releaseLine.QuantityInfo);

			releaseLine.Quantity = 6m;
			AssertHasError(releaseLine.QuantityInfo, "The Release Captured Quantity for this Product cannot be greater than the Quantity Allocated for this Order Line.");
		}

		#endregion

		#region TestCheckQuantity_ReleaseCapturedAttribs_PickLinesNotUnderReleased

		public void TestCheckQuantity_ReleaseCapturedAttribs_PickLinesNotUnderReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			releaseLine.PartAttribute1 = "RED";
			AssertNoErrors(releaseLine.PartAttribute1Info);
			AssertNoErrors(releaseLine.QuantityInfo);

			releaseLine.Quantity = 4m;
			AssertNoErrors(releaseLine.QuantityInfo);

			orderLine.PickLines.ForEach(l => l.WZ_PickedDateTime = ZDateTimeOffset.Now);
			releaseLine.Validation.ValidateQuantity();
			AssertNoErrors("Should not be any errors as quantity was reduced before Picking.", releaseLine.QuantityInfo);

			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertHasError(releaseLine.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");

			releaseLine.Quantity = 5m;
			orderLine.ReleaseLines.RunPreSaveValidation();
			AssertNoErrors(releaseLine.UnreleasedQtyInfo);
		}

		public void TestCheckQuantity_ReleaseCapturedAttribs_PickLinesNotUnderReleased_PickedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			AssertNoErrors("Precondition", releaseLine.QuantityInfo);

			releaseLine.PartAttribute1 = "RED";
			AssertNoErrors(releaseLine.PartAttribute1Info);
			AssertNoErrors(releaseLine.QuantityInfo);

			orderLine.PickLines.ForEach(l => l.WZ_PickedDateTime = ZDateTimeOffset.Now);
			releaseLine.Quantity = 4m;
			releaseLine.Validation.ValidateQuantity();
			AssertHasError(releaseLine.QuantityInfo, "You must Release Capture the same Quantity that has been Picked for this Order Line.");

			releaseLine.Quantity = 5m;
			AssertNoErrors(releaseLine.QuantityInfo);
		}

		#endregion

		#region TestValidateUnreleasedQty

		public void TestValidateUnreleasedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 0m;
			AssertEquals("Precondition", 10m, releaseLine.UnreleasedQty);

			releaseLine.Validation.ValidateUnreleasedQty();
			AssertNoErrors("Precondition", releaseLine.UnreleasedQtyInfo);

			releaseLine.RunPreSaveValidation();
			AssertHasError(releaseLine.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");

			releaseLine.Quantity = 10m;
			AssertNoErrors(releaseLine.UnreleasedQtyInfo);

			releaseLine.RunPreSaveValidation();
			AssertNoErrors(releaseLine.UnreleasedQtyInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];

			var partAttrib1Validated = 0;
			var partAttrib2Validated = 0;
			var partAttrib3Validated = 0;
			var quantityValidated = 0;
			var serialNumberValidated = 0;
			releaseLine.PartAttribute1Info.AdditionalValidation += () => partAttrib1Validated++;
			releaseLine.PartAttribute2Info.AdditionalValidation += () => partAttrib2Validated++;
			releaseLine.PartAttribute3Info.AdditionalValidation += () => partAttrib3Validated++;
			releaseLine.SerialNumberInfo.AdditionalValidation += () => serialNumberValidated++;
			releaseLine.QuantityInfo.AdditionalValidation += () => quantityValidated++;

			releaseLine.Validation.ValidateAll();
			CombineAssertions("Should validate all fields when calling ValidateAll().", () =>
			{
				AssertEquals("Part Attribute 1 should have been validated.", 1, partAttrib1Validated);
				AssertEquals("Part Attribute 2 should have been validated.", 1, partAttrib2Validated);
				AssertEquals("Part Attribute 3 should have been validated.", 1, partAttrib3Validated);
				AssertEquals("Serial Number should have been validated.", 1, serialNumberValidated);
				AssertEquals("Quantity should have been validated.", 1, quantityValidated);
			});
		}

		#endregion
	}
}
