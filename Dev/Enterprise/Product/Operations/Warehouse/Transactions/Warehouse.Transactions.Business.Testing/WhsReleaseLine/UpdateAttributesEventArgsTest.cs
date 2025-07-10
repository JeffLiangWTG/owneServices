using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class UpdateAttributesEventArgsTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new UpdateAttributesEventArgs(null, PartAttributeNumber.None));

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var today = ZDate.Today;
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "PA1";
			releaseLine.PartAttribute2 = "PA2";
			releaseLine.PartAttribute3 = "PA3";
			releaseLine.SetExpiryDateForTesting(today.AddDays(-1));
			releaseLine.SetPackingDateForTesting(today.AddDays(1));

			var args = new UpdateAttributesEventArgs(AttributeParts.New(releaseLine), PartAttributeNumber.One);
			AssertEquals("args.OldAttributes.PartAttribute1", "PA1", args.OldAttributes.PartAttribute1);
			AssertEquals("args.OldAttributes.PartAttribute2", "PA2", args.OldAttributes.PartAttribute2);
			AssertEquals("args.OldAttributes.PartAttribute3", "PA3", args.OldAttributes.PartAttribute3);
			AssertEquals("args.OldAttributes.ExpiryDate", today.AddDays(-1), args.OldAttributes.ExpiryDate);
			AssertEquals("args.OldAttributes.PackingDate", today.AddDays(1), args.OldAttributes.PackingDate);
			AssertEquals("args.PartAttribChanged", PartAttributeNumber.One, args.PartAttribChanged);
			AssertEquals("args.OldKey", args.OldAttributes.Key, args.OldKey);
		}

		#endregion
	}
}
