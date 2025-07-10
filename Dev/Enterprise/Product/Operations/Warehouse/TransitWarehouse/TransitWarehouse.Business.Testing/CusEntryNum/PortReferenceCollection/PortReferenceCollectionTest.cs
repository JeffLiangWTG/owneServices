using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(PortReferenceCollection))]
	public class PortReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<PortReferenceCollection>
	{
		public void TestDefaultValues()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new PortReferenceCollection(bizObj);
			var number = numbers.AddNew();
			AssertEquals(TransitWarehouseReferenceCategories.Codes.PortReference, number.CE_Category);
			AssertEquals(bizObj.PK, number.CE_ParentID);
			AssertEquals(AutoDummyBizo.Schema.TableName, number.CE_ParentTable);
			AssertEquals(bizObj, number.Parent);
		}

		public void TestLoadAfterSave()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new PortReferenceCollection(bizObj);
			var number = numbers.AddNew();
			number.CE_EntryNum = "111";
			number.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			var loadedBizObj = NewFactory().Load<DummyBusinessObject>(bizObj.PK);
			var loadedPortReferences = new PortReferenceCollection(loadedBizObj);
			var portReference = loadedPortReferences.Single();
			AssertEquals($"Category is {TransitWarehouseReferenceCategories.Codes.PortReference}",
				TransitWarehouseReferenceCategories.Codes.PortReference, portReference.CE_Category);
			AssertEquals("Numbers contains previously saved number", "111", portReference.CE_EntryNum);
			AssertEquals("Parent retained", loadedBizObj, portReference.Parent);
		}

		protected override PortReferenceCollection GetCollectionToTest()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			return new PortReferenceCollection(bizObj);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var number = Factory.New<CusEntryNumber>();
			number.CE_Category = TransitWarehouseReferenceCategories.Codes.PortReference;
			return number;
		}
	}
}
