using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBondedWarehouseAttributeCollection))]
	class WhsBondedWarehouseAttributeCollectionTestCase : WhsBusinessObjectCollectionTestCase
	{
		public void TestContructorWithEntryKey()
		{
			CreateNewAttibuteWithData("ENT1", 1);
			CreateNewAttibuteWithData("ENT1", 1);
			CreateNewAttibuteWithData("ENT2", 2);
			CreateNewAttibuteWithData("ENT2", 1);
			CreateNewAttibuteWithData("ENT1", 1);

			WhsBondedWarehouseAttributeCollection customsData = new WhsBondedWarehouseAttributeCollection(Factory, "ENT1", 1);
			customsData.Load();
			AssertEquals("CustomsData.Count", 3, customsData.Count);

			foreach (WhsBondedWarehouseAttribute attributeFound in customsData)
			{
				AssertEquals("AttributeFound.WB_EntryKey", "ENT1", attributeFound.WB_EntryKey);
				AssertEquals("AttributeFound.WB_EntryLineNo", (short)1, (short)attributeFound.WB_EntryLineNo);
			}
		}

		WhsBondedWarehouseAttribute CreateNewAttibuteWithData(ZString entryNo, ZShort entryLineNo)
		{
			WhsBondedWarehouseAttribute attribute = Factory.New<WhsBondedWarehouseAttribute>();
			attribute.WB_EntryKey = entryNo;
			attribute.WB_EntryLineNo = entryLineNo;
			return attribute;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsBondedWarehouseAttributeCollection(Factory);
		}
	}
}
