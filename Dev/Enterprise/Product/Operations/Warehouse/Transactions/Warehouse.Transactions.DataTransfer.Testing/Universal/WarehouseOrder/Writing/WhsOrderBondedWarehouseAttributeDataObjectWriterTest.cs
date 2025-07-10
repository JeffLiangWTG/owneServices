using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsOrderBondedWarehouseAttributeDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var customsData = WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(orderLine);
			AssertExceptionThrown<ArgumentNullException>(() => new WhsOrderBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, customsData)), null));
		}

		public void TestInwardsEntryKey()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var customsData = WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(orderLine);
			var customsDataObject = new WhsOrderBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, customsData)), orderLine).GetDataObject(customsData);

			AssertNotNull("customsDataObject", customsDataObject);

			CombineAssertions(delegate
			{
				AssertContents(customsDataObject);
			});
		}

		internal static void AssertContents(CustomsEntryInfo customsDataObject)
		{
			WhsBondedWarehouseAttributeDataObjectWriterTest.AssertContents(customsDataObject);

			AssertEquals("customsDataObject.InwardsEntryKey", "INWARDSKEY", customsDataObject.InwardsEntryKey);
			AssertEquals("customsDataObject.InwardsEntryLineNumber", new ZShort(1), customsDataObject.InwardsEntryLineNumber);
		}
	}
}
