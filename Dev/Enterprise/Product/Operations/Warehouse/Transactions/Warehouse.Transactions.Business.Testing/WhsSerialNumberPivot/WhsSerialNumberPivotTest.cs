using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsSerialNumberPivot))]
	class WhsSerialNumberPivotTest : WhsBusinessObjectTestCase
	{
		public void TestSerialNumber()
		{
			var serialPivot = Factory.New<WhsSerialNumberPivot>();
			AssertEquals(ZString.Empty, serialPivot.SerialNumberValue);

			var serial = Factory.New<WhsSerialNumber>();
			serialPivot.WSV_WSN_SerialNumber = serial.PK;
			AssertEquals(serial.PK, serialPivot.WSV_WSN_SerialNumber);

			var serialNew = Factory.New<WhsSerialNumber>();
			serialPivot.WSV_WSN_SerialNumber = serialNew.PK;
			AssertEquals(serialNew.PK, serialPivot.WSV_WSN_SerialNumber);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Factory.Save();

			var serialNumberPivot = Helper.CreateWhsSerialNumberPivot(receive.Lines[0], serialNumber);

			AssertNoExceptionThrown("Should save without exception", Factory.Save);

			serialNumberPivot.Delete();

			AssertNoExceptionThrown("Should delete without exception", Factory.Save);
		}

		[ExpectNoExceptions]
		public void TestConstraint_WSV_ParentTableCode_ASNLine()
		{
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WSV_ParentTableCode_ASNLine";

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var asnLine = Helper.CreateAsnLine(receive, data.Part1, 1m);
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Factory.Save();

			Helper.CreateWhsSerialNumberPivot(receive.Lines[0], serialNumber);
			var serialNumberPivot = Helper.CreateWhsSerialNumberPivot(asnLine, serialNumber);
			AssertEquals("Precondistion", false, serialNumberPivot.WSV_IsReleaseCaptured);
			AssertEquals("Precondistion", ZGuid.Empty, serialNumberPivot.WSV_WZ_PickingLine);
			AssertNoExceptionThrown("Should save without exception", Factory.Save);

			serialNumberPivot.WSV_IsReleaseCaptured = true;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WSV_ParentTableCode_ASNLine");

			serialNumberPivot.WSV_IsReleaseCaptured = false;
			AssertNoExceptionThrown("Should save without exception", Factory.Save);

			receive.FinaliseDocketWithoutUserConfirmation();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			Factory.Save();

			serialNumberPivot.WSV_WZ_PickingLine = pickLine.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WSV_ParentTableCode_ASNLine");
		}

		[ExpectNoExceptions]
		public void TestConstraint_WSV_ParentID_TableCode_PickingLine()
		{
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WSV_ParentID_TableCode_PickingLine";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var now = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.Now);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines[0];
			var pickLine2 = orderLine.PickLines[1];
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Factory.Save();

			var serialNumberPivot = Helper.CreateWhsSerialNumberPivot(pickLine1, serialNumber);
			AssertEquals("Precondistion", true, serialNumberPivot.WSV_IsReleaseCaptured);
			AssertEquals("Precondistion", WhsPickLineSchema.Constants.Prefix, serialNumberPivot.WSV_ParentTableCode);
			AssertEquals("Precondistion", pickLine1.PK, serialNumberPivot.WSV_WZ_PickingLine);
			AssertNoExceptionThrown("Should save without exception", Factory.Save);

			serialNumberPivot.WSV_IsReleaseCaptured = false;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WSV_ParentID_TableCode_PickingLine");

			serialNumberPivot.WSV_IsReleaseCaptured = true;
			AssertNoExceptionThrown("Should save without exception", Factory.Save);

			serialNumberPivot.WSV_WZ_PickingLine = pickLine2.PK;
			AssertNotEquals("Precondistion", pickLine2.PK, serialNumberPivot.WSV_ParentID);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WSV_ParentID_TableCode_PickingLine");
		}

		public void TestSerialNumberValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var serialPivot = receive.Lines[0].SerialNumbers.AddNew();

			AssertEquals("Precondition", string.Empty, serialPivot.SerialNumberValue);

			serialPivot.SerialNumberValue = "SN1";
			AssertEquals("SN1", serialPivot.SerialNumberValue);
			var serialNumber = Factory.Load<WhsSerialNumber>(serialPivot.WSV_WSN_SerialNumber);
			var serialNumberPK = serialNumber.PK;

			serialPivot.SerialNumberValue = "sn2";
			AssertEquals("sn2", serialPivot.SerialNumberValue);
			AssertEquals("Use same Serial Number", serialNumberPK, serialPivot.WSV_WSN_SerialNumber);
			Factory.Save();

			serialPivot.SerialNumberValue = "Sn2";
			AssertEquals("Sn2", serialPivot.SerialNumberValue);
			AssertEquals("Use same Serial Number", serialNumberPK, serialPivot.WSV_WSN_SerialNumber);
			Factory.Save();
			AssertEquals("Same value saved in DB.", "Sn2", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);

			serialPivot.SerialNumberValue = "SN3";
			AssertEquals("SN3", serialPivot.SerialNumberValue);
			AssertNotEquals("Not use same Serial Number", serialNumberPK, serialPivot.WSV_WSN_SerialNumber);
			AssertEquals("Should delete Serial Number", true, serialNumber.IsDeleted);
		}

		public void TestSerialNumberValue_InvalidOperationException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			Helper.CreateAsnLine(receive, data.Part1, 1);
			Factory.Save();

			var serialPivot = receive.AsnLines[0].SerialNumbers.AddNew();
			AssertExceptionThrown<InvalidOperationException>(
				"Try to create a serial number for the incorrect parent.",
				() => serialPivot.SerialNumberValue = "SN1");
		}

		public void TestSerialNumberValue_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			Factory.Save();
			var serialNumberPivot = receive.Lines[0].SerialNumbers.AddNew();
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(WhsSerialNumberPivot), nameof(WhsSerialNumberPivot.SerialNumberValue), true, attrib => attrib.Member == "SerialNumberReadOnly");
		}

		#region TestSerialNumber

		public void TestSerialNumberPivot_OnSaving()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			Factory.Save();
			var serialPivot = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot.SerialNumberValue = "SN1";
			Factory.Save();
			AssertEquals("Precondition", "SN1", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);

			serialPivot.SerialNumberValue = "SN2";
			Factory.Save();
			AssertEquals("It should save without error, and should be only one serial number in the database.", "SN2", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);
		}

		public void TestSerialNumberPivot_OnSaving_Finalising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			receive.Lines[0].WE_SerialNumber = "SN1";
			var serialPivot = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot.SerialNumberValue = "SN1";
			Factory.Save();
			AssertEquals("Precondition", "SN1", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);

			serialPivot.SerialNumberValue = "SN2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			AssertEquals("It should save without error, and should be only one serial number in the database.", "SN2", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);
		}

		public void TestSerialNumberPivot_OnSaving_OnlyDocketLineCanRecreatePivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var rlPivot = receive.Lines[0].SerialNumbers.AddNew();
			rlPivot.SerialNumberValue = "SN1";
			Helper.CreateAsnLine(receive, data.Part1, 1);
			var asnPivot = receive.AsnLines[0].SerialNumbers.AddNew();
			asnPivot.WSV_WSN_SerialNumber = rlPivot.WSV_WSN_SerialNumber;
			Factory.Save();
			AssertEquals("Precondition", "SN1", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);

			var serialNumber = Factory.Load<WhsSerialNumber>(asnPivot.WSV_WSN_SerialNumber);
			asnPivot.WSV_WSN_SerialNumber = CreateWhsSerialNumber(serialNumber.WSN_OP_Product, serialNumber.WSN_OH_Client, "SN2");
			serialNumber.Delete();

			AssertExceptionThrown<InvalidOperationException>("Try adding a serial number pivot for the incorrect parent.", Factory.Save);

			ZGuid CreateWhsSerialNumber(ZGuid productPK, ZGuid clientPK, ZString sn)
			{
				var serialNumber = Factory.New<WhsSerialNumber>();
				serialNumber.WSN_OP_Product = productPK;
				serialNumber.WSN_OH_Client = clientPK;
				serialNumber.WSN_SerialNumber = sn;
				return serialNumber.PK;
			}
		}

		public void TestSerialNumberPivot_OnSaving_InCaseOfSaveExeption()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfSerialNumbers = 10;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, numberOfSerialNumbers, true, false);
				Factory.Save();

				for (int i = 0; i < numberOfSerialNumbers; i++)
				{
					var serialPivot = receive.Lines[0].SerialNumbers.AddNew();
					serialPivot.SerialNumberValue = $"SN{i}";
				}
				Factory.Save();
				AssertSerialNumberInDB("Precondition", Enumerable.Range(0, 10).Select(r => $"SN{r}"));

				BusinessObjectFactory.SavingEventHandler handler = f => throw new ZCannotSaveException("Ops!", "Test", ExceptionType.BusinessFailure);

				Factory.Saving += handler;

				for (int i = 0; i < numberOfSerialNumbers; i++)
				{
					receive.Lines[0].SerialNumbers[i].SerialNumberValue = $"NEW{i}";
				}

				Helper.AssertZCannotSaveExceptionThrown("Ops!", Factory.Save);

				Factory.Saving -= handler;
				Factory.Save(); // Should be able to save after fixing error

				AssertSerialNumberInDB("Should be able to update serial numbers.", Enumerable.Range(0, numberOfSerialNumbers).Select(r => $"NEW{r}"));

				void AssertSerialNumberInDB(string msg, IEnumerable<string> expectedSerialNumbers)
				{
					AssertContainsExactElementsInAnyOrder(msg, expectedSerialNumbers, NewFactory().Load<WhsSerialNumber>(new ZQuery()).Select(s => s.WSN_SerialNumber));
				}
			}
		}

		public void TestIsSerialNumberExistsProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);

			var serialPivot1 = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot1.SerialNumberValue = "SN1";

			var serialPivot2 = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot2.SerialNumberValue = "SN1";

			AssertEquals("Should find duplicate serial number.", true, serialPivot2.IsSerialNumberAlreadyInUse());

			serialPivot2.WSV_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			AssertEquals("Should not have Exception.", false, serialPivot2.IsSerialNumberAlreadyInUse());
		}

		public void TestProperties_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var serialPivot = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot.SerialNumberValue = "SN1";

			AssertEquals(serialPivot.WSV_WSN_SerialNumber, serialPivot.SerialNumber.PK);

			var otherSerialNumber = Factory.New<WhsSerialNumber>();
			serialPivot.WSV_WSN_SerialNumber = otherSerialNumber.PK;
			AssertEquals(otherSerialNumber.PK, serialPivot.SerialNumber.PK);
		}

		#endregion

		#region TestCloneAuditProperties

		public override void TestCloneAuditProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var serialPivot = receive.Lines[0].SerialNumbers.AddNew();
			serialPivot.SerialNumberValue = "SN1";
			Factory.Save();
			AssertEquals("Precondition", serialPivot.PK, NewFactory().Load<WhsSerialNumberPivot>(new ZQuery()).Single().PK);
			AssertEquals("Precondition", "SN1", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);
			var serialPivotClone = (WhsSerialNumberPivot)serialPivot.Clone();
			AssertNotEquals("PK", serialPivotClone.PK, serialPivot.PK);
			AssertEquals("WSV_ParentID", serialPivotClone.WSV_ParentID, serialPivotClone.WSV_ParentID);
			AssertEquals("WSV_ParentTableCode", serialPivotClone.WSV_ParentTableCode, serialPivotClone.WSV_ParentTableCode);
			AssertEquals("WSV_WZ_PickingLine", serialPivotClone.WSV_WZ_PickingLine, serialPivotClone.WSV_WZ_PickingLine);
			AssertEquals("WSV_WSN_SerialNumber", serialPivotClone.WSV_WSN_SerialNumber, serialPivotClone.WSV_WSN_SerialNumber);

			serialPivot.Delete();
			Factory.Save();
			AssertEquals("Should save without issue.", serialPivotClone.PK, NewFactory().Load<WhsSerialNumberPivot>(new ZQuery()).Single().PK);
			AssertEquals("Should be the same.", "SN1", NewFactory().Load<WhsSerialNumber>(new ZQuery()).Single().WSN_SerialNumber);
		}

		public override void TestCloneAuditContextProperties()
		{
			Assert("No audit context properties to test for WhsSerialNumberPivot", true);
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var pivot = receive.Lines[0].SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";

			return pivot;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			enableSchemaRedesignChanges = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			enableSchemaRedesignChanges.Dispose();
			base.TearDown();
		}

		IDisposable enableSchemaRedesignChanges;

		#endregion
	}
}
