using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class InventoryTestHelperForPartAttributeValidation : NUnit.Framework.Assertion
	{
		public static void CreateInventory_Staged(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			var transferLine = CreateInventory_InTransit_Core(helper, warehouse, client, part, partAttributeToSet);
			helper.FinaliseDocketLine(transferLine.PK);
			AssertEquals("Precondition: Inventory is Staged.", "STA", transferLine.WE_CurrentInventoryStatus);
			client.Factory.Save();
		}

		public static void CreateInventory_InTransit(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			CreateInventory_InTransit_Core(helper, warehouse, client, part, partAttributeToSet);
		}

		static IWhsDocketLine CreateInventory_InTransit_Core(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			var factory = client.Factory;
			CreateInventory_Receive(helper, warehouse, client, part, partAttributeToSet);
			factory.Save();

			var orderPK = helper.CreateWhsOrder(client.PK, warehouse.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part.PK, 10m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });

			var pickLine = helper.GetPickLines(pickPK).Single();
			var transferLine = helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Today);
			factory.Save();

			return transferLine;
		}

		public static void CreateInventory_Receive(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			CreateInventory_Receive_Core(helper, warehouse, client, part, partAttributeToSet, isHeld: false);
		}

		public static void CreateInventory_Held(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			CreateInventory_Receive_Core(helper, warehouse, client, part, partAttributeToSet, isHeld: true);
		}

		static void CreateInventory_Receive_Core(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet, bool isHeld)
		{
			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", isHeld ? "HEL" : "");
			AssertEquals("Precondition.", isHeld ? "HEL" : "", client.Factory.Load<IWhsDocketLine>(receiveLinePK).WE_WHC_NKOriginalInventoryHeldCode);

			if (partAttributeToSet != null)
			{
				SetPartAttribute(client.Factory, receiveLinePK, partAttributeToSet);
			}

			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);
		}

		public static void CreateInventory_AdjustmentIn(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet)
		{
			var adjustment = helper.CreateWhsAdjustment(client.PK, warehouse.PK, "R1", new NotificationBuffer());
			var adjustmentLinePK = helper.CreateWhsAdjustmentLine(adjustment.PK, part.PK, 10m, "A-1-1");

			if (partAttributeToSet != null)
			{
				SetPartAttribute(client.Factory, adjustmentLinePK, partAttributeToSet);
			}

			helper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
		}

		public static void CreateInventory_Transfer(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part, SchemaColumn partAttributeToSet, bool lineFinalisedRatherThanDocket = false)
		{
			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1");

			if (partAttributeToSet != null)
			{
				SetPartAttribute(client.Factory, receiveLinePK, partAttributeToSet);
			}

			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var transferPK = helper.CreateWhsTransfer(client.PK, warehouse.PK, "R2", new NotificationBuffer());
			var transferLinePK = helper.CreateWhsTransferLine(transferPK, part.PK, 10m, "A-1-1", "A-2-1");

			if (partAttributeToSet != null)
			{
				SetPartAttribute(client.Factory, transferLinePK, partAttributeToSet);
			}

			if (lineFinalisedRatherThanDocket)
			{
				helper.FinaliseDocketLine(transferLinePK);
			}
			else
			{
				helper.FinaliseDocketWithoutUserConfirmation(transferPK);
			}
		}

		static void SetPartAttribute(BusinessObjectFactory factory, ZGuid docketLinePk, SchemaColumn partAttributeColumn)
		{
			const string partAttribute = "PA";
			var datePartAttribute = ZDate.Today.AddDays(1);

			var docketLine = factory.Load<IWhsDocketLine>(docketLinePk);
			switch (partAttributeColumn.Name)
			{
				case WhsDocketLineSchema.Constants.WE_PartAttrib1:
					docketLine.WE_PartAttrib1 = partAttribute;
					break;
				case WhsDocketLineSchema.Constants.WE_PartAttrib2:
					docketLine.WE_PartAttrib2 = partAttribute;
					break;
				case WhsDocketLineSchema.Constants.WE_PartAttrib3:
					docketLine.WE_PartAttrib3 = partAttribute;
					break;
				case WhsDocketLineSchema.Constants.WE_SerialNumber:
					docketLine.WE_SerialNumber = partAttribute;
					docketLine.WE_TransactionQuantity = 1m;
					docketLine.WE_ClientOrderedUnits = 1m;
					break;
				case WhsDocketLineSchema.Constants.WE_ExpiryDate:
					docketLine.WE_ExpiryDate = datePartAttribute;
					break;
				case WhsDocketLineSchema.Constants.WE_PackingDate:
					docketLine.WE_PackingDate = datePartAttribute;
					break;
				default:
					throw new ArgumentException($"Expected Warehouse Part Attribute, got {partAttributeColumn.Name}.");
			}
		}

		public static void SetOrgMiscServAttribs(OrgHeader client, string value)
		{
			client.MiscServ.OM_IMPartAttrib1Type = value;
			client.MiscServ.OM_IMPartAttrib2Type = value;
			client.MiscServ.OM_IMPartAttrib3Type = value;
		}

		public static void SetOrgMiscServDates(OrgHeader client, bool value)
		{
			client.MiscServ.OM_IMUseExpiryDate = value;
			client.MiscServ.OM_IMUsePackingDate = value;
		}
	}
}
