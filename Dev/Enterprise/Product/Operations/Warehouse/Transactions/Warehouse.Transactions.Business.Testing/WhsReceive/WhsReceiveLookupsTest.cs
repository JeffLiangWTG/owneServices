using System;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveLookupsTest : WhsDocketLookupsTest<WhsReceive>
	{
		public void TestReceiveCategories()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			receiveCategories.Add("RC3", (NoResString)"Receive Category 3");

			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var receive = GetNewBusinessObject();
			AssertEquals("RC1, RC2, RC3", ((WhsReceiveLookups)receive.Lookups).ReceiveCategories.CodesAsString);
		}

		protected override void TestSubTypesCore()
		{
			AssertEquals(true, ((WhsReceiveLookups)GetNewBusinessObject().Lookups).SubTypes.ContainsCode(CodeLists.ReceiveType.Codes.Receipt));
		}
	}
}
