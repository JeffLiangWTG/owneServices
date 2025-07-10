using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedJobNumberUpdaterTest : WhsTestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullArgument()
		{
			JobNumberUpdater = new WhsBondedJobNumberUpdater(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestUpdateWithWrongArgument1()
		{
			JobNumberUpdater.Update(ZGuid.Empty, "");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestUpdateWithWrongArgument2()
		{
			JobNumberUpdater.Update(ZGuid.NewZGuid(), ZString.Empty);
		}

		public void TestUpdateExWarehouseTransaction()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			var order = Helper.CreateWhsOrder(data.IReceive.Client, data.Whs);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			Helper.CreatePick_OLD(order);
			Assert("Order is ATP", order.IsAttachedToPickButNotFinalised);
			line1.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Order is PIC", DocketStatus.Codes.Picking, order.WD_DocketStatus);
			order.WD_ExWhsJobGuid = ZGuid.NewZGuid();
			AssertEquals("WB_DeclarationReference", order.WD_ExternalReference,
				line1.CustomsData.WB_DeclarationReference);
			AssertEquals("WB_DeclarationReference", order.WD_ExternalReference,
				line2.CustomsData.WB_DeclarationReference);
			line3.CustomsData.WB_DeclarationReference = "OLDDECREF";
			AssertEquals("OldRef WB_DeclarationReference", "OLDDECREF", line3.CustomsData.WB_DeclarationReference);
			JobNumberUpdater.Update(order.WD_ExWhsJobGuid, "NEWDECREF");
			AssertEquals("WB_DeclarationReference", "NEWDECREF", line1.CustomsData.WB_DeclarationReference);
			AssertEquals("WB_DeclarationReference", "NEWDECREF", line2.CustomsData.WB_DeclarationReference);
			AssertEquals("WB_DeclarationReference", "NEWDECREF", line3.CustomsData.WB_DeclarationReference);
		}

		public void TestUpdateExWarehouseTransactionNotPickingOrFinalised()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			var order = Helper.CreateWhsOrder(data.IReceive.Client, data.Whs);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "E11AA1-1", "DummyOutward-1", "");
			Helper.CreatePick_OLD(order);
			order.WD_WP = ZGuid.Empty;
			Assert("Order is Not Picking OR Finalised", !(order.IsAttachedToPick || order.IsFinalised));
			order.WD_ExWhsJobGuid = ZGuid.NewZGuid();
			AssertEquals("WB_DeclarationReference", order.WD_ExternalReference,
				line1.CustomsData.WB_DeclarationReference);
			AssertEquals("WB_DeclarationReference", order.WD_ExternalReference,
				line2.CustomsData.WB_DeclarationReference);
			line3.CustomsData.WB_DeclarationReference = "OLDDECREF";
			AssertEquals("OldRef WB_DeclarationReference", "OLDDECREF", line3.CustomsData.WB_DeclarationReference);
			JobNumberUpdater.Update(order.WD_ExWhsJobGuid, "NEWDECREF");
			AssertEquals("Nothing Happeded  OldRef WB_DeclarationReference", "OLDDECREF",
				line3.CustomsData.WB_DeclarationReference);
		}

		[ExpectNoExceptions()]
		public void TestUpdateExWarehouseTransactionNonPicking()
		{
			var data = new TestDataForBondedEntriesWithVOC(Factory);
			var order = Helper.CreateWhsOrder(data.IReceive.Client, data.Whs);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m, "", "", "");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m, "", "", "");
			order.WD_ExWhsJobGuid = ZGuid.NewZGuid();
			var updateTran = new WhsBondedWarehouseTransaction();
			updateTran.ExternalPK = order.WD_ExWhsJobGuid;
			updateTran.Reference = "NEWDECREF";
			JobNumberUpdater.Update(order.WD_ExWhsJobGuid, "NEWDECREF");
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			JobNumberUpdater = new WhsBondedJobNumberUpdater(Factory);
		}

		WhsBondedJobNumberUpdater JobNumberUpdater;

		#endregion
	}
}
