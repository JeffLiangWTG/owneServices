using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvLineComponentInventoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJIV_QuantityToDraw()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationMock = Factory.NewMoq<BaseJobDeclarationForTesting>();
				declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
				declarationMock.Setup(m => m.IsAllocatedQuantityRequiredForBondedWarehouse).Returns(true);

				var declaration = declarationMock.Object;
				var helperMock = new Mock<BondedWarehousingHelper>(declaration);
				helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
				declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);

				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				var helper = new WhsDataTestHelper(Factory);
				var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
				var whsInventory = helper.GetNewReceiveInventory(whsReceive, helper.Part, ZString.Empty, 10m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", "SN1", "EN00123-1");
				whsInventory.WI_AllocationKey = "WI123";

				var inventory = Factory.New<JobComInvLineComponentInventory>();
				inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
				invoiceLine.ComponentInventoryCollection.Add(inventory);
				invoiceLine.SetPartForTesting(helper.Part);

				AssertEquals("Available Quantity on Hand", 10m, inventory.QuantityOnHand);

				inventory.JIV_QuantityToDraw = 0;
				inventory.Validation.ValidateJIV_QuantityToDraw();
				AssertHasMessageError("Error when JIV_QuantityToDraw is less than or equal to 0", inventory.JIV_QuantityToDrawInfo, "Please enter a Quantity to Draw that is greater than 0 and does not exceed the available Quantity on Hand for Inventory Management integration.");

				inventory.JIV_QuantityToDraw = 11;
				inventory.Validation.ValidateJIV_QuantityToDraw();
				AssertHasMessageError("Error when JIV_QuantityToDraw is greater than QuantityOnHand", inventory.JIV_QuantityToDrawInfo, "Please enter a Quantity to Draw that is greater than 0 and does not exceed the available Quantity on Hand for Inventory Management integration.");

				inventory.JIV_QuantityToDraw = 5;
				inventory.Validation.ValidateJIV_QuantityToDraw();
				AssertNoError("No error when JIV_QuantityToDraw is greater than 0 and less than QuantityOnHand", inventory.JIV_QuantityToDrawInfo, "Please enter a Quantity to Draw that is greater than 0 and does not exceed the available Quantity on Hand for Inventory Management integration.");
			}
		}
	}
}
