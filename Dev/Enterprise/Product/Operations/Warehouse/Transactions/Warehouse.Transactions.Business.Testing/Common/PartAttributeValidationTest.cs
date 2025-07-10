using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PartAttributeValidationTest : MasterFiles.Business.Testing.PartAttributeValidationTest
	{
		#region TestSerialNumberValidation_ValidatesLineUnitsWithinTheCorrectPropertysValidation

		public void TestSerialNumberValidation_ValidatesLineUnitsWithinTheCorrectPropertysValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1", ZDateTimeOffset.Today);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory.WI_SerialNumber = "SERIAL123";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(false, receive.IsFinalised);
			AssertHasError(inventory.InDocketLine.WE_TransactionQuantityInfo, "Must always be 1 or less for serial number controlled products");
		}

		#endregion

		#region TestCheckSerialNumber_WithSerialNumberAdjustedOut

		public void TestCheckSerialNumber_WithSerialNumberAdjustedOut()
		{
			var client = Helper.CreateClient("TestClient");
			var part = Helper.CreateProduct(client, "TestProduct");
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			client.PartAttributeManager.SetProductToUseAttribute(part, 6, true);
			client.MiscServ.OM_IMUseSerialNumber = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client.PK, whs.PK, "Reference1", ZDateTimeOffset.Today);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, whs.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory.WI_SerialNumber = "SERIAL123";
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(client, whs);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, part, 1m, whs.DefaultLocation);
			line1.WE_SerialNumber = "SERIAL123";
			adjustment.RunPreSaveValidation();

			var validation = new PartAttributeValidation(line1);
			using (line1.SuspendValidationTesting())
			{
				line1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(client, part, line1.WE_SerialNumberInfo);
				AssertHasError("SERIAL123 is already in inventory", line1.WE_SerialNumberInfo, "Serial # already used.");

				var line2 = Helper.CreateWhsAdjustmentLine(adjustment, part, -1m, whs.DefaultLocation);
				line2.WE_SerialNumber = "SERIAL123";
				adjustment.RunPreSaveValidation();

				line1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(client, part, line1.WE_SerialNumberInfo);
				AssertNoError("SERIAL123 is being adjusted out", line1.WE_SerialNumberInfo, "Serial # already used.");

				line2.WE_SerialNumber = "SERIAL456";
				adjustment.RunPreSaveValidation();

				line1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(client, part, line1.WE_SerialNumberInfo);
				AssertHasError("SERIAL123 is no longer being adjusted out", line1.WE_SerialNumberInfo, "Serial # already used.");
			}
		}

		#endregion

		#region TestCheckSerialNumber_WithInTransitQty

		public void TestCheckSerialNumber_WithInTransitQty()
		{
			var client = Helper.CreateClient("TestClient");
			var part = Helper.CreateProduct(client, "TestProduct");
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			client.PartAttributeManager.SetProductToUseAttribute(part, 6, true);
			client.MiscServ.OM_IMUseSerialNumber = true;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client.PK, whs.PK, "Reference1", ZDateTimeOffset.Today);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 1m, whs.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SERIAL123";
			receive1.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, "O1", part, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			AssertEquals("Precondition: Stock On Hand.", 1m, inventory1.WI_TotalUnits);
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			AssertEquals("Precondition: No Stock On Hand.", 0m, inventory1.WI_TotalUnits);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(client, whs);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, part, 1m, whs.Rows[0].Locations[0]);
			adjustmentLine.WE_SerialNumber = "SERIAL123";

			var validation = new PartAttributeValidation(adjustmentLine);
			using (adjustmentLine.SuspendValidationTesting())
			{
				adjustmentLine.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(client, part, adjustmentLine.WE_SerialNumberInfo);
				AssertHasError("SERIAL123 is already in inventory", adjustmentLine.WE_SerialNumberInfo, "Serial # already used.");

				pick.FinaliseOrder(order);
				pick.FinalisePick();
				Factory.Save();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

				adjustmentLine.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(client, part, adjustmentLine.WE_SerialNumberInfo);
				AssertNoError("SERIAL123 no longer exists.", adjustmentLine.WE_SerialNumberInfo, "Serial # already used.");
			}
		}

		#endregion

		#region TestMustBeOneOrLessForSerialNumberProductsMessage

		public void TestMustBeOneOrLessForSerialNumberProductsMessage()
		{
			AssertEquals("Must always be 1 or less for serial number controlled products", PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
		}

		#endregion

		#region TestValidateSerialIsNotReleaseCapturedWithValue

		public void TestValidateSerialIsNotReleaseCapturedWithValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				receiveLine.WE_SerialNumber = "SER";
				AssertHasError(receiveLine.WE_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");

				receiveLine.WE_SerialNumber = "";
				AssertNoError(receiveLine.WE_SerialNumberInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");
			}
		}

		#endregion

		#region TestCheckSerialNumberIsUniqueCore_UsesNewColumn

		public void TestCheckSerialNumberIsUniqueCore_UsesNewColumn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SERIAL123";
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentline1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			adjustmentline1.WE_SerialNumber = "SERIAL123";
			adjustment.RunPreSaveValidation();

			var validation = new PartAttributeValidation(adjustmentline1);
			using (adjustmentline1.SuspendValidationTesting())
			{
				adjustmentline1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(data.Org1, data.Part1, adjustmentline1.WE_SerialNumberInfo);
				AssertHasError("SERIAL123 is already in inventory", adjustmentline1.WE_SerialNumberInfo, "Serial # already used.");

				var line2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, data.Whs1.DefaultLocation);
				line2.WE_SerialNumber = "SERIAL123";
				adjustment.RunPreSaveValidation();

				adjustmentline1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(data.Org1, data.Part1, adjustmentline1.WE_SerialNumberInfo);
				AssertNoError("SERIAL123 is being adjusted out", adjustmentline1.WE_SerialNumberInfo, "Serial # already used.");

				line2.WE_SerialNumber = "SERIAL456";
				adjustment.RunPreSaveValidation();

				adjustmentline1.WE_SerialNumberInfo.ClearAllNotifications();
				validation.CheckSerialNumber(data.Org1, data.Part1, adjustmentline1.WE_SerialNumberInfo);
				AssertHasError("SERIAL123 is no longer being adjusted out", adjustmentline1.WE_SerialNumberInfo, "Serial # already used.");
			}
		}

		#endregion

		#region TestExpiryDateLessThanExpiryNotificationPeriod

		[TestDate(2014, 01, 01)]
		public void TestExpiryDateLessThanExpiryNotificationPeriod()
		{
			var client = Helper.CreateClient("TestClient");
			var part = Helper.CreateProduct(client, "TestProduct");
			var whs = Helper.CreateWarehouse("Warehouse");

			var productRelationshipList = part.RelatedOrganisations;  //	change product to use expiry date
			client.MiscServ.OM_IMUseExpiryDate = true;
			var partRelation = productRelationshipList.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_UseExpiryDate = true;

			var productParam = Helper.CreateProductParamsByWhsAndClient(part, client, whs);
			productParam.W3_ExpiryNotificationPeriod = 30;

			var product = WhsProduct.GetWhsProduct(part);
			var expiryDate1 = new ZDate(2014, 01, 31);
			AssertEquals(true, PartAttributeValidation.ExpiryDateLessThanExpiryNotificationPeriod(client, whs, product, expiryDate1));

			var expiryDate2 = new ZDate(2014, 02, 01);
			AssertEquals(false, PartAttributeValidation.ExpiryDateLessThanExpiryNotificationPeriod(client, whs, product, expiryDate2));
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions helper;
		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		#endregion
	}
}
